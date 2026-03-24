using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.WorldObjects
{
    public abstract class WorldObject<T> : IPersistent<CreatablesData>, ILoadable<T>
    {
        public WorldObject(CreatablesDataRegistryEvent registryEvent)
        {
            registryEvent.Raise(this);
        }

        public virtual void Load(T data)
        {
            LoadData(data);
        }

        public abstract void SaveData(ref CreatablesData creatablesData);
        public abstract void LoadData(T data);
    }

    public abstract class WorldObjectController<T>
        : MonoBehaviour,
            IPersistent<ZoneData>,
            ILoadable<T>
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

        public abstract void LoadData(T data);

        public void Load(T data)
        {
            LoadData(data);
        }
    }
}
