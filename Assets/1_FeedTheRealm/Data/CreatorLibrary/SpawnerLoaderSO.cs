using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(
    fileName = "SpawnerLoader",
    menuName = "Scriptable Objects/WorldEditor/SpawnerLoader"
)]
public class SpawnerLoaderSO : ScriptableObject, ILoadable
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private List<SpawnerTypeGameObject> spawnerDefinitions = new();
    private List<SpawnerObject> spawnerObjects = new();

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
        spawnerObjects.Clear();
        logger.Log("Loading spawner objects...", null, Logging.LogType.Info);
        foreach (var entry in spawnerDefinitions)
        {
            SpawnerObject spawnerObject = new(
                entry.spawnerType,
                entry.spawnRadius,
                entry.spawnerPrefab
            );
            spawnerObjects.Add(spawnerObject);
        }
    }

    public List<IPlaceable> GetObjects()
    {
        logger.Log(
            "Retrieving structure objects: count = " + spawnerObjects.Count,
            null,
            Logging.LogType.Info
        );
        return spawnerObjects.Cast<IPlaceable>().ToList();
    }

    public void LoadWorld(WorldData worldData)
    {
        logger.Log("Loading spawners into world...", this, Logging.LogType.Info);
        foreach (var spawner in worldData.enemySpawnAreas)
        {
            _ = OnLoadAsync(spawner);
        }
        foreach (var spawner in worldData.playerSpawnAreas)
        {
            _ = OnLoadAsync(spawner);
        }
    }

    private async Task OnLoadAsync(EnemySpawnAreaData spawnerData)
    {
        await LoadSpawnerAsync(SpawnerType.EnemySpawner, spawnerData.Radius, spawnerData.Position);
    }

    private async Task OnLoadAsync(PlayerSpawnAreaData spawnerData)
    {
        await LoadSpawnerAsync(SpawnerType.PlayerSpawner, spawnerData.Radius, spawnerData.Position);
    }

    private async Task LoadSpawnerAsync(SpawnerType type, float radius, Vector3 position)
    {
        GameObject spawnerPrefab = spawnerDefinitions
            .FirstOrDefault(s => s.spawnerType == type)
            ?.spawnerPrefab;
        SpawnerObject spawnerObject = new(type, spawnRadius: radius, spawnerPrefab);
        GameObject spawnerInstance = await spawnerObject.GetPlaceableObject(
            WorldLayers.WorldObjectLayer
        );
        spawnerInstance.transform.position = position;
    }
}

[Serializable]
public class SpawnerTypeGameObject
{
    public SpawnerType spawnerType;
    public float spawnRadius;
    public GameObject spawnerPrefab;
}
