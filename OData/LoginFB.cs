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
    public class LoginFB
    {
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Token { get; private set; }
        [DataMember]
        public string ApiKey { get; private set; }

        public LoginFB()
        {
            Username = string.Empty;
            Token = string.Empty;
            ApiKey = string.Empty;
        }
    }
}