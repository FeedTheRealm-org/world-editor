using Models;
using UnityEngine;

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
        if (renderer != null)
        {
            if (renderer.material != null)
            {
                renderer.material.color = spawnerColor;
            }
        }
    }

    public virtual void SaveData(ref WorldData worldData)
    {
        throw new System.NotImplementedException();
    }
}
