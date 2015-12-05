using Contrib.Foundation.UserProfile.Services;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement.Handlers;

namespace Contrib.Foundation.UserProfile.Handlers
{
    public class PartWatcherHandler : ContentHandler
    {
        public PartWatcherHandler(IPartWatcher watcher)
        {
            OnGetDisplayShape<AutoroutePart>((ctx, part) =>
            {
                if (ctx.DisplayType != "Detail") return;
                watcher.Watch(part);
            });
        }
    }
}