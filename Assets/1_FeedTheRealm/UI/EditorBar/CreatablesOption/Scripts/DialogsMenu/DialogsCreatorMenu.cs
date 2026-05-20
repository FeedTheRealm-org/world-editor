using System;
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
    public class DialogCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject dialogsMenuPrefab;

        [SerializeField]
        private GameObject messageCreatorMenuPrefab;

        [SerializeField]
        private VisualTreeAsset messageItemTemplate;

        private Dialog currentDialog;
        private const int MaxDialogNameLength = 40;

        private TextField nameInput;
        private Button saveButton;
        private Button closeButton;
        private Button addMessageButton;
        private ScrollView messagesList;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            nameInput = root.Q<TextField>("NameField");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");
            addMessageButton = root.Q<Button>("CreateMessage");
            messagesList = root.Q<ScrollView>("MessagesList");

            root.Q<Button>("Return").clicked += ReturnToList;
            closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewDialog;
            addMessageButton.clicked += OpenMessageCreator;

            SetMessagesVisible(false);
            saveButton.style.display = DisplayStyle.Flex;
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            saveButton.clicked -= CreateNewDialog;
            addMessageButton.clicked -= OpenMessageCreator;
        }

        public void SetupEditor(Dialog dialog)
        {
            currentDialog = dialog;

            nameInput.value = dialog.data.name;
            nameInput.RegisterValueChangedCallback(evt => dialog.data.name = evt.newValue);

            saveButton.style.display = DisplayStyle.None;
            SetMessagesVisible(true);
            PopulateMessagesList();
        }

        // ── Validation ────────────────────────────────────────────────────────

        private bool ValidateDialogName(out string error)
        {
            if (string.IsNullOrEmpty(nameInput.value))
            {
                error = "Dialog name is required.";
                return false;
            }
            if (nameInput.value.Length > MaxDialogNameLength)
            {
                error = $"Dialog name must be at most {MaxDialogNameLength} characters.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        // ── Create ────────────────────────────────────────────────────────────

        private void CreateNewDialog()
        {
            if (!ValidateDialogName(out var error))
            {
                ToastNotification.Show($"Failed to save dialog: {error}", "error", Color.red);
                return;
            }

            var dialog = new Dialog(new DialogData(Guid.NewGuid().ToString(), nameInput.value));
            creatablesManager.Add(dialog);

            logger.Log($"Created dialog: {dialog.data.name}", this, Logging.LogType.Info);
            ToastNotification.Show("Dialog created!", "success", Color.green);

            SetupEditor(dialog);
        }

        // ── Messages list ─────────────────────────────────────────────────────

        private void SetMessagesVisible(bool visible)
        {
            messagesList.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            addMessageButton.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void PopulateMessagesList()
        {
            messagesList.Clear();

            foreach (var message in currentDialog.data.messages)
            {
                var entry = messageItemTemplate.Instantiate();
                var capturedMessage = message;

                entry.Q<Label>("Header").text = capturedMessage.content;

                entry.Q<Button>("Edit").clicked += () => OpenMessageEditor(capturedMessage);
                entry.Q<Button>("Delete").clicked += () =>
                {
                    currentDialog.data.messages.Remove(capturedMessage);
                    entry.RemoveFromHierarchy();
                };

                messagesList.Add(entry);
            }
        }

        // ── Navigation ────────────────────────────────────────────────────────

        private void OpenMessageCreator()
        {
            var instance = resolver.Instantiate(messageCreatorMenuPrefab);
            instance.GetComponent<MessageCreatorMenu>().SetContext(currentDialog);
            Destroy(gameObject);
        }

        private void OpenMessageEditor(MessageData message)
        {
            var instance = resolver.Instantiate(messageCreatorMenuPrefab);
            instance.GetComponent<MessageCreatorMenu>().SetContext(currentDialog, message);
            Destroy(gameObject);
        }

        private void ReturnToList() => OpenMenu(dialogsMenuPrefab);
    }
}
