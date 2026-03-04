using System;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.WorldObjects.Dialogs;
using FeedTheRealm.Core.WorldObjects.NPCs;
using FeedTheRealm.UI.EditorBar.ElementOption.Base;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.NPCMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class NPCCreatorMenuController : BaseCreatorMenuController<GenericNPC>
    {
        private TextField descriptionInput;
        private DropdownField dialogDropdown;
        private ScrollView messagesScrollView;
        private Label messagesLabel;
        private string selectedDialogId = "";
        private Dictionary<string, string> messageQuestAssignments =
            new Dictionary<string, string>();
        private NPCMessageItemBuilder messageItemBuilder;

        protected override CreatorObjectCategories Category => CreatorObjectCategories.NPC;
        protected override string ObjectTypeName => "NPC";
        protected override string SaveButtonName => "SaveButton";
        protected override bool RequiresSprite => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (dialogDropdown != null)
            {
                dialogDropdown.RegisterValueChangedCallback(OnDialogChanged);
            }
        }

        protected override void InitializeSpecificFields(VisualElement root)
        {
            descriptionInput = root.Q<TextField>("DescriptionField");
            LogIfNull(descriptionInput, "Description input field");

            dialogDropdown = root.Q<DropdownField>("DialogDropdown");
            messagesScrollView = root.Q<ScrollView>("MessagesScrollView");
            messagesLabel = root.Q<Label>("MessagesLabel");

            messageItemBuilder = new NPCMessageItemBuilder(
                creatorObjectLibrary,
                logger,
                messageQuestAssignments
            );

            PopulateDialogDropdown();
        }

        protected override void PopulateFields()
        {
            nameInput.value = currentObject.name;
            descriptionInput.value = currentObject.description ?? "";

            LoadExistingSprite(currentObject.spriteFile);

            if (
                currentObject.npcDialog != null
                && !string.IsNullOrEmpty(currentObject.npcDialog.dialogId)
            )
            {
                var dialogId = currentObject.npcDialog.dialogId;

                var dialogs = creatorObjectLibrary
                    .GetCreatables(CreatorObjectCategories.Dialog)
                    .Cast<Dialog>()
                    .ToList();

                var npcDialog = dialogs.FirstOrDefault(d => d.ObjectId == dialogId);
                if (npcDialog != null && dialogDropdown != null)
                {
                    if (
                        dialogDropdown.choices != null
                        && dialogDropdown.choices.Contains(npcDialog.DisplayName)
                    )
                    {
                        dialogDropdown.value = npcDialog.DisplayName;
                        selectedDialogId = npcDialog.ObjectId;

                        messageQuestAssignments.Clear();
                        var questMap = currentObject.npcDialog.GetMessageQuestMap();
                        foreach (var kvp in questMap)
                        {
                            messageQuestAssignments[kvp.Key] = kvp.Value;
                        }

                        LoadDialogMessagesForEdit(selectedDialogId);

                        logger?.Log(
                            $"NPCCreator: Loaded dialog '{npcDialog.DisplayName}' with {messageQuestAssignments.Count} quest assignments for NPC '{currentObject.DisplayName}'",
                            this,
                            Logging.LogType.Info
                        );
                    }
                    else
                    {
                        logger?.Log(
                            $"NPCCreator: Dialog '{npcDialog.DisplayName}' not found in dropdown choices",
                            this,
                            Logging.LogType.Warning
                        );
                    }
                }
            }
        }

        private void LoadDialogMessagesForEdit(string dialogId)
        {
            if (messagesScrollView == null || messagesLabel == null)
                return;

            messagesScrollView.Clear();
            messagesScrollView.style.display = DisplayStyle.Flex;
            messagesLabel.style.display = DisplayStyle.Flex;

            var messages = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Message)
                .Cast<Message>()
                .Where(m => m.dialogId == dialogId)
                .ToList();

            logger?.Log(
                $"NPCCreator: Loading {messages.Count} messages for dialog {dialogId} in edit mode",
                this,
                Logging.LogType.Info
            );

            foreach (var message in messages)
            {
                var messageItem = messageItemBuilder.CreateMessageItem(message);
                messagesScrollView.Add(messageItem);
            }
        }

        protected override bool ValidateSpecificFields()
        {
            // NPC specific validation can go here if needed
            return true;
        }

        protected override void CreateNewObject()
        {
            string savedSpritePath = SaveSpriteIfNeeded();

            NPCDialogData npcDialogData = null;
            if (!string.IsNullOrEmpty(selectedDialogId))
            {
                npcDialogData = new NPCDialogData(selectedDialogId);
                npcDialogData.SetMessageQuestMap(messageQuestAssignments);

                logger?.Log(
                    $"NPCCreator: Created NPCDialogData with dialog ID {selectedDialogId} and {messageQuestAssignments.Count} quest assignments",
                    this,
                    Logging.LogType.Info
                );
            }

            var npcData = new NPCData(
                null,
                nameInput.value,
                descriptionInput.value ?? "",
                savedSpritePath,
                npcDialogData
            );
            currentObject = new GenericNPC(npcData);
            creatorObjectLibrary.AddCreatable(Category, currentObject);
            logger?.Log(
                $"Created new NPC: {currentObject.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }

        protected override void UpdateExistingObject()
        {
            string savedSpritePath = SaveSpriteIfNeeded();

            NPCDialogData npcDialogData = null;
            if (!string.IsNullOrEmpty(selectedDialogId))
            {
                npcDialogData = new NPCDialogData(selectedDialogId);
                npcDialogData.SetMessageQuestMap(messageQuestAssignments);

                logger?.Log(
                    $"NPCCreator: Updated NPCDialogData with dialog ID {selectedDialogId} and {messageQuestAssignments.Count} quest assignments",
                    this,
                    Logging.LogType.Info
                );
            }

            currentObject.name = nameInput.value;
            currentObject.description = descriptionInput.value;
            currentObject.npcDialog = npcDialogData;

            if (!string.IsNullOrEmpty(savedSpritePath))
            {
                currentObject.spriteFile = savedSpritePath;
            }

            logger?.Log($"Updated NPC: {currentObject.DisplayName}", this, Logging.LogType.Info);
        }

        private void PopulateDialogDropdown()
        {
            if (dialogDropdown == null || creatorObjectLibrary == null)
                return;

            var dialogs = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Dialog)
                .Cast<Dialog>()
                .ToList();

            dialogDropdown.choices = dialogs.Select(d => d.DisplayName).ToList();
            dialogDropdown.choices.Insert(0, "None");

            if (currentObject == null || currentObject.npcDialog == null)
            {
                dialogDropdown.value = "None";
            }

            logger?.Log(
                $"NPCCreator: Populated {dialogs.Count} dialogs",
                this,
                Logging.LogType.Info
            );
        }

        private void OnDialogChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == "None" || string.IsNullOrEmpty(evt.newValue))
            {
                selectedDialogId = "";
                if (messagesScrollView != null)
                    messagesScrollView.style.display = DisplayStyle.None;
                if (messagesLabel != null)
                    messagesLabel.style.display = DisplayStyle.None;
                return;
            }

            var dialogs = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Dialog)
                .Cast<Dialog>()
                .ToList();

            var selectedDialog = dialogs.FirstOrDefault(d => d.DisplayName == evt.newValue);
            if (selectedDialog != null)
            {
                selectedDialogId = selectedDialog.ObjectId;
                LoadDialogMessages(selectedDialogId);
            }
        }

        private void LoadDialogMessages(string dialogId)
        {
            if (messagesScrollView == null || messagesLabel == null)
                return;

            messagesScrollView.Clear();
            messagesScrollView.style.display = DisplayStyle.Flex;
            messagesLabel.style.display = DisplayStyle.Flex;

            var messages = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Message)
                .Cast<Message>()
                .Where(m => m.dialogId == dialogId)
                .ToList();

            logger?.Log(
                $"NPCCreator: Loading {messages.Count} messages for dialog {dialogId}",
                this,
                Logging.LogType.Info
            );

            foreach (var message in messages)
            {
                var messageItem = messageItemBuilder.CreateMessageItem(message);
                messagesScrollView.Add(messageItem);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (dialogDropdown != null)
                dialogDropdown.UnregisterValueChangedCallback(OnDialogChanged);
        }
    }
}
