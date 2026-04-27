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

        private TextField shopNameField;
        private Button saveButton;
        private Button returnButton;

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
        private HashSet<string> _initialCosmeticIds = new HashSet<string>();

        private class GoldItemBinding
        {
            public ProductData product;
        }

        private class CosmeticItemBinding
        {
            public ProductData product;
            public Cosmetic cosmetic;

            public Action onDelete;
            public Action onEdit;
            public Action onConfirm;
        }

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            shopNameField = root.Q<TextField>("ShopName");
            saveButton = root.Q<Button>("Save");
            returnButton = root.Q<Button>("Return");

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

            returnButton.clicked += ReturnToList;
            saveButton.clicked += Save;
            root.Q<Button>("AddItem").clicked += OnAddGoldItemClicked;

            var addCosmeticItemBtn = root.Q<Button>("AddCosmeticItem");
            if (addCosmeticItemBtn != null)
                addCosmeticItemBtn.clicked += OnAddCosmeticItemClicked;

            SetupListViews();
            RefreshProductList();
            SwitchTab(true);
        }

        public void SetupEditor(Shop shop)
        {
            editingData = shop.data;
            _initialCosmeticIds.Clear();
            if (editingData?.products != null)
            {
                foreach (var p in editingData.products)
                {
                    if (p.IsCosmetic)
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
                if (!product.IsCosmetic)
                    continue;

                var cosmetic = creatablesManager
                    .GetAll<Cosmetic>()
                    .FirstOrDefault(c => c.Id == product.productId);
                if (cosmetic == null)
                    continue;

                cosmetic.data.category_prices[product.categoryName] = product.price;
                currentCosmeticIds.Add(product.productId);
            }

            foreach (var pastId in _initialCosmeticIds)
            {
                if (currentCosmeticIds.Contains(pastId))
                    continue;

                var cosmetic = creatablesManager
                    .GetAll<Cosmetic>()
                    .FirstOrDefault(c => c.Id == pastId);
                if (cosmetic != null)
                    cosmetic.data.category_prices.Clear();
            }
            _initialCosmeticIds = new HashSet<string>(currentCosmeticIds);

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

            bool alreadyAdded = editingData.products.Any(p =>
                p.productId == itemId && !p.IsCosmetic
            );
            if (alreadyAdded)
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

            var cosmetic = creatablesManager.GetAll<Cosmetic>().FirstOrDefault(c => c.Id == itemId);
            if (cosmetic == null)
                return;

            foreach (var kvp in cosmetic.data.category_sprites)
            {
                bool alreadyAdded = editingData.products.Any(p =>
                    p.productId == itemId && p.categoryName == kvp.Key
                );
                if (alreadyAdded)
                    continue;

                string defaultName = $"{cosmetic.data.name} - {kvp.Key}";
                editingData.products.Add(
                    new ProductData(
                        productId: itemId,
                        price: 0,
                        currency: CurrencyType.Gems,
                        displayName: defaultName,
                        categoryName: kvp.Key
                    )
                );
            }

            RefreshProductList();
            cosmeticItemSelector.SetValueWithoutNotify(ItemPlaceholder);
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
                cosmeticItemSelector.choices = choices;
                cosmeticItemSelector.SetValueWithoutNotify(ItemPlaceholder);
            }
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
                var ve = productItemTemplate.Instantiate();
                ve.style.marginBottom = 8;

                HideNameEditControls(ve);

                var binding = new GoldItemBinding();
                ve.userData = binding;

                var priceField = ve.Q<IntegerField>("ProductPrice");
                if (priceField != null)
                    priceField.RegisterValueChangedCallback(evt =>
                    {
                        if (binding.product != null)
                            binding.product.price = evt.newValue;
                    });

                ve.Q<Button>("Delete")
                    ?.RegisterCallback<ClickEvent>(_ =>
                    {
                        if (binding.product != null && editingData != null)
                        {
                            editingData.products.Remove(binding.product);
                            RefreshProductList();
                        }
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

                LoadItemSprite(item, ve.Q<Image>("Preview"));

                var priceField = ve.Q<IntegerField>("ProductPrice");
                priceField?.SetValueWithoutNotify(binding.product.price);
            };

            itemContainer.unbindItem = (ve, _) =>
            {
                if (ve.userData is GoldItemBinding binding)
                    binding.product = null;
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
                var ve = productItemTemplate.Instantiate();
                ve.style.marginBottom = 8;

                var binding = new CosmeticItemBinding();
                ve.userData = binding;

                var priceField = ve.Q<IntegerField>("ProductPrice");
                if (priceField != null)
                    priceField.RegisterValueChangedCallback(evt =>
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
                binding.cosmetic = creatablesManager
                    .GetAll<Cosmetic>()
                    .FirstOrDefault(c => c.Id == binding.product.productId);

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
                if (nameRow != null)
                    nameRow.style.display = DisplayStyle.Flex;
                if (nameEditRow != null)
                    nameEditRow.style.display = DisplayStyle.None;

                var priceField = ve.Q<IntegerField>("ProductPrice");
                priceField?.SetValueWithoutNotify(binding.product.price);

                LoadCosmeticSprite(
                    binding.cosmetic,
                    binding.product.categoryName,
                    ve.Q<Image>("Preview")
                );

                binding.product.currency = CurrencyType.Gems;

                binding.onEdit = () =>
                {
                    if (nameRow != null)
                        nameRow.style.display = DisplayStyle.None;
                    if (nameEditRow != null)
                        nameEditRow.style.display = DisplayStyle.Flex;
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
                    if (nameRow != null)
                        nameRow.style.display = DisplayStyle.Flex;
                    if (nameEditRow != null)
                        nameEditRow.style.display = DisplayStyle.None;
                };

                binding.onDelete = () =>
                {
                    if (editingData != null)
                    {
                        editingData.products.Remove(binding.product);
                        RefreshProductList();
                    }
                };
            };

            cosmeticItemContainer.unbindItem = (ve, _) =>
            {
                if (ve.userData is CosmeticItemBinding binding)
                {
                    binding.product = null;
                    binding.cosmetic = null;
                    binding.onEdit = null;
                    binding.onConfirm = null;
                    binding.onDelete = null;
                }
            };
        }

        private void RefreshProductList()
        {
            if (editingData == null)
                return;

            var goldProducts = editingData
                .products.Where(p =>
                    !p.IsCosmetic && (FindItem(p.productId) is ConsumableItem or Weapon)
                )
                .ToList();

            var cosmeticProducts = editingData.products.Where(p => p.IsCosmetic).ToList();

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

        private void LoadCosmeticSprite(Cosmetic cosmetic, string categoryName, Image image)
        {
            if (image == null)
                return;
            if (
                !cosmetic.data.category_sprites.TryGetValue(categoryName, out var path)
                || string.IsNullOrEmpty(path)
            )
                return;
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(path);
            if (sprite != null)
                image.sprite = sprite;
        }

        private void HideNameEditControls(VisualElement ve)
        {
            var nameRow = ve.Q<VisualElement>("NameRow");
            var nameEditRow = ve.Q<VisualElement>("NameEditRow");
            if (nameRow != null)
                nameRow.style.display = DisplayStyle.Flex;
            if (nameEditRow != null)
                nameEditRow.style.display = DisplayStyle.None;
            var editBtn = ve.Q<Button>("EditName");
            var confirmBtn = ve.Q<Button>("ConfirmName");
            if (editBtn != null)
                editBtn.style.display = DisplayStyle.None;
            if (confirmBtn != null)
                confirmBtn.style.display = DisplayStyle.None;
        }

        private void ReturnToList() => OpenMenu(shopsMenuPrefab);
    }
}
