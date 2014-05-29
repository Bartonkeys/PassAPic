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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.PushRegistration;
using PassAPic.Data;
using PassAPic.Models;
using PassAPic.Providers;
using PassAPic.Results;

namespace PassAPic.Controllers
{

    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : BaseController
    {
        private const string LocalLoginProvider = "Local";


        [Inject]
        public AccountController(IUnitOfWork unitOfWork)
            : this(Startup.UserManagerFactory(), Startup.OAuthOptions.AccessTokenFormat)
        {
            UnitOfWork = unitOfWork;
        }

        public AccountController(UserManager<IdentityUser> userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public UserManager<IdentityUser> UserManager { get; private set; }
        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // POST api/Account/Anon
        /// <summary>
        ///     This is registration to get us started, so very simply. POST with JSON in the body with just username
        ///     and API will return username and userId, which can be stored in device.
        ///     Leave the username blank and API will generate a random 8 character username.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Anon")]
        [AllowAnonymous]
        public HttpResponseMessage RegisterAnon(AccountModel model)
        {
            try
            {
                var newUser = new User
                {
                    Username = String.IsNullOrEmpty(model.Username) ? GenerateRandomUsername() : model.Username,
                };
                UnitOfWork.User.Insert(newUser);
                UnitOfWork.Commit();
                model.UserId = newUser.Id;
                model.Username = newUser.Username;
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
        [Route("Online")]
        [AllowAnonymous]
        public HttpResponseMessage GetUsersOnline()
        {
            try
            {
                List<AccountModel> usersOnline =
                    UnitOfWork.User.SearchFor(x => x.IsOnline)
                        .Select(y => new AccountModel {UserId = y.Id, Username = y.Username})
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
                        .Select(y => new AccountModel {UserId = y.Id, Username = y.Username})
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
        [Route("Login/{userId}")]
        [AllowAnonymous]
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

        // POST api/Account/Login
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

        private String GenerateRandomUsername()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
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

            var user = UnitOfWork.User.SearchFor(x => x.FacebookId == int.Parse(fbUser.ID)).FirstOrDefault();

            if (user == null)
            {
                var papUser = new User
                    {
                        Username = String.Format("{0} {1}",fbUser.FirstName, fbUser.LastName),
                        FacebookId = int.Parse(fbUser.ID),
                        IsOnline = true
                    };
                    UnitOfWork.User.Insert(papUser);
                    UnitOfWork.Commit();
                var accountModel = new AccountModel
                {
                    UserId = papUser.Id,
                    Username = papUser.Username,
                    OpenGames = new List<GamesModel>()
                };
                    return Request.CreateResponse(HttpStatusCode.Created, accountModel);
            }
            else
            {
                var openGames =  user.Games.Select(y => new GamesModel
                {
                    GameId = y.Id,
                    StartingWord = y.StartingWord,
                    NumberOfGuesses = y.NumberOfGuesses,
                    GameOverMan = y.GameOverMan
                }).ToList();

                var accountModel = new AccountModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    OpenGames = openGames
                };

                return Request.CreateResponse(HttpStatusCode.OK, accountModel);
            }
        }



        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static readonly RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits%bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits/bitsPerByte;

                var data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
