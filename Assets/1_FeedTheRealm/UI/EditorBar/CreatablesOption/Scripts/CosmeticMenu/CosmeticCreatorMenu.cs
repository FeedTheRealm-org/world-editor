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
using Utils;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.CosmeticMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class CosmeticCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatblesManager;

        [SerializeField]
        private GameObject cosmeticMenuPrefab;

        private EditBuffer<CosmeticData> editBuffer;

        [SerializeField]
        private GameObject characterEditorPrefab;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField categoryInput;
        private Button loadSpriteButton;
        private Button closeButton;
        private Button saveButton;
        private Button returnButton;

        private Image spritePreview;
        private GameObject characterEditorInstance;
        private CharacterEditController characterEditor;
        private CharacterEditorPreviewRenderer characterPreviewRenderer;
        private bool pendingPreviewRefresh;

        public void SetupEditor(Cosmetic cosmetic)
        {
            editBuffer = new EditBuffer<CosmeticData>(cosmetic.data);

            if (cosmetic.data.category_sprites != null)
                editBuffer.Working.category_sprites = new Dictionary<string, string>(
                    cosmetic.data.category_sprites
                );
            else
                editBuffer.Working.category_sprites = new Dictionary<string, string>();

            if (isActiveAndEnabled)
            {
                SetupEditMode();
                pendingPreviewRefresh = true;
                RefreshCharacterPreview();
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            InitializeFields(root);

            characterEditorPrefab = CharacterEditorRuntimeUtility.ResolveCharacterEditorPrefab(
                this,
                characterEditorPrefab
            );
            CharacterEditorRuntimeUtility.HideEmbeddedCharacterEditors(this);

            SetCharacterEditorVisible(false);
            SetSpritePreviewVisible(true);

            if (editBuffer != null)
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

        private void OnDisable()
        {
            if (closeButton != null)
                closeButton.clicked -= ReturnToList;

            if (returnButton != null)
                returnButton.clicked -= ReturnToList;

            if (saveButton != null)
            {
                saveButton.clicked -= CreateNewObject;
                saveButton.clicked -= ReturnToList;
                saveButton.clicked -= SaveExistingCosmetic;
            }

            if (loadSpriteButton != null)
                loadSpriteButton.clicked -= LoadSprite;

            DestroyCharacterEditorInstance();
            DisposeCharacterPreviewRenderer();

            if (spritePreview != null)
            {
                spritePreview.image = null;
            }
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

        private void InitializeFields(VisualElement root)
        {
            nameInput = root.Q<TextField>("NameField");
            descriptionInput = root.Q<TextField>("DescriptionField");
            categoryInput = root.Q<DropdownField>("CategoryField");
            loadSpriteButton = root.Q<Button>("LoadSprite");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");
            returnButton = root.Q<Button>("Return");
            spritePreview = root.Q<Image>("SpritePreview");

            categoryInput.choices = CosmeticCategories.Groupings.Keys.ToList();
            categoryInput.value = CosmeticCategories.Groupings.Keys.First();

            closeButton.clicked += ReturnToList;
            if (returnButton != null)
                returnButton.clicked += ReturnToList;
        }

        private void LoadSprite()
        {
            CustomFileBrowser.ShowFilePickerDialog(
                onSuccess: paths =>
                {
                    if (paths == null || paths.Length == 0)
                        return;
                    if (!paths[0].EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        ToastNotification.Show(
                            "Selected file is not a valid PNG image.",
                            "error",
                            Color.red
                        );
                        return;
                    }

                    if (editBuffer != null && !string.IsNullOrEmpty(categoryInput.value))
                    {
                        if (
                            CosmeticCategories.Groupings.TryGetValue(
                                categoryInput.value,
                                out var parts
                            )
                        )
                        {
                            if (!EnsureCharacterPreviewRenderer())
                                return;

                            var testSprites = new Dictionary<string, string>();
                            foreach (var part in parts)
                            {
                                testSprites[part.ToString()] = paths[0];
                            }

                            if (characterPreviewRenderer.ValidateLocalOverrides(testSprites))
                            {
                                editBuffer.Working.category_sprites.Clear();
                                foreach (var part in parts)
                                {
                                    editBuffer.Working.category_sprites[part.ToString()] = paths[0];
                                }
                                pendingPreviewRefresh = true;
                                RefreshCharacterPreview();
                                ToastNotification.Show(
                                    "Sprite applied successfully!",
                                    "success",
                                    Color.green
                                );
                            }
                            else
                            {
                                ToastNotification.Show(
                                    "Invalid sprite format or dimensions for this category.",
                                    "error",
                                    Color.red
                                );
                            }
                        }
                    }
                },
                onCancel: () => Debug.Log("Sprite selection canceled.")
            );
        }

        private void SetupCreateMode()
        {
            var newCosmetic = new CosmeticData(
                Guid.NewGuid().ToString(),
                "",
                "",
                0f,
                new Dictionary<string, string>()
            );
            editBuffer = new EditBuffer<CosmeticData>(newCosmetic);
            editBuffer.Working.category_sprites = new Dictionary<string, string>();

            PopulateFields();
            BindEditMode();

            saveButton.text = "Create Cosmetic";
            saveButton.clicked -= SaveExistingCosmetic;
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked += CreateNewObject;

            loadSpriteButton.clicked -= LoadSprite;
            loadSpriteButton.clicked += LoadSprite;
        }

        private void SetupEditMode()
        {
            PopulateFields();
            BindEditMode();

            saveButton.clicked -= CreateNewObject;
            saveButton.clicked -= SaveExistingCosmetic;
            saveButton.text = "Save Cosmetic";
            saveButton.clicked += SaveExistingCosmetic;

            loadSpriteButton.clicked -= LoadSprite;
            loadSpriteButton.clicked += LoadSprite;
        }

        private void OpenCharacterEditor()
        {
            if (!EnsureCharacterEditorInstance())
                return;

            var characterInfo = BuildCharacterInfo();

            characterEditor.SetupWithCharacterInfo(characterInfo, SaveCharacterInfo);
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

        private void PopulateFields()
        {
            if (editBuffer != null)
            {
                nameInput.value = editBuffer.Working.name;
                descriptionInput.value = editBuffer.Working.description;
            }
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

        private void CreateNewObject()
        {
            if (!ValidateCosmeticFields(out var error))
            {
                ToastNotification.Show($"Failed to save cosmetic: {error}", "error", Color.red);
                return;
            }

            if (editBuffer != null)
            {
                editBuffer.Commit();
                editBuffer.Original.category_sprites = new Dictionary<string, string>(
                    editBuffer.Working.category_sprites
                );

                var cosmetic = new Cosmetic(editBuffer.Original);
                creatblesManager.Add(cosmetic);
            }

            ToastNotification.Show("Cosmetic created successfully!", "success", Color.green);
            ReturnToList();
        }

        private void SaveExistingCosmetic()
        {
            if (!ValidateCosmeticFields(out var error))
            {
                ToastNotification.Show($"Failed to save cosmetic: {error}", "error", Color.red);
                return;
            }

            ToastNotification.Show("Cosmetic updated successfully!", "success", Color.green);
            ReturnToList();
        }

        private bool ValidateCosmeticFields(out string error)
        {
            var name = editBuffer != null ? editBuffer.Working.name : nameInput.value;

            if (string.IsNullOrEmpty(name))
            {
                error = "Cosmetic name is required.";
                return false;
            }

            if (
                editBuffer == null
                || editBuffer.Working.category_sprites == null
                || editBuffer.Working.category_sprites.Count == 0
            )
            {
                error = "Please load at least one valid sprite before saving.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void ReturnToList()
        {
            SetCharacterEditorVisible(false);
            OpenMenu(cosmeticMenuPrefab);
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

        private bool EnsureCharacterEditorInstance()
        {
            if (characterEditorInstance != null && characterEditor != null)
                return true;

            characterEditorPrefab = CharacterEditorRuntimeUtility.ResolveCharacterEditorPrefab(
                this,
                characterEditorPrefab
            );

            return CharacterEditorRuntimeUtility.TryInstantiateCharacterEditor(
                this,
                characterEditorPrefab,
                out characterEditorInstance,
                out characterEditor
            );
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

            characterPreviewRenderer = new CharacterEditorPreviewRenderer(
                characterEditorPrefab,
                true
            );
            return true;
        }

        private void SetCharacterPreviewVisible(bool isVisible)
        {
            characterPreviewRenderer?.SetVisible(isVisible);
        }

        private void SetSpritePreviewVisible(bool isVisible)
        {
            if (spritePreview == null)
                return;

            spritePreview.style.visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            spritePreview.style.opacity = isVisible ? 1f : 0f;
            spritePreview.pickingMode = isVisible ? PickingMode.Position : PickingMode.Ignore;
        }

        private void DisposeCharacterPreviewRenderer()
        {
            if (characterPreviewRenderer == null)
            {
                return;
            }

            characterPreviewRenderer.Dispose();
            characterPreviewRenderer = null;
        }

        private void DestroyCharacterEditorInstance()
        {
            CharacterEditorRuntimeUtility.DestroyCharacterEditorInstance(
                ref characterEditorInstance,
                ref characterEditor
            );
        }
    }
}
