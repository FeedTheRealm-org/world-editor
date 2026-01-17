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
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button createMessageButton;
    public static string PendingDialogId;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        createMessageButton = root.Q<Button>("CreateMessage");

        createMessageButton.clicked += AddMessage;
        closeButton.clicked += CloseMenu;

        PopulateMessagesList();
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

            var editButton = entry.Q<Button>("Edit");
            var deleteButton = entry.Q<Button>("Delete");

            editButton.clicked += () => OnEditMessage(message);
            deleteButton.clicked += () => OnDeleteMessage(message, entry);

            messagesList.hierarchy.Add(entry);
        }
        // Clear pending filter after populating
        PendingDialogId = null;
    }

    void OnEditMessage(Message message)
    {
        logger.Log("Editing message: " + message.DisplayName, this, Logging.LogType.Info);
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
        OpenMenu(createMessageMenuPrefab);
    }
}
