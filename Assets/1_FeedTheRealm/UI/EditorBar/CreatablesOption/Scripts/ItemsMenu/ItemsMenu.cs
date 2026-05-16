using FeedTheRealm.Core.WorldObjects;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.ItemsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ItemsMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;

        [SerializeField]
        private GameObject consumableCreatorMenuPrefab;

        [SerializeField]
        private GameObject weaponCreatorMenuPrefab;

        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private readonly WorldPrefabProvider prefabProvider;

        private Button closeButton;
        private Button addConsumableButton;
        private Button addWeaponButton;

        private Creatable editingData;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addConsumableButton = root.Q<Button>("AddConsumableItem");
            addWeaponButton = root.Q<Button>("AddWeaponItem");

            closeButton.clicked += CloseMenu;
            addConsumableButton.clicked += () => OpenCreatorMenu(consumableCreatorMenuPrefab);
            addWeaponButton.clicked += () => OpenCreatorMenu(weaponCreatorMenuPrefab);

            PopulateList();
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
        }

        private void PopulateList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var list = root.Q<ListView>("ItemsList");
            list.Clear();

            foreach (var consumable in creatablesManager.GetAll<ConsumableItem>())
                AddListEntry(
                    list,
                    consumable,
                    consumable.data.name,
                    "Consumable",
                    consumableCreatorMenuPrefab
                );

            foreach (var weapon in creatablesManager.GetAll<Weapon>())
                AddListEntry(list, weapon, weapon.data.name, "Weapon", weaponCreatorMenuPrefab);
        }

        private void AddListEntry(
            ListView list,
            Creatable creatable,
            string displayName,
            string type,
            GameObject creatorPrefab
        )
        {
            var entry = itemListTemplate.Instantiate();
            entry.Q<Label>("Header").text = displayName;

            var typeLabel = entry.Q<Label>("Type");
            if (typeLabel != null)
                typeLabel.text = type;

            entry.Q<Button>("Edit").clicked += () =>
            {
                editingData = creatable;
                OpenMenu(creatorPrefab);
            };

            entry.Q<Button>("Delete").clicked += () =>
            {
                var confirmPopup = Instantiate(prefabProvider.confirmPopup);
                var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();

                dialogController.Show(
                    title: "Delete Item",
                    question: "Are you sure you want to delete this item? This cannot be undone.",
                    onConfirm: () =>
                    {
                        foreach (var lootTable in creatablesManager.GetAll<LootTable>())
                        {
                            lootTable.data.lootItems.RemoveAll(i => i.id == creatable.Id);
                        }

                        foreach (var shop in creatablesManager.GetAll<Shop>())
                        {
                            shop.data.products.RemoveAll(p =>
                                p.productId == creatable.Id && !p.IsCosmetic
                            );
                        }
                        if (creatable is Weapon)
                            creatablesManager.Delete<Weapon>(creatable.Id);
                        else if (creatable is ConsumableItem)
                            creatablesManager.Delete<ConsumableItem>(creatable.Id);
                        entry.RemoveFromHierarchy();
                    },
                    onCancel: () => { }
                );
            };

            list.hierarchy.Add(entry);
        }

        private void OpenCreatorMenu(GameObject prefab)
        {
            editingData = null;
            OpenMenu(prefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);

            if (editingData != null)
            {
                if (editingData is ConsumableItem consumable)
                    menuInstance.GetComponent<ConsumableItemCreatorMenu>()?.SetupEditor(consumable);
                else if (editingData is Weapon weapon)
                    menuInstance.GetComponent<WeaponCreatorMenu>()?.SetupEditor(weapon);
            }

            Destroy(gameObject);
        }
    }
}
