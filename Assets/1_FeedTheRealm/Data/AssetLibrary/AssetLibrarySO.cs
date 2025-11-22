using System;
using System.Collections.Generic;
using System.IO;
using Models;
using UnityEngine;

/// <summary>
/// This is the Asset Database, this is used for the world editor asset Library, in the future, this will be extended to be 
/// a generic library to store any kind of obect data, like assets, items, etc
/// </summary>
[CreateAssetMenu(fileName = "AssetLibrary", menuName = "Scriptable Objects/Persistence/AssetLibrary")]
public class AssetLibrarySO : ScriptableObject {
    [Header("Asset storage config")]
    [SerializeField] private string assetsDirectory = "Assets";
    [SerializeField] public string assetsFileName = "models.json";
    [SerializeField] private Logging.Logger logger;
    private bool isInitialized = false;
    private List<Asset> objectData = new();

    public Asset GetAssetById(string id) {
        EnsureInitialized();
        return objectData.Find(obj => obj.Id == id);
    }

    public List<Asset> GetAllAssets() {
        EnsureInitialized();
        return objectData;
    }

    private void EnsureInitialized() {
        if (!isInitialized) {
            InitializeDatabase();
        }
    }

    public void InitializeDatabase() {
        objectData.Clear();
        logger.Log("Initializing Asset Database...", this, Logging.LogType.Info);

        string assetDirPath = Path.Combine(Application.persistentDataPath, assetsDirectory);

        try {
            if (!File.Exists(Path.Combine(assetDirPath, assetsFileName))) {
                logger.Log($"Asset Database JSON not found at path: {Path.Combine(assetDirPath, assetsFileName)}", this, Logging.LogType.Error);
                return;
            }
            string fullPath = Path.Combine(assetDirPath, assetsFileName);
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