using Orchard;
using Orchard.Security;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Contrib.Foundation.UserProfile.OData;
using Orchard.Users.Models;
using Orchard.ContentManagement;
using Contrib.Foundation.Common.OData;
using System;
using Orchard.Users.Events;
using Contrib.Foundation.UserProfile.Helpers;
using Contrib.Foundation.Application.Services;
using Contrib.Foundation.Application.Models;
using Contrib.Foundation.UserProfile.Models;
using Facebook;
using System.Text.RegularExpressions;
using Contrib.Foundation.UserProfile.Services;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Extensions;

namespace Contrib.Foundation.UserProfile.Controllers
{
    public class loginController : ApiController
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IApplicationsService _applicationsService;
        private readonly IProfileService _profileService;
        private readonly ILoginsService _loginsService;

        public loginController(
                                IAuthenticationService authenticationService, 
                                IOrchardServices orchardServices,
                                IMembershipService membershipService,
                                IUserEventHandler userEventHandler,
                                IApplicationsService applicationsService,
                                IProfileService profileService,
                                ILoginsService loginsService
                               )
        {
            _authenticationService = authenticationService;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _userEventHandler = userEventHandler;
            _profileService = profileService;
            _applicationsService = applicationsService;
            _loginsService = loginsService;
        }


        [AlwaysAccessible]
        [HttpPost]
        public HttpResponseMessage Login(Login login)
        {
            IUser user = _orchardServices.WorkContext.CurrentUser;

            ApplicationRecord apprecord = _applicationsService.GetApplicationByKey(login.ApiKey);
            if (apprecord == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, new uError("Not Found", 404));

            if (user != null)
            {
                IUser newUser = ValidateLogOn(login);
                if (newUser != null && newUser.Id == user.Id)
                {
                    Contrib.Foundation.UserProfile.OData.Profile profile = new Contrib.Foundation.UserProfile.OData.Profile(user, Request, _loginsService.GetHash(user.As<UserProfilePart>(), apprecord));
                    _orchardServices.WorkContext.HttpContext.Session["doticca_aid"] = apprecord.Id;
                    return Request.CreateResponse(HttpStatusCode.OK, profile);
                }
                else
                {
                    LogOut();
                }
            }
            user = ValidateLogOn(login);
            if (user != null) {
                UserProfilePart profilePart = user.As<UserProfilePart>(); //_profileService.Get(user).As<UserProfilePart>();
                _profileService.CreateUserForApplicationRecord(profilePart, apprecord);
                _authenticationService.SignIn(user, false);
                _userEventHandler.LoggedIn(user);
                string newHash = login.Hash;
                if (string.IsNullOrWhiteSpace(newHash))
                    newHash = _loginsService.CreateHash(profilePart, apprecord);

                Contrib.Foundation.UserProfile.OData.Profile profile = new Contrib.Foundation.UserProfile.OData.Profile(user, Request, newHash);
                _orchardServices.WorkContext.HttpContext.Session["doticca_aid"] = apprecord.Id;
                return Request.CreateResponse(HttpStatusCode.OK, profile);
            }
            _orchardServices.WorkContext.HttpContext.Session.Remove("doticca_aid");
            return Request.CreateResponse(HttpStatusCode.Unauthorized, new uError("User not authorized", 401));
        }

        private IUser ValidateLogonFacebook(LoginFB login, out string Hash)
        {
            Hash = string.Empty;
            ApplicationRecord apprecord = _applicationsService.GetApplicationByKey(login.ApiKey);
            if (apprecord == null)
                return null;           // wrong cloudbast application id

            DebugFB debuginfo = FBHelper.GetDebugInfo(login.Token, apprecord);
            if (!debuginfo.isValid)
                return null;           // access token is not valid
            if (debuginfo.Application != apprecord.Name || debuginfo.AppId != apprecord.fbAppKey)
                return null;           // access token for another application

            string email = login.Username;
            var lowerEmail = email == null ? "" : email.ToLowerInvariant();

            // load user with FBemail
            IUser user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerEmail).List().FirstOrDefault();
            UserProfilePart profile = null;
            if (user == null)
            {
                var fb = new FacebookClient(login.Token);
                dynamic me = fb.Get("me");

                // since everything is correct, we have to create a new user
                var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
                if (registrationSettings.UsersCanRegister)
                {
                    // create a user with random password
                    user = _membershipService.CreateUser(new CreateUserParams(lowerEmail, Guid.NewGuid().ToString(), lowerEmail, null, null, true)) as UserPart;

                    // add facebook fields
                    profile = user.As<UserProfilePart>();
                    profile.FBemail = lowerEmail;
                    profile.FBtoken = login.Token;
                    profile.FirstName = me.first_name;
                    profile.LastName = me.last_name;
                }                
            }
            else
            {
                profile = user.As<UserProfilePart>();
                profile.FBemail = lowerEmail;
                profile.FBtoken = login.Token;
            }
            Hash = _loginsService.CreateHash(profile, apprecord);
            _profileService.CreateUserForApplicationRecord(profile, apprecord);
            _orchardServices.WorkContext.HttpContext.Session["doticca_aid"] = apprecord.Id;
            return user;
        }

        [AlwaysAccessible]
        [HttpPost]
        public HttpResponseMessage LoginFacebook(LoginFB login)
        {
            string Hash = string.Empty;
            IUser user = ValidateLogonFacebook(login, out Hash);

            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new uError("User not authorized", 401));
            }

            _authenticationService.SignIn(user, false);
            _userEventHandler.LoggedIn(user);

            Contrib.Foundation.UserProfile.OData.Profile profile = new Contrib.Foundation.UserProfile.OData.Profile(user, Request, Hash);
            return Request.CreateResponse(HttpStatusCode.OK, profile);                            
        }

        [AlwaysAccessible]
        [HttpGet]
        public HttpResponseMessage LogOut(string Hash = null)
        {
            var wasLoggedInUser = _authenticationService.GetAuthenticatedUser();
            _authenticationService.SignOut();
            _orchardServices.WorkContext.HttpContext.Session.Remove("doticca_aid");
            if (wasLoggedInUser != null)
            {
                if (Hash != null)
                {
                    _loginsService.DeleteHash(Hash);
                }
                _userEventHandler.LoggedOut(wasLoggedInUser);
            }


            return Request.CreateResponse(HttpStatusCode.OK, "User succesfully logged out");
        }

        public IUser GetUserByMail(string email)
        {
            var lowerName = email == null ? "" : email.ToLowerInvariant();

            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerName).List().FirstOrDefault();
        }

        private IUser ValidateLogOn(Login login)
        {
            bool validate = true;
            IUser user = null;

            if (!string.IsNullOrWhiteSpace(login.Hash))
            {
                user = _loginsService.ValidateHash(login.Hash, login.ApiKey);
                return user; 
            }

            if (String.IsNullOrEmpty(login.Username))
            {
                validate = false;
            }
            if (String.IsNullOrEmpty(login.Password))
            {
                validate = false;
            }

            if (!validate)
                return null;

            user = _membershipService.ValidateUser(login.Username, login.Password);
            if (user == null)
            {
                return null;
            }

            return user;
        }

        private int MinPasswordLength
        {
            get
            {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }

        [HttpPost]
        [AlwaysAccessible]
        [System.Web.Mvc.ValidateInput(false)]
        public HttpResponseMessage Register(Register Register)
        {
            // ensure users can register
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.UsersCanRegister)
            {
                return Request.CreateResponse(HttpStatusCode.MethodNotAllowed, new uError("Method Not Allowed", 405));
            }

            if (Register.Password.Length < MinPasswordLength)
            {
                return Request.CreateResponse(HttpStatusCode.MethodNotAllowed, new uError("Method Not Allowed", 405));
            }

            if (!_profileService.VerifyUserUnicity(Register.Email, Register.Email))
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, new uError("Conflict on the Server", 409));
            }
            ApplicationRecord apprecord = _applicationsService.GetApplicationByKey(Register.ApiKey);
            if (apprecord == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, new uError("Not Found", 404));

            if (ValidateRegistration(Register))
            {
                // Attempt to register the user
                // No need to report this to IUserEventHandler because _membershipService does that for us
                var user = _membershipService.CreateUser(new CreateUserParams(Register.Email, Register.Password, Register.Email, null, null, false));

                if (user != null)
                {
                    UserProfilePart profile = user.As<UserProfilePart>();
                    if (profile != null)
                    {
                        profile.FirstName = Register.FirstName;
                        profile.LastName = Register.LastName;
                    }
                    if (user.As<UserPart>().EmailStatus == UserStatus.Pending)
                    {
                        var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                        //if (String.IsNullOrWhiteSpace(siteUrl))
                        //{
                        //    siteUrl = Request.ToRootUrlString();
                        //}
                        //var url = Url.Route("challengeemail", new { controller = "login", action = "ChallengeEmail", returnUrl = "hello" });

                        var _Url = new System.Web.Mvc.UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);

                        _profileService.SendChallengeMail(
                            apprecord, 
                            user.As<UserPart>(),
                            nonce =>

                                 _Url.MakeAbsolute(
                                    _Url.Action("ChallengeEmail", "Account", new 
                                        {
                                            Area = "Contrib.Foundation.UserProfile", 
                                            nonce = nonce
                                        }
                                    )
                                )

                                //_Url.MakeAbsolute(
                                //    _Url.Action("ChallengeEmail", "login", new
                                //        {
                                //            httproute = true,
                                //            area = "Contrib.Foundation.UserProfile",
                                //            nonce = nonce
                                //        }
                                //    )
                                //)

                                //protocolChallengeEmail(nonce)
                            );
                        _userEventHandler.SentChallengeEmail(user);
                        return Request.CreateResponse(HttpStatusCode.Created, new uError("Create", 201,false));
                    }

                    if (user.As<UserPart>().RegistrationStatus == UserStatus.Pending)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotModified, new uError("Not Modified", 304));
                    }

                    _authenticationService.SignIn(user, false);
                    return Request.CreateResponse(HttpStatusCode.OK, new uError("OK", 200));
                }

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new uError("Internal Server Error", 500));
            }

            return Request.CreateResponse(HttpStatusCode.InternalServerError, new uError("Internal Server Error", 500)); ;
        }

        private string protocolChallengeEmail(string nonce)
        {
            return "alterniity://challenge?nonce="+nonce;
        }
        private bool ValidateRegistration(Register Register)
        {
            bool validate = true;

            if (String.IsNullOrEmpty(Register.Email))
            {     
                validate = false;
            }
            else if (Register.Email.Length >= 255)
            {
                validate = false;
            }
            else if (!Regex.IsMatch(Register.Email, UserPart.EmailPattern, RegexOptions.IgnoreCase))
            {
                validate = false;
            }
            if (Register.Password == null || Register.Password.Length < MinPasswordLength)
            {
                validate = false;
            }
            if (!validate)
                return false;

            return true;
        }

    }
}