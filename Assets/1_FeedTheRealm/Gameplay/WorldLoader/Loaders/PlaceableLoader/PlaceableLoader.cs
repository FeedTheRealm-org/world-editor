using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Loaders
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

        protected abstract List<TData> GetData(WorldData worldData);
        protected abstract UniTask<GameObject> GetObject(TData data);

        public async UniTask Load(WorldData worldData)
        {
            var dataList = GetData(worldData);

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
