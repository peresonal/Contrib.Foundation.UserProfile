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
    public class Register
    {
        [DataMember]
        public string FirstName { get; private set; }
        [DataMember]
        public string LastName { get; private set; }
        [DataMember]
        public string Email { get; private set; }
        [DataMember]
        public string Password { get; private set; }
        [DataMember]
        public string ApiKey { get; private set; }

        public Register()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ApiKey = string.Empty;
        }
    }
}