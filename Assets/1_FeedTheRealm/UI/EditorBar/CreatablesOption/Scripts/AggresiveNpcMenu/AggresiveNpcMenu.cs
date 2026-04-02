using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.EnemyMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class AggresiveNpcMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject aggresiveNpcCreatorMenuPrefab;

        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;
        private Button closeButton;
        private Button addEnemyButton;
        private AggresiveNpc editingData;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addEnemyButton = root.Q<Button>("AddEnemy");

            addEnemyButton.clicked += AddEnemy;
            closeButton.clicked += CloseMenu;

            PopulateItemsList();
        }

        private void PopulateItemsList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var enemiesList = root.Q<ListView>("EnemiesList");
            enemiesList.Clear();

            foreach (AggresiveNpc enemy in creatablesManager.GetAll<AggresiveNpc>())
            {
                VisualElement enemyEntry = itemListTemplate.Instantiate();
                var headerLabel = enemyEntry.Q<Label>("Header");
                headerLabel.text = enemy.data.name;

                var editButton = enemyEntry.Q<Button>("Edit");
                var deleteButton = enemyEntry.Q<Button>("Delete");

                var typeLabel = enemyEntry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Enemy";

                editButton.clicked += () => OnEdit(enemy);
                deleteButton.clicked += () => OnDeleteEnemy(enemy, enemyEntry);

                enemiesList.hierarchy.Add(enemyEntry);
            }
        }

        void OnEdit(AggresiveNpc enemy)
        {
            logger.Log("Editing enemy: " + enemy.data.name, this, Logging.LogType.Info);
            editingData = enemy;
            OpenMenu(aggresiveNpcCreatorMenuPrefab);
        }

        void OnDeleteEnemy(AggresiveNpc enemy, VisualElement enemyListEntry)
        {
            logger.Log("Deleting enemy: " + enemy.data.name, this, Logging.LogType.Info);
            creatablesManager.Delete<AggresiveNpc>(enemy.data.id);
            enemyListEntry.RemoveFromHierarchy();
        }

        void OnDisable()
        {
            addEnemyButton.clicked -= AddEnemy;
            closeButton.clicked -= CloseMenu;
        }

        private void AddEnemy()
        {
            logger.Log("Opening Create Enemy Menu", this, Logging.LogType.Info);
            editingData = null;
            OpenMenu(aggresiveNpcCreatorMenuPrefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);
            if (editingData != null)
            {
                var creatorMenu = menuInstance.GetComponent<AggresiveNpcCreatorMenu>();
                creatorMenu.SetupEditor(editingData);
                logger.Log(
                    "Opened Edit Enemy Menu for: " + editingData.data.name,
                    this,
                    Logging.LogType.Info
                );
            }
            Destroy(gameObject);
        }
    }
}
