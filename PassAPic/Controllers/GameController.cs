using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Data;
using PassAPic.Models;

namespace PassAPic.Controllers
{
    /// <summary>
    /// Handles setup of game and management of guesses and results
    /// </summary>
    [RoutePrefix("api/Game")]
    public class GameController : BaseController
    {
        [Inject]
        public GameController(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            Words = UnitOfWork.Word.GetAll().Select(x => x.word).ToList();
        }

        // POST /api/game/start
        /// <summary>
        /// Lets play. Set up game, generate first word and return to user.
        /// </summary>
        /// <returns></returns>
        [Route("Start")]
        public HttpResponseMessage PostStart(GameSetupModel model)
        {
            try
            {
                var user = UnitOfWork.User.GetById(model.UserId);
                var game = new Game
                {
                    StartingWord = Words[random.Next(Words.Count)],
                    NumberOfGuesses = model.NumberOfPlayers,
                    Creator = user
                };

                UnitOfWork.Game.Insert(game);
                UnitOfWork.Commit();

                var wordModel = new WordModel
                {
                    GameId = game.Id,
                    UserId = user.Id,
                    Word = game.StartingWord
                };

                return Request.CreateResponse(HttpStatusCode.Created, wordModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST /api/game/guessimage
        /// <summary>
        /// Post up image guess, with next user. Store all that in Guess table. Return 201 if all good, 406 if next user has already had a go on this one.
        /// If this is last guess then dont set next user and mark game as over. Im aware this wont happen on an image, but shall stick it in here anyway.
        /// </summary>
        /// <returns></returns>
        [Route("GuessImage")]
        public HttpResponseMessage PostImageGuess(ImageModel model)
        {
            try
            {
                var user = UnitOfWork.User.GetById(model.UserId);
                var nextUser = UnitOfWork.User.GetById(model.NextUserId);
                var game = UnitOfWork.Game.GetById(model.GameId);

                if (NextUserAlreadyHadAGo(game, model.NextUserId))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        String.Format("Please pick another user, {0} has already had a turn", nextUser.Username));

                SetPreviousGuessAsComplete(game, model.UserId);

                var order = game.Guesses.Count + 1;

                var imageGuess = new ImageGuess
                {
                    Order = order,
                    User = user,
                    Image = model.Image
                };

                if (order < game.NumberOfGuesses) imageGuess.NextUser = nextUser;
                else game.GameOverMan = true;

                game.Guesses.Add(imageGuess);

                UnitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private bool NextUserAlreadyHadAGo(Game game, int nextUserId)
        {
            return game.Guesses.Any(x => x.User.Id == nextUserId);
        }

        // POST /api/game/guessword
        /// <summary>
        /// Post up word guess, with next user. Store all that in Guess table. Return 201 if all good, 406 if next user has already had a go on this one.
        /// If this is last guess then dont set next user and mark game as over.
        /// </summary>
        /// <returns></returns>
        [Route("GuessWord")]
        public HttpResponseMessage PostWordGuess(WordModel model)
        {
            try
            {
                var user = UnitOfWork.User.GetById(model.UserId);
                var nextUser = UnitOfWork.User.GetById(model.NextUserId);
                var game = UnitOfWork.Game.GetById(model.GameId);

                if (NextUserAlreadyHadAGo(game, model.NextUserId))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        String.Format("Please pick another user, {0} has already had a turn", nextUser.Username));

                SetPreviousGuessAsComplete(game, model.UserId);

                var order = game.Guesses.Count + 1;

                var wordGuess = new WordGuess
                {
                    Order = order,
                    User = user,
                    Word = model.Word
                };

                if (order < game.NumberOfGuesses) wordGuess.NextUser = nextUser;
                else game.GameOverMan = true;

                game.Guesses.Add(wordGuess);

                UnitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/game/Guesses
        /// <summary>
        /// Returns collection of guesses for user
        /// </summary>
        /// <returns></returns>
        [Route("Guesses/{userId}")]
        public HttpResponseMessage GetGuesses(int userId)
        {
            try
            {
                var guesses = UnitOfWork.Guess.SearchFor(x => x.NextUser.Id == userId && !x.Complete);
                var gameModelList = new List<GameBaseModel>();

                foreach (var guess in guesses)
                {
                    if (guess is WordGuess)
                    {
                        var wordGuess = (WordGuess) guess;
                        var wordModel = new WordModel
                        {
                            GameId = wordGuess.Game.Id,
                            UserId = wordGuess.NextUser.Id,
                            Word = wordGuess.Word
                        };

                        gameModelList.Add(wordModel);
                    }
                    else if (guess is ImageGuess)
                    {
                        var imageGuess = (ImageGuess) guess;

                        var imageModel = new ImageModel
                        {
                            GameId = imageGuess.Game.Id,
                            UserId = imageGuess.NextUser.Id,
                            Image = imageGuess.Image
                        };

                        gameModelList.Add(imageModel);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, gameModelList);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/game/Results
        /// <summary>
        /// Returns collection of finished games for user
        /// </summary>
        /// <returns></returns>
        [Route("Results/{userId}")]
        public HttpResponseMessage GetResults(int userId)
        {
            try
            {
                var results = UnitOfWork.Guess
                    .SearchFor(x => x.User.Id == userId && x.Game.GameOverMan)
                    .Select(y => new GamesModel{GameId = y.Game.Id, StartingWord = y.Game.StartingWord,
                        NumberOfGuesses = y.Game.NumberOfGuesses, GameOverMan = y.Game.GameOverMan}).ToList();

                foreach (var result in results)
                {
                    var game = UnitOfWork.Game.GetById(result.GameId);
                    result.Guesses = new List<GameBaseModel>();

                    foreach (var guess in game.Guesses.OrderBy(x => x.Order))
                    {
                        if (guess is WordGuess)
                        {
                            var wordGuess = (WordGuess)guess;
                            var wordModel = new WordModel
                            {
                                GameId = wordGuess.Game.Id,
                                UserId = wordGuess.NextUser == null ? 0 : wordGuess.NextUser.Id,
                                Word = wordGuess.Word
                            };

                            result.Guesses.Add(wordModel);
                        }
                        else if (guess is ImageGuess)
                        {
                            var imageGuess = (ImageGuess)guess;

                            var imageModel = new ImageModel
                            {
                                GameId = imageGuess.Game.Id,
                                UserId = imageGuess.NextUser == null ? 0 : imageGuess.NextUser.Id,
                                Image = imageGuess.Image
                            };

                            result.Guesses.Add(imageModel);
                        }
                    }
                    
                }

                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        private void SetPreviousGuessAsComplete(Game game, int userId)
        {
            var previousGuess = game.Guesses.SingleOrDefault(x => x.NextUser.Id == userId);
            if (previousGuess != null) previousGuess.Complete = true;
        }
    }
}