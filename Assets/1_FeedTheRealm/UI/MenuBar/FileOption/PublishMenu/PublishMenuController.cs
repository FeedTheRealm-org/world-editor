using System.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar.FileOption.PublishMenu
{
    public class PublishMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        [SerializeField]
        private WorldPublisherController worldPublisherController;

        [SerializeField]
        private WorldUIObjectProvider worldUIObjectProvider;

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
            var currentData = dataPersistenceManager.CurrentWorldData;
            currentData.worldName = dataPersistenceManager.CurrentWorldData.worldName;

            dataPersistenceManager.SaveWorld(currentData.worldName);
            string fileName = dataPersistenceManager.GetWorldFile(currentData.worldName);

            (string worldId, string error, long statusCode) =
                await worldPublisherController.PublishWorld(
                    currentData,
                    fileName,
                    descriptionInput.value
                );

            if (!string.IsNullOrEmpty(error) && error != "No assets to upload.")
            {
                logger.Log(
                    $"PublishMenuController: Error publishing world (status {statusCode}): {error}",
                    this,
                    Logging.LogType.Warning
                );
                string message = error;
                Color color = Color.red;
                if (statusCode == 401)
                {
                    var loginObj = Instantiate(worldUIObjectProvider.loginMenuObject);
                    var signUpObj = Instantiate(worldUIObjectProvider.signUpMenuObject);
                    var verifyCodeObj = Instantiate(worldUIObjectProvider.verifyCodeMenuObject);

                    var loginCtrl = loginObj.GetComponent<LoginController>();
                    if (loginCtrl != null)
                        loginCtrl.showBackground = false;

                    loginObj.name = "LoginMenu";
                    signUpObj.name = "SignUpMenu";
                    verifyCodeObj.name = "VerifyCodeMenu";

                    var authFlow = new AuthFlowManager(loginObj, signUpObj, verifyCodeObj);
                    authFlow.OnAuthComplete += () => authFlow.Destroy();
                    authFlow.Initialize();
                }
                else if (statusCode >= 500 || statusCode == 0)
                    message = "Unable to connect to server. Please try again later.";
                ToastNotification.Show(message, "error", color);
                return;
            }
            logger.Log(
                $"PublishMenuController: World published successfully with id='{worldId}'",
                this,
                Logging.LogType.Info
            );

            dataPersistenceManager.CurrentWorldData.id = worldId;
            dataPersistenceManager.SetWorldId(worldId);

            dataPersistenceManager.SaveWorld(dataPersistenceManager.CurrentWorldData.worldName);

            ToastNotification.Show("World published successfully", "success", Color.green);
            CloseMenu();
        }
    }
}
