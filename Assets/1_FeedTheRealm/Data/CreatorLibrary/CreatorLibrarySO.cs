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
    private List<SpawnerTypeGameObject> spawnerTypes;

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
