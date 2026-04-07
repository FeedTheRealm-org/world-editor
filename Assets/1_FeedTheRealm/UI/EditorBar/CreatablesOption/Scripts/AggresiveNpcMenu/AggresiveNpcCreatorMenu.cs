using System;
using API;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.EditorBar.ElementOption.CharacterEditor;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.EnemyMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class AggresiveNpcCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatblesManager;

        [SerializeField]
        private GameObject aggresiveNpcMenuPrefab;

        [SerializeField]
        private GameObject characterEditorPrefab;

        private EnemyData editingEnemyData;
        private Dictionary<string, string> pendingCategorySprites = new();

        private TextField nameInput;
        private TextField descriptionInput;
        private IntegerField healthPointsInput;
        private IntegerField damageInput;
        private IntegerField speedInput;
        private IntegerField rangeInput;
        private DropdownField lootTableInput;
        private Button editCharacterButton;
        private Button closeButton;
        private Button saveButton;
        private Image spritePreview;

        private GameObject characterEditorInstance;
        private CharacterEditController characterEditor;
        private CharacterEditorPreviewRenderer characterPreviewRenderer;
        private bool pendingPreviewRefresh;
        private bool editModeBindingsRegistered;

        public void SetupEditor(AggresiveNpc npc)
        {
            editingEnemyData = npc.data;

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
            PopulateLootTables();

            characterEditorPrefab = CharacterEditorRuntimeUtility.ResolveCharacterEditorPrefab(
                this,
                characterEditorPrefab
            );
            CharacterEditorRuntimeUtility.HideEmbeddedCharacterEditors(this);

            SetCharacterEditorVisible(false);
            SetSpritePreviewVisible(true);

            if (editingEnemyData != null)
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

            if (saveButton != null)
            {
                saveButton.clicked -= CreateNewObject;
                saveButton.clicked -= ReturnToList;
            }

            if (editCharacterButton != null)
                editCharacterButton.clicked -= OpenCharacterEditor;

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
            healthPointsInput = root.Q<IntegerField>("HealthPoints");
            damageInput = root.Q<IntegerField>("AttackDamage");
            speedInput = root.Q<IntegerField>("Speed");
            rangeInput = root.Q<IntegerField>("Range");
            lootTableInput = root.Q<DropdownField>("LootTableField");
            editCharacterButton = root.Q<Button>("EditCharacter");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");
            spritePreview = root.Q<Image>("SpritePreview");

            closeButton.clicked += ReturnToList;
        }

        private void PopulateLootTables()
        {
            var lootTables = creatblesManager.GetAll<LootTable>();
            lootTableInput.choices = lootTables.Select(lt => lt.data.name).ToList();
        }

        private void SetupCreateMode()
        {
            pendingCategorySprites.Clear();

            saveButton.text = "Create Enemy";
            saveButton.clicked -= ReturnToList;
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked += CreateNewObject;

            editCharacterButton.clicked -= OpenCharacterEditor;
            editCharacterButton.clicked += OpenCharacterEditor;
        }

        private void SetupEditMode()
        {
            PopulateFields();
            BindEditMode();

            saveButton.text = "Return to List";
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked -= ReturnToList;
            saveButton.clicked += ReturnToList;

            editCharacterButton.clicked -= OpenCharacterEditor;
            editCharacterButton.clicked += OpenCharacterEditor;
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
            if (editingEnemyData != null)
            {
                editingEnemyData.category_sprites = categorySprites ?? new Dictionary<string, string>();
                pendingPreviewRefresh = true;
                return;
            }

            pendingCategorySprites = categorySprites ?? new Dictionary<string, string>();
            pendingPreviewRefresh = true;
        }

        private void PopulateFields()
        {
            nameInput.value = editingEnemyData.name;
            descriptionInput.value = editingEnemyData.description;
            healthPointsInput.value = editingEnemyData.healthPoints;
            damageInput.value = editingEnemyData.damage;
            speedInput.value = editingEnemyData.speed;
            rangeInput.value = editingEnemyData.range;

            var lootTables = creatblesManager.GetAll<LootTable>();
            var selected = lootTables.FirstOrDefault(lt =>
                lt.data.id == editingEnemyData.lootTableId
            );

            if (selected != null)
                lootTableInput.value = selected.data.name;
        }

        private void BindEditMode()
        {
            if (editModeBindingsRegistered)
                return;

            nameInput.RegisterValueChangedCallback(evt => editingEnemyData.name = evt.newValue);

            descriptionInput.RegisterValueChangedCallback(evt =>
                editingEnemyData.description = evt.newValue
            );

            healthPointsInput.RegisterValueChangedCallback(evt =>
                editingEnemyData.healthPoints = evt.newValue
            );

            damageInput.RegisterValueChangedCallback(evt => editingEnemyData.damage = evt.newValue);

            speedInput.RegisterValueChangedCallback(evt => editingEnemyData.speed = evt.newValue);

            rangeInput.RegisterValueChangedCallback(evt => editingEnemyData.range = evt.newValue);

            lootTableInput.RegisterValueChangedCallback(evt =>
            {
                var lootTables = creatblesManager.GetAll<LootTable>();
                var selected = lootTables.FirstOrDefault(lt => lt.data.name == evt.newValue);

                editingEnemyData.lootTableId = selected?.data.id;
            });

            editModeBindingsRegistered = true;
        }

        private void CreateNewObject()
        {
            var lootTables = creatblesManager.GetAll<LootTable>();

            var selectedLootTable = lootTables.FirstOrDefault(lt =>
                lt.data.name == lootTableInput.value
            );

            var enemyData = new EnemyData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                healthPointsInput.value,
                damageInput.value,
                speedInput.value,
                rangeInput.value,
                selectedLootTable?.data.id,
                editingEnemyData?.category_sprites
                    ?? pendingCategorySprites
                    ?? new Dictionary<string, string>()
            );

            var enemy = new AggresiveNpc(enemyData);

            creatblesManager.Add(enemy);

            Debug.Log($"Enemy created: {enemy.data.name}");

            OpenMenu(aggresiveNpcMenuPrefab);
        }

        private void ReturnToList()
        {
            SetCharacterEditorVisible(false);
            OpenMenu(aggresiveNpcMenuPrefab);
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
            var categorySprites = editingEnemyData?.category_sprites ?? pendingCategorySprites;
            if (categorySprites == null)
                categorySprites = new Dictionary<string, string>();

            return new CharacterInfoResponse
            {
                character_name = editingEnemyData?.name ?? nameInput?.value ?? string.Empty,
                character_bio = editingEnemyData?.description ?? descriptionInput?.value ?? string.Empty,
                category_sprites = new Dictionary<string, string>(categorySprites)
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
