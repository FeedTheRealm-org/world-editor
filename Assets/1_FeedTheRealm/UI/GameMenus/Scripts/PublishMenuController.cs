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
    private ItemsService itemsService;

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
        (string worldId, string worldError) = await PublishWorld();
        if (!string.IsNullOrEmpty(worldError) || string.IsNullOrEmpty(worldId))
        {
            logger.Log($"Failed to publish world: {worldError}", this, Logging.LogType.Warning);
            CloseMenu();
            return;
        }
        string uploadError = await PublishModels(worldId);
        if (!string.IsNullOrEmpty(uploadError))
        {
            logger.Log($"Failed to publish models: {uploadError}", this, Logging.LogType.Warning);
            CloseMenu();
            return;
        }
        logger.Log("World published successfully!", this, Logging.LogType.Info);

        // we save the world id to the local world data for future reference
        worldData.id = worldId;
        dataPersistenceManager.SaveWorld(worldData.worldName);

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
        dataPersistenceManager.SaveWorld(worldData.worldName);
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

    // TODO: re-implement item publishing
    private async Task PublishItems()
    {
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

            string itemError = null;
            SpriteCreatedData createdSprite = null;

            string type = Path.GetExtension(path).Replace(".", "").ToLower();

            await itemsService.UploadItemSprite(
                spriteBytes,
                $"{sprite.name}{Path.GetExtension(path)}",
                $"image/{type}",
                (data, error) =>
                {
                    createdSprite = data;
                    itemError = error;
                }
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
}
