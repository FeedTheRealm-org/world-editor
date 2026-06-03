using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.Core.Common.Config;
using FTR.UI;
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

        [Inject]
        private Config config;

        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject shopsMenuPrefab;

        [SerializeField]
        private VisualTreeAsset productItemTemplate;

        private EditBuffer<ShopData> editBuffer;
        private ShopData editingData => editBuffer?.Working;

        private TextField shopNameField;
        private VisualElement _tabGold;
        private VisualElement _tabCosmetic;
        private Label _tabGoldLabel;
        private Label _tabCosmeticLabel;
        private VisualElement _tabGoldContent;
        private VisualElement _tabCosmeticContent;
        private DropdownField itemSelector;
        private ScrollView itemContainer;
        private ScrollView cosmeticItemContainer;
        private DropdownField cosmeticItemSelector;

        private bool _isGoldTabActive = true;
        private const string ItemPlaceholder = "Select an item to add";
        private HashSet<string> _initialCosmeticIds = new();

        void OnEnable()
        {
            ShopSpriteLoader.SpritesBasePath = config.SpritesDirectory;

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
            itemContainer = root.Q<ScrollView>("ItemContainer");
            cosmeticItemSelector = root.Q<DropdownField>("AddCosmeticItemContainer");
            cosmeticItemContainer = root.Q<ScrollView>("CosmeticItemContainer");

            _initialCosmeticIds.Clear();
            editBuffer = new EditBuffer<ShopData>(
                new ShopData { id = Guid.NewGuid().ToString(), shopName = "" }
            );
            shopNameField.RegisterValueChangedCallback(evt =>
            {
                if (editingData != null)
                    editingData.shopName = evt.newValue;
            });

            RefreshProductList();
            SwitchTab(true);
        }

        public void SetupEditor(Shop shop)
        {
            editBuffer = new EditBuffer<ShopData>(shop.data);
            _initialCosmeticIds =
                editingData?.products.Where(p => p.IsCosmetic).Select(p => p.productId).ToHashSet()
                ?? new HashSet<string>();

            shopNameField.value = editingData.shopName;
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

            if (editingData.products.Any(p => p.currency == CurrencyType.Gems && p.price <= 0))
            {
                ToastNotification.Show(
                    "Error: there are cosmetics with price less than or equal to zero.",
                    "error",
                    Color.red
                );
                return;
            }

            editingData.shopName = shopName;

            if (editBuffer != null)
            {
                editBuffer.Commit();
            }

            if (!creatablesManager.GetAll<Shop>().Any(s => s.Id == editBuffer.Original.id))
                creatablesManager.Add(new Shop(editBuffer.Original));

            var currentIds = new HashSet<string>();
            foreach (var product in editingData.products.Where(p => p.IsCosmetic))
            {
                var cosmetic = FindCosmetic(product.productId);
                if (cosmetic == null)
                    continue;

                if (
                    cosmetic.data.categories.TryGetValue(
                        product.categoryName,
                        out var categoryEntry
                    )
                )
                {
                    categoryEntry.price = product.price;
                }

                cosmetic.data.OnBeforeSerialize();
                currentIds.Add(product.productId);
            }

            foreach (var pastId in _initialCosmeticIds.Where(id => !currentIds.Contains(id)))
            {
                var cosmetic = FindCosmetic(pastId);
                if (cosmetic != null)
                {
                    var categoryEntry = cosmetic.data.categories.FirstOrDefault(x =>
                        x.Value.url_id == pastId
                        || (string.IsNullOrEmpty(x.Value.url_id) && cosmetic.data.id == pastId)
                    );
                    if (categoryEntry.Key != null)
                    {
                        categoryEntry.Value.price = 0;
                    }
                    else if (cosmetic.Id == pastId)
                    {
                        foreach (var cat in cosmetic.data.categories.Values)
                        {
                            cat.price = 0;
                        }
                    }
                    cosmetic.data.OnBeforeSerialize();
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

            foreach (var kvp in cosmetic.data.categories)
            {
                if (kvp.Key == "EarringL")
                {
                    continue;
                }

                string productIdToUse = !string.IsNullOrEmpty(kvp.Value.url_id)
                    ? kvp.Value.url_id
                    : itemId;

                if (
                    editingData.products.Any(p =>
                        p.productId == productIdToUse && p.categoryName == kvp.Key
                    )
                )
                    continue;

                string displaySuffix = kvp.Key == "EarringR" ? "Earrings" : kvp.Key;

                editingData.products.Add(
                    new ProductData(
                        productId: productIdToUse,
                        price: 0,
                        currency: CurrencyType.Gems,
                        displayName: $"{cosmetic.data.name} - {displaySuffix}",
                        categoryName: kvp.Key
                    )
                );
            }

            RefreshProductList();
            cosmeticItemSelector.SetValueWithoutNotify(ItemPlaceholder);
        }

        private void RefreshProductList()
        {
            if (editingData == null)
                return;

            itemContainer?.Clear();
            cosmeticItemContainer?.Clear();

            foreach (var product in editingData.products.Where(p => !p.IsCosmetic))
            {
                var item = FindItem(product.productId);
                if (item == null)
                    continue;

                var ve = MakeProductItem();
                HideNameEditControls(ve);

                var capturedProduct = product;

                var nameLabel = ve.Q<Label>("ProductName");
                if (nameLabel != null)
                    nameLabel.text = GetItemName(item);

                ShopSpriteLoader.LoadItemSprite(item, ve.Q<Image>("Preview"));
                ve.Q<IntegerField>("ProductPrice")?.SetValueWithoutNotify(capturedProduct.price);
                ve.Q<IntegerField>("ProductPrice")
                    ?.RegisterValueChangedCallback(evt => capturedProduct.price = evt.newValue);

                ve.Q<Button>("Delete")
                    ?.RegisterCallback<ClickEvent>(_ =>
                    {
                        editingData.products.Remove(capturedProduct);
                        RefreshProductList();
                    });

                itemContainer?.Add(ve);
            }

            foreach (var product in editingData.products.Where(p => p.IsCosmetic))
            {
                var cosmetic = FindCosmetic(product.productId);
                if (cosmetic == null)
                    continue;

                var ve = MakeProductItem();
                var capturedProduct = product;

                var nameLabel = ve.Q<Label>("ProductName");
                var nameInput = ve.Q<TextField>("ProductNameInput");
                var nameRow = ve.Q<VisualElement>("NameRow");
                var nameEditRow = ve.Q<VisualElement>("NameEditRow");

                string resolvedName = string.IsNullOrEmpty(capturedProduct.displayName)
                    ? $"{cosmetic.data.name} - {capturedProduct.categoryName}"
                    : capturedProduct.displayName;

                if (nameLabel != null)
                    nameLabel.text = resolvedName;
                if (nameInput != null)
                    nameInput.SetValueWithoutNotify(resolvedName);
                nameRow?.SetDisplay(true);
                nameEditRow?.SetDisplay(false);

                ve.Q<IntegerField>("ProductPrice")?.SetValueWithoutNotify(capturedProduct.price);
                ve.Q<IntegerField>("ProductPrice")
                    ?.RegisterValueChangedCallback(evt => capturedProduct.price = evt.newValue);

                ShopSpriteLoader.LoadCosmeticSprite(
                    cosmetic,
                    capturedProduct,
                    ve.Q<Image>("Preview")
                );
                capturedProduct.currency = CurrencyType.Gems;

                ve.Q<Button>("EditName")
                    ?.RegisterCallback<ClickEvent>(_ =>
                    {
                        nameRow?.SetDisplay(false);
                        nameEditRow?.SetDisplay(true);
                        nameInput?.SetValueWithoutNotify(nameLabel?.text ?? "");
                        nameInput?.Focus();
                    });

                ve.Q<Button>("ConfirmName")
                    ?.RegisterCallback<ClickEvent>(_ =>
                    {
                        string newName = nameInput?.value ?? "";
                        capturedProduct.displayName = newName;
                        if (nameLabel != null)
                            nameLabel.text = newName;
                        nameRow?.SetDisplay(true);
                        nameEditRow?.SetDisplay(false);
                    });

                ve.Q<Button>("Delete")
                    ?.RegisterCallback<ClickEvent>(_ =>
                    {
                        editingData.products.Remove(capturedProduct);
                        RefreshProductList();
                    });

                cosmeticItemContainer?.Add(ve);
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
                    || (
                        c.data.categories != null
                        && c.data.categories.Values.Any(v => v.url_id == id)
                    )
                );

        private object FindItem(string id) =>
            (object)creatablesManager.GetAll<ConsumableItem>().FirstOrDefault(i => i.Id == id)
            ?? (object)creatablesManager.GetAll<Weapon>().FirstOrDefault(w => w.Id == id)
            ?? creatablesManager
                .GetAll<Cosmetic>()
                .FirstOrDefault(c =>
                    c.Id == id
                    || (
                        c.data.categories != null
                        && c.data.categories.Values.Any(v => v.url_id == id)
                    )
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
