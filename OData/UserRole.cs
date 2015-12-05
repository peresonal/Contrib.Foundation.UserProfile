using Contrib.Foundation.Application.Models;
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
    public class UserRole
    {
        [DataMember]
        public int Id { get; private set; }

        [DataMember]
        public int UserId { get; private set; }

        [DataMember]
        public string Type { get; private set; }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string Description { get; private set; }

        [DataMember]
        public bool IsDefault { get; private set; }

        [DataMember]
        public Uri link { get; private set; }

        public UserRole(IUser User, UserRoleRecord UserRole, HttpRequestMessage m)
        {
            Type = "Role";

            // part fields
            Id = UserRole.Id;
            UserId = User.Id;

            Name = UserRole.Name;
            Description = UserRole.Description;
            IsDefault = UserRole.IsDefaultRole;

            string Username = User.UserName;
            // computed fields
            UriBuilder b = new UriBuilder(m.RequestUri.Scheme , m.RequestUri.Host,m.RequestUri.Port);
            b.Path  = "/v1/user/profile('" + Username + "')/roles/('" + Name + "')";
            link = b.Uri;
        }
    }
}