using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Contracts;
using PassAPic.Data;
using PassAPic.Models.Models;
using Phonix;

namespace PassAPic.Core.Services
{
    //TODO: extract Interface and inject at runtime
    public class GameService
    {
        private readonly IDataContext _dataContext;

        public GameService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Leaderboard>> DoScoringAsync(Game game)
        {
            //Calculate Scores
            var scores = await Task.Run(() => CalculateScoreForGame(game));
            //try writing scores to DB
            await SaveScoresToDatabaseAsync(scores);
            //Update Leaderboard
            return await RecalculateLeaderboardAsync();
        }


        public List<GameScoringModel> CalculateScoreForGame(Game game)
        {
            /*
             * Scoring algorithm:
             * 
             * 1. If first word matches final word:
             *   - game creator gets equivalent number of points to game passes
             *  
             * 2. If a user guesses the same word as the previous usr was trying to draw:
             *   - both the drawer and the guesser get one point each
             *   
             * 3. If this is a timed game
             *   - both drawers and guessers get double points
             * 
             */

            //await Task.Run(() =>
            //{
                var gameScores = new List<GameScoringModel>();
                var scoreMultiplier = game.TimerInSeconds == 0 ? 1 : 2;

                List<Guess> guesses = game.Guesses.OrderBy(g => g.Order).ToList();
                var startingWord = new WordGuess()
                {
                    Word = game.StartingWord,
                    Order = 0,
                    NextUser = guesses.ElementAt(0).User
                };
                guesses.Insert(0, startingWord);

                var wordGuesses = guesses.Where(g => g is WordGuess).ToList();



                //Other guesses
                for (int i = 0; i < wordGuesses.Count - 1; i++)
                {
                    var word1 = wordGuesses.ElementAt(i);
                    var word2 = wordGuesses.ElementAt(i + 1);

                    var similar = compareWords((WordGuess) word1, (WordGuess) word2);
                    if (similar)
                    {
                        gameScores.Add(new GameScoringModel()
                        {
                            GameId = game.Id,
                            UserId = word1.NextUser.Id,
                            UserName = word1.NextUser.Username,
                            Score = 1*scoreMultiplier
                        });

                        gameScores.Add(new GameScoringModel()
                        {
                            GameId = game.Id,
                            UserId = word2.User.Id,
                            UserName = word2.User.Username,
                            Score = 1*scoreMultiplier
                        });
                    }
                }

                //First and last word
                var firstWord = wordGuesses.First();
                var lastWord = wordGuesses.Last();

                if (firstWord is WordGuess && lastWord is WordGuess)
                {
                    var similar = compareWords((WordGuess) firstWord, (WordGuess) lastWord);
                    if (similar)
                    {
                        //Check if the game creator has already scored in this game
                        if (gameScores.Any(s => s.UserId == game.Creator.Id))
                        {
                            var score = gameScores.Find(s => s.UserId == game.Creator.Id);
                            score.Score += (game.NumberOfGuesses*scoreMultiplier);
                        }
                        else
                        {
                            gameScores.Add(new GameScoringModel()
                            {
                                GameId = game.Id,
                                UserId = game.Creator.Id,
                                UserName = game.Creator.Username,
                                Score = game.NumberOfGuesses*scoreMultiplier
                            });
                        }

                    }
                }

                return gameScores;
            //});

            //return null;

        }

        public async Task<List<GameScoringModel>> CalculateScoreForGameAsync(Game game)
        {
            return await Task.Run(() => CalculateScoreForGame(game));
        }

        public string SaveScoresToDatabase(List<GameScoringModel> scores)
        {
            string message = "Scores saved successfuly";

            foreach (var gameScoringModel in scores)
            {
                try
                {
                    var gameScore = new Game_Scoring()
                    {
                        GameId = gameScoringModel.GameId,
                        UserId = gameScoringModel.UserId,
                        Score = gameScoringModel.Score,
                        DateCreated = DateTime.UtcNow
                    };
                    if (!_dataContext.Score.Any(s => s.GameId == gameScore.GameId && s.UserId == gameScore.UserId))
                    { _dataContext.Score.Add(gameScore); }
                }
                catch (Exception ex)
                {
                    message += "Score already exists for game/user Ids";
                    Debug.WriteLine(ex.Message);
                }

            }

            _dataContext.Commit();

            return message;
        }

        public async Task<string> SaveScoresToDatabaseAsync(List<GameScoringModel> scores)
        {
            return await Task.Run(() => SaveScoresToDatabase(scores));
        }

        public List<Leaderboard> RecalculateLeaderboard()
        {

            try
            {
                
                var allScores = _dataContext.Score.ToList();
                var newLeaderboard = CollateScoresForLeaderboard(allScores);

                //Clear out old leaderboad
                foreach (var leaderboard in _dataContext.Leaderboard)
                {
                    _dataContext.Leaderboard.Remove(leaderboard);
                }

                //Write all leaderboard items together
                foreach (var leaderboard in newLeaderboard)
                {
                    _dataContext.Leaderboard.Add(leaderboard);
                }

                _dataContext.Commit();

                return newLeaderboard;
            }

            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public List<LeaderboardSplit> RecalculateLeaderboardSplit()
        {

            try
            {
                var today = DateTime.UtcNow;
                Calendar cal = new GregorianCalendar();
                var weekNumber = cal.GetWeekOfYear(today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                var startOfWeek = FirstDateOfWeek(today.Year, weekNumber, new CultureInfo("en-GB"));
                var endOfWeek = startOfWeek.AddDays(7);

                var scoresThisWeek = _dataContext.Score.Where(s => startOfWeek <= s.DateCreated && s.DateCreated  <= endOfWeek).ToList();
                var newLeaderboardSplit = CollateScoresForLeaderboardSplit(scoresThisWeek, weekNumber);

                //Clear out old leaderboad
                foreach (var leaderboardSplit in _dataContext.LeaderboardSplit)
                {
                    if (leaderboardSplit.WeekNumber == weekNumber)
                    {_dataContext.LeaderboardSplit.Remove(leaderboardSplit);}
                }

                //Write all leaderboard items together
                foreach (var leaderboardSplit in newLeaderboardSplit)
                {
                    _dataContext.LeaderboardSplit.Add(leaderboardSplit);
                }

                _dataContext.Commit();

                return newLeaderboardSplit;
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<List<Leaderboard>> RecalculateLeaderboardAsync()
        {
            return await Task.Run(() => RecalculateLeaderboard());
        }
        private List<Leaderboard> CollateScoresForLeaderboard(List<Game_Scoring> scores)
        {
            var newLeaderboard = new List<Leaderboard>();
            var now = DateTime.UtcNow;

            try
            {

                foreach (var gameScoringModel in scores)
                {

                    var leaderboardItem = new Leaderboard();

                    if (newLeaderboard.Any(l => l.UserId == gameScoringModel.User.Id))
                    {
                        leaderboardItem =
                            (newLeaderboard.FirstOrDefault(l => l.UserId == gameScoringModel.User.Id));
                        if (leaderboardItem != null)
                        {
                            leaderboardItem.TotalScore += gameScoringModel.Score;
                            leaderboardItem.DateCreated = now;
                        }
                    }
                    else
                    {
                        leaderboardItem.UserId = gameScoringModel.User.Id;
                        leaderboardItem.Username = gameScoringModel.User.Username;
                        leaderboardItem.TotalScore = gameScoringModel.Score;
                        leaderboardItem.DateCreated = now;

                    }

                    newLeaderboard.Add(leaderboardItem);

                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }

            return newLeaderboard;
            
        }

        private List<LeaderboardSplit> CollateScoresForLeaderboardSplit(List<Game_Scoring> scores, int weekNumber )
        {
            var newLeaderboard = new List<LeaderboardSplit>();
            var now = DateTime.UtcNow;

            try
            {

                foreach (var gameScoringModel in scores)
                {

                    var leaderboardItem = new LeaderboardSplit();

                    if (newLeaderboard.Any(l => l.UserId == gameScoringModel.User.Id))
                    {
                        leaderboardItem =
                            (newLeaderboard.FirstOrDefault(l => l.UserId == gameScoringModel.User.Id));
                        if (leaderboardItem != null)
                        {
                            leaderboardItem.TotalScore += gameScoringModel.Score;
                            leaderboardItem.DateCreated = now;
                            leaderboardItem.WeekNumber = weekNumber;
                        }
                    }
                    else
                    {
                        leaderboardItem.UserId = gameScoringModel.User.Id;
                        leaderboardItem.Username = gameScoringModel.User.Username;
                        leaderboardItem.TotalScore = gameScoringModel.Score;
                        leaderboardItem.DateCreated = now;

                    }

                    newLeaderboard.Add(leaderboardItem);

                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }

            return newLeaderboard;

        }

        private bool compareWords(WordGuess firstWordGuess, WordGuess secondWordGuess)
        {         
            var firstWordModel = new WordModel
            {
                Word = firstWordGuess.Word.ToLower().Trim()
            };

            var secondWordModel = new WordModel
            {
                Word = secondWordGuess.Word.ToLower().Trim()
            };

           
            var stringArray = new string[] { firstWordModel.Word, secondWordModel.Word };
            
            var metaphone = new Metaphone();
            var soundex = new Soundex();
            var similarMetaphone = metaphone.IsSimilar(stringArray);
            var similarSoundex = soundex.IsSimilar(stringArray);

            return similarMetaphone || similarSoundex;
        }


        //private DateTime FirstDateOfWeek(int year, int weekNum, CalendarWeekRule rule)
        //{
        //    Debug.Assert(weekNum >= 1);

        //    DateTime jan1 = new DateTime(year, 1, 1);

        //    int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
        //    DateTime firstMonday = jan1.AddDays(daysOffset);
        //    Debug.Assert(firstMonday.DayOfWeek == DayOfWeek.Monday);

        //    var cal = CultureInfo.CurrentCulture.Calendar;
        //    int firstWeek = cal.GetWeekOfYear(firstMonday, rule, DayOfWeek.Monday);

        //    if (firstWeek <= 1)
        //    {
        //        weekNum -= 1;
        //    }

        //    DateTime result = firstMonday.AddDays(weekNum * 7);

        //    return result;
        //}

        public static DateTime FirstDateOfWeek(int year, int weekOfYear, CultureInfo ci)
        {
            DateTime jul1 = new DateTime(year, 7, 1);
            while (jul1.DayOfWeek != ci.DateTimeFormat.FirstDayOfWeek)
            {
                jul1 = jul1.AddDays(1.0);
            }
            int refWeek = ci.Calendar.GetWeekOfYear(jul1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);

            int weekOffset = weekOfYear - refWeek;

            return jul1.AddDays(7 * weekOffset);
        }
       
    }
}
