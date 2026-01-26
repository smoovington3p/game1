using System;
using UnityEngine;
using BlockPuzzle.Core;
using BlockPuzzle.Save;
using BlockPuzzle.Analytics;

namespace BlockPuzzle.Ads
{
    /// <summary>
    /// High-level ad management with frequency capping and rules.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        public event Action OnRewardedAdComplete;
        public event Action<string> OnRewardedAdFailed;

        [SerializeField] private MockAdService _mockAdService;

        private IAdService _adService;
        private int _gamesPlayedSinceInterstitial;
        private float _lastInterstitialTime;
        private int _sessionNumber;

        public bool AdsRemoved => SaveManager.Instance?.IsAdsRemoved() ?? false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sessionNumber = PlayerPrefs.GetInt("session_count", 1);
        }

        private void Start()
        {
            // Use mock service by default
            if (_mockAdService != null)
            {
                _adService = _mockAdService;
            }
            else
            {
                _adService = gameObject.AddComponent<MockAdService>();
            }

            _adService.Initialize();
        }

        public void SetAdService(IAdService service)
        {
            _adService = service;
            _adService.Initialize();
        }

        // === Rewarded Ads ===

        public bool IsRewardedAdReady()
        {
            if (AdsRemoved) return false;
            return _adService?.IsRewardedAdReady() ?? false;
        }

        public void ShowRewardedAd(AdPlacement placement, Action onReward, Action onFailed = null)
        {
            if (_adService == null)
            {
                onFailed?.Invoke();
                return;
            }

            AnalyticsManager.Instance?.TrackAdShown("rewarded", placement.ToString());

            _adService.ShowRewardedAd(
                placement,
                () =>
                {
                    AnalyticsManager.Instance?.TrackAdCompleted("rewarded", placement.ToString());
                    onReward?.Invoke();
                    OnRewardedAdComplete?.Invoke();
                },
                (error) =>
                {
                    AnalyticsManager.Instance?.TrackAdFailed("rewarded", placement.ToString(), error);
                    onFailed?.Invoke();
                    OnRewardedAdFailed?.Invoke(error);
                }
            );
        }

        // === Interstitial Ads ===

        public bool ShouldShowInterstitial()
        {
            if (AdsRemoved) return false;

            var config = GameManager.Instance?.Config;
            if (config == null) return false;

            // Don't show before minimum sessions
            if (_sessionNumber < config.SessionsBeforeInterstitial) return false;

            // Check games since last interstitial
            if (_gamesPlayedSinceInterstitial < config.GamesBeforeInterstitial) return false;

            // Check time since last interstitial
            if (Time.realtimeSinceStartup - _lastInterstitialTime < config.MinTimeBetweenInterstitials) return false;

            return _adService?.IsInterstitialAdReady() ?? false;
        }

        public void ShowInterstitialIfReady(Action onComplete = null)
        {
            if (!ShouldShowInterstitial())
            {
                onComplete?.Invoke();
                return;
            }

            AnalyticsManager.Instance?.TrackAdShown("interstitial", "game_complete");

            _adService.ShowInterstitialAd(
                () =>
                {
                    _gamesPlayedSinceInterstitial = 0;
                    _lastInterstitialTime = Time.realtimeSinceStartup;
                    AnalyticsManager.Instance?.TrackAdCompleted("interstitial", "game_complete");
                    onComplete?.Invoke();
                },
                (error) =>
                {
                    AnalyticsManager.Instance?.TrackAdFailed("interstitial", "game_complete", error);
                    onComplete?.Invoke();
                }
            );
        }

        public void OnGameComplete()
        {
            _gamesPlayedSinceInterstitial++;
        }

        // === Banner Ads ===

        public void ShowBanner()
        {
            if (AdsRemoved) return;
            _adService?.ShowBanner();
        }

        public void HideBanner()
        {
            _adService?.HideBanner();
        }

        // === Specific Placements ===

        public void ShowContinueAd(Action onContinue, Action onDecline = null)
        {
            ShowRewardedAd(AdPlacement.Continue, onContinue, onDecline);
        }

        public void ShowExtraPieceAd(Action onReward, Action onDecline = null)
        {
            ShowRewardedAd(AdPlacement.ExtraPiece, onReward, onDecline);
        }

        public void ShowDoubleDailyRewardAd(Action onDouble, Action onDecline = null)
        {
            ShowRewardedAd(AdPlacement.DoubleDailyReward, onDouble, onDecline);
        }
    }
}
