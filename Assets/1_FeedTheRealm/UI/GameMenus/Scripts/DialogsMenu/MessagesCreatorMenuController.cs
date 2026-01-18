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
    private DropdownField dialogDropdown;

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

        dialogDropdown = root.Q<DropdownField>("DialogField");
        if (dialogDropdown == null)
            logger.Log("DialogField not found in UI", this, Logging.LogType.Error);

        saveButton = root.Q<Button>("SaveMessage");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToMessagesMenu;
        closeButton.clicked += CloseMenu;

        PopulateDialogDropdown();

        if (currentMessage != null)
        {
            PopulateFields();
        }
    }

    private void PopulateDialogDropdown()
    {
        var dialogs = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog);
        var names = dialogs.Select(d => d.DisplayName).ToList();
        dialogDropdown.choices = names;
        if (names.Count > 0)
        {
            if (!string.IsNullOrEmpty(PendingDialogId))
            {
                var pending = dialogs.Find(d => d.ObjectId == PendingDialogId);
                if (pending != null)
                {
                    dialogDropdown.value = pending.DisplayName;
                }
                else
                {
                    dialogDropdown.value = names[0];
                }
            }
            else
            {
                dialogDropdown.value = names[0];
            }
        }
        // clear after applying
        PendingDialogId = null;
    }

    private void PopulateFields()
    {
        contentField.value = currentMessage.Content;
        // attempt to select the dialog by id
        var dialogs = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog);
        var dlg = dialogs.Find(d => d.ObjectId == currentMessage.dialogId);
        if (dlg != null)
            dialogDropdown.value = dlg.DisplayName;
    }

    private void OnSaveClicked()
    {
        var selectedDialogName = dialogDropdown.value;
        var dialogs = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog);
        var dialog = dialogs.Find(d => d.DisplayName == selectedDialogName) as Dialog;

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
