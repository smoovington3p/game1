using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzle.Analytics
{
    public interface IAnalyticsProvider
    {
        void Initialize();
        void TrackEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserProperty(string name, object value);
    }

    /// <summary>
    /// Stub analytics provider for development.
    /// Replace with Firebase/Unity Analytics in production.
    /// </summary>
    public class StubAnalyticsProvider : IAnalyticsProvider
    {
        public void Initialize()
        {
            Debug.Log("[Analytics] Stub provider initialized");
        }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            var paramStr = parameters != null ? string.Join(", ", parameters) : "none";
            Debug.Log($"[Analytics] Event: {eventName} | Params: {paramStr}");
        }

        public void SetUserProperty(string name, object value)
        {
            Debug.Log($"[Analytics] UserProperty: {name} = {value}");
        }
    }

    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        private IAnalyticsProvider _provider;
        private int _sessionNumber;
        private float _sessionStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Use stub provider by default - replace with real SDK provider in production
            _provider = new StubAnalyticsProvider();
            _provider.Initialize();

            _sessionNumber = PlayerPrefs.GetInt("session_count", 0) + 1;
            PlayerPrefs.SetInt("session_count", _sessionNumber);
            _sessionStartTime = Time.time;

            TrackSessionStart();
        }

        public void SetProvider(IAnalyticsProvider provider)
        {
            _provider = provider;
            _provider.Initialize();
        }

        // === Session Events ===

        public void TrackSessionStart()
        {
            _provider.TrackEvent("session_start", new Dictionary<string, object>
            {
                { "session_number", _sessionNumber }
            });
        }

        // === Gameplay Events ===

        public void TrackRunStart(bool isDailyChallenge = false)
        {
            _provider.TrackEvent("run_start", new Dictionary<string, object>
            {
                { "is_daily_challenge", isDailyChallenge }
            });
        }

        public void TrackRunEnd(int score, float duration, string endReason)
        {
            _provider.TrackEvent("run_end", new Dictionary<string, object>
            {
                { "score", score },
                { "duration", duration },
                { "end_reason", endReason }
            });
        }

        public void TrackPlacement(int pieceId)
        {
            _provider.TrackEvent("placement", new Dictionary<string, object>
            {
                { "piece_id", pieceId }
            });
        }

        public void TrackClear(int rows, int cols, int blocks, int comboCount)
        {
            _provider.TrackEvent("clear", new Dictionary<string, object>
            {
                { "rows", rows },
                { "cols", cols },
                { "blocks", blocks },
                { "combo_count", comboCount }
            });
        }

        // === Monetization Events ===

        public void TrackAdShown(string adType, string placement)
        {
            _provider.TrackEvent("ad_shown", new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "placement", placement }
            });
        }

        public void TrackAdCompleted(string adType, string placement)
        {
            _provider.TrackEvent("ad_completed", new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "placement", placement }
            });
        }

        public void TrackAdFailed(string adType, string placement, string error)
        {
            _provider.TrackEvent("ad_failed", new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "placement", placement },
                { "error", error }
            });
        }

        public void TrackIAPAttempt(string productId)
        {
            _provider.TrackEvent("iap_attempt", new Dictionary<string, object>
            {
                { "product_id", productId }
            });
        }

        public void TrackIAPSuccess(string productId, float price)
        {
            _provider.TrackEvent("iap_success", new Dictionary<string, object>
            {
                { "product_id", productId },
                { "price", price }
            });
        }

        // === Daily Events ===

        public void TrackDailyRewardClaimed(int day, int coinsAwarded)
        {
            _provider.TrackEvent("daily_reward_claimed", new Dictionary<string, object>
            {
                { "day", day },
                { "coins", coinsAwarded }
            });
        }

        public void TrackDailyChallengeStarted(string dateKey)
        {
            _provider.TrackEvent("daily_challenge_started", new Dictionary<string, object>
            {
                { "date", dateKey }
            });
        }

        public void TrackDailyChallengeCompleted(string dateKey, int score)
        {
            _provider.TrackEvent("daily_challenge_completed", new Dictionary<string, object>
            {
                { "date", dateKey },
                { "score", score }
            });
        }

        // === Economy Events ===

        public void TrackCoinEarned(int amount, string source)
        {
            _provider.TrackEvent("coin_earned", new Dictionary<string, object>
            {
                { "amount", amount },
                { "source", source }
            });
        }

        public void TrackCoinSpent(int amount, string item)
        {
            _provider.TrackEvent("coin_spent", new Dictionary<string, object>
            {
                { "amount", amount },
                { "item", item }
            });
        }

        // === Progression Events ===

        public void TrackLevelUp(int newLevel)
        {
            _provider.TrackEvent("level_up", new Dictionary<string, object>
            {
                { "new_level", newLevel }
            });
            _provider.SetUserProperty("player_level", newLevel);
        }

        public void TrackUnlock(string unlockType, string unlockId)
        {
            _provider.TrackEvent("unlock", new Dictionary<string, object>
            {
                { "type", unlockType },
                { "id", unlockId }
            });
        }

        // === Booster Events ===

        public void TrackBoosterUsed(string boosterType)
        {
            _provider.TrackEvent("booster_used", new Dictionary<string, object>
            {
                { "type", boosterType }
            });
        }
    }
}
