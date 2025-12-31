using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CreatorLibrary",
    menuName = "Scriptable Objects/WorldEditor/CreatorLibrary"
)]
public class CreatorLibrarySO : ScriptableObject
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private List<Loaders> loaders;

    private Dictionary<WorldObjectCategories, ILoadable> loaderCache;

    public void Initialize()
    {
        logger.Log("Initializing Creator Library...", this, Logging.LogType.Info);
        loaderCache = new Dictionary<WorldObjectCategories, ILoadable>();
        foreach (var loader in loaders)
        {
            if (loader.loader is ILoadable loadable)
            {
                loadable.LoadLibrary();
                loaderCache[loader.category] = loadable;
            }
            else
            {
                logger.Log(
                    $"Loader for category {loader.category} does not implement ILoadable.",
                    this,
                    Logging.LogType.Error
                );
            }
        }
        logger.Log("Library loaded", this, Logging.LogType.Info);
    }

    public List<IPlaceable> GetObjects(WorldObjectCategories category)
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
        public WorldObjectCategories category;
        public ScriptableObject loader;
    }
}
