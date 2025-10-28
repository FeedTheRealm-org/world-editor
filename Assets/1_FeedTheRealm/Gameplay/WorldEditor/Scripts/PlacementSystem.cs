using UnityEngine;


public class PlacementSystem : MonoBehaviour {
    #region Inspector Fields
    [SerializeField]
    private GameObject placementIndicator, cellIndicator;

    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private Material placementGridMaterial;

    private ObjectData selectedObjectData = null;

    private PlacementManager placementManager;

    private bool isRemoving = false;
    #endregion


    #region  Placement Methods
    public void StartPlacement(ObjectData objData) {
        logger.Log($"Started placement of object ID: {objData.Id}", this, Logging.LogType.Info);
        selectedObjectData = objData;
        isRemoving = false; // make sure we’re not in remove mode
        ToggleGridVisualization(true);
        cellIndicator.SetActive(true);

        // Subscribe to input events
        inputManager.OnClicked += PlaceObject;
        inputManager.OnExit += StopPlacement;
    }

    private void StopPlacement() {
        selectedObjectData = null;
        ToggleGridVisualization(false);
        cellIndicator.SetActive(false);

        inputManager.OnClicked -= PlaceObject;
        inputManager.OnExit -= StopPlacement;
    }

    private void PlaceObject() {
        if (selectedObjectData == null) {
            return;
        }
        (bool canBePlaced, GameObject placeableObject) = placementManager.AddPlacedObject(
            grid.WorldToCell(inputManager.GetSelectedMapPosition()),
            selectedObjectData
        );

        if (!canBePlaced) {
            return;
        }
        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(placementPosition);
        Vector3 pos = grid.GetCellCenterWorld(cellPosition);
        placeableObject.transform.position = pos;
    }
    #endregion


    #region Removal Methods

    private void RemoveObjectAt(Vector3Int gridPosition) {
        GameObject removedObject = placementManager.RemovePlacedObject(gridPosition);
        if (removedObject == null) {
            return;
        }
        logger.Log($"Destroyed placed GameObject for: {removedObject.name}", this, Logging.LogType.Info);
        DestroyImmediate(removedObject, true);
    }


    public void StartRemoving() {
        isRemoving = true;
        selectedObjectData = null;
        ToggleGridVisualization(true);
        cellIndicator.SetActive(true);
        inputManager.OnClicked += HandleRemoveClick;
        inputManager.OnExit += StopRemoving;
    }

    private void StopRemoving() {
        isRemoving = false;
        ToggleGridVisualization(false);
        cellIndicator.SetActive(false);

        inputManager.OnClicked -= HandleRemoveClick;
        inputManager.OnExit -= StopRemoving;
    }

    private void HandleRemoveClick() {
        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(placementPosition);
        RemoveObjectAt(cellPosition);
    }

    #endregion


    #region Start & Update
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
    #endregion



    private void ToggleGridVisualization(bool isVisible) {
        placementGridMaterial.SetFloat("_Show", isVisible ? 1f : 0f);
    }

}

