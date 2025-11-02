using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour {
    [SerializeField] private AssetDatabaseSO assetDatabase;
    [SerializeField] private PlacementSystem placementSystem;

    private ScrollView itemScrollView;

    private void OnEnable() {
        // Get the UIDocument attached to this GameObject
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var saveButton = root.Q<Button>("SaveButton");
        saveButton.clicked += () => DataPersistenceManager.instance.SaveWorld();

        var deleteButton = root.Q<Button>("DeleteButton");
        deleteButton.clicked += () => placementSystem.StartRemoving();

        // Find the ScrollView from the UXML by its name
        itemScrollView = root.Q<ScrollView>("ItemScrollView");

        PopulateButtons();
    }

    private void PopulateButtons() {
        itemScrollView.Clear();

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
}
