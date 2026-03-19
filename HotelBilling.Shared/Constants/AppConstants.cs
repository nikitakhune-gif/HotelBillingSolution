namespace HotelBilling.Shared.Constants
{
    public static class AppConstants
    {
        // =========================
        // Application Info
        // =========================
        public const string AppName = "Hotel Billing System";
        public const string DefaultCurrency = "INR";

        // =========================
        // Date Formats
        // =========================
        public const string DateFormat = "dd-MM-yyyy";
        public const string DateTimeFormat = "dd-MM-yyyy HH:mm:ss";

        // =========================
        // Business Rules
        // =========================
        public const int MaxRoomCapacity = 4;
        public const decimal TaxPercentage = 0.18m; // 18% GST

        // =========================
        // API Messages
        // =========================
        public const string SuccessMessage = "Request successful";
        public const string ErrorMessage = "Something went wrong";
        public const string NotFoundMessage = "Record not found";

        // =========================
        // Cache Keys (Future Use)
        // =========================
        public const string CustomerCacheKey = "CUSTOMERS";
        public const string BillingCacheKey = "BILLINGS";

        // =========================
        // Pagination Defaults
        // =========================
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 10;
    }
}