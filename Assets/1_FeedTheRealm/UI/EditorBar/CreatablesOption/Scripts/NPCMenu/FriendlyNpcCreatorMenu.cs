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
        private string selectedDialogId = string.Empty;
        private Dictionary<string, string> messageQuestAssignments = new();
        private Dictionary<string, string> pendingCategorySprites = new();
        private NPCMessageItemBuilder messageItemBuilder;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField dialogDropdown;
        private ScrollView messagesScrollView;
        private Label messagesLabel;
        private Image spritePreview;
        private Button editCharacterButton;
        private Button saveButton;
        private Button closeButton;

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

            characterEditorPrefab = CharacterEditorRuntimeUtility.ResolveCharacterEditorPrefab(
                this,
                characterEditorPrefab
            );
            CharacterEditorRuntimeUtility.HideEmbeddedCharacterEditors(this);
            SetSpritePreviewVisible(true);

            messageItemBuilder = new NPCMessageItemBuilder(
                creatablesManager,
                messageQuestAssignments,
                OnMessageQuestAssignmentsChanged
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
            closeButton.clicked += ReturnToList;
            editCharacterButton.clicked += OpenCharacterEditor;
            dialogDropdown.RegisterValueChangedCallback(OnDialogChanged);
            nameInput.RegisterValueChangedCallback(OnNameChanged);
            descriptionInput.RegisterValueChangedCallback(OnDescriptionChanged);
        }

        private void UnregisterCallbacks()
        {
            if (closeButton != null)
                closeButton.clicked -= ReturnToList;

            if (editCharacterButton != null)
                editCharacterButton.clicked -= OpenCharacterEditor;

            if (saveButton != null)
            {
                saveButton.clicked -= CreateNewObject;
                saveButton.clicked -= ReturnToList;
            }

            dialogDropdown?.UnregisterValueChangedCallback(OnDialogChanged);
            nameInput?.UnregisterValueChangedCallback(OnNameChanged);
            descriptionInput?.UnregisterValueChangedCallback(OnDescriptionChanged);
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            if (editingData != null)
                editingData.name = evt.newValue;
        }

        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            if (editingData != null)
                editingData.description = evt.newValue;
        }

        private void SetupCreateMode()
        {
            selectedDialogId = string.Empty;
            messageQuestAssignments.Clear();
            pendingCategorySprites.Clear();
            dialogDropdown.SetValueWithoutNotify("None");
            HideDialogMessages();

            saveButton.text = "Create NPC";
            saveButton.clicked -= ReturnToList;
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked += CreateNewObject;
        }

        private void SetupEditMode()
        {
            PopulateFields();

            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Return to List";
            saveButton.clicked -= ReturnToList;
            saveButton.clicked += ReturnToList;
        }

        private void PopulateFields()
        {
            nameInput.SetValueWithoutNotify(editingData.name);
            descriptionInput.SetValueWithoutNotify(editingData.description);

            if (
                editingData.npcDialog == null
                || string.IsNullOrEmpty(editingData.npcDialog.dialogId)
            )
            {
                selectedDialogId = string.Empty;
                dialogDropdown.SetValueWithoutNotify("None");
                HideDialogMessages();
                return;
            }

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.Id == editingData.npcDialog.dialogId);

            if (dialog == null)
            {
                selectedDialogId = string.Empty;
                dialogDropdown.SetValueWithoutNotify("None");
                HideDialogMessages();
                return;
            }

            dialogDropdown.SetValueWithoutNotify(dialog.data.name);
            selectedDialogId = dialog.Id;

            messageQuestAssignments.Clear();
            var questMap = editingData.npcDialog.GetMessageQuestMap();
            foreach (var kvp in questMap)
                messageQuestAssignments[kvp.Key] = kvp.Value;

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
            // Dialog changed explicitly by user, clear stale assignments before reloading messages.
            messageQuestAssignments.Clear();

            if (evt.newValue == "None" || string.IsNullOrEmpty(evt.newValue))
            {
                selectedDialogId = string.Empty;
                HideDialogMessages();

                if (editingData != null)
                    editingData.npcDialog = null;

                return;
            }

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.data.name == evt.newValue);

            if (dialog == null)
            {
                selectedDialogId = string.Empty;
                HideDialogMessages();
                return;
            }

            selectedDialogId = dialog.Id;

            if (editingData != null)
            {
                if (editingData.npcDialog == null)
                    editingData.npcDialog = new NPCDialogData(dialog.Id);
                else
                    editingData.npcDialog.dialogId = dialog.Id;

                editingData.npcDialog.SetMessageQuestMap(messageQuestAssignments);
            }

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
            if (editingData?.npcDialog == null)
                return;

            editingData.npcDialog.SetMessageQuestMap(messageQuestAssignments);
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
            if (editingData != null)
            {
                editingData.category_sprites = categorySprites ?? new Dictionary<string, string>();
                pendingPreviewRefresh = true;
                return;
            }

            pendingCategorySprites = categorySprites ?? new Dictionary<string, string>();
            pendingPreviewRefresh = true;
        }

        private CharacterInfoResponse BuildCharacterInfo()
        {
            var categorySprites = editingData?.category_sprites ?? pendingCategorySprites;
            if (categorySprites == null)
                categorySprites = new Dictionary<string, string>();

            return new CharacterInfoResponse
            {
                character_name = editingData?.name ?? nameInput?.value ?? string.Empty,
                character_bio = editingData?.description ?? descriptionInput?.value ?? string.Empty,
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

        private void CreateNewObject()
        {
            NPCDialogData npcDialogData = null;
            if (!string.IsNullOrEmpty(selectedDialogId))
            {
                npcDialogData = new NPCDialogData(selectedDialogId);
                npcDialogData.SetMessageQuestMap(messageQuestAssignments);
            }

            var categorySprites =
                editingData?.category_sprites
                ?? pendingCategorySprites
                ?? new Dictionary<string, string>();

            var npcData = new NPCData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                npcDialogData,
                categorySprites
            );

            creatablesManager.Add(new FriendlyNpc(npcData));
            ReturnToList();
        }

        private void ReturnToList()
        {
            SetCharacterEditorVisible(false);
            OpenMenu(npcsMenuPrefab);
        }
    }
}
