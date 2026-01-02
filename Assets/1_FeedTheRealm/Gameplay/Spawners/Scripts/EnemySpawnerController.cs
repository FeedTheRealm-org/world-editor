using Models;
using UnityEngine;

public class EnemySpawnerController : SpawnerController, IPersistent, ISelectable
{
    [SerializeField]
    public MakerInputReader inputReader;

    public void OnObjectSelected()
    {
        Debug.Log("I have been clicked:" + gameObject.name);
    }

    public override void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        EnemySpawnAreaData spawnAreaData = new(transform.position, transform.localScale.x);
        worldData.enemySpawnAreas.Add(spawnAreaData);
    }
}
