using System.Collections;
using System.IO;
using System.Threading.Tasks;
using API;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

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
    private TextField nameInput;
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
        nameInput = root.Q<TextField>("NameInput");
        descriptionInput = root.Q<TextField>("DescriptionInput");

        nameInput.value = worldData.worldName;
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
        if (string.IsNullOrEmpty(nameInput.value))
        {
            logger.Log(
                "PublishMenuController: No world name entered!",
                this,
                Logging.LogType.Error
            );
            ToastNotification.Show("World name is required", "error", Color.red);
            return;
        }
        await PublishWorld();
    }

    private async Task PublishWorld()
    {
        // Update the world name with the input value
        worldData.worldName = nameInput.value;

        // We save the world to ensure the latest changes are included
        logger.Log(
            $"PublishMenuController: Before SaveWorld, worldData.id='{worldData?.id}', worldData.worldName='{worldData?.worldName}'",
            this,
            Logging.LogType.Info
        );
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
                Logging.LogType.Error
            );
            string message = error;
            Color color = Color.red;
            if (statusCode == 401)
            {
                message = "Session expired. Please log in again.";
            }
            else if (statusCode >= 500 || statusCode == 0)
            {
                message = "Unable to connect to server. Please try again later.";
            }
            ToastNotification.Show(message, "error", color);
            return;
        }

        logger.Log(
            $"PublishMenuController: World published successfully with server id='{worldId}'",
            this,
            Logging.LogType.Info
        );
        worldData.id = worldId;
        logger.Log(
            $"PublishMenuController: After setting id, worldData.id='{worldData.id}', saving world again.",
            this,
            Logging.LogType.Info
        );
        dataPersistenceManager.SaveWorld(worldData.worldName);

        ToastNotification.Show("World published successfully", "success", Color.green);

        CloseMenu();
    }
}
