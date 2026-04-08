using System;
using Enums;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.QuestMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestsMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject createQuestMenuPrefab;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;

        [Inject]
        private CreatablesManager creatablesManager;

        private Button closeButton;
        private Button addQuestButton;
        private Quest editingQuest;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addQuestButton = root.Q<Button>("AddQuest");

            addQuestButton.clicked += AddQuest;
            closeButton.clicked += CloseMenu;

            PopulateQuestsList();
        }

        void OnDisable()
        {
            addQuestButton.clicked -= AddQuest;
            closeButton.clicked -= CloseMenu;
        }

        private void PopulateQuestsList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var questsList = root.Q<ListView>("QuestsList");
            questsList.Clear();

            foreach (Quest quest in creatablesManager.GetAll<Quest>())
            {
                var entry = itemListTemplate.Instantiate();
                entry.Q<Label>("Header").text = quest.data.title;

                var typeLabel = entry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Quest";

                entry.Q<Button>("Edit").clicked += () => OnEditQuest(quest);
                entry.Q<Button>("Delete").clicked += () => OnDeleteQuest(quest, entry);

                questsList.hierarchy.Add(entry);
            }
        }

        private void OnEditQuest(Quest quest)
        {
            logger.Log("Editing quest: " + quest.data.title, this, Logging.LogType.Info);
            editingQuest = quest;
            OpenMenu(createQuestMenuPrefab);
        }

        private void OnDeleteQuest(Quest quest, VisualElement entry)
        {
            logger.Log("Deleting quest: " + quest.data.title, this, Logging.LogType.Info);
            creatablesManager.Delete<Quest>(quest.data.id);
            entry.RemoveFromHierarchy();
        }

        private void AddQuest()
        {
            logger.Log("Opening Create Quest Menu", this, Logging.LogType.Info);
            editingQuest = null;
            OpenMenu(createQuestMenuPrefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);
            var editor = menuInstance.GetComponent<QuestCreatorMenu>();
            if (editingQuest != null)
            {
                editor?.SetupEditor(editingQuest);
            }
            else
            {
                var stagingQuestData = new QuestData(
                    Guid.NewGuid().ToString(),
                    string.Empty,
                    string.Empty,
                    QuestType.EnemySlays,
                    string.Empty,
                    1,
                    null,
                    null
                );
                editor?.SetupStagingQuest(stagingQuestData);
            }

            Destroy(gameObject);
        }
    }
}
