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
[CreateAssetMenu(
    fileName = "AssetLibrary",
    menuName = "Scriptable Objects/Persistence/AssetLibrary"
)]
public class AssetLibrarySO : ScriptableObject
{
    [Header("Asset storage config")]
    [SerializeField]
    private string assetsDirectory = "Assets";

    [SerializeField]
    public string assetsFileName = "models.json";

    [SerializeField]
    private Logging.Logger logger;
    private bool isInitialized = false;
    private List<Asset> objectData = new();
    private List<Asset> spawnerAssets = new();

    [SerializeField]
    private List<SpawnerData> spawners = new();

    public Asset GetAssetById(string id)
    {
        EnsureInitialized();
        return objectData.Find(obj => obj.Id == id);
    }

    public Asset GetSpawnerByName(string name)
    {
        EnsureInitialized();
        return spawnerAssets.Find(obj => obj.Name == name);
    }

    public List<Asset> GetAssetsFromWorld(WorldData worldData)
    {
        EnsureInitialized();
        var assetsSet = new HashSet<Asset>();
        foreach (var placedAsset in worldData.objectPlacementData)
        {
            var asset = GetAssetById(placedAsset.AssetDataId);
            if (asset != null)
            {
                assetsSet.Add(asset);
            }
            else
            {
                logger.Log(
                    $"Asset with ID {placedAsset.AssetDataId} not found in Asset Library.",
                    this,
                    Logging.LogType.Warning
                );
            }
        }
        logger.Log(
            $"Found {assetsSet.Count} unique assets from world data.",
            this,
            Logging.LogType.Info
        );
        return new List<Asset>(assetsSet);
    }

    public List<Asset> GetAllAssets()
    {
        EnsureInitialized();
        return objectData;
    }

    public List<Asset> GetAllSpawners()
    {
        EnsureInitialized();
        return spawnerAssets;
    }

    public void ForceReinitialize()
    {
        InitializeDatabase();
        InitSpawners();
    }

    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            InitializeDatabase();
            InitSpawners();
        }
    }

    /// <summary>
    /// Initializes the asset database by loading existing assets from a JSON file
    /// and scanning the Models directory for new assets. Also copies models to StreamingAssets.
    /// </summary>
    private void InitializeDatabase()
    {
        objectData.Clear();
        string assetDirPath = Path.Combine(Application.persistentDataPath, assetsDirectory);

        try
        {
            if (!Directory.Exists(assetDirPath))
            {
                Directory.CreateDirectory(assetDirPath);
            }

            // TODO: This is a temporary solution to make models available at runtime when publishing a world
            CopyModelsToStreamingAssets();

            GameObject[] models = Resources.LoadAll<GameObject>("Models");

            logger.Log(
                $"[AssetLibrary] Models found in Resources: {models.Length}",
                this,
                Logging.LogType.Info
            );

            if (models.Length == 0)
            {
                logger.Log(
                    "[AssetLibrary] No models found in Resources/Models",
                    this,
                    Logging.LogType.Error
                );
            }

            string filePath = Path.Combine(assetDirPath, assetsFileName);

            // Load existing assets from JSON file
            AssetModelsRaw rawModels = new() { assetObjects = new Asset[0] };
            List<Asset> assets = new();

            if (File.Exists(filePath))
            {
                try
                {
                    using FileStream fs = new(filePath, FileMode.Open);
                    using StreamReader reader = new(fs);
                    string jsonContent = reader.ReadToEnd();
                    rawModels = JsonUtility.FromJson<AssetModelsRaw>(jsonContent);
                    assets.AddRange(rawModels.assetObjects);
                    logger.Log(
                        $"Loaded {assets.Count} existing assets from JSON.",
                        this,
                        Logging.LogType.Info
                    );
                }
                catch (Exception e)
                {
                    logger.Log($"Error loading existing JSON: {e}", this, Logging.LogType.Error);
                }
            }

            string materialsResourcePath = Path.Combine(
                Application.dataPath,
                "Resources",
                "Materials"
            );
            int addedCount = 0;

            foreach (GameObject prefab in models)
            {
                string name = prefab.name;

                bool assetExists = assets.Any(a => a.Name == name);
                if (assetExists)
                    continue;

                Asset asset = new(
                    Guid.NewGuid().ToString(),
                    name,
                    Vector2Int.one,
                    $"Models/{name}",
                    ""
                );
                assets.Add(asset);
                addedCount++;
            }

            objectData.AddRange(assets);

            if (addedCount > 0)
            {
                rawModels.assetObjects = assets.ToArray();
                string jsonContent = JsonUtility.ToJson(rawModels, true);

                using (FileStream fs = new(filePath, FileMode.Create))
                using (StreamWriter writer = new(fs))
                {
                    writer.Write(jsonContent);
                }

                logger.Log(
                    $"Asset Database updated: {addedCount} new assets added.",
                    this,
                    Logging.LogType.Info
                );
            }
            else
            {
                logger.Log("No new assets to add.", this, Logging.LogType.Info);
            }

            logger.Log(
                $"Asset Database initialized with {objectData.Count} total assets.",
                this,
                Logging.LogType.Info
            );
            isInitialized = true;
        }
        catch (Exception e)
        {
            logger.Log($"Error initializing Asset Database: {e}", this, Logging.LogType.Error);
        }
    }

    private void CopyModelsToStreamingAssets()
    {
        string sourcePath = Path.Combine(Application.dataPath, "Resources", "Models");
        string destPath = Path.Combine(Application.dataPath, "StreamingAssets", "Models");

        if (!Directory.Exists(sourcePath))
        {
            logger.Log(
                $"[AssetLibrary] Source Models directory not found: {sourcePath}",
                this,
                Logging.LogType.Warning
            );
            return;
        }

        try
        {
            // Create destination directory if it doesn't exist
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            // Copy all files from Resources/Models to StreamingAssets/Models
            string[] files = Directory.GetFiles(sourcePath);
            foreach (string file in files)
            {
                // Skip .meta files
                if (file.EndsWith(".meta"))
                    continue;

                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destPath, fileName);
                File.Copy(file, destFile, true);
            }
        }
        catch (Exception e)
        {
            logger.Log(
                $"[AssetLibrary] Error copying models to StreamingAssets: {e}",
                this,
                Logging.LogType.Error
            );
        }
    }

    private void InitSpawners()
    {
        spawnerAssets.Clear();
        // Don't instantiate here, just create Asset references to the prefab
        foreach (var spawnerData in spawners)
        {
            SpawnerController spawnerController =
                spawnerData.spawnerPrefab.GetComponent<SpawnerController>();
            if (spawnerController == null)
            {
                logger.Log(
                    $"Spawner prefab {spawnerData.spawnerPrefab.name} does not have a SpawnerController component.",
                    this,
                    Logging.LogType.Warning
                );
                continue;
            }
            spawnerController.SetSize(spawnerData.size);
            string spawnerName = $"{spawnerController.GetSpawnerType()}_spawn";
            Asset spawnerAsset = new Asset(
                spawnerName,
                spawnerName,
                spawnerController.GetSize(),
                spawnerData.spawnerPrefab
            );
            spawnerAssets.Add(spawnerAsset);
        }
    }
}

[Serializable]
public class AssetModelsRaw
{
    public Asset[] assetObjects;
}

[Serializable]
public class SpawnerData
{
    public GameObject spawnerPrefab;
    public int size;
}
