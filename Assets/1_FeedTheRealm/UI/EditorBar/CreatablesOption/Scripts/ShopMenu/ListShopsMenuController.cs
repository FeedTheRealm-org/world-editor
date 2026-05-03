using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.CreatablesOption.Scripts.ShopMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ListShopsMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

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
        }

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
}
