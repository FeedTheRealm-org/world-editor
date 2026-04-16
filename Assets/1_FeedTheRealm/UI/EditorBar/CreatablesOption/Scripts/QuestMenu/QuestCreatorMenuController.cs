using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.QuestMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject questsMenuPrefab;

        [SerializeField]
        private GameObject rewardMenuPrefab;

        private QuestData editingData;
        private QuestData stagingData;
        private EditBuffer<QuestData> editBuffer;
        private bool isNewQuest;
        private const int MaxQuestNameLength = 40;
        private const int MaxQuestContentLength = 100;

        private TextField nameInput;
        private TextField contentInput;
        private DropdownField questTypeDropdown;
        private DropdownField enemyDropdown;
        private DropdownField npcDropdown;
        private IntegerField targetAmountField;
        private Button rewardsButton;
        private Label rewardsSummary;
        private VisualElement enemyContainer;
        private VisualElement npcContainer;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            nameInput = root.Q<TextField>("NameField");
            contentInput = root.Q<TextField>("ContentField");
            questTypeDropdown = root.Q<DropdownField>("QuestTypeDropdown");
            enemyDropdown = root.Q<DropdownField>("EnemyDropdown");
            npcDropdown = root.Q<DropdownField>("NPCDropdown");
            targetAmountField = root.Q<IntegerField>("TargetAmount");
            rewardsButton = root.Q<Button>("RewardsButton");
            rewardsSummary = root.Q<Label>("RewardsSummary");
            enemyContainer = root.Q<VisualElement>("EnemyContainer");
            npcContainer = root.Q<VisualElement>("NPCContainer");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");

            questTypeDropdown.choices = Enum.GetNames(typeof(QuestType)).ToList();
            questTypeDropdown.value = QuestType.EnemySlays.ToString();
            enemyContainer.style.display = DisplayStyle.Flex;
            npcContainer.style.display = DisplayStyle.None;
            PopulateEnemyDropdown();

            questTypeDropdown.RegisterValueChangedCallback(OnQuestTypeChanged);
            root.Q<Button>("Return").clicked += ReturnToList;
            if (rewardsButton != null)
                rewardsButton.clicked += OpenRewardsMenu;
            if (closeButton != null)
                closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;
        }

        void OnDisable()
        {
            questTypeDropdown?.UnregisterValueChangedCallback(OnQuestTypeChanged);
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Q<Button>("Return").clicked -= ReturnToList;
            if (rewardsButton != null)
                rewardsButton.clicked -= OpenRewardsMenu;
            if (closeButton != null)
                closeButton.clicked -= CloseMenu;
            saveButton.clicked -= CreateNewObject;
        }

        public void SetupEditor(Quest quest)
        {
            editingData = quest.data;
            editBuffer = new EditBuffer<QuestData>(editingData);
            isNewQuest = false;
            saveButton.text = "Save Quest";
            PopulateFields();
            BindEditMode();
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked -= SaveExistingQuest;
            saveButton.clicked += SaveExistingQuest;
        }

        public void SetupStagingQuest(QuestData stagingData)
        {
            this.stagingData = stagingData;
            editBuffer = new EditBuffer<QuestData>(stagingData);
            isNewQuest = true;
            saveButton.text = "Save Quest";
            saveButton.clicked -= ReturnToList;
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked += CreateNewObject;
            PopulateFields();
            BindEditMode();
        }

        private QuestData CurrentQuestData => editBuffer != null ? editBuffer.Working : stagingData;

        private void PopulateFields()
        {
            var questData = CurrentQuestData;
            if (questData == null)
                return;

            nameInput.value = questData.title;
            contentInput.value = questData.content;
            questTypeDropdown.value = questData.type.ToString();
            targetAmountField.value = questData.targetAmount;
            UpdateQuestTypeUI(questData.type.ToString());
            if (rewardsSummary != null)
                UpdateRewardsSummary();

            if (questData.type == QuestType.EnemySlays)
            {
                var enemy = creatablesManager
                    .GetAll<AggresiveNpc>()
                    .FirstOrDefault(e => e.Id == questData.targetId);
                if (enemy != null)
                    enemyDropdown.value = enemy.data.name;
            }
            else if (questData.type == QuestType.NpcInteract)
            {
                var npc = creatablesManager
                    .GetAll<FriendlyNpc>()
                    .FirstOrDefault(n => n.Id == questData.targetId);
                if (npc != null)
                    npcDropdown.value = npc.data.name;
            }
        }

        private void BindEditMode()
        {
            var questData = CurrentQuestData;
            if (questData == null)
                return;

            nameInput.RegisterValueChangedCallback(evt => questData.title = evt.newValue);
            contentInput.RegisterValueChangedCallback(evt => questData.content = evt.newValue);
            targetAmountField.RegisterValueChangedCallback(evt =>
                questData.targetAmount = evt.newValue
            );
            questTypeDropdown.RegisterValueChangedCallback(evt =>
                questData.type = Enum.Parse<QuestType>(evt.newValue)
            );
            enemyDropdown.RegisterValueChangedCallback(evt =>
                questData.targetId = GetEnemyId(evt.newValue)
            );
            npcDropdown.RegisterValueChangedCallback(evt =>
                questData.targetId = GetNpcId(evt.newValue)
            );
        }

        private bool ValidateQuestFields(out string error)
        {
            if (string.IsNullOrEmpty(nameInput.value))
            {
                error = "Quest name is required.";
                return false;
            }

            if (nameInput.value.Length > MaxQuestNameLength)
            {
                error = $"Quest name must be at most {MaxQuestNameLength} characters.";
                return false;
            }

            if (string.IsNullOrEmpty(contentInput.value))
            {
                error = "Quest content is required.";
                return false;
            }

            if (contentInput.value.Length > MaxQuestContentLength)
            {
                error = $"Quest content must be at most {MaxQuestContentLength} characters.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void OpenRewardsMenu()
        {
            var menuInstance = resolver.Instantiate(rewardMenuPrefab);
            var rewardController = menuInstance.GetComponent<QuestRewardMenu>();
            rewardController?.SetupEditor(CurrentQuestData, isNewQuest);
            Destroy(gameObject);
        }

        private void UpdateRewardsSummary()
        {
            var questData = CurrentQuestData;
            if (questData == null || questData.rewards == null || questData.rewards.Count == 0)
            {
                rewardsSummary.text = "Rewards: None";
                return;
            }

            var summaries = questData.rewards.Select(reward =>
            {
                if (reward.rewardType == QuestRewardType.Gold)
                    return $"Gold: {reward.goldAmount}";
                if (reward.rewardType == QuestRewardType.Item)
                    return $"Item: {GetItemNameById(reward.itemId)}";
                if (reward.rewardType == QuestRewardType.LootTable)
                    return $"LootTable: {GetLootTableNameById(reward.lootTableId)}";
                return "Reward";
            });

            var summaryText = "Rewards: " + string.Join(", ", summaries);
            rewardsSummary.text = TruncateText(summaryText, 125);
        }

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }

        private void OnQuestTypeChanged(ChangeEvent<string> evt)
        {
            UpdateQuestTypeUI(evt.newValue);
        }

        private void UpdateQuestTypeUI(string questTypeValue)
        {
            enemyContainer.style.display = DisplayStyle.None;
            npcContainer.style.display = DisplayStyle.None;

            if (!Enum.TryParse<QuestType>(questTypeValue, out var questType))
                return;

            if (questType == QuestType.EnemySlays)
            {
                enemyContainer.style.display = DisplayStyle.Flex;
                PopulateEnemyDropdown();
            }
            else if (questType == QuestType.NpcInteract)
            {
                npcContainer.style.display = DisplayStyle.Flex;
                PopulateNpcDropdown();
            }
        }

        private void PopulateEnemyDropdown()
        {
            enemyDropdown.choices = creatablesManager
                .GetAll<AggresiveNpc>()
                .Select(e => e.data.name)
                .ToList();
        }

        private void PopulateNpcDropdown()
        {
            npcDropdown.choices = creatablesManager
                .GetAll<FriendlyNpc>()
                .Select(n => n.data.name)
                .ToList();
        }

        private string GetEnemyId(string name) =>
            creatablesManager.GetAll<AggresiveNpc>().FirstOrDefault(e => e.data.name == name)?.Id;

        private string GetNpcId(string name) =>
            creatablesManager.GetAll<FriendlyNpc>().FirstOrDefault(n => n.data.name == name)?.Id;

        private string GetItemNameById(string id)
        {
            ItemData item = creatablesManager
                .GetAll<ConsumableItem>()
                .Select(i => i.data)
                .FirstOrDefault(i => i.id == id);

            if (item != null)
                return item.name;

            item = creatablesManager
                .GetAll<Weapon>()
                .Select(i => i.data)
                .FirstOrDefault(i => i.id == id);

            return item?.name ?? "Unknown Item";
        }

        private string GetLootTableNameById(string id)
        {
            var lootTable = creatablesManager
                .GetAll<LootTable>()
                .FirstOrDefault(l => l.data.id == id);
            return lootTable?.data.name ?? "Unknown LootTable";
        }

        private bool ValidateQuestTarget(QuestType questType, string targetId, out string error)
        {
            if (questType == QuestType.EnemySlays && string.IsNullOrEmpty(targetId))
            {
                error = "An enemy must be selected for this quest type.";
                return false;
            }

            if (questType == QuestType.NpcInteract && string.IsNullOrEmpty(targetId))
            {
                error = "An NPC must be selected for this quest type.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void SaveExistingQuest()
        {
            if (!ValidateQuestFields(out var error))
            {
                ToastNotification.Show($"Failed to save quest: {error}", "error", Color.red);
                return;
            }

            var questType = Enum.Parse<QuestType>(questTypeDropdown.value);
            string targetId =
                questType == QuestType.EnemySlays
                    ? GetEnemyId(enemyDropdown.value)
                    : GetNpcId(npcDropdown.value);

            if (!ValidateQuestTarget(questType, targetId, out error))
            {
                ToastNotification.Show($"Failed to save quest: {error}", "error", Color.red);
                return;
            }

            if (editBuffer != null)
            {
                editBuffer.Working.targetId = targetId;
                editBuffer.Working.type = questType;
                editBuffer.Commit();
            }

            ToastNotification.Show("Quest updated successfully!", "success", Color.green);
            ReturnToList();
        }

        private void CreateNewObject()
        {
            if (!ValidateQuestFields(out var error))
            {
                ToastNotification.Show($"Failed to save quest: {error}", "error", Color.red);
                return;
            }

            var questType = Enum.Parse<QuestType>(questTypeDropdown.value);
            string targetId =
                questType == QuestType.EnemySlays
                    ? GetEnemyId(enemyDropdown.value)
                    : GetNpcId(npcDropdown.value);

            if (!ValidateQuestTarget(questType, targetId, out error))
            {
                ToastNotification.Show($"Failed to save quest: {error}", "error", Color.red);
                return;
            }

            var questData = CurrentQuestData;
            int targetAmount = questType == QuestType.NpcInteract ? 1 : targetAmountField.value;

            questData.title = nameInput.value;
            questData.content = contentInput.value ?? string.Empty;
            questData.type = questType;
            questData.targetId = targetId;
            questData.targetAmount = targetAmount;

            if (editBuffer != null)
                editBuffer.Commit();

            if (isNewQuest && editBuffer != null)
                creatablesManager.Add(new Quest(editBuffer.Original));

            ToastNotification.Show("Quest saved successfully!", "success", Color.green);
            ReturnToList();
        }

        private void ReturnToList()
        {
            OpenMenu(questsMenuPrefab);
        }
    }
}
