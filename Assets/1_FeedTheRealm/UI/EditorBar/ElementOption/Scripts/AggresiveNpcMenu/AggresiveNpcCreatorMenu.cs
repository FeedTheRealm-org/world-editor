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

        private EnemyData editingEnemyData;

        private TextField nameInput;
        private TextField descriptionInput;
        private IntegerField healthPointsInput;
        private IntegerField damageInput;
        private IntegerField speedInput;
        private IntegerField rangeInput;
        private DropdownField lootTableInput;
        private Button closeButton;
        private Button saveButton;
        private bool IsPopulated = false;

        public void SetupEditor(AggresiveNpc aggresiveNpc)
        {
            editingEnemyData = aggresiveNpc.data;
            PopulateFields();
            BindEditMode();
            saveButton.clicked += ReturnToList;
            saveButton.text = "Return to List";
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            InitializeFields(root);
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

            closeButton.clicked += ReturnToList;
            saveButton.clicked += CreateNewObject;
        }

        private void PopulateLootTables()
        {
            if (lootTableInput == null || IsPopulated)
                return;

            var lootTables = creatblesManager.GetAll<LootTable>();
            lootTableInput.choices = lootTables.Select(lt => lt.data.name).ToList();
            IsPopulated = true;
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

            PopulateLootTables();
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
                null, // sprite for now
                selectedLootTable?.data.id
            );

            var enemy = new AggresiveNpc(enemyData);

            creatblesManager.Add(enemy);

            Debug.Log($"Enemy created: {enemy.data.name} with ID: {enemy.data.id}");

            OpenMenu(aggresiveNpcMenuPrefab);
        }

        private void ReturnToList()
        {
            OpenMenu(aggresiveNpcMenuPrefab);
        }
    }
}
