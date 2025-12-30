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
    private DataPersistenceManagerSO dataPersistenceManager;

    [SerializeField]
    private WorldService worldService;

    [SerializeField]
    private ModelService modelUploadService;

    [SerializeField]
    private ItemsService itemsService;

    [SerializeField]
    private Session.Session session;
    private WorldData worldData;

    private Button publishButton;
    private Button closeButton;
    private TextField nameInput;
    private TextField descriptionInput;
    private VisualElement root;
    private string Token => session != null ? session.APIToken : "";

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        //worldData = dataPersistenceManager.CurrentWorldData;

        publishButton = root.Q<Button>("Publish");
        closeButton = root.Q<Button>("Close");
        nameInput = root.Q<TextField>("NameInput");
        descriptionInput = root.Q<TextField>("DescriptionInput");

        if (worldData != null && !string.IsNullOrEmpty(worldData.worldName))
        {
            nameInput.value = worldData.worldName;
        }

        publishButton.clicked += OnPublishClicked;
        closeButton.clicked += OnCloseClicked;
    }

    private void OnDisable()
    {
        publishButton.clicked -= OnPublishClicked;
        closeButton.clicked -= OnCloseClicked;
    }

    private void OnPublishClicked()
    {
        Debug.Log("PublishMenuController: Publishing world.");
        _ = PublishAll();
    }

    private async Task PublishAll()
    {
        //Validate input
        WorldData worldData = dataPersistenceManager.CurrentWorldData;
        string filePath = dataPersistenceManager.GetCurrentWorldFilePath();

        if (string.IsNullOrEmpty(worldData?.worldName))
        {
            Debug.LogError("World name cannot be empty.");
            CloseMenu();
            return;
        }
        if (worldData.objectPlacementData == null || worldData.objectPlacementData.Count == 0)
        {
            Debug.LogError("World has no objects placed. Cannot publish an empty world.");
            CloseMenu();
            return;
        }
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning("World file path is empty. Saving world before publishing.");
            dataPersistenceManager.SaveWorld(worldData.worldName);
            CloseMenu();
            return;
        }
        if (string.IsNullOrEmpty(Token))
        {
            Debug.LogError("Session token is missing.");
            CloseMenu();
            return;
        }

        //Upload world and wait
        string worldId = null;
        string worldError = null;

        await worldService.PublishWorld(
            worldData,
            Token,
            (id, error) =>
            {
                worldId = id;
                worldError = error;
            }
        );

        foreach (var sprite in worldData.consumableItems)
        {
            Debug.Log($"Uploading sprite for consumable item '{sprite.name}'");
            string path = SpriteStorage.GetFilePathFromIdOrPath(sprite.spriteId);
            byte[] spriteBytes = SpriteStorage.LoadSpriteBytesFromPath(path);
            if (spriteBytes == null || spriteBytes.Length == 0)
            {
                Debug.LogWarning(
                    $"Sprite bytes for asset ID '{sprite.spriteId}' are null or empty. Skipping upload."
                );
                continue;
            }

            string itemError = null;
            SpriteCreatedData createdSprite = null;

            Debug.Log($"Sprite name: {path}");
            Debug.Log($"Sprite path: {spriteBytes.Length}");
            Debug.Log($"Uploading sprite from path: {Path.GetExtension(path)}");

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
                Debug.LogError($"Failed to upload sprite for item '{sprite.name}': {itemError}");
                continue;
            }

            Debug.Log($"Sprite uploaded successfully for item '{sprite.name}'");
        }

        if (!string.IsNullOrEmpty(worldError) || string.IsNullOrEmpty(worldId))
        {
            Debug.LogError($"Failed to publish world: {worldError}");
            CloseMenu();
            return;
        }

        Debug.Log($"World published successfully! ID: {worldId}");

        //Upload assets and wait
        // var assets = null //= assetLibrary.GetAssetsFromWorld(worldData);
        // if (assets == null || assets.Count == 0)
        // {
        //     Debug.LogWarning("No assets found to upload for this world.");
        //     CloseMenu();
        //     return;
        // }

        string uploadError = null;

        // await modelUploadService.UploadAssets(
        //     assets,
        //     worldId,
        //     Token,
        //     (error) =>
        //     {
        //         uploadError = error;
        //     }
        // );

        if (!string.IsNullOrEmpty(uploadError))
        {
            Debug.LogError($"Failed to upload assets: {uploadError}");
        }
        else
        {
            Debug.Log("All assets uploaded successfully!");
        }

        Debug.Log("Publishing complete. Closing menu.");
        CloseMenu();
        CloseMenu();
    }

    private void OnCloseClicked()
    {
        Debug.Log("PublishMenuController: Closing publish menu.");
        CloseMenu();
    }
}
