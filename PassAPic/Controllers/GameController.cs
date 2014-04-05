using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.PushRegistration;
using PassAPic.Core.CloudImage;
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
        protected PushRegisterService PushRegisterService;
        protected CloudImageService CloudImageService;

        [Inject]
        public GameController(IUnitOfWork unitOfWork, IPushProvider pushProvider, ICloudImageProvider cloudImageProvider)
        {
            UnitOfWork = unitOfWork;
            Words = UnitOfWork.Word.GetAll().Select(x => x.word).ToList();
            PushRegisterService = new PushRegisterService(pushProvider);
            CloudImageService = new CloudImageService(cloudImageProvider);
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

                //SendPushMessage(nextUser.Id, PushRegisterService.ImageGuessPushString);
                SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new image to guess!", user.Username)); 

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

                //SendPushMessage(nextUser.Id, PushRegisterService.WordGuessPushString);
                SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new word to draw!", user.Username)); 

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
                    var filePathServerAnimatedGif = HttpContext.Current.Server.MapPath("~/App_Data/" + result.GameId + ".gif");

                    bool animationExists = File.Exists(filePathServerAnimatedGif);

                    var tempDirName = "temp_" + Guid.NewGuid();
                    var filePathTemp = HttpContext.Current.Server.MapPath("~/App_Data/temp/" + tempDirName);
                    
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

                            if (!animationExists)
                            {
                                filePathServer = HttpContext.Current.Server.MapPath("~/App_Data/temp/" + tempDirName + "/" + wordGuess.Order + ".png");

                                Image wordImage = TextToImageConversion.CreateBitmapImage(wordGuess.Word);
                                wordImage.Save(filePathServer, ImageFormat.Png);

                                imageFilePaths[wordGuess.Order - 1] = filePathServer;
                         
                            }
                            

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

                            if (!animationExists)
                            {
                                String imageStr = imageGuess.Image;

                                //Bitmap bmpFromString = imageStr.Base64StringToBitmap();

                                byte[] byteBuffer = Convert.FromBase64String(imageStr);
                                MemoryStream memoryStream = new MemoryStream(byteBuffer);

                                memoryStream.Position = 0;

                                Bitmap bmpFromString = (Bitmap)Bitmap.FromStream(memoryStream);
                                var resizedBitmap = ResizeBitmap(bmpFromString, MyGlobals.ImageWidth, MyGlobals.ImageHeight);
                                //memoryStream.Close();

                                filePathServer = HttpContext.Current.Server.MapPath("~/App_Data/temp/" + tempDirName + "/" + imageGuess.Order + ".png");

                                resizedBitmap.Save(filePathServer, ImageFormat.Png);
                                imageFilePaths[imageGuess.Order - 1] = filePathServer;

                                memoryStream.Close(); 
                            }
                            
                        }
                       
                    }

                    //Add the new Animated Gif to this Result object
                    string animatedGifAsbase64;
                    if (animationExists)
                    {
                        animatedGifAsbase64 = ImageToBase64(new Bitmap(filePathServerAnimatedGif), ImageFormat.Gif);
                    }
                    else
                    {
                        Image animatedGif = AnimatedGifController.CreateAnimatedGifFromBitmapArray(imageFilePaths, false,
                                                                                               filePathServerAnimatedGif);
                        animatedGifAsbase64 = ImageToBase64(animatedGif, ImageFormat.Gif);
                    }
                    
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

        ///POST /api/game/imageGuessTask
        /// <summary>
        /// Please see test project for how to hit this api. Here are the steps:
        /// 1. Turn image into FileStream.
        /// 2. Create a StreamContent using FileStream.
        /// 3. Create a multipart form data and add StreamContent.
        /// 4. Create http request and add form data as content.
        /// 5. Add these headers to the request: gameId, userId, nextUserId, image (this is image name).
        /// 6. Send request to this api.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpPost]
        [Route("imageGuessTask")]
        public async Task<HttpResponseMessage> ImageGuessTask()
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));

            try
            {
                await Request.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider()).ContinueWith(task =>
                {
                    MultipartMemoryStreamProvider streamProvider = task.Result;
                    foreach (HttpContent content in streamProvider.Contents)
                    {
                        var userId = int.Parse(Request.Headers.GetValues("userId").First());
                        var gameId = int.Parse(Request.Headers.GetValues("gameId").First());
                        var nextUserId = int.Parse(Request.Headers.GetValues("nextUserId").First());
                        var imageName = Request.Headers.GetValues("image").First();

                        var user = UnitOfWork.User.GetById(userId);
                        var nextUser = UnitOfWork.User.GetById(nextUserId);
                        var game = UnitOfWork.Game.GetById(gameId);

                        if (NextUserAlreadyHadAGo(game, nextUserId))
                            return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                                String.Format("Please pick another user, {0} has already had a turn", nextUser.Username));

                        Stream stream = content.ReadAsStreamAsync().Result;
                        Image image = Image.FromStream(stream);

                        //var imageUrl = SaveImage(image, imageName); //TODO Save image to Sky Drive
                        var imageUrl = SaveImageToCloud(image, imageName);
                        SetPreviousGuessAsComplete(game, userId);

                        var order = game.Guesses.Count + 1;

                        var imageGuess = new ImageGuess
                        {
                            Order = order,
                            User = user,
                            Image = imageUrl
                        };

                        if (order < game.NumberOfGuesses) imageGuess.NextUser = nextUser;
                        else game.GameOverMan = true;

                        game.Guesses.Add(imageGuess);

                        UnitOfWork.Commit();

                        SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new image to guess!", user.Username));

                    }
                    return Request.CreateResponse(HttpStatusCode.Created);
                });
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



        #region "Helper methods"

        private string SaveImage(Image image, string imageName)
        {
            //TODO Handle Image - Below is just for testing
            var serverUploadFolder = Path.GetTempPath();
            image.Save(Path.Combine(serverUploadFolder, "Placeholder2.jpg"));

            return Path.Combine(serverUploadFolder, "Placeholder2.jpg");
        }

       
        private string SaveImageToCloud(Image image, string imageName)
        {
            return CloudImageService.SaveImageToCloud(image, imageName);
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

        private static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
        }

        private void SendPushMessage(int userId, String messageToPush)
        {
            //Grab the list of devices while we have access to the UnitOfWork object
            var listOfPushDevices = UnitOfWork.PushRegister.SearchFor(x => x.UserId == userId).ToList();

            //Check user has any devices registered for push
            if (listOfPushDevices.Count > 0) PushRegisterService.SendPush(userId, messageToPush, listOfPushDevices);
           
        }


        #endregion

    }
}