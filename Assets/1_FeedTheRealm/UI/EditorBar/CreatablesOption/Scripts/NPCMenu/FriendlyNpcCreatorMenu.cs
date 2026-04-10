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
        private const int MaxNpcNameLength = 25;
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
                messageQuestAssignments,
                FlushToNpcDialog
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

            dialogDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "None" || string.IsNullOrEmpty(evt.newValue))
                {
                    editingData.npcDialog = null;
                    return;
                }

                var dialog = creatablesManager
                    .GetAll<Dialog>()
                    .FirstOrDefault(d => d.data.name == evt.newValue);

                if (dialog == null)
                    return;

                selectedDialogId = dialog.Id;

                if (editingData.npcDialog == null)
                    editingData.npcDialog = new NPCDialogData(dialog.Id);
                else
                    editingData.npcDialog.dialogId = dialog.Id;

                editingData.npcDialog.SetMessageQuestMap(messageQuestAssignments);
            });
        }

        private void PopulateDialogDropdown()
        {
            var dialogs = creatablesManager.GetAll<Dialog>();
            // unregister before setting choices to avoid spurious OnDialogChanged calls
            dialogDropdown.UnregisterValueChangedCallback(OnDialogChanged);
            dialogDropdown.choices = new List<string> { "None" }
                .Concat(dialogs.Select(d => d.data.name))
                .ToList();
            dialogDropdown.value = "None";
            dialogDropdown.RegisterValueChangedCallback(OnDialogChanged);
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
            {
                selectedDialogId = "";
                return;
            }

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

        private DropdownField CreateQuestDropdown(MessageData message, string currentQuestId)
        {
            var dropdown = new DropdownField();
            dropdown.AddToClassList("npc-quest-dropdown");
            dropdown.style.display = string.IsNullOrEmpty(currentQuestId)
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            PopulateQuestDropdown(dropdown, currentQuestId);

            dropdown.RegisterValueChangedCallback(evt =>
            {
                var selected = creatablesManager
                    .GetAll<Quest>()
                    .FirstOrDefault(q => q.data.title == evt.newValue);
                if (selected != null)
                {
                    messageQuestAssignments[message.id] = selected.Id;
                    FlushToNpcDialog(); // flush after every quest assignment change
                }
            });

            return dropdown;
        }

        private void PopulateQuestDropdown(DropdownField dropdown, string currentQuestId = "")
        {
            var quests = creatablesManager.GetAll<Quest>();
            dropdown.choices = quests.Select(q => q.data.title).ToList();

            if (!string.IsNullOrEmpty(currentQuestId))
            {
                var current = quests.FirstOrDefault(q => q.Id == currentQuestId);
                if (current != null)
                    dropdown.value = current.data.title;
            }
            else if (dropdown.choices.Count > 0)
                dropdown.value = dropdown.choices[0];
        }

        private Button CreateRemoveQuestButton(
            MessageData message,
            DropdownField questDropdown,
            Button addQuestButton,
            string currentQuestId
        )
        {
            var button = new Button { text = "✕" };
            button.AddToClassList("npc-remove-quest-button");

            button.clicked += () =>
            {
                questDropdown.style.display = DisplayStyle.Flex;
                button.style.display = DisplayStyle.None;

                var removeButton = button.parent?.Q<Button>();
                if (removeButton != null && removeButton.text == "✕")
                    removeButton.style.display = DisplayStyle.Flex;

                var initial = creatablesManager
                    .GetAll<Quest>()
                    .FirstOrDefault(q => q.data.title == questDropdown.value);
                if (initial != null)
                {
                    messageQuestAssignments[message.id] = initial.Id;
                    FlushToNpcDialog(); // flush on add too
                }
            };

            if (string.IsNullOrEmpty(currentQuestId))
                button.style.display = DisplayStyle.None;

            return button;
        }

        private void FlushToNpcDialog()
        {
            if (editingData?.npcDialog == null)
                return;
            editingData.npcDialog.SetMessageQuestMap(messageQuestAssignments);
        }

        private void LoadExistingSprite(string spritePath)
        {
            if (string.IsNullOrEmpty(spritePath))
                return;
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(spritePath);
            if (sprite != null)
                spritePreview.sprite = sprite;
        }

        private bool ValidateNpcName(out string error)
        {
            if (string.IsNullOrEmpty(nameInput.value))
            {
                error = "NPC name is required.";
                return false;
            }

            if (nameInput.value.Length > MaxNpcNameLength)
            {
                error = $"NPC name must be at most {MaxNpcNameLength} characters.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void CreateNewObject()
        {
            if (!ValidateNpcName(out var error))
            {
                ToastNotification.Show($"Failed to save NPC: {error}", "error", Color.red);
                return;
            }

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
                pendingSpritePath ?? "",
                npcDialogData
            );

            creatablesManager.Add(new FriendlyNpc(npcData));
            ReturnToList();
        }

        private void ReturnToList()
        {
            if (!ValidateNpcName(out var error))
            {
                ToastNotification.Show($"Failed to return: {error}", "error", Color.red);
                return;
            }

            OpenMenu(npcsMenuPrefab);
        }
    }
}
