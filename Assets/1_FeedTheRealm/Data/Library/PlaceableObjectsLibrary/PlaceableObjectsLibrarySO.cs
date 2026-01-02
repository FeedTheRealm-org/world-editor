using System;
using System.Collections.Generic;
using UnityEngine;

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

    private Dictionary<PlaceableObjectCategories, IPlaceableLoader> loaderCache;

    public void Initialize()
    {
        logger.Log("Initializing Creator Library...", this, Logging.LogType.Info);
        loaderCache = new Dictionary<PlaceableObjectCategories, IPlaceableLoader>();
        foreach (var loader in loaders)
        {
            if (
                loader.loader is ILoadable loadable
                && loader.loader is IPlaceableLoader placeableLoader
            )
            {
                loadable.LoadLibrary();
                loaderCache[loader.category] = placeableLoader;
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
