using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Foundation.UserProfile.Models
{
    public class UserProfilePartRecord : ContentPartRecord
    {
        public UserProfilePartRecord()
        {
            Applications = new List<UserApplicationRecord>();
            Roles = new List<UserUserRoleRecord>();
        }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual bool ShowEmail { get; set; }
        public virtual string WebSite { get; set; }
        public virtual string Location { get; set; }
        public virtual string Bio { get; set; }
        public virtual string FBusername { get; set; }
        public virtual string FBemail { get; set; }
        public virtual string FBtoken { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<UserApplicationRecord> Applications { get; set; }
        [CascadeAllDeleteOrphan]
        public virtual IList<UserUserRoleRecord> Roles { get; set; }
    }
}