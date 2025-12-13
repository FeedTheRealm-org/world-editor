using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Models;

[RequireComponent(typeof(UIDocument))]
public class ConsumableItemHUDController : MonoBehaviour {
  [Header("Menus")]
  [SerializeField] private GameObject listItemsMenu;
  [SerializeField] private GameObject addItemMenu;
  [SerializeField] private GameObject CreatorHUD;
  [SerializeField] private Logging.Logger logger;
  [SerializeField] private ConsumableItems consumableItemsDatabase;
  [SerializeField] private Maker player;

  private ListView itemListView;
  private Button addItemButton;
  private Button listItemsButton;
  private Button closeConsumableButton;

  private void OnEnable() {
    // Get the UIDocument attached to this GameObject
    var uiDocument = GetComponent<UIDocument>();
    var root = uiDocument.rootVisualElement;
    if (root == null) {
      logger.Log("ConsumableItemHUDController: UIDocument has no visual tree. Assign a UXML to the Source Asset.", this, Logging.LogType.Error);
      return;
    }

    addItemButton = root.Q<Button>("AddItemButton");
    listItemsButton = root.Q<Button>("ListItemsButton");
    itemListView = root.Q<ListView>("ItemListView");
    closeConsumableButton = root.Q<Button>("CloseConsumableHUDButton");

    if (addItemButton != null) addItemButton.clicked += OpenAddItemMenu;
    if (listItemsButton != null) listItemsButton.clicked += OpenListItemsMenu;
    if (closeConsumableButton != null) closeConsumableButton.clicked += CloseConsumableHUD;

    if (player != null) player.ToggleMovement(false);

    if (itemListView != null) SetupListView();
    RefreshItems();
  }

  private void OnDisable() {
    if (addItemButton != null) addItemButton.clicked -= OpenAddItemMenu;
    if (listItemsButton != null) listItemsButton.clicked -= OpenListItemsMenu;
    if (closeConsumableButton != null) closeConsumableButton.clicked -= CloseConsumableHUD;
    if (player != null) player.ToggleMovement(true);
  }

  private void SetupListView() {
    itemListView.itemsSource = new List<ConsumableItem>();
    itemListView.makeItem = () => {
      var rootElem = new VisualElement();
      rootElem.style.flexDirection = FlexDirection.Row;
      rootElem.style.alignItems = Align.Center;
      rootElem.style.justifyContent = Justify.FlexStart;
      rootElem.style.paddingLeft = 6;
      rootElem.style.paddingRight = 6;

      var btn = new Button() { name = "itemButton" };
      btn.AddToClassList("item_box");
      btn.style.flexGrow = 1;
      btn.style.height = 70;

      btn.clicked += () => {
        int idx = (int?)(rootElem.userData as int?) ?? -1;
        if (idx >= 0) OnItemSelected(idx);
      };

      rootElem.Add(btn);
      return rootElem;
    };

    itemListView.bindItem = (element, i) => {
      element.userData = i;
      var btn = element.Q<Button>("itemButton");
      var items = (consumableItemsDatabase != null) ? consumableItemsDatabase.GetAllConsumableItems() : new List<ConsumableItem>();
      if (i < 0 || i >= items.Count) {
        if (btn != null) btn.text = "";
        return;
      }
      var data = items[i];
      if (btn != null) btn.text = data.name ?? "(unnamed)";
    };

    itemListView.fixedItemHeight = 80;
  }

  private void RefreshItems() {
    if (itemListView == null) return;
    var items = consumableItemsDatabase != null ? consumableItemsDatabase.GetAllConsumableItems() ?? new List<ConsumableItem>() : new List<ConsumableItem>();
    itemListView.itemsSource = items;
    itemListView.Rebuild();
  }

  private void OnItemSelected(int index) {
    var items = consumableItemsDatabase != null ? consumableItemsDatabase.GetAllConsumableItems() : null;
    if (items == null || index < 0 || index >= items.Count) return;
    var item = items[index];
    logger.Log($"ConsumableItemHUDController: Selected item '{item.name}' (Effect {item.effectType}, Value {item.value})", this);
    // future integration: inspect or place item
  }

  // TODO: refactor to a MenuManager
  private void OpenAddItemMenu() {
    if (addItemMenu != null) {
      addItemMenu.SetActive(true);
    } else {
      logger.Log("ConsumableItemHUDController: Add Item menu reference is not set.", this, Logging.LogType.Warning);
    }
  }

  private void OpenListItemsMenu() {
    if (listItemsMenu != null) {
      listItemsMenu.SetActive(true);
    } else {
      logger.Log("ConsumableItemHUDController: List Items menu reference is not set.", this, Logging.LogType.Warning);
    }
  }

  private void CloseConsumableHUD() {
    if (CreatorHUD != null) {
      CreatorHUD.SetActive(true);
    } else {
      logger.Log("ConsumableItemHUDController: CreatorHUD reference is not set.", this, Logging.LogType.Warning);
    }
    gameObject.SetActive(false);
  }
}
