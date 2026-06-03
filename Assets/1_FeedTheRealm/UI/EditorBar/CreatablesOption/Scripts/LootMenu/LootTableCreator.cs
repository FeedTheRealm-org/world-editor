using System;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.UI;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using static FTRShared.Runtime.Models.LootTableData;

namespace FeedTheRealm.UI.EditorBar.ElementOption.LootMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class LootTableCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject lootMenuPrefab;

        [SerializeField]
        private VisualTreeAsset lootItemTemplate;

        private LootTableData editingData;
        private List<LootEntryData> localEntries = new();

        private TextField nameInput;
        private DropdownField itemDropdown;
        private SliderInt probabilitySlider;
        private FloatField minGoldInput;
        private FloatField maxGoldInput;
        private ScrollView entriesScrollView;
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
            saveButton = root.Q<Button>("SaveButton");

            root.Q<Button>("AddItemButton").clicked += AddEntryToTable;
            root.Q<Button>("Return").clicked += ReturnToList;
            root.Q<Button>("Close").clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;

            PopulateItemDropdown();
        }

        public void SetupEditor(LootTable lootTable)
        {
            editingData = lootTable.data;
            localEntries = new List<LootEntryData>(editingData.lootItems);

            PopulateFields();
            BindEditMode();

            saveButton.clicked -= CreateNewObject;
            saveButton.text = "save";
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
                itemDropdown.value = itemDropdown.choices[0];
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

            foreach (var entry in localEntries.ToList())
            {
                var capturedEntry = entry;
                var ve = lootItemTemplate.Instantiate();

                ve.Q<Label>("ItemName").text = GetItemNameById(capturedEntry.id);
                ve.Q<Label>("ProbabilityLabel").text = $"{capturedEntry.dropProbability}%";

                ve.Q<Button>("DeleteButton").clicked += () =>
                {
                    localEntries.Remove(capturedEntry);
                    if (editingData != null)
                        editingData.lootItems = new List<LootEntryData>(localEntries);
                    RefreshEntriesList();
                };

                entriesScrollView.Add(ve);
            }
        }

        private List<ItemData> GetAllAvailableItems()
        {
            var list = new List<ItemData>();
            list.AddRange(creatablesManager.GetAll<ConsumableItem>().Select(c => c.data));
            list.AddRange(creatablesManager.GetAll<Weapon>().Select(w => w.data));
            return list;
        }

        private string GetItemNameById(string id) =>
            GetAllAvailableItems().FirstOrDefault(i => i.id == id)?.name ?? "Unknown Item";

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
            ToastNotification.Show("Loot table created successfully!", "success", Color.green);
            ReturnToList();
        }

        private void ReturnToList() => OpenMenu(lootMenuPrefab);
    }
}
