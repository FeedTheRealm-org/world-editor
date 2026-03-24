using FeedTheRealm.Core.EventChannels.WorldEvents;

namespace FeedTheRealm.Core.WorldObjects.Items
{
    public abstract class Item : WorldObject
    {
        protected Item(CreatablesDataRegistryEvent registryEvent)
            : base(registryEvent) { }
    }
}
