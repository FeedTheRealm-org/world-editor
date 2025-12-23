using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SpawnerController : MonoBehaviour
{
    [SerializeField]
    private Material spawnerMaterialTemplate;

    [SerializeField]
    private Color spawnerColor = Color.white;

    [SerializeField]
    private string spawnerType = "default";
    private float spawnerHeight = 0.2f;

    void Awake()
    {
        SetupMaterial();
        ApplyColor();
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;
        SetupMaterial();
        ApplyColor();
    }

    private void SetupMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && spawnerMaterialTemplate != null)
        {
            renderer.material = new Material(spawnerMaterialTemplate);
        }
    }

    private void ApplyColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = spawnerColor;
        }
    }

    public void SetColor(Color color)
    {
        spawnerColor = color;
        ApplyColor();
    }

    public void SetSize(int size)
    {
        float spawnerSize = Mathf.Max(0.1f, size);
        transform.localScale = new Vector3(spawnerSize, spawnerHeight, spawnerSize);
    }

    public string GetSpawnerType()
    {
        return spawnerType;
    }

    public Vector3 GetSize()
    {
        return transform.localScale;
    }

    public int GetSizeAsInt()
    {
        return Mathf.RoundToInt(transform.localScale.x);
    }
}
