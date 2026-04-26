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

        private HashSet<string> _initialCosmeticIds = new HashSet<string>();

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
            _initialCosmeticIds.Clear();
            editingData = new ShopData { id = Guid.NewGuid().ToString(), shopName = "" };

            shopNameField.RegisterValueChangedCallback(evt => editingData.shopName = evt.newValue);

            returnButton.clicked += ReturnToList;
            saveButton.clicked += Save;
            root.Q<Button>("AddItem").clicked += OnAddItemClicked;

            var addCosmeticItemBtn = root.Q<Button>("AddCosmeticItem");
            if (addCosmeticItemBtn != null)
                addCosmeticItemBtn.clicked += OnAddItemClicked;

            SetupListView();
            RefreshProductList();

            SwitchTab(true);
        }

        public void SetupEditor(Shop shop)
        {
            editingData = shop.data;
            _initialCosmeticIds.Clear();
            if (editingData != null && editingData.products != null)
            {
                foreach (var p in editingData.products)
                {
                    if (FindItem(p.productId) is Cosmetic)
                        _initialCosmeticIds.Add(p.productId);
                }
            }
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
            PopulateItemDropdown(_isGoldTabActive);
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

            var currentCosmeticIds = new HashSet<string>();
            foreach (var product in editingData.products)
            {
                if (FindItem(product.productId) is Cosmetic cosmetic)
                {
                    cosmetic.data.price = product.price;
                    currentCosmeticIds.Add(product.productId);
                }
            }

            foreach (var pastId in _initialCosmeticIds)
            {
                if (!currentCosmeticIds.Contains(pastId))
                {
                    var cosmetic = creatablesManager
                        .GetAll<Cosmetic>()
                        .FirstOrDefault(c => c.Id == pastId);
                    if (cosmetic != null)
                        cosmetic.data.price = 0;
                }
            }
            _initialCosmeticIds = new HashSet<string>(currentCosmeticIds);

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

            var selector = _isGoldTabActive ? itemSelector : cosmeticItemSelector;
            if (selector == null)
                return;

            if (selector.value == ItemPlaceholder || string.IsNullOrEmpty(selector.value))
            {
                ToastNotification.Show("Please select an item to add.", "error", Color.yellow);
                return;
            }

            string itemId = FindItemId(selector.value);
            if (itemId == null)
                return;

            editingData.products.Add(new ProductData(itemId, 0));
            RefreshProductList();
            selector.SetValueWithoutNotify(ItemPlaceholder);
        }

        private void PopulateItemDropdown(bool goldTab)
        {
            if (goldTab)
            {
                var choices = new List<string> { ItemPlaceholder };
                choices.AddRange(
                    creatablesManager.GetAll<ConsumableItem>().Select(i => i.data.name)
                );
                choices.AddRange(creatablesManager.GetAll<Weapon>().Select(w => w.data.name));
                itemSelector.choices = choices;
                itemSelector.SetValueWithoutNotify(ItemPlaceholder);
            }
            else
            {
                var choices = new List<string> { ItemPlaceholder };
                choices.AddRange(creatablesManager.GetAll<Cosmetic>().Select(i => i.data.name));
                Debug.Log("Cosmetic choices: " + string.Join(", ", choices));
                cosmeticItemSelector.choices = choices;
                cosmeticItemSelector.SetValueWithoutNotify(ItemPlaceholder);
            }
        }

        private void SetupListView()
        {
            SetupSingleListView(itemContainer);
            SetupSingleListView(cosmeticItemContainer);
        }

        private void SetupSingleListView(ListView listView)
        {
            if (listView == null)
            {
                return;
            }

            listView.fixedItemHeight = 120;
            listView.selectionType = SelectionType.None;

            listView.makeItem = () =>
            {
                var ve = productItemTemplate.Instantiate();
                ve.style.marginBottom = 8;
                return ve;
            };

            listView.bindItem = (ve, index) =>
            {
                if (
                    editingData == null
                    || listView.itemsSource == null
                    || index >= listView.itemsSource.Count
                )
                    return;

                var product = (ProductData)listView.itemsSource[index];
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
        }

        private void RefreshProductList()
        {
            if (editingData == null)
                return;

            var goldProducts = editingData
                .products.Where(p => FindItem(p.productId) is ConsumableItem or Weapon)
                .ToList();
            var cosmeticProducts = editingData
                .products.Where(p => FindItem(p.productId) is Cosmetic)
                .ToList();

            if (itemContainer != null)
            {
                itemContainer.itemsSource = goldProducts;
                itemContainer.RefreshItems();
            }

            if (cosmeticItemContainer != null)
            {
                cosmeticItemContainer.itemsSource = cosmeticProducts;
                cosmeticItemContainer.RefreshItems();
            }
        }

        private object FindItem(string id)
        {
            return (object)
                    creatablesManager.GetAll<ConsumableItem>().FirstOrDefault(i => i.Id == id)
                ?? (object)creatablesManager.GetAll<Weapon>().FirstOrDefault(w => w.Id == id)
                ?? (object)creatablesManager.GetAll<Cosmetic>().FirstOrDefault(c => c.Id == id);
        }

        private string FindItemId(string displayName)
        {
            var consumable = creatablesManager
                .GetAll<ConsumableItem>()
                .FirstOrDefault(i => i.data.name == displayName);
            if (consumable != null)
                return consumable.Id;

            var weapon = creatablesManager
                .GetAll<Weapon>()
                .FirstOrDefault(w => w.data.name == displayName);
            if (weapon != null)
                return weapon.Id;

            return creatablesManager
                .GetAll<Cosmetic>()
                .FirstOrDefault(c => c.data.name == displayName)
                ?.Id;
        }

        private string GetItemName(object item) =>
            item switch
            {
                ConsumableItem c => c.data.name,
                Weapon w => w.data.name,
                Cosmetic c => c.data.name,
                _ => "",
            };

        private void LoadItemSprite(object item, Image image)
        {
            string path = item switch
            {
                ConsumableItem c => c.data.spriteFilePath,
                Weapon w => w.data.spriteFilePath,
                Cosmetic c => c.data.category_sprites.Values.FirstOrDefault(v =>
                    !string.IsNullOrEmpty(v)
                ),
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
