//using FeedTheRealm.Core.WorldObjects.Shop;
using FeedTheRealm.UI.Common;
using UI.EditorBar.ElementOption.Scripts.ShopMenu;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ShopCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private ShopManagerSO shopManager;

    [SerializeField]
    private GameObject listShopsMenuPrefab;

    private TextField nameField;
    private Button saveButton;
    private Button returnButton;
    private Button closeButton;

    //private ShopObject shopBeingEdited;

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        nameField = root.Q<TextField>("NameField");
        saveButton = root.Q<Button>("SaveButton");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToListMenu;
        closeButton.clicked += CloseMenu;

        // shopBeingEdited = ShopEditContext.GetAndClear();
        // if (shopBeingEdited != null)
        //     nameField?.SetValueWithoutNotify(shopBeingEdited.displayName);
    }

    private void OnSaveClicked()
    {
        string shopName = nameField?.value?.Trim();
        if (string.IsNullOrEmpty(shopName))
        {
            logger.Log("Shop name cannot be empty.", this, Logging.LogType.Warning);
            return;
        }

        // if (shopBeingEdited != null)
        //     shopBeingEdited.displayName = shopName;
        // else
        //     shopManager.CreateShop(shopName);

        ReturnToListMenu();
    }

    private void ReturnToListMenu()
    {
        OpenMenu(listShopsMenuPrefab);
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToListMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
    }
}
