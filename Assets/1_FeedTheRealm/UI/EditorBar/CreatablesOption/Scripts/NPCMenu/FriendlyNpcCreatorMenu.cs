using System;
using System.Collections.Generic;
using System.Linq;
using API;
using FeedTheRealm.Core.DataPersistence;
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

        [SerializeField]
        private VisualTreeAsset progressionItemTemplate;

        [SerializeField]
        private WorldSelector worldSelector;

        private NPCData editingData;
        private EditBuffer<NPCData> editBuffer;
        private const int MaxNpcNameLength = 25;

        private List<NPCDialogData> dialogProgression = new();
        private NPCDialogProgressionItemBuilder progressionItemBuilder;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField dialogDropdown;
        private Button addDialogButton;
        private ScrollView progressionScrollView;
        private Label progressionLabel;
        private Image spritePreview;
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
            addDialogButton = root.Q<Button>("AddDialogButton");
            progressionScrollView = root.Q<ScrollView>("ProgressionScrollView");
            progressionLabel = root.Q<Label>("ProgressionLabel");
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

            progressionItemBuilder = new NPCDialogProgressionItemBuilder(
                progressionItemTemplate,
                creatablesManager,
                OnProgressionChanged
            );

            PopulateDialogDropdown();
            RegisterCallbacks();

            if (editingData != null)
                SetupEditMode();
            else
                SetupCreateMode();

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
                spritePreview.image = null;
        }

        private void SetupCreateMode()
        {
            dialogProgression.Clear();

            var newData = new NPCData(
                Guid.NewGuid().ToString(),
                "",
                "",
                new List<NPCDialogData>(),
                new Dictionary<string, string>()
            );
            editBuffer = new EditBuffer<NPCData>(newData);
            editBuffer.Working.category_sprites = new Dictionary<string, string>();

            RefreshProgressionUI();

            saveButton.text = "Create NPC";
            saveButton.clicked -= ReturnToList;
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked += CreateNewObject;
        }

        private void SetupEditMode()
        {
            editBuffer = new EditBuffer<NPCData>(editingData);
            editBuffer.Working.category_sprites =
                editingData.category_sprites != null
                    ? new Dictionary<string, string>(editingData.category_sprites)
                    : new Dictionary<string, string>();

            dialogProgression =
                editingData.dialogProgression != null
                    ? editingData.dialogProgression.Select(CloneDialogData).ToList()
                    : new List<NPCDialogData>();

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
            RefreshProgressionUI();
        }

        private void PopulateDialogDropdown()
        {
            var dialogs = creatablesManager.GetAll<Dialog>();
            dialogDropdown.choices = new List<string> { "None" }
                .Concat(dialogs.Select(d => d.data.name))
                .ToList();
            dialogDropdown.SetValueWithoutNotify("None");
        }

        private void RefreshProgressionUI()
        {
            progressionScrollView.Clear();

            bool hasEntries = dialogProgression.Count > 0;
            progressionScrollView.style.display = hasEntries
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            progressionLabel.style.display = hasEntries ? DisplayStyle.Flex : DisplayStyle.None;

            for (int i = 0; i < dialogProgression.Count; i++)
            {
                int idx = i;
                var item = progressionItemBuilder.Build(
                    dialogProgression[idx],
                    idx + 1,
                    onRemove: () => RemoveEntry(idx),
                    onMoveUp: () => MoveEntry(idx, -1),
                    onMoveDown: () => MoveEntry(idx, +1)
                );
                progressionScrollView.Add(item);
            }
        }

        private void AddProgressionEntry()
        {
            if (dialogDropdown.value == "None" || string.IsNullOrEmpty(dialogDropdown.value))
            {
                ToastNotification.Show("Select a dialog before adding.", "error", Color.red);
                return;
            }

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.data.name == dialogDropdown.value);

            if (dialog == null)
                return;

            if (dialogProgression.Any(e => e.dialogId == dialog.Id))
            {
                ToastNotification.Show(
                    "That dialog is already in the progression.",
                    "error",
                    Color.red
                );
                return;
            }

            dialogProgression.Add(new NPCDialogData(dialog.Id));
            RefreshProgressionUI();
            OnProgressionChanged();
        }

        private void RemoveEntry(int index)
        {
            if (index < 0 || index >= dialogProgression.Count)
                return;
            dialogProgression.RemoveAt(index);
            RefreshProgressionUI();
            OnProgressionChanged();
        }

        private void MoveEntry(int index, int direction)
        {
            int newIndex = index + direction;
            if (newIndex < 0 || newIndex >= dialogProgression.Count)
                return;

            var temp = dialogProgression[index];
            dialogProgression[index] = dialogProgression[newIndex];
            dialogProgression[newIndex] = temp;

            RefreshProgressionUI();
            OnProgressionChanged();
        }

        private void OnProgressionChanged()
        {
            if (editBuffer?.Working != null)
                editBuffer.Working.dialogProgression = dialogProgression;
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

        private bool ValidateProgression(out string error)
        {
            for (int i = 0; i < dialogProgression.Count; i++)
            {
                var entry = dialogProgression[i];

                if (!entry.HasQuestAssigned)
                {
                    entry.repeatableQuestCooldown = string.Empty;
                    entry.onQuestAcceptedDialogId = string.Empty;
                }

                bool isLast = i == dialogProgression.Count - 1;
                bool hasNext = !isLast;

                if (hasNext && !entry.HasQuestAssigned)
                {
                    var dialog = creatablesManager
                        .GetAll<Dialog>()
                        .FirstOrDefault(d => d.Id == entry.dialogId);
                    string name = dialog?.data.name ?? $"entry {i + 1}";
                    error =
                        $"Dialog \"{name}\" (position {i + 1}) has no quest assigned. "
                        + "A quest is required to advance to the next dialog.";
                    return false;
                }

                if (entry.IsRepeatable && hasNext)
                {
                    var dialog = creatablesManager
                        .GetAll<Dialog>()
                        .FirstOrDefault(d => d.Id == entry.dialogId);
                    string name = dialog?.data.name ?? $"entry {i + 1}";
                    error =
                        $"Dialog \"{name}\" (position {i + 1}) is marked as Repeatable "
                        + "but has dialogs after it. A repeatable dialog must be the last entry.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private void CreateNewObject()
        {
            if (!ValidateNpcName(out var nameError))
            {
                ToastNotification.Show($"Failed to save NPC: {nameError}", "error", Color.red);
                return;
            }

            if (!ValidateProgression(out var progError))
            {
                ToastNotification.Show($"Failed to save NPC: {progError}", "error", Color.red);
                return;
            }

            if (editBuffer != null)
            {
                editBuffer.Working.dialogProgression = dialogProgression;
                editBuffer.Commit();
                editBuffer.Original.category_sprites = new Dictionary<string, string>(
                    editBuffer.Working.category_sprites
                );

                creatablesManager.Add(new FriendlyNpc(editBuffer.Original));
            }

            ToastNotification.Show("Friendly NPC created successfully!", "success", Color.green);
            ReturnToList();
        }

        private void SaveExistingNpc()
        {
            if (!ValidateNpcName(out var nameError))
            {
                ToastNotification.Show($"Failed to save NPC: {nameError}", "error", Color.red);
                return;
            }

            if (!ValidateProgression(out var progError))
            {
                ToastNotification.Show($"Failed to save NPC: {progError}", "error", Color.red);
                return;
            }

            if (editBuffer != null)
            {
                editBuffer.Working.dialogProgression = dialogProgression;
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

        private void RegisterCallbacks()
        {
            if (closeButton != null)
                closeButton.clicked += ReturnToList;
            if (returnButton != null)
                returnButton.clicked += ReturnToList;
            if (editCharacterButton != null)
                editCharacterButton.clicked += OpenCharacterEditor;
            if (addDialogButton != null)
                addDialogButton.clicked += AddProgressionEntry;

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
            if (addDialogButton != null)
                addDialogButton.clicked -= AddProgressionEntry;

            if (saveButton != null)
            {
                saveButton.clicked -= CreateNewObject;
                saveButton.clicked -= ReturnToList;
                saveButton.clicked -= SaveExistingNpc;
            }

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

        private void OpenCharacterEditor()
        {
            if (!EnsureCharacterEditorInstance())
                return;

            characterEditorController.SetAssetsWorldId(worldSelector.selectedWorldId);

            characterEditorController.SetupWithCharacterInfo(
                BuildCharacterInfo(),
                SaveCharacterInfo
            );
            SetCharacterEditorVisible(true);
        }

        private void SaveCharacterInfo(Dictionary<string, string> categorySprites)
        {
            if (editBuffer != null)
                editBuffer.Working.category_sprites =
                    categorySprites ?? new Dictionary<string, string>();
            pendingPreviewRefresh = true;
        }

        private CharacterInfoResponse BuildCharacterInfo() =>
            new CharacterInfoResponse
            {
                character_name = (editBuffer?.Working.name ?? nameInput?.value) ?? string.Empty,
                character_bio =
                    (editBuffer?.Working.description ?? descriptionInput?.value) ?? string.Empty,
                category_sprites = new Dictionary<string, string>(
                    editBuffer?.Working.category_sprites ?? new Dictionary<string, string>()
                ),
            };

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

        private void SetCharacterPreviewVisible(bool v) => characterPreviewRenderer?.SetVisible(v);

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
            characterPreviewRenderer?.Dispose();
            characterPreviewRenderer = null;
        }

        private void SetSpritePreviewVisible(bool v)
        {
            if (spritePreview == null)
                return;
            spritePreview.style.visibility = v ? Visibility.Visible : Visibility.Hidden;
            spritePreview.style.opacity = v ? 1f : 0f;
            spritePreview.pickingMode = v ? PickingMode.Position : PickingMode.Ignore;
        }

        private void DestroyCharacterEditorInstance() =>
            CharacterEditorRuntimeUtility.DestroyCharacterEditorInstance(
                ref characterEditorInstance,
                ref characterEditorController
            );

        private static NPCDialogData CloneDialogData(NPCDialogData src)
        {
            var clone = new NPCDialogData(src.dialogId)
            {
                onQuestAcceptedDialogId = src.onQuestAcceptedDialogId,
                repeatableQuestCooldown = src.repeatableQuestCooldown,
            };
            clone.SetMessageQuestMap(src.GetMessageQuestMap());
            return clone;
        }
    }
}
