using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "SpawnerLoader", menuName = "Scriptable Objects/Loaders/SpawnerLoader")]
public class SpawnerLoaderSO : ScriptableObject, ILoadable, IPlaceableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private List<SpawnerTypeGameObject> spawnerDefinitions = new();
    private Dictionary<SpawnerType, SpawnerObject> spawnerObjectsMap = new();

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
        spawnerObjectsMap.Clear();
        logger.Log("Loading spawner objects...", null, Logging.LogType.Info);
        foreach (var entry in spawnerDefinitions)
        {
            SpawnerObject spawnerObject = new(
                entry.spawnerType,
                entry.spawnRadius,
                entry.spawnerPrefab
            );
            spawnerObjectsMap[entry.spawnerType] = spawnerObject;
        }
        logger.Log(
            $"Loaded {spawnerObjectsMap.Count} spawner objects into library.",
            null,
            Logging.LogType.Info
        );
    }

    public List<IPlaceable> GetObjects()
    {
        logger.Log(
            "Retrieving structure objects: count = " + spawnerObjectsMap.Count,
            null,
            Logging.LogType.Info
        );
        return spawnerObjectsMap.Values.Cast<IPlaceable>().ToList();
    }

    public async void LoadWorld(WorldData worldData)
    {
        LoadLibrary();
        logger.Log("Loading spawners into world...", this, Logging.LogType.Info);
        var tasks = new List<Task>();
        foreach (var enemySpawnerData in worldData.enemySpawnAreas)
        {
            tasks.Add(LoadEnemySpawnerAsync(enemySpawnerData));
        }
        foreach (var playerSpawnerData in worldData.playerSpawnAreas)
        {
            tasks.Add(LoadPlayerSpawnerAsync(playerSpawnerData));
        }
        await Task.WhenAll(tasks);
        logger.Log("Spawners loaded successfully.", this, Logging.LogType.Info);
    }

    private async Task LoadPlayerSpawnerAsync(PlayerSpawnerData spawnerData) =>
        await LoadSpawnerAsync<PlayerSpawnerController, PlayerSpawnerData>(
            SpawnerType.PlayerSpawner,
            spawnerData,
            (controller, data) => controller.PlayerSpawnData = data
        );

    private async Task LoadEnemySpawnerAsync(EnemySpawnerData spawnerData) =>
        await LoadSpawnerAsync<EnemySpawnerController, EnemySpawnerData>(
            SpawnerType.EnemySpawner,
            spawnerData,
            (controller, data) => controller.EnemySpawnData = data
        );

    private async Task LoadSpawnerAsync<TController, TSpawnData>(
        SpawnerType spawnerType,
        TSpawnData spawnData,
        Action<TController, TSpawnData> setDataAction
    )
        where TController : Component
    {
        if (!spawnerObjectsMap.TryGetValue(spawnerType, out var spawnerObject))
        {
            logger.Log(
                $"Spawner type {spawnerType} not found in library.",
                this,
                Logging.LogType.Error
            );
            return;
        }
        try
        {
            GameObject spawnerInstance = await spawnerObject.GetPlaceableObject(
                WorldLayers.WorldObjectLayer
            );
            if (!spawnerInstance.TryGetComponent(out TController controller))
            {
                logger.Log(
                    $"Spawner instance does not have {typeof(TController).Name} component.",
                    this,
                    Logging.LogType.Error
                );
                return;
            }
            setDataAction(controller, spawnData);
        }
        catch (Exception e)
        {
            logger.Log($"Error loading spawner: {e.Message}", this, Logging.LogType.Error);
        }
    }
}

[Serializable]
public class SpawnerTypeGameObject
{
    public SpawnerType spawnerType;
    public float spawnRadius;
    public GameObject spawnerPrefab;
}
