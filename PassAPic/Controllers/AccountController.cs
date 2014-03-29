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
using System.Web.Http.ModelBinding;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.PushRegistration;
using PassAPic.Data;
using PassAPic.Models;
using PassAPic.Providers;
using PassAPic.Results;


namespace PassAPic.Controllers
{
    /// <summary>
    /// Responsible for registering users, logging in and out and letting client know who is online.
    /// </summary>
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
        /// This is registration to get us started, so very simply. POST with JSON in the body with just username 
        /// and API will return username and userId, which can be stored in device. 
        /// Leave the username blank and API will generate a random 8 character username.
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
                    Username = String.IsNullOrEmpty( model.Username) ? GenerateRandomUsername() : model.Username,
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
        /// This API will return a list of online users.
        /// </summary>
        /// <returns></returns>
        [Route("Online")]
        [AllowAnonymous]
        public HttpResponseMessage GetUsersOnline()
        {
            try
            {
                var usersOnline =
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

        // POST api/Account/Login
        /// <summary>
        /// POST this baby up to let server know user is online.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Login/{userId}")]
        [AllowAnonymous]
        public HttpResponseMessage Login(int userId)
        {
            try
            {
                var user = UnitOfWork.User.GetById(userId);
                if (user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                user.IsOnline = true;

                UnitOfWork.User.Update(user);
                UnitOfWork.Commit();

                var results = user.Games.Select(y => new GamesModel{GameId = y.Id, StartingWord = y.StartingWord,
                        NumberOfGuesses = y.NumberOfGuesses, GameOverMan = y.GameOverMan}).ToList();

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
        /// POST this bitch up to let server know user has fucked off.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Logout/{userId}")]
        [AllowAnonymous]
        public HttpResponseMessage LogOut(int userId)
        {
            try
            {
                var user = UnitOfWork.User.GetById(userId);
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
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

        }



        // POST api/Account/RegisterPush
        /// <summary>
        /// POST a JObject up to register a device
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
                var registration = UnitOfWork.PushRegister.SearchFor(x => x.DeviceToken == deviceToken).FirstOrDefault();
                if (registration != null)
                {
                    if (registration.UserId == userId && deviceType == PushRegisterLibrary.DeviceTypeIos)
                    {
                        //DO NOTHING - the record already exists and is correct 
                        //NB iOS will always have the same UUID per device
                    }
                    else if (registration.UserId == userId && deviceType != PushRegisterLibrary.DeviceTypeIos)
                    {
                        //Droid and WP8 could have multiple device IDs linking user & device - its generated each time
                        //NB this code is identical to the base case below but we may vary for other devices
                        UnitOfWork.PushRegister.Delete(registration);
                        UnitOfWork.PushRegister.Insert(new PushRegister() { UserId = userId, DeviceToken = deviceToken, DeviceType = deviceType, Timestamp = DateTime.Now });
                    }

                    else
                    {
                        //deviceToken associated with different member ID so remove this record to prevent duplicate push meassages
                        UnitOfWork.PushRegister.Delete(registration);
                        UnitOfWork.PushRegister.Insert(new PushRegister() { UserId = userId, DeviceToken = deviceToken, DeviceType = deviceType, Timestamp = DateTime.Now });
                    }
                    
                }
                else
                {
                    //No entry for this deviceToken - so create a new one
                    UnitOfWork.PushRegister.Insert(new PushRegister() { UserId = userId, DeviceToken = deviceToken, DeviceType = deviceType, Timestamp = DateTime.Now });
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
                else
                {throw;}
            }


            return Request.CreateResponse(HttpStatusCode.Created, msg);
       
        }

       

        #region Lets Comment out this code for a while




        //// GET api/Account/UserInfo
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //[Route("UserInfo")]
        //public UserInfoViewModel GetUserInfo()
        //{
        //    ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    return new UserInfoViewModel
        //    {
        //        UserName = User.Identity.GetUserName(),
        //        HasRegistered = externalLogin == null,
        //        LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
        //    };
        //}

        //// POST api/Account/Logout
        //[Route("Logout")]
        //public IHttpActionResult Logout()
        //{
        //    Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        //    return Ok();
        //}

        //// GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        //[Route("ManageInfo")]
        //public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        //{
        //    IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

        //    foreach (IdentityUserLogin linkedAccount in user.Logins)
        //    {
        //        logins.Add(new UserLoginInfoViewModel
        //        {
        //            LoginProvider = linkedAccount.LoginProvider,
        //            ProviderKey = linkedAccount.ProviderKey
        //        });
        //    }

        //    if (user.PasswordHash != null)
        //    {
        //        logins.Add(new UserLoginInfoViewModel
        //        {
        //            LoginProvider = LocalLoginProvider,
        //            ProviderKey = user.UserName,
        //        });
        //    }

        //    return new ManageInfoViewModel
        //    {
        //        LocalLoginProvider = LocalLoginProvider,
        //        UserName = user.UserName,
        //        Logins = logins,
        //        ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
        //    };
        //}

        //// POST api/Account/ChangePassword
        //[Route("ChangePassword")]
        //public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
        //        model.NewPassword);
        //    IHttpActionResult errorResult = GetErrorResult(result);

        //    if (errorResult != null)
        //    {
        //        return errorResult;
        //    }

        //    return Ok();
        //}

        //// POST api/Account/SetPassword
        //[Route("SetPassword")]
        //public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
        //    IHttpActionResult errorResult = GetErrorResult(result);

        //    if (errorResult != null)
        //    {
        //        return errorResult;
        //    }

        //    return Ok();
        //}

        //// POST api/Account/AddExternalLogin
        //[Route("AddExternalLogin")]
        //public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

        //    if (ticket == null || ticket.Identity == null || (ticket.Properties != null
        //        && ticket.Properties.ExpiresUtc.HasValue
        //        && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
        //    {
        //        return BadRequest("External login failure.");
        //    }

        //    ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

        //    if (externalData == null)
        //    {
        //        return BadRequest("The external login is already associated with an account.");
        //    }

        //    IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
        //        new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

        //    IHttpActionResult errorResult = GetErrorResult(result);

        //    if (errorResult != null)
        //    {
        //        return errorResult;
        //    }

        //    return Ok();
        //}

        //// POST api/Account/RemoveLogin
        //[Route("RemoveLogin")]
        //public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result;

        //    if (model.LoginProvider == LocalLoginProvider)
        //    {
        //        result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
        //    }
        //    else
        //    {
        //        result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
        //            new UserLoginInfo(model.LoginProvider, model.ProviderKey));
        //    }

        //    IHttpActionResult errorResult = GetErrorResult(result);

        //    if (errorResult != null)
        //    {
        //        return errorResult;
        //    }

        //    return Ok();
        //}

        //// GET api/Account/ExternalLogin
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        //[AllowAnonymous]
        //[Route("ExternalLogin", Name = "ExternalLogin")]
        //public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        //{
        //    if (error != null)
        //    {
        //        return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
        //    }

        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return new ChallengeResult(provider, this);
        //    }

        //    ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    if (externalLogin == null)
        //    {
        //        return InternalServerError();
        //    }

        //    if (externalLogin.LoginProvider != provider)
        //    {
        //        Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //        return new ChallengeResult(provider, this);
        //    }

        //    IdentityUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
        //        externalLogin.ProviderKey));

        //    bool hasRegistered = user != null;

        //    if (hasRegistered)
        //    {
        //        Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //        ClaimsIdentity oAuthIdentity = await UserManager.CreateIdentityAsync(user,
        //            OAuthDefaults.AuthenticationType);
        //        ClaimsIdentity cookieIdentity = await UserManager.CreateIdentityAsync(user,
        //            CookieAuthenticationDefaults.AuthenticationType);
        //        AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
        //        Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
        //    }
        //    else
        //    {
        //        IEnumerable<Claim> claims = externalLogin.GetClaims();
        //        ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
        //        Authentication.SignIn(identity);
        //    }

        //    return Ok();
        //}

        //// GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        //[AllowAnonymous]
        //[Route("ExternalLogins")]
        //public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        //{
        //    IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
        //    List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

        //    string state;

        //    if (generateState)
        //    {
        //        const int strengthInBits = 256;
        //        state = RandomOAuthStateGenerator.Generate(strengthInBits);
        //    }
        //    else
        //    {
        //        state = null;
        //    }

        //    foreach (AuthenticationDescription description in descriptions)
        //    {
        //        ExternalLoginViewModel login = new ExternalLoginViewModel
        //        {
        //            Name = description.Caption,
        //            Url = Url.Route("ExternalLogin", new
        //            {
        //                provider = description.AuthenticationType,
        //                response_type = "token",
        //                client_id = Startup.PublicClientId,
        //                redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
        //                state = state
        //            }),
        //            State = state
        //        };
        //        logins.Add(login);
        //    }

        //    return logins;
        //}

        //// POST api/Account/Register
        //[AllowAnonymous]
        //[Route("Register")]
        //public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityUser user = new IdentityUser
        //    {
        //        UserName = model.UserName
        //    };

        //    IdentityResult result = await UserManager.CreateAsync(user, model.Password);
        //    IHttpActionResult errorResult = GetErrorResult(result);

        //    if (errorResult != null)
        //    {
        //        return errorResult;
        //    }

        //    return Ok();
        //}

        //// POST api/Account/RegisterExternal
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //[Route("RegisterExternal")]
        //public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    if (externalLogin == null)
        //    {
        //        return InternalServerError();
        //    }

        //    IdentityUser user = new IdentityUser
        //    {
        //        UserName = model.UserName
        //    };
        //    user.Logins.Add(new IdentityUserLogin
        //    {
        //        LoginProvider = externalLogin.LoginProvider,
        //        ProviderKey = externalLogin.ProviderKey
        //    });
        //    IdentityResult result = await UserManager.CreateAsync(user);
        //    IHttpActionResult errorResult = GetErrorResult(result);

        //    if (errorResult != null)
        //    {
        //        return errorResult;
        //    }

        //    return Ok();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        UserManager.Dispose();
        //    }

        //    base.Dispose(disposing);
        //}
        #endregion

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
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
