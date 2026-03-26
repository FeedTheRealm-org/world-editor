using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.EnemyMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class EnemiesMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject createEnemyMenuPrefab;

        [SerializeField]
        private CreatablesManager creatorObjectLibrary;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;
        private Button closeButton;
        private Button addEnemyButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addEnemyButton = root.Q<Button>("AddEnemy");

            addEnemyButton.clicked += AddEnemy;
            closeButton.clicked += CloseMenu;

            PopulateEnemysList();
        }

        private void PopulateEnemysList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var enemiesList = root.Q<ListView>("EnemiesList");
            enemiesList.Clear();

            foreach (AggresiveNpc enemy in creatorObjectLibrary.GetAll<AggresiveNpc>())
            {
                VisualElement enemyEntry = itemListTemplate.Instantiate();
                var headerLabel = enemyEntry.Q<Label>("Header");
                headerLabel.text = enemy.data.name;

                var editButton = enemyEntry.Q<Button>("Edit");
                var deleteButton = enemyEntry.Q<Button>("Delete");

                var typeLabel = enemyEntry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Enemy";

                editButton.clicked += () => OnEditEnemy(enemy);
                deleteButton.clicked += () => OnDeleteEnemy(enemy, enemyEntry);

                enemiesList.hierarchy.Add(enemyEntry);
            }
        }

        void OnEditEnemy(AggresiveNpc enemy)
        {
            logger.Log("Editing enemy: " + enemy.data.name, this, Logging.LogType.Info);

            // EditContext.SetObjectToEdit(enemy);
            // OpenMenu(createEnemyMenuPrefab);
        }

        void OnDeleteEnemy(AggresiveNpc enemy, VisualElement enemyListEntry)
        {
            logger.Log("Deleting enemy: " + enemy.data.name, this, Logging.LogType.Info);
            creatorObjectLibrary.Delete<AggresiveNpc>(enemy.data.id);
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
            OpenMenu(createEnemyMenuPrefab);
        }
    }
}
