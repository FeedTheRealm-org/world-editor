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
    private GameObject shopMenuPrefab;

    private TextField nameField;
    private Button saveButton;
    private Button returnButton;
    private Button closeButton;

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        nameField = root.Q<TextField>("NameField");
        saveButton = root.Q<Button>("SaveButton");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToShopMenu;
        closeButton.clicked += CloseMenu;
    }

    private void OnSaveClicked()
    {
        string shopName = nameField?.value?.Trim();
        if (string.IsNullOrEmpty(shopName))
        {
            logger.Log("Shop name cannot be empty.", this, Logging.LogType.Warning);
            return;
        }

        shopManager.CreateShop(shopName);
        ReturnToShopMenu();
    }

    private void ReturnToShopMenu()
    {
        OpenMenu(shopMenuPrefab);
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToShopMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
    }
}
