//using FeedTheRealm.Core.WorldObjects.Shop;
using FeedTheRealm.UI.Common;
using UI.EditorBar.ElementOption.Scripts.ShopMenu;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ListShopsMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private ShopManagerSO shopManager;

    [SerializeField]
    private GameObject shopEditorMenuPrefab;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;

    private Button closeButton;
    private Button addShopButton;

    [SerializeField]
    private GameObject shopMenuControllerPrefab;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        closeButton = root.Q<Button>("Close");
        addShopButton = root.Q<Button>("AddNPC");

        closeButton.clicked += ReturnToShopMenu;
        addShopButton.clicked += AddShop;

        PopulateShopList();
    }

    private void PopulateShopList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var shopList = root.Q<ListView>("ShopList");
        shopList.Clear();

        // foreach (ShopObject shop in shopManager.GetShops())
        // {
        //     VisualElement entry = itemListTemplate.Instantiate();

        //     entry.Q<Label>("Header").text = shop.displayName;

        //     var typeLabel = entry.Q<Label>("Type");
        //     if (typeLabel != null)
        //         typeLabel.text = "Shop";

        //     var editButton = entry.Q<Button>("Edit");
        //     var deleteButton = entry.Q<Button>("Delete");

        //     editButton.clicked += () => OnEditShop(shop);
        //     deleteButton.clicked += () => OnDeleteShop(shop, entry);

        //     shopList.hierarchy.Add(entry);
        // }
    }

    // private void OnEditShop(ShopObject shop)
    // {
    //     logger.Log("Editing shop: " + shop.displayName, this, Logging.LogType.Info);
    //     ShopEditContext.SetShopToEdit(shop);
    //     OpenMenu(shopEditorMenuPrefab);
    // }

    // private void OnDeleteShop(ShopObject shop, VisualElement entry)
    // {
    //     logger.Log("Deleting shop: " + shop.displayName, this, Logging.LogType.Info);
    //     shopManager.DeleteShop(shop.id);
    //     entry.RemoveFromHierarchy();
    // }

    private void AddShop()
    {
        logger.Log("Opening Create Shop Menu", this, Logging.LogType.Info);
        OpenMenu(shopEditorMenuPrefab);
    }

    void OnDisable()
    {
        if (closeButton != null)
            closeButton.clicked -= ReturnToShopMenu;
        if (addShopButton != null)
            addShopButton.clicked -= AddShop;
    }

    private void ReturnToShopMenu()
    {
        OpenMenu(shopMenuControllerPrefab);
    }
}
