using Contrib.Foundation.Application.Models;
using Contrib.Foundation.UserProfile.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using System;
using System.Collections.Generic;

namespace Contrib.Foundation.UserProfile.Services
{
    public interface ILoginsService : IDependency
    {
        LoginsRecord LoginWithHash(string Hash);
        string CreateHash(UserProfilePart profilePart, ApplicationRecord applicationRecord);
        string GetHash(UserProfilePart profilePart, ApplicationRecord applicationRecord);
        void CleanupHashes(UserProfilePart profilePart, ApplicationRecord applicationRecord);
        void DeleteHash(string Hash);
        IUser ValidateHash(string Hash, string ApiKey);
    }
}