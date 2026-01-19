using System;
using System.IO;
using System.Linq;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public class DialogsCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private Dialog currentDialog;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject dialogMenuPrefab;

    private TextField nameInput;

    private Button saveButton;
    private Button returnButton;
    private Button closeButton;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // note: these if statements are helpful when debugging missing UI elements
        nameInput = root.Q<TextField>("NameField");
        if (nameInput == null)
            logger.Log("Name input field not found in UI", this, Logging.LogType.Error);

        saveButton = root.Q<Button>("SaveDialog");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToItemsMenu;
        closeButton.clicked += CloseMenu;

        // Populate fields if editing existing item
        if (currentDialog != null)
        {
            PopulateFields();
        }
    }

    private void PopulateFields()
    {
        nameInput.value = currentDialog.name;
    }

    private void OnSaveClicked()
    {
        if (currentDialog == null)
        {
            var dialogData = new DialogData("", nameInput.value, "");
            currentDialog = new Dialog(dialogData);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.Dialog, currentDialog);
            logger.Log($"Created new dialog: {currentDialog.name}", this, Logging.LogType.Info);
        }
        else
        {
            currentDialog.name = nameInput.value;
            logger.Log($"Updated dialog: {currentDialog.name}", this, Logging.LogType.Info);
        }
        ReturnToItemsMenu();
    }

    private void ReturnToItemsMenu()
    {
        OpenMenu(dialogMenuPrefab);
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToItemsMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
    }
}
