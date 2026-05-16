using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.DialogsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class MessagesMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject createMessageMenuPrefab;

        [SerializeField]
        private GameObject dialogsMenuPrefab;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;

        [Inject]
        private CreatablesManager creatablesManager;

        private VisualElement root;
        private Button closeButton;
        private Button createMessageButton;
        private Button backToDialogsButton;

        private Dialog currentDialog;
        private MessageData editingMessage;

        public void SetDialog(Dialog dialog)
        {
            currentDialog = dialog;
            SetHeaderLabel(root);
            PopulateMessagesList(root);
        }

        void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            createMessageButton = root.Q<Button>("CreateMessage");
            backToDialogsButton = root.Q<Button>("BackToDialogs");

            createMessageButton.clicked += AddMessage;
            backToDialogsButton.clicked += BackToDialogs;
            closeButton.clicked += CloseMenu;
        }

        void OnDisable()
        {
            createMessageButton.clicked -= AddMessage;
            backToDialogsButton.clicked -= BackToDialogs;
            closeButton.clicked -= CloseMenu;
        }

        private void SetHeaderLabel(VisualElement root)
        {
            var headerLabel = root.Q<Label>("HeaderLabel");
            if (headerLabel == null)
                return;

            if (currentDialog != null)
            {
                var name = currentDialog.data.name;
                if (name.Length > 20)
                    name = name.Substring(0, 20) + "...";
                headerLabel.text = $"Messages (Dialog: {name})";
            }
            else
                headerLabel.text = "Messages";
        }

        private void PopulateMessagesList(VisualElement root)
        {
            var messagesList = root.Q<ListView>("MessagesList");
            messagesList.Clear();

            if (currentDialog == null)
                return;

            foreach (var message in currentDialog.data.messages)
            {
                var entry = itemListTemplate.Instantiate();

                var headerLabel = entry.Q<Label>("Header");
                var displayName = message.content ?? string.Empty;
                if (displayName.Length > 30)
                    displayName = displayName.Substring(0, 30) + "...";
                headerLabel.text = displayName;

                var typeLabel = entry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Message";

                entry.Q<Button>("Edit").clicked += () => OnEditMessage(message);
                entry.Q<Button>("Delete").clicked += () => OnDeleteMessage(message, entry);

                messagesList.hierarchy.Add(entry);
            }
        }

        private void OnEditMessage(MessageData message)
        {
            logger.Log("Editing message", this, Logging.LogType.Info);
            editingMessage = message;
            OpenMenu(createMessageMenuPrefab);
        }

        private void OnDeleteMessage(MessageData message, VisualElement entry)
        {
            var npcs = creatablesManager.GetAll<FriendlyNpc>();
            bool isInUse = false;
            foreach (var npc in npcs)
            {
                if (
                    System.Linq.Enumerable.Any(
                        npc.data.dialogProgression,
                        p =>
                            p.questAssignments != null
                            && System.Linq.Enumerable.Any(
                                p.questAssignments,
                                qa => qa.messageId == message.id
                            )
                    )
                )
                {
                    isInUse = true;
                    break;
                }
            }

            if (isInUse)
            {
                ToastNotification.Show(
                    "Cannot delete message: It has an active quest assignment in an NPC.",
                    "error",
                    Color.red
                );
                return;
            }

            logger.Log("Deleting message", this, Logging.LogType.Info);
            currentDialog.data.messages.Remove(message);
            entry.RemoveFromHierarchy();
        }

        private void AddMessage()
        {
            editingMessage = null;
            OpenMenu(createMessageMenuPrefab);
        }

        private void BackToDialogs()
        {
            OpenMenu(dialogsMenuPrefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);

            if (menuPrefab == createMessageMenuPrefab)
            {
                var creator = menuInstance.GetComponent<MessageCreatorMenu>();
                creator?.SetContext(currentDialog, editingMessage);
            }

            Destroy(gameObject);
        }
    }
}
