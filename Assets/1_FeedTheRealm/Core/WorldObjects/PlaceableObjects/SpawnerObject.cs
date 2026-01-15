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
    private float spawnerHeight = 0.1f;

    public SpawnerObject(SpawnerType spawnerType, float spawnRadius, GameObject spawnerObject)
    {
        this.spawnerType = spawnerType;
        this.spawnRadius = spawnRadius;
        this.spawnerObject = spawnerObject;
        spawnerObject.transform.localScale = new Vector3(spawnRadius, spawnerHeight, spawnRadius);
        spawnerObject.SetActive(false);
    }

    public string DisplayName => spawnerType.ToString();

#pragma warning disable CS1998
    public async Task<GameObject> GetPlaceableObject(int layerMask)
    {
        GameObject instance = Object.Instantiate(spawnerObject);
        instance.SetActive(true);
        instance.name = spawnerType.ToString();
        instance.layer = layerMask;
        return instance;
    }
}
