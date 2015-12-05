using System.Linq;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Messaging.Services;
using Orchard.DisplayManagement;
using Orchard.Settings;
using Contrib.Foundation.Application.Services;
using Orchard.Localization;
using Contrib.Foundation.Application.Models;
using System;
using System.Xml.Linq;
using System.Text;
using Orchard.Services;
using System.Globalization;
using Orchard.Users.Models;
using System.Collections.Generic;
using Orchard.Data;
using Contrib.Foundation.UserProfile.Models;
using Orchard.Caching;

namespace Contrib.Foundation.UserProfile.Services
{
    [UsedImplicitly]
    public class ProfileService : IProfileService
    {
        private static readonly TimeSpan DelayToValidate = new TimeSpan(7, 0, 0, 0); // one week to validate email

        private readonly IMessageService _messageService;
        private readonly IShapeFactory _shapeFactory;
        private readonly ISiteService _siteService;
        private readonly IApplicationsService _applicationsService;
        private readonly IContentManager _contentManager;
        private readonly IEncryptionService _encryptionService;
        private readonly IClock _clock;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IMembershipService _membershipService;
        private readonly IRepository<UserProfilePartRecord> _userprofileRepository;
        private readonly IRepository<UserApplicationRecord> _userapplicationRepository;

        private readonly ISignals _signals;
        public const string SignalName = "Contrib.Foundation.UserProfile.ProfileService";
        public ProfileService(
            IContentManager contentManager,
            IMembershipService membershipService,
            ISiteService siteService,
            IClock clock,
            IMessageService messageService,
            IShapeFactory shapeFactory,
            IApplicationsService applicationsService,
            IShapeDisplay shapeDisplay,
            IEncryptionService encryptionService,
            IRepository<UserProfilePartRecord> userprofileRepository,
            IRepository<UserApplicationRecord> userapplicationRepository,
            ISignals signals
            )
        {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _clock = clock;
            _applicationsService = applicationsService;
            _messageService = messageService;
            _shapeFactory = shapeFactory;
            _siteService = siteService;
            _encryptionService = encryptionService;
            _shapeDisplay = shapeDisplay;
            T = NullLocalizer.Instance;
            _userprofileRepository = userprofileRepository;
            _userapplicationRepository = userapplicationRepository;
            _signals = signals;
        }

        public Localizer T { get; set; }

        //public ContentItem Get(int id)
        //{
        //    return _contentManager.Get(id);
        //}

        private void TriggerSignal()
        {
            _signals.Trigger(SignalName);
        }

        public ContentPart Get(IUser owner)
        {   
            ContentItem user = owner.ContentItem;
            var profilepart = user.Parts.FirstOrDefault(p => p is ContentPart && p.TypePartDefinition.PartDefinition.Name == "UserProfilePart");
            return profilepart;
        }

        public bool CreateUserForApplicationRecord(UserProfilePart profilePart, ApplicationRecord appRecord)
        {
            UserProfilePartRecord profileRecord =  _userprofileRepository.Get(profilePart.Id);
            if (profileRecord == null) return false;

            var utcNow = _clock.UtcNow;

            var record = profileRecord.Applications.FirstOrDefault(x => x.ApplicationRecord.Name == appRecord.Name);
            if (record == null)
            {

                profileRecord.Applications.Add(new UserApplicationRecord
                {
                    UserProfilePartRecord = profileRecord,
                    ApplicationRecord = appRecord,
                    RegistrationStart = utcNow
                });

                TriggerSignal();
            }

            if (profileRecord.Roles == null || profileRecord.Roles.Count == 0)
            {
                UserRoleRecord defaultrole = _applicationsService.GetDefaultRole(appRecord);
                profileRecord.Roles.Add(new UserUserRoleRecord
                    {
                        UserProfilePartRecord = profileRecord,
                        UserRoleRecord = defaultrole
                    });
            }

            return true;
        }
        public IEnumerable<UserRoleRecord> GetUserRoles(UserProfilePart profilePart, ApplicationRecord appRecord)
        {
            UserProfilePartRecord profileRecord = _userprofileRepository.Get(profilePart.Id);
            if (profileRecord == null) return null;
            var record = profileRecord.Applications.FirstOrDefault(x => x.ApplicationRecord.Name == appRecord.Name);
            if (record == null)
            {
                return new List<UserRoleRecord>();
            }
            var Roles = new List<UserRoleRecord>();
            foreach (UserUserRoleRecord con in profileRecord.Roles)
            {
                if (con.UserRoleRecord.ApplicationRecord.Id == appRecord.Id)
                {
                    Roles.Add(con.UserRoleRecord);
                }
            }
            return Roles;
        }
        public IEnumerable<UserProfilePartRecord> GetUsersForApplication(ApplicationRecord appRecord)
        {
            try
            {
                var modules = from module in _userapplicationRepository.Table where module.ApplicationRecord.Name == appRecord.Name select module.UserProfilePartRecord;
                return modules.ToList();
            }
            catch
            {
                return new List<UserProfilePartRecord>();
            }
        }

        public IEnumerable<int> GetUserIDsForApplication(ApplicationRecord appRecord)
        {
            try
            {
                var users = GetUsersForApplication(appRecord);
                IList<int> myList = new List<int>();
                foreach (var user in users)
                {
                    myList.Add(user.Id);
                }
                return myList;
            }
            catch
            {
                return new List<int>();
            }
        }

        #region registration
        public string CreateNonce(IUser user, TimeSpan delay, string appkey)
        {
            var challengeToken = new XElement("n",
                new XAttribute("ak", appkey),
                new XAttribute("un", user.UserName), 
                new XAttribute("utc", _clock.UtcNow.ToUniversalTime().Add(delay).ToString(CultureInfo.InvariantCulture))).ToString();
            var data = Encoding.UTF8.GetBytes(challengeToken);
            return Convert.ToBase64String(_encryptionService.Encode(data));
        }

        public bool VerifyUserUnicity(string userName, string email)
        {
            string normalizedUserName = userName.ToLowerInvariant();

            if (_contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user =>
                                          user.NormalizedUserName == normalizedUserName ||
                                          user.Email == email)
                                   .List().Any())
            {
                return false;
            }

            return true;
        }

        public bool DecryptNonce(string nonce, out string username, out DateTime validateByUtc, out string appkey)
        {
            username = null;
            appkey = null;
            validateByUtc = _clock.UtcNow;

            try
            {
                var data = _encryptionService.Decode(Convert.FromBase64String(nonce));
                var xml = Encoding.UTF8.GetString(data);
                var element = XElement.Parse(xml);
                appkey = element.Attribute("ak").Value;
                username = element.Attribute("un").Value;
                validateByUtc = DateTime.Parse(element.Attribute("utc").Value, CultureInfo.InvariantCulture);
                return _clock.UtcNow <= validateByUtc;
            }
            catch
            {
                return false;
            }

        }
        public bool SendChallengeMail(ApplicationRecord app, IUser user, Func<string, string> createUrl)
        {
            string nonce = CreateNonce(user, DelayToValidate, app.AppKey);
            string url = createUrl(nonce);

            var site = _siteService.GetSiteSettings();

            var template = _shapeFactory.Create("Template_User_Validation", Arguments.From(new
            {
                RegisteredWebsite = app.Name, //site.As<RegistrationSettingsPart>().ValidateEmailRegisteredWebsite,
                ContactEmail = site.As<RegistrationSettingsPart>().ValidateEmailContactEMail,
                ChallengeUrl = url,
                ChallengeText = app.Name + " Registration"
            }));
            template.Metadata.Wrappers.Add("Template_User_Wrapper");

            var parameters = new Dictionary<string, object> {
                        {"Application", app.AppKey},
                        {"Subject", T("Verification E-Mail").Text},
                        {"Body", _shapeDisplay.Display(template)},
                        {"Recipients", user.Email }
                    };

            _messageService.Send("Email", parameters);
            return true;
        }

        public IUser ValidateChallenge(string nonce, out string appKey)
        {
            string username;
            string appkey;
            appKey = null;
            DateTime validateByUtc;

            if (!DecryptNonce(nonce, out username, out validateByUtc, out appkey))
            {
                return null;
            }

            if (validateByUtc < _clock.UtcNow)
                return null;

            var user = _membershipService.GetUser(username);
            if (user == null)
                return null;

            user.As<UserPart>().EmailStatus = UserStatus.Approved;
            appKey = appkey;

            ApplicationRecord apprecord = _applicationsService.GetApplicationByKey(appkey);
            if (apprecord == null)
                return user;

            CreateUserForApplicationRecord(user.As<UserProfilePart>(), apprecord);

            return user;
        }

        #endregion
    }
}