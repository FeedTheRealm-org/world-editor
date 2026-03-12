using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.UI.Common;
using FeedTheRealm.UI.Common.Components;
using FeedTheRealm.UI.EditorBar.PlacementOption;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar
{
    public class EditorBarController : MonoBehaviour
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private UIDocument menuBarUI;

        [Header("Menu Options")]
        [SerializeField]
        private MenuOption ZoneOption;

        [SerializeField]
        private MenuOption PlacementOption;

        [SerializeField]
        private MenuOption ElementOption;

        [Inject]
        private CategorySelectedEvent categorySelectedEvent;

        [Inject]
        private EnableInputEvent enableInputEvent;

        [Inject]
        private EnableEditorEvent enableEditorEvent;

        [Inject]
        private IObjectResolver resolver;

        private VisualElement root;
        private MenuStack menuStack;

        void Awake()
        {
            root = menuBarUI.rootVisualElement;
            menuStack = new MenuStack(root, enableEditorEvent, enableInputEvent, resolver);
            BindButton("Zone", ZoneOption);
            BindButton("Placement", PlacementOption);
            BindButton("Element", ElementOption);
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
            foreach (var subOption in option.MenuOptions)
            {
                if (subOption is ICategoryOption categoryOption)
                    categoryOption.SetCategoryEvent(categorySelectedEvent);
            }
            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                enableInputEvent.Raise(false);
            });
            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (menuStack == null || !menuStack.AnyOpen)
                    enableInputEvent.Raise(true);
            });
            button.clicked += () =>
            {
                if (option.MenuOptions.Count == 0)
                {
                    logger.Log(
                        $"MenuOption '{option.Label}' has no submenu options defined.",
                        this,
                        Logging.LogType.Warning
                    );
                    return;
                }
                menuStack.Toggle(button, option.MenuOptions);
            };
        }
    }
}
