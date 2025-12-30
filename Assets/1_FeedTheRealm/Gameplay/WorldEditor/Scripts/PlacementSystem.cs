using Models;
using UnityEngine;
using World;
using System.Collections.Generic;
using Enums;


public class PlacementSystem : MonoBehaviour, IDataPersistence {
    #region Inspector Fields
    [Header("Indicator settings")]
    [SerializeField]
    private GameObject cellIndicator;

    [Header("Enemy Spawn Settings")]
    [SerializeField]
    private GameObject enemySpawnIndicator;
    [SerializeField]
    private GameObject enemySpawnPlacePrefab;

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
    private bool isPlacingEnemySpawn = false;
    #endregion

    #region  Placement Methods
    public void StartPlacement(Asset objData) {
        logger.Log($"Started placement of object ID: {objData.Id}", this, Logging.LogType.Info);
        selectedObjectData = objData;
        isRemoving = false;
        isPlacingEnemySpawn = false;
        worldController.ToggleGridVisualization(true);
        // Default indicator for general placement
        if (cellIndicator != null) cellIndicator.SetActive(true);
        if (enemySpawnIndicator != null) enemySpawnIndicator.SetActive(false);

        // Subscribe to input events
        inputManager.OnClicked += PlaceObject;
        inputManager.OnExit += StopPlacement;
    }


    private void StopPlacement() {
        selectedObjectData = null;
        worldController.ToggleGridVisualization(false);
        if (cellIndicator != null) cellIndicator.SetActive(false);
        if (enemySpawnIndicator != null) enemySpawnIndicator.SetActive(false);
        isPlacingEnemySpawn = false;
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
        isPlacingEnemySpawn = false;
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
        if (selectedObjectData == null && !isRemoving && !isPlacingEnemySpawn) {
            return;
        }

        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = worldController.GetSelectedPosition(placementPosition);

        cellIndicator.transform.position = worldController.GetCellCenterPosition(cellPosition);
        enemySpawnIndicator.transform.position = worldController.GetCellCenterPosition(cellPosition);
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

        foreach (EnemySpawnAreaData enemySpawnAreaData in data.enemySpawnAreas) {

            Vector3Int gridPosition = Vector3Int.FloorToInt(enemySpawnAreaData.Position);
            Asset enemySpawnAsset = assetLibrary.GetSpawnerByName(SpawnerTypes.EnemySpawn);
            bool canBePlaced = TryPlaceObjectAt(enemySpawnAsset, gridPosition);
            if (!canBePlaced) {
                logger.Log("Failed to load placed Enemy Spawn Point", this, Logging.LogType.Error);
            }
        }

        foreach (PlayerSpawnAreaData playerSpawnAreaData in data.playerSpawnAreas) {

            Vector3Int gridPosition = Vector3Int.FloorToInt(playerSpawnAreaData.Position);
            Asset playerSpawnAsset = assetLibrary.GetSpawnerByName(SpawnerTypes.PlayerSpawn);
            bool canBePlaced = TryPlaceObjectAt(playerSpawnAsset, gridPosition);
            if (!canBePlaced) {
                logger.Log("Failed to load placed Player Spawn Point", this, Logging.LogType.Error);
            }
        }

        logger.Log($"Loaded {data.objectPlacementData.Count} placed objects.", this, Logging.LogType.Info);
        logger.Log($"Loaded {data.enemySpawnAreas.Count} enemy spawn areas.", this, Logging.LogType.Info);
    }

    public void SaveData(ref WorldData data) {
        data.objectPlacementData = new List<PlacedAsset>();
        data.enemySpawnAreas = new List<EnemySpawnAreaData>();
        data.playerSpawnAreas = new List<PlayerSpawnAreaData>();


        foreach (PlacedAsset placementData in placementManager.GetAllPlacedObjects()) {
            switch (placementData.AssetDataId) {
                case SpawnerTypes.EnemySpawn:
                    EnemySpawnAreaData enemySpawnDataFromSpawner = placementData.InstancedGameObject.GetComponent<EnemySpawnPlace>().GetData();
                    data.enemySpawnAreas.Add(new EnemySpawnAreaData(
                            placementData.Position,
                            placementData.InstancedGameObject.GetComponent<SpawnerController>().GetSizeAsInt(),
                            enemySpawnDataFromSpawner.MaxEnemies,
                            enemySpawnDataFromSpawner.SpawnRate,
                            enemySpawnDataFromSpawner.ResetAfterKills,
                            enemySpawnDataFromSpawner.ResetDelay
                        )
                    );
                    break;
                case SpawnerTypes.PlayerSpawn:
                    data.playerSpawnAreas.Add(new PlayerSpawnAreaData(placementData.Position, placementData.InstancedGameObject.GetComponent<SpawnerController>().GetSizeAsInt()));
                    break;
                default:
                    data.objectPlacementData.Add(placementData);
                    break;
            }
        }

        logger.Log($"Saved {data.objectPlacementData.Count} placed objects.", this, Logging.LogType.Info);
        logger.Log($"Saved {data.enemySpawnAreas.Count} enemy spawn areas.", this, Logging.LogType.Info);
    }

    #endregion
}