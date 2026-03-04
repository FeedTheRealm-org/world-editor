using System;
using System.Collections.Generic;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar
{
    public class MenuBarController : MonoBehaviour
    {
        [Inject]
        private EnableEditorEvent enableEditorEvent;

        [Inject]
        private IObjectResolver resolver;

        [Inject]
        private EnableInputEvent enableInputEvent;

        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private UIDocument menuBarUI;

        [Header("Menu Options")]
        [SerializeField]
        private MenuOption fileOptionController;

        [SerializeField]
        private MenuOption editOptionController;

        [SerializeField]
        private MenuOption subscriptionsOptionController;

        [SerializeField]
        private MenuOption helpOptionController;

        [SerializeField]
        private MenuOption aboutOptionController;
        private VisualElement root;
        private MenuStack menuStack;

        void Awake()
        {
            root = menuBarUI.rootVisualElement;
            menuStack = new MenuStack(root, enableEditorEvent, resolver);
            BindButton("File", fileOptionController);
            BindButton("Edit", editOptionController);
            BindButton("Subscriptions", subscriptionsOptionController);
            BindButton("Help", helpOptionController);
            BindButton("About", aboutOptionController);
        }

        private void BindButton(string buttonName, MenuOption option)
        {
            Button button = root.Q<Button>(buttonName);

            if (button == null)
            {
                logger.Log(
                    $"Button '{buttonName}' not found in MenuBar UI.",
                    this,
                    Logging.LogType.Error
                );
                return;
            }

            if (option == null)
            {
                button.SetEnabled(false);
                return;
            }

            button.text = option.Label;
            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                enableInputEvent.Raise(false);
            });
            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                enableInputEvent.Raise(true);
            });
            button.clicked += () =>
            {
                if (option.MenuOptions.Count == 0)
                    return;
                menuStack.Toggle(button, option.MenuOptions);
            };
        }
    }
}
