using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using LootEntryData = Models.LootTableData.LootEntryData;

[RequireComponent(typeof(UIDocument))]
public class LootTableCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private LootTable currentLootTable;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject lootMenuPrefab;

    private TextField nameInput;
    private DropdownField itemListDropdown;
    private SliderInt itemProbabilitySlider;
    private FloatField minGoldDropAmountInput;
    private FloatField maxGoldDropAmountInput;
    private Button addItemButton;
    private Button saveLootTableButton;
    private Button returnButton;
    private Button closeButton;
    private Image spritePreview;
    private Foldout itemsFoldout;

    private List<LootEntryData> addedEntries = new();

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        nameInput = root.Q<TextField>("NameField");
        if (nameInput == null)
            logger?.Log("Name input field not found in UI", this, Logging.LogType.Error);

        itemListDropdown = root.Q<DropdownField>("ItemList");
        if (itemListDropdown == null)
            logger?.Log("ItemList dropdown not found in UI", this, Logging.LogType.Error);
        else
        {
            PopulateItemDropdown();
            itemListDropdown.RegisterValueChangedCallback(OnItemSelected);
        }

        itemProbabilitySlider = root.Q<SliderInt>("ItemProbabilitySlider");
        if (itemProbabilitySlider == null)
            logger?.Log("Item probability slider not found in UI", this, Logging.LogType.Error);

        minGoldDropAmountInput = root.Q<FloatField>("MinGoldDropAmount");
        if (minGoldDropAmountInput == null)
            logger?.Log("Min gold drop amount field not found in UI", this, Logging.LogType.Error);

        maxGoldDropAmountInput = root.Q<FloatField>("MaxGoldDropAmount");
        if (maxGoldDropAmountInput == null)
            logger?.Log("Max gold drop amount field not found in UI", this, Logging.LogType.Error);

        addItemButton = root.Q<Button>("AddItemButton");
        saveLootTableButton = root.Q<Button>("SaveLootTableButton");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");
        spritePreview = root.Q<Image>("SpritePreview");
        itemsFoldout = root.Q<Foldout>();

        if (itemsFoldout != null)
        {
            itemsFoldout.text = "Added Items";
        }

        addItemButton.clicked += OnAddItemClicked;
        saveLootTableButton.clicked += OnSaveLootTableClicked;
        returnButton.clicked += ReturnToLootMenu;
        closeButton.clicked += CloseMenu;

        // Populate fields if editing existing loot table
        if (currentLootTable != null)
        {
            PopulateFields();
        }
    }

    private void PopulateItemDropdown()
    {
        var items = GetItems(creatorObjectLibrary);
        itemListDropdown.choices = items.Select(item => item.DisplayName).ToList();
        if (itemListDropdown.choices.Count > 0)
        {
            itemListDropdown.value = itemListDropdown.choices[0];
        }
    }

    private void OnItemSelected(ChangeEvent<string> evt)
    {
        var items = GetItems(creatorObjectLibrary);
        var selectedItem = items.FirstOrDefault(item => item.DisplayName == evt.newValue);
        if (selectedItem != null)
        {
            LootTableMenuUI.UpdateSpritePreview(spritePreview, selectedItem.spriteFile);
        }
    }

    public static LootEntryData CreateLootEntryDataFromObject(object entry)
    {
        if (entry is LootEntryData led)
        {
            return led;
        }

        if (entry is ItemData itemData)
        {
            return new LootEntryData(itemData.id, 0);
        }

        return null;
    }

    public static List<Item> GetItems(CreatorObjectLibrarySO library)
    {
        return library.GetAllCreatorObjects().OfType<Item>().ToList();
    }

    public static string GetItemDisplayNameById(CreatorObjectLibrarySO library, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return "<Missing Item Id>";

        var item = GetItems(library).FirstOrDefault(i => i.ObjectId == itemId);

        return item != null ? item.DisplayName : $"<Missing Item {itemId}>";
    }

    private void PopulateFields()
    {
        nameInput.value = currentLootTable.DisplayName;
        minGoldDropAmountInput.value = currentLootTable.minGoldDropAmount;
        maxGoldDropAmountInput.value = currentLootTable.maxGoldDropAmount;

        addedEntries = new List<LootEntryData>();
        if (currentLootTable.lootItems != null)
        {
            foreach (var entry in currentLootTable.lootItems)
            {
                var lootEntry = CreateLootEntryDataFromObject(entry);
                if (lootEntry != null)
                    addedEntries.Add(lootEntry);
            }
        }
        RefreshItemsFoldout();
    }

    private void OnAddItemClicked()
    {
        if (string.IsNullOrEmpty(itemListDropdown.value))
        {
            logger?.Log("No item selected", this, Logging.LogType.Warning);
            return;
        }

        var items = GetItems(creatorObjectLibrary);
        var selectedItem = items.FirstOrDefault(item => item.DisplayName == itemListDropdown.value);
        if (selectedItem == null)
        {
            logger?.Log("Selected item not found", this, Logging.LogType.Error);
            return;
        }

        var entry = new LootEntryData(selectedItem.ObjectId, itemProbabilitySlider.value);

        addedEntries.Add(entry);
        if (itemProbabilitySlider != null)
        {
            itemProbabilitySlider.showInputField = true;
        }

        logger?.Log(
            $"Added item '{selectedItem.DisplayName}' with {itemProbabilitySlider.value}% probability",
            this,
            Logging.LogType.Info
        );

        RefreshItemsFoldout();
    }

    private void RefreshItemsFoldout()
    {
        if (itemsFoldout == null)
            return;

        itemsFoldout.Clear();
        foreach (var entry in addedEntries)
        {
            var displayName = GetItemDisplayNameById(creatorObjectLibrary, entry.id);
            var itemElement = LootTableMenuUI.CreateLootItemElement(
                entry,
                displayName,
                OnRemoveItemClicked
            );
            itemsFoldout.Add(itemElement);
        }
        itemsFoldout.text = $"Added Items ({addedEntries.Count})";
    }

    private void OnRemoveItemClicked(LootEntryData entry)
    {
        addedEntries.Remove(entry);
        logger?.Log(
            $"Removed item with id '{entry.id}' from loot table",
            this,
            Logging.LogType.Info
        );
        RefreshItemsFoldout();
    }

    private void OnSaveLootTableClicked()
    {
        if (string.IsNullOrEmpty(nameInput.value))
        {
            logger?.Log("Loot table name is required", this, Logging.LogType.Warning);
            return;
        }

        var lootEntries = new List<LootEntryData>(addedEntries);

        if (currentLootTable == null)
        {
            var lootTableData = new LootTableData(
                null,
                nameInput.value,
                (int)minGoldDropAmountInput.value,
                (int)maxGoldDropAmountInput.value,
                lootEntries
            );

            currentLootTable = new LootTable(lootTableData);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.LootTable, currentLootTable);
            logger?.Log(
                $"Created new loot table: {currentLootTable.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        else
        {
            currentLootTable.name = nameInput.value;
            currentLootTable.minGoldDropAmount = (int)minGoldDropAmountInput.value;
            currentLootTable.maxGoldDropAmount = (int)maxGoldDropAmountInput.value;
            currentLootTable.lootItems = lootEntries;
            logger?.Log(
                $"Updated loot table: {currentLootTable.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }

        ReturnToLootMenu();
    }

    private void ReturnToLootMenu()
    {
        OpenMenu(lootMenuPrefab);
    }

    void OnDisable()
    {
        if (addItemButton != null)
            addItemButton.clicked -= OnAddItemClicked;
        if (saveLootTableButton != null)
            saveLootTableButton.clicked -= OnSaveLootTableClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToLootMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
        if (itemListDropdown != null)
            itemListDropdown.UnregisterValueChangedCallback(OnItemSelected);
    }
}
