using System;
using System.Collections;
using UnityEngine;

namespace BlockPuzzle.Ads
{
    /// <summary>
    /// Mock ad service for development and testing.
    /// Simulates ad loading delays and success/failure scenarios.
    /// </summary>
    public class MockAdService : MonoBehaviour, IAdService
    {
        [Header("Mock Settings")]
        [SerializeField] private float _loadDelay = 0.5f;
        [SerializeField] private float _showDelay = 2f;
        [SerializeField] [Range(0f, 1f)] private float _successRate = 0.9f;
        [SerializeField] private bool _simulateNoFill = false;

        private bool _rewardedAdReady = false;
        private bool _interstitialAdReady = false;
        private bool _isShowingAd = false;

        public void Initialize()
        {
            Debug.Log("[MockAdService] Initialized");
            StartCoroutine(PreloadAdsCoroutine());
        }

        public bool IsRewardedAdReady() => _rewardedAdReady && !_isShowingAd;
        public bool IsInterstitialAdReady() => _interstitialAdReady && !_isShowingAd;

        public void ShowRewardedAd(AdPlacement placement, Action onComplete, Action<string> onFailed)
        {
            if (_isShowingAd)
            {
                onFailed?.Invoke("Ad already showing");
                return;
            }

            if (!_rewardedAdReady)
            {
                onFailed?.Invoke("No ad ready");
                return;
            }

            StartCoroutine(ShowRewardedAdCoroutine(placement, onComplete, onFailed));
        }

        private IEnumerator ShowRewardedAdCoroutine(AdPlacement placement, Action onComplete, Action<string> onFailed)
        {
            _isShowingAd = true;
            _rewardedAdReady = false;

            Debug.Log($"[MockAdService] Showing rewarded ad for placement: {placement}");

            // Simulate ad duration
            yield return new WaitForSecondsRealtime(_showDelay);

            _isShowingAd = false;

            // Simulate success/failure
            if (UnityEngine.Random.value <= _successRate)
            {
                Debug.Log("[MockAdService] Rewarded ad completed successfully");
                onComplete?.Invoke();
            }
            else
            {
                Debug.Log("[MockAdService] Rewarded ad failed/skipped");
                onFailed?.Invoke("User skipped ad");
            }

            // Preload next ad
            StartCoroutine(PreloadRewardedAdCoroutine());
        }

        public void ShowInterstitialAd(Action onComplete, Action<string> onFailed)
        {
            if (_isShowingAd)
            {
                onFailed?.Invoke("Ad already showing");
                return;
            }

            if (!_interstitialAdReady)
            {
                onFailed?.Invoke("No ad ready");
                return;
            }

            StartCoroutine(ShowInterstitialAdCoroutine(onComplete, onFailed));
        }

        private IEnumerator ShowInterstitialAdCoroutine(Action onComplete, Action<string> onFailed)
        {
            _isShowingAd = true;
            _interstitialAdReady = false;

            Debug.Log("[MockAdService] Showing interstitial ad");

            // Simulate ad duration (shorter than rewarded)
            yield return new WaitForSecondsRealtime(_showDelay * 0.5f);

            _isShowingAd = false;

            if (UnityEngine.Random.value <= _successRate)
            {
                Debug.Log("[MockAdService] Interstitial ad completed");
                onComplete?.Invoke();
            }
            else
            {
                Debug.Log("[MockAdService] Interstitial ad failed");
                onFailed?.Invoke("Ad display failed");
            }

            // Preload next ad
            StartCoroutine(PreloadInterstitialAdCoroutine());
        }

        public void ShowBanner()
        {
            Debug.Log("[MockAdService] Banner shown");
        }

        public void HideBanner()
        {
            Debug.Log("[MockAdService] Banner hidden");
        }

        public void PreloadAds()
        {
            StartCoroutine(PreloadAdsCoroutine());
        }

        private IEnumerator PreloadAdsCoroutine()
        {
            yield return PreloadRewardedAdCoroutine();
            yield return PreloadInterstitialAdCoroutine();
        }

        private IEnumerator PreloadRewardedAdCoroutine()
        {
            yield return new WaitForSecondsRealtime(_loadDelay);

            if (_simulateNoFill)
            {
                Debug.Log("[MockAdService] Rewarded ad: No fill");
                _rewardedAdReady = false;
            }
            else
            {
                Debug.Log("[MockAdService] Rewarded ad ready");
                _rewardedAdReady = true;
            }
        }

        private IEnumerator PreloadInterstitialAdCoroutine()
        {
            yield return new WaitForSecondsRealtime(_loadDelay);

            if (_simulateNoFill)
            {
                Debug.Log("[MockAdService] Interstitial ad: No fill");
                _interstitialAdReady = false;
            }
            else
            {
                Debug.Log("[MockAdService] Interstitial ad ready");
                _interstitialAdReady = true;
            }
        }

        // Debug methods
        public void SetSuccessRate(float rate) => _successRate = Mathf.Clamp01(rate);
        public void SetSimulateNoFill(bool noFill) => _simulateNoFill = noFill;
    }
}
