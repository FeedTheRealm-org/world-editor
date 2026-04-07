using System;
using API;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
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
        private AggresiveNpcCharacterPreviewRenderer characterPreviewRenderer;
        private bool pendingPreviewRefresh;

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
            HideEmbeddedCharacterEditors();
            SetCharacterEditorVisible(false);

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
                editingEnemyData.category_sprites = categorySprites;
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
                characterEditorInstance.SetActive(true);
                return;
            }

            if (characterEditorInstance != null)
            {
                characterEditorInstance.SetActive(false);
                DestroyCharacterEditorInstance();
            }

            SetCharacterPreviewVisible(true);

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

            ResolveCharacterEditorPrefabFallback();

            if (characterEditorPrefab == null)
            {
                Debug.LogError("Character editor prefab is not assigned.", this);
                return false;
            }

            characterEditorInstance = Instantiate(characterEditorPrefab);
            characterEditorInstance.name = $"{characterEditorPrefab.name}_Runtime";
            characterEditorInstance.transform.SetParent(null, false);
            characterEditor = characterEditorInstance.GetComponentInChildren<CharacterEditController>(
                true
            );

            if (characterEditor == null)
            {
                Debug.LogError(
                    "CharacterEditController component was not found on instantiated character editor prefab.",
                    this
                );
                DestroyCharacterEditorInstance();
                return false;
            }

            characterEditorInstance.SetActive(false);
            return true;
        }

        private void ResolveCharacterEditorPrefabFallback()
        {
            if (characterEditorPrefab != null)
                return;

            var embeddedEditor = GetComponentInChildren<CharacterEditController>(true);
            if (embeddedEditor == null)
                return;

            characterEditorPrefab = embeddedEditor.transform.parent != null
                ? embeddedEditor.transform.parent.gameObject
                : embeddedEditor.gameObject;
        }

        private void HideEmbeddedCharacterEditors()
        {
            var embeddedEditors = GetComponentsInChildren<CharacterEditController>(true);
            foreach (var embeddedEditor in embeddedEditors)
            {
                var root = embeddedEditor.transform.parent != null
                    ? embeddedEditor.transform.parent.gameObject
                    : embeddedEditor.gameObject;

                root.SetActive(false);
            }
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

            ResolveCharacterEditorPrefabFallback();

            if (characterEditorPrefab == null)
            {
                Debug.LogError("Character editor prefab is not assigned.", this);
                return false;
            }

            characterPreviewRenderer = new AggresiveNpcCharacterPreviewRenderer(characterEditorPrefab);
            return true;
        }

        private void SetCharacterPreviewVisible(bool isVisible)
        {
            characterPreviewRenderer?.SetVisible(isVisible);
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
            if (characterEditorInstance != null)
            {
                Destroy(characterEditorInstance);
                characterEditorInstance = null;
            }

            characterEditor = null;
        }
    }
}
