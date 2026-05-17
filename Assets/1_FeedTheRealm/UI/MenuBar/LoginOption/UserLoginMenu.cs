using System;
using System.IO;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar.FileOption.LoginOption
{
    [RequireComponent(typeof(UIDocument))]
    public class UserLoginModelMenu : MenuController
    {
        [SerializeField]
        private Session.Session session;

        [Inject]
        private WorldUIObjectProvider worldUIObjectProvider;

        [Inject]
        private UpdateLoginEvent updateLoginEvent;

        private Button loginButton;
        private Button signOutButton;
        private Button closeButton;
        private Label notLoggedInLabel;
        private VisualElement loggedInContent;
        private Label usernameLabel;
        private bool isAuthFlowActive;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            loginButton = root.Q<Button>("Login");
            signOutButton = root.Q<Button>("SignOut");
            closeButton = root.Q<Button>("Close");
            notLoggedInLabel = root.Q<Label>("NotLoggedIn");
            loggedInContent = root.Q<VisualElement>("LoggedInContent");
            usernameLabel = root.Q<Label>("Username");

            loginButton.clicked += OnLoginClicked;
            signOutButton.clicked += OnSignOutClicked;
            closeButton.clicked += CloseMenu;

            RefreshSessionUI();
        }

        void OnDisable()
        {
            loginButton.clicked -= OnLoginClicked;
            signOutButton.clicked -= OnSignOutClicked;
            closeButton.clicked -= CloseMenu;
        }

        private void RefreshSessionUI()
        {
            bool isLoggedIn = !string.IsNullOrEmpty(session.AccessToken);

            notLoggedInLabel.style.display = isLoggedIn ? DisplayStyle.None : DisplayStyle.Flex;
            loggedInContent.style.display = isLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;
            loginButton.style.display = isLoggedIn ? DisplayStyle.None : DisplayStyle.Flex;
            signOutButton.style.display = isLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;

            if (isLoggedIn)
                usernameLabel.text = session.Email;
        }

        private void OnSignOutClicked()
        {
            session.ClearSession();
            updateLoginEvent.Raise();
            RefreshSessionUI();
        }

        private async void OnLoginClicked()
        {
            if (isAuthFlowActive || IsAuthMenuOpen())
                return;

            var loginMenuObject = worldUIObjectProvider.loginMenuObject;
            var signUpMenuObject = worldUIObjectProvider.signUpMenuObject;
            var verifyCodeMenuObject = worldUIObjectProvider.verifyCodeMenuObject;
            isAuthFlowActive = true;
            try
            {
                GameObject loginMenu = resolver.Instantiate(loginMenuObject);
                loginMenu.name = "LoginMenu";
                var signUpObj = resolver.Instantiate(signUpMenuObject);
                signUpObj.name = "SignUpMenu";
                var verifyCodeObj = resolver.Instantiate(verifyCodeMenuObject);
                verifyCodeObj.name = "VerifyCodeMenu";
                var authFlowManager = new AuthFlowManager(loginMenu, signUpObj, verifyCodeObj);
                authFlowManager.OnAuthComplete += () =>
                {
                    authFlowManager.Destroy();
                    isAuthFlowActive = false;
                    RefreshSessionUI();
                    updateLoginEvent.Raise();
                };
                authFlowManager.Initialize();
            }
            catch
            {
                isAuthFlowActive = false;
                throw;
            }
        }

        private static bool IsAuthMenuOpen()
        {
            return GameObject.Find("LoginMenu") != null
                || GameObject.Find("SignUpMenu") != null
                || GameObject.Find("VerifyCodeMenu") != null;
        }
    }
}
