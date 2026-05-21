using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.DialogsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class DialogsMenu : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private GameObject createDialogMenuPrefab;

        [SerializeField]
        private VisualTreeAsset itemListTemplate;

        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private readonly WorldPrefabProvider prefabProvider;

        private Button closeButton;
        private Button addDialogButton;
        private Dialog editingDialog;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            addDialogButton = root.Q<Button>("AddDialog");

            addDialogButton.clicked += AddDialog;
            closeButton.clicked += CloseMenu;

            PopulateDialogsList();
        }

        void OnDisable()
        {
            addDialogButton.clicked -= AddDialog;
            closeButton.clicked -= CloseMenu;
        }

        private void PopulateDialogsList()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var dialogsList = root.Q<ScrollView>("DialogsList");
            dialogsList.Clear();

            foreach (Dialog dialog in creatablesManager.GetAll<Dialog>())
            {
                VisualElement entry = itemListTemplate.Instantiate();
                entry.Q<Label>("Header").text = dialog.data.name;
                entry.Q<Label>("Amount").text = dialog.data.messages.Count.ToString();

                var typeLabel = entry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Dialog";

                entry.Q<Button>("Edit").clicked += () => OpenEditor(dialog);
                entry.Q<Button>("Delete").clicked += () => OnDeleteDialog(dialog, entry);

                dialogsList.Add(entry);
            }
        }

        private void AddDialog()
        {
            editingDialog = null;
            OpenMenu(createDialogMenuPrefab);
        }

        private void OpenEditor(Dialog dialog)
        {
            logger.Log($"Opening editor for: {dialog.data.name}", this, Logging.LogType.Info);
            editingDialog = dialog;
            OpenMenu(createDialogMenuPrefab);
        }

        private void OnDeleteDialog(Dialog dialog, VisualElement entry)
        {
            var npcs = creatablesManager.GetAll<FriendlyNpc>();
            bool inUse = false;

            foreach (var npc in npcs)
            {
                if (
                    System.Linq.Enumerable.Any(
                        npc.data.dialogProgression,
                        p =>
                            p.dialogId == dialog.data.id
                            || p.onQuestAcceptedDialogId == dialog.data.id
                    )
                )
                {
                    inUse = true;
                    break;
                }
            }

            if (inUse)
            {
                ToastNotification.Show(
                    "Cannot delete dialog: It is currently used by an NPC.",
                    "error",
                    Color.red
                );
                return;
            }

            var confirmPopup = Instantiate(prefabProvider.confirmPopup);
            var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();

            dialogController.Show(
                title: "Delete Dialog",
                question: "Are you sure you want to delete this dialog? This cannot be undone.",
                onConfirm: () =>
                {
                    creatablesManager.Delete<Dialog>(dialog.data.id);
                    entry.RemoveFromHierarchy();
                },
                onCancel: () => { }
            );
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var instance = resolver.Instantiate(menuPrefab);
            if (editingDialog != null)
                instance.GetComponent<DialogCreatorMenu>().SetupEditor(editingDialog);
            Destroy(gameObject);
        }
    }
}
