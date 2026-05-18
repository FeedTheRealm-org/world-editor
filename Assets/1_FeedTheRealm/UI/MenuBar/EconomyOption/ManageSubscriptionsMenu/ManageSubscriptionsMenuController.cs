using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.UIEvents;
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
        // ── Serialized ────────────────────────────────────────────────────────
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

        // ── Injected ──────────────────────────────────────────────────────────
        [Inject]
        private WorldUIObjectProvider worldUIObjectProvider;

        [Inject]
        private readonly WorldPrefabProvider prefabProvider;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        [Inject]
        private AuthFlowManager authFlowManager;

        [Inject]
        private UpdateLoginEvent updateLoginEvent;

        // ── UI refs ───────────────────────────────────────────────────────────
        private ZoneStatusBadgeController zoneStatusBadge;
        private VisualElement notLoggedInPanel,
            createSubscriptionPanel,
            subscriptionPanel,
            loadingPanel;
        private Button closeButton,
            loginButton,
            createSubscriptionButton,
            cancelSubscriptionButton;
        private Button decreaseSlotsButton,
            increaseSlotsButton,
            updateSlotsButton;
        private Button decreaseCreateSlotsButton,
            increaseCreateSlotsButton;
        private Button prevPageButton,
            nextPageButton;
        private Label billingDateValue,
            amountDueValue,
            activeZonesValue,
            freeZonesValue;
        private Label slotCountLabel,
            slotFeedbackLabel,
            createSlotCountLabel;
        private Label createSubscriptionFeedbackLabel,
            pageLabel;
        private ScrollView worldsList;

        // ── State ─────────────────────────────────────────────────────────────
        private SubscriptionResponse currentSubscription;
        private int pendingSlotCount;
        private int pendingCreateSlotCount = MinSlots;
        private int currentPage;
        private SubscriptionsCallbackServer callbackServer;

        private const int MinSlots = 1;
        private const int MaxSlots = 50;
        private const int WorldsPerPage = 3;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private async void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            zoneStatusBadge = GetComponent<ZoneStatusBadgeController>();
            BindElements(root);
            RegisterCallbacks();
            await LoadSubscriptionAsync();
            updateLoginEvent.OnRaised += async () => await LoadSubscriptionAsync();
        }

        private void OnDisable() => UnregisterCallbacks();

        // ── Bind & callbacks ──────────────────────────────────────────────────

        private void BindElements(VisualElement root)
        {
            notLoggedInPanel = root.Q("NotLoggedInPanel");
            createSubscriptionPanel = root.Q("CreateSubscriptionPanel");
            subscriptionPanel = root.Q("SubscriptionPanel");
            loadingPanel = root.Q("LoadingPanel");

            closeButton = root.Q<Button>("Close");
            loginButton = root.Q<Button>("LoginButton");
            createSubscriptionButton = root.Q<Button>("CreateSubscriptionButton");
            cancelSubscriptionButton = root.Q<Button>("CancelSubscriptionButton");
            decreaseSlotsButton = root.Q<Button>("DecreaseSlots");
            increaseSlotsButton = root.Q<Button>("IncreaseSlots");
            updateSlotsButton = root.Q<Button>("UpdateSlots");
            decreaseCreateSlotsButton = root.Q<Button>("DecreaseCreateSlots");
            increaseCreateSlotsButton = root.Q<Button>("IncreaseCreateSlots");
            prevPageButton = root.Q<Button>("PrevPageButton");
            nextPageButton = root.Q<Button>("NextPageButton");

            billingDateValue = root.Q<Label>("BillingDateValue");
            amountDueValue = root.Q<Label>("AmountDueValue");
            activeZonesValue = root.Q<Label>("ActiveZonesValue");
            freeZonesValue = root.Q<Label>("FreeZonesValue");
            slotCountLabel = root.Q<Label>("SlotCountLabel");
            slotFeedbackLabel = root.Q<Label>("SlotFeedbackLabel");
            createSlotCountLabel = root.Q<Label>("CreateSlotCountLabel");
            createSubscriptionFeedbackLabel = root.Q<Label>("CreateSubscriptionFeedbackLabel");
            pageLabel = root.Q<Label>("PageLabel");

            worldsList = root.Q<ScrollView>("WorldsList");
        }

        private void RegisterCallbacks()
        {
            closeButton.clicked += CloseMenu;
            loginButton.clicked += authFlowManager.ShowAuthMenu;
            createSubscriptionButton.clicked += OnCreateSubscriptionClicked;
            decreaseSlotsButton.clicked += OnDecreaseSlots;
            increaseSlotsButton.clicked += OnIncreaseSlots;
            updateSlotsButton.clicked += OnUpdateSlotsClicked;
            decreaseCreateSlotsButton.clicked += OnDecreaseCreateSlots;
            increaseCreateSlotsButton.clicked += OnIncreaseCreateSlots;
            prevPageButton.clicked += OnPrevPage;
            nextPageButton.clicked += OnNextPage;
            if (cancelSubscriptionButton != null)
                cancelSubscriptionButton.clicked += OnCancelSubscriptionClicked;
        }

        private void UnregisterCallbacks()
        {
            closeButton.clicked -= CloseMenu;
            loginButton.clicked -= authFlowManager.ShowAuthMenu;
            createSubscriptionButton.clicked -= OnCreateSubscriptionClicked;
            decreaseSlotsButton.clicked -= OnDecreaseSlots;
            increaseSlotsButton.clicked -= OnIncreaseSlots;
            updateSlotsButton.clicked -= OnUpdateSlotsClicked;
            decreaseCreateSlotsButton.clicked -= OnDecreaseCreateSlots;
            increaseCreateSlotsButton.clicked -= OnIncreaseCreateSlots;
            prevPageButton.clicked -= OnPrevPage;
            nextPageButton.clicked -= OnNextPage;
            if (cancelSubscriptionButton != null)
                cancelSubscriptionButton.clicked -= OnCancelSubscriptionClicked;
        }

        // ── Load subscription ─────────────────────────────────────────────────

        private async Task LoadSubscriptionAsync()
        {
            await session.EnsureValidSession();

            var (isLogged, _) = await authService.IsLogged();
            if (!isLogged)
            {
                session.ClearSession();
                ShowPanel(notLoggedInPanel);
                return;
            }

            ShowPanel(loadingPanel);

            try
            {
                var (data, error, statusCode) = await subscriptionService.GetSubscription();

                bool noSub =
                    !string.IsNullOrEmpty(error) && IsMissingSubscription(statusCode, error);
                bool isActive =
                    data != null
                    && string.Equals(data.status, "active", StringComparison.OrdinalIgnoreCase);

                if (noSub || data == null || !isActive)
                {
                    RefreshCreateSubscriptionPanel();
                    ShowPanel(createSubscriptionPanel);
                    return;
                }

                currentSubscription = data;
                pendingSlotCount = data.slots;

                RefreshBillingSummary();
                RefreshSlotControls();
                await LoadWorldsAsync();
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
            billingDateValue.text =
                DateTime.TryParse(currentSubscription.next_billing_date, out var d) && d.Year > 1
                    ? d.ToLocalTime().ToString("MMM dd, yyyy")
                    : "—";

            amountDueValue.text = $"$ {currentSubscription.amount_due:F2}";
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
            if (pendingSlotCount > MinSlots)
            {
                pendingSlotCount--;
                RefreshSlotControls();
            }
        }

        private void OnIncreaseSlots()
        {
            if (pendingSlotCount < MaxSlots)
            {
                pendingSlotCount++;
                RefreshSlotControls();
            }
        }

        private void RefreshCreateSlotControls()
        {
            createSlotCountLabel.text = pendingCreateSlotCount.ToString();
            decreaseCreateSlotsButton.SetEnabled(pendingCreateSlotCount > MinSlots);
            increaseCreateSlotsButton.SetEnabled(pendingCreateSlotCount < MaxSlots);
        }

        private void OnDecreaseCreateSlots()
        {
            if (pendingCreateSlotCount > MinSlots)
            {
                pendingCreateSlotCount--;
                RefreshCreateSlotControls();
            }
        }

        private void OnIncreaseCreateSlots()
        {
            if (pendingCreateSlotCount < MaxSlots)
            {
                pendingCreateSlotCount++;
                RefreshCreateSlotControls();
            }
        }

        private void OnUpdateSlotsClicked()
        {
            ConfirmAction(
                "Update Slots",
                $"Are you sure you want to update your slot count to {pendingSlotCount}? This change cannot be undone.",
                async () =>
                {
                    updateSlotsButton.SetEnabled(false);
                    slotFeedbackLabel.text = "Updating...";

                    try
                    {
                        var (_, error, statusCode) = await subscriptionService.UpdateSlots(
                            pendingSlotCount
                        );
                        if (!string.IsNullOrEmpty(error))
                            throw new Exception(error);

                        await LoadSubscriptionAsync();

                        currentSubscription.slots = pendingSlotCount;
                        RefreshBillingSummary();
                        RefreshSlotControls();
                        slotFeedbackLabel.text = "Updated successfully.";
                        ToastNotification.Show("Slots updated!", "success", Color.green);
                    }
                    catch (Exception ex)
                    {
                        pendingSlotCount = currentSubscription.slots;
                        RefreshSlotControls();
                        slotFeedbackLabel.text = "Update failed. Try again.";
                        ToastNotification.Show(
                            $"Failed to update slots: {ex.Message}",
                            "error",
                            Color.red
                        );
                    }
                }
            );
        }

        private void OnCancelSubscriptionClicked()
        {
            if (currentSubscription?.used_slots > 0)
            {
                ToastNotification.Show(
                    "Cannot cancel subscription. Please delete all your worlds first.",
                    "error",
                    Color.red
                );
                return;
            }

            ConfirmAction(
                "Cancel Subscription",
                "Are you sure you want to cancel your subscription? This will remove access to your allocated worlds.",
                async () =>
                {
                    cancelSubscriptionButton?.SetEnabled(false);

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
                        await LoadSubscriptionAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.Log(
                            $"[SubscriptionMenu] Cancel error: {ex.Message}",
                            this,
                            Logging.LogType.Error
                        );
                        ToastNotification.Show(
                            $"Failed to cancel subscription: {ex.Message}",
                            "error",
                            Color.red
                        );
                        cancelSubscriptionButton?.SetEnabled(true);
                    }
                }
            );
        }

        // ── Checkout ──────────────────────────────────────────────────────────

        private void InitCallbackServer()
        {
            callbackServer ??=
                gameObject.GetComponent<SubscriptionsCallbackServer>()
                ?? gameObject.AddComponent<SubscriptionsCallbackServer>();
        }

        private async void OnCreateSubscriptionClicked()
        {
            createSubscriptionButton.SetEnabled(false);
            createSubscriptionFeedbackLabel.text = "Opening checkout...";
            InitCallbackServer();

            try
            {
                var (pricingData, pricingError, _) = await subscriptionService.GetPricingInfo();
                if (!string.IsNullOrEmpty(pricingError))
                    throw new Exception($"Failed to get pricing info: {pricingError}");

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

                decimal.TryParse(
                    pricingData?.price_per_slot,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal pricePerSlot
                );

                _ = callbackServer
                    .StartServer(
                        new SubscriptionData
                        {
                            Slots = pendingCreateSlotCount,
                            PricePerSlot = pricePerSlot.ToString(
                                "F2",
                                System.Globalization.CultureInfo.InvariantCulture
                            ),
                            TotalPrice = (pricePerSlot * pendingCreateSlotCount).ToString(
                                "F2",
                                System.Globalization.CultureInfo.InvariantCulture
                            ),
                        }
                    )
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            logger.Log(
                                $"Subscription server error: {t.Exception?.Message}",
                                this,
                                Logging.LogType.Error
                            );
                    });

                Application.OpenURL(checkoutUrl);
                createSubscriptionFeedbackLabel.text = "Checkout opened in your browser.";
                ToastNotification.Show("Subscription checkout opened.", "success", Color.green);
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
            if (callbackServer == null)
                return;
            callbackServer.OnSuccessEvent -= OnSubscriptionSuccess;
            callbackServer.OnCancelledEvent -= OnSubscriptionCancelled;
        }

        // ── Worlds list & pagination ───────────────────────────────────────────

        private async Task LoadWorldsAsync()
        {
            currentPage = 0;
            await FetchAndRenderPage();
        }

        private async Task FetchAndRenderPage()
        {
            worldsList.Clear();
            var loadingLabel = new Label("Loading worlds...");
            loadingLabel.AddToClassList("header-label");
            worldsList.Add(loadingLabel);

            int offset = currentPage * WorldsPerPage;
            var (amount, worlds, error) = await worldService.GetWorldPage(
                offset,
                WorldsPerPage + 1,
                "",
                true
            );

            worldsList.Clear();

            if (!string.IsNullOrEmpty(error))
            {
                var errorLabel = new Label($"Error loading worlds: {error}");
                errorLabel.AddToClassList("header-label");
                worldsList.Add(errorLabel);
                return;
            }

            bool hasNextPage = worlds != null && worlds.Count > WorldsPerPage;
            var pageWorlds =
                worlds == null
                    ? new List<WorldMetadata>()
                    : worlds.GetRange(0, Math.Min(WorldsPerPage, worlds.Count));

            if (pageWorlds.Count == 0)
            {
                var emptyLabel = new Label("No created worlds yet.");
                emptyLabel.AddToClassList("header-label");
                worldsList.Add(emptyLabel);
            }
            else
            {
                foreach (var world in pageWorlds)
                    worldsList.Add(BuildWorldEntry(world));
            }

            pageLabel.text = $"Page {currentPage + 1}";
            prevPageButton.SetEnabled(currentPage > 0);
            nextPageButton.SetEnabled(hasNextPage);
        }

        private void OnPrevPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                _ = FetchAndRenderPage();
            }
        }

        private void OnNextPage()
        {
            if (nextPageButton.enabledSelf)
            {
                currentPage++;
                _ = FetchAndRenderPage();
            }
        }

        // ── World entry builder ───────────────────────────────────────────────

        private VisualElement BuildWorldEntry(WorldMetadata world)
        {
            var zones = world.zones ?? new List<WorldZoneMetadata>();
            int zoneCount = zones.Count;
            int activeZoneCount = zones.FindAll(z => z.is_active).Count;

            var wrapper = new VisualElement();
            var worldEntry = worldItemTemplate.Instantiate();
            worldEntry.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 0.5f));

            worldEntry.Q<Label>("Header").text = world.name;
            worldEntry.Q<Label>("ZoneName").text = $"{activeZoneCount}/{zoneCount} active zone(s)";
            worldEntry.Q<Label>("SlotBadge").style.display = DisplayStyle.None;

            var worldBadgeContainer = worldEntry.Q<VisualElement>("ZoneStatusBadgeContainer");
            worldBadgeContainer.Clear();
            worldBadgeContainer.Add(zoneStatusBadge.Create(zoneStatusBadge.Evaluate(zones)));

            // Dropdown
            bool isExpanded = false;
            var dropdownBtn = worldEntry.Q<Button>("DropdownBtn");
            var zoneContainer = new VisualElement { style = { display = DisplayStyle.None } };
            dropdownBtn.text = "▶";

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

            // Activate All / Deactivate All
            var zoneToggleButtons = new List<Button>();
            var activateAllBtn = worldEntry.Q<Button>("ToggleActive");
            bool anyActive = activeZoneCount > 0;
            SetActivateAllBtn(activateAllBtn, anyActive);

            activateAllBtn.clicked += () =>
            {
                bool activating = activateAllBtn.text == "Activate All";
                ConfirmAction(
                    activating ? "Activate World Zones" : "Deactivate World Zones",
                    activating
                        ? $"Are you sure you want to activate all zones in '{world.name}'?"
                        : $"Are you sure you want to deactivate all zones in '{world.name}'?",
                    async () =>
                    {
                        activateAllBtn.SetEnabled(false);
                        zoneToggleButtons.ForEach(b => b.SetEnabled(false));

                        bool success = true;

                        foreach (var zone in zones)
                        {
                            var res = activating
                                ? await zoneService.ActivateZone(world.id, zone.zone_id)
                                : await zoneService.DeactivateZone(world.id, zone.zone_id);

                            if (!string.IsNullOrEmpty(res.error))
                                success = false;
                            else
                                zone.is_active = activating;
                        }

                        if (success)
                        {
                            SetActivateAllBtn(activateAllBtn, activating);
                            zoneToggleButtons.ForEach(b =>
                            {
                                b.text = activating ? "Deactivate" : "Activate";
                                b.style.backgroundColor = new StyleColor(
                                    activating
                                        ? new Color(0.8f, 0.2f, 0.2f, 0.3f)
                                        : new Color(0.2f, 0.6f, 0.2f, 0.3f)
                                );
                            });
                            ToastNotification.Show(
                                $"World {world.name} zones {(activating ? "activated" : "deactivated")}",
                                "success",
                                Color.green
                            );
                            _ = LoadSubscriptionAsync();
                        }
                        else
                        {
                            ToastNotification.Show(
                                $"Some zones in {world.name} failed.",
                                "error",
                                Color.red
                            );
                        }

                        activateAllBtn.SetEnabled(true);
                        zoneToggleButtons.ForEach(b => b.SetEnabled(true));
                    }
                );
            };
            // Delete
            var deleteBtn = worldEntry.Q<Button>("Unsubscribe");
            deleteBtn.text = "Delete World";
            deleteBtn.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f, 0.3f));
            deleteBtn.clicked += () => OnDeleteWorldClicked(world, worldEntry);

            wrapper.Add(worldEntry);

            // Zone entries
            if (zoneCount > 0)
            {
                foreach (var zone in zones)
                {
                    var zoneEntry = BuildZoneEntry(world, zone);
                    var btn = zoneEntry.Q<Button>("ToggleActive");
                    zoneToggleButtons.Add(btn);
                    zoneContainer.Add(zoneEntry);
                }
                wrapper.Add(zoneContainer);
            }

            return wrapper;
        }

        private VisualElement BuildZoneEntry(WorldMetadata world, WorldZoneMetadata zone)
        {
            var zoneEntry = worldItemTemplate.Instantiate();
            zoneEntry.style.paddingLeft = 24;

            zoneEntry.Q<Label>("Header").text = $"Zone {zone.zone_id}";
            zoneEntry.Q<Label>("ZoneName").text = $"World: {world.name}";
            zoneEntry.Q<Label>("SlotBadge").style.display = DisplayStyle.None;

            var badgeContainer = zoneEntry.Q<VisualElement>("ZoneStatusBadgeContainer");
            badgeContainer.Clear();
            badgeContainer.Add(
                zoneStatusBadge.Create(
                    zone.is_online
                        ? ZoneStatusBadgeController.State.Online
                        : ZoneStatusBadgeController.State.Offline
                )
            );

            var toggleBtn = zoneEntry.Q<Button>("ToggleActive");
            toggleBtn.text = zone.is_active ? "Deactivate" : "Activate";
            toggleBtn.style.backgroundColor = new StyleColor(
                zone.is_active
                    ? new Color(0.8f, 0.2f, 0.2f, 0.3f)
                    : new Color(0.2f, 0.6f, 0.2f, 0.3f)
            );

            toggleBtn.clicked += () =>
            {
                bool activating = toggleBtn.text == "Activate";
                ConfirmAction(
                    activating ? "Activate Zone" : "Deactivate Zone",
                    activating
                        ? $"Are you sure you want to activate zone {zone.zone_id} in '{world.name}'?"
                        : $"Are you sure you want to deactivate zone {zone.zone_id} in '{world.name}'?",
                    async () =>
                    {
                        toggleBtn.SetEnabled(false);

                        var res = activating
                            ? await zoneService.ActivateZone(world.id, zone.zone_id)
                            : await zoneService.DeactivateZone(world.id, zone.zone_id);

                        if (string.IsNullOrEmpty(res.error))
                        {
                            zone.is_active = activating;
                            toggleBtn.text = activating ? "Deactivate" : "Activate";
                            toggleBtn.style.backgroundColor = new StyleColor(
                                activating
                                    ? new Color(0.8f, 0.2f, 0.2f, 0.3f)
                                    : new Color(0.2f, 0.6f, 0.2f, 0.3f)
                            );
                            ToastNotification.Show(
                                $"Zone {zone.zone_id} {(activating ? "activated" : "deactivated")}",
                                "success",
                                Color.green
                            );
                            _ = LoadSubscriptionAsync();
                        }
                        else
                        {
                            ToastNotification.Show(
                                $"Failed to {(activating ? "activate" : "deactivate")} zone {zone.zone_id}: {res.error}",
                                "error",
                                Color.red
                            );
                        }

                        toggleBtn.SetEnabled(true);
                    }
                );
            };
            zoneEntry.Q<Button>("Unsubscribe").style.display = DisplayStyle.None;
            return zoneEntry;
        }

        private static void SetActivateAllBtn(Button btn, bool active)
        {
            btn.text = active ? "Deactivate All" : "Activate All";
            btn.style.backgroundColor = new StyleColor(
                active ? new Color(0.8f, 0.2f, 0.2f, 0.3f) : new Color(0.2f, 0.6f, 0.2f, 0.3f)
            );
        }

        // ── Delete world ──────────────────────────────────────────────────────

        private void OnDeleteWorldClicked(WorldMetadata world, VisualElement entry)
        {
            var button = entry.Q<Button>("Unsubscribe");
            ConfirmAction(
                "Delete World",
                $"Are you sure you want to permanently delete '{world.name}'? This will remove it from your subscription.",
                async () =>
                {
                    button.SetEnabled(false);

                    try
                    {
                        var (error, statusCode) = await worldService.DeleteWorld(world.id);
                        if (!string.IsNullOrEmpty(error))
                            throw new Exception($"{error} (status {statusCode})");

                        entry.parent.RemoveFromHierarchy();
                        ToastNotification.Show(
                            $"World '{world.name}' deleted.",
                            "info",
                            Color.aliceBlue
                        );

                        var localWorldData = dataPersistenceManager?.GetWorldData(world.name);
                        if (localWorldData?.worldId == world.id)
                        {
                            localWorldData.worldId = "";
                            dataPersistenceManager.SaveWorldMetadata(localWorldData);
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
                        ToastNotification.Show(
                            $"Failed to delete world: {ex.Message}",
                            "error",
                            Color.red
                        );
                        button.SetEnabled(true);
                    }
                }
            );
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool IsMissingSubscription(long statusCode, string error) =>
            statusCode == 404
            || error.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0
            || error.IndexOf("no subscription", StringComparison.OrdinalIgnoreCase) >= 0;

        private void ShowPanel(VisualElement panel)
        {
            notLoggedInPanel.style.display = DisplayStyle.None;
            createSubscriptionPanel.style.display = DisplayStyle.None;
            subscriptionPanel.style.display = DisplayStyle.None;
            loadingPanel.style.display = DisplayStyle.None;
            panel.style.display = DisplayStyle.Flex;
        }

        private void ConfirmAction(string title, string question, Func<Task> onConfirm)
        {
            var confirmPopup = Instantiate(prefabProvider.confirmPopup);
            var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
            dialogController.Show(
                question: question,
                onConfirm: () => _ = onConfirm(),
                onCancel: () => { },
                title: title
            );
        }

        public override void OpenMenu(GameObject menuPrefab)
        {
            resolver.Instantiate(menuPrefab);
            Destroy(gameObject);
        }
    }
}
