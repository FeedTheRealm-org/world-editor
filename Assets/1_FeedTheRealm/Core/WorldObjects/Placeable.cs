using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.WorldObjects
{
    public interface IPlaceable
    {
        PlaceableObjectCategories Category { get; }
    }

    public abstract class Placeable<T>
        : MonoBehaviour,
            IPersistent<ZoneData>,
            ILoadable<T>,
            IPlaceable
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
