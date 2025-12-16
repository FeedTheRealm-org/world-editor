using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public List<Asset> GetAssetsFromWorld(WorldData worldData) {
        EnsureInitialized();
        var assetsSet = new HashSet<Asset>();
        foreach (var placedAsset in worldData.objectPlacementData) {
            var asset = GetAssetById(placedAsset.AssetDataId);
            if (asset != null) {
                assetsSet.Add(asset);
            } else {
                logger.Log($"Asset with ID {placedAsset.AssetDataId} not found in Asset Library.", this, Logging.LogType.Warning);
            }
        }
        logger.Log($"Found {assetsSet.Count} unique assets from world data.", this, Logging.LogType.Info);
        return new List<Asset>(assetsSet);
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

    /// <summary>
    /// Initializes the asset database by loading existing assets from a JSON file 
    /// and scanning the Models directory for new assets.
    /// </summary>
    public void InitializeDatabase() {
        objectData.Clear();
        string assetDirPath = Path.Combine(Application.persistentDataPath, assetsDirectory);

        try {
            if (!Directory.Exists(assetDirPath)) {
                Directory.CreateDirectory(assetDirPath);
            }

            string modelsResourcePath = Path.Combine(Application.dataPath, "Resources", "Models");
            if (!Directory.Exists(modelsResourcePath)) {
                logger.Log($"Models directory not found at: {modelsResourcePath}", this, Logging.LogType.Error);
                return;
            }

            string filePath = Path.Combine(assetDirPath, assetsFileName);

            // Load existing assets from JSON file
            AssetModelsRaw rawModels = new() { assetObjects = new Asset[0] };
            List<Asset> existingAssets = new();

            if (File.Exists(filePath)) {
                try {
                    using FileStream fs = new(filePath, FileMode.Open);
                    using StreamReader reader = new(fs);
                    string jsonContent = reader.ReadToEnd();
                    rawModels = JsonUtility.FromJson<AssetModelsRaw>(jsonContent);
                    existingAssets.AddRange(rawModels.assetObjects);
                    logger.Log($"Loaded {existingAssets.Count} existing assets from JSON.", this, Logging.LogType.Info);
                } catch (Exception e) {
                    logger.Log($"Error loading existing JSON: {e}", this, Logging.LogType.Error);
                }
            }

            string materialsResourcePath = Path.Combine(Application.dataPath, "Resources", "Materials");
            string[] modelFiles = Directory.GetFiles(modelsResourcePath)
                .Where(f => Path.GetExtension(f) != ".meta")
                .ToArray();

            List<Asset> newAssets = new();
            int addedCount = 0;

            foreach (string modelFile in modelFiles) {
                string fileName = Path.GetFileNameWithoutExtension(modelFile);
                string fileNameWithExt = Path.GetFileName(modelFile);

                bool assetExists = existingAssets.Any(a => a.ModelPath == $"Models/{fileNameWithExt}");

                if (assetExists) {
                    logger.Log($"Asset already exists: {fileName}", this, Logging.LogType.Info);
                    continue;
                }

                string materialPath = "";
                if (Directory.Exists(materialsResourcePath)) {
                    string[] materialFiles = Directory.GetFiles(materialsResourcePath, $"{fileName}.*");
                    if (materialFiles.Length > 0) {
                        materialPath = $"Materials/{Path.GetFileName(materialFiles[0])}";
                    }
                }

                Asset asset = new(
                    Guid.NewGuid().ToString(),
                    fileName,
                    Vector2Int.one,
                    $"Models/{fileNameWithExt}",
                    materialPath
                );

                newAssets.Add(asset);
                existingAssets.Add(asset);
                addedCount++;

                logger.Log($"New asset added: {fileName}", this, Logging.LogType.Info);
            }

            objectData.AddRange(existingAssets);

            if (addedCount > 0) {
                rawModels.assetObjects = existingAssets.ToArray();
                string jsonContent = JsonUtility.ToJson(rawModels, true);

                using (FileStream fs = new(filePath, FileMode.Create))
                using (StreamWriter writer = new(fs)) {
                    writer.Write(jsonContent);
                }

                logger.Log($"Asset Database updated: {addedCount} new assets added.", this, Logging.LogType.Info);
            } else {
                logger.Log("No new assets to add.", this, Logging.LogType.Info);
            }

            logger.Log($"Asset Database initialized with {objectData.Count} total assets.", this, Logging.LogType.Info);
            isInitialized = true;

        } catch (Exception e) {
            logger.Log($"Error initializing Asset Database: {e}", this, Logging.LogType.Error);
        }
    }
}


[Serializable]
public class AssetModelsRaw {
    public Asset[] assetObjects;
}