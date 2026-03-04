using System;
using System.Linq;
using FeedTheRealm.Core.WorldObjects.Enemies;
using FeedTheRealm.Core.WorldObjects.LootTable;
using FeedTheRealm.UI.EditorBar.ElementOption.Base;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.EnemyMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class EnemyCreatorMenuController : BaseCreatorMenuController<GenericEnemy>
    {
        private TextField descriptionInput;
        private IntegerField healthPointsInput;
        private IntegerField damageInput;
        private IntegerField speedInput;
        private IntegerField rangeInput;
        private DropdownField lootTableInput;

        protected override CreatorObjectCategories Category => CreatorObjectCategories.Enemy;
        protected override string ObjectTypeName => "Enemy";
        protected override string SaveButtonName => "SaveButton";
        protected override bool RequiresSprite => true;

        protected override void InitializeSpecificFields(VisualElement root)
        {
            descriptionInput = root.Q<TextField>("DescriptionField");
            LogIfNull(descriptionInput, "Description input field");

            healthPointsInput = root.Q<IntegerField>("HealthPoints");
            LogIfNull(healthPointsInput, "Health points input field");

            damageInput = root.Q<IntegerField>("AttackDamage");
            LogIfNull(damageInput, "Attack damage input field");

            speedInput = root.Q<IntegerField>("Speed");
            LogIfNull(speedInput, "Speed input field");

            rangeInput = root.Q<IntegerField>("Range");
            LogIfNull(rangeInput, "Range input field");

            lootTableInput = root.Q<DropdownField>("LootTableField");
            LogIfNull(lootTableInput, "LootTable dropdown field");

            if (lootTableInput != null)
            {
                var lootTables = creatorObjectLibrary
                    .GetCreatables(CreatorObjectCategories.LootTable)
                    .Cast<LootTable>()
                    .ToList();
                lootTableInput.choices = lootTables.Select(lt => lt.DisplayName).ToList();
            }
        }

        protected override void PopulateFields()
        {
            nameInput.value = currentObject.name;
            descriptionInput.value = currentObject.description ?? "";
            healthPointsInput.value = currentObject.healthPoints;
            damageInput.value = currentObject.damage;
            speedInput.value = currentObject.speed;
            rangeInput.value = currentObject.range;

            if (currentObject.objectId != null)
                lootTableInput.value = currentObject.name;

            LoadExistingSprite(currentObject.spriteFile);
        }

        protected override bool ValidateSpecificFields()
        {
            var lootTables = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.LootTable)
                .Cast<LootTable>()
                .ToList();
            var selectedLootTable = lootTables.FirstOrDefault(lt =>
                lt.DisplayName == lootTableInput?.value
            );

            if (selectedLootTable == null)
            {
                ShowValidationError("Valid loot table selection is required");
                return false;
            }
            return true;
        }

        protected override void CreateNewObject()
        {
            var lootTables = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.LootTable)
                .Cast<LootTable>()
                .ToList();
            var selectedLootTable = lootTables.FirstOrDefault(lt =>
                lt.DisplayName == lootTableInput.value
            );

            var enemyData = new EnemyData(
                null,
                nameInput.value,
                descriptionInput.value ?? "",
                healthPointsInput.value,
                damageInput.value,
                speedInput.value,
                rangeInput.value,
                pendingSpriteSourcePath,
                lootTableId: selectedLootTable.ObjectId
            );

            currentObject = new GenericEnemy(enemyData);
            creatorObjectLibrary.AddCreatable(Category, currentObject);
            logger?.Log(
                $"Created new enemy: {currentObject.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }

        protected override void UpdateExistingObject()
        {
            var lootTables = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.LootTable)
                .Cast<LootTable>()
                .ToList();
            var selectedLootTable = lootTables.FirstOrDefault(lt =>
                lt.DisplayName == lootTableInput.value
            );

            currentObject.name = nameInput.value;
            currentObject.description = descriptionInput.value;
            currentObject.healthPoints = healthPointsInput.value;
            currentObject.damage = damageInput.value;
            currentObject.speed = speedInput.value;
            currentObject.range = rangeInput.value;
            currentObject.lootTableId = selectedLootTable.ObjectId;

            if (!string.IsNullOrEmpty(pendingSpriteSourcePath))
            {
                currentObject.spriteFile = pendingSpriteSourcePath;
            }

            logger?.Log($"Updated enemy: {currentObject.DisplayName}", this, Logging.LogType.Info);
        }
    }
}
