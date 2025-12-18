using Models;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class HUDController : MonoBehaviour {
    [SerializeField] private AssetLibrarySO assetDatabase;
    [SerializeField] private PlacementSystem placementSystem;

    [Header("Menus")]
    [SerializeField] private GameObject saveMenu;
    [SerializeField] private GameObject publishMenu;

    private ListView itemListView;
    private List<Asset> assetList;

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

        var addEnemySpawnButton = root.Q<Button>("AddEnemySpawn");
        addEnemySpawnButton.clicked += OnAddEnemySpawn;

        // Find the ListView from the UXML
        itemListView = root.Q<ListView>("ItemListView");
        if (itemListView == null) {
            Debug.LogError("HUDController: ItemListView not found in UXML");
            return;
        }

        assetDatabase.InitializeDatabase();
        SetupListView();
    }

    private void SetupListView() {
        assetList = new List<Asset>(assetDatabase.GetAllAssets());

        if (assetList == null || assetList.Count == 0) {
            Debug.LogWarning("HUDController: No assets found in database");
            return;
        }

        itemListView.itemsSource = assetList;

        itemListView.makeItem = () => {
            var button = new Button();
            button.AddToClassList("item_box");
            button.style.marginBottom = 12;
            button.style.marginTop = 12;
            return button;
        };

        itemListView.bindItem = (element, index) => {
            var button = element as Button;
            var asset = assetList[index];

            button.text = asset.Name;
            button.clicked += () => OnItemSelected(asset);
        };

        itemListView.fixedItemHeight = 150;
    }

    private void OnItemSelected(Asset obj) {
        Debug.Log($"Selected object: {obj.Name} (ID: {obj.Id})");
        placementSystem.StartPlacement(obj);
    }

    private void OnAddEnemySpawn() {
        Debug.Log("HUDController: Starting Enemy Spawn placement");
        placementSystem.StartEnemySpawnPlacement();
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
