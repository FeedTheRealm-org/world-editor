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
    private float defaultSpawnRadius = 3.0f;
    private GameObject spawnerObject;
    private float spawnerHeight = 0.1f;

    public SpawnerObject(SpawnerType spawnerType, GameObject spawnerObject)
    {
        this.spawnerType = spawnerType;
        this.spawnerObject = spawnerObject;
        spawnerObject.transform.localScale = new Vector3(
            defaultSpawnRadius,
            spawnerHeight,
            defaultSpawnRadius
        );
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
