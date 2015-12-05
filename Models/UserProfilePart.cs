using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Foundation.UserProfile.Models
{
    public class UserProfilePart : ContentPart<UserProfilePartRecord>
    {
        public string FirstName 
        {
            get { return Record.FirstName; }
            set { Record.FirstName = value; } 
        }
        public string LastName
        {
            get { return Record.LastName; }
            set { Record.LastName = value; }
        }
        public string WebSite
        {
            get { return Record.WebSite; }
            set { Record.WebSite = value; }
        }
        public string Location
        {
            get { return Record.Location; }
            set { Record.Location = value; }
        }
        public string Bio
        {
            get { return Record.Bio; }
            set { Record.Bio = value; }
        }
        public bool ShowEmail
        {
            get { return Record.ShowEmail; }
            set { Record.ShowEmail = value; }
        }

        public string FBusername
        {
            get { return Record.FBusername; }
            set { Record.FBusername = value; }
        }

        public string FBemail
        {
            get { return Record.FBemail; }
            set { Record.FBemail = value; }
        }
        public string FBtoken
        {
            get { return Record.FBtoken; }
            set { Record.FBtoken = value; }
        }
    }
}