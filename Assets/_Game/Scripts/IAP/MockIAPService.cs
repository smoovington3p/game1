using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzle.IAP
{
    /// <summary>
    /// Mock IAP service for development and testing.
    /// </summary>
    public class MockIAPService : MonoBehaviour, IIAPService
    {
        [Header("Mock Settings")]
        [SerializeField] private float _purchaseDelay = 1f;
        [SerializeField] [Range(0f, 1f)] private float _successRate = 1f;
        [SerializeField] private bool _simulateStoreError = false;

        private bool _initialized = false;
        private Dictionary<string, IAPProduct> _products;
        private HashSet<string> _purchasedProducts;

        public static class ProductIds
        {
            public const string RemoveAds = "com.game.removeads";
            public const string BoosterPack = "com.game.boosterpack";
            public const string ThemePack = "com.game.themepack";
            public const string CoinPack1 = "com.game.coins1";
            public const string CoinPack2 = "com.game.coins2";
        }

        private void Awake()
        {
            _products = new Dictionary<string, IAPProduct>();
            _purchasedProducts = new HashSet<string>();
            LoadPurchasedProducts();
        }

        public void Initialize(Action onSuccess, Action<string> onFailed)
        {
            if (_simulateStoreError)
            {
                onFailed?.Invoke("Store unavailable");
                return;
            }

            // Setup mock products
            _products[ProductIds.RemoveAds] = new IAPProduct
            {
                ProductId = ProductIds.RemoveAds,
                DisplayName = "Remove Ads",
                Description = "Remove all ads permanently",
                PriceString = "$4.99",
                PriceValue = 4.99f,
                Type = IAPProductType.NonConsumable
            };

            _products[ProductIds.BoosterPack] = new IAPProduct
            {
                ProductId = ProductIds.BoosterPack,
                DisplayName = "Booster Pack",
                Description = "5 Undos, 5 Extra Pieces, 2 Bombs",
                PriceString = "$2.99",
                PriceValue = 2.99f,
                Type = IAPProductType.Consumable
            };

            _products[ProductIds.ThemePack] = new IAPProduct
            {
                ProductId = ProductIds.ThemePack,
                DisplayName = "Theme Pack",
                Description = "Unlock all themes",
                PriceString = "$1.99",
                PriceValue = 1.99f,
                Type = IAPProductType.NonConsumable
            };

            _products[ProductIds.CoinPack1] = new IAPProduct
            {
                ProductId = ProductIds.CoinPack1,
                DisplayName = "500 Coins",
                Description = "A small pile of coins",
                PriceString = "$0.99",
                PriceValue = 0.99f,
                Type = IAPProductType.Consumable
            };

            _products[ProductIds.CoinPack2] = new IAPProduct
            {
                ProductId = ProductIds.CoinPack2,
                DisplayName = "2000 Coins",
                Description = "A big bag of coins",
                PriceString = "$2.99",
                PriceValue = 2.99f,
                Type = IAPProductType.Consumable
            };

            _initialized = true;
            Debug.Log("[MockIAPService] Initialized with mock products");
            onSuccess?.Invoke();
        }

        public bool IsInitialized() => _initialized;

        public IAPProduct GetProduct(string productId)
        {
            _products.TryGetValue(productId, out var product);
            return product;
        }

        public void Purchase(string productId, Action<string> onSuccess, Action<string> onFailed)
        {
            if (!_initialized)
            {
                onFailed?.Invoke("Store not initialized");
                return;
            }

            if (!_products.ContainsKey(productId))
            {
                onFailed?.Invoke("Product not found");
                return;
            }

            StartCoroutine(ProcessPurchase(productId, onSuccess, onFailed));
        }

        private IEnumerator ProcessPurchase(string productId, Action<string> onSuccess, Action<string> onFailed)
        {
            Debug.Log($"[MockIAPService] Processing purchase: {productId}");

            // Simulate store delay
            yield return new WaitForSecondsRealtime(_purchaseDelay);

            if (UnityEngine.Random.value > _successRate)
            {
                Debug.Log($"[MockIAPService] Purchase failed (simulated): {productId}");
                onFailed?.Invoke("Purchase cancelled");
                yield break;
            }

            var product = _products[productId];

            // Track non-consumables
            if (product.Type == IAPProductType.NonConsumable)
            {
                _purchasedProducts.Add(productId);
                SavePurchasedProducts();
            }

            Debug.Log($"[MockIAPService] Purchase successful: {productId}");
            onSuccess?.Invoke(productId);
        }

        public void RestorePurchases(Action onSuccess, Action<string> onFailed)
        {
            if (!_initialized)
            {
                onFailed?.Invoke("Store not initialized");
                return;
            }

            Debug.Log("[MockIAPService] Restoring purchases...");
            LoadPurchasedProducts();
            onSuccess?.Invoke();
        }

        public bool HasPurchased(string productId)
        {
            return _purchasedProducts.Contains(productId);
        }

        private void SavePurchasedProducts()
        {
            var json = string.Join(",", _purchasedProducts);
            PlayerPrefs.SetString("mock_iap_purchases", json);
            PlayerPrefs.Save();
        }

        private void LoadPurchasedProducts()
        {
            var json = PlayerPrefs.GetString("mock_iap_purchases", "");
            if (!string.IsNullOrEmpty(json))
            {
                foreach (var id in json.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _purchasedProducts.Add(id);
                    }
                }
            }
        }

        // Debug methods
        public void SetSuccessRate(float rate) => _successRate = Mathf.Clamp01(rate);
        public void SetSimulateError(bool error) => _simulateStoreError = error;
        public void ClearPurchases()
        {
            _purchasedProducts.Clear();
            PlayerPrefs.DeleteKey("mock_iap_purchases");
        }
    }
}
