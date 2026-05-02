using System;
using System.Threading.Tasks;
using API;
using FeedTheRealm.UI.Common;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar.TransactionsMenu
{
    public class TransactionMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private AuthService authService;

        [SerializeField]
        private PaymentService paymentService;

        [SerializeField]
        private Session.Session session;

        [Inject]
        private FeedTheRealm.Core.WorldObjects.Provider.WorldUIObjectProvider worldUIObjectProvider;
        private bool isAuthFlowActive;

        private VisualElement notLoggedInPanel;
        private VisualElement balancePanel;
        private VisualElement loadingPanel;

        private Label balanceValueLabel;

        private Button closeButton;
        private Button loginButton;
        private Button refreshButton;
        private Button withdrawButton;

        private Label withdrawFeedbackLabel;

        private async void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            BindElements(root);
            RegisterCallbacks();
            await LoadBalanceAsync();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements(VisualElement root)
        {
            notLoggedInPanel = root.Q<VisualElement>("NotLoggedInPanel");
            balancePanel = root.Q<VisualElement>("BalancePanel");
            loadingPanel = root.Q<VisualElement>("LoadingPanel");

            balanceValueLabel = root.Q<Label>("BalanceValue");

            closeButton = root.Q<Button>("Close");
            loginButton = root.Q<Button>("LoginButton");
            refreshButton = root.Q<Button>("RefreshButton");
            withdrawButton = root.Q<Button>("WithdrawButton");
            withdrawFeedbackLabel = root.Q<Label>("WithdrawFeedbackLabel");
        }

        private void RegisterCallbacks()
        {
            closeButton.clicked += CloseMenu;
            loginButton.clicked += OnLoginClicked;
            refreshButton.clicked += OnRefreshClicked;

            if (withdrawButton != null)
                withdrawButton.clicked += OnWithdrawClicked;
        }

        private void UnregisterCallbacks()
        {
            closeButton.clicked -= CloseMenu;
            loginButton.clicked -= OnLoginClicked;
            refreshButton.clicked -= OnRefreshClicked;

            if (withdrawButton != null)
                withdrawButton.clicked -= OnWithdrawClicked;
        }

        private async Task LoadBalanceAsync()
        {
            var (isLogged, _) = await authService.IsLogged();
            if (!isLogged)
            {
                logger.Log("[TransactionMenu] User not logged in.", this, Logging.LogType.Info);
                session.ClearSession();
                ShowPanel(notLoggedInPanel);
                return;
            }

            ShowPanel(loadingPanel);

            try
            {
                var (data, error, statusCode) = await paymentService.GetCreatorBalance(
                    session.APIToken
                );

                logger.Log(
                    $"[TransactionMenu] GetCreatorBalance response: data={data.balance}, error={error}, statusCode={statusCode}",
                    this,
                    Logging.LogType.Info
                );

                if (!string.IsNullOrEmpty(error))
                {
                    logger.Log(
                        $"[TransactionMenu] Error fetching balance: {error} (status {statusCode})",
                        this,
                        Logging.LogType.Error
                    );
                    ToastNotification.Show("Could not load balance.", "error", Color.red);
                    ShowPanel(notLoggedInPanel);
                    return;
                }

                RefreshBalanceUI(data);
                ShowPanel(balancePanel);
            }
            catch (Exception ex)
            {
                logger.Log($"[TransactionMenu] {ex.Message}", this, Logging.LogType.Error);
                ToastNotification.Show("Could not load balance.", "error", Color.red);
                ShowPanel(notLoggedInPanel);
            }
        }

        private void RefreshBalanceUI(CreatorBalanceResponse data)
        {
            if (data == null)
                return;

            balanceValueLabel.text = $"${data.balance:F2}";
        }

        private async void OnRefreshClicked()
        {
            refreshButton.SetEnabled(false);
            await LoadBalanceAsync();
            refreshButton.SetEnabled(true);
        }

        private void OnWithdrawClicked()
        {
            if (withdrawFeedbackLabel != null)
            {
                if (string.IsNullOrEmpty(withdrawFeedbackLabel.text))
                {
                    withdrawFeedbackLabel.text =
                        "Contact the Feed The Realm team to withdraw your rewards.";
                }
                else
                {
                    withdrawFeedbackLabel.text = string.Empty;
                }
            }
        }

        private void OnLoginClicked()
        {
            if (isAuthFlowActive || IsAuthMenuOpen())
                return;

            isAuthFlowActive = true;
            try
            {
                var loginObj = resolver.Instantiate(worldUIObjectProvider.loginMenuObject);
                var signUpObj = resolver.Instantiate(worldUIObjectProvider.signUpMenuObject);
                var verifyCodeObj = resolver.Instantiate(
                    worldUIObjectProvider.verifyCodeMenuObject
                );

                loginObj.name = "LoginMenu";
                signUpObj.name = "SignUpMenu";
                verifyCodeObj.name = "VerifyCodeMenu";

                var authFlow = new AuthFlowManager(loginObj, signUpObj, verifyCodeObj);
                authFlow.OnAuthComplete += async () =>
                {
                    authFlow.Destroy();
                    isAuthFlowActive = false;
                    await LoadBalanceAsync();
                };
                authFlow.Initialize();
            }
            catch
            {
                isAuthFlowActive = false;
                throw;
            }
        }

        private static bool IsAuthMenuOpen() =>
            GameObject.Find("LoginMenu") != null
            || GameObject.Find("SignUpMenu") != null
            || GameObject.Find("VerifyCodeMenu") != null;

        private void ShowPanel(VisualElement panel)
        {
            notLoggedInPanel.style.display = DisplayStyle.None;
            balancePanel.style.display = DisplayStyle.None;
            loadingPanel.style.display = DisplayStyle.None;
            panel.style.display = DisplayStyle.Flex;
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            resolver.Instantiate(menuPrefab);
            Destroy(gameObject);
        }
    }
}
