using Contrib.Foundation.UserProfile.Models;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Users.Models;

namespace Contrib.Foundation.UserProfile.Handlers
{
    [UsedImplicitly]
    public class UserProfilePartHandler : ContentHandler
    {
        public UserProfilePartHandler(IRepository<UserProfilePartRecord> repository)
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<UserProfilePart>("User"));
            Filters.Add(StorageFilter.For(repository));
        }

        public Localizer T { get; set; }
    }
}