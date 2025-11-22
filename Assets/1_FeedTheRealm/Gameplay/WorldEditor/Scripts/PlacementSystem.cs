using Models;
using UnityEngine;
using World;


public class PlacementSystem : MonoBehaviour, IDataPersistence {
    #region Inspector Fields
    [Header("Indicator settings")]
    [SerializeField]
    private GameObject placementIndicator;
    [SerializeField]
    private GameObject cellIndicator;

    [Header("Dependencies")]
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Logging.Logger logger;
    [SerializeField]
    private AssetLibrarySO assetLibrary;
    [SerializeField]
    private DataPersistenceManagerSO dataPersistenceManager;
    [SerializeField]
    private WorldController worldController;

    private Asset selectedObjectData = null;
    private PlacementManager placementManager;

    private bool isRemoving = false;
    #endregion

    #region  Placement Methods
    public void StartPlacement(Asset objData) {
        logger.Log($"Started placement of object ID: {objData.Id}", this, Logging.LogType.Info);
        selectedObjectData = objData;
        isRemoving = false;
        worldController.ToggleGridVisualization(true);
        cellIndicator.SetActive(true);

        // Subscribe to input events
        inputManager.OnClicked += PlaceObject;
        inputManager.OnExit += StopPlacement;
    }

    private void StopPlacement() {
        selectedObjectData = null;
        worldController.ToggleGridVisualization(false);
        cellIndicator.SetActive(false);

        inputManager.OnClicked -= PlaceObject;
        inputManager.OnExit -= StopPlacement;
    }
    private void PlaceObject() {
        if (selectedObjectData == null) {
            return;
        }
        Vector3Int gridPosition = worldController.GetSelectedPosition(inputManager.GetSelectedMapPosition());

        bool canBePlaced = TryPlaceObjectAt(
            selectedObjectData,
            gridPosition
        );

        if (!canBePlaced) {
            logger.Log($"Placement failed for object ID: {selectedObjectData.Id}", this, Logging.LogType.Warning);
            return;
        }
    }


    private bool TryPlaceObjectAt(Asset objectData, Vector3Int gridPosition) {
        PlacedAsset placeableObject = placementManager.TryPlaceObject(objectData, gridPosition);

        if (placeableObject == null) {
            return false;
        }
        // Vector3Int cellPosition = grid.WorldToCell(gridPosition);
        // Vector3 pos = grid.GetCellCenterWorld(cellPosition);
        // placeableObject.InstancedGameObject.transform.position = pos;
        worldController.PlaceObjectAt(gridPosition, placeableObject.InstancedGameObject);
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
        worldController.ToggleGridVisualization(true);
        cellIndicator.SetActive(true);
        inputManager.OnClicked += HandleRemoveClick;
        inputManager.OnExit += StopRemoving;
    }

    private void StopRemoving() {
        isRemoving = false;
        worldController.ToggleGridVisualization(false);
        cellIndicator.SetActive(false);

        inputManager.OnClicked -= HandleRemoveClick;
        inputManager.OnExit -= StopRemoving;
    }

    private void HandleRemoveClick() {
        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = worldController.GetSelectedPosition(placementPosition);
        RemoveObjectAt(cellPosition);
    }

    #endregion


    #region Initialization & Update
    void Awake() {
        placementManager = new PlacementManager();
        dataPersistenceManager.LoadWorld();
    }

    void Start() {
        StopPlacement();
    }

    void Update() {
        if (selectedObjectData == null && !isRemoving) {
            return;
        }

        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = worldController.GetSelectedPosition(placementPosition);

        placementIndicator.transform.position = placementPosition;
        cellIndicator.transform.position = worldController.GetCellCenterPosition(cellPosition);
    }
    #endregion

    #region Data Persistence Implementation
    public void LoadData(WorldData data) {

        if (data.objectPlacementData == null || data.objectPlacementData.Count == 0) {
            logger.Log("New world created!", this, Logging.LogType.Info);
            return;
        }

        foreach (PlacedAsset placementData in data.objectPlacementData) {
            Asset assetData = assetLibrary.GetAssetById(placementData.AssetDataId);
            Vector3Int gridPosition = placementData.Position;
            bool canBePlaced = TryPlaceObjectAt(assetData, gridPosition);
            if (!canBePlaced) {
                logger.Log($"Failed to load placed object ID: {assetData.Id}", this, Logging.LogType.Error);
            }
        }
        logger.Log($"Loaded {data.objectPlacementData.Count} placed objects.", this, Logging.LogType.Info);
    }

    public void SaveData(ref WorldData data) {
        data.objectPlacementData = placementManager.GetAllPlacedObjects();
        logger.Log($"Saved {data.objectPlacementData.Count} placed objects.", this, Logging.LogType.Info);
    }

    #endregion
}