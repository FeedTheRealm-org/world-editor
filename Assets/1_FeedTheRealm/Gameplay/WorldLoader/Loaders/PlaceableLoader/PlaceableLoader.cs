using System;
using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTR.Core.Common.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Loaders
{
    public abstract class PlaceableLoader<TData> : ILoader
    {
        [Inject]
        protected Logging.Logger logger;

        protected abstract List<TData> GetData(WorldData worldData);
        protected abstract GameObject GetPrefab(WorldPrefabProvider prefabProvider);

        public void Load(
            WorldData worldData,
            WorldPrefabProvider worldPrefabProvider,
            IObjectResolver objectResolver
        )
        {
            var dataList = GetData(worldData);
            var prefab = GetPrefab(worldPrefabProvider);

            if (prefab == null)
            {
                logger.Log($"Prefab for {typeof(TData).Name} is null", Logging.LogType.Error);
                return;
            }

            foreach (var data in dataList)
            {
                try
                {
                    GameObject instance = objectResolver.Instantiate(prefab);

                    // Find component that implements IInitializeable<TData>
                    var initializeable = instance.GetComponent<IInitializeable<TData>>();

                    if (initializeable == null)
                    {
                        logger.Log(
                            $"Prefab {prefab.name} does not implement IInitializeable<{typeof(TData).Name}>",
                            Logging.LogType.Error
                        );
                        continue;
                    }

                    initializeable.Initialize(data);
                }
                catch (Exception ex)
                {
                    logger.Log(
                        $"Failed to load {typeof(TData).Name}: {ex.Message}",
                        Logging.LogType.Error
                    );
                }
            }
        }
    }
}
