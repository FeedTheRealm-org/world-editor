using UnityEngine;


public class PlacementSystem : MonoBehaviour, IDataPersistence {
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

    [SerializeField]
    private AssetDatabaseSO assetDatabase;

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
        bool canBePlaced = PlaceObjectAt(
            grid.WorldToCell(inputManager.GetSelectedMapPosition()),
            selectedObjectData
        );

        if (!canBePlaced) {
            logger.Log($"Placement failed for object ID: {selectedObjectData.Id}", this, Logging.LogType.Warning);
            return;
        }
    }


    private bool PlaceObjectAt(Vector3Int gridPosition, ObjectData objectData) {
        (bool canBePlaced, PlacementData placeableObject) = placementManager.CheckAndPlaceObject(
            gridPosition,
            objectData
        );

        if (!canBePlaced) {
            return false;
        }
        Vector3Int cellPosition = grid.WorldToCell(placeableObject.GridPosition);
        Vector3 pos = grid.GetCellCenterWorld(cellPosition);

        GameObject gameObject = placeableObject.gameObject;
        gameObject.transform.position = pos;
        return true;
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


    #region Initialization & Update
    void Awake() {
        placementManager = new PlacementManager();
    }

    void Start() {
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

    #region Data Persistence Implementation

    public void LoadData(WorldData data) {

        foreach (PlacementData placementData in data.objectPlacementData) {

            ObjectData objectData = assetDatabase.GetObjectDataById(placementData.ObjectDataRef.Id);
            placementData.ObjectDataRef = objectData;

            bool canBePlaced = PlaceObjectAt(placementData.GridPosition, placementData.ObjectDataRef);
            if (!canBePlaced) {
                logger.Log($"Failed to load placed object at {placementData.GridPosition}", this, Logging.LogType.Error);
            }
        }
        logger.Log($"Loaded {data.objectPlacementData.Count} placed objects.", this, Logging.LogType.Info);
    }

    public void SaveData(ref WorldData data) {
        // Save the world data
        // TODO: refactor this to only save changes, not the entire list every time
        // Since this is going to be a big project, we want to optimize data saving/loading
        data.objectPlacementData = placementManager.GetAllPlacedObjects();
        logger.Log($"Saved {data.objectPlacementData.Count} placed objects.", this, Logging.LogType.Info);
    }

    #endregion
}

