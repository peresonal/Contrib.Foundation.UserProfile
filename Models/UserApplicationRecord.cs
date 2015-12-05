using Contrib.Foundation.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Foundation.UserProfile.Models
{
    public class UserApplicationRecord
    {
        public virtual int Id { get; set; }
        public virtual DateTime? RegistrationStart { get; set; }
        public virtual DateTime? RegistrationEnd { get; set; }
        public virtual UserProfilePartRecord UserProfilePartRecord { get; set; }
        public virtual ApplicationRecord ApplicationRecord { get; set; }
    }
}