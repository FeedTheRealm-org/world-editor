using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MainMenu.Services
{
    public class MainMenuUISetupService : SetupService
    {
        private readonly GameObject mainMenuGameObject;
        private readonly GameObject menuBarGameObject;
        private readonly IObjectResolver objectResolver;
        private readonly GameObject logingMenuObject;
        private readonly GameObject signUpMenuObject;
        private readonly GameObject verifyCodeMenuObject;

        public MainMenuUISetupService(
            MainMenuUIObjectProvider mainMenuUIObjectProvider,
            IObjectResolver objectResolver,
            WorldSetupEvent setupEvent
        )
            : base(setupEvent)
        {
            if (mainMenuUIObjectProvider == null)
            {
                Debug.LogError("mainMenuUIObjectProvider not set!");
                return;
            }
            mainMenuGameObject = mainMenuUIObjectProvider.mainMenuGameObject;
            menuBarGameObject = mainMenuUIObjectProvider.menuBarGameObject;
            logingMenuObject = mainMenuUIObjectProvider.logingMenuObject;
            signUpMenuObject = mainMenuUIObjectProvider.signUpMenuObject;
            verifyCodeMenuObject = mainMenuUIObjectProvider.verifyCodeMenuObject;
            this.objectResolver = objectResolver;
        }

        public override void Setup()
        {
            if (mainMenuGameObject == null)
                throw new System.Exception(
                    "MainMenu GameObject not set in mainMenuUIObjectProvider!"
                );
            objectResolver.Instantiate(mainMenuGameObject).name = "MainMenu";

            if (menuBarGameObject == null)
                throw new System.Exception(
                    "MenuBar GameObject not set in mainMenuUIObjectProvider!"
                );
            objectResolver.Instantiate(menuBarGameObject).name = "MenuBar";

            if (logingMenuObject == null)
                throw new System.Exception(
                    "LogingMenu GameObject not set in mainMenuUIObjectProvider!"
                );
            GameObject loginMenu = objectResolver.Instantiate(logingMenuObject);
            loginMenu.name = "LogingMenu";
            LoginController loginController = loginMenu.GetComponent<LoginController>();
            if (loginController != null)
                loginController.InitializeBackground(true);

            if (signUpMenuObject == null)
                throw new System.Exception(
                    "SignUpMenu GameObject not set in mainMenuUIObjectProvider!"
                );

            if (verifyCodeMenuObject == null)
                throw new System.Exception(
                    "VerifyCodeMenu GameObject not set in mainMenuUIObjectProvider!"
                );
        }
    }
}
