using Models;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SpawnerController : MonoBehaviour, IPersistent
{
    [SerializeField]
    private Color spawnerColor = Color.white;

    void Awake()
    {
        ApplyColor();
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;
        ApplyColor();
    }

    private void ApplyColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = spawnerColor;
        }
    }

    public void SaveData(ref WorldData worldData)
    {
        EnemySpawnAreaData spawnAreaData = new(transform.position, transform.localScale.x);
        worldData.enemySpawnAreas.Add(spawnAreaData);
    }
}
