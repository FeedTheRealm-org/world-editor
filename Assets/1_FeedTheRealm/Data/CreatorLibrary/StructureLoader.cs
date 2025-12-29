using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureLoader : ILoadable
{
    // TODO: make these configurable
    private string libraryFilePath = "Assets/models.json";
    private string modelsDirectory = "Models";

    private List<WorldObjectDefinition> objectDataReferences = new();

    public void LoadLibrary()
    {
        libraryFilePath = System.IO.Path.Combine(Application.persistentDataPath, libraryFilePath);
        if (System.IO.File.Exists(libraryFilePath))
        {
            LoadStructureLibrary();
        }
        else
        {
            GenerateLibrary(libraryFilePath);
        }
    }

    public List<WorldObjectDefinition> GetObjects()
    {
        Debug.Log("Retrieving structure objects: count = " + objectDataReferences.Count);
        return objectDataReferences;
    }

    private void LoadStructureLibrary()
    {
        Debug.Log("Loading structure library from: " + libraryFilePath);
        string json = System.IO.File.ReadAllText(libraryFilePath);
        List<WorldObjectDefinition> objects = JsonUtility
            .FromJson<WorldObjectReferenceList>(json)
            .objects;
        objectDataReferences = objects;
        Debug.Log("Loaded structure objects: count = " + objectDataReferences.Count);
    }

    private void GenerateLibrary(string outputPath)
    {
        string modelsPath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            modelsDirectory
        );
        if (!System.IO.Directory.Exists(modelsPath))
        {
            Debug.LogError($"Models directory not found at: {modelsPath}");
            return;
        }
        string[] objectFiles = System
            .IO.Directory.GetFiles(modelsPath, "*.*")
            .Where(f => !f.EndsWith(".meta"))
            .ToArray();
        foreach (string objectFile in objectFiles)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(objectFile);
            StructureObject worldRef = new(
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
    public List<WorldObjectDefinition> objects;
}
