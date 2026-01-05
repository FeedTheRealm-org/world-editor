using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(
    fileName = "StructureLoader",
    menuName = "Scriptable Objects/Loaders/StructureLoader"
)]
public class StructureLoaderSO : ScriptableObject, ILoadable, IPlaceableLoader
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

    private string PersistentLibraryFilePath =>
        System.IO.Path.Combine(Application.persistentDataPath, libraryFilePath);

    public void LoadLibrary()
    {
        logger.Log("Loading structure library...", this, Logging.LogType.Info);
        structureObjects.Clear();
        if (
            System.IO.File.Exists(PersistentLibraryFilePath)
            && new System.IO.FileInfo(PersistentLibraryFilePath).Length > 0
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
            GenerateLibrary(PersistentLibraryFilePath);
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
        string json = System.IO.File.ReadAllText(PersistentLibraryFilePath);
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
            System.IO.Directory.CreateDirectory(modelsPath);
            logger.Log(
                "Models directory not found. Created new directory at: "
                    + modelsPath
                    + " make sure to add model files here. and regenerate the library.",
                this,
                Logging.LogType.Warning
            );
            return;
        }
        string[] objectFiles = System
            .IO.Directory.GetFiles(modelsPath, "*.*")
            .Where(f => !f.EndsWith(".meta"))
            .ToArray();
        if (objectFiles.Length == 0)
        {
            logger.Log(
                "No model files found in models directory: "
                    + modelsPath
                    + " make sure to add model files here. and regenerate the library.",
                this,
                Logging.LogType.Warning
            );
            return;
        }
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
        string outputDirectory = System.IO.Path.GetDirectoryName(outputPath);
        if (!System.IO.Directory.Exists(outputDirectory))
        {
            System.IO.Directory.CreateDirectory(outputDirectory);
        }
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
        LoadLibrary(); // we make sure the library is loaded before placing objects
        _ = OnLoadAsync(worldData.objectPlacementData);
    }

    private async Task OnLoadAsync(List<StructureData> structureDatas)
    {
        foreach (var structureData in structureDatas)
        {
            StructureObject structureObject = structureObjects.Find(obj =>
                obj.id == structureData.id
            );
            if (structureObject == null)
                continue;
            structureObject.size = structureData.size;
            structureObject.rotation = structureData.rotation;
            structureObject.offset = structureData.offset;
            GameObject structureInstance = await structureObject.GetPlaceableObject(
                WorldLayers.WorldObjectLayer
            );
            structureInstance.transform.position = structureData.position;
        }
    }
}

[System.Serializable]
public class WorldObjectReferenceList
{
    public List<StructureObject> objects;
}
