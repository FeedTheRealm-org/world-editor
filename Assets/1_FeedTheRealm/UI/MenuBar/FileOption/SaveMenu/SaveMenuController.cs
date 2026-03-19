using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.FileOption.SaveMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class SaveMenuController : MenuController
    {
        [SerializeField]
        private WorldSelector worldSelector;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        private Button saveButton;
        private Button closeButton;
        private TextField nameInput;
        private VisualElement root;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            saveButton = root.Q<Button>("Save");
            closeButton = root.Q<Button>("Close");
            nameInput = root.Q<TextField>("NameInput");

            string worldName = worldSelector.selectedWorld;
            if (worldName != null && !string.IsNullOrEmpty(worldName))
            {
                nameInput.value = worldName;
            }

            saveButton.clicked += OnSaveClicked;
            closeButton.clicked += CloseMenu;
        }

        private void OnDisable()
        {
            saveButton.clicked -= OnSaveClicked;
            closeButton.clicked -= CloseMenu;
        }

        private void OnSaveClicked()
        {
            string worldName = nameInput?.value?.Trim();
            if (string.IsNullOrEmpty(worldName))
            {
                Debug.LogWarning("SaveMenuController: No world name entered!");
                ToastNotification.Show("World name is required", "error", Color.red);
                return;
            }

            Debug.Log($"SaveMenuController: Saving world '{worldName}'");
            dataPersistenceManager.SaveWorld(worldName);

            ToastNotification.Show("World saved successfully", "success", Color.green);

            CloseMenu();
        }
    }
}
