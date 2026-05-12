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

namespace FeedTheRealm.UI.EditorBar.ElementOption.EnemyMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class AggresiveNpcCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject aggresiveNpcMenuPrefab;

        private EditBuffer<EnemyData> editBuffer;
        private string currentLootTableId;
        private string currentWeaponId;

        [SerializeField]
        private GameObject characterEditorPrefab;

        [SerializeField]
        private WorldSelector worldSelector;

        private TextField nameInput;
        private TextField descriptionInput;
        private IntegerField healthPointsInput;
        private DropdownField weaponInput;
        private DropdownField lootTableInput;
        private Button editCharacterButton;
        private Button closeButton;
        private Button saveButton;
        private Button returnButton;

        private Image spritePreview;
        private GameObject characterEditorInstance;
        private CharacterEditController characterEditor;
        private CharacterEditorPreviewRenderer characterPreviewRenderer;
        private bool pendingPreviewRefresh;

        public void SetupEditor(AggresiveNpc npc)
        {
            editBuffer = new EditBuffer<EnemyData>(npc.data);

            if (npc.data.category_sprites != null)
                editBuffer.Working.category_sprites = new Dictionary<string, string>(
                    npc.data.category_sprites
                );
            else
                editBuffer.Working.category_sprites = new Dictionary<string, string>();

            editBuffer.Working.skin_color =
                npc.data.skin_color
                ?? new API.CharacterColorHsv
                {
                    h = 0f,
                    s = 0f,
                    v = 100f,
                };
            editBuffer.Working.hair_color =
                npc.data.hair_color
                ?? new API.CharacterColorHsv
                {
                    h = 0f,
                    s = 0f,
                    v = 100f,
                };
            editBuffer.Working.eye_color =
                npc.data.eye_color
                ?? new API.CharacterColorHsv
                {
                    h = 0f,
                    s = 0f,
                    v = 100f,
                };

            currentLootTableId = editBuffer.Working.lootTableId;
            currentWeaponId = editBuffer.Working.weaponId;

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
            PopulateWeapons();

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
                saveButton.clicked -= SaveExistingEnemy;
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
            weaponInput = root.Q<DropdownField>("WeaponField");
            lootTableInput = root.Q<DropdownField>("LootTableField");
            editCharacterButton = root.Q<Button>("EditCharacter");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");
            returnButton = root.Q<Button>("Return");
            spritePreview = root.Q<Image>("SpritePreview");

            closeButton.clicked += ReturnToList;
            if (returnButton != null)
                returnButton.clicked += ReturnToList;
        }

        private void PopulateLootTables()
        {
            var lootTables = creatablesManager.GetAll<LootTable>();
            lootTableInput.choices = lootTables.Select(lt => lt.data.name).ToList();
        }

        private void PopulateWeapons()
        {
            var weapons = creatablesManager.GetAll<Weapon>();
            weaponInput.choices = weapons.Select(w => w.data.name).ToList();
        }

        private void SetupCreateMode()
        {
            var newEnemy = new EnemyData(
                Guid.NewGuid().ToString(),
                "",
                "",
                0,
                null,
                null,
                new Dictionary<string, string>()
            );
            editBuffer = new EditBuffer<EnemyData>(newEnemy);
            editBuffer.Working.category_sprites = new Dictionary<string, string>();
            editBuffer.Working.skin_color = new API.CharacterColorHsv
            {
                h = 0f,
                s = 0f,
                v = 100f,
            };
            editBuffer.Working.hair_color = new API.CharacterColorHsv
            {
                h = 0f,
                s = 0f,
                v = 100f,
            };
            editBuffer.Working.eye_color = new API.CharacterColorHsv
            {
                h = 0f,
                s = 0f,
                v = 100f,
            };

            PopulateFields();
            BindEditMode();

            saveButton.text = "Create Enemy";
            saveButton.clicked -= SaveExistingEnemy;
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked += CreateNewObject;

            editCharacterButton.clicked -= OpenCharacterEditor;
            editCharacterButton.clicked += OpenCharacterEditor;
        }

        private void SetupEditMode()
        {
            PopulateFields();
            BindEditMode();

            saveButton.clicked -= CreateNewObject;
            saveButton.clicked -= SaveExistingEnemy;
            saveButton.text = "Save Enemy";
            saveButton.clicked += SaveExistingEnemy;

            editCharacterButton.clicked -= OpenCharacterEditor;
            editCharacterButton.clicked += OpenCharacterEditor;
        }

        private void OpenCharacterEditor()
        {
            if (!EnsureCharacterEditorInstance())
                return;

            characterEditor.SetAssetsWorldId(worldSelector.selectedWorldId);

            var characterInfo = BuildCharacterInfo();

            characterEditor.SetupWithCharacterInfo(characterInfo, SaveCharacterInfo);
            SetCharacterEditorVisible(true);
        }

        private void SaveCharacterInfo(API.CharacterInfoResponse characterInfo)
        {
            if (editBuffer != null && characterInfo != null)
            {
                editBuffer.Working.category_sprites =
                    characterInfo.category_sprites ?? new Dictionary<string, string>();
                editBuffer.Working.skin_color =
                    characterInfo.skin_color
                    ?? new API.CharacterColorHsv
                    {
                        h = 0f,
                        s = 0f,
                        v = 100f,
                    };
                editBuffer.Working.hair_color =
                    characterInfo.hair_color
                    ?? new API.CharacterColorHsv
                    {
                        h = 0f,
                        s = 0f,
                        v = 100f,
                    };
                editBuffer.Working.eye_color =
                    characterInfo.eye_color
                    ?? new API.CharacterColorHsv
                    {
                        h = 0f,
                        s = 0f,
                        v = 100f,
                    };
            }
            pendingPreviewRefresh = true;
        }

        private void PopulateFields()
        {
            if (editBuffer != null)
            {
                nameInput.value = editBuffer.Working.name;
                descriptionInput.value = editBuffer.Working.description;
                healthPointsInput.value = editBuffer.Working.healthPoints;
            }

            var lootTables = creatablesManager.GetAll<LootTable>();
            if (!string.IsNullOrEmpty(currentLootTableId))
            {
                var selected = lootTables.FirstOrDefault(lt => lt.data.id == currentLootTableId);
                lootTableInput.value = selected?.data.name ?? string.Empty;
            }
            else if (lootTables.Any())
            {
                lootTableInput.value = lootTables[0].data.name;
            }

            var weapons = creatablesManager.GetAll<Weapon>();
            var selectedWeapon = !string.IsNullOrEmpty(currentWeaponId)
                ? weapons.FirstOrDefault(w => w.data.id == currentWeaponId)
                : null;
            if (selectedWeapon == null && weapons.Any())
            {
                selectedWeapon = weapons[0];
                currentWeaponId = selectedWeapon.data.id;
            }

            weaponInput.value = selectedWeapon?.data.name ?? string.Empty;
            if (editBuffer != null && selectedWeapon != null)
                editBuffer.Working.weaponId = selectedWeapon.data.id;
        }

        private void BindEditMode()
        {
            if (editBuffer == null)
                return;
            nameInput.RegisterValueChangedCallback(evt => editBuffer.Working.name = evt.newValue);
            descriptionInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.description = evt.newValue
            );
            healthPointsInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.healthPoints = evt.newValue
            );

            lootTableInput.RegisterValueChangedCallback(evt =>
            {
                var selected = creatablesManager
                    .GetAll<LootTable>()
                    .FirstOrDefault(lt => lt.data.name == evt.newValue);
                editBuffer.Working.lootTableId = selected?.data.id;
            });

            weaponInput.RegisterValueChangedCallback(evt =>
            {
                var selected = creatablesManager
                    .GetAll<Weapon>()
                    .FirstOrDefault(w => w.data.name == evt.newValue);
                editBuffer.Working.weaponId = selected?.data.id;
            });
        }

        private void CreateNewObject()
        {
            if (!ValidateEnemyFields(out var error))
            {
                ToastNotification.Show($"Failed to save enemy: {error}", "error", Color.red);
                return;
            }

            if (editBuffer != null)
            {
                editBuffer.Commit();
                editBuffer.Original.category_sprites = new Dictionary<string, string>(
                    editBuffer.Working.category_sprites
                );

                var enemy = new AggresiveNpc(editBuffer.Original);
                creatablesManager.Add(enemy);
            }

            ToastNotification.Show("Aggressive NPC created successfully!", "success", Color.green);

            ReturnToList();
        }

        private void SaveExistingEnemy()
        {
            if (!ValidateEnemyFields(out var error))
            {
                ToastNotification.Show($"Failed to save enemy: {error}", "error", Color.red);
                return;
            }

            var lootTables = creatablesManager.GetAll<LootTable>();
            var selectedLootTable = lootTables.FirstOrDefault(lt =>
                lt.data.name == lootTableInput.value
            );

            var weapons = creatablesManager.GetAll<Weapon>();
            var selectedWeapon = weapons.FirstOrDefault(w => w.data.name == weaponInput.value);

            if (editBuffer != null)
            {
                editBuffer.Working.lootTableId = selectedLootTable?.data.id;
                editBuffer.Working.weaponId = selectedWeapon?.data.id;
                editBuffer.Commit();

                editBuffer.Original.category_sprites = new Dictionary<string, string>(
                    editBuffer.Working.category_sprites
                );
            }

            ToastNotification.Show("Aggressive NPC updated successfully!", "success", Color.green);
            ReturnToList();
        }

        private bool ValidateEnemyFields(out string error)
        {
            var name = editBuffer != null ? editBuffer.Working.name : nameInput.value;
            var health =
                editBuffer != null ? editBuffer.Working.healthPoints : healthPointsInput.value;

            if (string.IsNullOrEmpty(name))
            {
                error = "Enemy name is required.";
                return false;
            }

            if (health <= 0)
            {
                error = "Health points must be greater than zero.";
                return false;
            }

            error = string.Empty;
            return true;
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
                skin_color =
                    editBuffer != null
                        ? editBuffer.Working.skin_color
                        : new API.CharacterColorHsv
                        {
                            h = 0f,
                            s = 0f,
                            v = 100f,
                        },
                hair_color =
                    editBuffer != null
                        ? editBuffer.Working.hair_color
                        : new API.CharacterColorHsv
                        {
                            h = 0f,
                            s = 0f,
                            v = 100f,
                        },
                eye_color =
                    editBuffer != null
                        ? editBuffer.Working.eye_color
                        : new API.CharacterColorHsv
                        {
                            h = 0f,
                            s = 0f,
                            v = 100f,
                        },
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
                false
            );
            characterPreviewRenderer.SetAssetsWorldId(worldSelector.selectedWorldId);

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
