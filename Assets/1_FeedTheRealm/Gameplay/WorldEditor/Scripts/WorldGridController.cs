using UnityEngine;

[ExecuteAlways] // Runs in edit mode too
public class WorldController : MonoBehaviour {
    [SerializeField]
    private GameObject worldPlane;

    [SerializeField]
    private Grid grid;

    [SerializeField, Min(0.1f)]
    private float cellSize = 1f;

    [SerializeField, Min(1)]
    private int gridSize = 10;

    private void OnValidate() {
        UpdateWorld();
    }

    private void UpdateWorld() {
        if (grid == null || worldPlane == null)
            return;

        // Set the cell size for the Unity Grid
        grid.cellSize = new Vector3(cellSize, 0, cellSize);

        // Scale the world plane to match the grid
        // Default Unity plane is 10x10 units, so scale accordingly
        float scale = gridSize * cellSize / 10f;
        worldPlane.transform.localScale = new Vector3(scale, 1, scale);

        // Optional: position the world plane so its center aligns with the grid center
        worldPlane.transform.position = new Vector3(
            (gridSize * cellSize) / 2f - cellSize / 2f,
            worldPlane.transform.position.y,
            gridSize * cellSize / 2f - cellSize / 2f
        );
    }
}
