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
using VContainer;

namespace FeedTheRealm.UI.EditorBar.CreatablesOption.Scripts.ShopMenu
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

        private TextField shopNameField;
        private VisualElement _tabGold;
        private VisualElement _tabCosmetic;
        private Label _tabGoldLabel;
        private Label _tabCosmeticLabel;
        private VisualElement _tabGoldContent;
        private VisualElement _tabCosmeticContent;
        private DropdownField itemSelector;
        private ListView itemContainer;
        private DropdownField cosmeticItemSelector;
        private ListView cosmeticItemContainer;

        private bool _isGoldTabActive = true;
        private const string ItemPlaceholder = "Select an item to add";
        private HashSet<string> _initialCosmeticIds = new();

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            shopNameField = root.Q<TextField>("ShopName");
            root.Q<Button>("Save").clicked += Save;
            root.Q<Button>("Return").clicked += ReturnToList;
            root.Q<Button>("AddItem").clicked += OnAddGoldItemClicked;
            root.Q<Button>("AddCosmeticItem")
                ?.RegisterCallback<ClickEvent>(_ => OnAddCosmeticItemClicked());

            _tabGold = root.Q<VisualElement>("TabGold");
            _tabCosmetic = root.Q<VisualElement>("TabCosmetic");
            _tabGoldLabel = root.Q<Label>("TabGoldLabel");
            _tabCosmeticLabel = root.Q<Label>("TabCosmeticLabel");
            _tabGoldContent = root.Q<VisualElement>("TabGoldContent");
            _tabCosmeticContent = root.Q<VisualElement>("TabCosmeticContent");

            _tabGold?.RegisterCallback<ClickEvent>(_ => SwitchTab(true));
            _tabCosmetic?.RegisterCallback<ClickEvent>(_ => SwitchTab(false));

            itemSelector = root.Q<DropdownField>("AddItemContaier");
            itemContainer = root.Q<ListView>("ItemContainer");
            cosmeticItemSelector = root.Q<DropdownField>("AddCosmeticItemContainer");
            cosmeticItemContainer = root.Q<ListView>("CosmeticItemContainer");

            _initialCosmeticIds.Clear();
            editingData = new ShopData { id = Guid.NewGuid().ToString(), shopName = "" };
            shopNameField.RegisterValueChangedCallback(evt => editingData.shopName = evt.newValue);

            SetupListViews();
            RefreshProductList();
            SwitchTab(true);
        }

        public void SetupEditor(Shop shop)
        {
            editingData = shop.data;
            _initialCosmeticIds =
                editingData?.products.Where(p => p.IsCosmetic).Select(p => p.productId).ToHashSet()
                ?? new HashSet<string>();

            shopNameField.value = editingData.shopName;
            shopNameField.RegisterValueChangedCallback(evt => editingData.shopName = evt.newValue);
            RefreshProductList();
        }

        private void SwitchTab(bool goldTab)
        {
            _isGoldTabActive = goldTab;

            _tabGoldContent?.SetDisplay(goldTab);
            _tabCosmeticContent?.SetDisplay(!goldTab);
            _tabGold?.EnableInClassList("editor-tab--active", goldTab);
            _tabCosmetic?.EnableInClassList("editor-tab--active", !goldTab);

            var inactiveColor = new StyleColor(new Color(1f, 1f, 1f, 0.45f));
            if (_tabGoldLabel != null)
                _tabGoldLabel.style.color = goldTab
                    ? new StyleColor(new Color(1f, 210f / 255f, 80f / 255f))
                    : inactiveColor;
            if (_tabCosmeticLabel != null)
                _tabCosmeticLabel.style.color = !goldTab
                    ? new StyleColor(new Color(200f / 255f, 180f / 255f, 1f))
                    : inactiveColor;

            PopulateDropdown(
                goldTab ? itemSelector : cosmeticItemSelector,
                goldTab
                    ? creatablesManager
                        .GetAll<ConsumableItem>()
                        .Select(i => i.data.name)
                        .Concat(creatablesManager.GetAll<Weapon>().Select(w => w.data.name))
                    : creatablesManager.GetAll<Cosmetic>().Select(c => c.data.name)
            );
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

            if (!creatablesManager.GetAll<Shop>().Any(s => s.Id == editingData.id))
                creatablesManager.Add(new Shop(editingData));

            var currentIds = new HashSet<string>();
            foreach (var product in editingData.products.Where(p => p.IsCosmetic))
            {
                var cosmetic = FindCosmetic(product.productId);
                if (cosmetic == null)
                    continue;
                cosmetic.data.category_prices[product.categoryName] = product.price;
                currentIds.Add(product.productId);
            }

            foreach (var pastId in _initialCosmeticIds.Where(id => !currentIds.Contains(id)))
            {
                var cosmetic = FindCosmetic(pastId);
                if (cosmetic != null)
                {
                    if (cosmetic.data.category_urls.Values.Contains(pastId))
                    {
                        var category = cosmetic
                            .data.category_urls.FirstOrDefault(x => x.Value == pastId)
                            .Key;
                        if (!string.IsNullOrEmpty(category))
                            cosmetic.data.category_prices.Remove(category);
                    }
                    else if (cosmetic.Id == pastId)
                    {
                        cosmetic.data.category_prices.Clear();
                    }
                }
            }

            _initialCosmeticIds = currentIds;
            ToastNotification.Show("Shop saved successfully!", "success", Color.green);
            ReturnToList();
        }

        private void OnAddGoldItemClicked()
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
            if (
                itemSelector == null
                || itemSelector.value == ItemPlaceholder
                || string.IsNullOrEmpty(itemSelector.value)
            )
            {
                ToastNotification.Show("Please select an item to add.", "error", Color.yellow);
                return;
            }

            string itemId = FindItemId(itemSelector.value);
            if (itemId == null)
                return;

            if (editingData.products.Any(p => p.productId == itemId && !p.IsCosmetic))
            {
                ToastNotification.Show("Item already in shop.", "error", Color.yellow);
                return;
            }

            editingData.products.Add(new ProductData(itemId, 0, CurrencyType.Gold));
            RefreshProductList();
            itemSelector.SetValueWithoutNotify(ItemPlaceholder);
        }

        private void OnAddCosmeticItemClicked()
        {
            if (cosmeticItemSelector == null || cosmeticItemSelector.value == ItemPlaceholder)
                return;

            string itemId = FindItemId(cosmeticItemSelector.value);
            if (itemId == null)
                return;

            var cosmetic = FindCosmetic(itemId);
            if (cosmetic == null)
                return;

            foreach (var kvp in cosmetic.data.category_sprites)
            {
                string productIdToUse =
                    cosmetic.data.category_urls != null
                    && cosmetic.data.category_urls.TryGetValue(kvp.Key, out var url)
                    && !string.IsNullOrEmpty(url)
                        ? url
                        : itemId;

                if (
                    editingData.products.Any(p =>
                        p.productId == productIdToUse && p.categoryName == kvp.Key
                    )
                )
                    continue;

                editingData.products.Add(
                    new ProductData(
                        productId: productIdToUse,
                        price: 0,
                        currency: CurrencyType.Gems,
                        displayName: $"{cosmetic.data.name} - {kvp.Key}",
                        categoryName: kvp.Key
                    )
                );
            }

            RefreshProductList();
            cosmeticItemSelector.SetValueWithoutNotify(ItemPlaceholder);
        }

        private void SetupListViews()
        {
            SetupGoldListView();
            SetupCosmeticListView();
        }

        private void SetupGoldListView()
        {
            if (itemContainer == null)
                return;

            itemContainer.fixedItemHeight = 120;
            itemContainer.selectionType = SelectionType.None;

            itemContainer.makeItem = () =>
            {
                var ve = MakeProductItem();
                HideNameEditControls(ve);

                var binding = new GoldItemBinding();
                ve.userData = binding;

                ve.Q<IntegerField>("ProductPrice")
                    ?.RegisterValueChangedCallback(evt =>
                    {
                        if (binding.product != null)
                            binding.product.price = evt.newValue;
                    });
                ve.Q<Button>("Delete")
                    ?.RegisterCallback<ClickEvent>(_ =>
                    {
                        if (binding.product == null || editingData == null)
                            return;
                        editingData.products.Remove(binding.product);
                        RefreshProductList();
                    });

                return ve;
            };

            itemContainer.bindItem = (ve, index) =>
            {
                if (editingData == null || index >= itemContainer.itemsSource.Count)
                    return;

                var binding = (GoldItemBinding)ve.userData;
                binding.product = (ProductData)itemContainer.itemsSource[index];
                binding.product.currency = CurrencyType.Gold;

                var item = FindItem(binding.product.productId);
                if (item == null)
                    return;

                var nameLabel = ve.Q<Label>("ProductName");
                if (nameLabel != null)
                    nameLabel.text = GetItemName(item);

                ShopSpriteLoader.LoadItemSprite(item, ve.Q<Image>("Preview"));
                ve.Q<IntegerField>("ProductPrice")?.SetValueWithoutNotify(binding.product.price);
            };

            itemContainer.unbindItem = (ve, _) =>
            {
                if (ve.userData is GoldItemBinding b)
                    b.product = null;
            };
        }

        private void SetupCosmeticListView()
        {
            if (cosmeticItemContainer == null)
                return;

            cosmeticItemContainer.fixedItemHeight = 120;
            cosmeticItemContainer.selectionType = SelectionType.None;

            cosmeticItemContainer.makeItem = () =>
            {
                var ve = MakeProductItem();
                ve.Q<DropdownField>("CurrencyType")?.SetDisplay(false);

                var binding = new CosmeticItemBinding();
                ve.userData = binding;

                ve.Q<IntegerField>("ProductPrice")
                    ?.RegisterValueChangedCallback(evt =>
                    {
                        if (binding.product != null)
                            binding.product.price = evt.newValue;
                    });
                ve.Q<Button>("EditName")
                    ?.RegisterCallback<ClickEvent>(_ => binding.onEdit?.Invoke());
                ve.Q<Button>("ConfirmName")
                    ?.RegisterCallback<ClickEvent>(_ => binding.onConfirm?.Invoke());
                ve.Q<Button>("Delete")
                    ?.RegisterCallback<ClickEvent>(_ => binding.onDelete?.Invoke());

                return ve;
            };

            cosmeticItemContainer.bindItem = (ve, index) =>
            {
                if (editingData == null || index >= cosmeticItemContainer.itemsSource.Count)
                    return;

                var binding = (CosmeticItemBinding)ve.userData;
                binding.product = (ProductData)cosmeticItemContainer.itemsSource[index];
                binding.cosmetic = FindCosmetic(binding.product.productId);
                if (binding.cosmetic == null)
                    return;

                var nameLabel = ve.Q<Label>("ProductName");
                var nameInput = ve.Q<TextField>("ProductNameInput");
                var nameRow = ve.Q<VisualElement>("NameRow");
                var nameEditRow = ve.Q<VisualElement>("NameEditRow");

                string resolvedName = string.IsNullOrEmpty(binding.product.displayName)
                    ? $"{binding.cosmetic.data.name} - {binding.product.categoryName}"
                    : binding.product.displayName;

                if (nameLabel != null)
                    nameLabel.text = resolvedName;
                if (nameInput != null)
                    nameInput.SetValueWithoutNotify(resolvedName);
                nameRow?.SetDisplay(true);
                nameEditRow?.SetDisplay(false);

                ve.Q<IntegerField>("ProductPrice")?.SetValueWithoutNotify(binding.product.price);
                ShopSpriteLoader.LoadCosmeticSprite(
                    binding.cosmetic,
                    binding.product.categoryName,
                    ve.Q<Image>("Preview")
                );
                binding.product.currency = CurrencyType.Gems;

                binding.onEdit = () =>
                {
                    nameRow?.SetDisplay(false);
                    nameEditRow?.SetDisplay(true);
                    if (nameInput != null)
                    {
                        nameInput.SetValueWithoutNotify(nameLabel?.text ?? "");
                        nameInput.Focus();
                    }
                };

                binding.onConfirm = () =>
                {
                    string newName = nameInput?.value ?? "";
                    binding.product.displayName = newName;
                    if (nameLabel != null)
                        nameLabel.text = newName;
                    nameRow?.SetDisplay(true);
                    nameEditRow?.SetDisplay(false);
                };

                binding.onDelete = () =>
                {
                    editingData?.products.Remove(binding.product);
                    RefreshProductList();
                };
            };

            cosmeticItemContainer.unbindItem = (ve, _) =>
            {
                cosmeticItemContainer.unbindItem = (ve, _) =>
                {
                    if (ve.userData is CosmeticItemBinding b)
                    {
                        b.product = null;
                        b.cosmetic = null;
                        b.onEdit = b.onConfirm = b.onDelete = null;
                    }
                };
            };
        }

        private void RefreshProductList()
        {
            if (editingData == null)
                return;

            if (itemContainer != null)
            {
                itemContainer.itemsSource = editingData
                    .products.Where(p =>
                        !p.IsCosmetic && FindItem(p.productId) is ConsumableItem or Weapon
                    )
                    .ToList();
                itemContainer.RefreshItems();
            }

            if (cosmeticItemContainer != null)
            {
                cosmeticItemContainer.itemsSource = editingData
                    .products.Where(p => p.IsCosmetic)
                    .ToList();
                cosmeticItemContainer.RefreshItems();
            }
        }

        private VisualElement MakeProductItem()
        {
            var ve = productItemTemplate.Instantiate();
            ve.style.marginBottom = 8;
            return ve;
        }

        private static void PopulateDropdown(DropdownField dropdown, IEnumerable<string> names)
        {
            var choices = new List<string> { ItemPlaceholder };
            choices.AddRange(names);
            dropdown.choices = choices;
            dropdown.SetValueWithoutNotify(ItemPlaceholder);
        }

        private static void HideNameEditControls(VisualElement ve)
        {
            ve.Q<VisualElement>("NameRow")?.SetDisplay(true);
            ve.Q<VisualElement>("NameEditRow")?.SetDisplay(false);
            ve.Q<Button>("EditName")?.SetDisplay(false);
            ve.Q<Button>("ConfirmName")?.SetDisplay(false);
        }

        private Cosmetic FindCosmetic(string id) =>
            creatablesManager
                .GetAll<Cosmetic>()
                .FirstOrDefault(c =>
                    c.Id == id
                    || (c.data.category_urls != null && c.data.category_urls.Values.Contains(id))
                );

        private object FindItem(string id) =>
            (object)creatablesManager.GetAll<ConsumableItem>().FirstOrDefault(i => i.Id == id)
            ?? (object)creatablesManager.GetAll<Weapon>().FirstOrDefault(w => w.Id == id)
            ?? creatablesManager
                .GetAll<Cosmetic>()
                .FirstOrDefault(c =>
                    c.Id == id
                    || (c.data.category_urls != null && c.data.category_urls.Values.Contains(id))
                );

        private string FindItemId(string displayName) =>
            creatablesManager
                .GetAll<ConsumableItem>()
                .FirstOrDefault(i => i.data.name == displayName)
                ?.Id
            ?? creatablesManager
                .GetAll<Weapon>()
                .FirstOrDefault(w => w.data.name == displayName)
                ?.Id
            ?? creatablesManager
                .GetAll<Cosmetic>()
                .FirstOrDefault(c => c.data.name == displayName)
                ?.Id;

        private static string GetItemName(object item) =>
            item switch
            {
                ConsumableItem c => c.data.name,
                Weapon w => w.data.name,
                Cosmetic c => c.data.name,
                _ => "",
            };

        private void ReturnToList() => OpenMenu(shopsMenuPrefab);
    }

    internal static class VisualElementExtensions
    {
        internal static void SetDisplay(this VisualElement ve, bool visible) =>
            ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
