using System;
using UnityEngine;
using BlockPuzzle.Economy;
using BlockPuzzle.Save;
using BlockPuzzle.Analytics;
using BlockPuzzle.Core;

namespace BlockPuzzle.Progression
{
    public class DailyRewardManager : MonoBehaviour
    {
        public static DailyRewardManager Instance { get; private set; }

        public event Action<int, int> OnRewardClaimed; // day, coins
        public event Action OnStreakReset;

        private int _currentStreak;
        private string _lastClaimDate;

        public int CurrentStreak => _currentStreak;
        public bool CanClaimToday => !HasClaimedToday();
        public int TodayReward => GetRewardForDay(_currentStreak);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            LoadFromSave();
            CheckStreak();
        }

        private void LoadFromSave()
        {
            var saveData = SaveManager.Instance?.CurrentData;
            if (saveData != null)
            {
                _currentStreak = saveData.DailyStreak;
                _lastClaimDate = saveData.LastDailyRewardDate;
            }
        }

        private void CheckStreak()
        {
            if (string.IsNullOrEmpty(_lastClaimDate)) return;

            DateTime lastClaim;
            if (!DateTime.TryParse(_lastClaimDate, out lastClaim)) return;

            var today = DateTime.UtcNow.Date;
            var daysSinceClaim = (today - lastClaim.Date).Days;

            // If more than 1 day passed, reset streak
            if (daysSinceClaim > 1)
            {
                _currentStreak = 0;
                OnStreakReset?.Invoke();
            }
        }

        public bool HasClaimedToday()
        {
            if (string.IsNullOrEmpty(_lastClaimDate)) return false;

            DateTime lastClaim;
            if (!DateTime.TryParse(_lastClaimDate, out lastClaim)) return false;

            return lastClaim.Date == DateTime.UtcNow.Date;
        }

        public bool TryClaimReward()
        {
            if (HasClaimedToday()) return false;

            int reward = GetRewardForDay(_currentStreak);

            // Add coins
            EconomyManager.Instance?.AddCoins(reward, "daily_reward");

            // Update streak
            _currentStreak = (_currentStreak + 1) % 7; // Cycle through 7 days
            _lastClaimDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Save
            var saveData = SaveManager.Instance?.CurrentData;
            if (saveData != null)
            {
                saveData.DailyStreak = _currentStreak;
                saveData.LastDailyRewardDate = _lastClaimDate;
                SaveManager.Instance?.SaveGame();
            }

            OnRewardClaimed?.Invoke(_currentStreak, reward);
            AnalyticsManager.Instance?.TrackDailyRewardClaimed(_currentStreak, reward);

            return true;
        }

        public int GetRewardForDay(int day)
        {
            var config = GameManager.Instance?.Config;
            if (config == null || config.DailyRewardCoins == null || config.DailyRewardCoins.Length == 0)
            {
                return 10 + (day * 10); // Default fallback
            }

            int index = Mathf.Clamp(day, 0, config.DailyRewardCoins.Length - 1);
            return config.DailyRewardCoins[index];
        }

        public int[] GetAllRewards()
        {
            var config = GameManager.Instance?.Config;
            return config?.DailyRewardCoins ?? new int[] { 10, 20, 30, 40, 50, 75, 100 };
        }
    }
}
