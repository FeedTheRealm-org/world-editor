using System;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.WorldObjects.Items;
using FeedTheRealm.Core.WorldObjects.LootTable;
using FeedTheRealm.Gameplay.Library.CreatorObjectLibrary;
using Models;
using UnityEngine;
using UnityEngine.UIElements;
using LootEntryData = Models.LootTableData.LootEntryData;

[RequireComponent(typeof(UIDocument))]
public class LootTableCreatorMenuController : BaseCreatorMenuController<LootTable>
{
    private DropdownField itemListDropdown;
    private SliderInt itemProbabilitySlider;
    private FloatField minGoldDropAmountInput;
    private FloatField maxGoldDropAmountInput;
    private Button addItemButton;
    private Image itemSpritePreview;
    private ScrollView itemsScrollView;

    private List<LootEntryData> addedEntries = new();

    protected override CreatorObjectCategories Category => CreatorObjectCategories.LootTable;
    protected override string ObjectTypeName => "Loot Table";
    protected override string SaveButtonName => "SaveButton";

    protected override void InitializeSpecificFields(VisualElement root)
    {
        itemListDropdown = root.Q<DropdownField>("ItemList");
        LogIfNull(itemListDropdown, "ItemList dropdown");

        if (itemListDropdown != null)
        {
            PopulateItemDropdown();
            itemListDropdown.RegisterValueChangedCallback(OnItemSelected);
        }

        itemProbabilitySlider = root.Q<SliderInt>("ItemProbabilitySlider");
        LogIfNull(itemProbabilitySlider, "Item probability slider");

        if (itemProbabilitySlider != null)
        {
            itemProbabilitySlider.showInputField = true;
        }

        minGoldDropAmountInput = root.Q<FloatField>("MinGoldDropAmount");
        LogIfNull(minGoldDropAmountInput, "Min gold drop amount field");

        maxGoldDropAmountInput = root.Q<FloatField>("MaxGoldDropAmount");
        LogIfNull(maxGoldDropAmountInput, "Max gold drop amount field");

        addItemButton = root.Q<Button>("AddItemButton");
        itemSpritePreview = root.Q<Image>("SpritePreview");
        itemsScrollView = root.Q<ScrollView>("ItemsScrollView");

        RegisterButtonCallback(addItemButton, OnAddItemClicked);
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
            LootTableMenuUI.UpdateSpritePreview(itemSpritePreview, selectedItem.spriteFile);
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

    protected override void PopulateFields()
    {
        nameInput.value = currentObject.DisplayName;
        minGoldDropAmountInput.value = currentObject.minGoldDropAmount;
        maxGoldDropAmountInput.value = currentObject.maxGoldDropAmount;

        addedEntries = new List<LootEntryData>();
        if (currentObject.lootItems != null)
        {
            foreach (var entry in currentObject.lootItems)
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
            ToastNotification.Show(
                "No item selected. Create an item first.",
                "alert",
                Color.yellow
            );
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

        logger?.Log(
            $"Added item '{selectedItem.DisplayName}' with {itemProbabilitySlider.value}% probability",
            this,
            Logging.LogType.Info
        );

        RefreshItemsFoldout();
    }

    private void RefreshItemsFoldout()
    {
        if (itemsScrollView == null)
            return;

        itemsScrollView.Clear();
        foreach (var entry in addedEntries)
        {
            var displayName = GetItemDisplayNameById(creatorObjectLibrary, entry.id);
            var itemElement = LootTableMenuUI.CreateLootItemElement(
                entry,
                displayName,
                OnRemoveItemClicked
            );
            itemsScrollView.Add(itemElement);
        }
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

    protected override void CreateNewObject()
    {
        var lootTableData = new LootTableData(
            null,
            nameInput.value,
            (int)minGoldDropAmountInput.value,
            (int)maxGoldDropAmountInput.value,
            new List<LootEntryData>(addedEntries)
        );

        currentObject = new LootTable(lootTableData);
        creatorObjectLibrary.AddCreatable(Category, currentObject);
        logger?.Log(
            $"Created new loot table: {currentObject.DisplayName}",
            this,
            Logging.LogType.Info
        );
    }

    protected override void UpdateExistingObject()
    {
        currentObject.name = nameInput.value;
        currentObject.minGoldDropAmount = (int)minGoldDropAmountInput.value;
        currentObject.maxGoldDropAmount = (int)maxGoldDropAmountInput.value;
        currentObject.lootItems = new List<LootEntryData>(addedEntries);
        logger?.Log($"Updated loot table: {currentObject.DisplayName}", this, Logging.LogType.Info);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (itemListDropdown != null)
            itemListDropdown.UnregisterValueChangedCallback(OnItemSelected);
    }
}
