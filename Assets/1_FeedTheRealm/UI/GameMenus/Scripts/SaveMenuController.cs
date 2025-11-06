using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SaveMenuController : MonoBehaviour {
    [SerializeField] private DataPersistenceManagerSO dataPersistenceManager;
    [SerializeField] private Maker player;

    private Button saveButton;
    private Button closeButton;
    private TextField nameInput;
    private VisualElement root;

    private void OnEnable() {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        if (player != null) {
            player.ToggleMovement(false);
        }

        saveButton = root.Q<Button>("Save");
        closeButton = root.Q<Button>("Close");
        nameInput = root.Q<TextField>("NameInput");

        if (saveButton != null)
            saveButton.clicked += OnSaveClicked;

        if (closeButton != null)
            closeButton.clicked += OnCloseClicked;
    }

    private void OnDisable() {
        // Unhook to avoid leaks / duplicates
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;

        if (closeButton != null)
            closeButton.clicked -= OnCloseClicked;
    }

    private void OnSaveClicked() {
        string worldName = nameInput?.value?.Trim();
        if (string.IsNullOrEmpty(worldName)) {
            Debug.LogWarning("SaveMenuController: No world name entered!");
            return;
        }

        Debug.Log($"SaveMenuController: Saving world '{worldName}'");
        dataPersistenceManager.SaveWorld(worldName);

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
