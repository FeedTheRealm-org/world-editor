using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Dialogs;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class DialogsMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GameObject createDialogMenuPrefab;

    [SerializeField]
    private GameObject messagesMenuPrefab;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button addDialogButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        addDialogButton = root.Q<Button>("AddDialog");

        addDialogButton.clicked += AddDialog;
        closeButton.clicked += CloseMenu;

        PopulateDialogsList();
    }

    private void PopulateDialogsList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var dialogsList = root.Q<ListView>("DialogsList");
        dialogsList.Clear();

        foreach (
            Dialog dialog in creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog)
        )
        {
            VisualElement dialogEntry = itemListTemplate.Instantiate();
            var headerLabel = dialogEntry.Q<Label>("Header");
            headerLabel.text = dialog.DisplayName;

            var editButton = dialogEntry.Q<Button>("Edit");
            var editMessagesButton = dialogEntry.Q<Button>("EditMessages");
            var deleteButton = dialogEntry.Q<Button>("Delete");

            var typeLabel = dialogEntry.Q<Label>("Type");
            if (typeLabel != null)
                typeLabel.text = "Dialog";

            editButton.clicked += () => OnEditDialog(dialog);
            editMessagesButton.clicked += () => OnEditMessages(dialog);
            deleteButton.clicked += () => OnDeleteDialog(dialog, dialogEntry);

            dialogsList.hierarchy.Add(dialogEntry);
        }
    }

    void OnEditDialog(Dialog dialog)
    {
        EditContext.SetObjectToEdit(dialog);
        OpenMenu(createDialogMenuPrefab);
    }

    void OnEditMessages(Dialog dialog)
    {
        logger.Log(
            "Opening messages for dialog: " + dialog.DisplayName,
            this,
            Logging.LogType.Info
        );
        MessagesMenuController.PendingDialogId = dialog.ObjectId;
        OpenMenu(messagesMenuPrefab);
    }

    void OnDeleteDialog(Dialog dialog, VisualElement dialogListEntry)
    {
        logger.Log("Deleting dialog: " + dialog.DisplayName, this, Logging.LogType.Info);
        creatorObjectLibrary.RemoveCreatable(CreatorObjectCategories.Dialog, dialog);
        dialogListEntry.RemoveFromHierarchy();
    }

    void OnDisable()
    {
        addDialogButton.clicked -= AddDialog;
        closeButton.clicked -= CloseMenu;
    }

    private void AddDialog()
    {
        logger.Log("Opening Create Dialog Menu", this, Logging.LogType.Info);
        OpenMenu(createDialogMenuPrefab);
    }
}
