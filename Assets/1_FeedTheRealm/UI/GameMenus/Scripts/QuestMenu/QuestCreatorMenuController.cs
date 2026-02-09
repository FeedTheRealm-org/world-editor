using System;
using System.Linq;
using Enums;
using Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public class QuestCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GenericQuest currentQuest;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject questMenuPrefab;

    private TextField nameInput;
    private TextField contentInput;
    private DropdownField questTypeDropdown;
    private DropdownField enemyDropdown;
    private DropdownField npcDropdown;
    private IntegerField targetAmountField;
    private VisualElement enemyContainer;
    private VisualElement npcContainer;
    private Button saveButton;
    private Button returnButton;
    private Button closeButton;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        nameInput = root.Q<TextField>("NameField");
        contentInput = root.Q<TextField>("ContentField");
        questTypeDropdown = root.Q<DropdownField>("QuestTypeDropdown");
        enemyDropdown = root.Q<DropdownField>("EnemyDropdown");
        npcDropdown = root.Q<DropdownField>("NPCDropdown");
        targetAmountField = root.Q<IntegerField>("TargetAmount");
        enemyContainer = root.Q<VisualElement>("EnemyContainer");
        npcContainer = root.Q<VisualElement>("NPCContainer");
        saveButton = root.Q<Button>("SaveQuest");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");

        if (nameInput == null)
            logger.Log("Name input field not found in UI", this, Logging.LogType.Error);
        if (contentInput == null)
            logger.Log("Content input field not found in UI", this, Logging.LogType.Error);
        if (questTypeDropdown == null)
            logger.Log("QuestType dropdown not found in UI", this, Logging.LogType.Error);

        if (questTypeDropdown != null)
        {
            questTypeDropdown.choices = Enum.GetNames(typeof(QuestType)).ToList();
            questTypeDropdown.RegisterValueChangedCallback(OnQuestTypeChanged);
        }

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToQuestsMenu;
        closeButton.clicked += CloseMenu;

        if (currentQuest == null)
        {
            currentQuest = EditContext.GetAndClearObjectToEdit<GenericQuest>();
        }

        if (enemyContainer != null)
            enemyContainer.style.display = DisplayStyle.None;
        if (npcContainer != null)
            npcContainer.style.display = DisplayStyle.None;

        if (currentQuest != null)
        {
            PopulateFields();
        }
    }

    private void PopulateFields()
    {
        nameInput.value = currentQuest.name;
        contentInput.value = currentQuest.content ?? "";
        questTypeDropdown.value = currentQuest.questType.ToString();
        targetAmountField.value = currentQuest.targetAmount;

        UpdateQuestTypeUI(currentQuest.questType.ToString());

        if (currentQuest.questType == QuestType.EnemySlays && enemyDropdown != null)
        {
            var enemies = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Enemy)
                .Cast<GenericEnemy>()
                .ToList();
            var selectedEnemy = enemies.FirstOrDefault(e => e.ObjectId == currentQuest.targetId);
            if (selectedEnemy != null)
                enemyDropdown.value = selectedEnemy.DisplayName;
        }
        else if (currentQuest.questType == QuestType.NpcInteract && npcDropdown != null)
        {
            var npcs = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.NPC)
                .Cast<GenericNPC>()
                .ToList();
            var selectedNPC = npcs.FirstOrDefault(n => n.ObjectId == currentQuest.targetId);
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
        logger?.Log($"QuestCreator: Populated {enemies.Count} enemies", this, Logging.LogType.Info);
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

    private void OnSaveClicked()
    {
        if (string.IsNullOrEmpty(nameInput.value))
        {
            logger?.Log("Quest name is required", this, Logging.LogType.Warning);
            ToastNotification.Show("Quest name is required", "error", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(questTypeDropdown.value))
        {
            logger?.Log("Quest type is required", this, Logging.LogType.Warning);
            ToastNotification.Show("Quest type is required", "error", Color.red);
            return;
        }

        if (!Enum.TryParse<QuestType>(questTypeDropdown.value, out var questType))
        {
            logger?.Log("Invalid quest type", this, Logging.LogType.Warning);
            return;
        }

        string targetId = null;
        int targetAmount = questType == QuestType.NpcInteract ? 1 : targetAmountField.value;

        if (questType == QuestType.EnemySlays && enemyDropdown != null)
        {
            var enemies = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Enemy)
                .Cast<GenericEnemy>()
                .ToList();
            var selectedEnemy = enemies.FirstOrDefault(e => e.DisplayName == enemyDropdown.value);
            if (selectedEnemy == null)
            {
                ToastNotification.Show("Please select an enemy", "error", Color.red);
                return;
            }
            targetId = selectedEnemy.ObjectId;
        }
        else if (questType == QuestType.NpcInteract && npcDropdown != null)
        {
            var npcs = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.NPC)
                .Cast<GenericNPC>()
                .ToList();
            var selectedNPC = npcs.FirstOrDefault(n => n.DisplayName == npcDropdown.value);
            if (selectedNPC == null)
            {
                ToastNotification.Show("Please select an NPC", "error", Color.red);
                return;
            }
            targetId = selectedNPC.ObjectId;
        }

        if (currentQuest == null)
        {
            var questData = new QuestData(
                null,
                nameInput.value,
                contentInput.value ?? "",
                questType,
                targetId,
                targetAmount,
                null
            );
            currentQuest = new GenericQuest(questData);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.Quest, currentQuest);
            logger.Log(
                $"Created new quest: {currentQuest.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        else
        {
            currentQuest.name = nameInput.value;
            currentQuest.content = contentInput.value;
            currentQuest.questType = questType;
            currentQuest.targetId = targetId;
            currentQuest.targetAmount = targetAmount;
            logger.Log($"Updated quest: {currentQuest.DisplayName}", this, Logging.LogType.Info);
        }

        ToastNotification.Show("Quest saved successfully", "success", Color.green);

        ReturnToQuestsMenu();
    }

    private void ReturnToQuestsMenu()
    {
        OpenMenu(questMenuPrefab);
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToQuestsMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
        if (questTypeDropdown != null)
            questTypeDropdown.UnregisterValueChangedCallback(OnQuestTypeChanged);
    }
}
