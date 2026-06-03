using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Gameplay.Inputs;
using FTR.UI;
using FTR.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

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

        [Inject]
        private InputReader inputReader;

        [Inject]
        private UpdateLoginEvent updateLoginEvent;

        [Inject]
        private Logging.Logger logger;

        [Inject]
        private Session.Session session;

        [Inject]
        private EnableEditorEvent editorEvent;

        [Header("UI References")]
        [SerializeField]
        private UIDocument menuBarUI;

        [Header("Menu Options")]
        [SerializeField]
        private GameObject fileOptionController;

        [SerializeField]
        private GameObject editOptionController;

        [SerializeField]
        private GameObject subscriptionsOptionController;

        [SerializeField]
        private GameObject helpOptionController;

        [SerializeField]
        private GameObject aboutOptionController;

        [SerializeField]
        private GameObject loginOptionController;

        // UI Elements
        private VisualElement root;
        private Image defaultUserIcon;
        private MenuStack menuStack;
        private Button loginButton;

        void Awake()
        {
            root = menuBarUI.rootVisualElement;
            defaultUserIcon = root.Q<Image>("DefaultUserIcon");
            loginButton = root.Q<Button>("Login");

            editorEvent.OnRaised += ToggleLoginButton;

            menuStack = new MenuStack(root, enableEditorEvent, enableInputEvent, resolver);
            BindButton("File", fileOptionController);
            BindButton("Edit", editOptionController);
            BindButton("Subscriptions", subscriptionsOptionController);
            BindButton("Help", helpOptionController);
            BindButton("About", aboutOptionController);
            BindButton("Login", loginOptionController);
            logger.Log("MenuBarController initialized successfully.", this);
            UpdateLoginButton();

            updateLoginEvent.OnRaised += UpdateLoginButton;
        }

        private void OnDestroy()
        {
            if (updateLoginEvent != null)
                updateLoginEvent.OnRaised -= UpdateLoginButton;
        }

        private void BindButton(string buttonName, GameObject option)
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
                enableInputEvent.Raise(false);
                inputReader?.RaiseSecondaryInteraction();
                if (menuOption.MenuOptions.Count == 0)
                    menuOption.Execute();
                menuStack.Toggle(button, menuOption.MenuOptions);
            };
        }

        private void ToggleLoginButton(bool enabled)
        {
            loginButton.SetEnabled(enabled);
        }

        private void UpdateLoginButton()
        {
            bool isLoggedIn = !string.IsNullOrEmpty(session?.Email);

            // Toggle default icon visibility
            defaultUserIcon.style.display = isLoggedIn ? DisplayStyle.None : DisplayStyle.Flex;

            // Show first initial if logged in
            loginButton.text = isLoggedIn ? session.Email[0].ToString().ToUpper() : "";
        }
    }
}
