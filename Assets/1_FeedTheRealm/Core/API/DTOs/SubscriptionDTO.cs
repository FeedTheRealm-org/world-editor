namespace API
{
    [System.Serializable]
    public class CreateSubscriptionRequest
    {
        public int slots;
        public string success_url;
        public string cancel_url;
    }

    [System.Serializable]
    public class UpdateSlotsRequest
    {
        public int slots;
    }

    [System.Serializable]
    public class SubscriptionResponse
    {
        public int slots;
        public int used_slots;
        public string status;
        public string next_billing_date;
        public float amount_due;
    }

    [System.Serializable]
    public class CheckoutSessionResponse
    {
        public string checkout_url;
    }

    [System.Serializable]
    public class PricingInfoResponse
    {
        public string price_per_slot;
        public string next_billing_date;
    }
}
