using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Data;
using PassAPic.Models;
using PassAPic.Core.AnimatedGif;

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
                var gameModelList = new OpenGamesModel();
                var wordModelList = new List<WordModel>();
                var imageModelList = new List<ImageModel>();

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

                        wordModelList.Add(wordModel);
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

                        imageModelList.Add(imageModel);
                    }
                }

                gameModelList.WordModelList = wordModelList;
                gameModelList.ImageModelList = imageModelList;

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
                    var imageFilePaths = new String[result.NumberOfGuesses];
                    var filePathServer = "";
                    var tempDirName = "temp_" + Guid.NewGuid();
                    var filePathTemp = HttpContext.Current.Server.MapPath("~/App_Data/temp/" + tempDirName);
                    
                   // var inputPath = filePathServer + "images\\";
                    //var outputPath = filePathServer + "animation\\";

                    //Delete all files in temp dir
                    //string[] fileNames = Directory.GetFiles(filePathTemp);
                    //foreach (string fileName in fileNames)
                    //    File.Delete(fileName);

                    //Create new folder
                    Directory.CreateDirectory(filePathTemp);


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
                            filePathServer = HttpContext.Current.Server.MapPath("~/App_Data/temp/" + tempDirName + "/" + wordGuess.Order + ".png");

                            Image wordImage = TextToImageConversion.CreateBitmapImage(wordGuess.Word);
                            wordImage.Save(filePathServer, ImageFormat.Png);

                            imageFilePaths[wordGuess.Order - 1] = filePathServer;
                         

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
                            String imageStr = imageGuess.Image;

                            //Bitmap bmpFromString = imageStr.Base64StringToBitmap();

                            byte[] byteBuffer = Convert.FromBase64String(imageStr);
                            MemoryStream memoryStream = new MemoryStream(byteBuffer);

                            memoryStream.Position = 0;

                            Bitmap bmpFromString = (Bitmap)Bitmap.FromStream(memoryStream);

                            //memoryStream.Close();

                            filePathServer = HttpContext.Current.Server.MapPath("~/App_Data/temp/" + tempDirName + "/" + imageGuess.Order + ".png");

                            bmpFromString.Save(filePathServer, ImageFormat.Png);
                            imageFilePaths[imageGuess.Order - 1] = filePathServer;

                            memoryStream.Close();
                        }
                       
                    }

                    //Add the new Animated Gif to this Result object
                    filePathServer = HttpContext.Current.Server.MapPath("~/App_Data/" + result.GameId + ".gif");
                    if (File.Exists(filePathServer))
                    {
                        File.Delete(filePathServer);
                    }
                    Image animatedGif = AnimatedGifController.CreateAnimatedGifFromBitmapArray(imageFilePaths, false,
                                                                                               filePathServer);
                    string animatedGifAsbase64 = ImageToBase64(animatedGif, ImageFormat.Gif);
                    result.Animation = animatedGifAsbase64;  
                   
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

        public static Bitmap Base64StringToBitmap(string base64String)
        {
            Bitmap bmpReturn = null;

            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);

            memoryStream.Position = 0;

            bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);

            memoryStream.Close();

            return bmpReturn;
        }

        public string ImageToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }

    
}