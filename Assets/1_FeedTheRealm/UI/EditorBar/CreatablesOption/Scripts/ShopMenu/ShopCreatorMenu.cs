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
        private DropdownField itemSelector;
        private ListView itemContainer;
        private Button saveButton;
        private Button returnButton;

        private const string ItemPlaceholder = "Select an item to add";

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            shopNameField = root.Q<TextField>("ShopName");
            itemSelector = root.Q<DropdownField>("AddItemContaier");
            itemContainer = root.Q<ListView>("ItemContainer");
            saveButton = root.Q<Button>("Save");
            returnButton = root.Q<Button>("Return");
            var addItemButton = root.Q<Button>("AddItem");

            // initialize fresh data immediately so items can be added before saving
            editingData = new ShopData { id = Guid.NewGuid().ToString(), shopName = "" };

            shopNameField.RegisterValueChangedCallback(evt => editingData.shopName = evt.newValue);

            returnButton.clicked += ReturnToList;
            saveButton.clicked += Save;
            addItemButton.clicked += OnAddItemClicked;

            PopulateItemDropdown();
            SetupListView();
            RefreshProductList();
        }

        public void SetupEditor(Shop shop)
        {
            editingData = shop.data;
            shopNameField.value = editingData.shopName;
            shopNameField.RegisterValueChangedCallback(evt => editingData.shopName = evt.newValue);
            RefreshProductList();
        }

        private void Save()
        {
            string shopName = shopNameField.value?.Trim();
            if (string.IsNullOrEmpty(shopName))
            {
                logger.Log("Shop name cannot be empty.", this, Logging.LogType.Warning);
                return;
            }

            editingData.shopName = shopName;

            // only add to manager if this is a new shop (not already registered)
            var existing = creatablesManager
                .GetAll<Shop>()
                .FirstOrDefault(s => s.Id == editingData.id);
            if (existing == null)
                creatablesManager.Add(new Shop(editingData));

            ReturnToList();
        }

        private void OnAddItemClicked()
        {
            if (editingData == null)
            {
                logger.Log(
                    "Save the shop first before adding items.",
                    this,
                    Logging.LogType.Warning
                );
                return;
            }

            if (itemSelector.value == ItemPlaceholder || string.IsNullOrEmpty(itemSelector.value))
            {
                logger.Log("Select an item first.", this, Logging.LogType.Warning);
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
