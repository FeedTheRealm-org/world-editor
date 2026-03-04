using System;
using System.Collections.Generic;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Library.CreatorObjectLibrary
{
    [CreateAssetMenu(
        fileName = "CreatorObjectLibrary",
        menuName = "Scriptable Objects/Library/CreatorObjectLibrary"
    )]
    public class CreatorObjectLibrarySO : ScriptableObject, IPersistent
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private List<Loaders> loaders;

        private Dictionary<CreatorObjectCategories, ICreatableLoader> loaderCache;

        public void Initialize()
        {
            logger.Log($"Initializing Creator Library", this, Logging.LogType.Info);
            loaderCache = new Dictionary<CreatorObjectCategories, ICreatableLoader>();
            foreach (var loader in loaders)
            {
                loaderCache[loader.category] = loader.loader as ICreatableLoader;
            }
            logger.Log(
                $"Creator Object Library loaded {loaders.Count} loaders",
                this,
                Logging.LogType.Info
            );
        }

        public List<CreatorObject> GetCreatables(CreatorObjectCategories category)
        {
            if (loaderCache != null && loaderCache.TryGetValue(category, out var loader))
            {
                return loader.GetCreatables();
            }
            logger.Log(
                $"No loader found for category {category}. Returning empty list.",
                this,
                Logging.LogType.Warning
            );
            return new List<CreatorObject>();
        }

        public List<CreatorObject> GetAllCreatorObjects()
        {
            List<CreatorObject> allCreatables = new();
            foreach (var loader in loaderCache.Values)
            {
                allCreatables.AddRange(loader.GetCreatables());
            }
            return allCreatables;
        }

        public void AddCreatable(CreatorObjectCategories category, CreatorObject creatable)
        {
            if (loaderCache != null && loaderCache.TryGetValue(category, out var loader))
            {
                loader.AddCreatable(creatable);
                return;
            }
            logger.Log(
                $"No loader found for category {category}. Cannot add creatable.",
                this,
                Logging.LogType.Warning
            );
        }

        public void RemoveCreatable(CreatorObjectCategories category, CreatorObject creatable)
        {
            if (loaderCache != null && loaderCache.TryGetValue(category, out var loader))
            {
                loader.RemoveCreatable(creatable);
                return;
            }
            logger.Log(
                $"No loader found for category {category}. Cannot remove creatable.",
                this,
                Logging.LogType.Warning
            );
        }

        public void UpdateCreatable(CreatorObjectCategories category, CreatorObject creatable)
        {
            if (loaderCache != null && loaderCache.TryGetValue(category, out var loader))
            {
                loader.UpdateCreatable(creatable);
                return;
            }
            logger.Log(
                $"No loader found for category {category}. Cannot update creatable.",
                this,
                Logging.LogType.Warning
            );
        }

        // TODO: REMOVE: the data persistence manager is in charge of handleing save operations
        public void SaveData(ref WorldData worldData)
        {
            foreach (var loader in loaderCache.Values)
            {
                foreach (var creatable in loader.GetCreatables())
                {
                    creatable.SaveData(ref worldData);
                }
            }
        }

        [Serializable]
        public class Loaders
        {
            public CreatorObjectCategories category;
            public ScriptableObject loader;
        }
    }
}
