using System;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.DataPersistence;
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
        private WorldService worldService;

        [SerializeField]
        private ZoneService zoneService;

        [SerializeField]
        private VisualTreeAsset worldItemTemplate;

        [Inject]
        private WorldUIObjectProvider worldUIObjectProvider;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        // ── Panels ────────────────────────────────────────────────────────────
        private VisualElement notLoggedInPanel;
        private VisualElement createSubscriptionPanel;
        private VisualElement subscriptionPanel;
        private VisualElement loadingPanel;

        // ── Elements ──────────────────────────────────────────────────────────
        private Button closeButton;
        private Button loginButton;
        private Button createSubscriptionButton;
        private Button cancelSubscriptionButton;

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
            cancelSubscriptionButton = root.Q<Button>("CancelSubscriptionButton");

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

            if (cancelSubscriptionButton != null)
                cancelSubscriptionButton.clicked += OnCancelSubscriptionClicked;
        }

        private void UnregisterCallbacks()
        {
            closeButton.clicked -= CloseMenu;
            loginButton.clicked -= OnLoginClicked;
            createSubscriptionButton.clicked -= OnCreateSubscriptionClicked;
            decreaseSlotsButton.clicked -= OnDecreaseSlots;
            increaseSlotsButton.clicked -= OnIncreaseSlots;
            updateSlotsButton.clicked -= OnUpdateSlotsClicked;

            if (cancelSubscriptionButton != null)
                cancelSubscriptionButton.clicked -= OnCancelSubscriptionClicked;
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

                if (
                    data == null
                    || string.Equals(data.status, "canceled", StringComparison.OrdinalIgnoreCase)
                )
                {
                    logger.Log(
                        data == null
                            ? "[SubscriptionMenu] Subscription response was empty."
                            : "[SubscriptionMenu] Subscription is canceled.",
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
                _ = LoadWorldsListAsync();

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

            amountDueValue.text = FormatTotalPrice(currentSubscription.amount_due);
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
                int targetSlots = pendingSlotCount;

                var (_, error, statusCode) = await subscriptionService.UpdateSlots(targetSlots);

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                // Get the updated state from the server (this updates amount_due, etc.)
                await LoadSubscriptionAsync();

                // Since LoadSubscriptionAsync might receive old slot counts due to backend/webhook delays,
                // and it overwrites pendingSlotCount, we must manually force the local state to the known correct value.
                currentSubscription.slots = targetSlots;
                pendingSlotCount = targetSlots;

                RefreshBillingSummary();
                RefreshSlotControls();

                slotFeedbackLabel.text = "Updated successfully.";
                ToastNotification.Show("Slots updated!", "success", Color.green);
                logger.Log($"[SubscriptionMenu] Slots updated.", this, Logging.LogType.Info);
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[SubscriptionMenu] Update slots error: {ex.Message}",
                    this,
                    Logging.LogType.Error
                );

                // Reset pending slot count to the actual current state on failure
                pendingSlotCount = currentSubscription.slots;
                RefreshSlotControls();

                slotFeedbackLabel.text = "Update failed. Try again.";
                ToastNotification.Show($"Failed to update slots: {ex.Message}", "error", Color.red);
            }
        }

        private async void OnCancelSubscriptionClicked()
        {
            if (currentSubscription != null && currentSubscription.used_slots > 0)
            {
                ToastNotification.Show(
                    "Cannot cancel subscription. Please delete all your worlds first.",
                    "error",
                    Color.red
                );
                return;
            }

            if (cancelSubscriptionButton != null)
                cancelSubscriptionButton.SetEnabled(false);

            try
            {
                var (error, statusCode) = await subscriptionService.CancelSubscription();

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                ToastNotification.Show(
                    "Subscription cancelled successfully.",
                    "success",
                    Color.green
                );
                logger.Log(
                    "[SubscriptionMenu] Subscription cancelled.",
                    this,
                    Logging.LogType.Info
                );

                // Reload the subscription data to show the updated (e.g. absent or cancelled) state
                await LoadSubscriptionAsync();
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[SubscriptionMenu] Cancel subscription error: {ex.Message}",
                    this,
                    Logging.LogType.Error
                );
                ToastNotification.Show(
                    $"Failed to cancel subscription: {ex.Message}",
                    "error",
                    Color.red
                );
                if (cancelSubscriptionButton != null)
                    cancelSubscriptionButton.SetEnabled(true);
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

        private async Task LoadWorldsListAsync()
        {
            worldsList.Clear();
            worldsList.hierarchy.Clear();

            if (worldService == null)
            {
                worldService = FindObjectOfType<WorldService>();
                zoneService = FindObjectOfType<ZoneService>();
            }

            var loadingLabel = new Label("Loading worlds...");
            loadingLabel.AddToClassList("header-label");
            worldsList.hierarchy.Add(loadingLabel);

            var (amount, worlds, error) = await worldService.GetWorldPage(
                0,
                100,
                "",
                session.APIToken
            );

            worldsList.Clear();
            worldsList.hierarchy.Clear();

            if (!string.IsNullOrEmpty(error))
            {
                var errorLabel = new Label($"Error loading worlds: {error}");
                errorLabel.AddToClassList("header-label");
                worldsList.hierarchy.Add(errorLabel);
                return;
            }

            if (worlds == null || worlds.Count == 0)
            {
                var emptyLabel = new Label("No created worlds yet.");
                emptyLabel.AddToClassList("header-label");
                worldsList.hierarchy.Add(emptyLabel);
                return;
            }

            foreach (var world in worlds)
            {
                VisualElement entry = worldItemTemplate.Instantiate();

                entry.Q<Label>("Header").text = world.worldName;

                int zoneCount = 0;
                var (zones, zError, zStatusCode) = await zoneService.GetZonesList(
                    world.worldId,
                    session.APIToken
                );

                if (string.IsNullOrEmpty(zError) && zones != null)
                {
                    zoneCount = zones.Count;
                }

                entry.Q<Label>("ZoneName").text = $"{zoneCount} zone(s)";
                entry.Q<Label>("SlotBadge").style.display = DisplayStyle.None;
                entry.Q<Label>("StatusBadge").style.display = DisplayStyle.None;

                var capturedWorld = world;
                var capturedEntry = entry;
                var deleteButton = entry.Q<Button>("Unsubscribe");
                deleteButton.text = "Delete World";
                deleteButton.clicked += () => OnDeleteWorldClicked(capturedWorld, capturedEntry);

                worldsList.hierarchy.Add(entry);
            }
        }

        private async void OnDeleteWorldClicked(
            FTRShared.Runtime.Models.WorldData world,
            VisualElement entry
        )
        {
            var button = entry.Q<Button>("Unsubscribe");
            button.SetEnabled(false);

            try
            {
                var (error, statusCode) = await worldService.DeleteWorld(
                    world.worldId,
                    session.APIToken
                );

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                entry.RemoveFromHierarchy();

                ToastNotification.Show(
                    $"World '{world.worldName}' deleted.",
                    "info",
                    Color.aliceBlue
                );

                // Clear the local worldId so future publishes create a new world instead of updating a deleted one.
                if (dataPersistenceManager != null)
                {
                    var localWorldData = dataPersistenceManager.GetWorldData(world.worldName);
                    if (localWorldData != null && localWorldData.worldId == world.worldId)
                    {
                        localWorldData.worldId = "";
                        dataPersistenceManager.SaveWorldMetadata(localWorldData);
                    }
                }

                // Refresh subscription info because deleting a world frees up zones
                _ = LoadSubscriptionAsync();
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[SubscriptionMenu] Delete world error: {ex.Message}",
                    this,
                    Logging.LogType.Error
                );
                ToastNotification.Show($"Failed to delete world: {ex.Message}", "error", Color.red);
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

        private static string FormatTotalPrice(string amountDue)
        {
            if (
                decimal.TryParse(
                    amountDue,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var amount
                )
            )
                return $"${amount:F2}";
            return "—";
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            resolver.Instantiate(menuPrefab);
            Destroy(gameObject);
        }
    }
}
