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

        // Debug summary of world contents before publish
        int itemCount = worldData?.consumableItems != null ? worldData.consumableItems.Count : 0;
        int enemyCount = worldData?.enemies != null ? worldData.enemies.Count : 0;
        logger.Log(
            $"PublishMenuController: About to publish world '{worldData.worldName}' with {itemCount} items and {enemyCount} enemies.",
            this,
            Logging.LogType.Info
        );

        (string worldId, string worldError) = await PublishWorld();
        if (!string.IsNullOrEmpty(worldError) || string.IsNullOrEmpty(worldId))
        {
            logger.Log($"Failed to publish world: {worldError}", this, Logging.LogType.Warning);
            CloseMenu();
            return;
        }

        await PublishItems();
        await PublishEnemies();

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
        // Ensure WorldData is fully up to date (including items/enemies)
        dataPersistenceManager.SaveWorld(worldData.worldName);

        // Refresh local reference in case the manager recreated the WorldData instance
        worldData = dataPersistenceManager.CurrentWorldData;

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

    private async Task PublishItems()
    {
        if (worldData == null || worldData.consumableItems == null || itemsService == null)
            return;

        foreach (var sprite in worldData.consumableItems)
        {
            logger.Log(
                $"Uploading sprite for consumable item '{sprite.name}'",
                this,
                Logging.LogType.Info
            );
            string path = SpriteStorage.GetFilePathFromIdOrPath(sprite.spriteId);
            byte[] spriteBytes = SpriteStorage.LoadSpriteBytesFromPath(path);
            if (spriteBytes == null || spriteBytes.Length == 0)
            {
                logger.Log(
                    $"Sprite bytes for asset ID '{sprite.spriteId}' are null or empty. Skipping upload.",
                    this,
                    Logging.LogType.Warning
                );
                continue;
            }

            string type = Path.GetExtension(path).Replace(".", "").ToLower();
            var (createdSprite, itemError) = await itemsService.UploadItemSpriteAsync(
                spriteBytes,
                $"{sprite.name}{Path.GetExtension(path)}",
                $"image/{type}"
            );

            if (!string.IsNullOrEmpty(itemError) || createdSprite == null)
            {
                logger.Log(
                    $"Failed to upload sprite for item '{sprite.name}': {itemError}",
                    this,
                    Logging.LogType.Warning
                );
                continue;
            }
            logger.Log(
                $"Sprite uploaded successfully for item '{sprite.name}'",
                this,
                Logging.LogType.Info
            );
        }
    }

    private Task<(SpriteCreatedData data, string error)> UploadEnemySpriteAsync(
        byte[] fileBytes,
        string filename,
        string mimeType
    )
    {
        var tcs = new System.Threading.Tasks.TaskCompletionSource<(
            SpriteCreatedData data,
            string error
        )>();

        StartCoroutine(
            enemiesService.UploadEnemySprite(
                fileBytes,
                filename,
                mimeType,
                (data, error) => { tcs.SetResult((data, error)); }
            )
        );

        return tcs.Task;
    }

    private async Task PublishEnemies()
    {
        if (worldData == null || worldData.enemies == null || enemiesService == null)
            return;

        foreach (var enemy in worldData.enemies)
        {
            if (enemy == null || string.IsNullOrEmpty(enemy.spriteId))
                continue;

            logger.Log(
                $"Uploading sprite for enemy '{enemy.name}'",
                this,
                Logging.LogType.Info
            );

            string path = SpriteStorage.GetFilePathFromIdOrPath(enemy.spriteId);
            byte[] spriteBytes = SpriteStorage.LoadSpriteBytesFromPath(path);
            if (spriteBytes == null || spriteBytes.Length == 0)
            {
                logger.Log(
                    $"Sprite bytes for enemy spriteId '{enemy.spriteId}' are null or empty. Skipping upload.",
                    this,
                    Logging.LogType.Warning
                );
                continue;
            }

            string type = Path.GetExtension(path).Replace(".", "").ToLower();
            var (createdSprite, enemyError) = await UploadEnemySpriteAsync(
                spriteBytes,
                $"{enemy.name}{Path.GetExtension(path)}",
                $"image/{type}"
            );

            if (!string.IsNullOrEmpty(enemyError) || createdSprite == null)
            {
                logger.Log(
                    $"Failed to upload sprite for enemy '{enemy.name}': {enemyError}",
                    this,
                    Logging.LogType.Warning
                );
                continue;
            }

            logger.Log(
                $"Sprite uploaded successfully for enemy '{enemy.name}'",
                this,
                Logging.LogType.Info
            );
        }
    }
}
