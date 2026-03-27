using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.QuestMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestsMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject createQuestMenuPrefab;

        [SerializeField]
        private CreatablesManager creatorObjectLibrary;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;
        private Button closeButton;
        private Button addQuestButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addQuestButton = root.Q<Button>("AddQuest");

            addQuestButton.clicked += AddQuest;
            closeButton.clicked += CloseMenu;

            PopulateQuestsList();
        }

        private void PopulateQuestsList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var questsList = root.Q<ListView>("QuestsList");

            if (questsList == null)
            {
                logger?.Log("QuestsList ListView not found in UI", this, Logging.LogType.Error);
                return;
            }

            questsList.Clear();

            if (itemListTemplate == null)
            {
                logger?.Log("itemListTemplate is not assigned", this, Logging.LogType.Error);
                return;
            }

            // foreach (
            //     GenericQuest quest in creatorObjectLibrary.GetCreatables(
            //         CreatableObjectCategories.Quest
            //     )
            // )
            // {
            //     VisualElement questEntry = itemListTemplate.Instantiate();
            //     var headerLabel = questEntry.Q<Label>("Header");
            //     if (headerLabel != null)
            //     {
            //         headerLabel.text = quest.DisplayName;
            //     }

            //     var editButton = questEntry.Q<Button>("Edit");
            //     var deleteButton = questEntry.Q<Button>("Delete");

            //     var typeLabel = questEntry.Q<Label>("Type");
            //     if (typeLabel != null)
            //         typeLabel.text = "Quest";

            //     if (editButton != null)
            //         editButton.clicked += () => OnEditQuest(quest);
            //     if (deleteButton != null)
            //         deleteButton.clicked += () => OnDeleteQuest(quest, questEntry);

            //     questsList.hierarchy.Add(questEntry);
            // }
        }

        // void OnEditQuest(CreatorObject quest)
        // {
        //     logger.Log("Editing quest: " + quest.DisplayName, this, Logging.LogType.Info);

        //     EditContext.SetObjectToEdit(quest);

        //     OpenMenu(createQuestMenuPrefab);
        // }

        // void OnDeleteQuest(CreatorObject quest, VisualElement questListEntry)
        // {
        //     logger.Log("Deleting quest: " + quest.DisplayName, this, Logging.LogType.Info);
        //     creatorObjectLibrary.RemoveCreatable(CreatableObjectCategories.Quest, quest);
        //     questListEntry.RemoveFromHierarchy();
        // }

        void OnDisable()
        {
            addQuestButton.clicked -= AddQuest;
            closeButton.clicked -= CloseMenu;
        }

        private void AddQuest()
        {
            logger.Log("Opening Create Quest Menu", this, Logging.LogType.Info);
            OpenMenu(createQuestMenuPrefab);
        }
    }
}
