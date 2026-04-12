using System;
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

        private EditBuffer<EnemyData> editBuffer;
        private string currentLootTableId;

        private TextField nameInput;
        private TextField descriptionInput;
        private IntegerField healthPointsInput;
        private IntegerField damageInput;
        private IntegerField speedInput;
        private IntegerField rangeInput;
        private DropdownField lootTableInput;
        private Button closeButton;
        private Button saveButton;
        private Button returnButton;

        public void SetupEditor(AggresiveNpc npc)
        {
            editBuffer = new EditBuffer<EnemyData>(npc.data);
            currentLootTableId = editBuffer.Working.lootTableId;

            SetupEditMode();
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            InitializeFields(root);
            PopulateLootTables();
            SetupCreateMode();
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
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");
            returnButton = root.Q<Button>("Return");

            closeButton.clicked += ReturnToList;
            if (returnButton != null)
                returnButton.clicked += ReturnToList;
        }

        private void PopulateLootTables()
        {
            var lootTables = creatblesManager.GetAll<LootTable>();
            lootTableInput.choices = lootTables.Select(lt => lt.data.name).ToList();
        }

        private void SetupCreateMode()
        {
            saveButton.text = "Create Enemy";
            saveButton.clicked -= SaveExistingEnemy;
            saveButton.clicked += CreateNewObject;
        }

        private void SetupEditMode()
        {
            PopulateFields();
            BindEditMode();

            saveButton.clicked -= CreateNewObject;
            saveButton.clicked -= SaveExistingEnemy;
            saveButton.text = "Save Enemy";
            saveButton.clicked += SaveExistingEnemy;
        }

        private void PopulateFields()
        {
            if (editBuffer != null)
            {
                nameInput.value = editBuffer.Working.name;
                descriptionInput.value = editBuffer.Working.description;
                healthPointsInput.value = editBuffer.Working.healthPoints;
                damageInput.value = editBuffer.Working.damage;
                speedInput.value = editBuffer.Working.speed;
                rangeInput.value = editBuffer.Working.range;
            }

            var lootTables = creatblesManager.GetAll<LootTable>();
            if (!string.IsNullOrEmpty(currentLootTableId))
            {
                var selected = lootTables.FirstOrDefault(lt => lt.data.id == currentLootTableId);
                lootTableInput.value = selected?.data.name ?? string.Empty;
            }
            else if (lootTables.Any())
            {
                lootTableInput.value = lootTables[0].data.name;
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
            healthPointsInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.healthPoints = evt.newValue
            );
            damageInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.damage = evt.newValue
            );
            speedInput.RegisterValueChangedCallback(evt => editBuffer.Working.speed = evt.newValue);
            rangeInput.RegisterValueChangedCallback(evt => editBuffer.Working.range = evt.newValue);
        }

        private void CreateNewObject()
        {
            if (!ValidateEnemyFields(out var error))
            {
                ToastNotification.Show($"Failed to save enemy: {error}", "error", Color.red);
                return;
            }

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
                null,
                selectedLootTable?.data.id
            );

            var enemy = new AggresiveNpc(enemyData);

            creatblesManager.Add(enemy);

            ToastNotification.Show("Aggressive NPC created successfully!", "success", Color.green);

            OpenMenu(aggresiveNpcMenuPrefab);
        }

        private void SaveExistingEnemy()
        {
            if (!ValidateEnemyFields(out var error))
            {
                ToastNotification.Show($"Failed to save enemy: {error}", "error", Color.red);
                return;
            }

            var lootTables = creatblesManager.GetAll<LootTable>();
            var selectedLootTable = lootTables.FirstOrDefault(lt =>
                lt.data.name == lootTableInput.value
            );

            if (editBuffer != null)
            {
                editBuffer.Working.lootTableId = selectedLootTable?.data.id;
                editBuffer.Commit();
            }

            ToastNotification.Show("Aggressive NPC updated successfully!", "success", Color.green);
            OpenMenu(aggresiveNpcMenuPrefab);
        }

        private bool ValidateEnemyFields(out string error)
        {
            var name = editBuffer != null ? editBuffer.Working.name : nameInput.value;
            var health =
                editBuffer != null ? editBuffer.Working.healthPoints : healthPointsInput.value;
            var damage = editBuffer != null ? editBuffer.Working.damage : damageInput.value;
            var speed = editBuffer != null ? editBuffer.Working.speed : speedInput.value;
            var range = editBuffer != null ? editBuffer.Working.range : rangeInput.value;

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

            if (damage < 0 || speed < 0 || range < 0)
            {
                error = "Enemy stats cannot be negative.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void ReturnToList()
        {
            OpenMenu(aggresiveNpcMenuPrefab);
        }
    }
}
