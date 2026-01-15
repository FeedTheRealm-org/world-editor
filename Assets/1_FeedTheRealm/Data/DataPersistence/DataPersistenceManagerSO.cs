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

    // ---------------- Public Methods ----------------

    public WorldData CurrentWorldData => worldData;

    public void NewWorld()
    {
        worldData = new WorldData();
    }

    public void SaveWorld(string worldName)
    {
        if (worldData == null)
            NewWorld();
        PrepWorld();
        worldData.worldName = worldName;
        SaveAllDataPersistentObjects();
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

    // ---------------- Private Methods ----------------

    private void SaveAllDataPersistentObjects()
    {
        var monoFound = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IPersistent>();
        var soFound = Resources.FindObjectsOfTypeAll<ScriptableObject>().OfType<IPersistent>();
        dataPersistenceObjects = new List<IPersistent>(monoFound.Concat(soFound));
        foreach (IPersistent dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref worldData);
        }
    }

    // TODO: this method preps the world to be saved but it's not scalable due to having to save ALL of the data
    // instead of only the changes in the world.
    private void PrepWorld()
    {
        if (worldData == null)
        {
            worldData = new WorldData();
        }

        // Preserve current identifiers while clearing dynamic collections
        string worldName = worldData.worldName;
        string worldId = worldData.id;

        // Clear existing collections to avoid duplicated data before re-saving
        worldData.enemySpawnAreas.Clear();
        worldData.playerSpawnAreas.Clear();
        worldData.objectPlacementData.Clear();
        worldData.consumableItems.Clear();
        worldData.weaponItems.Clear();

        // Restore identifiers on the same instance so all references remain valid
        worldData.worldName = worldName;
        worldData.id = worldId;
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
