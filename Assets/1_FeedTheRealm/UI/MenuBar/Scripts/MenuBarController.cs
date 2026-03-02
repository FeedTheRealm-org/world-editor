using System;
using System.Collections.Generic;
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
            menuStack = new MenuStack(root, enableEditorEvent);
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
            button.clicked += () =>
            {
                if (option.MenuOptions.Count == 0)
                    return;
                menuStack.Toggle(button, option.MenuOptions);
            };
        }
    }
}
