using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using UnityEngine;

public class StructureLoader : ILoadable
{
    // TODO: make these configurable
    private string libraryFilePath = "Assets/models.json";
    private string modelsDirectory = "Models";
    private Logging.Logger logger;
    private List<StructureObject> structureObjects = new();

    public StructureLoader(Logging.Logger logger)
    {
        this.logger = logger;
    }

    public void LoadLibrary()
    {
        libraryFilePath = System.IO.Path.Combine(Application.persistentDataPath, libraryFilePath);
        if (
            System.IO.File.Exists(libraryFilePath)
            && new System.IO.FileInfo(libraryFilePath).Length > 0
        )
        {
            LoadStructureLibrary();
        }
        else
        {
            GenerateLibrary(libraryFilePath);
        }
    }

    public List<IPlaceable> GetObjects()
    {
        logger.Log(
            "Retrieving structure objects: count = " + structureObjects.Count,
            null,
            Logging.LogType.Info
        );
        return structureObjects.Cast<IPlaceable>().ToList();
    }

    private void LoadStructureLibrary()
    {
        logger.Log(
            "Loading structure library from: " + libraryFilePath,
            null,
            Logging.LogType.Info
        );
        string json = System.IO.File.ReadAllText(libraryFilePath);
        List<StructureObject> objects = JsonUtility
            .FromJson<WorldObjectReferenceList>(json)
            .objects;
        structureObjects = objects;
        logger.Log(
            "Loaded structure objects: count = " + structureObjects.Count,
            null,
            Logging.LogType.Info
        );
    }

    private void GenerateLibrary(string outputPath)
    {
        string modelsPath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            modelsDirectory
        );
        if (!System.IO.Directory.Exists(modelsPath))
        {
            logger.Log($"Models directory not found at: {modelsPath}", null, Logging.LogType.Error);
            return;
        }
        string[] objectFiles = System
            .IO.Directory.GetFiles(modelsPath, "*.*")
            .Where(f => !f.EndsWith(".meta"))
            .ToArray();
        foreach (string objectFile in objectFiles)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(objectFile);
            StructureObject structureObject = new(
                System.Guid.NewGuid().ToString(),
                Vector3.one,
                Vector3.zero,
                Vector3.zero,
                fileName
            );
            structureObjects.Add(structureObject);
        }
        string json = JsonUtility.ToJson(
            new WorldObjectReferenceList { objects = structureObjects },
            true
        );
        System.IO.File.WriteAllText(outputPath, json);
    }

    public static void LoadWorld(WorldData worldData)
    {
        if (worldData.objectPlacementData == null)
            return;
        foreach (var structureData in worldData.objectPlacementData)
        {
            _ = OnLoadAsync(structureData);
        }
    }

    private static async Task OnLoadAsync(StructureData structureData)
    {
        StructureObject structureObject = new(
            structureData.id,
            structureData.size,
            structureData.rotation,
            structureData.offset,
            structureData.objectName
        );
        GameObject structureInstance = await structureObject.GetPlaceableObject(
            WorldLayers.WorldObjectLayer
        );
        structureInstance.transform.position = structureData.position;
    }
}

[System.Serializable]
public class WorldObjectReferenceList
{
    public List<StructureObject> objects;
}
