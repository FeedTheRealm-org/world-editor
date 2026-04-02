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
        private readonly GameObject menuBarGameObject;
        private readonly IObjectResolver objectResolver;
        private readonly GameObject loginMenuObject;
        private readonly GameObject signUpMenuObject;
        private readonly GameObject verifyCodeMenuObject;
        private AuthFlowManager authFlowManager;

        public MainMenuUISetupService(
            MainMenuUIObjectProvider mainMenuUIObjectProvider,
            IObjectResolver objectResolver
        )
        {
            if (mainMenuUIObjectProvider == null)
            {
                Debug.LogError("mainMenuUIObjectProvider not set!");
                return;
            }
            mainMenuGameObject = mainMenuUIObjectProvider.mainMenuGameObject;
            menuBarGameObject = mainMenuUIObjectProvider.menuBarGameObject;
            loginMenuObject = mainMenuUIObjectProvider.loginMenuObject;
            signUpMenuObject = mainMenuUIObjectProvider.signUpMenuObject;
            verifyCodeMenuObject = mainMenuUIObjectProvider.verifyCodeMenuObject;
            this.objectResolver = objectResolver;
        }

        public void Setup()
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

            if (loginMenuObject == null)
                throw new System.Exception(
                    "LoginMenu GameObject not set in mainMenuUIObjectProvider!"
                );

            GameObject loginMenu = objectResolver.Instantiate(loginMenuObject);
            var loginObj = loginMenu;
            loginObj.name = "LoginMenu";
            var signUpObj = objectResolver.Instantiate(signUpMenuObject);
            signUpObj.name = "SignUpMenu";
            var verifyCodeObj = objectResolver.Instantiate(verifyCodeMenuObject);
            verifyCodeObj.name = "VerifyCodeMenu";

            authFlowManager = new AuthFlowManager(loginObj, signUpObj, verifyCodeObj);
            authFlowManager.OnAuthComplete += () =>
            {
                authFlowManager.Destroy();
            };
            authFlowManager.Initialize();

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
