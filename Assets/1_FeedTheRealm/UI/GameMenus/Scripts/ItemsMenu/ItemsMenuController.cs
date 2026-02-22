using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ItemsMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GameObject createConsumableItemMenuPrefab;

    [SerializeField]
    private GameObject createWeaponItemMenuPrefab;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private List<CreatorObjectCategories> itemCategory;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button addConsumableItemButton;
    private Button addWeaponItemButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        addConsumableItemButton = root.Q<Button>("AddConsumableItem");
        addWeaponItemButton = root.Q<Button>("AddWeaponItem");

        addConsumableItemButton.clicked += AddConsumableItem;
        addWeaponItemButton.clicked += AddWeaponItem;
        closeButton.clicked += CloseMenu;

        PopulateItemsList();
    }

    private void PopulateItemsList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var itemsList = root.Q<ListView>("ItemsList");
        itemsList.Clear();

        foreach (CreatorObjectCategories category in itemCategory)
        {
            foreach (Item item in creatorObjectLibrary.GetCreatables(category))
            {
                VisualElement itemEntry = itemListTemplate.Instantiate();
                var headerLabel = itemEntry.Q<Label>("Header");
                headerLabel.text = item.DisplayName;

                var typeLabel = itemEntry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = category.GetDisplayName();

                var editButton = itemEntry.Q<Button>("Edit");
                var deleteButton = itemEntry.Q<Button>("Delete");

                editButton.clicked += () => OnEditItem(item);
                deleteButton.clicked += () => OnDeleteItem(item, itemEntry);

                itemsList.hierarchy.Add(itemEntry);
            }
        }
    }

    void OnEditItem(CreatorObject item)
    {
        logger.Log("Editing item: " + item.DisplayName, this, Logging.LogType.Info);

        EditContext.SetObjectToEdit(item);

        if (item is ConsumableItem)
        {
            OpenMenu(createConsumableItemMenuPrefab);
        }
        else if (item is WeaponItem)
        {
            OpenMenu(createWeaponItemMenuPrefab);
        }
        else
        {
            logger.Log($"Unknown item type: {item.GetType().Name}", this, Logging.LogType.Error);
        }
    }

    void OnDeleteItem(CreatorObject item, VisualElement itemListEntry)
    {
        logger.Log("Deleting item: " + item.DisplayName, this, Logging.LogType.Info);
        creatorObjectLibrary.RemoveCreatable(CreatorObjectCategories.ConsumableItem, item);
        itemListEntry.RemoveFromHierarchy();
    }

    void OnDisable()
    {
        addConsumableItemButton.clicked -= AddConsumableItem;
        addWeaponItemButton.clicked -= AddWeaponItem;
        closeButton.clicked -= CloseMenu;
    }

    private void AddConsumableItem()
    {
        logger.Log("Opening Create Consumable Item Menu", this, Logging.LogType.Info);
        OpenMenu(createConsumableItemMenuPrefab);
    }

    private void AddWeaponItem()
    {
        logger.Log("Opening Create Weapon Item Menu", this, Logging.LogType.Info);
        OpenMenu(createWeaponItemMenuPrefab);
    }
}
