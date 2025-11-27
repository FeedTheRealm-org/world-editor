using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDController : MonoBehaviour {
    [SerializeField] private AssetLibrarySO assetDatabase;
    [SerializeField] private PlacementSystem placementSystem;

    [Header("Menus")]
    [SerializeField] private GameObject saveMenu;
    [SerializeField] private GameObject publishMenu;

    private ScrollView itemScrollView;

    private void OnEnable() {
        // Get the UIDocument attached to this GameObject
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var saveButton = root.Q<Button>("SaveButton");
        saveButton.clicked += OpenSaveMenu;

        var deleteButton = root.Q<Button>("DeleteButton");
        deleteButton.clicked += () => placementSystem.StartRemoving();

        var publishButton = root.Q<Button>("PublishWorld");
        publishButton.clicked += OpenPublishMenu;

        // Find the ScrollView from the UXML by its name
        itemScrollView = root.Q<ScrollView>("ItemScrollView");

        assetDatabase.InitializeDatabase();

        PopulateButtons();
    }

    private void PopulateButtons() {
        itemScrollView.Clear();

        foreach (var objData in assetDatabase.GetAllAssets()) {
            var button = new Button {
                text = objData.Name
            };
            button.AddToClassList("item_box");
            button.clicked += () => OnItemSelected(objData);

            itemScrollView.Add(button);
        }
    }

    private void OnItemSelected(Asset obj) {
        Debug.Log($"Selected object: {obj.Name} (ID: {obj.Id})");
        placementSystem.StartPlacement(obj);
    }

    // TODO: refactor to a MenuManager
    private void OpenSaveMenu() {
        if (saveMenu != null) {
            saveMenu.SetActive(true);
        } else {
            Debug.LogWarning("HUDController: Save menu reference is not set.");
        }
    }

    private void OpenPublishMenu() {
        if (publishMenu != null) {
            publishMenu.SetActive(true);
        } else {
            Debug.LogWarning("HUDController: Publish menu reference is not set.");
        }
    }
}
