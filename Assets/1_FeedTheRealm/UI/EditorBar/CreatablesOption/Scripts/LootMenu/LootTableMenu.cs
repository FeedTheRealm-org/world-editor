using FeedTheRealm.Core.WorldObjects;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldObjects;
using FTR.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.LootMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class LootTableMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;

        [SerializeField]
        private GameObject lootTableCreatorMenuPrefab;

        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private readonly WorldPrefabProvider prefabProvider;

        private Button closeButton;
        private Button addLootTableButton;

        // This holds the reference to the table we want to edit
        private LootTable editingData;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Query elements based on your UXML names
            closeButton = root.Q<Button>("Close");
            addLootTableButton = root.Q<Button>("AddLootTable");

            closeButton.clicked += CloseMenu;
            addLootTableButton.clicked += () => OpenCreatorMenu(lootTableCreatorMenuPrefab);

            PopulateList();
        }

        void OnDisable()
        {
            if (closeButton != null)
                closeButton.clicked -= CloseMenu;
        }

        private void PopulateList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var list = root.Q<ScrollView>("LootTablesList");
            list.Clear();

            // Retrieve all LootTable creatables from the manager
            foreach (var lootTable in creatablesManager.GetAll<LootTable>())
            {
                AddListEntry(
                    list,
                    lootTable,
                    lootTable.data.name, // Assuming LootTable has a .data.name property
                    "Loot Table",
                    lootTableCreatorMenuPrefab
                );
            }
        }

        private void AddListEntry(
            ScrollView list,
            LootTable creatable,
            string displayName,
            string type,
            GameObject creatorPrefab
        )
        {
            var entry = itemListTemplate.Instantiate();
            entry.Q<Label>("Header").text = displayName;

            var itemsAmountLabel = entry.Q<Label>("LootLabel");
            itemsAmountLabel.text = creatable.data.lootItems.Count.ToString();

            // Handle Edit Logic
            entry.Q<Button>("Edit").clicked += () =>
            {
                editingData = creatable;
                OpenMenu(creatorPrefab);
            };

            // Handle Delete Logic
            entry.Q<Button>("Delete").clicked += () =>
            {
                var confirmPopup = Instantiate(prefabProvider.confirmPopup);
                var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
                dialogController.Show(
                    title: "Delete Loot Table",
                    question: $"Are you sure you want to delete the loot table '{displayName}'? This cannot be undone.",
                    onConfirm: () =>
                    {
                        var chests = FindObjectsByType<ChestObject>(
                            FindObjectsInactive.Exclude,
                            FindObjectsSortMode.None
                        );
                        foreach (var chest in chests)
                        {
                            if (chest.data != null && chest.data.lootTableId == creatable.data.id)
                            {
                                chest.data.lootTableId = string.Empty;
                            }
                        }

                        foreach (var enemy in creatablesManager.GetAll<AggresiveNpc>())
                        {
                            enemy.data.lootTableId =
                                enemy.data.lootTableId == creatable.data.id
                                    ? string.Empty
                                    : enemy.data.lootTableId;
                        }

                        creatablesManager.Delete<LootTable>(creatable.Id);
                        entry.RemoveFromHierarchy();
                    },
                    onCancel: () => { }
                );
            };

            list.Add(entry);
        }

        private void OpenCreatorMenu(GameObject prefab)
        {
            editingData = null; // Clear state so the next menu knows it's a "New" item
            OpenMenu(prefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            // Use VContainer resolver to instantiate and inject dependencies
            var menuInstance = resolver.Instantiate(menuPrefab);

            if (editingData != null)
            {
                // Try to get the creator component and pass the existing data
                var creatorMenu = menuInstance.GetComponent<LootTableCreatorMenu>();
                if (creatorMenu != null)
                {
                    creatorMenu.SetupEditor(editingData);
                }
            }

            Destroy(gameObject);
        }
    }
}
