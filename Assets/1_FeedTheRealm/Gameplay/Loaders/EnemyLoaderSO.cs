using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Enemies;
using Models;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLoader", menuName = "Scriptable Objects/Loaders/EnemyLoader")]
public class EnemyLoaderSO : ScriptableObject, ILoadable, ICreatableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private WorldSelectedEvent worldSelectedEvent;

    private List<CreatorObject> enemies = new();

    void OnEnable()
    {
        worldSelectedEvent.OnRaised += LoadWorld;
    }

    void OnDisable()
    {
        worldSelectedEvent.OnRaised -= LoadWorld;
    }

    public List<CreatorObject> GetCreatables()
    {
        return enemies.FindAll(item => !item.IsDeleted);
    }

    public void AddCreatable(CreatorObject creatable)
    {
        enemies.Add(creatable);
    }

    public void RemoveCreatable(CreatorObject creatable)
    {
        creatable.Delete();
        enemies.Remove(creatable);
    }

    public void UpdateCreatable(CreatorObject creatable)
    {
        int index = enemies.FindIndex(item => item.ObjectId == creatable.ObjectId);
        if (index != -1)
        {
            enemies[index] = creatable;
        }
    }

    public void LoadWorld(WorldData worldData)
    {
        enemies.Clear();
        if (worldData == null)
        {
            logger.Log("ItemLoader.LoadWorld: worldData is null.", this, Logging.LogType.Warning);
            return;
        }

        foreach (EnemyData itemData in worldData.enemies ?? new List<EnemyData>())
        {
            enemies.Add(new GenericEnemy(itemData));
        }
        logger.Log($"Loaded {enemies.Count} enemies from world data.", this, Logging.LogType.Info);
    }
}
