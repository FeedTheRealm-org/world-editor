using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.UI;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.QuestMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestRewardMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject questCreatorMenuPrefab;

        private QuestData questData;
        private bool isNewQuest;

        private DropdownField rewardTypeDropdown;
        private DropdownField itemDropdown;
        private DropdownField lootTableDropdown;
        private IntegerField goldAmountField;
        private ScrollView rewardsScrollView;
        private Button addRewardButton;
        private Button returnButton;
        private Button returnToQuestEditorButton;
        private Button closeButton;
        private VisualElement itemRow;
        private VisualElement lootTableRow;
        private VisualElement goldRow;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            rewardTypeDropdown = root.Q<DropdownField>("RewardTypeDropdown");
            itemDropdown = root.Q<DropdownField>("ItemRewardDropdown");
            lootTableDropdown = root.Q<DropdownField>("LootTableRewardDropdown");
            goldAmountField = root.Q<IntegerField>("GoldAmountField");
            rewardsScrollView = root.Q<ScrollView>("RewardsScrollView");
            addRewardButton = root.Q<Button>("AddRewardButton");
            returnButton = root.Q<Button>("Return");
            closeButton = root.Q<Button>("Close");
            itemRow = root.Q<VisualElement>("ItemRow");
            lootTableRow = root.Q<VisualElement>("LootTableRow");
            goldRow = root.Q<VisualElement>("GoldRow");

            rewardTypeDropdown.choices = Enum.GetNames(typeof(QuestRewardType)).ToList();
            rewardTypeDropdown.value = QuestRewardType.Gold.ToString();
            UpdateRewardTypeUI(rewardTypeDropdown.value);

            rewardTypeDropdown.RegisterValueChangedCallback(OnRewardTypeChanged);
            addRewardButton.clicked += AddReward;
            returnButton.clicked += ReturnToQuestEditor;
            returnToQuestEditorButton = root.Q<Button>("ReturnToQuestEditor");
            if (returnToQuestEditorButton != null)
                returnToQuestEditorButton.clicked += ReturnToQuestEditor;
            closeButton.clicked += CloseMenu;
        }

        void OnDisable()
        {
            addRewardButton.clicked -= AddReward;
            returnButton.clicked -= ReturnToQuestEditor;
            if (returnToQuestEditorButton != null)
                returnToQuestEditorButton.clicked -= ReturnToQuestEditor;
            closeButton.clicked -= CloseMenu;
            rewardTypeDropdown?.UnregisterValueChangedCallback(OnRewardTypeChanged);
        }

        public void SetupEditor(QuestData questData, bool isNewQuest)
        {
            this.questData = questData;
            this.isNewQuest = isNewQuest;

            if (this.questData != null && this.questData.rewards == null)
                this.questData.rewards = new List<QuestRewardData>();

            RefreshRewardList();
        }

        private void OnRewardTypeChanged(ChangeEvent<string> evt)
        {
            UpdateRewardTypeUI(evt.newValue);
        }

        private void UpdateRewardTypeUI(string rewardTypeValue)
        {
            itemRow.style.display = DisplayStyle.None;
            lootTableRow.style.display = DisplayStyle.None;
            goldRow.style.display = DisplayStyle.None;

            if (!Enum.TryParse<QuestRewardType>(rewardTypeValue, out var rewardType))
                return;

            switch (rewardType)
            {
                case QuestRewardType.Gold:
                    goldRow.style.display = DisplayStyle.Flex;
                    break;
                case QuestRewardType.Item:
                    itemRow.style.display = DisplayStyle.Flex;
                    PopulateItemDropdown();
                    break;
                case QuestRewardType.LootTable:
                    lootTableRow.style.display = DisplayStyle.Flex;
                    PopulateLootTableDropdown();
                    break;
            }
        }

        private void PopulateItemDropdown()
        {
            var allItems = new List<ItemData>();
            allItems.AddRange(creatablesManager.GetAll<ConsumableItem>().Select(i => i.data));
            allItems.AddRange(creatablesManager.GetAll<Weapon>().Select(i => i.data));

            itemDropdown.choices = allItems.Select(i => i.name).ToList();
            itemDropdown.userData = allItems;

            if (itemDropdown.choices.Count > 0)
                itemDropdown.value = itemDropdown.choices[0];
        }

        private void PopulateLootTableDropdown()
        {
            var lootTables = creatablesManager.GetAll<LootTable>().Select(l => l.data).ToList();
            lootTableDropdown.choices = lootTables.Select(l => l.name).ToList();
            lootTableDropdown.userData = lootTables;

            if (lootTableDropdown.choices.Count > 0)
                lootTableDropdown.value = lootTableDropdown.choices[0];
        }

        private string GetSelectedItemId()
        {
            var items = itemDropdown.userData as List<ItemData>;
            return items?.FirstOrDefault(i => i.name == itemDropdown.value)?.id;
        }

        private string GetSelectedLootTableId()
        {
            var lootTables = lootTableDropdown.userData as List<LootTableData>;
            return lootTables?.FirstOrDefault(l => l.name == lootTableDropdown.value)?.id;
        }

        private void AddReward()
        {
            if (questData == null)
                return;

            if (!Enum.TryParse<QuestRewardType>(rewardTypeDropdown.value, out var rewardType))
                return;

            QuestRewardData reward;
            switch (rewardType)
            {
                case QuestRewardType.Gold:
                    reward = new QuestRewardData(
                        rewardType,
                        goldAmountField.value,
                        string.Empty,
                        string.Empty
                    );
                    break;
                case QuestRewardType.Item:
                    var itemId = GetSelectedItemId();
                    if (string.IsNullOrEmpty(itemId))
                        return;

                    reward = new QuestRewardData(rewardType, 0, itemId, string.Empty);
                    break;
                case QuestRewardType.LootTable:
                    var lootTableId = GetSelectedLootTableId();
                    if (string.IsNullOrEmpty(lootTableId))
                        return;

                    reward = new QuestRewardData(rewardType, 0, string.Empty, lootTableId);
                    break;
                default:
                    reward = new QuestRewardData(rewardType, 0, string.Empty, string.Empty);
                    break;
            }

            if (questData.rewards == null)
                questData.rewards = new List<QuestRewardData>();

            questData.rewards.Add(reward);
            RefreshRewardList();
        }

        private void RefreshRewardList()
        {
            rewardsScrollView.Clear();
            if (questData == null || questData.rewards == null)
                return;

            foreach (var reward in questData.rewards)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.paddingBottom = 4;
                row.style.paddingTop = 4;
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = Color.gray;

                var rewardText = GetRewardDisplayText(reward);
                var label = new Label(rewardText);
                label.style.color = Color.white;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                label.style.fontSize = 14;

                var removeBtn = new Button(() =>
                {
                    questData.rewards.Remove(reward);
                    RefreshRewardList();
                })
                {
                    text = "X",
                };

                removeBtn.style.width = 26;
                removeBtn.style.height = 26;
                removeBtn.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
                removeBtn.style.color = Color.white;

                row.Add(label);
                row.Add(removeBtn);
                rewardsScrollView.Add(row);
            }
        }

        private string GetRewardDisplayText(QuestRewardData reward)
        {
            if (reward.rewardType == QuestRewardType.Gold)
                return $"Gold: {reward.goldAmount}";
            if (reward.rewardType == QuestRewardType.Item)
                return $"Item: {GetItemNameById(reward.itemId)}";
            if (reward.rewardType == QuestRewardType.LootTable)
                return $"LootTable: {GetLootTableNameById(reward.lootTableId)}";
            return "Reward";
        }

        private string GetItemNameById(string id)
        {
            var allItems = new List<ItemData>();
            allItems.AddRange(creatablesManager.GetAll<ConsumableItem>().Select(i => i.data));
            allItems.AddRange(creatablesManager.GetAll<Weapon>().Select(i => i.data));
            return allItems.FirstOrDefault(i => i.id == id)?.name ?? "No Item";
        }

        private string GetLootTableNameById(string id)
        {
            var lootTable = creatablesManager
                .GetAll<LootTable>()
                .FirstOrDefault(l => l.data.id == id);
            return lootTable?.data.name ?? "No LootTable";
        }

        private void ReturnToQuestEditor()
        {
            var menuInstance = resolver.Instantiate(questCreatorMenuPrefab);
            var controller = menuInstance.GetComponent<QuestCreatorMenu>();
            if (controller != null)
            {
                if (isNewQuest)
                    controller.SetupStagingQuest(questData);
                else
                    controller.SetupEditor(new Quest(questData));
            }

            Destroy(gameObject);
        }
    }
}
