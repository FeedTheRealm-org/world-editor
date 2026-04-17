using System;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.UI.Common;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar.SubscriptionMenu
{
    public class SubscriptionMenuController : MenuController
    {
        private SubscriptionsCallbackServer callbackServer;

        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private AuthService authService;

        [SerializeField]
        private SubscriptionService subscriptionService;

        [SerializeField]
        private Session.Session session;

        [SerializeField]
        private VisualTreeAsset worldItemTemplate;

        [Inject]
        private WorldUIObjectProvider worldUIObjectProvider;

        // ── Panels ────────────────────────────────────────────────────────────
        private VisualElement notLoggedInPanel;
        private VisualElement createSubscriptionPanel;
        private VisualElement subscriptionPanel;
        private VisualElement loadingPanel;

        // ── Elements ──────────────────────────────────────────────────────────
        private Button closeButton;
        private Button loginButton;
        private Button createSubscriptionButton;

        private Label billingDateValue;
        private Label amountDueValue;
        private Label activeZonesValue;
        private Label freeZonesValue;

        private Label createSubscriptionFeedbackLabel;

        private Button decreaseSlotsButton;
        private Button increaseSlotsButton;
        private Button updateSlotsButton;
        private Label slotCountLabel;
        private Label slotFeedbackLabel;

        private ListView worldsList;

        // ── State ─────────────────────────────────────────────────────────────
        private SubscriptionResponse currentSubscription;
        private int pendingSlotCount;
        private bool isAuthFlowActive;

        private const int MinSlots = 1;
        private const int MaxSlots = 50;

        // ─────────────────────────────────────────────────────────────────────

        private async void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            BindElements(root);
            RegisterCallbacks();
            await LoadSubscriptionAsync();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        // ── Setup ─────────────────────────────────────────────────────────────

        private void BindElements(VisualElement root)
        {
            notLoggedInPanel = root.Q<VisualElement>("NotLoggedInPanel");
            createSubscriptionPanel = root.Q<VisualElement>("CreateSubscriptionPanel");
            subscriptionPanel = root.Q<VisualElement>("SubscriptionPanel");
            loadingPanel = root.Q<VisualElement>("LoadingPanel");

            closeButton = root.Q<Button>("Close");
            loginButton = root.Q<Button>("LoginButton");
            createSubscriptionButton = root.Q<Button>("CreateSubscriptionButton");

            billingDateValue = root.Q<Label>("BillingDateValue");
            amountDueValue = root.Q<Label>("AmountDueValue");
            activeZonesValue = root.Q<Label>("ActiveZonesValue");
            freeZonesValue = root.Q<Label>("FreeZonesValue");

            createSubscriptionFeedbackLabel = root.Q<Label>("CreateSubscriptionFeedbackLabel");

            decreaseSlotsButton = root.Q<Button>("DecreaseSlots");
            increaseSlotsButton = root.Q<Button>("IncreaseSlots");
            updateSlotsButton = root.Q<Button>("UpdateSlots");
            slotCountLabel = root.Q<Label>("SlotCountLabel");
            slotFeedbackLabel = root.Q<Label>("SlotFeedbackLabel");

            worldsList = root.Q<ListView>("WorldsList");
        }

        private void RegisterCallbacks()
        {
            closeButton.clicked += CloseMenu;
            loginButton.clicked += OnLoginClicked;
            createSubscriptionButton.clicked += OnCreateSubscriptionClicked;
            decreaseSlotsButton.clicked += OnDecreaseSlots;
            increaseSlotsButton.clicked += OnIncreaseSlots;
            updateSlotsButton.clicked += OnUpdateSlotsClicked;
        }

        private void UnregisterCallbacks()
        {
            closeButton.clicked -= CloseMenu;
            loginButton.clicked -= OnLoginClicked;
            createSubscriptionButton.clicked -= OnCreateSubscriptionClicked;
            decreaseSlotsButton.clicked -= OnDecreaseSlots;
            increaseSlotsButton.clicked -= OnIncreaseSlots;
            updateSlotsButton.clicked -= OnUpdateSlotsClicked;
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private async Task LoadSubscriptionAsync()
        {
            var (isLogged, _) = await authService.IsLogged();
            if (!isLogged)
            {
                logger.Log("[SubscriptionMenu] User not logged in.", this, Logging.LogType.Info);
                session.ClearSession();
                ShowPanel(notLoggedInPanel);
                return;
            }

            ShowPanel(loadingPanel);

            try
            {
                var (data, error, statusCode) = await subscriptionService.GetSubscription();

                if (!string.IsNullOrEmpty(error))
                {
                    if (IsMissingSubscription(statusCode, error))
                    {
                        logger.Log(
                            "[SubscriptionMenu] No subscription found for user.",
                            this,
                            Logging.LogType.Info
                        );
                        RefreshCreateSubscriptionPanel();
                        ShowPanel(createSubscriptionPanel);
                        return;
                    }
                }

                if (data == null)
                {
                    logger.Log(
                        "[SubscriptionMenu] Subscription response was empty.",
                        this,
                        Logging.LogType.Info
                    );
                    RefreshCreateSubscriptionPanel();
                    ShowPanel(createSubscriptionPanel);
                    return;
                }

                currentSubscription = data;
                pendingSlotCount = data.slots;

                RefreshBillingSummary();
                RefreshSlotControls();
                RefreshWorldsList();

                ShowPanel(subscriptionPanel);
            }
            catch (Exception ex)
            {
                logger.Log($"[SubscriptionMenu] {ex.Message}", this, Logging.LogType.Error);
                ToastNotification.Show("Could not load subscription data.", "error", Color.red);
                ShowPanel(notLoggedInPanel);
            }
        }

        // ── Billing summary ───────────────────────────────────────────────────

        private void RefreshBillingSummary()
        {
            if (
                DateTime.TryParse(currentSubscription.next_billing_date, out DateTime parsedDate)
                && parsedDate.Year > 1
            )
            {
                billingDateValue.text = parsedDate.ToLocalTime().ToString("MMM dd, yyyy");
            }
            else
            {
                billingDateValue.text = "—";
            }

            amountDueValue.text = FormatTotalPrice(
                currentSubscription.price_per_slot,
                currentSubscription.slots
            );
            activeZonesValue.text = currentSubscription.used_slots.ToString();
            freeZonesValue.text = (
                currentSubscription.slots - currentSubscription.used_slots
            ).ToString();
        }

        private void RefreshCreateSubscriptionPanel()
        {
            if (createSubscriptionFeedbackLabel != null)
                createSubscriptionFeedbackLabel.text = string.Empty;

            if (createSubscriptionButton != null)
                createSubscriptionButton.SetEnabled(true);
        }

        // ── Slot controls ─────────────────────────────────────────────────────

        private void RefreshSlotControls()
        {
            slotCountLabel.text = pendingSlotCount.ToString();
            decreaseSlotsButton.SetEnabled(pendingSlotCount > MinSlots);
            increaseSlotsButton.SetEnabled(pendingSlotCount < MaxSlots);
            updateSlotsButton.SetEnabled(pendingSlotCount != currentSubscription.slots);
            slotFeedbackLabel.text = string.Empty;
        }

        private void OnDecreaseSlots()
        {
            if (pendingSlotCount <= MinSlots)
                return;
            pendingSlotCount--;
            RefreshSlotControls();
        }

        private void OnIncreaseSlots()
        {
            if (pendingSlotCount >= MaxSlots)
                return;
            pendingSlotCount++;
            RefreshSlotControls();
        }

        private async void OnUpdateSlotsClicked()
        {
            updateSlotsButton.SetEnabled(false);
            slotFeedbackLabel.text = "Updating...";

            try
            {
                var (data, error, statusCode) = await subscriptionService.UpdateSlots(
                    pendingSlotCount
                );

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                currentSubscription = data;
                pendingSlotCount = data.slots;

                RefreshBillingSummary();
                RefreshSlotControls();

                slotFeedbackLabel.text = "Updated successfully.";
                ToastNotification.Show("Slots updated!", "success", Color.green);
                logger.Log(
                    $"[SubscriptionMenu] Slots updated to {pendingSlotCount}.",
                    this,
                    Logging.LogType.Info
                );
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[SubscriptionMenu] Update slots error: {ex.Message}",
                    this,
                    Logging.LogType.Error
                );
                slotFeedbackLabel.text = "Update failed. Try again.";
                ToastNotification.Show($"Failed to update slots: {ex.Message}", "error", Color.red);
                updateSlotsButton.SetEnabled(true);
            }
        }

        private void InitCallbackServer()
        {
            callbackServer = gameObject.GetComponent<SubscriptionsCallbackServer>();
            if (callbackServer == null)
            {
                callbackServer = gameObject.AddComponent<SubscriptionsCallbackServer>();
            }
        }

        private async void OnCreateSubscriptionClicked()
        {
            createSubscriptionButton.SetEnabled(false);
            createSubscriptionFeedbackLabel.text = "Opening checkout...";

            InitCallbackServer();

            try
            {
                var (checkoutUrl, error, statusCode) =
                    await subscriptionService.CreateCheckoutSession(
                        MinSlots,
                        callbackServer.SuccessUrl,
                        callbackServer.CancelUrl
                    );

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                if (string.IsNullOrEmpty(checkoutUrl))
                    throw new Exception("Checkout URL was empty.");

                callbackServer.OnSuccessEvent += OnSubscriptionSuccess;
                callbackServer.OnCancelledEvent += OnSubscriptionCancelled;

                _ = callbackServer
                    .StartServer(
                        new SubscriptionData
                        {
                            Slots = MinSlots,
                            PricePerSlot = "N/A", // This should be available or fetched
                            TotalPrice = "N/A",
                        }
                    )
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                            logger.Log(
                                $"Subscription server error: {task.Exception?.Message}",
                                this,
                                Logging.LogType.Error
                            );
                    });

                Application.OpenURL(checkoutUrl);
                createSubscriptionFeedbackLabel.text = "Checkout opened in your browser.";
                ToastNotification.Show("Subscription checkout opened.", "success", Color.green);
                logger.Log(
                    "[SubscriptionMenu] Subscription checkout opened.",
                    this,
                    Logging.LogType.Info
                );
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[SubscriptionMenu] Create subscription error: {ex.Message}",
                    this,
                    Logging.LogType.Error
                );
                createSubscriptionFeedbackLabel.text = "Could not start checkout. Try again.";
                ToastNotification.Show(
                    $"Failed to start checkout: {ex.Message}",
                    "error",
                    Color.red
                );
                createSubscriptionButton.SetEnabled(true);
            }
        }

        private void OnSubscriptionSuccess()
        {
            UnsubscribeCallbackServer();
            ToastNotification.Show("Subscription successful!", "success", Color.green);
            _ = LoadSubscriptionAsync();
        }

        private void OnSubscriptionCancelled()
        {
            UnsubscribeCallbackServer();
            ToastNotification.Show("Subscription cancelled.", "error", Color.red);
            createSubscriptionButton.SetEnabled(true);
            createSubscriptionFeedbackLabel.text = string.Empty;
        }

        private void UnsubscribeCallbackServer()
        {
            if (callbackServer != null)
            {
                callbackServer.OnSuccessEvent -= OnSubscriptionSuccess;
                callbackServer.OnCancelledEvent -= OnSubscriptionCancelled;
            }
        }

        // ── Worlds list ───────────────────────────────────────────────────────

        private void RefreshWorldsList()
        {
            worldsList.Clear();

            if (
                currentSubscription.active_zones == null
                || currentSubscription.active_zones.Length == 0
            )
            {
                var emptyLabel = new Label("No active zones yet.");
                emptyLabel.AddToClassList("header-label");
                worldsList.hierarchy.Add(emptyLabel);
                return;
            }

            foreach (var zone in currentSubscription.active_zones)
            {
                VisualElement entry = worldItemTemplate.Instantiate();

                entry.Q<Label>("Header").text = zone.world_name;
                entry.Q<Label>("ZoneName").text = zone.zone_name;
                entry.Q<Label>("SlotBadge").text = $"Slot #{zone.slot_index}";
                entry.Q<Label>("StatusBadge").text = zone.is_free ? "Free" : "Paid";

                // Capture to avoid closure bug in loop.
                var capturedZone = zone;
                var capturedEntry = entry;
                entry.Q<Button>("Unsubscribe").clicked += () =>
                    OnUnsubscribeClicked(capturedZone, capturedEntry);

                worldsList.hierarchy.Add(entry);
            }
        }

        private async void OnUnsubscribeClicked(ActiveZone zone, VisualElement entry)
        {
            var button = entry.Q<Button>("Unsubscribe");
            button.SetEnabled(false);

            try
            {
                var (error, statusCode) = await subscriptionService.UnsubscribeZone(zone.zone_id);

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                entry.RemoveFromHierarchy();

                // Decrement local count so the summary stays in sync without a full reload.
                currentSubscription.used_slots--;
                RefreshBillingSummary();

                ToastNotification.Show("Zone unsubscribed.", "info", Color.aliceBlue);
                logger.Log(
                    $"[SubscriptionMenu] Zone {zone.zone_id} unsubscribed.",
                    this,
                    Logging.LogType.Info
                );
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[SubscriptionMenu] Unsubscribe error: {ex.Message}",
                    this,
                    Logging.LogType.Error
                );
                ToastNotification.Show(
                    $"Failed to unsubscribe zone: {ex.Message}",
                    "error",
                    Color.red
                );
                button.SetEnabled(true);
            }
        }

        // ── Auth ──────────────────────────────────────────────────────────────

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
                    await LoadSubscriptionAsync();
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

        private static bool IsMissingSubscription(long statusCode, string error) =>
            statusCode == 404
            || (
                !string.IsNullOrEmpty(error)
                && error.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0
            )
            || (
                !string.IsNullOrEmpty(error)
                && error.IndexOf("no subscription", StringComparison.OrdinalIgnoreCase) >= 0
            );

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ShowPanel(VisualElement panel)
        {
            notLoggedInPanel.style.display = DisplayStyle.None;
            createSubscriptionPanel.style.display = DisplayStyle.None;
            subscriptionPanel.style.display = DisplayStyle.None;
            loadingPanel.style.display = DisplayStyle.None;
            panel.style.display = DisplayStyle.Flex;
        }

        private static string FormatTotalPrice(string pricePerSlot, int totalSlots)
        {
            if (
                decimal.TryParse(
                    pricePerSlot,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var price
                )
            )
                return $"${price * totalSlots:F2}";
            return "—";
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            resolver.Instantiate(menuPrefab);
            Destroy(gameObject);
        }
    }
}
