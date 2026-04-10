using System;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.DialogsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class MessageCreatorMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject messagesMenuPrefab;

        private Dialog currentDialog;
        private MessageData editingMessage;
        private const int MaxMessageLength = 90;
        private bool isEditingMessage;
        private string pendingContent;

        private TextField contentField;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            contentField = root.Q<TextField>("ContentField");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");

            root.Q<Button>("Return").clicked += ReturnToList;
            closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewMessage;
        }

        public void SetContext(Dialog dialog, MessageData message = null)
        {
            currentDialog = dialog;
            editingMessage = message;
            isEditingMessage = editingMessage != null;
            pendingContent = message?.content ?? string.Empty;

            PopulateFields();
            BindEditMode();

            saveButton.clicked -= CreateNewMessage;
            saveButton.clicked -= SaveExistingMessage;

            if (isEditingMessage)
            {
                saveButton.text = "Save Message";
                saveButton.clicked += SaveExistingMessage;
            }
            else
            {
                saveButton.text = "Save Message";
                saveButton.clicked += CreateNewMessage;
            }
        }

        private void PopulateFields()
        {
            contentField.value = pendingContent;
        }

        private void BindEditMode()
        {
            contentField.RegisterValueChangedCallback(evt => pendingContent = evt.newValue);
        }

        private bool ValidateMessageContent(out string error)
        {
            if (string.IsNullOrEmpty(contentField.value))
            {
                error = "Message content is required.";
                return false;
            }

            if (contentField.value.Length > MaxMessageLength)
            {
                error = $"Message content must be at most {MaxMessageLength} characters.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void CreateNewMessage()
        {
            if (!ValidateMessageContent(out var error))
            {
                ToastNotification.Show($"Failed to save message: {error}", "error", Color.red);
                return;
            }

            var message = new MessageData(
                Guid.NewGuid().ToString(),
                sender: "",
                content: pendingContent
            );

            currentDialog.data.messages.Add(message);
            logger.Log(
                $"Created new message for dialog: {currentDialog.data.name}",
                this,
                Logging.LogType.Info
            );
            ReturnToList();
        }

        private void SaveExistingMessage()
        {
            if (!ValidateMessageContent(out var error))
            {
                ToastNotification.Show($"Failed to save message: {error}", "error", Color.red);
                return;
            }

            editingMessage.content = pendingContent;
            logger.Log(
                $"Saved existing message for dialog: {currentDialog.data.name}",
                this,
                Logging.LogType.Info
            );
            ReturnToList();
        }

        private void ReturnToList()
        {
            var menuInstance = resolver.Instantiate(messagesMenuPrefab);
            menuInstance.GetComponent<MessagesMenu>()?.SetDialog(currentDialog);
            Destroy(gameObject);
        }
    }
}
