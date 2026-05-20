using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldObjects;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.NPCMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class FriendlyNpcMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject createNPCMenuPrefab;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;

        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private readonly WorldPrefabProvider prefabProvider;

        private Button closeButton;
        private Button addNPCButton;
        private FriendlyNpc editingNpc;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addNPCButton = root.Q<Button>("AddNPC");

            addNPCButton.clicked += AddNPC;
            closeButton.clicked += CloseMenu;

            PopulateNPCsList();
        }

        void OnDisable()
        {
            addNPCButton.clicked -= AddNPC;
            closeButton.clicked -= CloseMenu;
        }

        private void PopulateNPCsList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var npcsList = root.Q<ScrollView>("NPCList");
            npcsList.Clear();

            foreach (FriendlyNpc npc in creatablesManager.GetAll<FriendlyNpc>())
            {
                var amountOfDialogs = npc.data.dialogProgression.Count;
                var entry = itemListTemplate.Instantiate();
                entry.Q<Label>("Header").text = npc.data.name;
                entry.Q<Label>("DialogsAmount").text = amountOfDialogs.ToString();

                var typeLabel = entry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "NPC";

                entry.Q<Button>("Edit").clicked += () => OnEditNPC(npc);
                entry.Q<Button>("Delete").clicked += () => OnDeleteNPC(npc, entry);

                npcsList.hierarchy.Add(entry);
            }
        }

        private void OnEditNPC(FriendlyNpc npc)
        {
            editingNpc = npc;
            OpenMenu(createNPCMenuPrefab);
        }

        private void OnDeleteNPC(FriendlyNpc npc, VisualElement entry)
        {
            var confirmPopup = Instantiate(prefabProvider.confirmPopup);
            var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
            dialogController.Show(
                title: "Delete NPC",
                question: $"Are you sure you want to delete the NPC '{npc.data.name}'? This cannot be undone.",
                onConfirm: () =>
                {
                    var spawners = FindObjectsByType<FriendlyNpcSpawnerObject>(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None
                    );
                    foreach (var spawner in spawners)
                    {
                        if (spawner.data != null && spawner.data.NpcId == npc.data.id)
                        {
                            spawner.data.NpcId = string.Empty;
                        }
                    }
                    creatablesManager.Delete<FriendlyNpc>(npc.data.id);
                    entry.RemoveFromHierarchy();
                },
                onCancel: () => { }
            );
        }

        private void AddNPC()
        {
            editingNpc = null;
            OpenMenu(createNPCMenuPrefab);
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);
            if (editingNpc != null)
                menuInstance.GetComponent<FriendlyNpcCreatorMenu>()?.SetupEditor(editingNpc);
            Destroy(gameObject);
        }
    }
}
