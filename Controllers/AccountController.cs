using System;
using System.Text.RegularExpressions; 
using System.Diagnostics.CodeAnalysis;
using Orchard.Localization;
using System.Web.Mvc;
using System.Web.Security;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Users.Services;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Utility.Extensions;
using Contrib.Foundation.UserProfile.Services;
using Contrib.Foundation.Common.Services;
using Contrib.Foundation.Application.Services;
using Contrib.Foundation.Application.Models;

namespace Contrib.Foundation.UserProfile.Controllers
{
    [HandleError, Themed]
    public class AccountController : Controller {
        private readonly IProfileService _profileService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IDetectMobileService _detectMobileService;
        private readonly IApplicationsService _applicationsService;
        public AccountController(
            IUserEventHandler userEventHandler,
            IProfileService profileService,
            IDetectMobileService detectMobileService,
            IApplicationsService applicationsService
            )
        {
            _userEventHandler = userEventHandler;
            _profileService = profileService;
            _detectMobileService = detectMobileService;
            _applicationsService = applicationsService;
        }

        public ActionResult RegistrationPending() {
            return View();
        }

        public ActionResult ChallengeEmailSent() {
            return View();
        }

        public ActionResult ChallengeEmailSuccess() {
            return View();
        }

        public ActionResult ChallengeEmailFail() {
            return View();
        }

        public ActionResult ChallengeEmail(string nonce) {
            string appKey = null;
            var user = _profileService.ValidateChallenge(nonce, out appKey);
            
            if ( user != null ) {

                _userEventHandler.ConfirmedEmail(user);
                ApplicationRecord app = _applicationsService.GetApplicationByKey(appKey);
                if (app == null)
                {
                    return RedirectToAction("ChallengeEmailSuccess");
                }
                else
                {
                    if (!_detectMobileService.isMobileBrowser(Request.UserAgent))
                    {
                        return RedirectToAction("ChallengeEmailSuccess");
                    }
                    else
                    {
                        string protocol = _applicationsService.GetApplicationProtocol(app.Id);
                        return Redirect(protocol + "challengeemailsuccess?user="+user.Email);
                    }
                }
            }

            return RedirectToAction("ChallengeEmailFail");
        }
    }
}