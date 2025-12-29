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
    [SerializeField] private GameObject consumableItemsHUD;
    [SerializeField] private GameObject editEnemiesHUD;

    private ListView itemListView;
    private ListView spawnerListView;

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

        var consumableItemsButton = root.Q<Button>("ConsumableItemsButton");
        consumableItemsButton.clicked += OpenConsumableItemsMenu;

        var editEnemyButton = root.Q<Button>("EnemiesButton");
        editEnemyButton.clicked += OpenEnemyEditMenu;

        // Find the ListView from the UXML
        itemListView = root.Q<ListView>("ItemListView");
        if (itemListView == null) {
            Debug.LogError("HUDController: ItemListView not found in UXML");
            return;
        }

        spawnerListView = root.Q<ListView>("SpawnerListView");
        if (spawnerListView == null) {
            Debug.LogError("HUDController: SpawnerListView not found in UXML");
            return;
        }
        assetDatabase.ForceReinitialize();
        SetupItemListView();
        SetupSpawnerListView();
    }

    private void SetupItemListView() {
        var assetList = new List<Asset>(assetDatabase.GetAllAssets());

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
            button.style.width = Length.Percent(100);
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

    private void SetupSpawnerListView() {
        var spawnerList = new List<Asset>(assetDatabase.GetAllSpawners());

        if (spawnerList == null || spawnerList.Count == 0) {
            Debug.LogWarning("HUDController: No spawner assets found in database");
            return;
        }

        spawnerListView.itemsSource = spawnerList;

        spawnerListView.makeItem = () => {
            var button = new Button();
            button.AddToClassList("item_box");
            button.style.marginBottom = 12;
            button.style.marginTop = 12;
            button.style.width = Length.Percent(100);  // Add this line
            return button;
        };

        spawnerListView.bindItem = (element, index) => {
            var button = element as Button;
            var asset = spawnerList[index];

            button.text = $"{asset.Name} Spawner";
            button.clicked += () => OnItemSelected(asset);
        };

        spawnerListView.fixedItemHeight = 150;
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

    private void OpenConsumableItemsMenu() {
        if (consumableItemsHUD != null) {
            consumableItemsHUD.SetActive(true);
            gameObject.SetActive(false);
        } else {
            Debug.LogWarning("HUDController: Consumable Items menu reference is not set.");
        }
    }

    private void OpenEnemyEditMenu() {
        if (editEnemiesHUD != null) {
            editEnemiesHUD.SetActive(true);
            gameObject.SetActive(false);
        } else {
            Debug.LogWarning("HUDController: Edit Enemies menu reference is not set.");
        }
    }
}
