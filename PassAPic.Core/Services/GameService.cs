using System;
using System.Collections.Generic;
using System.Diagnostics;
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
       
    }
}
