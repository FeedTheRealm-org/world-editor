using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldObjects;
using FTR.UI;
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

        [Inject]
        private readonly WorldPrefabProvider prefabProvider;

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
            var enemiesList = root.Q<ScrollView>("EnemiesList");
            enemiesList.Clear();

            foreach (AggresiveNpc enemy in creatablesManager.GetAll<AggresiveNpc>())
            {
                VisualElement enemyEntry = itemListTemplate.Instantiate();

                var weapon = creatablesManager.GetById<Weapon>(enemy.data.weaponId);
                var lootTable = creatablesManager.GetById<LootTable>(enemy.data.lootTableId);

                enemyEntry.Q<Label>("Header").text = enemy.data.name;
                enemyEntry.Q<Label>("HPLabel").text = $"{enemy.data.healthPoints}";
                enemyEntry.Q<Label>("WeaponLabel").text = $"{weapon?.data.name ?? "None"}";
                enemyEntry.Q<Label>("LootLabel").text = $"{lootTable?.data.name ?? "None"}";

                enemyEntry.Q<Button>("Edit").clicked += () => OnEdit(enemy);
                enemyEntry.Q<Button>("Delete").clicked += () => OnDeleteEnemy(enemy, enemyEntry);

                enemiesList.Add(enemyEntry);
            }
        }

        void OnEdit(AggresiveNpc enemy)
        {
            editingData = enemy;
            OpenMenu(aggresiveNpcCreatorMenuPrefab);
        }

        void OnDeleteEnemy(AggresiveNpc enemy, VisualElement enemyListEntry)
        {
            var confirmPopup = Instantiate(prefabProvider.confirmPopup);
            var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
            dialogController.Show(
                title: "Delete Enemy",
                question: $"Are you sure you want to delete the enemy '{enemy.data.name}'? This cannot be undone.",
                onConfirm: () =>
                {
                    var spawners = FindObjectsByType<AggresiveNpcSpawnerObject>(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None
                    );
                    foreach (var spawner in spawners)
                    {
                        if (spawner.data != null && spawner.data.EnemyId == enemy.data.id)
                        {
                            spawner.data.EnemyId = string.Empty;
                        }
                    }

                    creatablesManager.Delete<AggresiveNpc>(enemy.data.id);
                    enemyListEntry.RemoveFromHierarchy();
                },
                onCancel: () => { }
            );
        }

        void OnDisable()
        {
            addEnemyButton.clicked -= AddEnemy;
            closeButton.clicked -= CloseMenu;
        }

        private void AddEnemy()
        {
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
            }
            Destroy(gameObject);
        }
    }
}
