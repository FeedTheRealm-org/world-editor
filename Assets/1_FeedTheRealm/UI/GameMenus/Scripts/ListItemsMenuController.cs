using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Models;

[RequireComponent(typeof(UIDocument))]
public class ListItemsMenuController : MonoBehaviour {
  [SerializeField] private ConsumableItems consumableItemsDatabase;
  [SerializeField] private Maker player;
  [SerializeField] private Logging.Logger logger;

  private VisualElement root;
  private ListView listView;
  private Button closeButton;

  private void OnEnable() {
    var uiDoc = GetComponent<UIDocument>();
    root = uiDoc.rootVisualElement;
    if (root == null) {
      logger.Log("ListItemsMenuController: UIDocument has no visual tree. Assign a UXML to the Source Asset.", this, Logging.LogType.Error);
      return;
    }

    var container = root.Q<VisualElement>("ListContainer");
    if (container == null) {
      logger.Log("ListItemsMenuController: Could not find 'ListContainer' element in UXML.", this, Logging.LogType.Error);
      return;
    }

    closeButton = root.Q<Button>("Close");

    if (closeButton != null) closeButton.clicked += CloseMenu;

    if (player != null) player.ToggleMovement(false);

    SetUpListView();
    container.Add(listView);
    RefreshItems();
  }

  private void SetUpListView() {
    listView = new ListView();
    listView.name = "ConsumableListView";
    listView.selectionType = SelectionType.Single;
    listView.fixedItemHeight = 56;
    listView.showBoundCollectionSize = true;

    listView.makeItem = () => CreateItemForList();
    listView.bindItem = (element, i) => FillElementWithData(element, i);
  }

  private void FillElementWithData(VisualElement element, int i) {
    element.userData = i;
    var items = (consumableItemsDatabase != null) ? consumableItemsDatabase.GetAllConsumableItems() : new List<ConsumableItem>();
    if (i < 0 || i >= items.Count) return;
    var data = items[i];

    var img = element.Q<Image>("itemImage");
    if (img != null) {
      Sprite sprite = null;
      try {
        string path = data != null ? data.spritePath : null;
        if (!string.IsNullOrEmpty(path)) {
          if (Path.IsPathRooted(path) || File.Exists(path)) {
            sprite = LoadSpriteFromAbsoluteFile(path);
          } else {
            sprite = Resources.Load<Sprite>(path);
          }
        }
      } catch (System.Exception ex) {
        if (logger != null) logger.Log($"ListItemsMenuController: Failed to load sprite for item '{data?.name}': {ex.Message}", this, Logging.LogType.Warning);
      }

      img.image = sprite != null ? sprite.texture : null;
    }

    var nameLabel = element.Q<Label>("itemName");
    if (nameLabel != null) nameLabel.text = data.name ?? "(unnamed)";

    var infoLabel = element.Q<Label>("itemInfo");
    if (infoLabel != null) infoLabel.text = $"{data.effectType} • Value: {data.value} • MaxStack: {data.maxStack}";
  }

  private VisualElement CreateItemForList() {
    var rootElem = new VisualElement();
    rootElem.style.flexDirection = FlexDirection.Row;
    rootElem.style.alignItems = Align.Center;
    rootElem.style.paddingLeft = 6;
    rootElem.style.paddingRight = 6;

    var img = new Image { name = "itemImage" };
    img.style.width = 48;
    img.style.height = 48;
    img.style.marginRight = 8;

    var textCol = new VisualElement();
    textCol.style.flexDirection = FlexDirection.Column;
    textCol.style.flexGrow = 1;

    var nameLabel = new Label { name = "itemName" };
    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

    var infoLabel = new Label { name = "itemInfo" };
    infoLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
    infoLabel.style.fontSize = 12;

    textCol.Add(nameLabel);
    textCol.Add(infoLabel);

    var deleteBtn = new Button() { name = "deleteBtn", text = "Delete" };
    deleteBtn.style.width = 80;
    deleteBtn.style.height = 30;

    // Handler uses the element.userData which is updated in bindItem
    deleteBtn.clicked += () => {
      int idx = (int?)(rootElem.userData as int?) ?? -1;
      if (idx >= 0) DeleteItemAtIndex(idx);
    };

    rootElem.Add(img);
    rootElem.Add(textCol);
    rootElem.Add(deleteBtn);

    return rootElem;
  }

  private void OnDisable() {
    if (closeButton != null) closeButton.clicked -= CloseMenu;
    if (player != null) player.ToggleMovement(true);
  }

  private void RefreshItems() {
    if (consumableItemsDatabase == null) {
      logger.Log("ListItemsMenuController: consumableItemsDatabase is not assigned.", this, Logging.LogType.Warning);
      listView.itemsSource = new List<ConsumableItem>();
      listView.Rebuild();
      return;
    }

    var items = consumableItemsDatabase.GetAllConsumableItems() ?? new List<ConsumableItem>();
    listView.itemsSource = items;
    listView.Rebuild();
  }

  private void DeleteItemAtIndex(int index) {
    if (consumableItemsDatabase == null) return;
    var list = consumableItemsDatabase.GetAllConsumableItems();
    if (list == null || index < 0 || index >= list.Count) return;

    var removed = list[index];
    consumableItemsDatabase.RemoveConsumableItem(removed);

#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(consumableItemsDatabase);
    UnityEditor.AssetDatabase.SaveAssets();
#endif

    logger.Log($"ListItemsMenuController: Removed item '{removed.name}' at index {index}.", this);
    RefreshItems();
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
      if (logger != null) logger.Log($"ListItemsMenuController: Failed to load sprite from file '{absolutePath}'. {ex.Message}", this, Logging.LogType.Warning);
      return null;
    }
  }
}