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

        [Header("UI References")]
        [SerializeField]
        private UIDocument menuBarUI;

        [SerializeField]
        private Sprite userIconSprite;

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

        private VisualElement root;
        private MenuStack menuStack;

        void Awake()
        {
            root = menuBarUI.rootVisualElement;
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

        private void UpdateLoginButton()
        {
            logger.Log("Updating login button UI.", this);
            Button loginButton = root.Q<Button>("Login");
            if (loginButton == null)
                return;
            if (!string.IsNullOrEmpty(session?.Email))
            {
                loginButton.text = session.Email[0].ToString().ToUpper();
                loginButton.style.backgroundImage = null;
                loginButton.style.width = 32;
                loginButton.style.height = 32;
                loginButton.style.backgroundColor = new StyleColor(Color.black);
                loginButton.style.borderTopLeftRadius = 16;
                loginButton.style.borderTopRightRadius = 16;
                loginButton.style.borderBottomLeftRadius = 16;
                loginButton.style.borderBottomRightRadius = 16;
                loginButton.style.unityTextAlign = TextAnchor.MiddleCenter;
                loginButton.style.color = new StyleColor(Color.white);
                loginButton.style.borderTopWidth = 0;
                loginButton.style.borderBottomWidth = 0;
                loginButton.style.borderLeftWidth = 0;
                loginButton.style.borderRightWidth = 0;
            }
            else
            {
                loginButton.text = "";
                loginButton.style.width = 32;
                loginButton.style.height = 32;
                loginButton.style.backgroundImage = new StyleBackground(userIconSprite);
                loginButton.style.backgroundColor = StyleKeyword.None;
                loginButton.style.borderTopWidth = 0;
                loginButton.style.borderBottomWidth = 0;
                loginButton.style.borderLeftWidth = 0;
                loginButton.style.borderRightWidth = 0;
            }
        }
    }
}
