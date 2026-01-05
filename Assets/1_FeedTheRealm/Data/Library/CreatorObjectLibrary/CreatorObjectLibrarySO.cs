using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CreatorObjectLibrary",
    menuName = "Scriptable Objects/Library/CreatorObjectLibrary"
)]
public class CreatorObjectLibrarySO : ScriptableObject, ILibraryInitializer
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

    public List<ICreatable> GetCreatables(CreatorObjectCategories category)
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
        return new List<ICreatable>();
    }

    public void AddCreatable(CreatorObjectCategories category, ICreatable creatable)
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

    public void RemoveCreatable(CreatorObjectCategories category, ICreatable creatable)
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

    public void UpdateCreatable(CreatorObjectCategories category, ICreatable creatable)
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

    [Serializable]
    public class Loaders
    {
        public CreatorObjectCategories category;
        public ScriptableObject loader;
    }
}
