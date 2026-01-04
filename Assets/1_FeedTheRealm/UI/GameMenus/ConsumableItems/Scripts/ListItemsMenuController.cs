using System.Collections.Generic;
using System.IO;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ListItemsMenuController : MenuController
{
    [SerializeField]
    private ConsumableItemLibrarySO consumableItemsDatabase;

    // [SerializeField]
    // private Maker player;

    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    public GameObject addItemMenu;

    private VisualElement root;
    private ListView listView;
    private Button addButton;
    private Button closeButton;

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;
        if (root == null)
        {
            logger.Log(
                "ListItemsMenuController: UIDocument has no visual tree. Assign a UXML to the Source Asset.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        var container = root.Q<VisualElement>("ListContainer");
        if (container == null)
        {
            logger.Log(
                "ListItemsMenuController: Could not find 'ListContainer' element in UXML.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        closeButton = root.Q<Button>("Close");
        addButton = root.Q<Button>("AddItem");

        if (closeButton != null)
            closeButton.clicked += CloseMenu;

        if (addButton != null)
        {
            GameObject addItemMenuInstance = Instantiate(addItemMenu);
            addItemMenuInstance.SetActive(false);
            var menuController =
                addItemMenuInstance.GetComponent<MenuController>()
                ?? throw new System.InvalidOperationException(
                    "ListItemsMenuController: The addItemMenu prefab does not have a MenuController component."
                );
            addButton.clicked += () =>
            {
                menuController.OpenMenu();
            };
        }

        // if (player != null)
        //     player.ToggleMovement(false);

        SetUpListView();
        container.Add(listView);
        RefreshItems();
    }

    private void Update()
    {
        RefreshItems();
    }

    private void SetUpListView()
    {
        listView = new ListView();
        listView.name = "ConsumableListView";
        // Disable selecting items in the list
        listView.selectionType = SelectionType.None;
        listView.fixedItemHeight = 92;
        listView.showBoundCollectionSize = true;

        listView.makeItem = () => CreateItemForList();
        listView.bindItem = (element, i) => FillElementWithData(element, i);
    }

    private void FillElementWithData(VisualElement element, int i)
    {
        element.userData = i;
        var items =
            (consumableItemsDatabase != null)
                ? consumableItemsDatabase.GetAllConsumableItems()
                : new List<ConsumableItem>();
        if (i < 0 || i >= items.Count)
            return;
        var data = items[i];

        var img = element.Q<Image>("itemImage");
        if (img != null)
        {
            Sprite sprite = null;
            try
            {
                string idOrPath = data != null ? data.spriteId : null;
                if (!string.IsNullOrEmpty(idOrPath))
                {
                    string resolved = SpriteStorage.GetFilePathFromIdOrPath(idOrPath);
                    if (
                        !string.IsNullOrEmpty(resolved)
                        && (Path.IsPathRooted(resolved) || File.Exists(resolved))
                    )
                    {
                        sprite = LoadSpriteFromAbsoluteFile(resolved);
                    }
                    else
                    {
                        sprite = Resources.Load<Sprite>(idOrPath);
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (logger != null)
                    logger.Log(
                        $"ListItemsMenuController: Failed to load sprite for item '{data?.name}': {ex.Message}",
                        this,
                        Logging.LogType.Warning
                    );
            }

            img.image = sprite != null ? sprite.texture : null;
        }

        var nameLabel = element.Q<Label>("itemName");
        if (nameLabel != null)
            nameLabel.text = data.name ?? "(unnamed)";

        var infoLabel = element.Q<Label>("itemInfo");
        if (infoLabel != null)
            infoLabel.text = $"{data.effectType} • Value: {data.value} • MaxStack: {data.maxStack}";
    }

    private VisualElement CreateItemForList()
    {
        var rootElem = new VisualElement { focusable = false };
        rootElem.style.flexDirection = FlexDirection.Row;
        rootElem.style.height = new Length(80);
        rootElem.style.width = Length.Percent(100);
        rootElem.style.alignItems = Align.Center;
        rootElem.style.paddingLeft = new Length(6);
        rootElem.style.paddingRight = new Length(6);
        rootElem.style.paddingTop = new Length(6);
        rootElem.style.paddingBottom = new Length(6);
        rootElem.style.marginBottom = new Length(12);

        var img = new Image { name = "itemImage" };
        img.style.width = 65;
        img.style.height = 65;
        img.style.marginRight = 8;

        var textCol = new VisualElement();
        textCol.style.flexDirection = FlexDirection.Column;
        textCol.style.flexGrow = 1;

        var nameLabel = new Label { name = "itemName" };
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLabel.style.fontSize = 22;
        nameLabel.style.color = new StyleColor(Color.white);

        var infoLabel = new Label { name = "itemInfo" };
        infoLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
        infoLabel.style.fontSize = 18;
        infoLabel.style.color = new StyleColor(Color.white);

        textCol.Add(nameLabel);
        textCol.Add(infoLabel);

        var deleteBtn = new Button() { name = "deleteBtn", text = "Delete" };
        deleteBtn.style.width = 90;
        deleteBtn.style.height = 34;
        deleteBtn.style.fontSize = 16;
        deleteBtn.style.marginLeft = 8;
        deleteBtn.style.paddingLeft = 8;
        deleteBtn.style.paddingRight = 8;
        deleteBtn.style.borderTopLeftRadius = 6;
        deleteBtn.style.borderTopRightRadius = 6;
        deleteBtn.style.borderBottomLeftRadius = 6;
        deleteBtn.style.borderBottomRightRadius = 6;
        deleteBtn.style.backgroundColor = new StyleColor(new Color(0.80f, 0.20f, 0.20f));
        deleteBtn.style.color = new StyleColor(Color.white);
        deleteBtn.style.alignSelf = Align.Center;
        deleteBtn.style.unityTextAlign = TextAnchor.MiddleCenter;

        // Handler uses the element.userData which is updated in bindItem
        deleteBtn.clicked += () =>
        {
            logger.Log("ListItemsMenuController: Delete button clicked.", this);
            int idx = (int?)(rootElem.userData as int?) ?? -1;
            if (idx >= 0)
                DeleteItemAtIndex(idx);
        };

        rootElem.Add(img);
        rootElem.Add(textCol);
        rootElem.Add(deleteBtn);

        return rootElem;
    }

    private void AddItemMenu() { }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
        if (addButton != null)
            addButton.clicked -= AddItemMenu;
        // if (player != null)
        //     player.ToggleMovement(true);
    }

    private void RefreshItems()
    {
        if (consumableItemsDatabase == null)
        {
            logger.Log(
                "ListItemsMenuController: consumableItemsDatabase is not assigned.",
                this,
                Logging.LogType.Warning
            );
            listView.itemsSource = new List<ConsumableItem>();
            listView.Rebuild();
            return;
        }

        var items = consumableItemsDatabase.GetAllConsumableItems() ?? new List<ConsumableItem>();
        listView.itemsSource = items;
        listView.Rebuild();
    }

    private void DeleteItemAtIndex(int index)
    {
        if (consumableItemsDatabase == null)
            return;
        var list = consumableItemsDatabase.GetAllConsumableItems();
        if (list == null || index < 0 || index >= list.Count)
            return;

        var removed = list[index];
        consumableItemsDatabase.RemoveConsumableItem(removed);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(consumableItemsDatabase);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        logger.Log(
            $"ListItemsMenuController: Removed item '{removed.name}' at index {index}.",
            this
        );
        RefreshItems();
    }

    private Sprite LoadSpriteFromAbsoluteFile(string absolutePath)
    {
        try
        {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
                return null;
            byte[] data = File.ReadAllBytes(absolutePath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
                return null;
            tex.name = Path.GetFileNameWithoutExtension(absolutePath);
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sprite.name = tex.name;
            return sprite;
        }
        catch (System.Exception ex)
        {
            if (logger != null)
                logger.Log(
                    $"ListItemsMenuController: Failed to load sprite from file '{absolutePath}'. {ex.Message}",
                    this,
                    Logging.LogType.Warning
                );
            return null;
        }
    }
}
