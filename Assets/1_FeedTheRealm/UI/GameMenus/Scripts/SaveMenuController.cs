using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SaveMenuController : MenuController
{
    [SerializeField]
    private DataPersistenceManagerSO dataPersistenceManager;

    private Button saveButton;
    private Button closeButton;
    private TextField nameInput;
    private VisualElement root;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        saveButton = root.Q<Button>("Save");
        closeButton = root.Q<Button>("Close");
        nameInput = root.Q<TextField>("NameInput");
        WorldData worldData = dataPersistenceManager.CurrentWorldData;
        if (worldData != null && !string.IsNullOrEmpty(worldData.worldName))
        {
            nameInput.value = worldData.worldName;
        }

        saveButton.clicked += OnSaveClicked;
        closeButton.clicked += CloseMenu;
    }

    private void OnDisable()
    {
        saveButton.clicked -= OnSaveClicked;
        closeButton.clicked -= CloseMenu;
    }

    private void OnSaveClicked()
    {
        string worldName = nameInput?.value?.Trim();
        if (string.IsNullOrEmpty(worldName))
        {
            Debug.LogWarning("SaveMenuController: No world name entered!");
            return;
        }

        Debug.Log($"SaveMenuController: Saving world '{worldName}'");
        dataPersistenceManager.SaveWorld(worldName);

        CloseMenu();
    }
}
