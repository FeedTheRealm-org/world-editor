using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
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
            var npcsList = root.Q<ListView>("NPCList");
            npcsList.Clear();

            foreach (FriendlyNpc npc in creatablesManager.GetAll<FriendlyNpc>())
            {
                var entry = itemListTemplate.Instantiate();
                entry.Q<Label>("Header").text = npc.data.name;

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
            logger.Log("Editing NPC: " + npc.data.name, this, Logging.LogType.Info);
            editingNpc = npc;
            OpenMenu(createNPCMenuPrefab);
        }

        private void OnDeleteNPC(FriendlyNpc npc, VisualElement entry)
        {
            logger.Log("Deleting NPC: " + npc.data.name, this, Logging.LogType.Info);
            creatablesManager.Delete<FriendlyNpc>(npc.data.id);
            entry.RemoveFromHierarchy();
        }

        private void AddNPC()
        {
            logger.Log("Opening Create NPC Menu", this, Logging.LogType.Info);
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
