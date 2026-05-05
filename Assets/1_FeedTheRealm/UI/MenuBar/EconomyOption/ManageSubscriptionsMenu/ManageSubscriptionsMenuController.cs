using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.UI.Common;
using FTRShared.UI.AuthMenu;
using FTRShared.UI.ZoneStatusBadge;
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

        [SerializeField]
        private ZoneStatusBadgeController zoneStatusBadge;

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

        private Button decreaseCreateSlotsButton;
        private Button increaseCreateSlotsButton;
        private Label createSlotCountLabel;

        private ScrollView worldsList;

        // ── State ─────────────────────────────────────────────────────────────
        private SubscriptionResponse currentSubscription;
        private int pendingSlotCount;
        private int pendingCreateSlotCount = MinSlots;
        private bool isAuthFlowActive;

        private const int MinSlots = 1;
        private const int MaxSlots = 50;

        // ── Lifecycle ─────────────────────────────────────────────────────────

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

            decreaseCreateSlotsButton = root.Q<Button>("DecreaseCreateSlots");
            increaseCreateSlotsButton = root.Q<Button>("IncreaseCreateSlots");
            createSlotCountLabel = root.Q<Label>("CreateSlotCountLabel");

            worldsList = root.Q<ScrollView>("WorldsList");
        }

        private void RegisterCallbacks()
        {
            closeButton.clicked += CloseMenu;
            loginButton.clicked += OnLoginClicked;
            createSubscriptionButton.clicked += OnCreateSubscriptionClicked;
            decreaseSlotsButton.clicked += OnDecreaseSlots;
            increaseSlotsButton.clicked += OnIncreaseSlots;
            updateSlotsButton.clicked += OnUpdateSlotsClicked;
            decreaseCreateSlotsButton.clicked += OnDecreaseCreateSlots;
            increaseCreateSlotsButton.clicked += OnIncreaseCreateSlots;

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
            decreaseCreateSlotsButton.clicked -= OnDecreaseCreateSlots;
            increaseCreateSlotsButton.clicked -= OnIncreaseCreateSlots;

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
                billingDateValue.text = parsedDate.ToLocalTime().ToString("MMM dd, yyyy");
            else
                billingDateValue.text = "—";

            amountDueValue.text = currentSubscription.amount_due.ToString("C");
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

            pendingCreateSlotCount = MinSlots;
            RefreshCreateSlotControls();
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

        private void RefreshCreateSlotControls()
        {
            createSlotCountLabel.text = pendingCreateSlotCount.ToString();
            decreaseCreateSlotsButton.SetEnabled(pendingCreateSlotCount > MinSlots);
            increaseCreateSlotsButton.SetEnabled(pendingCreateSlotCount < MaxSlots);
        }

        private void OnDecreaseCreateSlots()
        {
            if (pendingCreateSlotCount <= MinSlots)
                return;
            pendingCreateSlotCount--;
            RefreshCreateSlotControls();
        }

        private void OnIncreaseCreateSlots()
        {
            if (pendingCreateSlotCount >= MaxSlots)
                return;
            pendingCreateSlotCount++;
            RefreshCreateSlotControls();
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
                {
                    logger.Log(
                        $"[SubscriptionMenu] Update slots error: {error} (status {statusCode})",
                        this,
                        Logging.LogType.Error
                    );
                    throw new Exception($"{error}");
                }

                await LoadSubscriptionAsync();

                // Force local state to the known correct value since backend/webhook delays
                // may cause LoadSubscriptionAsync to return stale slot counts.
                currentSubscription.slots = targetSlots;
                pendingSlotCount = targetSlots;

                RefreshBillingSummary();
                RefreshSlotControls();

                slotFeedbackLabel.text = "Updated successfully.";
                ToastNotification.Show("Slots updated!", "success", Color.green);
                logger.Log("[SubscriptionMenu] Slots updated.", this, Logging.LogType.Info);
            }
            catch (Exception ex)
            {
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
                callbackServer = gameObject.AddComponent<SubscriptionsCallbackServer>();
        }

        private async void OnCreateSubscriptionClicked()
        {
            createSubscriptionButton.SetEnabled(false);
            createSubscriptionFeedbackLabel.text = "Opening checkout...";

            InitCallbackServer();

            try
            {
                var (pricingData, pricingError, pricingStatusCode) =
                    await subscriptionService.GetPricingInfo();

                if (!string.IsNullOrEmpty(pricingError))
                    throw new Exception(
                        $"Failed to get pricing info: {pricingError} (status {pricingStatusCode})"
                    );

                var (checkoutUrl, error, statusCode) =
                    await subscriptionService.CreateCheckoutSession(
                        pendingCreateSlotCount,
                        callbackServer.SuccessUrl,
                        callbackServer.CancelUrl
                    );

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                if (string.IsNullOrEmpty(checkoutUrl))
                    throw new Exception("Checkout URL was empty.");

                callbackServer.OnSuccessEvent += OnSubscriptionSuccess;
                callbackServer.OnCancelledEvent += OnSubscriptionCancelled;

                string pricePerSlotStr = pricingData?.price_per_slot ?? "0";
                string totalPriceStr = "0";

                if (
                    decimal.TryParse(
                        pricePerSlotStr,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal pricePerSlot
                    )
                )
                {
                    totalPriceStr = (pricePerSlot * pendingCreateSlotCount).ToString(
                        "F2",
                        System.Globalization.CultureInfo.InvariantCulture
                    );
                    pricePerSlotStr = pricePerSlot.ToString(
                        "F2",
                        System.Globalization.CultureInfo.InvariantCulture
                    );
                }

                _ = callbackServer
                    .StartServer(
                        new SubscriptionData
                        {
                            Slots = pendingCreateSlotCount,
                            PricePerSlot = pricePerSlotStr,
                            TotalPrice = totalPriceStr,
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

            var loadingLabel = new Label("Loading worlds...");
            loadingLabel.AddToClassList("header-label");
            worldsList.Add(loadingLabel);

            var (amount, worlds, error) = await worldService.GetWorldPage(
                0,
                100,
                "",
                session.APIToken
            );

            worldsList.Clear();

            if (!string.IsNullOrEmpty(error))
            {
                var errorLabel = new Label($"Error loading worlds: {error}");
                errorLabel.AddToClassList("header-label");
                worldsList.Add(errorLabel);
                return;
            }

            if (worlds == null || worlds.Count == 0)
            {
                var emptyLabel = new Label("No created worlds yet.");
                emptyLabel.AddToClassList("header-label");
                worldsList.Add(emptyLabel);
                return;
            }

            foreach (var world in worlds)
            {
                var zones = world.zones ?? new List<WorldZoneMetadata>();
                int zoneCount = zones.Count;
                int activeZoneCount = 0;
                foreach (var z in zones)
                    if (z.is_active)
                        activeZoneCount++;

                VisualElement worldWrapper = new VisualElement();

                // --- World header entry ---
                VisualElement worldEntry = worldItemTemplate.Instantiate();
                worldEntry.style.backgroundColor = new StyleColor(
                    new Color(0.1f, 0.1f, 0.1f, 0.5f)
                );

                worldEntry.Q<Label>("Header").text = world.name;
                worldEntry.Q<Label>("ZoneName").text =
                    $"{activeZoneCount}/{zoneCount} active zone(s)";
                worldEntry.Q<Label>("SlotBadge").style.display = DisplayStyle.None;

                // ── World status badge via shared utility ──────────────────────
                zoneStatusBadge.Apply(
                    worldEntry.Q<Label>("StatusBadge"),
                    zoneStatusBadge.Evaluate(zones)
                );

                // Dropdown
                var dropdownBtn = worldEntry.Q<Button>("DropdownBtn");
                bool isExpanded = false;
                VisualElement zoneContainer = new VisualElement();

                if (zoneCount > 0)
                {
                    dropdownBtn.style.display = DisplayStyle.Flex;
                    dropdownBtn.clicked += () =>
                    {
                        isExpanded = !isExpanded;
                        dropdownBtn.text = isExpanded ? "▼" : "▶";
                        zoneContainer.style.display = isExpanded
                            ? DisplayStyle.Flex
                            : DisplayStyle.None;
                    };
                }

                dropdownBtn.text = "▶";
                zoneContainer.style.display = DisplayStyle.None;

                var activateAllBtn = worldEntry.Q<Button>("ToggleActive");
                bool anyZoneActive = activeZoneCount > 0;
                activateAllBtn.text = anyZoneActive ? "Deactivate All" : "Activate All";
                activateAllBtn.style.backgroundColor = new StyleColor(
                    anyZoneActive
                        ? new Color(0.8f, 0.2f, 0.2f, 0.3f)
                        : new Color(0.2f, 0.6f, 0.2f, 0.3f)
                );

                var capturedWorld = world;
                var deleteWorldButton = worldEntry.Q<Button>("Unsubscribe");
                deleteWorldButton.text = "Delete World";
                deleteWorldButton.style.backgroundColor = new StyleColor(
                    new Color(0.8f, 0.2f, 0.2f, 0.3f)
                );
                deleteWorldButton.clicked += () => OnDeleteWorldClicked(capturedWorld, worldEntry);

                worldWrapper.Add(worldEntry);

                var zoneToggleButtons = new List<Button>();

                activateAllBtn.clicked += async () =>
                {
                    activateAllBtn.SetEnabled(false);
                    bool activating = activateAllBtn.text == "Activate All";

                    foreach (var zoneBtn in zoneToggleButtons)
                        zoneBtn.SetEnabled(false);

                    bool success = true;
                    foreach (var zone in zones)
                    {
                        var res = activating
                            ? await zoneService.ActivateZone(
                                world.id,
                                zone.zone_id,
                                session.APIToken
                            )
                            : await zoneService.DeactivateZone(
                                world.id,
                                zone.zone_id,
                                session.APIToken
                            );

                        if (!string.IsNullOrEmpty(res.error))
                            success = false;
                        else
                            zone.is_active = activating;
                    }

                    if (success)
                    {
                        bool nowAnyActive = activating;
                        activateAllBtn.text = nowAnyActive ? "Deactivate All" : "Activate All";
                        activateAllBtn.style.backgroundColor = new StyleColor(
                            nowAnyActive
                                ? new Color(0.8f, 0.2f, 0.2f, 0.3f)
                                : new Color(0.2f, 0.6f, 0.2f, 0.3f)
                        );

                        foreach (var zoneBtn in zoneToggleButtons)
                            zoneBtn.text = activating ? "Deactivate" : "Activate";

                        ToastNotification.Show(
                            $"World {capturedWorld.name} zones {(activating ? "activated" : "deactivated")}.",
                            "success",
                            Color.green
                        );
                        _ = LoadSubscriptionAsync();
                    }
                    else
                    {
                        ToastNotification.Show(
                            $"Some zones in {capturedWorld.name} failed to {(activating ? "activate" : "deactivate")}.",
                            "error",
                            Color.red
                        );
                    }

                    foreach (var zoneBtn in zoneToggleButtons)
                        zoneBtn.SetEnabled(true);
                    activateAllBtn.SetEnabled(true);
                };

                // --- Zone entries ---
                if (zoneCount > 0)
                {
                    foreach (var zone in zones)
                    {
                        VisualElement zoneEntry = worldItemTemplate.Instantiate();
                        zoneEntry.style.paddingLeft = 24;

                        zoneEntry.Q<Label>("Header").text = $"Zone {zone.zone_id}";
                        zoneEntry.Q<Label>("ZoneName").text = $"World: {world.name}";
                        zoneEntry.Q<Label>("SlotBadge").style.display = DisplayStyle.None;

                        // ── Zone status badge via shared utility ───────────────
                        zoneStatusBadge.Apply(zoneEntry.Q<Label>("StatusBadge"), zone.is_online);

                        var capturedZone = zone;
                        var toggleActiveBtn = zoneEntry.Q<Button>("ToggleActive");
                        toggleActiveBtn.text = zone.is_active ? "Deactivate" : "Activate";
                        zoneToggleButtons.Add(toggleActiveBtn);

                        toggleActiveBtn.clicked += async () =>
                        {
                            toggleActiveBtn.SetEnabled(false);
                            bool isActivating = toggleActiveBtn.text == "Activate";

                            var res = isActivating
                                ? await zoneService.ActivateZone(
                                    world.id,
                                    capturedZone.zone_id,
                                    session.APIToken
                                )
                                : await zoneService.DeactivateZone(
                                    world.id,
                                    capturedZone.zone_id,
                                    session.APIToken
                                );

                            if (string.IsNullOrEmpty(res.error))
                            {
                                capturedZone.is_active = isActivating;
                                toggleActiveBtn.text = isActivating ? "Deactivate" : "Activate";
                                ToastNotification.Show(
                                    $"Zone {capturedZone.zone_id} {(isActivating ? "activated" : "deactivated")}.",
                                    "success",
                                    Color.green
                                );
                                _ = LoadSubscriptionAsync();
                            }
                            else
                            {
                                ToastNotification.Show(
                                    $"Failed to {(isActivating ? "activate" : "deactivate")} zone {capturedZone.zone_id}: {res.error}",
                                    "error",
                                    Color.red
                                );
                            }
                            toggleActiveBtn.SetEnabled(true);
                        };

                        zoneEntry.Q<Button>("Unsubscribe").style.display = DisplayStyle.None;
                        zoneContainer.Add(zoneEntry);
                    }
                    worldWrapper.Add(zoneContainer);
                }

                worldsList.Add(worldWrapper);
            }
        }

        private async void OnDeleteWorldClicked(API.WorldMetadata world, VisualElement entry)
        {
            var button = entry.Q<Button>("Unsubscribe");
            button.SetEnabled(false);

            try
            {
                var (error, statusCode) = await worldService.DeleteWorld(
                    world.id,
                    session.APIToken
                );

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"{error} (status {statusCode})");

                entry.parent.RemoveFromHierarchy();
                ToastNotification.Show($"World '{world.name}' deleted.", "info", Color.aliceBlue);

                if (dataPersistenceManager != null)
                {
                    var localWorldData = dataPersistenceManager.GetWorldData(world.name);
                    if (localWorldData != null && localWorldData.worldId == world.id)
                    {
                        localWorldData.worldId = "";
                        dataPersistenceManager.SaveWorldMetadata(localWorldData);
                    }
                }

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

        // ── Helpers ───────────────────────────────────────────────────────────

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

        private void ShowPanel(VisualElement panel)
        {
            notLoggedInPanel.style.display = DisplayStyle.None;
            createSubscriptionPanel.style.display = DisplayStyle.None;
            subscriptionPanel.style.display = DisplayStyle.None;
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
