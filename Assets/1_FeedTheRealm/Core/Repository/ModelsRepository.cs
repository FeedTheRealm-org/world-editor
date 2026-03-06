using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTRShared.Runtime.Models;
using UnityEngine;

// public class ModelsRepository
// {
//     private readonly string libraryFilePath = "Models/models.json";
//     private readonly string modelsDirectory = "Models";
//     private readonly Dictionary<string, GameObject> modelCache;
//     private string PersistentLibraryFilePath =>
//     Path.Combine(Application.persistentDataPath, libraryFilePath);

//     private string PersistentModelsDirectory =>
//         Path.Combine(Application.streamingAssetsPath, modelsDirectory);

//     public ModelsRepository()
//     {
//         modelCache = new Dictionary<string, GameObject>();
//     }

//     public GameObject LoadModel(string modelPath)
//     {
//         if (modelCache.TryGetValue(modelPath, out var cachedModel))
//         {
//             return cachedModel;
//         }

//         GameObject model = Resources.Load<GameObject>(modelPath);

//         if (model == null)
//         {
//             return null;
//         }

//         modelCache[modelPath] = model;
//         return model;
//     }

//     public GameObject InstantiateModel(string modelPath, Vector3 position = default, Quaternion rotation = default)
//     {
//         GameObject model = LoadModel(modelPath);

//         if (model == null)
//             return null;

//         GameObject instance = UnityEngine.Object.Instantiate(model, position, rotation);
//         return instance;
//     }

//     public void ClearCache()
//     {
//         modelCache.Clear();
//     }

//     private void LoadStructureLibrary()
//     {
//         string json = System.IO.File.ReadAllText(PersistentLibraryFilePath);
//         List<StructureData> structureDataList = JsonUtility
//             .FromJson<WorldObjectReferenceList>(json)
//             .structureData;
//         foreach (var structureData in structureDataList)
//         {
//             StructureObject structureObject = new(
//                 structureData,
//                 structureData.structureName,
//                 structurePrefab
//             );
//             structureObjects.Add(structureObject);
//         }
//     }

//     public void LoadModels(string outputPath)
//     {
//         string modelsPath = PersistentModelsDirectory;
//         if (!EnsurePathExists(modelsPath, true))
//         {
//             return;
//         }
//         string[] objectFiles = System
//             .IO.Directory.GetFiles(modelsPath, "*.*")
//             .Where(f => !f.EndsWith(".meta"))
//             .ToArray();

//         List<StructureData> structureDataList = objectFiles
//             .Select(objectFile => new StructureData(
//                 Guid.NewGuid().ToString(),
//                 GetFileNameWithoutExtension(objectFile),
//                 Vector3.one,
//                 Vector3.zero,
//                 Vector3.zero,
//                 Vector3.zero
//             ))
//             .ToList();
//         string json = JsonUtility.ToJson(
//             new WorldObjectReferenceList { structureData = structureDataList },
//             true
//         );
//         string outputDirectory = GetDirectoryName(outputPath);
//         if (!System.IO.Directory.Exists(outputDirectory))
//         {
//             System.IO.Directory.CreateDirectory(outputDirectory);
//         }
//         System.IO.File.WriteAllText(outputPath, json);
//     }

//     private bool EnsurePathExists(string path, bool isDirectory = false)
//     {
//         if (isDirectory)
//         {
//             if (!System.IO.Directory.Exists(path))
//             {
//                 System.IO.Directory.CreateDirectory(path);
//                 return false;
//             }
//             return true;
//         }
//         if (!System.IO.File.Exists(path))
//         {
//             string outputDirectory = GetDirectoryName(path);
//             if (!System.IO.Directory.Exists(outputDirectory))
//             {
//                 System.IO.Directory.CreateDirectory(outputDirectory);
//             }
//             System.IO.File.WriteAllText(path, "");
//             return false;
//         }
//         return true;
//     }
// }

[Serializable]
public class WorldObjectReferenceList
{
    public List<StructureData> structureData;
}
