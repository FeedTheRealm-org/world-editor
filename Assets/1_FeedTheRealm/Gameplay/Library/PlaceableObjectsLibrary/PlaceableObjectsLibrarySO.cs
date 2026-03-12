using System;
using System.Collections.Generic;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.PlaceableObjects;
using FeedTheRealm.Gameplay.Loaders;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    [CreateAssetMenu(
        fileName = "PlaceableObjectsLibrary",
        menuName = "Scriptable Objects/Library/PlaceableObjectsLibrary"
    )]
    public class PlaceableObjectsLibrarySO : ScriptableObject
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private List<Loaders> loaders;

        // [Inject]
        // private ModelsRepository modelsRepository;

        private Dictionary<PlaceableObjectCategories, IPlaceableLoader> loaderCache;

        public void Initialize()
        {
            logger.Log($"Initializing Placeable Objects Library", this, Logging.LogType.Info);
            loaderCache = new Dictionary<PlaceableObjectCategories, IPlaceableLoader>();
            foreach (var loader in loaders)
            {
                loaderCache[loader.category] = loader.loader as IPlaceableLoader;
                loaderCache[loader.category].LoadLibrary();
            }
            logger.Log(
                $"Placeable Objects Library loaded {loaders.Count} loaders",
                this,
                Logging.LogType.Info
            );
        }

        public List<IPlaceable> GetObjects(PlaceableObjectCategories category)
        {
            if (loaderCache != null && loaderCache.TryGetValue(category, out var loader))
            {
                return loader.GetObjects();
            }
            logger.Log(
                $"No loader found for category {category}. Returning empty list.",
                this,
                Logging.LogType.Warning
            );
            return new List<IPlaceable>();
        }

        [Serializable]
        public class Loaders
        {
            public PlaceableObjectCategories category;
            public ScriptableObject loader;
        }
    }
}
