using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureLoader : ILoadable
{
    // TODO: make these configurable
    private string libraryFilePath = "Assets/models.json";
    private string modelsDirectory = "Models";
    private Logging.Logger logger;
    private List<WorldObject> objectDataReferences = new();

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
            "Retrieving structure objects: count = " + objectDataReferences.Count,
            null,
            Logging.LogType.Info
        );
        return objectDataReferences.Cast<IPlaceable>().ToList();
    }

    private void LoadStructureLibrary()
    {
        logger.Log(
            "Loading structure library from: " + libraryFilePath,
            null,
            Logging.LogType.Info
        );
        string json = System.IO.File.ReadAllText(libraryFilePath);
        List<WorldObject> objects = JsonUtility.FromJson<WorldObjectReferenceList>(json).objects;
        objectDataReferences = objects;
        logger.Log(
            "Loaded structure objects: count = " + objectDataReferences.Count,
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
            WorldObject worldRef = new(
                System.Guid.NewGuid().ToString(),
                Vector3.one,
                Vector3.zero,
                Vector3.zero,
                fileName
            );
            objectDataReferences.Add(worldRef);
        }
        string json = JsonUtility.ToJson(
            new WorldObjectReferenceList { objects = objectDataReferences },
            true
        );
        System.IO.File.WriteAllText(outputPath, json);
    }
}

[System.Serializable]
public class WorldObjectReferenceList
{
    public List<WorldObject> objects;
}
