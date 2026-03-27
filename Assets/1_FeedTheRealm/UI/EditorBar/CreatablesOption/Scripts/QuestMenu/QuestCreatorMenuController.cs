using System;
using System.Linq;
using Enums;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.QuestMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject questsMenuPrefab;
        private QuestData editingData;
        private TextField nameInput;
        private TextField contentInput;
        private DropdownField questTypeDropdown;
        private DropdownField enemyDropdown;
        private DropdownField npcDropdown;
        private IntegerField targetAmountField;
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
            closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;
        }

        void OnDisable()
        {
            questTypeDropdown?.UnregisterValueChangedCallback(OnQuestTypeChanged);
        }

        public void SetupEditor(Quest quest)
        {
            editingData = quest.data;
            PopulateFields();
            BindEditMode();
            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Return to List";
            saveButton.clicked += ReturnToList;
        }

        private void PopulateFields()
        {
            nameInput.value = editingData.title;
            contentInput.value = editingData.content;
            questTypeDropdown.value = editingData.type.ToString();
            targetAmountField.value = editingData.targetAmount;
            UpdateQuestTypeUI(editingData.type.ToString());

            if (editingData.type == QuestType.EnemySlays)
            {
                var enemy = creatablesManager
                    .GetAll<AggresiveNpc>()
                    .FirstOrDefault(e => e.Id == editingData.targetId);
                if (enemy != null)
                    enemyDropdown.value = enemy.data.name;
            }
            else if (editingData.type == QuestType.NpcInteract)
            {
                var npc = creatablesManager
                    .GetAll<FriendlyNpc>()
                    .FirstOrDefault(n => n.Id == editingData.targetId);
                if (npc != null)
                    npcDropdown.value = npc.data.name;
            }
        }

        private void BindEditMode()
        {
            nameInput.RegisterValueChangedCallback(evt => editingData.title = evt.newValue);
            contentInput.RegisterValueChangedCallback(evt => editingData.content = evt.newValue);
            targetAmountField.RegisterValueChangedCallback(evt =>
                editingData.targetAmount = evt.newValue
            );
            questTypeDropdown.RegisterValueChangedCallback(evt =>
                editingData.type = Enum.Parse<QuestType>(evt.newValue)
            );
            enemyDropdown.RegisterValueChangedCallback(evt =>
                editingData.targetId = GetEnemyId(evt.newValue)
            );
            npcDropdown.RegisterValueChangedCallback(evt =>
                editingData.targetId = GetNpcId(evt.newValue)
            );
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

        private void CreateNewObject()
        {
            var questType = Enum.Parse<QuestType>(questTypeDropdown.value);
            string targetId =
                questType == QuestType.EnemySlays
                    ? GetEnemyId(enemyDropdown.value)
                    : GetNpcId(npcDropdown.value);
            int targetAmount = questType == QuestType.NpcInteract ? 1 : targetAmountField.value;

            var questData = new QuestData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                contentInput.value ?? "",
                questType,
                targetId,
                targetAmount,
                null
            );

            creatablesManager.Add(new Quest(questData));
            ReturnToList();
        }

        private void ReturnToList() => OpenMenu(questsMenuPrefab);
    }
}
