using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Contrib.Foundation.UserProfile.Services
{
    public interface IPartWatcher : IDependency
    {
        void Watch<T>(T part) where T : IContent;
        IEnumerable<T> Get<T>() where T : IContent;
    }
}