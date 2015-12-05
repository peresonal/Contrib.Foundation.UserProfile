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
    public class LoginsService : ILoginsService
    {
        private static readonly TimeSpan DelayToValidate = new TimeSpan(7, 0, 0, 0); // one week to keep hash alive
        private readonly IOrchardServices _orchardServices;
        private readonly IApplicationsService _applicationsService;
        private readonly IEncryptionService _encryptionService;
        private readonly IClock _clock;
        private readonly IRepository<UserProfilePartRecord> _userprofileRepository;
        private readonly IRepository<UserApplicationRecord> _userapplicationRepository;
        private readonly IRepository<LoginsRecord> _loginsRepository;

        public LoginsService(
            IOrchardServices orchardServices, 
            IContentManager contentManager,
            IClock clock,
            IApplicationsService applicationsService,
            IEncryptionService encryptionService,
            IRepository<UserProfilePartRecord> userprofileRepository,
            IRepository<UserApplicationRecord> userapplicationRepository,
            IRepository<LoginsRecord> loginsRepository

            )
        {
            _orchardServices = orchardServices;
            _clock = clock;
            _applicationsService = applicationsService;
            _encryptionService = encryptionService;
            _userprofileRepository = userprofileRepository;
            _userapplicationRepository = userapplicationRepository;
            _loginsRepository = loginsRepository;

        }

        public LoginsRecord LoginWithHash(string Hash)
        {
            return null;
        }

        private IUser GetUser(int Id)
        {
            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Id == Id).List().FirstOrDefault();
        }

        public IUser ValidateHash(string Hash, string ApiKey)
        {
            try
            {
                var logins = from login in _loginsRepository.Table where login.Hash == Hash select login;
                if (logins == null) return null;
                var loginrecord = logins.FirstOrDefault();
                if (loginrecord == null) return null;

                var data = _encryptionService.Decode(Convert.FromBase64String(loginrecord.Hash));
                var xml = Encoding.UTF8.GetString(data);
                var element = XElement.Parse(xml);
                DateTime validateByUtc;
                string appid = element.Attribute("ai").Value;
                string userid = element.Attribute("ui").Value;
                validateByUtc = DateTime.Parse(element.Attribute("utc").Value, CultureInfo.InvariantCulture);
                if (_clock.UtcNow <= validateByUtc)
                {
                    int aid;
                    ApplicationRecord app = null;
                    if(Int32.TryParse(appid, out aid))
                        app = _applicationsService.GetApplication(aid);
                    if (app != null && app.AppKey == ApiKey)
                    {
                        int uid;
                        if (Int32.TryParse(userid, out uid))
                            return GetUser(uid);
                    }
                }
                else
                {
                    _loginsRepository.Delete(loginrecord);
                    return null;
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public void DeleteHash(string Hash)
        {
            try
            {
                var logins = from login in _loginsRepository.Table where login.Hash == Hash select login;
                foreach (LoginsRecord login in logins)
                {
                    _loginsRepository.Delete(login);
                }
            }
            catch
            {
                return;
            }
        }
        public void CleanupHashes(UserProfilePart profilePart, ApplicationRecord applicationRecord)
        {
            try
            {
                var logins = from login in _loginsRepository.Table where login.ApplicationRecord.Id == applicationRecord.Id && login.UserProfilePartRecord.Id == profilePart.Id select login;
                foreach (LoginsRecord login in logins)
                {
                    _loginsRepository.Delete(login);
                }
            }
            catch
            {
                return;
            }
        }
        public string GetHash(UserProfilePart profilePart, ApplicationRecord applicationRecord)
        {
            UserProfilePartRecord profileRecord = _userprofileRepository.Get(profilePart.Id);
            if (profileRecord == null) return null;
            try
            {
                var logins = from login in _loginsRepository.Table where login.ApplicationRecord.Id == applicationRecord.Id && login.UserProfilePartRecord.Id == profilePart.Id select login;
                //foreach (LoginsRecord login in logins)
                //{
                //    _loginsRepository.Delete(login);
                //}
                var first = logins.FirstOrDefault();
                if(first !=null)
                    return first.Hash;
            }
            catch
            {
                return null;
            }
            return null;
        }
        public string CreateHash(UserProfilePart profilePart, ApplicationRecord applicationRecord)
        {
            UserProfilePartRecord profileRecord =  _userprofileRepository.Get(profilePart.Id);
            if (profileRecord == null) return null;

            // first delete all hashes for this user and application
            CleanupHashes(profilePart, applicationRecord);

            var utcNow = _clock.UtcNow;

            LoginsRecord r = new LoginsRecord();
            r.Hash = createHash(profileRecord.Id, applicationRecord.Id, DelayToValidate);
            r.UserProfilePartRecord = profileRecord;
            r.ApplicationRecord = applicationRecord;
            r.UpdatedUtc = utcNow;

            _loginsRepository.Create(r);

            return r.Hash;
        }

        private string createHash(int userId, int appId, TimeSpan delay)
        {
            var challengeToken = new XElement("n",
                new XAttribute("ui", userId),
                new XAttribute("ai", appId),
                new XAttribute("utc", _clock.UtcNow.ToUniversalTime().Add(delay).ToString(CultureInfo.InvariantCulture))).ToString();
            var data = Encoding.UTF8.GetBytes(challengeToken);
            return Convert.ToBase64String(_encryptionService.Encode(data));
        }

    }
}