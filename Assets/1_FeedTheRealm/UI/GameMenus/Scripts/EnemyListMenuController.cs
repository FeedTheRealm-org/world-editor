using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Models;

[RequireComponent(typeof(UIDocument))]
public class EnemyListMenuController : MonoBehaviour {
  [SerializeField] private Enemy enemyDatabase;
  [SerializeField] private Maker player;
  [SerializeField] private Logging.Logger logger;

  // Reference to the Add Enemy menu so we can open it for editing
  [SerializeField] private GameObject addEnemyMenu;
  private AddEnemyMenuController addEnemyMenuController;

  private VisualElement root;
  private ListView listView;
  private Button closeButton;

  private void OnEnable() {
    var uiDoc = GetComponent<UIDocument>();
    root = uiDoc.rootVisualElement;
    if (root == null) {
      if (logger != null)
        logger.Log("EnemyListMenuController: UIDocument has no visual tree. Assign a UXML to the Source Asset.", this, Logging.LogType.Error);
      return;
    }

    var container = root.Q<VisualElement>("ListContainer");
    if (container == null) {
      if (logger != null)
        logger.Log("EnemyListMenuController: Could not find 'ListContainer' element in UXML.", this, Logging.LogType.Error);
      return;
    }

    closeButton = root.Q<Button>("Close");
    if (closeButton != null) closeButton.clicked += CloseMenu;

    if (addEnemyMenu != null && addEnemyMenuController == null) {
      addEnemyMenuController = addEnemyMenu.GetComponent<AddEnemyMenuController>();
      if (addEnemyMenuController == null && logger != null) {
        logger.Log("EnemyListMenuController: addEnemyMenu does not have an AddEnemyMenuController component.", this, Logging.LogType.Warning);
      }
    }

    if (player != null) player.ToggleMovement(false);

    SetUpListView();
    container.Add(listView);
    RefreshEnemies();
  }

  private void OnDisable() {
    if (closeButton != null) closeButton.clicked -= CloseMenu;
    if (player != null) player.ToggleMovement(true);
  }

  private void SetUpListView() {
    listView = new ListView {
      name = "EnemyListView",
      selectionType = SelectionType.Single,
      fixedItemHeight = 56,
      showBoundCollectionSize = true
    };

    listView.makeItem = () => CreateItemForList();
    listView.bindItem = (element, i) => FillElementWithData(element, i);
  }

  private VisualElement CreateItemForList() {
    var rootElem = new VisualElement();
    rootElem.style.flexDirection = FlexDirection.Row;
    rootElem.style.alignItems = Align.Center;
    rootElem.style.paddingLeft = 6;
    rootElem.style.paddingRight = 6;

    var img = new Image { name = "enemyImage" };
    img.style.width = 48;
    img.style.height = 48;
    img.style.marginRight = 8;

    var textCol = new VisualElement();
    textCol.style.flexDirection = FlexDirection.Column;
    textCol.style.flexGrow = 1;

    var nameLabel = new Label { name = "enemyName" };
    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

    var infoLabel = new Label { name = "enemyInfo" };
    infoLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
    infoLabel.style.fontSize = 12;

    textCol.Add(nameLabel);
    textCol.Add(infoLabel);

    var editBtn = new Button { name = "editBtn", text = "Edit" };
    editBtn.style.width = 80;
    editBtn.style.height = 30;
    editBtn.style.marginRight = 4;

    var deleteBtn = new Button { name = "deleteBtn", text = "Delete" };
    deleteBtn.style.width = 80;
    deleteBtn.style.height = 30;

    editBtn.clicked += () => {
      int idx = rootElem.userData is int index ? index : -1;
      if (idx >= 0) EditEnemyAtIndex(idx);
    };

    deleteBtn.clicked += () => {
      int idx = rootElem.userData is int index ? index : -1;
      if (idx >= 0) DeleteEnemyAtIndex(idx);
    };

    rootElem.Add(img);
    rootElem.Add(textCol);
    rootElem.Add(editBtn);
    rootElem.Add(deleteBtn);

    return rootElem;
  }

  private void FillElementWithData(VisualElement element, int i) {
    element.userData = i;

    var enemies = enemyDatabase != null ? enemyDatabase.GetAllEnemies() : new List<EnemyData>();
    if (i < 0 || i >= enemies.Count) return;

    var data = enemies[i];
    if (data == null) return;

    var img = element.Q<Image>("enemyImage");
    if (img != null) {
      Sprite sprite = null;
      try {
        string idOrPath = data.spriteId;
        if (!string.IsNullOrEmpty(idOrPath)) {
          string resolved = SpriteStorage.GetFilePathFromIdOrPath(idOrPath);
          if (!string.IsNullOrEmpty(resolved) && (Path.IsPathRooted(resolved) || File.Exists(resolved))) {
            sprite = LoadSpriteFromAbsoluteFile(resolved);
          } else {
            sprite = Resources.Load<Sprite>(idOrPath);
          }
        }
      } catch (System.Exception ex) {
        if (logger != null)
          logger.Log($"EnemyListMenuController: Failed to load sprite for enemy '{data.name}': {ex.Message}", this, Logging.LogType.Warning);
      }

      img.image = sprite != null ? sprite.texture : null;
    }

    var nameLabel = element.Q<Label>("enemyName");
    if (nameLabel != null) nameLabel.text = data.name ?? "(unnamed)";

    var infoLabel = element.Q<Label>("enemyInfo");
    if (infoLabel != null) {
      int lootCount = data.lootItems != null ? data.lootItems.Count : 0;
      infoLabel.text = $"HP: {data.healthPoints} • DMG: {data.damage} • SPD: {data.speed} • RNG: {data.range} • Loots: {lootCount} • Gold: {data.goldAmount}";
    }
  }

  private void RefreshEnemies() {
    if (listView == null) return;

    if (enemyDatabase == null) {
      if (logger != null)
        logger.Log("EnemyListMenuController: enemyDatabase is not assigned.", this, Logging.LogType.Warning);
      listView.itemsSource = new List<EnemyData>();
      listView.Rebuild();
      return;
    }

    var enemies = enemyDatabase.GetAllEnemies() ?? new List<EnemyData>();
    listView.itemsSource = enemies;
    listView.Rebuild();
  }

  private void DeleteEnemyAtIndex(int index) {
    if (enemyDatabase == null) return;

    var list = enemyDatabase.GetAllEnemies();
    if (list == null || index < 0 || index >= list.Count) return;

    var removed = list[index];
    if (removed == null) return;

    enemyDatabase.RemoveEnemy(removed);

#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(enemyDatabase);
    UnityEditor.AssetDatabase.SaveAssets();
#endif

    if (logger != null)
      logger.Log($"EnemyListMenuController: Removed enemy '{removed.name}' at index {index}.", this);

    RefreshEnemies();
  }

  private void EditEnemyAtIndex(int index) {
    if (enemyDatabase == null) return;

    var list = enemyDatabase.GetAllEnemies();
    if (list == null || index < 0 || index >= list.Count) return;

    if (addEnemyMenu == null) {
      if (logger != null)
        logger.Log("EnemyListMenuController: addEnemyMenu reference is not assigned.", this, Logging.LogType.Warning);
      return;
    }

    if (addEnemyMenuController == null) {
      addEnemyMenuController = addEnemyMenu.GetComponent<AddEnemyMenuController>();
      if (addEnemyMenuController == null) {
        if (logger != null)
          logger.Log("EnemyListMenuController: Could not find AddEnemyMenuController on addEnemyMenu GameObject.", this, Logging.LogType.Error);
        return;
      }
    }

    // Open the Add Enemy menu and initialize it for editing the selected enemy
    addEnemyMenu.SetActive(true);
    addEnemyMenuController.BeginEditEnemy(index);

    // Close this list menu while editing
    gameObject.SetActive(false);
  }

  private void CloseMenu() {
    if (player != null) player.ToggleMovement(true);
    gameObject.SetActive(false);
  }

  private Sprite LoadSpriteFromAbsoluteFile(string absolutePath) {
    try {
      if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath)) return null;
      byte[] data = File.ReadAllBytes(absolutePath);
      var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
      if (!tex.LoadImage(data)) return null;
      tex.name = Path.GetFileNameWithoutExtension(absolutePath);
      var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
      sprite.name = tex.name;
      return sprite;
    } catch (System.Exception ex) {
      if (logger != null)
        logger.Log($"EnemyListMenuController: Failed to load sprite from file '{absolutePath}'. {ex.Message}", this, Logging.LogType.Warning);
      return null;
    }
  }
}
