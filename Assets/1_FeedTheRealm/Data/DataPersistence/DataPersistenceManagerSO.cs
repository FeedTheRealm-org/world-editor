using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEditor;

[CreateAssetMenu(fileName = "DataPersistence", menuName = "Scriptable Objects/Persistence/DataPersistenceManager")]
public class DataPersistenceManagerSO : ScriptableObject {
    [Header("World storage config")]
    [SerializeField] private string saveDirectory = "Worlds";
    [SerializeField] private string fileExtension = ".world";

    [SerializeField] private Logging.Logger logger;

    [SerializeField] private ConsumableItems consumableItemsDatabase;
    [SerializeField] private Enemy enemyDatabase;

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

#if UNITY_EDITOR
    [CustomEditor(typeof(DataPersistenceManagerSO))]
    public class DataPersistenceManagerSOEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            DataPersistenceManagerSO manager = (DataPersistenceManagerSO)target;

            if (GUILayout.Button("Unset Active World")) {
                manager.NewWorld();
            }
        }
    }
#endif

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

        worldData.consumableItems = consumableItemsDatabase.GetAllConsumableItems();
        if (enemyDatabase != null) {
            worldData.enemies = enemyDatabase.GetAllEnemies();
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
        try {
            logger.Log("Loading world data...", this, Logging.LogType.Info);
            dataPersistenceObjects = FindAllDataPersistenceObjects();
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
                dataPersistenceObj.LoadData(worldData);
            }

            consumableItemsDatabase.LoadConsumableItems(worldData.consumableItems);

            if (enemyDatabase != null) {
                enemyDatabase.LoadEnemies(worldData.enemies);
            }

            logger.Log("World data loaded successfully!", this, Logging.LogType.Info);
        } catch (System.Exception) {
            logger.Log("Error loading world data, initiating a new world", this, Logging.LogType.Error);
            NewWorld();
        }
    }

    public List<string> ListAllWorlds() {
        return worldDataHandler.GetAllWorlds(saveDirectory, fileExtension);
    }


    public string GetCurrentWorldFilePath() {
        if (worldData == null) {
            logger.Log("No active world data found.", this, Logging.LogType.Warning);
            return null;
        }
        return worldDataHandler.GetWorldFilePath(worldData.worldName, saveDirectory, fileExtension);
    }
}
