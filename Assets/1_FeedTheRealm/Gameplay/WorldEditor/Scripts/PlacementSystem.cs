using System;
using UnityEngine;

public class PlacementSystem : MonoBehaviour {
    # region Inspector Fields
    [SerializeField]
    private GameObject placementIndicator, cellIndicator;

    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private AssetDatabaseSO assetDatabase;
    private int selectedObjectIndex = -1;

    [SerializeField]
    private GameObject gridVisualization;
    #endregion

    #region Methods
    public void StartPlacement(int ID) {
        Debug.Log("Started placement of object ID: " + ID);
        selectedObjectIndex = assetDatabase.objectData.FindIndex(obj => obj.Id == ID);
        if (selectedObjectIndex < 0) {
            Debug.LogWarning("Object with ID " + ID + " not found in Asset Database.");
        }
        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);
        // Subscribe to input events
        inputManager.OnClicked += PlaceObject;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceObject() {
        Debug.Log("Placing object...");
        if (selectedObjectIndex < 0) {
            return;
        }

        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(placementPosition);
        GameObject placeableObject = Instantiate(assetDatabase.objectData[selectedObjectIndex].Prefab);
        placeableObject.transform.position = grid.GetCellCenterWorld(cellPosition);
    }

    private void StopPlacement() {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
        // Unsubscribe from input events
        inputManager.OnClicked -= PlaceObject;
        inputManager.OnExit -= StopPlacement;
    }
    #endregion

    void Start() {
        StopPlacement();
    }

    void Update() {

        if (selectedObjectIndex < 0) {
            return;
        }

        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(placementPosition);
        placementIndicator.transform.position = placementPosition;
        cellIndicator.transform.position = grid.GetCellCenterWorld(cellPosition);
    }
}
