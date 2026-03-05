using System.Collections;
using System.IO;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.UI.Common;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar.FileOption.PublishMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class PublishMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        [SerializeField]
        private WorldPublisherController worldPublisherController;

        // -------- Ui related elements --------
        private Button publishButton;
        private Button closeButton;
        private Label nameInput;
        private TextField descriptionInput;
        private VisualElement root;
        private WorldData worldData;

        private void OnEnable()
        {
            worldData = dataPersistenceManager.CurrentWorldData;
            var uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            publishButton = root.Q<Button>("Publish");
            closeButton = root.Q<Button>("Close");
            nameInput = root.Q<Label>("WorldName");
            descriptionInput = root.Q<TextField>("DescriptionInput");

            nameInput.text = worldData.worldName;
            publishButton.clicked += OnPublishClicked;
            closeButton.clicked += CloseMenu;
        }

        private void OnDisable()
        {
            publishButton.clicked -= OnPublishClicked;
            closeButton.clicked -= CloseMenu;
        }

        private async void OnPublishClicked()
        {
            logger.Log(
                $"PublishMenuController: Publishing world. Current local id='{worldData?.id}', name='{worldData?.worldName}'",
                this,
                Logging.LogType.Info
            );
            await PublishWorld();
        }

        private async Task PublishWorld()
        {
            worldData.worldName = dataPersistenceManager.CurrentWorldData.worldName;
            dataPersistenceManager.SaveWorld(worldData.worldName);
            string fileName = dataPersistenceManager.GetWorldFile(worldData.worldName);

            (string worldId, string error, long statusCode) =
                await worldPublisherController.PublishWorld(
                    worldData,
                    fileName,
                    descriptionInput.value
                );

            if (!string.IsNullOrEmpty(error))
            {
                logger.Log(
                    $"PublishMenuController: Error publishing world (status {statusCode}): {error}",
                    this,
                    Logging.LogType.Warning
                );
                string message = error;
                Color color = Color.red;
                if (statusCode == 401)
                    message = "Session expired. Please log in again.";
                else if (statusCode >= 500 || statusCode == 0)
                    message = "Unable to connect to server. Please try again later.";
                ToastNotification.Show(message, "error", color);
                return;
            }
            worldData.id = worldId;
            dataPersistenceManager.SaveWorld(worldData.worldName);
            ToastNotification.Show("World published successfully", "success", Color.green);
            CloseMenu();
        }
    }
}
