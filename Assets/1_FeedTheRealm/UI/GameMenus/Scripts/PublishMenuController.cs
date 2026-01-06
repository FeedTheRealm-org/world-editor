using System.Collections;
using System.IO;
using System.Threading.Tasks;
using API;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PublishMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private DataPersistenceManagerSO dataPersistenceManager;

    [SerializeField]
    private StructureLoaderSO structureLoader;

    [SerializeField]
    private WorldService worldService;

    [SerializeField]
    private ModelService modelUploadService;

    [SerializeField]
    private ItemSpritesService itemsService;

    [SerializeField]
    private EnemiesService enemiesService;

    [SerializeField]
    private Session.Session session;

    // -------- Ui related elements --------
    private Button publishButton;
    private Button closeButton;
    private TextField nameInput;
    private TextField descriptionInput;
    private VisualElement root;
    private WorldData worldData;
    private string fileName;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        publishButton = root.Q<Button>("Publish");
        closeButton = root.Q<Button>("Close");
        nameInput = root.Q<TextField>("NameInput");
        descriptionInput = root.Q<TextField>("DescriptionInput");

        worldData = dataPersistenceManager.CurrentWorldData;

        if (worldData != null && !string.IsNullOrEmpty(worldData.worldName))
        {
            nameInput.value = worldData.worldName;
        }

        publishButton.clicked += OnPublishClicked;
        closeButton.clicked += CloseMenu;
    }

    private void OnDisable()
    {
        publishButton.clicked -= OnPublishClicked;
        closeButton.clicked -= CloseMenu;
    }

    private async void OnPublishClicked()
    {
        logger.Log("PublishMenuController: Publishing world.", this, Logging.LogType.Info);
        await PublishAll();
    }

    private async Task PublishAll()
    {
        if (!ValidatePublish() || !ValidateModels())
        {
            logger.Log(
                "PublishMenuController: Cannot publish world, validation failed: ",
                this,
                Logging.LogType.Warning
            );
            return;
        }

        // 1) Upload item sprites first and update spriteIds with backend IDs.
        await WorldPublishSpriteHelpers.UploadItemSpritesAndUpdateIds(
            worldData,
            itemsService,
            logger,
            this
        );

        // Debug summary of world contents before publish (after spriteId normalization)
        int itemCount = worldData?.consumableItems != null ? worldData.consumableItems.Count : 0;
        int enemyCount = worldData?.enemies != null ? worldData.enemies.Count : 0;
        logger.Log(
            $"PublishMenuController: About to publish world '{worldData.worldName}' with {itemCount} items and {enemyCount} enemies.",
            this,
            Logging.LogType.Info
        );

        // 2) Save world (including new spriteIds) and publish it.
        (string worldId, string worldError) = await PublishWorld();
        if (!string.IsNullOrEmpty(worldError) || string.IsNullOrEmpty(worldId))
        {
            logger.Log($"Failed to publish world: {worldError}", this, Logging.LogType.Warning);
            CloseMenu();
            return;
        }

        // After world metadata is stored, upload enemy sprites (optional, best-effort).
        await WorldPublishSpriteHelpers.PublishEnemies(worldData, enemiesService, logger, this);

        string uploadError = await PublishModels(worldId);
        if (!string.IsNullOrEmpty(uploadError))
        {
            logger.Log($"Failed to publish models: {uploadError}", this, Logging.LogType.Warning);
            CloseMenu();
            return;
        }
        logger.Log("World published successfully!", this, Logging.LogType.Info);

        // Save worldId in memory; avoid an extra SaveWorld() here to
        // prevent re-running ClearWorld/SaveData and confusing item/enemy databases.
        worldData.id = worldId;

        CloseMenu();
    }

    private bool ValidatePublish()
    {
        if (string.IsNullOrEmpty(worldData?.worldName))
        {
            logger.Log("World name cannot be empty.", this, Logging.LogType.Warning);
            return false;
        }
        if (string.IsNullOrEmpty(session?.APIToken))
        {
            logger.Log("Session token is missing.", this, Logging.LogType.Warning);
            return false;
        }
        return true;
    }

    private bool ValidateModels()
    {
        if (worldData.objectPlacementData == null || worldData.objectPlacementData.Count == 0)
        {
            logger.Log(
                "World has no objects placed. Cannot publish an empty world.",
                this,
                Logging.LogType.Warning
            );
            return false;
        }
        foreach (var structure in worldData.objectPlacementData)
        {
            if (!structureLoader.IsModelPresent(structure.structureName))
            {
                logger.Log(
                    $"Model '{structure.structureName}' is missing. Cannot publish world.",
                    this,
                    Logging.LogType.Warning
                );
                return false;
            }
        }
        return true;
    }

    private async Task<(string, string)> PublishWorld()
    {
        fileName = dataPersistenceManager.GetWorldFile(worldData.worldName);
        (string worldId, string worldError) = await worldService.PublishWorld(
            worldData,
            fileName,
            descriptionInput.value,
            session.APIToken
        );
        return (worldId, worldError);
    }

    private async Task<string> PublishModels(string worldId)
    {
        foreach (var structure in worldData.objectPlacementData)
        {
            structure.structureFilepath = structureLoader.GetModelFilePath(structure.structureName);
        }
        return await modelUploadService.UploadModels(
            worldData.objectPlacementData,
            worldId,
            session.APIToken
        );
    }
}
