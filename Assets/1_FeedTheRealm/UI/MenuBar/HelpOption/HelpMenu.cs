using System;
using System.IO;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTR.UI;
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
    public class HelpMenu : MenuController
    {
        [SerializeField]
        private Session.Session session;

        [Inject]
        private AuthFlowManager authFlowManager;

        [Inject]
        private UpdateLoginEvent updateLoginEvent;

        private Button loginButton;
        private Button signOutButton;
        private Button closeButton;
        private VisualElement loggedInContent;
        private VisualElement notLoggedInContent;
        private Label usernameLabel;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            loginButton = root.Q<Button>("Login");
            signOutButton = root.Q<Button>("SignOut");
            closeButton = root.Q<Button>("Close");
            notLoggedInContent = root.Q<VisualElement>("NotLoggedInContent");
            loggedInContent = root.Q<VisualElement>("LoggedInContent");
            usernameLabel = root.Q<Label>("Username");

            loginButton.clicked += authFlowManager.ShowAuthMenu;
            authFlowManager.OnAuthCancelled += CloseMenu;
            signOutButton.clicked += OnSignOutClicked;
            closeButton.clicked += CloseMenu;
            updateLoginEvent.OnRaised += RefreshSessionUI;

            RefreshSessionUI();
        }

        void OnDisable()
        {
            loginButton.clicked -= authFlowManager.ShowAuthMenu;
            signOutButton.clicked -= OnSignOutClicked;
            closeButton.clicked -= CloseMenu;
            updateLoginEvent.OnRaised -= RefreshSessionUI;
            authFlowManager.OnAuthCancelled -= CloseMenu;
        }

        private void RefreshSessionUI()
        {
            bool isLoggedIn = !string.IsNullOrEmpty(session.AccessToken);

            notLoggedInContent.style.display = isLoggedIn ? DisplayStyle.None : DisplayStyle.Flex;
            loggedInContent.style.display = isLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;

            if (isLoggedIn)
                usernameLabel.text = session.Email;
        }

        private void OnSignOutClicked()
        {
            session.ClearSession();
            updateLoginEvent.Raise();
            RefreshSessionUI();
        }
    }
}
