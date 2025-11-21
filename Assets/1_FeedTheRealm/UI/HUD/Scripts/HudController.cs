using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDController : MonoBehaviour {
    [SerializeField] private AssetDatabaseSO assetDatabase;
    [SerializeField] private PlacementSystem placementSystem;

    [Header("Menus")]
    [SerializeField] private GameObject saveMenu;

    private ScrollView itemScrollView;

    private void OnEnable() {
        // Get the UIDocument attached to this GameObject
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var saveButton = root.Q<Button>("SaveButton");
        saveButton.clicked += OpenSaveMenu;

        var deleteButton = root.Q<Button>("DeleteButton");
        deleteButton.clicked += () => placementSystem.StartRemoving();

        // Find the ScrollView from the UXML by its name
        itemScrollView = root.Q<ScrollView>("ItemScrollView");

        PopulateButtons();
    }

    private void PopulateButtons() {
        itemScrollView.Clear();

        assetDatabase.InitializeDatabase();

        foreach (var objData in assetDatabase.objectData) {
            var button = new Button {
                text = objData.Name
            };
            button.AddToClassList("item_box");
            button.clicked += () => OnItemSelected(objData);

            itemScrollView.Add(button);
        }
    }

    private void OnItemSelected(AssetData obj) {
        Debug.Log($"Selected object: {obj.Name} (ID: {obj.Id})");
        placementSystem.StartPlacement(obj);
    }

    private void OpenSaveMenu() {
        if (saveMenu != null) {
            saveMenu.SetActive(true);
        } else {
            Debug.LogWarning("HUDController: Save menu reference is not set.");
        }
    }
}
