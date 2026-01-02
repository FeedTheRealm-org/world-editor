using System.Collections.Generic;
using Models;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLibrary", menuName = "Scriptable Objects/Library/EnemyLibrary")]
public class EnemyLibrarySO : ScriptableObject
{
    [SerializeField]
    private Logging.Logger logger;

    [Header("List of Enemies")]
    [SerializeField]
    private List<EnemyData> enemies = new List<EnemyData>();

    public void AddEnemy(EnemyData enemy)
    {
        if (enemies == null)
            enemies = new List<EnemyData>();
        if (enemy == null)
        {
            if (logger != null)
                logger.Log(
                    "Tried to add null EnemyData to Enemy database.",
                    this,
                    Logging.LogType.Warning
                );
            return;
        }
        if (logger != null)
            logger.Log($"Adding enemy: {enemy.name}", this, Logging.LogType.Info);
        enemies.Add(enemy);
    }

    public List<EnemyData> GetAllEnemies()
    {
        if (enemies == null)
            enemies = new List<EnemyData>();
        return enemies;
    }

    public void RemoveEnemy(EnemyData enemy)
    {
        if (enemies == null || enemy == null)
            return;
        if (enemies.Remove(enemy))
        {
            if (logger != null)
                logger.Log($"Removed enemy: {enemy.name}", this, Logging.LogType.Info);
        }
        else
        {
            if (logger != null)
                logger.Log(
                    $"Failed to remove enemy (not found): {enemy.name}",
                    this,
                    Logging.LogType.Warning
                );
        }
    }

    public void LoadEnemies(List<EnemyData> enemiesToLoad)
    {
        if (enemies == null)
            enemies = new List<EnemyData>();
        enemies.Clear();
        if (enemiesToLoad != null)
            enemies.AddRange(enemiesToLoad);
        if (logger != null)
            logger.Log(
                $"Loaded {enemies.Count} enemies from world data.",
                this,
                Logging.LogType.Info
            );
    }
}
