using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;
using LootEntryData = FTRShared.Runtime.Models.LootTableData.LootEntryData;

namespace FeedTheRealm.UI.EditorBar.ElementOption.LootMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class LootTableCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private Config config;

        [SerializeField]
        private GameObject lootMenuPrefab;

        private LootTableData editingData;
        private List<LootEntryData> localEntries = new();

        private TextField nameInput;
        private DropdownField itemDropdown;
        private SliderInt probabilitySlider;
        private FloatField minGoldInput;
        private FloatField maxGoldInput;
        private ScrollView entriesScrollView;
        private Image spritePreview;
        private Button saveButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            nameInput = root.Q<TextField>("NameField");
            itemDropdown = root.Q<DropdownField>("ItemList");
            probabilitySlider = root.Q<SliderInt>("ItemProbabilitySlider");
            minGoldInput = root.Q<FloatField>("MinGoldDropAmount");
            maxGoldInput = root.Q<FloatField>("MaxGoldDropAmount");
            entriesScrollView = root.Q<ScrollView>("ItemsScrollView");
            spritePreview = root.Q<Image>("SpritePreview");
            saveButton = root.Q<Button>("SaveButton");

            root.Q<Button>("AddItemButton").clicked += AddEntryToTable;
            root.Q<Button>("Return").clicked += ReturnToList;
            root.Q<Button>("Close").clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;

            itemDropdown.RegisterValueChangedCallback(evt => UpdateSpritePreview(evt.newValue));

            PopulateItemDropdown();
        }

        public void SetupEditor(LootTable lootTable)
        {
            editingData = lootTable.data;
            localEntries = new List<LootEntryData>(editingData.lootItems);

            PopulateFields();
            BindEditMode();

            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Return to List";
            saveButton.clicked += ReturnToList;
        }

        private void PopulateFields()
        {
            nameInput.value = editingData.name;
            minGoldInput.value = editingData.minGoldDropAmount;
            maxGoldInput.value = editingData.maxGoldDropAmount;
            RefreshEntriesList();
        }

        private void BindEditMode()
        {
            nameInput.RegisterValueChangedCallback(evt => editingData.name = evt.newValue);
            minGoldInput.RegisterValueChangedCallback(evt =>
                editingData.minGoldDropAmount = (int)evt.newValue
            );
            maxGoldInput.RegisterValueChangedCallback(evt =>
                editingData.maxGoldDropAmount = (int)evt.newValue
            );
        }

        private void PopulateItemDropdown()
        {
            var allItems = GetAllAvailableItems();
            itemDropdown.choices = allItems.Select(i => i.name).ToList();
            itemDropdown.userData = allItems;

            if (itemDropdown.choices.Count > 0)
            {
                itemDropdown.value = itemDropdown.choices[0];
                UpdateSpritePreview(itemDropdown.value);
            }
        }

        private void UpdateSpritePreview(string itemName)
        {
            var allItems = itemDropdown.userData as List<ItemData>;
            var item = allItems?.FirstOrDefault(i => i.name == itemName);

            if (item == null || string.IsNullOrEmpty(item.spriteFilePath))
            {
                spritePreview.sprite = null;
                return;
            }

            string fullPath = Path.Combine(config.SpritesDirectory, item.spriteFilePath);
            spritePreview.sprite = CustomFileBrowser.LoadSpriteFromDisk(fullPath);
        }

        private void AddEntryToTable()
        {
            var allItems = itemDropdown.userData as List<ItemData>;
            var selectedItem = allItems?.FirstOrDefault(i => i.name == itemDropdown.value);

            if (selectedItem == null)
                return;

            var newEntry = new LootEntryData(selectedItem.id, probabilitySlider.value);
            localEntries.Add(newEntry);

            if (editingData != null)
                editingData.lootItems = new List<LootEntryData>(localEntries);

            RefreshEntriesList();
        }

        private void RefreshEntriesList()
        {
            entriesScrollView.Clear();
            foreach (var entry in localEntries)
            {
                // Create a simple row container
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.paddingBottom = 5;
                row.style.paddingTop = 5;
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = Color.gray;

                // Item Name and Probability Label
                var label = new Label($"{GetItemNameById(entry.id)} ({entry.dropProbability}%)");
                label.style.color = Color.white;
                label.style.fontSize = 14;

                // Small "x" Remove Button
                var removeBtn = new Button(() =>
                {
                    localEntries.Remove(entry);
                    if (editingData != null)
                        editingData.lootItems = new List<LootEntryData>(localEntries);
                    RefreshEntriesList();
                });

                // Style the 'x' button to be small and simple
                removeBtn.style.width = 25;
                removeBtn.style.height = 25;
                removeBtn.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
                removeBtn.style.color = Color.white;

                row.Add(label);
                row.Add(removeBtn);
                entriesScrollView.Add(row);
            }
        }

        private List<ItemData> GetAllAvailableItems()
        {
            var list = new List<ItemData>();
            list.AddRange(creatablesManager.GetAll<ConsumableItem>().Select(c => c.data));
            list.AddRange(creatablesManager.GetAll<Weapon>().Select(w => w.data));
            return list;
        }

        private string GetItemNameById(string id)
        {
            var item = GetAllAvailableItems().FirstOrDefault(i => i.id == id);
            return item?.name ?? "Unknown Item";
        }

        private void CreateNewObject()
        {
            var tableData = new LootTableData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                (int)minGoldInput.value,
                (int)maxGoldInput.value,
                new List<LootEntryData>(localEntries)
            );

            creatablesManager.Add(new LootTable(tableData));
            ReturnToList();
        }

        private void ReturnToList() => OpenMenu(lootMenuPrefab);
    }
}
