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


    public WorldData CurrentWorldData => worldData;

    public void NewWorld() {
        worldData = new WorldData();
    }

    public void SaveWorld(string worldName = null) {
        if (worldData == null) {
            logger.Log("No world data found. A new world will be created for saving.", this, Logging.LogType.Warning);
            NewWorld();
        }
        worldData.worldName = worldName;
        fileDataHandler.Save(worldData, saveDirectory, fileExtension);
        logger.Log("World data save completed!", this, Logging.LogType.Info);
    }

    public void LoadWorld(string dataFileName = null) {
        worldData = fileDataHandler.Load(dataFileName, saveDirectory);
        if (worldData == null) {
            NewWorld();
            logger.Log($"No world data found for: [{dataFileName}], creating new world.", this, Logging.LogType.Warning);
            return;
        }
        logger.Log("World data loaded successfully!", this, Logging.LogType.Info);
    }

    public List<string> ListAllWorlds() {
        return fileDataHandler.GetAllWorlds(saveDirectory, fileExtension);
    }
}
