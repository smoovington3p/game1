using System;
using UnityEngine;
using BlockPuzzle.Save;
using BlockPuzzle.Economy;
using BlockPuzzle.Analytics;

namespace BlockPuzzle.IAP
{
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action OnAdsRemoved;

        [SerializeField] private MockIAPService _mockIAPService;

        private IIAPService _iapService;

        public bool IsInitialized => _iapService?.IsInitialized() ?? false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Use mock service by default
            if (_mockIAPService != null)
            {
                _iapService = _mockIAPService;
            }
            else
            {
                _iapService = gameObject.AddComponent<MockIAPService>();
            }

            _iapService.Initialize(
                () => Debug.Log("[IAPManager] Store initialized"),
                (error) => Debug.LogError($"[IAPManager] Store init failed: {error}")
            );
        }

        public void SetIAPService(IIAPService service)
        {
            _iapService = service;
            _iapService.Initialize(
                () => Debug.Log("[IAPManager] Store initialized"),
                (error) => Debug.LogError($"[IAPManager] Store init failed: {error}")
            );
        }

        public IAPProduct GetProduct(string productId)
        {
            return _iapService?.GetProduct(productId);
        }

        public string GetPriceString(string productId)
        {
            return _iapService?.GetProduct(productId)?.PriceString ?? "---";
        }

        // === Purchase Methods ===

        public void PurchaseRemoveAds()
        {
            Purchase(MockIAPService.ProductIds.RemoveAds, (productId) =>
            {
                var saveData = SaveManager.Instance?.CurrentData;
                if (saveData != null)
                {
                    saveData.AdsRemoved = true;
                    SaveManager.Instance?.SaveGame();
                }
                OnAdsRemoved?.Invoke();
            });
        }

        public void PurchaseBoosterPack()
        {
            Purchase(MockIAPService.ProductIds.BoosterPack, (productId) =>
            {
                // Grant boosters - implement booster inventory system
                EconomyManager.Instance?.AddCoins(200, "iap_booster_pack");
            });
        }

        public void PurchaseThemePack()
        {
            Purchase(MockIAPService.ProductIds.ThemePack, (productId) =>
            {
                // Unlock all themes - implement in progression system
            });
        }

        public void PurchaseCoins(string productId)
        {
            Purchase(productId, (id) =>
            {
                int coins = 0;
                if (id == MockIAPService.ProductIds.CoinPack1) coins = 500;
                else if (id == MockIAPService.ProductIds.CoinPack2) coins = 2000;

                if (coins > 0)
                {
                    EconomyManager.Instance?.AddCoins(coins, $"iap_{id}");
                }
            });
        }

        private void Purchase(string productId, Action<string> onProcessReward)
        {
            if (_iapService == null || !_iapService.IsInitialized())
            {
                OnPurchaseFailed?.Invoke("Store not available");
                return;
            }

            var product = _iapService.GetProduct(productId);
            if (product == null)
            {
                OnPurchaseFailed?.Invoke("Product not found");
                return;
            }

            AnalyticsManager.Instance?.TrackIAPAttempt(productId);

            _iapService.Purchase(
                productId,
                (id) =>
                {
                    AnalyticsManager.Instance?.TrackIAPSuccess(id, product.PriceValue);
                    onProcessReward?.Invoke(id);
                    OnPurchaseSuccess?.Invoke(id);
                },
                (error) =>
                {
                    Debug.Log($"[IAPManager] Purchase failed: {error}");
                    OnPurchaseFailed?.Invoke(error);
                }
            );
        }

        public void RestorePurchases()
        {
            if (_iapService == null || !_iapService.IsInitialized())
            {
                OnPurchaseFailed?.Invoke("Store not available");
                return;
            }

            _iapService.RestorePurchases(
                () =>
                {
                    // Check what was restored
                    if (_iapService.HasPurchased(MockIAPService.ProductIds.RemoveAds))
                    {
                        var saveData = SaveManager.Instance?.CurrentData;
                        if (saveData != null)
                        {
                            saveData.AdsRemoved = true;
                            SaveManager.Instance?.SaveGame();
                        }
                        OnAdsRemoved?.Invoke();
                    }

                    OnPurchaseSuccess?.Invoke("restore");
                },
                (error) =>
                {
                    OnPurchaseFailed?.Invoke(error);
                }
            );
        }

        public bool HasRemovedAds()
        {
            return SaveManager.Instance?.IsAdsRemoved() ?? false;
        }
    }
}
