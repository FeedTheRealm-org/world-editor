using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(
    fileName = "StructureLoader",
    menuName = "Scriptable Objects/WorldEditor/StructureLoader"
)]
public class StructureLoaderSO : ScriptableObject, ILoadable
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private string libraryFilePath = "Assets/models.json";

    [SerializeField]
    private string modelsDirectory = "Models";
    private List<StructureObject> structureObjects = new();

    void OnEnable()
    {
        SelectionRaiser.WorldSelected += LoadWorld;
    }

    void OnDisable()
    {
        SelectionRaiser.WorldSelected -= LoadWorld;
    }

    public void LoadLibrary()
    {
        logger.Log("Loading structure library...", this, Logging.LogType.Info);
        structureObjects.Clear();
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
            logger.Log(
                "Structure library file not found. Generating new library...",
                this,
                Logging.LogType.Warning
            );
            GenerateLibrary(libraryFilePath);
        }
        logger.Log(
            "Structure library loaded. Count: " + structureObjects.Count,
            this,
            Logging.LogType.Info
        );
    }

    public List<IPlaceable> GetObjects()
    {
        return structureObjects.Cast<IPlaceable>().ToList();
    }

    private void LoadStructureLibrary()
    {
        string json = System.IO.File.ReadAllText(libraryFilePath);
        List<StructureObject> objects = JsonUtility
            .FromJson<WorldObjectReferenceList>(json)
            .objects;
        structureObjects = objects;
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

    public string GetModelFilePath(string structureName)
    {
        return System.IO.Path.Combine(
            Application.streamingAssetsPath,
            modelsDirectory,
            structureName + ".glb"
        );
    }

    public bool IsModelPresent(string structureName)
    {
        string modelPath = GetModelFilePath(structureName);
        return System.IO.File.Exists(modelPath);
    }

    public void LoadWorld(WorldData worldData)
    {
        if (worldData.objectPlacementData == null)
            return;
        foreach (var structureData in worldData.objectPlacementData)
        {
            logger.Log(
                $"Loading structure: {structureData.structureName} at position {structureData.position}",
                this,
                Logging.LogType.Info
            );
            _ = OnLoadAsync(structureData);
        }
    }

    private async Task OnLoadAsync(StructureData structureData)
    {
        StructureObject structureObject = structureObjects.Find(obj => obj.id == structureData.id);
        structureObject.size = structureData.size;
        structureObject.rotation = structureData.rotation;
        structureObject.offset = structureData.offset;

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
