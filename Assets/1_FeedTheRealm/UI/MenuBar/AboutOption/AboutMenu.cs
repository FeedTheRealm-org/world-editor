using System;
using System.IO;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTR.Core.Common.Config;
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
    public class AboutMenu : MenuController
    {
        private Button closeButton;
        private Button landingPageButton;

        [Inject]
        private readonly Config config;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            landingPageButton = root.Q<Button>("LandingPageButton");
            closeButton.clicked += CloseMenu;
            landingPageButton.clicked += OnLandingPageButtonClicked;
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            landingPageButton.clicked -= OnLandingPageButtonClicked;
        }

        private void OnLandingPageButtonClicked()
        {
            Application.OpenURL(config.LandingPageUrl);
        }
    }
}
