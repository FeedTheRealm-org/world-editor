using System.Threading.Tasks;
using UnityEngine;

public enum SpawnerType
{
    EnemySpawner,
    PlayerSpawner,
}

[System.Serializable]
public class SpawnerObject : IPlaceable
{
    public SpawnerType spawnerType;
    public float spawnRadius;
    private GameObject spawnerObject;

    public SpawnerObject(SpawnerType spawnerType, float spawnRadius, GameObject spawnerObject)
    {
        this.spawnerType = spawnerType;
        this.spawnRadius = spawnRadius;
        this.spawnerObject = spawnerObject;
        spawnerObject.transform.localScale = new Vector3(spawnRadius, 0.5f, spawnRadius);
        spawnerObject.SetActive(false);
    }

    public string DisplayName => spawnerType.ToString();

    public async Task<GameObject> PlaceObject(int layerMask)
    {
        GameObject instance = Object.Instantiate(spawnerObject);
        instance.SetActive(true);
        instance.name = spawnerType.ToString();
        return instance;
    }
}
