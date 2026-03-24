using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public abstract class PlaceableLoader<TData> : ILoader
    {
        protected Logging.Logger logger;

        protected PlaceablesLibrary placeableLibrary;

        public PlaceableLoader(Logging.Logger logger, PlaceablesLibrary placeableLibrary)
        {
            this.logger = logger;
            this.placeableLibrary = placeableLibrary;
        }

        /// <summary>
        /// Returns the list of data to be loaded for this loader.
        /// Classes that inherit from PlaceableLoader must implement this method to specify which data they will load.
        /// For example, the StructureLoader will return the list of structure data from the zone data,
        /// while the PlayerSpawnpointLoader will return the list of player spawn point data.
        /// </summary>
        protected abstract List<TData> GetData(ZoneData zoneData);

        /// <summary>
        /// Returns the GameObject to be instantiated for the given data.
        /// Classes that inherit from PlaceableLoader must implement this method to specify which GameObject will be instantiated for each data.
        /// This allows different loaders to instantiate different prefabs based on the type of data they are loading.
        /// For example, the StructureLoader will instantiate structure prefabs based on the structure data.
        /// <returns></returns>
        protected abstract UniTask<GameObject> GetObject(TData data);

        public async UniTask Load(ZoneData zoneData)
        {
            var dataList = GetData(zoneData);

            foreach (var data in dataList)
            {
                try
                {
                    var loadedObject = await GetObject(data);
                    loadedObject.GetComponent<ILoadable<TData>>().Load(data);
                }
                catch (Exception ex)
                {
                    logger.Log(
                        $"[Placeable Loader] Failed to load {typeof(TData).Name}: {ex.Message}",
                        Logging.LogType.Error
                    );
                }
            }
        }
    }
}
