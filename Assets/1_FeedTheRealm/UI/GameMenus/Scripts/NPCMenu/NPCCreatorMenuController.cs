using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public class NPCCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GenericNPC currentNPC;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject npcMenuPrefab;

    private TextField nameInput;
    private TextField descriptionInput;
    private DropdownField dialogDropdown;
    private ScrollView messagesScrollView;
    private Label messagesLabel;
    private Button saveButton;
    private Button returnButton;
    private Button closeButton;
    private Button loadSpriteButton;
    private Image spritePreview;
    private string pendingSpriteSourcePath;
    private string selectedDialogId = "";
    private Dictionary<string, string> messageQuestAssignments = new Dictionary<string, string>();
    private NPCMessageItemBuilder messageItemBuilder;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        nameInput = root.Q<TextField>("NameField");
        if (nameInput == null)
            logger.Log("Name input field not found in UI", this, Logging.LogType.Error);
        descriptionInput = root.Q<TextField>("DescriptionField");
        if (descriptionInput == null)
            logger.Log("Description input field not found in UI", this, Logging.LogType.Error);

        dialogDropdown = root.Q<DropdownField>("DialogDropdown");
        messagesScrollView = root.Q<ScrollView>("MessagesScrollView");
        messagesLabel = root.Q<Label>("MessagesLabel");

        saveButton = root.Q<Button>("SaveNPC");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");
        spritePreview = root.Q<Image>("SpritePreview");

        var npcPreviewContainer = root.Q<VisualElement>("NPCPreviewContainer");
        if (npcPreviewContainer != null)
        {
            loadSpriteButton = npcPreviewContainer.Q<Button>();
        }

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToNPCsMenu;
        closeButton.clicked += CloseMenu;
        loadSpriteButton.clicked += LoadSprite;

        if (currentNPC == null)
        {
            currentNPC = EditContext.GetAndClearObjectToEdit<GenericNPC>();
        }

        messageItemBuilder = new NPCMessageItemBuilder(
            creatorObjectLibrary,
            logger,
            messageQuestAssignments
        );

        PopulateDialogDropdown();

        if (dialogDropdown != null)
        {
            dialogDropdown.RegisterValueChangedCallback(OnDialogChanged);
        }

        if (currentNPC != null)
        {
            PopulateFields();
        }
    }

    private void PopulateFields()
    {
        nameInput.value = currentNPC.name;
        descriptionInput.value = currentNPC.description ?? "";

        LoadExistingSprite(currentNPC.spriteFile);

        if (currentNPC.npcDialog != null && !string.IsNullOrEmpty(currentNPC.npcDialog.dialogId))
        {
            var dialogId = currentNPC.npcDialog.dialogId;

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

                    messageQuestAssignments = currentNPC.npcDialog.GetMessageQuestMap();

                    LoadDialogMessagesForEdit(selectedDialogId);

                    logger?.Log(
                        $"NPCCreator: Loaded dialog '{npcDialog.DisplayName}' with {messageQuestAssignments.Count} quest assignments for NPC '{currentNPC.DisplayName}'",
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

    private void LoadExistingSprite(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
            return;

        string absolutePath = spritePath;
        if (!Path.IsPathRooted(spritePath))
        {
            absolutePath = Path.Combine(Application.streamingAssetsPath, spritePath);
        }

        if (FileBrowserHelpers.FileExists(absolutePath))
        {
            Sprite sprite = FileHandler.LoadSpriteFromDisk(absolutePath);
            if (sprite != null)
            {
                spritePreview.sprite = sprite;
            }
            else
            {
                logger?.Log(
                    $"Failed to load sprite from: {absolutePath}",
                    this,
                    Logging.LogType.Warning
                );
            }
        }
        else
        {
            logger?.Log($"Sprite file not found at: {absolutePath}", this, Logging.LogType.Warning);
        }
    }

    private void OnSaveClicked()
    {
        if (string.IsNullOrEmpty(nameInput.value))
        {
            logger?.Log("NPC name is required", this, Logging.LogType.Warning);
            ToastNotification.Show("NPC name is required", "error", Color.red);
            return;
        }

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

        if (currentNPC == null)
        {
            var npcData = new NPCData(
                null,
                nameInput.value,
                descriptionInput.value ?? "",
                pendingSpriteSourcePath,
                npcDialogData
            );
            currentNPC = new GenericNPC(npcData);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.NPC, currentNPC);
            logger.Log($"Created new NPC: {currentNPC.DisplayName}", this, Logging.LogType.Info);
        }
        else
        {
            currentNPC.name = nameInput.value;
            currentNPC.description = descriptionInput.value;
            currentNPC.npcDialog = npcDialogData;
            if (!string.IsNullOrEmpty(pendingSpriteSourcePath))
            {
                currentNPC.spriteFile = pendingSpriteSourcePath;
            }
            logger.Log($"Updated NPC: {currentNPC.DisplayName}", this, Logging.LogType.Info);
        }

        ToastNotification.Show("NPC saved successfully", "success", Color.green);

        ReturnToNPCsMenu();
    }

    private void ReturnToNPCsMenu()
    {
        OpenMenu(npcMenuPrefab);
    }

    private void LoadSprite()
    {
        FileHandler.ShowFilePickerDialog(
            onSuccess: OnSpriteSelected,
            onCancel: () => logger.Log("Sprite selection canceled", this, Logging.LogType.Info)
        );
    }

    private void OnSpriteSelected(string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return;

        string sourcePath = paths[0];

        if (!sourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            logger.Log("Selected file is not a PNG", this, Logging.LogType.Warning);
            return;
        }

        Sprite sprite = FileHandler.LoadSpriteFromDisk(sourcePath);
        if (sprite == null)
        {
            logger.Log("Failed to load sprite for preview", this, Logging.LogType.Error);
            return;
        }

        spritePreview.sprite = sprite;
        pendingSpriteSourcePath = sourcePath;
        logger.Log("Sprite loaded for preview (not saved yet)", this, Logging.LogType.Info);
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToNPCsMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked -= LoadSprite;
        if (dialogDropdown != null)
            dialogDropdown.UnregisterValueChangedCallback(OnDialogChanged);
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

        dialogDropdown.value = "None";

        logger?.Log($"NPCCreator: Populated {dialogs.Count} dialogs", this, Logging.LogType.Info);
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
}
