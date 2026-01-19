using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public class ShopMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private ShopManagerSO shopManager;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private DropdownField itemSelector;
    private ListView itemContainer;
    private const string Placeholder = "Select an item";

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        closeButton = root.Q<Button>("Close");
        itemSelector = root.Q<DropdownField>("ItemSelector");
        itemContainer = root.Q<ListView>("ItemContainer");

        closeButton.clicked += CloseMenu;
        itemSelector.RegisterValueChangedCallback(OnItemSelected);

        SetupDropdown();
        SetupListView();
    }

    private void SetupListView()
    {
        itemContainer.itemsSource = shopManager.GetProducts();
        itemContainer.fixedItemHeight = 120;
        itemContainer.selectionType = SelectionType.None;

        itemContainer.makeItem = () =>
        {
            var ve = itemListTemplate.Instantiate();
            ve.style.marginBottom = 8;
            return ve;
        };

        itemContainer.bindItem = (ve, index) =>
        {
            var product = shopManager.GetProducts()[index];
            var item = product.item;

            ve.Q<Label>("ProductName").text = item.DisplayName;

            var image = ve.Q<Image>("Preview");
            LoadItemSprite(item, image);

            var priceField = ve.Q<IntegerField>("ProductPrice");
            priceField.SetValueWithoutNotify(product.price);
            priceField.RegisterValueChangedCallback(evt =>
            {
                product.price = evt.newValue;
            });

            ve.Q<Button>("Delete").clicked += () =>
            {
                shopManager.RemoveProduct(product.id);
                itemContainer.RefreshItems();
            };
        };
    }

    private void SetupDropdown()
    {
        var allItems = creatorObjectLibrary.GetAllCreatorObjects();
        var itemNames = allItems.Select(item => item.DisplayName).ToList();
        itemNames.Insert(0, Placeholder);
        itemSelector.choices = itemNames;
        itemSelector.value = Placeholder;
    }

    private void LoadItemSprite(CreatorObject item, Image image)
    {
        Sprite sprite = FileHandler.LoadSpriteFromDisk(item.spriteFile, true);
        if (sprite == null)
        {
            logger.Log("Failed to load sprite for preview", this, Logging.LogType.Error);
            return;
        }
        image.sprite = sprite;
    }

    private void OnItemSelected(ChangeEvent<string> evt)
    {
        if (evt.newValue == Placeholder)
            return;

        var selectedItem = creatorObjectLibrary
            .GetAllCreatorObjects()
            .FirstOrDefault(item => item.DisplayName == evt.newValue);

        if (selectedItem == null)
            return;

        AddItemToProducts(selectedItem);

        itemSelector.SetValueWithoutNotify(Placeholder);
    }

    private void AddItemToProducts(CreatorObject item)
    {
        shopManager.AddProduct(item, 0);
        itemContainer.RefreshItems();
    }

    void OnDisable()
    {
        closeButton.clicked -= CloseMenu;
    }
}
