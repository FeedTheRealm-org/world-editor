using System.Collections.Generic;
using System.Linq;
using Enums;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Shop;
using FeedTheRealm.Gameplay.Library.CreatorObjectLibrary;
using FeedTheRealm.UI.Common;
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

    [SerializeField]
    private GameObject manageShopsMenuPrefab;

    [SerializeField]
    private Texture2D goldIcon;

    [SerializeField]
    private Texture2D gemsIcon;

    private Button closeButton;
    private Button manageShopsButton;
    private DropdownField shopSelector;
    private DropdownField itemSelector;
    private ListView itemContainer;
    private Label noShopLabel;

    private const string ShopPlaceholder = "No shop selected";
    private const string ItemPlaceholder = "Select an item";
    private string selectedShopId;

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        closeButton = root.Q<Button>("Close");
        manageShopsButton = root.Q<Button>("ManageShops");
        shopSelector = root.Q<DropdownField>("ShopDropdown");
        itemSelector = root.Q<DropdownField>("AddItemContaier");
        itemContainer = root.Q<ListView>("ItemContainer");
        noShopLabel = root.Q<Label>("NoShopLabel");

        closeButton.clicked += CloseMenu;
        manageShopsButton.clicked += OpenManageShopsMenu;
        shopSelector.RegisterValueChangedCallback(OnShopSelected);
        itemSelector.RegisterValueChangedCallback(OnItemSelected);

        PopulateShopDropdown();
        SetupItemDropdown();
        SetupListView();
        UpdateShopState();
    }

    private void PopulateShopDropdown()
    {
        shopSelector.choices.Clear();
        shopSelector.choices.Add(ShopPlaceholder);
        foreach (var shop in shopManager.GetShops())
            shopSelector.choices.Add(shop.displayName);
        shopSelector.SetValueWithoutNotify(ShopPlaceholder);
        selectedShopId = null;
    }

    private void SetupItemDropdown()
    {
        itemSelector.choices.Clear();
        itemSelector.choices.Add(ItemPlaceholder);
        AddItemsByCategory(CreatorObjectCategories.WeaponItem);
        AddItemsByCategory(CreatorObjectCategories.ConsumableItem);
        itemSelector.SetValueWithoutNotify(ItemPlaceholder);
    }

    private void AddItemsByCategory(CreatorObjectCategories category)
    {
        List<CreatorObject> items = creatorObjectLibrary.GetCreatables(category);
        itemSelector.choices.AddRange(items.Select(item => item.DisplayName));
    }

    private void SetupListView()
    {
        itemContainer.fixedItemHeight = 120;
        itemContainer.selectionType = SelectionType.None;

        itemContainer.makeItem = () =>
        {
            VisualElement ve = itemListTemplate.Instantiate();
            ve.style.marginBottom = 8;
            return ve;
        };

        itemContainer.bindItem = (ve, index) =>
        {
            if (selectedShopId == null)
                return;
            var products = shopManager.GetProducts(selectedShopId);
            if (products == null || index >= products.Count)
                return;

            ProductObject product = products[index];

            if (product.item == null && product.itemId != null)
                product.item = creatorObjectLibrary
                    .GetAllCreatorObjects()
                    .Find(co => co.ObjectId == product.itemId);

            if (product.item == null)
            {
                logger.Log(
                    $"Could not resolve item {product.itemId} for shop product.",
                    this,
                    Logging.LogType.Warning
                );
                return;
            }

            CreatorObject item = product.item;

            ve.Q<Label>("ProductName").text = item.DisplayName;

            Image image = ve.Q<Image>("Preview");
            LoadItemSprite(item, image);

            IntegerField priceField = ve.Q<IntegerField>("ProductPrice");
            priceField.SetValueWithoutNotify(product.price);
            priceField.RegisterValueChangedCallback(evt => product.price = evt.newValue);

            DropdownField currencyDropdown = ve.Q<DropdownField>("CurrencyType");
            currencyDropdown.choices = new List<string> { "Gold", "Gems" };
            currencyDropdown.SetValueWithoutNotify(product.currency.ToString());
            currencyDropdown.RegisterValueChangedCallback(evt =>
            {
                product.currency = (CurrencyType)
                    System.Enum.Parse(typeof(CurrencyType), evt.newValue);
                SetCurrencyIcon(ve.Q<Image>("CurrencyIcon"), product.currency);
            });
            SetCurrencyIcon(ve.Q<Image>("CurrencyIcon"), product.currency);

            ve.Q<Button>("Delete").clicked += () =>
            {
                shopManager.RemoveProduct(selectedShopId, product.id);
                itemContainer.RefreshItems();
            };
        };
    }

    private void UpdateShopState()
    {
        bool hasShop = selectedShopId != null;
        noShopLabel.style.display = hasShop ? DisplayStyle.None : DisplayStyle.Flex;
        itemContainer.style.display = hasShop ? DisplayStyle.Flex : DisplayStyle.None;
        itemSelector.SetEnabled(hasShop);

        if (hasShop)
        {
            itemContainer.itemsSource = shopManager.GetProducts(selectedShopId);
            itemContainer.RefreshItems();
        }
    }

    private void OnShopSelected(ChangeEvent<string> evt)
    {
        if (evt.newValue == ShopPlaceholder)
        {
            selectedShopId = null;
            UpdateShopState();
            return;
        }

        ShopObject shop = shopManager.GetShops().FirstOrDefault(s => s.displayName == evt.newValue);
        selectedShopId = shop?.id;
        UpdateShopState();
    }

    private void OnItemSelected(ChangeEvent<string> evt)
    {
        if (selectedShopId == null || evt.newValue == ItemPlaceholder)
            return;

        CreatorObject selectedItem = creatorObjectLibrary
            .GetAllCreatorObjects()
            .FirstOrDefault(item => item.DisplayName == evt.newValue);

        if (selectedItem == null)
            return;

        shopManager.AddProduct(selectedShopId, selectedItem, 0);
        itemContainer.itemsSource = shopManager.GetProducts(selectedShopId);
        itemContainer.RefreshItems();
        itemSelector.SetValueWithoutNotify(ItemPlaceholder);
    }

    private void SetCurrencyIcon(Image icon, CurrencyType currency)
    {
        icon.image = currency == CurrencyType.Gold ? goldIcon : gemsIcon;
    }

    private void LoadItemSprite(CreatorObject item, Image image)
    {
        Sprite sprite = CustomFileBrowser.LoadSpriteFromDisk(item.spriteFile, true);
        if (sprite == null)
        {
            logger.Log("Failed to load sprite for preview", this, Logging.LogType.Error);
            return;
        }
        image.sprite = sprite;
    }

    private void OpenManageShopsMenu()
    {
        OpenMenu(manageShopsMenuPrefab);
    }

    void OnDisable()
    {
        closeButton.clicked -= CloseMenu;
        manageShopsButton.clicked -= OpenManageShopsMenu;
    }
}
