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

    private ObjectData selectedObjectData = null;

    private PlacementManager placementManager;

    [SerializeField]
    private GameObject gridVisualization;

    private bool isRemoving = false; // 🆕 tracks remove mode
    #endregion

    #region Methods
    public void StartPlacement(ObjectData objData) {
        logger.Log($"Started placement of object ID: {objData.Id}", this, Logging.LogType.Info);
        selectedObjectData = objData;
        isRemoving = false; // make sure we’re not in remove mode
        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);

        // Subscribe to input events
        inputManager.OnClicked += PlaceObject;
        inputManager.OnExit += StopPlacement;
    }

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
        isRemoving = false;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);

        inputManager.OnClicked -= HandleRemoveClick;
        inputManager.OnExit -= StopRemoving;
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
        placeableObject.transform.position = grid.GetCellCenterWorld(cellPosition);
    }

    private void StopPlacement() {
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
