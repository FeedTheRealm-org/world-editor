using System.Collections;
using API;
using Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PublishMenuController : MonoBehaviour {
    [SerializeField] private DataPersistenceManagerSO dataPersistenceManager;
    [SerializeField] private AssetLibrarySO assetLibrary;
    [SerializeField] private WorldService worldService;
    [SerializeField] private ModelService modelUploadService;
    [SerializeField] private Maker player;
    [SerializeField] private Session.Session session;
    private Models.WorldData worldData;

    private Button publishButton;
    private Button closeButton;
    private TextField nameInput;
    private TextField descriptionInput;
    private VisualElement root;
    private string Token => session != null ? session.APIToken : "";

    private void OnEnable() {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        worldData = dataPersistenceManager.CurrentWorldData;

        if (player != null) {
            player.ToggleMovement(false);
        }

        publishButton = root.Q<Button>("Publish");
        closeButton = root.Q<Button>("Close");
        nameInput = root.Q<TextField>("NameInput");
        descriptionInput = root.Q<TextField>("DescriptionInput");

        if (worldData != null && !string.IsNullOrEmpty(worldData.worldName)) {
            nameInput.value = worldData.worldName;
        }

        if (publishButton != null)
            publishButton.clicked += OnPublishClicked;

        if (closeButton != null)
            closeButton.clicked += OnCloseClicked;
    }

    private void OnDisable() {
        if (publishButton != null)
            publishButton.clicked -= OnPublishClicked;

        if (closeButton != null)
            closeButton.clicked -= OnCloseClicked;
    }
    private void OnPublishClicked() {
        Debug.Log("PublishMenuController: Publishing world.");
        StartCoroutine(PublishAll());
    }

    private IEnumerator PublishAll() {
        //Validate input
        WorldData worldData = dataPersistenceManager.CurrentWorldData;
        string filePath = dataPersistenceManager.GetCurrentWorldFilePath();

        if (string.IsNullOrEmpty(worldData?.worldName)) {
            Debug.LogError("World name cannot be empty.");
            CloseMenu();
            yield break;
        }
        if (worldData.objectPlacementData == null || worldData.objectPlacementData.Count == 0) {
            Debug.LogError("World has no objects placed. Cannot publish an empty world.");
            CloseMenu();
            yield break;
        }
        if (string.IsNullOrEmpty(filePath)) {
            Debug.LogWarning("World file path is empty. Saving world before publishing.");
            dataPersistenceManager.SaveWorld(worldData.worldName);
            CloseMenu();
            yield break;
        }
        if (string.IsNullOrEmpty(Token)) {
            Debug.LogError("Session token is missing.");
            CloseMenu();
            yield break;
        }

        //Upload world and wait
        string worldId = null;
        string worldError = null;

        yield return StartCoroutine(
            worldService.CreateWorld(worldData, descriptionInput.value, Token,
                (id, error) => {
                    worldId = id;
                    worldError = error;
                })
        );

        if (!string.IsNullOrEmpty(worldError) || string.IsNullOrEmpty(worldId)) {
            Debug.LogError($"Failed to publish world: {worldError}");
            CloseMenu();
            yield break;
        }

        Debug.Log($"World published successfully! ID: {worldId}");

        //Upload assets and wait
        var assets = assetLibrary.GetAssetsFromWorld(worldData);
        if (assets == null || assets.Count == 0) {
            Debug.LogWarning("No assets found to upload for this world.");
            CloseMenu();
            yield break;
        }

        string uploadError = null;

        yield return StartCoroutine(
            modelUploadService.UploadAssets(assets, worldId, Token,
                (error) => {
                    uploadError = error;
                })
        );

        if (!string.IsNullOrEmpty(uploadError)) {
            Debug.LogError($"Failed to upload assets: {uploadError}");
        } else {
            Debug.Log("All assets uploaded successfully!");
        }

        Debug.Log("Publishing complete. Closing menu.");
        CloseMenu();
    }




    private void OnCloseClicked() {
        Debug.Log("SaveMenuController: Closing save menu.");
        CloseMenu();
    }

    private void CloseMenu() {
        player.ToggleMovement(true);
        gameObject.SetActive(false);
    }
}
