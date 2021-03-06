﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Data;
using PassAPic.Models;
using PassAPic.Models.Models;

namespace PassAPic.Controllers
{
    public class BaseController : ApiController
    {
        protected static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected IDataContext DataContext;
        protected List<String> Words;
        protected List<String> EasyWords;
        protected Random random = new Random();

        protected OpenGamesModel PopulateOpenGamesModel(IEnumerable<Guess> guesses)
        {
            var gameModelList = new OpenGamesModel();
            var wordModelList = new List<WordModel>();
            var imageModelList = new List<ImageModel>();

            foreach (var guess in guesses)
            {
               
                var isLastTurn = guess.Game.Guesses.Count() + 1 >= guess.Game.NumberOfGuesses;

                var sentFromUsername = ""; 
                var previousGuess = guess.Game.Guesses.FirstOrDefault(g => g.Order == guess.Order);
                if (previousGuess != null)
                {
                    sentFromUsername = previousGuess.User.Username;
                }
                else
                {
                    sentFromUsername = guess.Game.Creator.Username;
                }

               

                if (guess is WordGuess)
                {
                    var wordGuess = (WordGuess)guess;
                    var wordModel = new WordModel
                    {
                        GuessId = wordGuess.Id,
                        GameId = wordGuess.Game.Id,
                        UserId = wordGuess.NextUser.Id,
                        Word = wordGuess.Word,
                        IsLastTurn = isLastTurn,
                        CreatorId = guess.Game.Creator.Id,
                        SentFromUsername = sentFromUsername,
                        UserName = wordGuess.NextUser.Username,
                        DateCreated = guess.DateCreated ?? guess.Game.DateCreated,
                        TimerInSeconds = guess.Game.TimerInSeconds
                    };

                    wordModelList.Add(wordModel);
                }
                else if (guess is ImageGuess)
                {
                    var imageGuess = (ImageGuess)guess;

                    var imageModel = new ImageModel
                    {
                        GuessId = imageGuess.Id,
                        GameId = imageGuess.Game.Id,
                        UserId = imageGuess.NextUser.Id,
                        Image = imageGuess.Image,
                        IsLastTurn = isLastTurn,
                        CreatorId = guess.Game.Creator.Id,
                        SentFromUsername = sentFromUsername,
                        UserName = imageGuess.NextUser.Username,
                        DateCreated = guess.DateCreated ?? guess.Game.DateCreated,
                        TimerInSeconds = guess.Game.TimerInSeconds
                    };

                    imageModelList.Add(imageModel);
                }
            }

            gameModelList.WordModelList = wordModelList;
            gameModelList.ImageModelList = imageModelList;

            return gameModelList;
        }
    }
}
