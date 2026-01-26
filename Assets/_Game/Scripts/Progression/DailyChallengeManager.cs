using System;
using UnityEngine;
using BlockPuzzle.Core;
using BlockPuzzle.Economy;
using BlockPuzzle.Save;
using BlockPuzzle.Analytics;

namespace BlockPuzzle.Progression
{
    public class DailyChallengeManager : MonoBehaviour
    {
        public static DailyChallengeManager Instance { get; private set; }

        public event Action<string> OnChallengeStarted;
        public event Action<int, int> OnChallengeCompleted; // score, reward

        private string _currentDateKey;
        private bool _completedToday;
        private int _todayScore;

        public string TodayKey => GetTodayKey();
        public bool CompletedToday => _completedToday;
        public int TodayScore => _todayScore;

        /// <summary>
        /// Seed for today's challenge. Same seed = same puzzle for all players.
        /// </summary>
        public int TodaySeed => GetSeedForDate(TodayKey);

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
        }

        private void LoadFromSave()
        {
            var saveData = SaveManager.Instance?.CurrentData;
            if (saveData != null)
            {
                _currentDateKey = saveData.LastDailyChallengeDate;
                _completedToday = saveData.DailyChallengeCompleted && _currentDateKey == TodayKey;
            }
        }

        private string GetTodayKey()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
        }

        private int GetSeedForDate(string dateKey)
        {
            // Deterministic seed from date string
            int hash = 17;
            foreach (char c in dateKey)
            {
                hash = hash * 31 + c;
            }
            return hash;
        }

        public void StartDailyChallenge()
        {
            _currentDateKey = TodayKey;

            // Tell game loop to use today's seed
            var gameLoop = FindObjectOfType<GameLoopController>();
            if (gameLoop != null)
            {
                gameLoop.StartNewGame(TodaySeed);
            }

            GameManager.Instance?.SetState(GameState.DailyChallenge);

            OnChallengeStarted?.Invoke(_currentDateKey);
            AnalyticsManager.Instance?.TrackDailyChallengeStarted(_currentDateKey);
        }

        public void CompleteDailyChallenge(int score)
        {
            if (_completedToday) return;

            _completedToday = true;
            _todayScore = score;

            // Award reward
            var config = GameManager.Instance?.Config;
            int reward = config?.DailyChallengeReward ?? 50;
            EconomyManager.Instance?.AddCoins(reward, "daily_challenge");

            // Add XP
            ProgressionManager.Instance?.AddXP(100); // Daily challenge XP

            // Save
            var saveData = SaveManager.Instance?.CurrentData;
            if (saveData != null)
            {
                saveData.LastDailyChallengeDate = _currentDateKey;
                saveData.DailyChallengeCompleted = true;
                SaveManager.Instance?.SaveGame();
            }

            OnChallengeCompleted?.Invoke(score, reward);
            AnalyticsManager.Instance?.TrackDailyChallengeCompleted(_currentDateKey, score);
        }

        public bool CanPlayDailyChallenge()
        {
            return !_completedToday || _currentDateKey != TodayKey;
        }
    }
}
