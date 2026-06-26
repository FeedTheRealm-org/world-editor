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
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            closeButton.clicked += CloseMenu;
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
        }
    }
}
