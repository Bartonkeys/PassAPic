using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Data;
using PassAPic.Models.Models;
using Phonix;

namespace PassAPic.Core.Services
{
    public class GameService
    {
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
             */

            var gameScores = new List<GameScoringModel>();

            List<Guess> guesses = game.Guesses.OrderBy(g => g.Order).ToList();
            var startingWord = new WordGuess()
            {
                Word = game.StartingWord, 
                Order = 0,
                NextUser = guesses.ElementAt(0).User
            };
            guesses.Insert(0, startingWord);

            var wordGuesses = guesses.Where(g => g is WordGuess).ToList();

            //First and last word
            var firstWord = wordGuesses.First();
            var lastWord = wordGuesses.Last();

            if (firstWord is WordGuess && lastWord is WordGuess)
            {
                var similar = compareWords((WordGuess)firstWord, (WordGuess)lastWord);
                if (similar)
                {
                    gameScores.Add(new GameScoringModel()
                    {
                        GameId = game.Id,
                        UserId = game.Creator.Id,
                        UserName = game.Creator.Username,
                        Score = game.NumberOfGuesses
                    });
                }
            }

            //Other guesses
            for (int i = 0; i < wordGuesses.Count-1; i++)
            {
                var word1 = wordGuesses.ElementAt(i);
                var word2 = wordGuesses.ElementAt(i+1);

                var similar = compareWords((WordGuess)word1, (WordGuess)word2);
                if (similar)
                {
                    gameScores.Add(new GameScoringModel()
                    {
                        GameId = game.Id,
                        UserId = word1.NextUser.Id,
                        UserName = word1.NextUser.Username,
                        Score = 1
                    });

                    gameScores.Add(new GameScoringModel()
                    {
                        GameId = game.Id,
                        UserId = word2.User.Id,
                        UserName = word2.User.Username,
                        Score = 1
                    });
                }
            }


            return gameScores;
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

            var metaphone = new Metaphone();
            var stringArray = new string[] { firstWordModel.Word, secondWordModel.Word };
            var similar = metaphone.IsSimilar(stringArray);

            return similar;
        }
       
    }
}
