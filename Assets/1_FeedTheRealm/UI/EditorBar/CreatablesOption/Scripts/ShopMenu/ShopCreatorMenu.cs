using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.ShopMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ShopCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject shopsMenuPrefab;

        [SerializeField]
        private VisualTreeAsset productItemTemplate;

        private ShopData editingData;

        // Header
        private TextField shopNameField;
        private Button saveButton;
        private Button returnButton;

        // Tabs
        private VisualElement _tabGold;
        private VisualElement _tabCosmetic;
        private Label _tabGoldLabel;
        private Label _tabCosmeticLabel;
        private VisualElement _tabGoldContent;
        private VisualElement _tabCosmeticContent;

        // Gold tab controls
        private DropdownField itemSelector;
        private ListView itemContainer;

        // Cosmetic tab controls (stubbed — wired but not populated)
        private DropdownField cosmeticItemSelector;
        private ListView cosmeticItemContainer;

        private bool _isGoldTabActive = true;
        private const string ItemPlaceholder = "Select an item to add";

        // ─────────────────────────────────────────────────────────────

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Header
            shopNameField = root.Q<TextField>("ShopName");
            saveButton = root.Q<Button>("Save");
            returnButton = root.Q<Button>("Return");

            // Tabs
            _tabGold = root.Q<VisualElement>("TabGold");
            _tabCosmetic = root.Q<VisualElement>("TabCosmetic");
            _tabGoldLabel = root.Q<Label>("TabGoldLabel");
            _tabCosmeticLabel = root.Q<Label>("TabCosmeticLabel");
            _tabGoldContent = root.Q<VisualElement>("TabGoldContent");
            _tabCosmeticContent = root.Q<VisualElement>("TabCosmeticContent");

            _tabGold?.RegisterCallback<ClickEvent>(_ => SwitchTab(true));
            _tabCosmetic?.RegisterCallback<ClickEvent>(_ => SwitchTab(false));

            // Gold tab controls
            itemSelector = root.Q<DropdownField>("AddItemContaier");
            itemContainer = root.Q<ListView>("ItemContainer");

            // Cosmetic tab controls (stubbed)
            cosmeticItemSelector = root.Q<DropdownField>("AddCosmeticItemContainer");
            cosmeticItemContainer = root.Q<ListView>("CosmeticItemContainer");

            // Initialize fresh data
            editingData = new ShopData { id = Guid.NewGuid().ToString(), shopName = "" };

            shopNameField.RegisterValueChangedCallback(evt => editingData.shopName = evt.newValue);

            returnButton.clicked += ReturnToList;
            saveButton.clicked += Save;
            root.Q<Button>("AddItem").clicked += OnAddItemClicked;
            // AddCosmeticItem button is intentionally unbound until cosmetics are implemented.

            PopulateItemDropdown();
            SetupListView();
            RefreshProductList();

            SwitchTab(true);
        }

        public void SetupEditor(Shop shop)
        {
            editingData = shop.data;
            shopNameField.value = editingData.shopName;
            shopNameField.RegisterValueChangedCallback(evt => editingData.shopName = evt.newValue);
            RefreshProductList();
        }

        private void SwitchTab(bool goldTab)
        {
            _isGoldTabActive = goldTab;

            if (_tabGoldContent != null)
                _tabGoldContent.style.display = goldTab ? DisplayStyle.Flex : DisplayStyle.None;

            if (_tabCosmeticContent != null)
                _tabCosmeticContent.style.display = goldTab ? DisplayStyle.None : DisplayStyle.Flex;

            // Active tab: bottom border highlight + label color
            _tabGold?.EnableInClassList("editor-tab--active", goldTab);
            _tabCosmetic?.EnableInClassList("editor-tab--active", !goldTab);

            if (_tabGoldLabel != null)
                _tabGoldLabel.style.color = goldTab
                    ? new StyleColor(new Color(1f, 210f / 255f, 80f / 255f))
                    : new StyleColor(new Color(1f, 1f, 1f, 0.45f));

            if (_tabCosmeticLabel != null)
                _tabCosmeticLabel.style.color = !goldTab
                    ? new StyleColor(new Color(200f / 255f, 180f / 255f, 1f))
                    : new StyleColor(new Color(1f, 1f, 1f, 0.45f));
        }

        private void Save()
        {
            string shopName = shopNameField.value?.Trim();
            if (string.IsNullOrEmpty(shopName))
            {
                ToastNotification.Show("Shop name is required.", "error", Color.red);
                return;
            }

            editingData.shopName = shopName;

            var existing = creatablesManager
                .GetAll<Shop>()
                .FirstOrDefault(s => s.Id == editingData.id);
            if (existing == null)
                creatablesManager.Add(new Shop(editingData));

            ToastNotification.Show("Shop saved successfully!", "success", Color.green);
            ReturnToList();
        }

        private void OnAddItemClicked()
        {
            if (editingData == null)
            {
                ToastNotification.Show(
                    "Save the shop first before adding items.",
                    "error",
                    Color.red
                );
                return;
            }

            if (itemSelector.value == ItemPlaceholder || string.IsNullOrEmpty(itemSelector.value))
            {
                ToastNotification.Show("Please select an item to add.", "error", Color.yellow);
                return;
            }

            string itemId = FindItemId(itemSelector.value);
            if (itemId == null)
                return;

            editingData.products.Add(new ProductData(itemId, 0));
            RefreshProductList();
            itemSelector.SetValueWithoutNotify(ItemPlaceholder);
        }

        private void PopulateItemDropdown()
        {
            var choices = new List<string> { ItemPlaceholder };
            choices.AddRange(creatablesManager.GetAll<ConsumableItem>().Select(i => i.data.name));
            choices.AddRange(creatablesManager.GetAll<Weapon>().Select(w => w.data.name));
            itemSelector.choices = choices;
            itemSelector.SetValueWithoutNotify(ItemPlaceholder);

            // Cosmetic dropdown left empty until gem items are implemented.
            if (cosmeticItemSelector != null)
            {
                cosmeticItemSelector.choices = new List<string> { "No cosmetic items yet" };
                cosmeticItemSelector.SetValueWithoutNotify("No cosmetic items yet");
            }
        }

        private void SetupListView()
        {
            itemContainer.fixedItemHeight = 120;
            itemContainer.selectionType = SelectionType.None;

            itemContainer.makeItem = () =>
            {
                var ve = productItemTemplate.Instantiate();
                ve.style.marginBottom = 8;
                return ve;
            };

            itemContainer.bindItem = (ve, index) =>
            {
                if (editingData == null || index >= editingData.products.Count)
                    return;

                var product = editingData.products[index];
                var item = FindItem(product.productId);
                if (item == null)
                    return;

                ve.Q<Label>("ProductName").text = GetItemName(item);
                LoadItemSprite(item, ve.Q<Image>("Preview"));

                var priceField = ve.Q<IntegerField>("ProductPrice");
                priceField.SetValueWithoutNotify(product.price);
                priceField.RegisterValueChangedCallback(evt => product.price = evt.newValue);

                var currencyDropdown = ve.Q<DropdownField>("CurrencyType");
                currencyDropdown.choices = Enum.GetNames(typeof(CurrencyType)).ToList();
                currencyDropdown.SetValueWithoutNotify(product.currency.ToString());
                currencyDropdown.RegisterValueChangedCallback(evt =>
                {
                    product.currency = Enum.Parse<CurrencyType>(evt.newValue);
                });

                ve.Q<Button>("Delete").clicked += () =>
                {
                    editingData.products.Remove(product);
                    RefreshProductList();
                };
            };

            // Cosmetic ListView left unbound until cosmetics are implemented.
        }

        private void RefreshProductList()
        {
            if (editingData == null)
                return;
            itemContainer.itemsSource = editingData.products;
            itemContainer.RefreshItems();
        }

        private object FindItem(string id)
        {
            return (object)
                    creatablesManager.GetAll<ConsumableItem>().FirstOrDefault(i => i.Id == id)
                ?? creatablesManager.GetAll<Weapon>().FirstOrDefault(w => w.Id == id);
        }

        private string FindItemId(string displayName)
        {
            var consumable = creatablesManager
                .GetAll<ConsumableItem>()
                .FirstOrDefault(i => i.data.name == displayName);
            if (consumable != null)
                return consumable.Id;
            return creatablesManager
                .GetAll<Weapon>()
                .FirstOrDefault(w => w.data.name == displayName)
                ?.Id;
        }

        private string GetItemName(object item) =>
            item switch
            {
                ConsumableItem c => c.data.name,
                Weapon w => w.data.name,
                _ => "",
            };

        private void LoadItemSprite(object item, Image image)
        {
            string path = item switch
            {
                ConsumableItem c => c.data.spriteFilePath,
                Weapon w => w.data.spriteFilePath,
                _ => null,
            };
            if (string.IsNullOrEmpty(path))
                return;
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(path);
            if (sprite != null)
                image.sprite = sprite;
        }

        private void ReturnToList() => OpenMenu(shopsMenuPrefab);
    }
}
