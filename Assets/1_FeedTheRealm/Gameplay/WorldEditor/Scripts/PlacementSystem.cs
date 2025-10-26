using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementSystem : MonoBehaviour {
    #region Inspector Fields
    [SerializeField]
    private GameObject placementIndicator, cellIndicator;

    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Grid grid;

    private ObjectData selectedObjectData = null;

    private PlacementManager placementManager;

    [SerializeField]
    private GameObject gridVisualization;

    private bool isRemoving = false; // 🆕 tracks remove mode
    #endregion

    #region Methods
    public void StartPlacement(ObjectData objData) {
        Debug.Log("Started placement of object ID: " + objData.Id);
        selectedObjectData = objData;
        isRemoving = false; // make sure we’re not in remove mode
        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);

        // Subscribe to input events
        inputManager.OnClicked += PlaceObject;
        inputManager.OnExit += StopPlacement;
    }

    private void RemoveObjectAt(Vector3Int gridPosition) {
        Debug.Log($"Trying to remove object at {gridPosition}");
        GameObject removedObject = placementManager.RemovePlacedObject(gridPosition);

        if (removedObject == null) {
            Debug.LogWarning($"No object found at {gridPosition} to remove.");
            return;
        }

        Debug.Log($"Destroyed placed GameObject for: {removedObject.name}");
        DestroyImmediate(removedObject, true);
    }


    public void StartRemoving() {
        Debug.Log("Started removing mode.");
        isRemoving = true;
        selectedObjectData = null;
        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);

        inputManager.OnClicked += HandleRemoveClick;
        inputManager.OnExit += StopRemoving;
    }

    private void HandleRemoveClick() {
        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(placementPosition);
        RemoveObjectAt(cellPosition);
    }

    private void StopRemoving() {
        Debug.Log("Stopped removing mode.");
        isRemoving = false;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);

        inputManager.OnClicked -= HandleRemoveClick;
        inputManager.OnExit -= StopRemoving;
    }

    private void PlaceObject() {
        Debug.Log("Placing object...");
        if (selectedObjectData == null) {
            return;
        }

        (bool canBePlaced, GameObject placeableObject) = placementManager.AddPlacedObject(
            grid.WorldToCell(inputManager.GetSelectedMapPosition()),
            selectedObjectData
        );

        if (!canBePlaced) {
            Debug.LogWarning("Cannot place object here, cell is occupied.");
            return;
        }
        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(placementPosition);
        placeableObject.transform.position = grid.GetCellCenterWorld(cellPosition);
        Debug.Log($"Placed object: {selectedObjectData.Name} at {cellPosition} with UniqueID: {selectedObjectData.UniqueID}");
    }

    private void StopPlacement() {
        Debug.Log("Stopped placement mode.");
        selectedObjectData = null;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);

        inputManager.OnClicked -= PlaceObject;
        inputManager.OnExit -= StopPlacement;
    }
    #endregion

    void Start() {
        placementManager = new PlacementManager();
        StopPlacement();
    }

    void Update() {
        if (selectedObjectData == null && !isRemoving) {
            return;
        }

        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(placementPosition);

        placementIndicator.transform.position = placementPosition;
        cellIndicator.transform.position = grid.GetCellCenterWorld(cellPosition);
    }
}
