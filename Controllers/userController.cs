using Contrib.Foundation.Application.Models;
using Contrib.Foundation.Application.Services;
using Contrib.Foundation.Common.Extensions;
using Contrib.Foundation.Common.OData;
using Contrib.Foundation.UserProfile.Models;
using Contrib.Foundation.UserProfile.OData;
using Contrib.Foundation.UserProfile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Contrib.Foundation.UserProfile.Controllers
{
    public class userController: ApiController
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IApplicationsService _applicationsService;
        private readonly IProfileService _profileService;
        public Localizer T { get; set; }
        public userController(
            IOrchardServices orchardServices,
            IMembershipService membershipService,
            IApplicationsService applicationsService,
            IProfileService profileService
            )
        {
            _membershipService = membershipService;
            _orchardServices = orchardServices;
            _applicationsService = applicationsService;
            _profileService = profileService;

            T = NullLocalizer.Instance;
        }
        [ActionName("Profile")]
        public HttpResponseMessage GetProfile(string username = null)
        {
            IUser user = null;
            if(string.IsNullOrWhiteSpace(username))
            {
                user = _orchardServices.WorkContext.CurrentUser;
            }
            else
            {
                user = _membershipService.GetUser(username);
            }
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            
            Contrib.Foundation.UserProfile.OData.Profile profile = new Contrib.Foundation.UserProfile.OData.Profile(user, Request, null);
            return Request.CreateResponse(HttpStatusCode.OK, profile);
        }

        [ActionName("Profile")]
        public HttpResponseMessage PutProfile(uProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.Username) || profile.Id <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new uError("Not Found", 404));
            }

            IUser user = _orchardServices.WorkContext.CurrentUser;
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new uError("User not authorized", 401));
            }
            if (user.UserName != profile.Username || user.Id != profile.Id)
            {
                if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new uError("User not authorized", 401));
                }
            }


            var userp = _orchardServices.ContentManager.Get<UserPart>(profile.Id);
            profile.updateProfile(userp.As<UserProfilePart>());

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [ActionName("Profile")]
        public HttpResponseMessage PatchProfile(uProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.Username) || profile.Id <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new uError("Not Found", 404));
            }

            IUser user = _orchardServices.WorkContext.CurrentUser;
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new uError("User not authorized", 401));
            }
            if (user.UserName != profile.Username || user.Id != profile.Id)
            {
                if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new uError("User not authorized", 401));
                }
            }


            var userp = _orchardServices.ContentManager.Get<UserPart>(profile.Id);
            profile.patchProfile(userp.As<UserProfilePart>());

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [ActionName("ProfileField")]
        [HttpGet]
        public HttpResponseMessage GetProfileField(string username, string field)
        {
            IUser user = null;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(field))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new uError("Not Found", 404));                
            }
            else
            {
                user = _membershipService.GetUser(username);
            }
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }

            Contrib.Foundation.UserProfile.OData.Profile profile = new Contrib.Foundation.UserProfile.OData.Profile(user, Request, null);

            switch(field)
            {
                case "FirstName":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.FirstName);
                case "Email":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.Email);
                case "Username":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.Username);
                case "LastName":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.LastName);
                case "WebSite":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.WebSite);
                case "Bio":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.Bio);
                case "Location":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.Location);
                case "link":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.link);
                case "Type":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.Type);
                case "Id":
                    return Request.CreateResponse(HttpStatusCode.OK, profile.Id);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [ExtendedQueryable]
        [Orchard.Core.XmlRpc.Controllers.LiveWriterController.NoCache]
        [ActionName("Roles")]
        public IQueryable<UserRole> GetRoles(string username = null, string Hash = null)
        {
            IUser user = null;
            ApplicationRecord app = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                if (_orchardServices.WorkContext.CurrentUser == null)
                    return null;
                else
                    username = _orchardServices.WorkContext.CurrentUser.UserName;
                Hash = null;
            }

            // if hash is null then we can only return data for current user
            if (Hash == null)
            {
                try
                {
                    string appid = _orchardServices.WorkContext.HttpContext.Session["doticca_aid"].ToString();

                    if (string.IsNullOrWhiteSpace(appid)) return null;
                    int aid;
                    if (!Int32.TryParse(appid, out aid)) return null;
                    user = _orchardServices.WorkContext.CurrentUser;
                    if (user.UserName.ToLower() != username.ToLower()) return null;
                    app = _applicationsService.GetApplication(aid);
                    if (app == null) return null;
                }
                catch
                {
                    return null;
                }
            }


            //else
            //{
            //    user = _membershipService.GetUser(username);
            //    if(user == null) return null;
            //    app = _applicationsService.GetApplicationByKey(appID);
            //    if(app == null) return null;
            //}

            // get roles from service
            IEnumerable<UserRoleRecord> roles = _profileService.GetUserRoles(user.As<UserProfilePart>(), app);
            // create a new list
            List<UserRole> Roles = new List<UserRole>();
            foreach (UserRoleRecord role in roles)
            {
                Roles.Add(new UserRole(user, role, Request));
            }
            return Roles.AsQueryable();
        }

        //[ExtendedQueryable]
        //[Orchard.Core.XmlRpc.Controllers.LiveWriterController.NoCache]
        //[ActionName("Users")]
        //public IQueryable<UserRole> GetUsers(string username = null, string Hash = null)
        //{
        //    IUser user = null;
        //    ApplicationRecord app = null;

        //    if (string.IsNullOrWhiteSpace(username))
        //    {
        //        if (_orchardServices.WorkContext.CurrentUser == null)
        //            return null;
        //        else
        //            username = _orchardServices.WorkContext.CurrentUser.UserName;
        //        Hash = null;
        //    }

        //    // if hash is null then we can only return data for current user
        //    if (Hash == null)
        //    {
        //        try
        //        {
        //            string appid = _orchardServices.WorkContext.HttpContext.Session["doticca_aid"].ToString();

        //            if (string.IsNullOrWhiteSpace(appid)) return null;
        //            int aid;
        //            if (!Int32.TryParse(appid, out aid)) return null;
        //            user = _orchardServices.WorkContext.CurrentUser;
        //            if (user.UserName.ToLower() != username.ToLower()) return null;
        //            app = _applicationsService.GetApplication(aid);
        //            if (app == null) return null;
        //        }
        //        catch
        //        {
        //            return null;
        //        }
        //    }


        //    //else
        //    //{
        //    //    user = _membershipService.GetUser(username);
        //    //    if(user == null) return null;
        //    //    app = _applicationsService.GetApplicationByKey(appID);
        //    //    if(app == null) return null;
        //    //}

        //    // get roles from service
        //    IEnumerable<UserRoleRecord> roles = _profileService.GetUserRoles(user.As<UserProfilePart>(), app);
        //    // create a new list
        //    List<UserRole> Roles = new List<UserRole>();
        //    foreach (UserRoleRecord role in roles)
        //    {
        //        Roles.Add(new UserRole(user, role, Request));
        //    }
        //    return Roles.AsQueryable();
        //}
    }
}