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

    // [SerializeField]
    // private CreatorLibrary creatorLibrary;

    [SerializeField]
    private ConsumableItemLibrarySO consumableItemsDatabase;

    [SerializeField]
    private EnemyLibrarySO enemyDatabase;

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
        ClearWorld();
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

        // Copy items from their persistence database
        var itemsFromDb =
            consumableItemsDatabase != null
                ? consumableItemsDatabase.GetAllConsumableItems()
                : new List<ConsumableItem>();
        logger.Log(
            $"DataPersistenceManagerSO.SaveWorld: copying {itemsFromDb.Count} consumable items from database.",
            this,
            Logging.LogType.Info
        );
        worldData.consumableItems = itemsFromDb;

        // Copy enemies from their persistence database
        if (enemyDatabase != null)
        {
            var enemiesFromDb = enemyDatabase.GetAllEnemies() ?? new List<EnemyData>();
            logger.Log(
                $"DataPersistenceManagerSO.SaveWorld: copying {enemiesFromDb.Count} enemies from database.",
                this,
                Logging.LogType.Info
            );
            worldData.enemies = enemiesFromDb;
        }
        else
        {
            logger.Log(
                "DataPersistenceManagerSO: enemyDatabase is not assigned. Enemies will not be saved.",
                this,
                Logging.LogType.Warning
            );
        }

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

    // this is due to when saving, the data gets duplicated, so we clear it first
    // in the future we should figure a consistent saving mechanism to avoid this
    private void ClearWorld()
    {
        worldData.enemySpawnAreas.Clear();
        worldData.playerSpawnAreas.Clear();
        worldData.objectPlacementData.Clear();
        // NOTE: we intentionally do NOT clear consumableItems or enemies here.
        // Those are driven from the ScriptableObject databases (ConsumableItems, Enemy)
        // and are explicitly overwritten in SaveWorld from those sources.
        // Clearing them here caused item/enemy lists to appear "wiped" when saving/publishing.
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
