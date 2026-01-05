using System.Collections.Generic;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "EnemyLoader", menuName = "Scriptable Objects/Loaders/EnemyLoader")]
public class EnemyLoader : ScriptableObject, ILoadable, ICreatableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    private List<ICreatable> enemies = new();

    void OnEnable()
    {
        SelectionRaiser.WorldSelected += LoadWorld;
    }

    void OnDisable()
    {
        SelectionRaiser.WorldSelected -= LoadWorld;
    }

    public List<ICreatable> GetCreatables()
    {
        return enemies.FindAll(item => !item.IsDeleted);
    }

    public void AddCreatable(ICreatable creatable)
    {
        enemies.Add(creatable);
    }

    public void RemoveCreatable(ICreatable creatable)
    {
        creatable.Delete();
        enemies.Remove(creatable);
    }

    public void UpdateCreatable(ICreatable creatable)
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
    }
}
