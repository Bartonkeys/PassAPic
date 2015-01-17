using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Data;
using Phonix;


namespace PassAPic.Core.Services
{
    public class GameService
    {
        public string CalculateScoreForGame(Game game)
        {
            /*
             *Scoring algorihm:
             * If the final word matches the first word: 
             * - the game creator gets equivalent points to the number of passes
             * 
             * If a user guesses the right word that the previous user was trying to draw:
             *  - both the drawer and the guesser get ONE point
             */


            //if (guess is WordGuess)
            //        {
            //            var wordGuess = (WordGuess) guess;
            //            var wordModel = new WordModel
            //            {
            //                Word = wordGuess.Word                                    
            //            };

            //var guesses = game.Guesses.OrderBy( g => g.Order);
            //var firstWord = (WordModel)guesses.First();
            //var lastWord = guesses.Last();

            //var metaphone = new Metaphone();
            //var stringArray = new string[] { firstWord, lastWord. };
            //var similar = metaphone.IsSimilar(stringArray);



            return "";
        }
    }
}
