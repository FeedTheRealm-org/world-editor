using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// This is the Asset Database, this is used for the world editor asset Library, in the future, this will be extended to be 
/// a generic library to store any kind of obect data, like assets, items, etc
/// </summary>
[CreateAssetMenu(fileName = "AssetLibrary", menuName = "Scriptable Objects/Persistence/AssetLibrary")]
public class AssetDatabaseSO : ScriptableObject {

    [SerializeField] private Logging.Logger logger;
    [HideInInspector] public List<Asset> objectData = new();
    [SerializeField] public string dataFileName = "models.json";
    private bool isInitialized = false;


    public Asset GetAssetById(int id) {
        EnsureInitialized();
        return objectData.Find(obj => obj.Id == id);
    }

    private void EnsureInitialized() {
        if (!isInitialized) {
            InitializeDatabase();
        }
    }

    public void InitializeDatabase() {
        objectData.Clear();
        logger.Log("Initializing Asset Database...", this, Logging.LogType.Info);

        string dataDirPath = "Assets";

        // Combine with persistent data path
        dataDirPath = Path.Combine(Application.persistentDataPath, dataDirPath);

        try {
            if (!File.Exists(Path.Combine(dataDirPath, dataFileName))) {
                logger.Log($"Asset Database JSON not found at path: {Path.Combine(dataDirPath, dataFileName)}", this, Logging.LogType.Error);
                return;
            }
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            using FileStream fs = new(fullPath, FileMode.Open);
            using StreamReader reader = new(fs);
            string jsonContent = reader.ReadToEnd();

            AssetModelsRaw rawModels = JsonUtility.FromJson<AssetModelsRaw>(jsonContent);

            foreach (Asset model in rawModels.assetObjects) {
                // TODO: add validations later (if needed)
                objectData.Add(model);
            }
            logger.Log($"Asset Database initialized with {objectData.Count} assets.", this, Logging.LogType.Info);
            isInitialized = true;
        } catch (Exception e) {
            logger.Log($"Error loading Asset Database JSON: {e}", this, Logging.LogType.Error);
        }
    }


}


[Serializable]
public class AssetModelsRaw {
    public Asset[] assetObjects;
}