using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CreatorLibraryController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Object storage config")]
    [SerializeField]
    public string libraryFilePath = "Assets/models.json";

    [SerializeField]
    public string modelsDirectory = "Models";

    [SerializeField]
    private Logging.Logger logger;
    private List<WorldObjectReference> objectDataReferences = new();

    public void Initialize()
    {
        libraryFilePath = System.IO.Path.Combine(Application.persistentDataPath, libraryFilePath);
        if (System.IO.File.Exists(libraryFilePath))
        {
            logger.Log($"Loading library from: {libraryFilePath}", this, Logging.LogType.Info);
            LoadLibrary();
        }
        else
        {
            GenerateLibrary(libraryFilePath);
        }
    }

    public List<WorldObjectReference> GetObjects()
    {
        return objectDataReferences;
    }

    private void LoadLibrary()
    {
        string json = System.IO.File.ReadAllText(libraryFilePath);
        List<WorldObjectReference> objects = JsonUtility
            .FromJson<WorldObjectReferenceList>(json)
            .objects;
        objectDataReferences = objects;
        logger.Log(
            $"Loaded {objectDataReferences.Count} objects into the library from: {libraryFilePath}",
            this,
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
            WorldObjectReference worldRef = new(
                System.Guid.NewGuid().ToString(),
                fileName,
                Vector3.one,
                Vector3.zero,
                Vector3.zero
            );
            objectDataReferences.Add(worldRef);
            logger.Log($"Added asset: {fileName}", this, Logging.LogType.Info);
        }
        string json = JsonUtility.ToJson(
            new WorldObjectReferenceList { objects = objectDataReferences },
            true
        );
        System.IO.File.WriteAllText(outputPath, json);
        logger.Log(
            $"Generated library with {objectDataReferences.Count} assets at: {outputPath}",
            this,
            Logging.LogType.Info
        );
    }
}

[System.Serializable]
public class WorldObjectReferenceList
{
    public List<WorldObjectReference> objects;
}
