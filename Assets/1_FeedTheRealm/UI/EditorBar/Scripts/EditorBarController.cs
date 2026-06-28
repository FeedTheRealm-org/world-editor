using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.UI.EditorBar.PlacementOption;
using FTR.UI;
using FTR.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar
{
    public class EditorBarController : MonoBehaviour
    {
        [Inject]
        private Logging.Logger logger;

        [SerializeField]
        private UIDocument menuBarUI;

        [Header("Menu Options")]
        [SerializeField]
        private GameObject CreatablesOption;

        [Inject]
        private CategorySelectedEvent categorySelectedEvent;

        [Inject]
        private EnableInteractionsEvent enableInputEvent;

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
            BindButton("Creatables", CreatablesOption);
        }

        private void BindButton(string buttonName, GameObject option)
        {
            Button button = root.Q<Button>(buttonName);

            if (button == null)
            {
                logger.Log(
                    $"Button '{buttonName}' not found in EditorBar UI.",
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

            resolver.InjectGameObject(option);

            if (!option.TryGetComponent<MenuOption>(out var menuOption))
            {
                logger.Log(
                    $"MenuOption component not found on '{option.name}'.",
                    this,
                    Logging.LogType.Error
                );
                button.SetEnabled(false);
                return;
            }

            button.text = menuOption.Label;
            foreach (var subOption in menuOption.MenuOptions)
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
                if (menuOption.MenuOptions.Count == 0)
                {
                    logger.Log(
                        $"MenuOption '{menuOption.Label}' has no submenu options defined.",
                        this,
                        Logging.LogType.Warning
                    );
                    return;
                }
                menuStack.Toggle(button, menuOption.MenuOptions);
            };
        }
    }
}
