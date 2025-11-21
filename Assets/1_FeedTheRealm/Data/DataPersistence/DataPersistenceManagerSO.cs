using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Models;

[CreateAssetMenu(fileName = "DataPersistence", menuName = "Scriptable Objects/Persistence/DataPersistenceManager")]
public class DataPersistenceManagerSO : ScriptableObject {
    [Header("World storage config")]
    [SerializeField] private string saveDirectory = "Worlds";
    [SerializeField] private string fileExtension = ".world";

    [SerializeField] private Logging.Logger logger;

    private readonly WorldHandler worldDataHandler = new();
    private WorldData worldData = null;
    private List<IDataPersistence> dataPersistenceObjects = new();

    public WorldData CurrentWorldData => worldData;

    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        var found = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(found);
    }

    // --- Core Methods ---
    public void NewWorld() {
        worldData = new WorldData();
    }

    public void UnSetActiveWorld() {
        worldData = null;
    }

    public void SaveWorld(string worldName) {
        if (worldData == null) {
            logger.Log("No world data found. A new world will be created for saving.", this, Logging.LogType.Warning);
            NewWorld();
        }
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        worldData.worldName = worldName;
        logger.Log("Starting world save...", this, Logging.LogType.Info);

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            logger.Log($"Saving data from: {dataPersistenceObj.GetType().Name}", this, Logging.LogType.Info);
            dataPersistenceObj.SaveData(ref worldData);
        }

        worldDataHandler.Save(worldData, saveDirectory, fileExtension);
        logger.Log("World data save completed!", this, Logging.LogType.Info);
    }

    public void SetActiveWorld(string dataFileName) {
        worldData = worldDataHandler.Load(dataFileName, saveDirectory);
    }


    public void LoadWorld() {

        if (worldData == null) {
            NewWorld();
            logger.Log($"No world data set, creating new world.", this, Logging.LogType.Warning);
            return;
        }

        logger.Log("Loading world data...", this, Logging.LogType.Info);

        dataPersistenceObjects = FindAllDataPersistenceObjects();
        logger.Log($"Found {dataPersistenceObjects.Count} data persistence objects.", this, Logging.LogType.Info);

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            logger.Log($"Loading data into: {dataPersistenceObj.GetType().Name}", this, Logging.LogType.Info);
            dataPersistenceObj.LoadData(worldData);
        }

        logger.Log("World data loaded successfully!", this, Logging.LogType.Info);
    }

    public List<string> ListAllWorlds() {
        return worldDataHandler.GetAllWorlds(saveDirectory, fileExtension);
    }
}
