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
using System.Web.Http.Routing;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.PushRegistration;
using PassAPic.Core.CloudImage;
using PassAPic.Core.WordManager;
using PassAPic.Data;
using PassAPic.Models;
using PassAPic.Core.AnimatedGif;
using Word = PassAPic.Core.WordManager.Word;

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
        protected IWordManager WordManager;
        static readonly string ServerUploadFolder = Path.GetTempPath();

        [Inject]
        public GameController(IUnitOfWork unitOfWork, IPushProvider pushProvider, ICloudImageProvider cloudImageProvider, IWordManager wordManager)
        {
            UnitOfWork = unitOfWork;
            EasyWords = UnitOfWork.EasyWord.GetAll().Select(x => x.Word).ToList();
            PushRegisterService = new PushRegisterService(pushProvider);
            CloudImageService = new CloudImageService(cloudImageProvider);
            AnimatedGifService = new AnimatedGifService(CloudImageService, unitOfWork);
            WordManager = wordManager;
        }

        // POST /api/game/start
        /// <summary>
        /// Lets play. Set up game, generate first word and return to user. Remember to set whether easy mode or not
        /// </summary>
        /// <returns></returns>
        [Route("Start")]
        public async Task<HttpResponseMessage> PostStart(GameSetupModel model)
        {
            try
            {
                var user = UnitOfWork.User.GetById(model.UserId);

                var startingWord = await WordManager.GetWord(model.Mode);

                var game = new Game
                {
                    StartingWord = model.Mode == Mode.Easy ? EasyWords[random.Next(EasyWords.Count)] : startingWord.RandomWord,
                    NumberOfGuesses = model.NumberOfPlayers,
                    Creator = user,
                    DateCreated = DateTime.UtcNow
                };

                UnitOfWork.Game.Insert(game);
                UnitOfWork.Commit();

                var wordModel = new WordModel
                {
                    GameId = game.Id,
                    UserId = user.Id,
                    Word = game.StartingWord,
                    CreatorId = game.Creator.Id,
                    Mode = model.Mode
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
        /// <param name="isEasyMode"></param>
        /// <returns></returns>
        [Route("NewWord/{gameId}/{userId}/{mode?}")]
        public async Task<HttpResponseMessage> GetNewWord(int gameId, int userId, Mode mode = Mode.Normal)
        {
            try
            {
                var game = UnitOfWork.Game.GetById(gameId);

                if (game.Creator.Id != userId) return Request.CreateResponse(HttpStatusCode.Forbidden);
                if (game.Guesses.Count > 0) return Request.CreateResponse(HttpStatusCode.NotAcceptable);

                var normalWord = await WordManager.GetWord(mode);

                game.StartingWord = mode == Mode.Easy
                    ? EasyWords[random.Next(EasyWords.Count)]
                    : normalWord.RandomWord;

                UnitOfWork.Game.Update(game);
                UnitOfWork.Commit();

                var wordModel = new WordModel
                {
                    GameId = game.Id,
                    UserId = game.Creator.Id,
                    Word = game.StartingWord,
                    CreatorId = game.Creator.Id,
                    Mode = mode
                };

                return Request.CreateResponse(HttpStatusCode.Created, wordModel);
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

                if (CurrentUserAlreadyHadAGo(game, model.UserId))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        String.Format("You have already submitted this guess"));

                SetPreviousGuessAsComplete(game, model.UserId);

                var order = game.Guesses.Count + 1;

                var wordGuess = new WordGuess
                {
                    Order = order,
                    User = user,
                    Word = model.Word,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude
                };

                if (model.IsLastTurn)
                {
                    game.GameOverMan = true;
                    game.DateCompleted = DateTime.UtcNow;
                }
                else wordGuess.NextUser = nextUser;

                game.Guesses.Add(wordGuess);

                UnitOfWork.Commit();

                if (model.IsLastTurn)
                {
                    foreach (var guess in game.Guesses) SendPushMessage(guess.User.Id, "PassAPic Complete!!! - check your Completed Games now");
                }
                else
                {
                    SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new word to draw!", user.Username));
                } 

                return Request.CreateResponse(HttpStatusCode.Created, new ReturnMessage("Push message sent successfully"));
            }
            catch (PushMessageException pushEx)
            {
                //Push Message Exception is NOT fatal to the client so we hide the exception
                //Response body could be checked if required
                _log.Error(pushEx);
                return Request.CreateResponse(HttpStatusCode.Created, new ReturnMessage(pushEx.Message));
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

                return Request.CreateResponse(HttpStatusCode.OK, PopulateOpenGamesModel(guesses));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        // GET /api/game/CompletedGames
        /// <summary>
        /// Returns a Paginated collection of finished games for user, latest game at top.
        /// 
        /// Call as follows: /api/game/completedGames/{userId}/{page}/{pageSze}
        /// 
        /// page and pageSize are optional so if left out will return first 10 results.
        /// 
        /// Now listen up , this is important: You will get a new Response Header - X-Pagination
        /// 
        /// It will have the following format:
        /// 
        /// {"TotalCount":78,"TotalPages":8,"PrevPageLink":"thePreviousLink","NextPageLink":"theNextLink"}
        /// 
        /// Use wisely my brothers of the Watch!
        /// </summary>
        /// <returns></returns>
        [Route("completedGames/{userId}/{page?}/{pageSize?}", Name="CompletedGames")]
        public HttpResponseMessage GetCompletedGames(int userId, int page = 0, int pageSize = 10)
        {
            var completedGameModelList = new List<CompletedGamesModel>();
            try
            {
                var results = UnitOfWork.Guess
                    .SearchFor(x => x.User.Id == userId && x.Game.GameOverMan)
                    .OrderByDescending(x => x.Game.DateCompleted)
                    .Skip(pageSize * page)
                    .Take(pageSize)
                    .Select(y => new CompletedGamesModel
                    {
                        GameId = y.Game.Id,
                        StartingWord = y.Game.StartingWord,
                        NumberOfGuesses = y.Game.NumberOfGuesses,
                        GameOverMan = y.Game.GameOverMan,
                        DateCreated = y.Game.DateCreated,
                        DateCompleted = y.Game.DateCompleted,
                        Animation = y.Game.AnimatedResult
                        
                    }).ToList();

                PopulatePaginationHeaderForAction(userId, page, pageSize, "CompletedGames");

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

        private void PopulatePaginationHeaderForAction(int userId, int page, int pageSize, string action)
        {
            var totalCount = UnitOfWork.Guess.SearchFor(x => x.User.Id == userId && x.Game.GameOverMan).Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var urlHelper = new UrlHelper(Request);
            var prevLink = page > 0 ? urlHelper.Link(action, new { userId, page = page - 1, pageSize = pageSize }) : "";
            var nextLink = page < totalPages - 1 ? urlHelper.Link(action, new { userId, page = page + 1, pageSize = pageSize }) : "";

            var paginationHeader = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PrevPageLink = prevLink,
                NextPageLink = nextLink
            };

            HttpContext.Current.Response.Headers.Add("X-Pagination",
                                                                Newtonsoft.Json.JsonConvert.SerializeObject(paginationHeader));
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
                var latitude = streamProvider.FormData["latitude"] == null ? 0.0 : double.Parse(streamProvider.FormData["latitude"]);
                var longitude = streamProvider.FormData["longitude"] == null ? 0.0 : double.Parse(streamProvider.FormData["longitude"]);
                var imageName = streamProvider.FileData.Select(entry => entry.LocalFileName).First();
                
                var user = UnitOfWork.User.GetById(userId);
                var nextUser = UnitOfWork.User.GetById(nextUserId);
                var game = UnitOfWork.Game.GetById(gameId);

                if (NextUserAlreadyHadAGo(game, nextUserId))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        String.Format("Please pick another user, {0} has already had a turn", nextUser.Username));

                if (CurrentUserAlreadyHadAGo(game, userId))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        String.Format("You have already submitted this guess"));

                var imagePath = Path.Combine(ServerUploadFolder, imageName);
                 var imageUrl = SaveImageToCloud(imagePath, imageName.Split('\\').Last());
                SetPreviousGuessAsComplete(game, userId);

                var order = game.Guesses.Count + 1;

                var imageGuess = new ImageGuess
                {
                    Order = order,
                    User = user,
                    Image = imageUrl,
                    Latitude = latitude,
                    Longitude = longitude
                };

                if (!isLastTurn) imageGuess.NextUser = nextUser;
                else
                {
                    game.GameOverMan = true;
                    game.DateCompleted = DateTime.UtcNow;
                }

                game.Guesses.Add(imageGuess);

                UnitOfWork.Commit();

                if (!isLastTurn)
                {
                    SendPushMessage(nextUser.Id, String.Format("{0} has sent you a new image to guess!", user.Username));
                }

                return Request.CreateResponse(HttpStatusCode.Created, new ReturnMessage("Push message sent successfully"));
            }
            catch (PushMessageException pushEx)
            {
                //Push Message Exception is NOT fatal to the client so we hide the exception
                //Response body could be checked if required
                _log.Error(pushEx);
                return Request.CreateResponse(HttpStatusCode.Created, new ReturnMessage( pushEx.Message));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #region "Helper methods"

        private string SaveImageToCloud(string imagePath, string imageName)
        {
            return CloudImageService.SaveImageToCloud(imagePath, imageName);
        }

        private void SetPreviousGuessAsComplete(Game game, int userId)
        {
            var previousGuess = game.Guesses.SingleOrDefault(x => x.NextUser.Id == userId);
            if (previousGuess != null) previousGuess.Complete = true;
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
