using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public class SpawnerLoader : ILoadable
{
    private List<SpawnerObject> objectDataReferences = new();
    private List<SpawnerTypeGameObject> spawnerDefinition = new();
    private Logging.Logger logger;

    public SpawnerLoader(Logging.Logger logger, List<SpawnerTypeGameObject> spawnerTypeGameObjects)
    {
        spawnerDefinition = spawnerTypeGameObjects;
        this.logger = logger;
    }

    public void LoadLibrary()
    {
        logger.Log("Loading spawner objects...", null, Logging.LogType.Info);
        foreach (var entry in spawnerDefinition)
        {
            SpawnerObject spawnerObject = new(
                entry.spawnerType,
                entry.spawnRadius,
                entry.spawnerPrefab
            );
            objectDataReferences.Add(spawnerObject);
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
}

[Serializable]
public class SpawnerTypeGameObject
{
    public SpawnerType spawnerType;
    public float spawnRadius;
    public GameObject spawnerPrefab;
}
