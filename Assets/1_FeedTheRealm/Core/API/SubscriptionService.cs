using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    [CreateAssetMenu(
        fileName = "SubscriptionService",
        menuName = "Scriptable Objects/API/SubscriptionService"
    )]
    public class SubscriptionService : BaseApiService
    {
        [Header("API Config")]
        [SerializeField]
        private ApiConfig apiConfig;

        [SerializeField]
        private Session.Session session;

        private string GetBaseUrl() => $"{apiConfig.Hostname}:{apiConfig.Port}/subscriptions";

        // ── GetPricingInfo ────────────────────────────────────────────────────

        public async Task<(
            PricingInfoResponse data,
            string error,
            long statusCode
        )> GetPricingInfo()
        {
            string url = $"{GetBaseUrl()}/pricing";

            var (responseText, result, statusCode) = await SendRequestAsync(
                url,
                "GET",
                session.AccessToken,
                null,
                "GetPricingInfo"
            );

            if (result == UnityWebRequest.Result.ConnectionError)
            {
                logger.Log(
                    $"[SubscriptionService] GetPricingInfo connection error: {responseText}",
                    this,
                    Logging.LogType.Error
                );
                return (
                    null,
                    "Unable to connect to server. Please check your internet connection.",
                    statusCode
                );
            }

            if (result == UnityWebRequest.Result.ProtocolError)
            {
                var err = ParseError(responseText, statusCode);
                logger.Log(
                    $"[SubscriptionService] GetPricingInfo error ({statusCode}): {err}",
                    this,
                    Logging.LogType.Error
                );
                return (null, err, statusCode);
            }

            var res = JsonUtility.FromJson<DataEnvelope<PricingInfoResponse>>(responseText);
            return (res.data, null, statusCode);
        }

        // ── GetSubscription ───────────────────────────────────────────────────

        /// Returns the current subscription for the logged-in user.
        public async Task<(
            SubscriptionResponse data,
            string error,
            long statusCode
        )> GetSubscription()
        {
            string url = $"{GetBaseUrl()}/status";

            var (responseText, result, statusCode) = await SendRequestAsync(
                url,
                "GET",
                session.AccessToken,
                null,
                "GetSubscription"
            );

            if (result == UnityWebRequest.Result.ConnectionError)
            {
                logger.Log(
                    $"[SubscriptionService] GetSubscription connection error: {responseText}",
                    this,
                    Logging.LogType.Error
                );
                return (
                    null,
                    "Unable to connect to server. Please check your internet connection.",
                    statusCode
                );
            }

            if (result == UnityWebRequest.Result.ProtocolError)
            {
                var err = ParseError(responseText, statusCode);
                logger.Log(
                    $"[SubscriptionService] GetSubscription error ({statusCode}): {err}",
                    this,
                    Logging.LogType.Error
                );
                return (null, err, statusCode);
            }

            var res = JsonUtility.FromJson<DataEnvelope<SubscriptionResponse>>(responseText);
            logger.Log($"[SubscriptionService] GetSubscription response: {responseText}", this);
            return (res.data, null, statusCode);
        }

        // ── CreateCheckoutSession ─────────────────────────────────────────────

        /// Creates a Stripe Checkout session and returns the URL to redirect the user to.
        public async Task<(
            string checkoutUrl,
            string error,
            long statusCode
        )> CreateCheckoutSession(int slots, string successUrl, string cancelUrl)
        {
            string url = $"{GetBaseUrl()}/checkout";

            var payload = new CreateSubscriptionRequest
            {
                slots = slots,
                success_url = successUrl,
                cancel_url = cancelUrl,
            };
            string json = JsonUtility.ToJson(payload);

            Task<(string, UnityWebRequest.Result, long)> task = SendRequestAsync(
                url,
                "POST",
                session.AccessToken,
                json,
                "CreateCheckoutSession"
            );

            (string responseText, UnityWebRequest.Result result, long statusCode) = await task;

            if (result == UnityWebRequest.Result.ConnectionError)
            {
                logger.Log(
                    $"[SubscriptionService] CreateCheckoutSession connection error: {responseText}",
                    this,
                    Logging.LogType.Error
                );
                return (
                    null,
                    "Unable to connect to server. Please check your internet connection.",
                    statusCode
                );
            }

            if (result == UnityWebRequest.Result.ProtocolError)
            {
                var err = ParseError(responseText, statusCode);
                logger.Log(
                    $"[SubscriptionService] CreateCheckoutSession error ({statusCode}): {err}",
                    this,
                    Logging.LogType.Error
                );
                return (null, err, statusCode);
            }

            var res = JsonUtility.FromJson<DataEnvelope<CheckoutSessionResponse>>(responseText);
            logger.Log(
                $"[SubscriptionService] Checkout session created: {res.data.checkout_url}",
                this
            );
            return (res.data.checkout_url, null, statusCode);
        }

        // ── UpdateSlots ───────────────────────────────────────────────────────

        /// Updates the slot count on the active subscription.
        public async Task<(SubscriptionResponse data, string error, long statusCode)> UpdateSlots(
            int newSlots
        )
        {
            string url = $"{GetBaseUrl()}/slots";

            var payload = new UpdateSlotsRequest { slots = newSlots };
            string json = JsonUtility.ToJson(payload);

            var (responseText, result, statusCode) = await SendRequestAsync(
                url,
                "PUT",
                session.AccessToken,
                json,
                "UpdateSlots"
            );

            if (result == UnityWebRequest.Result.ConnectionError)
            {
                logger.Log(
                    $"[SubscriptionService] UpdateSlots connection error: {responseText}",
                    this,
                    Logging.LogType.Error
                );
                return (
                    null,
                    "Unable to connect to server. Please check your internet connection.",
                    statusCode
                );
            }

            if (result == UnityWebRequest.Result.ProtocolError)
            {
                var err = ParseError(responseText, statusCode);
                logger.Log(
                    $"[SubscriptionService] UpdateSlots error ({statusCode}): {err}",
                    this,
                    Logging.LogType.Error
                );
                return (null, err, statusCode);
            }

            var res = JsonUtility.FromJson<DataEnvelope<SubscriptionResponse>>(responseText);
            logger.Log($"[SubscriptionService] UpdateSlots response: {responseText}", this);
            return (res.data, null, statusCode);
        }

        // ── UnsubscribeZone ───────────────────────────────────────────────────

        /// Removes a single zone from the subscription at end of the billing period.
        public async Task<(string error, long statusCode)> UnsubscribeZone(string zoneId)
        {
            string url = $"{GetBaseUrl()}/{session.UserID}/zones/{zoneId}";

            var (responseText, result, statusCode) = await SendRequestAsync(
                url,
                "DELETE",
                session.AccessToken,
                null,
                "UnsubscribeZone"
            );

            if (result == UnityWebRequest.Result.ConnectionError)
            {
                logger.Log(
                    $"[SubscriptionService] UnsubscribeZone connection error: {responseText}",
                    this,
                    Logging.LogType.Error
                );
                return (
                    "Unable to connect to server. Please check your internet connection.",
                    statusCode
                );
            }

            if (result == UnityWebRequest.Result.ProtocolError)
            {
                var err = ParseError(responseText, statusCode);
                logger.Log(
                    $"[SubscriptionService] UnsubscribeZone error ({statusCode}): {err}",
                    this,
                    Logging.LogType.Error
                );
                return (err, statusCode);
            }

            logger.Log(
                $"[SubscriptionService] Zone {zoneId} unsubscribed for user {session.UserID}",
                this
            );
            return (null, statusCode);
        }

        // ── CancelSubscription ────────────────────────────────────────────────

        /// Cancels the entire subscription immediately.
        public async Task<(string error, long statusCode)> CancelSubscription()
        {
            string url = GetBaseUrl();

            var (responseText, result, statusCode) = await SendRequestAsync(
                url,
                "DELETE",
                session.AccessToken,
                null,
                "CancelSubscription"
            );

            if (result == UnityWebRequest.Result.ConnectionError)
            {
                logger.Log(
                    $"[SubscriptionService] CancelSubscription connection error: {responseText}",
                    this,
                    Logging.LogType.Error
                );
                return (
                    "Unable to connect to server. Please check your internet connection.",
                    statusCode
                );
            }

            if (result == UnityWebRequest.Result.ProtocolError)
            {
                var err = ParseError(responseText, statusCode);
                logger.Log(
                    $"[SubscriptionService] CancelSubscription error ({statusCode}): {err}",
                    this,
                    Logging.LogType.Error
                );
                return (err, statusCode);
            }

            logger.Log(
                $"[SubscriptionService] Subscription cancelled for user {session.UserID}",
                this
            );
            return (null, statusCode);
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static string ParseError(string responseText, long statusCode)
        {
            if (string.IsNullOrEmpty(responseText))
                return $"Unexpected error (status {statusCode}).";

            var err = JsonUtility.FromJson<ErrorResponse>(responseText);

            if (statusCode >= 500)
                return "Server error. Please try again later.";

            return err?.detail ?? err?.detail ?? responseText;
        }
    }
}
