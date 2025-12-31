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
    private SpawnerLoaderSO spawnerLoader;

    [SerializeField]
    public StructureLoaderSO structureLoader;

    public void Initialize()
    {
        // TODO: consider using an event based system like in the loader
        logger.Log("Initializing Creator Library...", this, Logging.LogType.Info);
        spawnerLoader.LoadLibrary();
        structureLoader.LoadLibrary();
        logger.Log("Library loaded", this, Logging.LogType.Info);
    }

    public List<IPlaceable> GetObjects(WorldObjectCategories category)
    {
        return category switch
        {
            WorldObjectCategories.Structure => structureLoader.GetObjects(),
            WorldObjectCategories.Spawner => spawnerLoader.GetObjects(),
            _ => new List<IPlaceable>(),
        };
    }
}
