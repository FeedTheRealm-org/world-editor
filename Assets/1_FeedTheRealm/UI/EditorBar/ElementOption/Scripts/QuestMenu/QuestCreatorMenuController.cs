using System;
using System.Linq;
using Enums;
using FeedTheRealm.Core.WorldObjects.Enemies;
using FeedTheRealm.Core.WorldObjects.NPCs;
using FeedTheRealm.Core.WorldObjects.Quests;
using FeedTheRealm.UI.EditorBar.ElementOption.Base;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.QuestMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestCreatorMenuController : BaseCreatorMenuController<GenericQuest>
    {
        private TextField contentInput;
        private DropdownField questTypeDropdown;
        private DropdownField enemyDropdown;
        private DropdownField npcDropdown;
        private IntegerField targetAmountField;
        private VisualElement enemyContainer;
        private VisualElement npcContainer;

        protected override CreatorObjectCategories Category => CreatorObjectCategories.Quest;
        protected override string ObjectTypeName => "Quest";
        protected override string SaveButtonName => "SaveButton";

        protected override void InitializeSpecificFields(VisualElement root)
        {
            contentInput = root.Q<TextField>("ContentField");
            LogIfNull(contentInput, "Content input field");

            questTypeDropdown = root.Q<DropdownField>("QuestTypeDropdown");
            LogIfNull(questTypeDropdown, "QuestType dropdown");

            enemyDropdown = root.Q<DropdownField>("EnemyDropdown");
            npcDropdown = root.Q<DropdownField>("NPCDropdown");
            targetAmountField = root.Q<IntegerField>("TargetAmount");
            enemyContainer = root.Q<VisualElement>("EnemyContainer");
            npcContainer = root.Q<VisualElement>("NPCContainer");

            if (questTypeDropdown != null)
            {
                questTypeDropdown.choices = Enum.GetNames(typeof(QuestType)).ToList();
                if (questTypeDropdown.choices.Count > 0)
                    questTypeDropdown.value = questTypeDropdown.choices[0];
                questTypeDropdown.RegisterValueChangedCallback(OnQuestTypeChanged);
            }

            if (enemyContainer != null)
                enemyContainer.style.display = DisplayStyle.None;
            if (npcContainer != null)
                npcContainer.style.display = DisplayStyle.None;
        }

        protected override void PopulateFields()
        {
            nameInput.value = currentObject.name;
            contentInput.value = currentObject.content ?? "";
            questTypeDropdown.value = currentObject.questType.ToString();
            targetAmountField.value = currentObject.targetAmount;

            UpdateQuestTypeUI(currentObject.questType.ToString());

            if (currentObject.questType == QuestType.EnemySlays && enemyDropdown != null)
            {
                var enemies = creatorObjectLibrary
                    .GetCreatables(CreatorObjectCategories.Enemy)
                    .Cast<GenericEnemy>()
                    .ToList();
                var selectedEnemy = enemies.FirstOrDefault(e =>
                    e.ObjectId == currentObject.targetId
                );
                if (selectedEnemy != null)
                    enemyDropdown.value = selectedEnemy.DisplayName;
            }
            else if (currentObject.questType == QuestType.NpcInteract && npcDropdown != null)
            {
                var npcs = creatorObjectLibrary
                    .GetCreatables(CreatorObjectCategories.NPC)
                    .Cast<GenericNPC>()
                    .ToList();
                var selectedNPC = npcs.FirstOrDefault(n => n.ObjectId == currentObject.targetId);
                if (selectedNPC != null)
                    npcDropdown.value = selectedNPC.DisplayName;
            }
        }

        private void OnQuestTypeChanged(ChangeEvent<string> evt)
        {
            UpdateQuestTypeUI(evt.newValue);
        }

        private void UpdateQuestTypeUI(string questTypeValue)
        {
            if (string.IsNullOrEmpty(questTypeValue))
                return;

            if (enemyContainer != null)
                enemyContainer.style.display = DisplayStyle.None;
            if (npcContainer != null)
                npcContainer.style.display = DisplayStyle.None;

            if (Enum.TryParse<QuestType>(questTypeValue, out var questType))
            {
                if (questType == QuestType.EnemySlays && enemyContainer != null)
                {
                    enemyContainer.style.display = DisplayStyle.Flex;
                    PopulateEnemyDropdown();
                }
                else if (questType == QuestType.NpcInteract && npcContainer != null)
                {
                    npcContainer.style.display = DisplayStyle.Flex;
                    PopulateNPCDropdown();
                }
            }
        }

        private void PopulateEnemyDropdown()
        {
            if (enemyDropdown == null || creatorObjectLibrary == null)
                return;

            var enemies = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Enemy)
                .Cast<GenericEnemy>()
                .ToList();

            enemyDropdown.choices = enemies.Select(e => e.DisplayName).ToList();
            logger?.Log(
                $"QuestCreator: Populated {enemies.Count} enemies",
                this,
                Logging.LogType.Info
            );
        }

        private void PopulateNPCDropdown()
        {
            if (npcDropdown == null || creatorObjectLibrary == null)
                return;

            var npcs = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.NPC)
                .Cast<GenericNPC>()
                .ToList();

            npcDropdown.choices = npcs.Select(n => n.DisplayName).ToList();
            logger?.Log($"QuestCreator: Populated {npcs.Count} NPCs", this, Logging.LogType.Info);
        }

        protected override bool ValidateSpecificFields()
        {
            if (string.IsNullOrEmpty(questTypeDropdown?.value))
            {
                ShowValidationError("Quest type is required");
                return false;
            }

            if (!Enum.TryParse<QuestType>(questTypeDropdown.value, out var questType))
            {
                ShowValidationError("Invalid quest type");
                return false;
            }

            if (questType == QuestType.EnemySlays && string.IsNullOrEmpty(enemyDropdown?.value))
            {
                ShowValidationError("Please select an enemy");
                return false;
            }

            if (questType == QuestType.NpcInteract && string.IsNullOrEmpty(npcDropdown?.value))
            {
                ShowValidationError("Please select an NPC");
                return false;
            }

            return true;
        }

        protected override void CreateNewObject()
        {
            var questType = Enum.Parse<QuestType>(questTypeDropdown.value);
            string targetId = GetTargetId(questType);
            int targetAmount = questType == QuestType.NpcInteract ? 1 : targetAmountField.value;

            var questData = new QuestData(
                null,
                nameInput.value,
                contentInput.value ?? "",
                questType,
                targetId,
                targetAmount,
                null
            );

            currentObject = new GenericQuest(questData);
            creatorObjectLibrary.AddCreatable(Category, currentObject);
            logger?.Log(
                $"Created new quest: {currentObject.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }

        protected override void UpdateExistingObject()
        {
            var questType = Enum.Parse<QuestType>(questTypeDropdown.value);

            currentObject.name = nameInput.value;
            currentObject.content = contentInput.value;
            currentObject.questType = questType;
            currentObject.targetId = GetTargetId(questType);
            currentObject.targetAmount =
                questType == QuestType.NpcInteract ? 1 : targetAmountField.value;

            logger?.Log($"Updated quest: {currentObject.DisplayName}", this, Logging.LogType.Info);
        }

        private string GetTargetId(QuestType questType)
        {
            if (questType == QuestType.EnemySlays && enemyDropdown != null)
            {
                var enemies = creatorObjectLibrary
                    .GetCreatables(CreatorObjectCategories.Enemy)
                    .Cast<GenericEnemy>()
                    .ToList();
                return enemies.FirstOrDefault(e => e.DisplayName == enemyDropdown.value)?.ObjectId;
            }
            else if (questType == QuestType.NpcInteract && npcDropdown != null)
            {
                var npcs = creatorObjectLibrary
                    .GetCreatables(CreatorObjectCategories.NPC)
                    .Cast<GenericNPC>()
                    .ToList();
                return npcs.FirstOrDefault(n => n.DisplayName == npcDropdown.value)?.ObjectId;
            }
            return null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (questTypeDropdown != null)
                questTypeDropdown.UnregisterValueChangedCallback(OnQuestTypeChanged);
        }
    }
}
