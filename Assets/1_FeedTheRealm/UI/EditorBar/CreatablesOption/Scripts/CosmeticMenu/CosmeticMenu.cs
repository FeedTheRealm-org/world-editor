using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.UI;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.CosmeticMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class CosmeticListMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject cosmeticCreatorMenuPrefab;

        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private readonly WorldPrefabProvider prefabProvider;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;
        private Button closeButton;
        private Button addCosmeticButton;
        private Cosmetic editingData;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addCosmeticButton = root.Q<Button>("AddCosmetic");

            addCosmeticButton.clicked += AddCosmetic;
            closeButton.clicked += CloseMenu;

            PopulateItemsList();
        }

        private void PopulateItemsList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var cosmeticsList = root.Q<ScrollView>("CosmeticsList");
            cosmeticsList.Clear();

            foreach (Cosmetic cosmetic in creatablesManager.GetAll<Cosmetic>())
            {
                VisualElement cosmeticEntry = itemListTemplate.Instantiate();
                var headerLabel = cosmeticEntry.Q<Label>("Header");

                headerLabel.text = cosmetic.data.name;
                var editButton = cosmeticEntry.Q<Button>("Edit");
                var deleteButton = cosmeticEntry.Q<Button>("Delete");

                var typeLabel = cosmeticEntry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Cosmetic";

                editButton.clicked += () => OnEdit(cosmetic);
                deleteButton.clicked += () => OnDeleteCosmetic(cosmetic, cosmeticEntry);

                cosmeticsList.Add(cosmeticEntry);
            }
        }

        void OnEdit(Cosmetic cosmetic)
        {
            logger.Log("Editing cosmetic: " + cosmetic.data.name, this, Logging.LogType.Info);
            editingData = cosmetic;
            OpenMenu(cosmeticCreatorMenuPrefab);
        }

        void OnDeleteCosmetic(Cosmetic cosmetic, VisualElement cosmeticListEntry)
        {
            var confirmPopup = Instantiate(prefabProvider.confirmPopup);
            var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
            dialogController.Show(
                title: "Delete Cosmetic",
                question: $"Are you sure you want to delete the cosmetic '{cosmetic.data.name}'? This cannot be undone.",
                onConfirm: () =>
                {
                    logger.Log(
                        "Deleting cosmetic: " + cosmetic.data.name,
                        this,
                        Logging.LogType.Info
                    );

                    foreach (var shop in creatablesManager.GetAll<Shop>())
                    {
                        shop.data.products.RemoveAll(p =>
                            p.IsCosmetic
                            && (
                                p.productId == cosmetic.data.id
                                || (
                                    cosmetic.data.categories != null
                                    && System.Linq.Enumerable.Any(
                                        cosmetic.data.categories.Values,
                                        v =>
                                            !string.IsNullOrEmpty(v.url_id)
                                            && v.url_id == p.productId
                                    )
                                )
                            )
                        );
                    }

                    creatablesManager.Delete<Cosmetic>(cosmetic.data.id);
                    cosmeticListEntry.RemoveFromHierarchy();
                },
                onCancel: () => { }
            );
        }

        void OnDisable()
        {
            addCosmeticButton.clicked -= AddCosmetic;
            closeButton.clicked -= CloseMenu;
        }

        private void AddCosmetic()
        {
            logger.Log("Opening Create Cosmetic Menu", this, Logging.LogType.Info);
            editingData = null;
            OpenMenu(cosmeticCreatorMenuPrefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);
            if (editingData != null)
            {
                var creatorMenu = menuInstance.GetComponent<CosmeticCreatorMenu>();
                creatorMenu.SetupEditor(editingData);
                logger.Log(
                    "Opened Edit Cosmetic Menu for: " + editingData.data.name,
                    this,
                    Logging.LogType.Info
                );
            }
            Destroy(gameObject);
        }
    }
}
