using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MessagesMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GameObject createMessageMenuPrefab;

    [SerializeField]
    private GameObject dialogMenuPrefab;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button createMessageButton;
    private Button dialogBackButton;
    public static string PendingDialogId;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        createMessageButton = root.Q<Button>("CreateMessage");
        dialogBackButton = root.Q<Button>("BackToDialogs");

        createMessageButton.clicked += AddMessage;
        dialogBackButton.clicked += BackToDialogs;
        closeButton.clicked += CloseMenu;

        SetHeaderLabel();
        PopulateMessagesList();
    }

    private void SetHeaderLabel()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var headerLabel = root.Q<Label>("HeaderLabel");
        if (headerLabel != null)
        {
            string dialogName = "";
            if (!string.IsNullOrEmpty(PendingDialogId) && creatorObjectLibrary != null)
            {
                var dialog =
                    creatorObjectLibrary
                        .GetCreatables(CreatorObjectCategories.Dialog)
                        .Find(d => d.ObjectId == PendingDialogId) as Dialog;
                if (dialog != null)
                {
                    dialogName = dialog.DisplayName;
                    if (dialogName.Length > 20)
                        dialogName = dialogName.Substring(0, 20) + "...";
                }
            }
            headerLabel.text = string.IsNullOrEmpty(dialogName)
                ? "Messages"
                : $"Messages (Dialog: {dialogName})";
        }
    }

    private void PopulateMessagesList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var messagesList = root.Q<ListView>("MessagesList");
        messagesList.Clear();

        var allMessages = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Message);

        foreach (Message message in allMessages)
        {
            if (!string.IsNullOrEmpty(PendingDialogId) && message.dialogId != PendingDialogId)
                continue;
            VisualElement entry = itemListTemplate.Instantiate();
            var headerLabel = entry.Q<Label>("Header");
            headerLabel.text = message.DisplayName;

            var typeLabel = entry.Q<Label>("Type");
            if (typeLabel != null)
                typeLabel.text = "Message";

            var editButton = entry.Q<Button>("Edit");
            var deleteButton = entry.Q<Button>("Delete");

            editButton.clicked += () => OnEditMessage(message);
            deleteButton.clicked += () => OnDeleteMessage(message, entry);

            messagesList.hierarchy.Add(entry);
        }
    }

    void OnEditMessage(Message message)
    {
        logger.Log("Editing message: " + message.DisplayName, this, Logging.LogType.Info);
        EditContext.SetObjectToEdit(message);
        MessagesCreatorMenuController.PendingDialogId = PendingDialogId;
        OpenMenu(createMessageMenuPrefab);
    }

    void OnDeleteMessage(Message message, VisualElement entry)
    {
        logger.Log("Deleting message: " + message.DisplayName, this, Logging.LogType.Info);
        creatorObjectLibrary.RemoveCreatable(CreatorObjectCategories.Message, message);
        entry.RemoveFromHierarchy();
    }

    void OnDisable()
    {
        createMessageButton.clicked -= AddMessage;
        closeButton.clicked -= CloseMenu;
    }

    private void AddMessage()
    {
        logger.Log("Opening Create Message Menu", this, Logging.LogType.Info);
        MessagesCreatorMenuController.PendingDialogId = PendingDialogId;
        OpenMenu(createMessageMenuPrefab);
    }

    private void BackToDialogs()
    {
        logger.Log("Returning to Dialogs Menu", this, Logging.LogType.Info);
        OpenMenu(dialogMenuPrefab);
    }
}
