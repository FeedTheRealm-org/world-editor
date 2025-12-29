using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: refactor this to be a ScriptableObject-based library
public class CreatorLibraryController : MonoBehaviour
{
    [SerializeField]
    private List<SpawnerTypeGameObject> spawnerTypes;

    [SerializeField]
    private Logging.Logger logger;

    private SpawnerLoader spawnerLoader;

    public StructureLoader structureLoader;

    public void Initialize()
    {
        spawnerLoader = new SpawnerLoader(logger, spawnerTypes);
        spawnerLoader.LoadLibrary();

        structureLoader = new StructureLoader(logger);
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
