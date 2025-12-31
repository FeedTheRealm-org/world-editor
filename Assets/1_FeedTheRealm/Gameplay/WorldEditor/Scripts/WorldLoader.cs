using Models;
using UnityEngine;
using Utils;

public class WorldLoader : MonoBehaviour
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private DataPersistenceManagerSO dataPersistenceManager;

    void Start()
    {
        WorldData worldData = dataPersistenceManager.CurrentWorldData;
        LoadWorld(worldData);
    }

    // TODO: consider adding a loading screen or something to avoid having the user
    // see how the world is being populated.
    public void LoadWorld(WorldData worldData)
    {
        logger.Log("Raising world selected event...", this, Logging.LogType.Info);
        SelectionRaiser.RaiseSelected(worldData);
    }
}
