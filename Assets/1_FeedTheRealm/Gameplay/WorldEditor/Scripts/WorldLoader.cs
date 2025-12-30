using Models;
using UnityEngine;

public class WorldLoader : MonoBehaviour
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private DataPersistenceManagerSO dataPersistenceManager;

    void Awake()
    {
        LoadWorldData(dataPersistenceManager.CurrentWorldData);
    }

    public void LoadWorldData(WorldData worldData)
    {
        if (worldData == null)
        {
            logger.Log("No world data to load.", this, Logging.LogType.Warning);
            return;
        }

        StructureLoader.LoadWorld(worldData);
        logger.Log("World data loaded", this, Logging.LogType.Info);
    }
}
