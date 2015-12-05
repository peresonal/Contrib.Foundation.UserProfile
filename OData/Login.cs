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
    public class Login
    {
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Password { get; private set; }
        [DataMember]
        public string ApiKey { get; private set; }
        [DataMember]
        public string Hash { get; private set; }
        public Login()
        {
            Username = string.Empty;
            Password = string.Empty;
            ApiKey = string.Empty;
            Hash = string.Empty;
        }
    }
}