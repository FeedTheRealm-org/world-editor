using Models;

public class EnemySpawnerController : SpawnerController, IPersistent
{
    public override void SaveData(ref WorldData worldData)
    {
        EnemySpawnAreaData spawnAreaData = new(transform.position, transform.localScale.x);
        worldData.enemySpawnAreas.Add(spawnAreaData);
    }
}
