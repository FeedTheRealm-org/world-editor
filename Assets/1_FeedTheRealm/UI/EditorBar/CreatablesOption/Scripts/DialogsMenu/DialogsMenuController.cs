using System.Linq;
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
        private GameObject messagesMenuPrefab;

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
            var dialogsList = root.Q<ListView>("DialogsList");
            dialogsList.Clear();

            foreach (Dialog dialog in creatablesManager.GetAll<Dialog>())
            {
                VisualElement dialogEntry = itemListTemplate.Instantiate();
                dialogEntry.Q<Label>("Header").text = dialog.data.name;

                var typeLabel = dialogEntry.Q<Label>("Type");
                if (typeLabel != null)
                    typeLabel.text = "Dialog";

                dialogEntry.Q<Button>("Edit").clicked += () => OnEditDialog(dialog);
                dialogEntry.Q<Button>("EditMessages").clicked += () => OnEditMessages(dialog);
                dialogEntry.Q<Button>("Delete").clicked += () =>
                    OnDeleteDialog(dialog, dialogEntry);

                dialogsList.hierarchy.Add(dialogEntry);
            }
        }

        private void AddDialog()
        {
            logger.Log("Opening Create Dialog Menu", this, Logging.LogType.Info);
            editingDialog = null;
            OpenMenu(createDialogMenuPrefab);
        }

        void OnEditDialog(Dialog dialog)
        {
            logger.Log("Editing dialog: " + dialog.data.name, this, Logging.LogType.Info);
            editingDialog = dialog;
            OpenMenu(createDialogMenuPrefab);
        }

        void OnEditMessages(Dialog dialog)
        {
            logger.Log(
                "Opening messages for dialog: " + dialog.data.name,
                this,
                Logging.LogType.Info
            );
            editingDialog = dialog;
            OpenMenu(messagesMenuPrefab);
        }

        void OnDeleteDialog(Dialog dialog, VisualElement dialogEntry)
        {
            var npcs = creatablesManager.GetAll<FriendlyNpc>();
            bool isInUse = false;
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
                    isInUse = true;
                    break;
                }
            }

            if (isInUse)
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
                    dialogEntry.RemoveFromHierarchy();
                },
                onCancel: () => { }
            );
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            var menuInstance = resolver.Instantiate(menuPrefab);

            if (editingDialog != null)
            {
                if (menuPrefab == createDialogMenuPrefab)
                    menuInstance.GetComponent<DialogCreatorMenu>().SetupEditor(editingDialog);

                if (menuPrefab == messagesMenuPrefab)
                    menuInstance.GetComponent<MessagesMenu>().SetDialog(editingDialog);
            }

            Destroy(gameObject);
        }
    }
}
