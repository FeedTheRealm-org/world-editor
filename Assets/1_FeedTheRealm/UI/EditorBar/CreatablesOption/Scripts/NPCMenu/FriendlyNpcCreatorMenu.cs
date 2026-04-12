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
        private EditBuffer<NPCData> editBuffer;
        private bool isEditingNpc;
        private string currentDialogId = "";
        private const int MaxNpcNameLength = 25;
        private Dictionary<string, string> messageQuestAssignments = new();
        private NPCMessageItemBuilder messageItemBuilder;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField dialogDropdown;
        private ScrollView messagesScrollView;
        private Label messagesLabel;
        private Image spritePreview;
        private string currentSpritePath;
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
                null
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
            editBuffer = new EditBuffer<NPCData>(editingData);
            isEditingNpc = true;
            currentSpritePath = editingData.spriteFilePath;
            currentDialogId = editingData.npcDialog?.dialogId ?? string.Empty;

            messageQuestAssignments.Clear();
            if (editingData.npcDialog != null)
            {
                var questMap = editingData.npcDialog.GetMessageQuestMap();
                foreach (var kvp in questMap)
                    messageQuestAssignments[kvp.Key] = kvp.Value;
            }

            PopulateFields();
            BindEditMode();
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked -= SaveExistingNpc;
            saveButton.text = "Save NPC";
            saveButton.clicked += SaveExistingNpc;
        }

        private void PopulateFields()
        {
            if (editBuffer != null)
            {
                nameInput.value = editBuffer.Working.name;
                descriptionInput.value = editBuffer.Working.description;
            }
            LoadExistingSprite(currentSpritePath);

            if (string.IsNullOrEmpty(currentDialogId))
                return;

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.Id == currentDialogId);

            if (dialog == null)
                return;

            dialogDropdown.SetValueWithoutNotify(dialog.data.name);

            messageQuestAssignments.Clear();
            if (editBuffer != null && editBuffer.Working.npcDialog != null)
            {
                var questMap = editBuffer.Working.npcDialog.GetMessageQuestMap();
                foreach (var kvp in questMap)
                    messageQuestAssignments[kvp.Key] = kvp.Value;
            }
            else if (editingData != null && editingData.npcDialog != null)
            {
                var questMap = editingData.npcDialog.GetMessageQuestMap();
                foreach (var kvp in questMap)
                    messageQuestAssignments[kvp.Key] = kvp.Value;
            }

            LoadDialogMessages(dialog);
        }

        private void BindEditMode()
        {
            if (editBuffer == null)
                return;
            nameInput.RegisterValueChangedCallback(evt => editBuffer.Working.name = evt.newValue);
            descriptionInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.description = evt.newValue
            );
        }

        private void PopulateDialogDropdown()
        {
            var dialogs = creatablesManager.GetAll<Dialog>();
            dialogDropdown.choices = new List<string> { "None" }
                .Concat(dialogs.Select(d => d.data.name))
                .ToList();
            dialogDropdown.SetValueWithoutNotify("None");
        }

        private void OnDialogChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == "None" || string.IsNullOrEmpty(evt.newValue))
            {
                currentDialogId = "";
                messagesScrollView.style.display = DisplayStyle.None;
                messagesLabel.style.display = DisplayStyle.None;
                return;
            }

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.data.name == evt.newValue);

            if (dialog == null)
            {
                currentDialogId = "";
                return;
            }

            currentDialogId = dialog.Id;
            messageQuestAssignments.Clear();
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
                    currentSpritePath = paths[0];
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
            if (!string.IsNullOrEmpty(currentDialogId))
            {
                npcDialogData = new NPCDialogData(currentDialogId);
                npcDialogData.SetMessageQuestMap(messageQuestAssignments);
            }

            var npcData = new NPCData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                currentSpritePath ?? "",
                npcDialogData
            );

            creatablesManager.Add(new FriendlyNpc(npcData));
            ToastNotification.Show("Friendly NPC created successfully!", "success", Color.green);
            ReturnToList();
        }

        private void SaveExistingNpc()
        {
            if (!ValidateNpcName(out var error))
            {
                ToastNotification.Show($"Failed to save NPC: {error}", "error", Color.red);
                return;
            }

            if (editBuffer != null)
            {
                editBuffer.Working.spriteFilePath = currentSpritePath ?? string.Empty;

                if (!string.IsNullOrEmpty(currentDialogId))
                {
                    if (editBuffer.Working.npcDialog == null)
                        editBuffer.Working.npcDialog = new NPCDialogData(currentDialogId);
                    else
                        editBuffer.Working.npcDialog.dialogId = currentDialogId;

                    editBuffer.Working.npcDialog.SetMessageQuestMap(messageQuestAssignments);
                }
                else
                {
                    editBuffer.Working.npcDialog = null;
                }

                editBuffer.Commit();
            }

            ToastNotification.Show("Friendly NPC saved successfully!", "success", Color.green);
            ReturnToList();
        }

        private void ReturnToList()
        {
            OpenMenu(npcsMenuPrefab);
        }
    }
}
