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
    [RequireComponent(typeof(UIDocument))]
    public class SubscriptionMenuController : MenuController
    {
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
        private VisualElement subscriptionPanel;
        private VisualElement loadingPanel;

        // ── Elements ──────────────────────────────────────────────────────────
        private Button closeButton;
        private Button loginButton;

        private Label billingDateValue;
        private Label amountDueValue;
        private Label activeZonesValue;
        private Label freeZonesValue;

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
            subscriptionPanel = root.Q<VisualElement>("SubscriptionPanel");
            loadingPanel = root.Q<VisualElement>("LoadingPanel");

            closeButton = root.Q<Button>("Close");
            loginButton = root.Q<Button>("LoginButton");

            billingDateValue = root.Q<Label>("BillingDateValue");
            amountDueValue = root.Q<Label>("AmountDueValue");
            activeZonesValue = root.Q<Label>("ActiveZonesValue");
            freeZonesValue = root.Q<Label>("FreeZonesValue");

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
            decreaseSlotsButton.clicked += OnDecreaseSlots;
            increaseSlotsButton.clicked += OnIncreaseSlots;
            updateSlotsButton.clicked += OnUpdateSlotsClicked;
        }

        private void UnregisterCallbacks()
        {
            closeButton.clicked -= CloseMenu;
            loginButton.clicked -= OnLoginClicked;
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
                    throw new Exception(
                        $"Failed to load subscription: {error} (status {statusCode})"
                    );

                currentSubscription = data;
                pendingSlotCount = data.total_slots;

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
            billingDateValue.text = currentSubscription.next_billing_date;
            amountDueValue.text = FormatTotalPrice(
                currentSubscription.price_per_slot,
                currentSubscription.total_slots
            );
            activeZonesValue.text = currentSubscription.total_slots.ToString();
            freeZonesValue.text = (currentSubscription.total_slots / 5).ToString();
        }

        // ── Slot controls ─────────────────────────────────────────────────────

        private void RefreshSlotControls()
        {
            slotCountLabel.text = pendingSlotCount.ToString();
            decreaseSlotsButton.SetEnabled(pendingSlotCount > MinSlots);
            increaseSlotsButton.SetEnabled(pendingSlotCount < MaxSlots);
            updateSlotsButton.SetEnabled(pendingSlotCount != currentSubscription.total_slots);
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
                pendingSlotCount = data.total_slots;

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
                currentSubscription.total_slots--;
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

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ShowPanel(VisualElement panel)
        {
            notLoggedInPanel.style.display = DisplayStyle.None;
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
