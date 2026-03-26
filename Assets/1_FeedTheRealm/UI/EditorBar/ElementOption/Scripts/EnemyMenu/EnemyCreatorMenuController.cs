using System;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.EnemyMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class EnemyCreatorMenuController : MenuController
    {
        [SerializeField]
        private CreatablesManager creatblesManager;

        [SerializeField]
        private GameObject enemyMenuPrefab;

        private TextField nameInput;
        private TextField descriptionInput;
        private IntegerField healthPointsInput;
        private IntegerField damageInput;
        private IntegerField speedInput;
        private IntegerField rangeInput;
        private DropdownField lootTableInput;
        private Button closeButton;
        private Button saveButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            InitializeFields(root);
            BindEvents();
            PopulateLootTables();
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
        }

        private void PopulateLootTables()
        {
            if (lootTableInput == null)
                return;

            var lootTables = creatblesManager.GetAll<LootTable>();
            lootTableInput.choices = lootTables.Select(lt => lt.data.name).ToList();
        }

        private void BindEvents()
        {
            if (saveButton != null)
                saveButton.clicked += CreateNewObject;
        }

        private void CreateNewObject()
        {
            var lootTables = creatblesManager.GetAll<LootTable>();

            var selectedLootTable = lootTables.FirstOrDefault(lt =>
                lt.data.name == lootTableInput.value
            );

            if (selectedLootTable == null)
            {
                Debug.LogError("Invalid loot table selected");
                return;
            }

            var enemyData = new EnemyData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                healthPointsInput.value,
                damageInput.value,
                speedInput.value,
                rangeInput.value,
                null, // sprite for now
                selectedLootTable.data.id
            );

            var enemy = new AggresiveNpc(enemyData);

            creatblesManager.Add(enemy);

            Debug.Log($"Enemy created: {enemy.data.name} with ID: {enemy.data.id}");
            OpenMenu(enemyMenuPrefab);
        }

        private void ReturnToList()
        {
            OpenMenu(enemyMenuPrefab);
        }
    }
}
