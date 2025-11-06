using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "DataPersistence", menuName = "Scriptable Objects/Persistence/DataPersistenceManager")]
public class DataPersistenceManagerSO : ScriptableObject {
    [Header("File storage config")]
    [SerializeField] private string saveDirectory = "Worlds";
    [SerializeField] private string fileExtension = ".world";

    [SerializeField] private Logging.Logger logger;

    private readonly FileDataHandler fileDataHandler = new();
    private WorldData worldData = null;
    private List<IDataPersistence> dataPersistenceObjects = new();

    private bool alreadyFound = false;

    public WorldData CurrentWorldData => worldData;

    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        if (alreadyFound) return dataPersistenceObjects;
        var found = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IDataPersistence>();
        alreadyFound = true;
        return new List<IDataPersistence>(found);
    }

    // --- Core Methods ---
    public void NewWorld() {
        worldData = new WorldData();
    }

    public void SaveWorld(string worldName = null) {
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

        fileDataHandler.Save(worldData, saveDirectory, fileExtension);
        logger.Log("World data save completed!", this, Logging.LogType.Info);
    }

    public void LoadWorld(string dataFileName = null) {
        this.worldData = fileDataHandler.Load(dataFileName, saveDirectory);

        if (worldData == null) {
            NewWorld();
            logger.Log($"No world data found for: [{dataFileName}], creating new world.", this, Logging.LogType.Warning);
            return;
        }

        logger.Log("Loading world data...", this, Logging.LogType.Info);

        // dataPersistenceObjects = FindAllDataPersistenceObjects();
        // logger.Log($"Found {dataPersistenceObjects.Count} data persistence objects.", this, Logging.LogType.Info);

        // foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
        //     logger.Log($"Loading data into: {dataPersistenceObj.GetType().Name}", this, Logging.LogType.Info);
        //     dataPersistenceObj.LoadData(worldData);
        // }

        logger.Log("World data loaded successfully!", this, Logging.LogType.Info);
    }

    public List<string> ListAllWorlds() {
        return fileDataHandler.GetAllWorlds(saveDirectory, fileExtension);
    }
}
