using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.WorldObjects
{
    /// <summary>
    /// The intention of this interface is to recognize the categories of placeable objects in the world,
    ///  so they can be edited in the editor.
    /// </summary>
    public interface IEditable
    {
        PlaceableObjectCategories Category { get; }
    }

    public abstract class Placeable<T>
        : MonoBehaviour,
            IPersistent<ZoneData>,
            ILoadable<T>,
            IEditable
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
