using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Core.WorldSetup;
using FeedTheRealm.Gameplay.WorldSetup;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.MainMenuSetup.Services
{
    public class MainMenuUISetupService : ISetup
    {
        private readonly GameObject mainMenuGameObject;
        private readonly IObjectResolver objectResolver;
        private readonly AuthFlowManager authFlowManager;
        private readonly UpdateLoginEvent updateLoginEvent;

        public MainMenuUISetupService(
            MainMenuUIObjectProvider mainMenuUIObjectProvider,
            UpdateLoginEvent updateLoginEvent,
            AuthFlowManager authFlowManager,
            IObjectResolver objectResolver
        )
        {
            if (mainMenuUIObjectProvider == null)
            {
                Debug.LogError("mainMenuUIObjectProvider not set!");
                return;
            }
            mainMenuGameObject = mainMenuUIObjectProvider.mainMenuGameObject;
            this.objectResolver = objectResolver;
            this.updateLoginEvent = updateLoginEvent;
            this.authFlowManager = authFlowManager;
        }

        public void Setup()
        {
            if (mainMenuGameObject == null)
                throw new System.Exception(
                    "MainMenu GameObject not set in mainMenuUIObjectProvider!"
                );
            objectResolver.Instantiate(mainMenuGameObject).name = "MainMenu";

            authFlowManager.OnAuthComplete += (message) =>
            {
                ToastNotification.Show(message, "success", Color.green);
                updateLoginEvent.Raise();
            };
            authFlowManager.OnPasswordResetComplete += (message) =>
            {
                ToastNotification.Show(message, "success", Color.green);
            };
        }
    }
}
