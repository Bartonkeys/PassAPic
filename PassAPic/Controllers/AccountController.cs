using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Microsoft.Owin.Security;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.AccountManagement;
using PassAPic.Core.PushRegistration;
using PassAPic.Data;
using PassAPic.Models;

namespace PassAPic.Controllers
{

    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : BaseController
    {

        [Inject]
        public AccountController(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        // POST api/Account/Anon
        /// <summary>
        ///     This is registration to get us started. POST with JSON in the body with email
        ///     and password - populate AccountModel accordingly.
        /// 
        ///     Returns a 403 if email or password are empty or username isnt an email.
        ///     Returns a 409 if email already exists.
        /// 
        ///     The only constraint on password is it is not blank.
        ///
        ///     API will return username and userId, which can be stored in device.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Register")]
        [AllowAnonymous]
        public HttpResponseMessage PostRegister(AccountModel model)
        {
            try
            {
                if (String.IsNullOrEmpty(model.Email) || String.IsNullOrEmpty(model.Password) || !EmailVerification.IsValidEmail(model.Email)) 
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable);

                if (UnitOfWork.User.SearchFor(x => x.Email == model.Email).Any())
                    return Request.CreateResponse(HttpStatusCode.Conflict);             

                var newUser = new User
                {
                    Username = String.IsNullOrEmpty(model.Username) ? model.Email.Split('@')[0] : model.Username,
                    Email = model.Email,
                    Password = PasswordHash.CreateHash(model.Password)
                };
                UnitOfWork.User.Insert(newUser);
                UnitOfWork.Commit();
                model.UserId = newUser.Id;
                model.Username = newUser.Username;
                model.OpenGames = new OpenGamesModel();
                return Request.CreateResponse(HttpStatusCode.Created, model);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET api/Account/Online
        /// <summary>
        ///     This API will return a list of online users.
        /// </summary>
        /// <returns></returns>
        [Route("Online/{currentUserId}/{page?}/{pageSize?}")]
        [AllowAnonymous]
        public HttpResponseMessage GetUsersOnline(int currentUserId, int page = 0, int pageSize = 10)
        {
            try
            {
                List<AccountModel> usersOnline =
                    UnitOfWork.User.SearchFor(x => x.IsOnline)
                    .OrderBy(x => x.Username)
                    .Skip(pageSize * page)
                    .Take(pageSize)
                        .Select(y => new AccountModel
                        {
                            UserId = y.Id, 
                            Username = y.Username,
                            LastActivity = y.Games.Max(d => d.DateCompleted),
                            NumberOfCompletedGames = y.Games.Count(g => g.GameOverMan),
                            HasPlayedWithUserBefore = y.Games.Any(g => g.Guesses.Any(h => h.User.Id == currentUserId))
                        })
                        .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, usersOnline);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET api/Account/AllUsers
        /// <summary>
        ///     This API will return a list of all registered users.
        /// </summary>
        /// <returns></returns>
        [Route("AllUsers")]
        [AllowAnonymous]
        public HttpResponseMessage GetAllUsers()
        {
            try
            {
                List<AccountModel> users =
                    UnitOfWork.User.SearchFor(x => x.Id > 0)
                        .Select(y => new AccountModel {UserId = y.Id, Username = y.Username, Email = y.Email})
                        .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, users);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST api/Account/Login
        /// <summary>
        ///     POST this baby up to let server know user is online.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("LoginWithUserName/{userName}")]
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage LoginWithUserName(string userName)
        {
            try
            {
                User user = UnitOfWork.User.SearchFor(x => x.Username == userName).FirstOrDefault();
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                user.IsOnline = true;

                UnitOfWork.User.Update(user);
                UnitOfWork.Commit();

                var guesses = UnitOfWork.Guess.SearchFor(x => x.NextUser.Id == user.Id && !x.Complete);
                var openGameModel = PopulateOpenGamesModel(guesses);

                var accountModel = new AccountModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    LastActivity = user.Games.Max(d => d.DateCompleted),
                    NumberOfCompletedGames = user.Games.Count(g => g.GameOverMan),
                    HasPlayedWithUserBefore = true,
                    OpenGames = openGameModel
                };            

                return Request.CreateResponse(HttpStatusCode.OK, accountModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST api/Account/LoginWithPassword
        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <returns></returns>
        [Route("LoginWithPassword/{email}/{password}")]
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage LoginWithPassword(string email, string password)
        {
            try
            {
                User user = UnitOfWork.User.SearchFor(x => x.Email == email).FirstOrDefault();
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                if (!PasswordHash.ValidatePassword(password, user.Password))
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);

                user.IsOnline = true;

                UnitOfWork.User.Update(user);
                UnitOfWork.Commit();

                var guesses = UnitOfWork.Guess.SearchFor(x => x.NextUser.Id == user.Id && !x.Complete);
                var openGameModel = PopulateOpenGamesModel(guesses);

                var accountModel = new AccountModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    LastActivity = user.Games.Max(d => d.DateCompleted),
                    NumberOfCompletedGames = user.Games.Count(g => g.GameOverMan),
                    HasPlayedWithUserBefore = true,
                    OpenGames = openGameModel
                };

                return Request.CreateResponse(HttpStatusCode.OK, accountModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        // POST api/Account/Login
        /// <summary>
        ///     POST this baby up to let server know user is online.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Login/{userId}")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Login(int userId)
        {
            try
            {
                User user = UnitOfWork.User.GetById(userId);
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                user.IsOnline = true;

                UnitOfWork.User.Update(user);
                UnitOfWork.Commit();

                List<GamesModel> results = user.Games.Select(y => new GamesModel
                {
                    GameId = y.Id,
                    StartingWord = y.StartingWord,
                    NumberOfGuesses = y.NumberOfGuesses,
                    GameOverMan = y.GameOverMan
                }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST api/Account/Logout
        /// <summary>
        ///     POST this bitch up to let server know user has fucked off.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Logout/{userId}")]
        [AllowAnonymous]
        public HttpResponseMessage LogOut(int userId)
        {
            try
            {
                User user = UnitOfWork.User.GetById(userId);
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                user.IsOnline = false;

                UnitOfWork.User.Update(user);
                UnitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST api/Account/RegisterPush
        /// <summary>
        ///     POST a JObject up to register a device
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        [Route("RegisterPush")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage RegisterPush([FromBody] RegisterItem registerItem)
        {
            //If deviceType is not passed in - it will default to zero
            int deviceType = registerItem.DeviceType;
            int userId = registerItem.UserId;
            String deviceToken = registerItem.DeviceToken;

            //RegisterLibrary.RegisterDeviceWithUser(userId, deviceToken, deviceType);

            string msg = "";

            try
            {
                PushRegister registration =
                    UnitOfWork.PushRegister.SearchFor(x => x.DeviceToken == deviceToken).FirstOrDefault();
                if (registration != null)
                {
                    if (registration.UserId == userId && deviceType == PushRegisterService.DeviceTypeIos)
                    {
                        //DO NOTHING - the record already exists and is correct 
                        //NB iOS will always have the same UUID per device
                    }
                    else if (registration.UserId == userId && deviceType != PushRegisterService.DeviceTypeIos)
                    {
                        //Droid and WP8 could have multiple device IDs linking user & device - its generated each time
                        //NB this code is identical to the base case below but we may vary for other devices
                        UnitOfWork.PushRegister.Delete(registration);
                        UnitOfWork.PushRegister.Insert(new PushRegister
                        {
                            UserId = userId,
                            DeviceToken = deviceToken,
                            DeviceType = deviceType,
                            Timestamp = DateTime.Now
                        });
                    }

                    else
                    {
                        //deviceToken associated with different member ID so remove this record to prevent duplicate push meassages
                        UnitOfWork.PushRegister.Delete(registration);
                        UnitOfWork.PushRegister.Insert(new PushRegister
                        {
                            UserId = userId,
                            DeviceToken = deviceToken,
                            DeviceType = deviceType,
                            Timestamp = DateTime.Now
                        });
                    }
                }
                else
                {
                    //No entry for this deviceToken - so create a new one
                    UnitOfWork.PushRegister.Insert(new PushRegister
                    {
                        UserId = userId,
                        DeviceToken = deviceToken,
                        DeviceType = deviceType,
                        Timestamp = DateTime.Now
                    });
                }

                UnitOfWork.Commit();

                msg = "Success";
            }
            catch (Exception ex)
            {
                if (ex.InnerException.InnerException.Message.Contains("Violation of PRIMARY KEY constraint"))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
                throw;
            }


            return Request.CreateResponse(HttpStatusCode.Created, new ReturnMessage(msg));
        }

        /// <summary>
        /// Verify Facebook access token.
        /// If not authorised fire back a 403.
        /// If authorised and not registered, then register and return 201 with empty list of games and userID
        /// If authorised and registered return 200 with list of games and UserID
        /// Forget about ASP .Net identity.
        /// Uses email address as username.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("FacebookLogin/{accessToken}")]
        public async Task<HttpResponseMessage> GetFacebookLogin(string accessToken)
        {
            FacebookUserViewModel fbUser = null;
            var path = "https://graph.facebook.com/me?access_token=" + accessToken;
            var client = new HttpClient();
            var uri = new Uri(path);
            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return Request.CreateResponse(HttpStatusCode.Unauthorized);
            
            var content = await response.Content.ReadAsStringAsync();
            fbUser = Newtonsoft.Json.JsonConvert.DeserializeObject<FacebookUserViewModel>(content);
            var fbUserId = long.Parse(fbUser.ID);

            var user = UnitOfWork.User.SearchFor(x => x.FacebookId == fbUserId).FirstOrDefault();

            if (user == null)
            {
                var papUser = new User
                    {
                        Username = String.IsNullOrEmpty(fbUser.Email) ? 
                            String.Format("{0} {1}",fbUser.FirstName, fbUser.LastName)
                            : fbUser.Email,
                        FacebookId = long.Parse(fbUser.ID),
                        IsOnline = true
                    };
                    UnitOfWork.User.Insert(papUser);
                    UnitOfWork.Commit();
                var accountModel = new AccountModel
                {
                    UserId = papUser.Id,
                    Username = papUser.Username,
                    OpenGames = new OpenGamesModel()
                };
                    return Request.CreateResponse(HttpStatusCode.Created, accountModel);
            }
            else
            {

                var guesses = UnitOfWork.Guess.SearchFor(x => x.NextUser.Id == user.Id && !x.Complete);
                var openGameModel = PopulateOpenGamesModel(guesses);

                var accountModel = new AccountModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    OpenGames = openGameModel
                };

                return Request.CreateResponse(HttpStatusCode.OK, accountModel);
            }
        }
    }
}
