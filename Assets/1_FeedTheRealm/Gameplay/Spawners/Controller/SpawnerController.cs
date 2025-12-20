using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SpawnerController : MonoBehaviour {
    [SerializeField] private Color spawnerColor = Color.green;
    [SerializeField] private float spawnerSize = 1.0f;

    void Start() {
        ApplyColor();
        ApplySize();
    }

    private void OnValidate() {
        if (!isActiveAndEnabled) return;
        ApplyColor();
        ApplySize();
    }

    public void SetSpawnerColor(Color color) {
        spawnerColor = color;
        ApplyColor();
    }

    public void SetSpawnerSize(float size) {
        spawnerSize = size;
        ApplySize();
    }

    private void ApplyColor() {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null) {
            renderer.sharedMaterial.color = spawnerColor;
        }
    }

    private void ApplySize() {
        transform.localScale = new Vector3(spawnerSize, 0.1f, spawnerSize);
    }
}
