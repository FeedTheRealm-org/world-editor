using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.CreatablesOption.Scripts.ShopMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ShopsMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject shopCreatorMenuPrefab;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;

        [Inject]
        private CreatablesManager creatablesManager;

        private Button closeButton;
        private Button addShopButton;
        private Shop editingShop;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addShopButton = root.Q<Button>("AddShop");

            closeButton.clicked += CloseMenu;
            addShopButton.clicked += AddShop;

            PopulateShopList();
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            addShopButton.clicked -= AddShop;
        }

        private void PopulateShopList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var shopList = root.Q<ListView>("ShopList");
            shopList.Clear();

            foreach (Shop shop in creatablesManager.GetAll<Shop>())
            {
                var entry = itemListTemplate.Instantiate();
                entry.Q<Label>("Header").text = shop.data.shopName;

                var typeLabel = entry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Shop";

                entry.Q<Button>("Edit").clicked += () => OnEditShop(shop);
                entry.Q<Button>("Delete").clicked += () => OnDeleteShop(shop, entry);

                shopList.hierarchy.Add(entry);
            }
        }

        private void OnEditShop(Shop shop)
        {
            logger.Log("Editing shop: " + shop.data.shopName, this, Logging.LogType.Info);
            editingShop = shop;
            OpenMenu(shopCreatorMenuPrefab);
        }

        private void OnDeleteShop(Shop shop, VisualElement entry)
        {
            logger.Log("Deleting shop: " + shop.data.shopName, this, Logging.LogType.Info);
            creatablesManager.Delete<Shop>(shop.data.id);
            entry.RemoveFromHierarchy();
        }

        private void AddShop()
        {
            editingShop = null;
            OpenMenu(shopCreatorMenuPrefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);
            if (editingShop != null && menuPrefab == shopCreatorMenuPrefab)
                menuInstance.GetComponent<ShopCreatorMenu>()?.SetupEditor(editingShop);
            Destroy(gameObject);
        }
    }
}
