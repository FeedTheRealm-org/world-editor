using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LootMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GameObject createLootTableMenuPrefab;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button addLootTableButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        addLootTableButton = root.Q<Button>("AddLootTable");

        addLootTableButton.clicked += AddLootTable;
        closeButton.clicked += CloseMenu;

        PopulateLootTablesList();
    }

    private void PopulateLootTablesList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var lootTablesList = root.Q<ListView>("LootTablesList");
        lootTablesList.Clear();

        foreach (
            LootTable item in creatorObjectLibrary.GetCreatables(CreatorObjectCategories.LootTable)
        )
        {
            VisualElement lootTableEntry = itemListTemplate.Instantiate();
            var headerLabel = lootTableEntry.Q<Label>("Header");
            headerLabel.text = item.DisplayName;

            var editButton = lootTableEntry.Q<Button>("Edit");
            var deleteButton = lootTableEntry.Q<Button>("Delete");

            editButton.clicked += () => OnEditItem(item);
            deleteButton.clicked += () => OnDeleteItem(item, lootTableEntry);

            lootTablesList.hierarchy.Add(lootTableEntry);
        }
    }

    void OnEditItem(CreatorObject lootTable)
    {
        logger.Log("Editing LootTable: " + lootTable.DisplayName, this, Logging.LogType.Info);
    }

    void OnDeleteItem(CreatorObject lootTable, VisualElement lootTableEntry)
    {
        logger.Log("Deleting LootTable: " + lootTable.DisplayName, this, Logging.LogType.Info);
        creatorObjectLibrary.RemoveCreatable(CreatorObjectCategories.LootTable, lootTable);
        lootTableEntry.RemoveFromHierarchy();
    }

    void OnDisable()
    {
        addLootTableButton.clicked -= AddLootTable;
        closeButton.clicked -= CloseMenu;
    }

    private void AddLootTable()
    {
        logger.Log("Opening Create Loot Table Menu", this, Logging.LogType.Info);
        OpenMenu(createLootTableMenuPrefab);
    }
}
