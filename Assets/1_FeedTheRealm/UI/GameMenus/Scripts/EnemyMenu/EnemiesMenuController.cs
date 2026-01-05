using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EnemiesMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GameObject createItemMenuPrefab;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button addItemButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        addItemButton = root.Q<Button>("AddItem");

        addItemButton.clicked += AddItem;
        closeButton.clicked += CloseMenu;

        PopulateItemsList();
    }

    private void PopulateItemsList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var itemsList = root.Q<ListView>("ItemsList");
        itemsList.Clear();

        foreach (
            ConsumableItem item in creatorObjectLibrary.GetCreatables(
                CreatorObjectCategories.ConsumableItem
            )
        )
        {
            VisualElement itemEntry = itemListTemplate.Instantiate();
            var headerLabel = itemEntry.Q<Label>("Header");
            headerLabel.text = item.DisplayName;

            var editButton = itemEntry.Q<Button>("Edit");
            var deleteButton = itemEntry.Q<Button>("Delete");

            editButton.clicked += () => OnEditItem(item);
            deleteButton.clicked += () => OnDeleteItem(item, itemEntry);

            itemsList.hierarchy.Add(itemEntry);
        }
    }

    void OnEditItem(CreatorObject item)
    {
        logger.Log("Editing item: " + item.DisplayName, this, Logging.LogType.Info);
    }

    void OnDeleteItem(CreatorObject item, VisualElement itemListEntry)
    {
        logger.Log("Deleting item: " + item.DisplayName, this, Logging.LogType.Info);
        creatorObjectLibrary.RemoveCreatable(CreatorObjectCategories.ConsumableItem, item);
        itemListEntry.RemoveFromHierarchy();
    }

    void OnDisable()
    {
        addItemButton.clicked -= AddItem;
        closeButton.clicked -= CloseMenu;
    }

    private void AddItem()
    {
        logger.Log("Opening Create Item Menu", this, Logging.LogType.Info);
        OpenMenu(createItemMenuPrefab);
    }
}
