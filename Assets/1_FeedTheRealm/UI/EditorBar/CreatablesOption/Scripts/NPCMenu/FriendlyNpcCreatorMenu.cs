using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.NPCMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class FriendlyNpcCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject npcsMenuPrefab;

        private NPCData editingData;
        private string selectedDialogId = "";
        private Dictionary<string, string> messageQuestAssignments = new();
        private NPCMessageItemBuilder messageItemBuilder;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField dialogDropdown;
        private ScrollView messagesScrollView;
        private Label messagesLabel;
        private Image spritePreview;
        private string pendingSpritePath;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            nameInput = root.Q<TextField>("NameField");
            descriptionInput = root.Q<TextField>("DescriptionField");
            dialogDropdown = root.Q<DropdownField>("DialogDropdown");
            messagesScrollView = root.Q<ScrollView>("MessagesScrollView");
            messagesLabel = root.Q<Label>("MessagesLabel");
            spritePreview = root.Q<Image>("SpritePreview");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");

            messageItemBuilder = new NPCMessageItemBuilder(
                creatablesManager,
                messageQuestAssignments
            );

            PopulateDialogDropdown();
            dialogDropdown.RegisterValueChangedCallback(OnDialogChanged);

            root.Q<Button>("LoadSprite").clicked += LoadSprite;
            root.Q<Button>("Return").clicked += ReturnToList;
            closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;
        }

        void OnDisable()
        {
            dialogDropdown?.UnregisterValueChangedCallback(OnDialogChanged);
        }

        public void SetupEditor(FriendlyNpc npc)
        {
            editingData = npc.data;
            PopulateFields();
            BindEditMode();
            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Return to List";
            saveButton.clicked += ReturnToList;
        }

        private void PopulateFields()
        {
            nameInput.value = editingData.name;
            descriptionInput.value = editingData.description;
            LoadExistingSprite(editingData.spriteFilePath);

            if (
                editingData.npcDialog == null
                || string.IsNullOrEmpty(editingData.npcDialog.dialogId)
            )
                return;

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.Id == editingData.npcDialog.dialogId);

            if (dialog == null)
                return;

            dialogDropdown.value = dialog.data.name;
            selectedDialogId = dialog.Id;

            messageQuestAssignments.Clear();
            var questMap = editingData.npcDialog.GetMessageQuestMap();
            foreach (var kvp in questMap)
                messageQuestAssignments[kvp.Key] = kvp.Value;

            LoadDialogMessages(dialog);
        }

        private void BindEditMode()
        {
            nameInput.RegisterValueChangedCallback(evt => editingData.name = evt.newValue);
            descriptionInput.RegisterValueChangedCallback(evt =>
                editingData.description = evt.newValue
            );
        }

        private void PopulateDialogDropdown()
        {
            var dialogs = creatablesManager.GetAll<Dialog>();
            dialogDropdown.choices = new List<string> { "None" }
                .Concat(dialogs.Select(d => d.data.name))
                .ToList();
            dialogDropdown.value = "None";
        }

        private void OnDialogChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == "None" || string.IsNullOrEmpty(evt.newValue))
            {
                selectedDialogId = "";
                messagesScrollView.style.display = DisplayStyle.None;
                messagesLabel.style.display = DisplayStyle.None;
                return;
            }

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.data.name == evt.newValue);

            if (dialog == null)
                return;

            selectedDialogId = dialog.Id;
            LoadDialogMessages(dialog);
        }

        private void LoadDialogMessages(Dialog dialog)
        {
            messagesScrollView.Clear();
            messagesScrollView.style.display = DisplayStyle.Flex;
            messagesLabel.style.display = DisplayStyle.Flex;

            foreach (var message in dialog.data.messages)
            {
                var item = messageItemBuilder.CreateMessageItem(message);
                messagesScrollView.Add(item);
            }
        }

        private void LoadSprite()
        {
            CustomFileBrowser.ShowFilePickerDialog(
                onSuccess: paths =>
                {
                    if (paths == null || paths.Length == 0)
                        return;
                    if (!paths[0].EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        return;
                    var sprite = CustomFileBrowser.LoadSpriteFromDisk(paths[0]);
                    if (sprite == null)
                        return;
                    spritePreview.sprite = sprite;
                    pendingSpritePath = paths[0];
                    if (editingData != null)
                        editingData.spriteFilePath = pendingSpritePath;
                },
                onCancel: () => { }
            );
        }

        private void LoadExistingSprite(string spritePath)
        {
            if (string.IsNullOrEmpty(spritePath))
                return;
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(spritePath);
            if (sprite != null)
                spritePreview.sprite = sprite;
        }

        private void CreateNewObject()
        {
            NPCDialogData npcDialogData = null;
            if (!string.IsNullOrEmpty(selectedDialogId))
            {
                npcDialogData = new NPCDialogData(selectedDialogId);
                npcDialogData.SetMessageQuestMap(messageQuestAssignments);
            }

            var npcData = new NPCData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                pendingSpritePath,
                npcDialogData
            );

            creatablesManager.Add(new FriendlyNpc(npcData));
            ReturnToList();
        }

        private void ReturnToList() => OpenMenu(npcsMenuPrefab);
    }
}
