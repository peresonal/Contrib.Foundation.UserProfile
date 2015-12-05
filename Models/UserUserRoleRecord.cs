using Contrib.Foundation.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Foundation.UserProfile.Models
{
    public class UserUserRoleRecord
    {
        public virtual int Id { get; set; }
        public virtual UserProfilePartRecord UserProfilePartRecord { get; set; }
        public virtual UserRoleRecord UserRoleRecord { get; set; }
    }
}