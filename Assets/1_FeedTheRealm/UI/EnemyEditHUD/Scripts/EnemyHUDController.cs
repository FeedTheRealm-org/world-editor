using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Models;

[RequireComponent(typeof(UIDocument))]
public class EnemyHUDController : MonoBehaviour {
  [Header("Menus")]
  [SerializeField] private GameObject listEnemyMenu;
  [SerializeField] private GameObject addEnemyMenu;
  [SerializeField] private GameObject CreatorHUD;
  [SerializeField] private Logging.Logger logger;
  [SerializeField] private Enemy enemyDatabase;
  [SerializeField] private Maker player;

  private ListView enemyListView;
  private Button addEnemyButton;
  private Button listEnemyButton;
  private Button closeEnemyButton;

  private DropdownField canMoveDropdown;

  private void OnEnable() {
    // Get the UIDocument attached to this GameObject
    var uiDocument = GetComponent<UIDocument>();
    var root = uiDocument.rootVisualElement;
    if (root == null) {
      logger.Log("EnemyHUDController: UIDocument has no visual tree. Assign a UXML to the Source Asset.", this, Logging.LogType.Error);
      return;
    }

    canMoveDropdown = root.Q<DropdownField>("CanMove");

    if (canMoveDropdown != null) {
        canMoveDropdown.choices = new List<string> { "true", "false" };
        if (string.IsNullOrEmpty(canMoveDropdown.value))
            canMoveDropdown.value = "true";
    }

    addEnemyButton = root.Q<Button>("AddEnemyButton");
    listEnemyButton = root.Q<Button>("EnemyListButton");
    enemyListView = root.Q<ListView>("EnemyListView");
    closeEnemyButton = root.Q<Button>("CloseEnemyHUDButton");

    if (addEnemyButton != null) addEnemyButton.clicked += OpenAddEnemyMenu;
    if (listEnemyButton != null) listEnemyButton.clicked += OpenListEnemyMenu;
    if (closeEnemyButton != null) closeEnemyButton.clicked += CloseEnemyHUD;

    if (player != null) player.ToggleMovement(false);

    if (enemyListView != null) SetupListView();
    RefreshItems();
  }

  private void OnDisable() {
    if (addEnemyButton != null) addEnemyButton.clicked -= OpenAddEnemyMenu;
    if (listEnemyButton != null) listEnemyButton.clicked -= OpenListEnemyMenu;
    if (closeEnemyButton != null) closeEnemyButton.clicked -= CloseEnemyHUD;
    if (player != null) player.ToggleMovement(true);
  }

  private void SetupListView() {
    enemyListView.itemsSource = new List<EnemyData>();
    enemyListView.makeItem = () => {
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

    enemyListView.bindItem = (element, i) => {
      element.userData = i;
      var btn = element.Q<Button>("itemButton");
      var items = (enemyDatabase != null) ? enemyDatabase.GetAllEnemies() : new List<EnemyData>();
      if (i < 0 || i >= items.Count) {
        if (btn != null) btn.text = "";
        return;
      }
      var data = items[i];
      if (btn != null) btn.text = data.name ?? "(unnamed)";
    };

    enemyListView.fixedItemHeight = 80;
  }

  private void RefreshItems() {
    if (enemyListView == null) return;
    var items = enemyDatabase != null ? enemyDatabase.GetAllEnemies() ?? new List<EnemyData>() : new List<EnemyData>();
    enemyListView.itemsSource = items;
    enemyListView.Rebuild();
  }

  private void OnItemSelected(int index) {
    var items = enemyDatabase != null ? enemyDatabase.GetAllEnemies() : null;
    if (items == null || index < 0 || index >= items.Count) return;
    var item = items[index];
    logger.Log($"EnemyHUDController: Selected item '{item.name}' (Can Move {item.canMove}, Damage {item.damage})", this);
    // future integration: inspect or place item
  }

  // TODO: refactor to a MenuManager
  private void OpenAddEnemyMenu() {
    if (addEnemyMenu != null) {
      addEnemyMenu.SetActive(true);
    } else {
      logger.Log("EnemyHUDController: Add Enemy menu reference is not set.", this, Logging.LogType.Warning);
    }
  }

  private void OpenListEnemyMenu() {
    if (listEnemyMenu != null) {
      listEnemyMenu.SetActive(true);
    } else {
      logger.Log("EnemyHUDController: List Enemy menu reference is not set.", this, Logging.LogType.Warning);
    }
  }

  private void CloseEnemyHUD() {
    if (CreatorHUD != null) {
      CreatorHUD.SetActive(true);
    } else {
      logger.Log("EnemyHUDController: CreatorHUD reference is not set.", this, Logging.LogType.Warning);
    }
    gameObject.SetActive(false);
  }
}
