using System;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MessagesCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private Message currentMessage;

    public static string PendingDialogId;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject messageMenuPrefab;

    private TextField contentField;

    private Button saveButton;
    private Button returnButton;
    private Button closeButton;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        contentField = root.Q<TextField>("ContentField");
        if (contentField == null)
            logger.Log("ContentField not found in UI", this, Logging.LogType.Error);

        saveButton = root.Q<Button>("SaveMessage");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToMessagesMenu;
        closeButton.clicked += CloseMenu;

        if (currentMessage == null && EditContext.HasObjectToEdit())
        {
            currentMessage = EditContext.GetAndClearObjectToEdit<Message>();
            PendingDialogId = currentMessage.dialogId;
        }

        if (currentMessage != null)
        {
            PopulateFields();
        }
    }

    private void PopulateFields()
    {
        contentField.value = currentMessage.Content;
    }

    private void OnSaveClicked()
    {
        var dialogs = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog);
        var dialog = dialogs.Find(d => d.ObjectId == PendingDialogId) as Dialog;

        if (dialog == null)
        {
            logger.Log("No dialog selected to attach message to", this, Logging.LogType.Warning);
            return;
        }

        if (currentMessage == null)
        {
            currentMessage = new Message("", "", contentField.value, dialog.ObjectId);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.Message, currentMessage);
            logger.Log(
                $"Created new message for dialog {dialog.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        else
        {
            currentMessage.Content = contentField.value;
            currentMessage.dialogId = dialog.ObjectId;
            logger.Log(
                $"Updated message: {currentMessage.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }

        ReturnToMessagesMenu();
    }

    private void ReturnToMessagesMenu()
    {
        OpenMenu(messageMenuPrefab);
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToMessagesMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
    }
}
