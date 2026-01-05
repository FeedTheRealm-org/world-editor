using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DataPersistence",
    menuName = "Scriptable Objects/Persistence/DataPersistenceManager"
)]
public class DataPersistenceManagerSO : ScriptableObject
{
    [Header("World storage config")]
    [SerializeField]
    private string saveDirectory = "Worlds";

    [SerializeField]
    private string fileExtension = ".world";

    [SerializeField]
    private Logging.Logger logger;

    private WorldData worldData = null;
    private List<IPersistent> dataPersistenceObjects = new();

    public WorldData CurrentWorldData => worldData;

    public void NewWorld()
    {
        worldData = new WorldData();
    }

    public void UnSetActiveWorld()
    {
        worldData = null;
    }

    public void SaveWorld(string worldName)
    {
        if (worldData == null)
        {
            logger.Log(
                "No world data found. A new world will be created for saving.",
                this,
                Logging.LogType.Warning
            );
            NewWorld();
        }
        PrepWorld();
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        worldData.worldName = worldName;
        logger.Log("Starting world save...", this, Logging.LogType.Info);

        foreach (IPersistent dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref worldData);
        }
        logger.Log(
            $"Saved {dataPersistenceObjects.Count} data persistence objects.",
            this,
            Logging.LogType.Info
        );

        WorldFileHandler.Save(worldData, saveDirectory, fileExtension);
        logger.Log("World data save completed!", this, Logging.LogType.Info);
    }

    public void SetActiveWorld(string dataFileName)
    {
        worldData = WorldFileHandler.Load(dataFileName, saveDirectory);
    }

    public List<string> ListAllWorlds()
    {
        return WorldFileHandler.GetAllWorlds(saveDirectory, fileExtension);
    }

    public string GetWorldFile(string worldName)
    {
        return worldName + fileExtension;
    }

    public bool WorldFileExists(string worldName)
    {
        return WorldFileHandler.IsWorldFilePresent(worldName, saveDirectory, fileExtension);
    }

    private List<IPersistent> FindAllDataPersistenceObjects()
    {
        var found = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IPersistent>();
        return new List<IPersistent>(found);
    }

    // TODO: this method preps the world to be saved but it's not scalable due to having to save ALL of the data
    // instead of only the changes in the world.
    private void PrepWorld()
    {
        worldData.objectPlacementData.Clear();
        worldData.enemySpawnAreas.Clear();
        worldData.playerSpawnAreas.Clear();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataPersistenceManagerSO))]
    public class DataPersistenceManagerSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DataPersistenceManagerSO manager = (DataPersistenceManagerSO)target;

            if (GUILayout.Button("Unset Active World"))
            {
                manager.NewWorld();
            }
        }
    }
#endif
}
