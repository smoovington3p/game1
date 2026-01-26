using System;

namespace BlockPuzzle.Ads
{
    public enum AdType
    {
        Rewarded,
        Interstitial,
        Banner
    }

    public enum AdPlacement
    {
        Continue,
        ExtraPiece,
        DoubleDailyReward,
        ScoreBoost,
        GameComplete
    }

    public interface IAdService
    {
        /// <summary>
        /// Initialize the ad service.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Returns true if a rewarded ad is ready to show.
        /// </summary>
        bool IsRewardedAdReady();

        /// <summary>
        /// Returns true if an interstitial ad is ready to show.
        /// </summary>
        bool IsInterstitialAdReady();

        /// <summary>
        /// Shows a rewarded ad.
        /// </summary>
        /// <param name="placement">Where in the app the ad is being shown.</param>
        /// <param name="onComplete">Called when user completes watching the ad.</param>
        /// <param name="onFailed">Called if ad fails to show or user skips.</param>
        void ShowRewardedAd(AdPlacement placement, Action onComplete, Action<string> onFailed);

        /// <summary>
        /// Shows an interstitial ad.
        /// </summary>
        /// <param name="onComplete">Called when ad is dismissed.</param>
        /// <param name="onFailed">Called if ad fails to show.</param>
        void ShowInterstitialAd(Action onComplete, Action<string> onFailed);

        /// <summary>
        /// Shows a banner ad.
        /// </summary>
        void ShowBanner();

        /// <summary>
        /// Hides the banner ad.
        /// </summary>
        void HideBanner();

        /// <summary>
        /// Preloads ads for faster display.
        /// </summary>
        void PreloadAds();
    }
}
