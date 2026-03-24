using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.WorldObjects
{
    public abstract class WorldObject : IPersistent<CreatablesData>
    {
        public WorldObject(CreatablesDataRegistryEvent registryEvent)
        {
            registryEvent.Raise(this);
        }

        public abstract void SaveData(ref CreatablesData creatablesData);
    }

    public abstract class WorldObjectController : MonoBehaviour, IPersistent<ZoneData>
    {
        [Inject]
        private ZoneDataRegistryEvent registryEvent;

        private void Start()
        {
            if (registryEvent != null)
                registryEvent.Raise(this);
        }

        public abstract PlaceableObjectCategories Category { get; }
        public abstract void SaveData(ref ZoneData zoneData);
    }
}
