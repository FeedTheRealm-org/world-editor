using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;



/// <summary>
/// This singleton class is responsible for managing data persistence within the application.
/// Usage:
///    DataPersistenceManager.instance.NewWorld();
///    DataPersistenceManager.instance.SaveWorld();
///    DataPersistenceManager.instance.LoadWorld();
/// </summary>
[Serializable]
public class DataPersistenceManager : MonoBehaviour {

    [Header("File storage config")]

    [SerializeField]
    private string fileName;

    private FileDataHandler fileDataHandler;

    [SerializeField]
    private Logging.Logger logger;
    private WorldData worldData;
    private List<IDataPersistence> dataPersistenceObjects;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake() {
        instance = this;
    }

    private void Start() {
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadWorld();
    }

    // TODO: review this method
    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    // private void OnApplicationQuit() {
    //     SaveWorld();
    // }

    public void NewWorld() {
        worldData = new WorldData();
    }

    public void SaveWorld() {
        if (worldData == null) {
            worldData = new WorldData();
            logger.Log("Created new WorldData instance for saving", this, Logging.LogType.Info);
        }
        logger.Log("Starting world save...", this, Logging.LogType.Info);
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            logger.Log($"Saving data from: {dataPersistenceObj.GetType().Name}", this, Logging.LogType.Info);
            dataPersistenceObj.SaveData(ref worldData);
        }
        fileDataHandler.Save(worldData);
        logger.Log("World data save completed!", this, Logging.LogType.Info);
    }

    public void LoadWorld() {

        worldData = fileDataHandler.Load();
        if (worldData == null) {
            NewWorld();
            logger.Log("No world data found, creating a new world.", this, Logging.LogType.Warning);
            return;
        }
        logger.Log("Loading world data...", this, Logging.LogType.Info);
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            logger.Log($"Loading data into: {dataPersistenceObj.GetType().Name}", this, Logging.LogType.Info);
            logger.Log($"Loading {worldData.objectPlacementData.Count} items into the world", this, Logging.LogType.Info);
            dataPersistenceObj.LoadData(worldData);
        }
        logger.Log("World data loaded successfully!", this, Logging.LogType.Info);
    }


    private List<string> ListAllWorlds() {
        return fileDataHandler.GetAllWorlds();
    }


}
