using Orchard.ContentManagement;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web;

namespace Contrib.Foundation.UserProfile.OData
{
    [DataContract]
    public class Profile
    {
        [DataMember]
        public int Id { get; private set; }

        [DataMember]
        public string Type { get; private set; }

        [DataMember]
        public string Username { get; private set; }

        [DataMember]
        public string Email { get; private set; }

        [DataMember]
        public string FirstName { get; private set; }

        [DataMember]
        public string LastName { get; private set; }

        [DataMember]
        public string WebSite { get; private set; }

        [DataMember]
        public string Bio { get; private set; }

        [DataMember]
        public string Location { get; private set; }

        [DataMember]
        public string Hash { get; private set; }

        [DataMember]
        public Uri link { get; private set; }


        public Profile(IUser User, HttpRequestMessage m, string hash)
        {
            Type = "Profile";

            // part fields
            Id = User.Id;
            Username = User.UserName;
            Email = User.Email;

            // dynamic fields
            dynamic user = User;
            if (user.UserProfilePart != null)
            {
                FirstName = user.UserProfilePart.FirstName;
                LastName = user.UserProfilePart.LastName;
                Location = user.UserProfilePart.Location;
                WebSite = user.UserProfilePart.WebSite;
                Bio = user.UserProfilePart.Bio;
            }
            else
            {
                //Preview = new Photo();
            }

            Hash = hash;
            // computed fields
            UriBuilder b = new UriBuilder(m.RequestUri.Scheme , m.RequestUri.Host,m.RequestUri.Port);
            b.Path  = "/v1/user/profile('" + Username + "')";
            link = b.Uri;
        }
    }
}