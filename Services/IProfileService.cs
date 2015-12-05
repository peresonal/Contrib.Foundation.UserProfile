using Contrib.Foundation.Application.Models;
using Contrib.Foundation.UserProfile.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using System;
using System.Collections.Generic;

namespace Contrib.Foundation.UserProfile.Services
{
    public interface IProfileService : IDependency
    {
        //ContentItem Get(int id);
        ContentPart Get(IUser owner);
        IEnumerable<UserProfilePartRecord> GetUsersForApplication(ApplicationRecord appRecord);
        IEnumerable<int> GetUserIDsForApplication(ApplicationRecord appRecord);
        bool CreateUserForApplicationRecord(UserProfilePart profilePart, ApplicationRecord appRecord);
        IEnumerable<UserRoleRecord> GetUserRoles(UserProfilePart profilePart, ApplicationRecord appRecord);
        string CreateNonce(IUser user, TimeSpan delay, string appkey);
        bool DecryptNonce(string nonce, out string username, out DateTime validateByUtc, out string appkey);
        bool VerifyUserUnicity(string userName, string email);
        bool SendChallengeMail(ApplicationRecord app, IUser user, Func<string, string> createUrl);
        IUser ValidateChallenge(string nonce, out string appKey);
    }
}