using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatorLibrary", menuName = "Scriptable Objects/CreatorLibrary")]
public class CreatorLibrary : InitializableSO
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Object storage config")]
    [SerializeField]
    public string libraryFilePath = "Assets/models.json";

    [SerializeField]
    public string modelsDirectory = "Models";

    [SerializeField]
    private Logging.Logger logger;
    private List<WorldObjectController> objectData = new();

    protected override void OnInitialize()
    {
        string fullPath = System.IO.Path.Combine(Application.persistentDataPath, libraryFilePath);
        if (!System.IO.File.Exists(fullPath))
        {
            logger.Log(
                $"Library file not found at: {fullPath}. Generating new library. . .",
                this,
                Logging.LogType.Warning
            );
            GenerateLibrary(fullPath);
        }
        LoadLibrary();
    }

    protected override void OnReset()
    {
        objectData.Clear();
    }

    public List<WorldObjectController> GetObjects()
    {
        return objectData;
    }

    public void InitializeLibrary()
    {
        Reset();
        Initialize();
    }

    private void LoadLibrary()
    {
        string json = System.IO.File.ReadAllText(libraryFilePath);
        List<WorldObjectData> objects = JsonUtility.FromJson<WorldObjectDataList>(json).objects;
        foreach (WorldObjectData obj in objects)
        {
            // WorldObjectController worldObject = WorldObjectController.SetupObject(
            //     obj.id,
            //     obj.assetUrl,
            //     obj.size,
            //     obj.position,
            //     obj.rotation,
            //     obj.offset
            // );
            // objectData.Add(worldObject);
        }
        logger.Log(
            $"Loaded {objectData.Count} objects into the library from: {libraryFilePath}",
            this,
            Logging.LogType.Info
        );
    }

    private void GenerateLibrary(string outputPath)
    {
        List<WorldObjectData> objectData = new();
        string modelsPath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            modelsDirectory
        );
        if (!System.IO.Directory.Exists(modelsPath))
        {
            logger.Log($"Models directory not found at: {modelsPath}", this, Logging.LogType.Error);
            return;
        }
        string[] objectFiles = System
            .IO.Directory.GetFiles(modelsPath, "*.*")
            .Where(f => !f.EndsWith(".meta"))
            .ToArray();

        foreach (string objectFile in objectFiles)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(objectFile);

            // WorldObjectData data = new()
            // {
            //     id = System.Guid.NewGuid().ToString(),
            //     assetUrl = fileName,
            //     size = Vector3.one,
            //     position = Vector3.zero,
            //     rotation = Vector3.zero,
            //     offset = Vector3.zero,
            // };
            // objectData.Add(data);
            logger?.Log($"Added asset: {fileName}", this, Logging.LogType.Info);
        }

        // Write to JSON file
        WorldObjectDataList dataList = new() { objects = objectData };
        string json = JsonUtility.ToJson(dataList, true);
        System.IO.File.WriteAllText(outputPath, json);
        logger.Log(
            $"Generated library with {objectData.Count} assets at: {outputPath}",
            this,
            Logging.LogType.Info
        );
    }
}

[System.Serializable]
public class WorldObjectData
{
    public string id;
    public Vector3 size = Vector3.one;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 offset;
    public string objectUrl;
    public bool isLocalAsset = true;
    public GameObject missingObjectPrefab;

    public static WorldObjectData Create(
        string id,
        string assetUrl,
        Vector3 size,
        Vector3 position,
        Vector3 rotation,
        Vector3 offset,
        bool isLocalAsset = true,
        GameObject missingObjectPrefab = null
    )
    {
        return new WorldObjectData
        {
            id = id,
            objectUrl = assetUrl,
            size = size,
            position = position,
            rotation = rotation,
            offset = offset,
            isLocalAsset = isLocalAsset,
            missingObjectPrefab = missingObjectPrefab,
        };
    }
}

[System.Serializable]
public class WorldObjectDataList
{
    public List<WorldObjectData> objects;
}
