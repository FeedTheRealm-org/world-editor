using System.Collections.Generic;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "EnemyLoader", menuName = "Scriptable Objects/WorldEditor/EnemyLoader")]
public class EnemyLoader : ScriptableObject, ILoadable
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private EnemyLibrarySO enemyDatabase;

    void OnEnable()
    {
        SelectionRaiser.WorldSelected += LoadWorld;
    }

    void OnDisable()
    {
        SelectionRaiser.WorldSelected -= LoadWorld;
    }

    // No separate shared enemy library for now – kept for interface symmetry.
    public void LoadLibrary() { }

    // Populate the Enemy ScriptableObject database from the world data
    // so that editor UIs (AddEnemyMenu, etc.) reflect the selected world.
    public void LoadWorld(WorldData worldData)
    {
        if (worldData == null)
        {
            logger.Log("EnemyLoader.LoadWorld: worldData is null.", this, Logging.LogType.Warning);
            return;
        }

        if (enemyDatabase == null)
        {
            logger.Log(
                "EnemyLoader.LoadWorld: enemyDatabase is not assigned.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        var enemiesFromWorld = worldData.enemies ?? new List<EnemyData>();
        logger.Log(
            $"EnemyLoader.LoadWorld: loading {enemiesFromWorld.Count} enemies into database.",
            this,
            Logging.LogType.Info
        );

        enemyDatabase.LoadEnemies(enemiesFromWorld);
    }
}
