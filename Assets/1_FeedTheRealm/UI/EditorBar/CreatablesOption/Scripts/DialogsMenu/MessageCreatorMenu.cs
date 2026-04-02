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

            if (editingMessage != null)
            {
                PopulateFields();
                BindEditMode();
                saveButton.clicked -= CreateNewMessage;
                saveButton.text = "Return to List";
                saveButton.clicked += ReturnToList;
            }
        }

        private void PopulateFields()
        {
            contentField.value = editingMessage.content;
        }

        private void BindEditMode()
        {
            contentField.RegisterValueChangedCallback(evt => editingMessage.content = evt.newValue);
        }

        private void CreateNewMessage()
        {
            if (string.IsNullOrEmpty(contentField.value))
            {
                logger.Log("Message content is required.", this, Logging.LogType.Warning);
                return;
            }

            var message = new MessageData(
                Guid.NewGuid().ToString(),
                sender: "",
                content: contentField.value
            );

            currentDialog.data.messages.Add(message);
            logger.Log(
                $"Created new message for dialog: {currentDialog.data.name}",
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
