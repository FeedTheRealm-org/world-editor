using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.WorldObjects
{
    public abstract class WorldObject : IPersistent
    {
        public WorldObject(DataPersistenceRegistryEvent registryEvent)
        {
            registryEvent.Raise(this);
        }

        public abstract void SaveData(ref WorldData worldData);
    }

    public abstract class WorldObjectController : MonoBehaviour, IPersistent
    {
        [Inject]
        private DataPersistenceRegistryEvent registryEvent;

        private void Start()
        {
            if (registryEvent != null)
                registryEvent.Raise(this);
        }

        public abstract void SaveData(ref WorldData worldData);
    }
}
