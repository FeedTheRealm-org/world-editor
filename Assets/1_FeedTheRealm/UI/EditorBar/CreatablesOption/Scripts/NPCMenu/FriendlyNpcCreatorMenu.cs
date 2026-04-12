using System;
using System.Collections.Generic;
using System.Linq;
using API;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FeedTheRealm.UI.EditorBar.ElementOption.CharacterEditor;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
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

        [SerializeField]
        private GameObject characterEditorPrefab;

        private NPCData editingData;
        private EditBuffer<NPCData> editBuffer;
        private bool isEditingNpc;
        private string currentDialogId = "";
        private const int MaxNpcNameLength = 25;
        private string selectedDialogId = string.Empty;
        private Dictionary<string, string> messageQuestAssignments = new();
        private NPCMessageItemBuilder messageItemBuilder;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField dialogDropdown;
        private ScrollView messagesScrollView;
        private Label messagesLabel;
        private Image spritePreview;
        private string currentSpritePath;
        private Button editCharacterButton;
        private Button saveButton;
        private Button closeButton;
        private Button returnButton;

        private GameObject characterEditorInstance;
        private CharacterEditController characterEditorController;
        private CharacterEditorPreviewRenderer characterPreviewRenderer;
        private bool pendingPreviewRefresh;

        public void SetupEditor(FriendlyNpc npc)
        {
            editingData = npc.data;

            if (isActiveAndEnabled)
            {
                SetupEditMode();
                pendingPreviewRefresh = true;
                RefreshCharacterPreview();
            }
        }

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            nameInput = root.Q<TextField>("NameField");
            descriptionInput = root.Q<TextField>("DescriptionField");
            dialogDropdown = root.Q<DropdownField>("DialogDropdown");
            messagesScrollView = root.Q<ScrollView>("MessagesScrollView");
            messagesLabel = root.Q<Label>("MessagesLabel");
            spritePreview = root.Q<Image>("SpritePreview");
            editCharacterButton = root.Q<Button>("EditCharacter");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");
            returnButton = root.Q<Button>("Return");

            characterEditorPrefab = CharacterEditorRuntimeUtility.ResolveCharacterEditorPrefab(
                this,
                characterEditorPrefab
            );
            CharacterEditorRuntimeUtility.HideEmbeddedCharacterEditors(this);
            SetSpritePreviewVisible(true);

            messageItemBuilder = new NPCMessageItemBuilder(
                creatablesManager,
                messageQuestAssignments,
                null
            );

            PopulateDialogDropdown();
            RegisterCallbacks();

            if (editingData != null)
            {
                SetupEditMode();
            }
            else
            {
                SetupCreateMode();
            }

            pendingPreviewRefresh = true;
            RefreshCharacterPreview();
        }

        private void Update()
        {
            if (characterEditorInstance != null && !characterEditorInstance.activeSelf)
            {
                DestroyCharacterEditorInstance();
                SetCharacterPreviewVisible(true);
                SetSpritePreviewVisible(true);
            }

            if (pendingPreviewRefresh && characterEditorInstance == null)
            {
                RefreshCharacterPreview();
                pendingPreviewRefresh = false;
            }
        }

        void OnDisable()
        {
            UnregisterCallbacks();
            DestroyCharacterEditorInstance();
            DisposeCharacterPreviewRenderer();

            if (spritePreview != null)
            {
                spritePreview.image = null;
            }
        }

        private void RegisterCallbacks()
        {
            if (closeButton != null)
                closeButton.clicked += ReturnToList;

            if (returnButton != null)
                returnButton.clicked += ReturnToList;

            if (editCharacterButton != null)
                editCharacterButton.clicked += OpenCharacterEditor;
            dialogDropdown.RegisterValueChangedCallback(OnDialogChanged);
            nameInput.RegisterValueChangedCallback(OnNameChanged);
            descriptionInput.RegisterValueChangedCallback(OnDescriptionChanged);
        }

        private void UnregisterCallbacks()
        {
            if (closeButton != null)
                closeButton.clicked -= ReturnToList;

            if (returnButton != null)
                returnButton.clicked -= ReturnToList;

            if (editCharacterButton != null)
                editCharacterButton.clicked -= OpenCharacterEditor;

            if (saveButton != null)
            {
                saveButton.clicked -= CreateNewObject;
                saveButton.clicked -= ReturnToList;
                saveButton.clicked -= SaveExistingNpc;
            }

            dialogDropdown?.UnregisterValueChangedCallback(OnDialogChanged);
            nameInput?.UnregisterValueChangedCallback(OnNameChanged);
            descriptionInput?.UnregisterValueChangedCallback(OnDescriptionChanged);
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            if (editBuffer != null)
                editBuffer.Working.name = evt.newValue;
        }

        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            if (editBuffer != null)
                editBuffer.Working.description = evt.newValue;
        }

        private void SetupCreateMode()
        {
            selectedDialogId = string.Empty;
            messageQuestAssignments.Clear();

            var newNpcData = new NPCData(
                Guid.NewGuid().ToString(),
                "",
                "",
                null,
                new Dictionary<string, string>()
            );
            editBuffer = new EditBuffer<NPCData>(newNpcData);
            editBuffer.Working.category_sprites = new Dictionary<string, string>();

            dialogDropdown.SetValueWithoutNotify("None");
            HideDialogMessages();

            saveButton.text = "Create NPC";
            saveButton.clicked -= ReturnToList;
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked += CreateNewObject;
        }

        private void SetupEditMode()
        {
            editBuffer = new EditBuffer<NPCData>(editingData);
            if (editingData.category_sprites != null)
            {
                editBuffer.Working.category_sprites = new Dictionary<string, string>(
                    editingData.category_sprites
                );
            }
            else
            {
                editBuffer.Working.category_sprites = new Dictionary<string, string>();
            }

            isEditingNpc = true;
            currentDialogId = editingData.npcDialog?.dialogId ?? string.Empty;

            messageQuestAssignments.Clear();
            if (editingData.npcDialog != null)
            {
                var questMap = editingData.npcDialog.GetMessageQuestMap();
                foreach (var kvp in questMap)
                    messageQuestAssignments[kvp.Key] = kvp.Value;
            }

            PopulateFields();

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

            if (string.IsNullOrEmpty(currentDialogId))
            {
                dialogDropdown.SetValueWithoutNotify("None");
                HideDialogMessages();
                return;
            }

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.Id == currentDialogId);

            if (dialog == null)
            {
                selectedDialogId = string.Empty;
                dialogDropdown.SetValueWithoutNotify("None");
                HideDialogMessages();
                return;
            }

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
            messageQuestAssignments.Clear();

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

        private void HideDialogMessages()
        {
            messagesScrollView.Clear();
            messagesScrollView.style.display = DisplayStyle.None;
            messagesLabel.style.display = DisplayStyle.None;
        }

        private void OnMessageQuestAssignmentsChanged()
        {
            if (editBuffer?.Working.npcDialog == null)
                return;

            editBuffer.Working.npcDialog.SetMessageQuestMap(messageQuestAssignments);
        }

        private void OpenCharacterEditor()
        {
            if (!EnsureCharacterEditorInstance())
                return;

            var characterInfo = BuildCharacterInfo();

            characterEditorController.SetupWithCharacterInfo(characterInfo, SaveCharacterInfo);
            SetCharacterEditorVisible(true);
        }

        private void SaveCharacterInfo(Dictionary<string, string> categorySprites)
        {
            if (editBuffer != null)
            {
                editBuffer.Working.category_sprites =
                    categorySprites ?? new Dictionary<string, string>();
            }
            pendingPreviewRefresh = true;
        }

        private CharacterInfoResponse BuildCharacterInfo()
        {
            var categorySprites =
                editBuffer != null
                    ? editBuffer.Working.category_sprites
                    : new Dictionary<string, string>();

            return new CharacterInfoResponse
            {
                character_name =
                    (editBuffer != null ? editBuffer.Working.name : nameInput?.value)
                    ?? string.Empty,
                character_bio =
                    (editBuffer != null ? editBuffer.Working.description : descriptionInput?.value)
                    ?? string.Empty,
                category_sprites = new Dictionary<string, string>(categorySprites),
            };
        }

        private void RefreshCharacterPreview()
        {
            if (spritePreview == null)
                return;

            if (!EnsureCharacterPreviewRenderer())
            {
                spritePreview.image = null;
                return;
            }

            characterPreviewRenderer.Refresh(spritePreview, BuildCharacterInfo());
        }

        private bool EnsureCharacterPreviewRenderer()
        {
            if (characterPreviewRenderer != null)
                return true;

            characterEditorPrefab = CharacterEditorRuntimeUtility.ResolveCharacterEditorPrefab(
                this,
                characterEditorPrefab
            );

            if (characterEditorPrefab == null)
            {
                Debug.LogError("Character editor prefab is not assigned.", this);
                return false;
            }

            characterPreviewRenderer = new CharacterEditorPreviewRenderer(characterEditorPrefab);
            return true;
        }

        private void SetCharacterPreviewVisible(bool isVisible)
        {
            characterPreviewRenderer?.SetVisible(isVisible);
        }

        private bool EnsureCharacterEditorInstance()
        {
            if (characterEditorInstance != null && characterEditorController != null)
                return true;

            characterEditorPrefab = CharacterEditorRuntimeUtility.ResolveCharacterEditorPrefab(
                this,
                characterEditorPrefab
            );

            return CharacterEditorRuntimeUtility.TryInstantiateCharacterEditor(
                this,
                characterEditorPrefab,
                out characterEditorInstance,
                out characterEditorController
            );
        }

        private void SetCharacterEditorVisible(bool isVisible)
        {
            if (isVisible)
            {
                if (!EnsureCharacterEditorInstance())
                    return;

                SetCharacterPreviewVisible(false);
                SetSpritePreviewVisible(false);
                characterEditorInstance.SetActive(true);
                return;
            }

            if (characterEditorInstance != null)
            {
                characterEditorInstance.SetActive(false);
                DestroyCharacterEditorInstance();
            }

            SetCharacterPreviewVisible(true);
            SetSpritePreviewVisible(true);

            if (pendingPreviewRefresh)
            {
                RefreshCharacterPreview();
                pendingPreviewRefresh = false;
            }
        }

        private void DisposeCharacterPreviewRenderer()
        {
            if (characterPreviewRenderer == null)
                return;

            characterPreviewRenderer.Dispose();
            characterPreviewRenderer = null;
        }

        private void SetSpritePreviewVisible(bool isVisible)
        {
            if (spritePreview == null)
                return;

            spritePreview.style.visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            spritePreview.style.opacity = isVisible ? 1f : 0f;
            spritePreview.pickingMode = isVisible ? PickingMode.Position : PickingMode.Ignore;
        }

        private void DestroyCharacterEditorInstance()
        {
            CharacterEditorRuntimeUtility.DestroyCharacterEditorInstance(
                ref characterEditorInstance,
                ref characterEditorController
            );
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

            if (editBuffer != null)
            {
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
                editBuffer.Original.category_sprites = new Dictionary<string, string>(
                    editBuffer.Working.category_sprites
                );

                var npc = new FriendlyNpc(editBuffer.Original);
                creatablesManager.Add(npc);
            }

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
                editBuffer.Original.category_sprites = new Dictionary<string, string>(
                    editBuffer.Working.category_sprites
                );
            }

            ToastNotification.Show("Friendly NPC saved successfully!", "success", Color.green);
            ReturnToList();
        }

        private void ReturnToList()
        {
            SetCharacterEditorVisible(false);
            OpenMenu(npcsMenuPrefab);
        }
    }
}
