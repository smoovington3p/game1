using System;

namespace BlockPuzzle.IAP
{
    public enum IAPProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    public class IAPProduct
    {
        public string ProductId;
        public string DisplayName;
        public string Description;
        public string PriceString;
        public float PriceValue;
        public IAPProductType Type;
    }

    public interface IIAPService
    {
        /// <summary>
        /// Initialize the IAP service.
        /// </summary>
        void Initialize(Action onSuccess, Action<string> onFailed);

        /// <summary>
        /// Returns true if the service is initialized.
        /// </summary>
        bool IsInitialized();

        /// <summary>
        /// Gets product info for a product ID.
        /// </summary>
        IAPProduct GetProduct(string productId);

        /// <summary>
        /// Starts a purchase flow.
        /// </summary>
        void Purchase(string productId, Action<string> onSuccess, Action<string> onFailed);

        /// <summary>
        /// Restores previous purchases (iOS requirement).
        /// </summary>
        void RestorePurchases(Action onSuccess, Action<string> onFailed);

        /// <summary>
        /// Checks if a non-consumable product has been purchased.
        /// </summary>
        bool HasPurchased(string productId);
    }
}
