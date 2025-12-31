using Models;

public class PlayerSpawnerController : SpawnerController, IPersistent
{
    public override void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;
        PlayerSpawnAreaData spawnAreaData = new(transform.position, transform.localScale.x);
        worldData.playerSpawnAreas.Add(spawnAreaData);
    }
}
