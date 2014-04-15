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
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        protected AnimatedGifService AnimatedGifService;
        static readonly string ServerUploadFolder = Path.GetTempPath();

        [Inject]
        public GameController(IUnitOfWork unitOfWork, IPushProvider pushProvider, ICloudImageProvider cloudImageProvider)
        {
            UnitOfWork = unitOfWork;
            Words = UnitOfWork.Word.GetAll().Select(x => x.word).ToList();
            PushRegisterService = new PushRegisterService(pushProvider);
            CloudImageService = new CloudImageService(cloudImageProvider);
            AnimatedGifService = new AnimatedGifService(CloudImageService, unitOfWork);
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
                    Word = game.StartingWord,
                    CreatorId = game.Creator.Id
                };

                return Request.CreateResponse(HttpStatusCode.Created, wordModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// The game creator can call this API to get a new word for their just started game. 
        /// Only game creator can change and it can only happen at start of game
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Route("NewWord/{gameId}/{userId}")]
        public HttpResponseMessage GetNewWord(int gameId, int userId)
        {
            try
            {
                var game = UnitOfWork.Game.GetById(gameId);

                if (game.Creator.Id != userId) return Request.CreateResponse(HttpStatusCode.Forbidden);
                if (game.Guesses.Count > 0) return Request.CreateResponse(HttpStatusCode.NotAcceptable);

                game.StartingWord = Words[random.Next(Words.Count)];

                UnitOfWork.Game.Update(game);
                UnitOfWork.Commit();

                var wordModel = new WordModel
                {
                    GameId = game.Id,
                    UserId = game.Creator.Id,
                    Word = game.StartingWord,
                    CreatorId = game.Creator.Id
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

                if (CurrentUserAlreadyHadAGo(game, model.UserId))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        String.Format("You have already submitted this guess"));

                SetPreviousGuessAsComplete(game, model.UserId);

                var order = game.Guesses.Count + 1;

                var imageGuess = new ImageGuess
                {
                    Order = order,
                    User = user,
                    Image = model.Image
                };

                if (model.IsLastTurn) game.GameOverMan = true;
                else imageGuess.NextUser = nextUser;

                game.Guesses.Add(imageGuess);

                UnitOfWork.Commit();

                if (!model.IsLastTurn) SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new image to guess!", user.Username)); 

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private bool CurrentUserAlreadyHadAGo(Game game, int userId)
        {
            return game.Guesses.Any(x => x.User.Id == userId);
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
        public async Task<HttpResponseMessage> PostWordGuess(WordModel model)
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

                if (model.IsLastTurn) game.GameOverMan = true;
                else wordGuess.NextUser = nextUser;

                game.Guesses.Add(wordGuess);

                UnitOfWork.Commit();

                if (model.IsLastTurn)
                {
                    //DO We want to send a push here too?
                    var tempAnimatedGif = HttpContext.Current.Server.MapPath("~/App_Data/" + game.Id + ".gif");
                    Task.Run(() => AnimatedGifService.CreateAnimatedGif(game.Id, tempAnimatedGif));                   
                }
                else
                {
                    SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new word to draw!", user.Username));
                } 

                return Request.CreateResponse(HttpStatusCode.Created, "Push message sent successfully");
            }
            catch (PushMessageException pushEx)
            {
                //Push Message Exception is NOT fatal to the client so we hide the exception
                //Response body could be checked if required
                _log.Error(pushEx);
                return Request.CreateResponse(HttpStatusCode.Created, pushEx.Message);
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
                    var isLastTurn = guess.Game.Guesses.Count() + 1 >= guess.Game.NumberOfGuesses;

                    if (guess is WordGuess)
                    {
                        var wordGuess = (WordGuess) guess;
                        var wordModel = new WordModel
                        {
                            GameId = wordGuess.Game.Id,
                            UserId = wordGuess.NextUser.Id,
                            Word = wordGuess.Word,
                            IsLastTurn = isLastTurn,
                            CreatorId = guess.Game.Creator.Id
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
                            Image = imageGuess.Image,
                            IsLastTurn = isLastTurn,
                            CreatorId = guess.Game.Creator.Id
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

                    foreach (var guess in game.Guesses.OrderBy(x => x.Order))
                    {
                        if (guess is WordGuess)
                        {
                            var wordGuess = (WordGuess)guess;
                            var wordModel = new WordModel
                            {
                                GameId = wordGuess.Game.Id,
                                UserId = wordGuess.NextUser == null ? 0 : wordGuess.NextUser.Id,
                                Word = wordGuess.Word,
                                CreatorId = game.Creator.Id
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
                                Image = imageGuess.Image,
                                CreatorId = game.Creator.Id
                            };

                            result.Guesses.Add(imageModel);
                            
                        }
                       
                    }

                    result.Animation = game.AnimatedResult;                    
                   
                }
             
                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/game/CompletedGames
        /// <summary>
        /// Returns collection of finished games for user
        /// </summary>
        /// <returns></returns>
        [Route("CompletedGames/{userId}")]
        public HttpResponseMessage GetCompletedGames(int userId)
        {
            var completedGameModelList = new List<CompletedGamesModel>();
            try
            {
                var results = UnitOfWork.Guess
                    .SearchFor(x => x.User.Id == userId && x.Game.GameOverMan)
                    .Select(y => new CompletedGamesModel
                    {
                        GameId = y.Game.Id,
                        StartingWord = y.Game.StartingWord,
                        NumberOfGuesses = y.Game.NumberOfGuesses,
                        GameOverMan = y.Game.GameOverMan
                    }).ToList();

                foreach (var result in results)
                {
                    var game = UnitOfWork.Game.GetById(result.GameId);
                    var wordModelList = new List<WordModel>();
                    var imageModelList = new List<ImageModel>();
               
                    foreach (var guess in game.Guesses.OrderBy(x => x.Order))
                    {
                        if (guess is WordGuess)
                        {
                            var wordGuess = (WordGuess)guess;
                            var wordModel = new WordModel
                            {
                                GameId = wordGuess.Game.Id,
                                UserId = wordGuess.NextUser == null ? 0 : wordGuess.NextUser.Id,
                                Word = wordGuess.Word,
                                Order = wordGuess.Order,
                                CreatorId = game.Creator.Id
                                
                            };

                            wordModelList.Add(wordModel);
                           
                        }
                        else if (guess is ImageGuess)
                        {
                            var imageGuess = (ImageGuess)guess;

                            var imageModel = new ImageModel
                            {
                                GameId = imageGuess.Game.Id,
                                UserId = imageGuess.NextUser == null ? 0 : imageGuess.NextUser.Id,
                                Image = imageGuess.Image,
                                Order = imageGuess.Order,
                                CreatorId = game.Creator.Id
                            };

                            imageModelList.Add(imageModel);

                        }

                    }

                    result.ImageModelList = imageModelList;;
                    result.WordModelList = wordModelList;
                    completedGameModelList.Add(result);
                }


                return Request.CreateResponse(HttpStatusCode.OK, completedGameModelList);
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

                        var imageUrl = SaveImageCloudinary(image, imageName);
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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        ///POST /api/game/imageGuessMultiPart
        /// <summary>
        /// Please see test project for how to hit this api. Here are the steps:
        /// 1. Turn image into FileStream.
        /// 2. Create a StreamContent using FileStream.
        /// 3. Create a multipart form data and add StreamContent.
        /// 4. Add these values as StringContent to form data as well: gameId, userId, nextUserId, image (this is image name).
        /// 6. Post request to this api.
        /// 7. Server will use MultipartFormDataStreamProvider to save file to temp directory and then call SaveImageCloudinary()
        /// with just image path. I;ve sorted it out in ICloudImageProvider and concrete implmentations too.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("imageGuessMultiPart")]
        public async Task<HttpResponseMessage> PostImageGuessMultiPart()
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            }

            try
            {
                var streamProvider = new MultipartFormDataStreamProvider(ServerUploadFolder);

                // Read the MIME multipart asynchronously content using the stream provider we just created.
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                var userId = int.Parse(streamProvider.FormData["userId"]);
                var gameId = int.Parse(streamProvider.FormData["gameId"]);
                var nextUserId = int.Parse(streamProvider.FormData["nextUserId"]);
                var isLastTurn = Boolean.Parse(streamProvider.FormData["isLastTurn"]);
                var imageName = streamProvider.FileData.Select(entry => entry.LocalFileName).First();

                var user = UnitOfWork.User.GetById(userId);
                var nextUser = UnitOfWork.User.GetById(nextUserId);
                var game = UnitOfWork.Game.GetById(gameId);

                if (NextUserAlreadyHadAGo(game, nextUserId))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        String.Format("Please pick another user, {0} has already had a turn", nextUser.Username));

                var imagePath = Path.Combine(ServerUploadFolder, imageName);
                var imageUrl = SaveImageCloudinary(imagePath);
                SetPreviousGuessAsComplete(game, userId);

                var order = game.Guesses.Count + 1;

                var imageGuess = new ImageGuess
                {
                    Order = order,
                    User = user,
                    Image = imageUrl
                };

                if (!isLastTurn) imageGuess.NextUser = nextUser;
                else game.GameOverMan = true;

                game.Guesses.Add(imageGuess);

                UnitOfWork.Commit();

                if (!isLastTurn)
                {
                    SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new image to guess!", user.Username));
                }

                return Request.CreateResponse(HttpStatusCode.Created, "Push message sent successfully");
            }
            catch (PushMessageException pushEx)
            {
                //Push Message Exception is NOT fatal to the client so we hide the exception
                //Response body could be checked if required
                _log.Error(pushEx);
                return Request.CreateResponse(HttpStatusCode.Created, pushEx.Message);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



        #region "Helper methods"

        private string SaveImageCloudinary(Image image, string imageName)
        {
            return CloudImageService.SaveImageToCloud(image, imageName);
        }

        private string SaveImageCloudinary(string imagePath)
        {
            return CloudImageService.SaveImageToCloud(imagePath);
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
            try
            {
                if (listOfPushDevices.Count > 0) PushRegisterService.SendPush(userId, messageToPush, listOfPushDevices);
           
            }
            catch (Exception ex)
            {
                throw new PushMessageException("There has been an error while trying to send Push Message", ex);
            }
            
        }


        #endregion

    }
}
