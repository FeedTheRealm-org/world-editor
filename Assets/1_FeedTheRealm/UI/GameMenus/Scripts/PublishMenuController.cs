using API;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PublishMenuController : MonoBehaviour {
    [SerializeField] private DataPersistenceManagerSO dataPersistenceManager;
    [SerializeField] private WorldService worldService;
    [SerializeField] private Maker player;
    [SerializeField] private Session.Session session;
    private Models.WorldData worldData;

    private Button saveButton;
    private Button closeButton;
    private TextField nameInput;
    private VisualElement root;

    private void OnEnable() {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        worldData = dataPersistenceManager.CurrentWorldData;

        if (player != null) {
            player.ToggleMovement(false);
        }

        saveButton = root.Q<Button>("Publish");
        closeButton = root.Q<Button>("Close");
        nameInput = root.Q<TextField>("NameInput");

        if (worldData != null && !string.IsNullOrEmpty(worldData.worldName)) {
            nameInput.value = worldData.worldName;
        }

        if (saveButton != null)
            saveButton.clicked += OnPublishClicked;

        if (closeButton != null)
            closeButton.clicked += OnCloseClicked;
    }

    private void OnDisable() {
        // Unhook to avoid leaks / duplicates
        if (saveButton != null)
            saveButton.clicked -= OnPublishClicked;

        if (closeButton != null)
            closeButton.clicked -= OnCloseClicked;
    }

    private void OnPublishClicked() {
        Debug.Log("PublishMenuController: Publishing world.");

        string worldName = nameInput.value.Trim();
        string filePath = worldData.filepath;
        string token = session.APIToken;

        if (string.IsNullOrEmpty(worldName)) {
            Debug.LogError("World name cannot be empty.");
            return;
        }
        if (string.IsNullOrEmpty(filePath)) {
            Debug.LogError("World filepath is missing.");
            return;
        }
        if (string.IsNullOrEmpty(token)) {
            Debug.LogError("Session token is missing.");
            return;
        }

        StartCoroutine(
            worldService.CreateWorld(filePath, worldName, token,
            (worldId, error) => {
                if (!string.IsNullOrEmpty(error)) {
                    Debug.LogError($"Failed to publish world: {error}");
                } else {
                    Debug.Log($"World published successfully! New world ID: {worldId}");
                }
            })
        );

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
