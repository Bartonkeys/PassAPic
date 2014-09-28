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
using Ninject;
using PassAPic.Contracts;
using PassAPic.Contracts.EmailService;
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
        private IEmailService _emailService;

        [Inject]
        public AccountController(IDataContext dataContext, IEmailService emailService)
        {
            DataContext = dataContext;
            _emailService = emailService;
        }

        [HttpPost]
        [Route("NullPasswords")]
        [AllowAnonymous]
        public HttpResponseMessage UpdateNullPasswords()
        {
            var usersWithNullPasswords = DataContext.User.Where(x => String.IsNullOrEmpty(x.Password));

            foreach (var user in usersWithNullPasswords)
            {
                user.Password = PasswordHash.CreateHash("password01");
            }
            DataContext.Commit();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("NullEmails")]
        [AllowAnonymous]
        public HttpResponseMessage UpdateNullEmails()
        {
            var usersWithNullEmails = DataContext.User.Where(x => String.IsNullOrEmpty(x.Email));

            foreach (var user in usersWithNullEmails)
            {
                user.Email = user.Username + "@passapic.com";
            }
            DataContext.Commit();
            return Request.CreateResponse(HttpStatusCode.OK);
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

                if (DataContext.User.Any(x => x.Email == model.Email))
                    return Request.CreateResponse(HttpStatusCode.Conflict);             

                var newUser = new User
                {
                    Username = String.IsNullOrEmpty(model.Username) ? model.Email.Split('@')[0] : model.Username,
                    Email = model.Email,
                    Password = PasswordHash.CreateHash(model.Password)
                };
                DataContext.User.Add(newUser);
                DataContext.Commit();
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
                    DataContext.User.Where(x => x.IsOnline)
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

                List<AccountModel> sortedUsersOnline = usersOnline.OrderBy(o => o.HasPlayedWithUserBefore).ToList();
                sortedUsersOnline.Reverse();

                return Request.CreateResponse(HttpStatusCode.OK, sortedUsersOnline);
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
                    DataContext.User.Where(x => x.Id > 0 && x.IsOnline)
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
        /// Login with email and password
        /// </summary>
        /// <returns></returns>
        [Route("Login")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Login(AccountModel model)
        {
            try
            {
                string email = model.Email;
                string password = model.Password;

                User user = DataContext.User.FirstOrDefault(x => x.Email == email);
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                if (!PasswordHash.ValidatePassword(password, user.Password))
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);

                user.IsOnline = true;

                DataContext.Commit();

                var guesses = DataContext.Guess.Where(x => x.NextUser.Id == user.Id && !x.Complete);
                var openGameModel = PopulateOpenGamesModel(guesses);

                var accountModel = new AccountModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
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

        /// <summary>
        /// HTTP GET to this API with email. Password will be randomly generated and emailed to user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("forgotPassword")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage PostForgotPassword(ResetPasswordModel model)
        {
            
            try
            {
                string email = model.Email;
                User user = DataContext.User.FirstOrDefault(x => x.Email == email);
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                var password = GenerateRandomPassword();
                user.Password = PasswordHash.CreateHash(password);
                DataContext.Commit();
                
                _emailService.SendPasswordToEmail(password, email);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// HTTP POST to this API with ResetPasswordModel populated with email, password and new password. 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("resetPassword")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage PostResetPassword(ResetPasswordModel model)
        {
            try
            {
                User user = DataContext.User.FirstOrDefault(x => x.Email == model.Email);
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                //if (!PasswordHash.ValidatePassword(model.Password, user.Password))
                //    return Request.CreateResponse(HttpStatusCode.Unauthorized);

                user.Password = PasswordHash.CreateHash(model.NewPassword);

                DataContext.Commit();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private String GenerateRandomPassword()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, 5)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
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
                User user = DataContext.User.Find(userId);
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                user.IsOnline = false;

                DataContext.Commit();

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
                    DataContext.PushRegister.FirstOrDefault(x => x.DeviceToken == deviceToken);
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
                        DataContext.PushRegister.Remove(registration);
                        DataContext.PushRegister.Add(new PushRegister
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
                        DataContext.PushRegister.Remove(registration);
                        DataContext.PushRegister.Add(new PushRegister
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
                    DataContext.PushRegister.Add(new PushRegister
                    {
                        UserId = userId,
                        DeviceToken = deviceToken,
                        DeviceType = deviceType,
                        Timestamp = DateTime.Now
                    });
                }

                DataContext.Commit();

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

            var user = DataContext.User.FirstOrDefault(x => x.FacebookId == fbUserId);

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
                    DataContext.User.Add(papUser);
                    DataContext.Commit();
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

                var guesses = DataContext.Guess.Where(x => x.NextUser.Id == user.Id && !x.Complete);
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
