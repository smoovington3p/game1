using System;
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzle.Save;
using BlockPuzzle.Analytics;

namespace BlockPuzzle.Progression
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        public event Action<int> OnXPGained;
        public event Action<int> OnLevelUp;
        public event Action<string, string> OnUnlock; // type, id

        [SerializeField] private ProgressionConfig _config;

        private int _xp;
        private int _level;
        private List<string> _unlockedThemes;
        private List<string> _unlockedBoards;

        public int XP => _xp;
        public int Level => _level;
        public int XPToNextLevel => GetXPRequiredForLevel(_level + 1) - _xp;
        public float LevelProgress => GetLevelProgress();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _unlockedThemes = new List<string> { "default" };
            _unlockedBoards = new List<string> { "default" };
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
                _xp = saveData.XP;
                _level = Mathf.Max(1, saveData.PlayerLevel);
                if (saveData.UnlockedThemes != null)
                    _unlockedThemes = new List<string>(saveData.UnlockedThemes);
                if (saveData.UnlockedBoards != null)
                    _unlockedBoards = new List<string>(saveData.UnlockedBoards);
            }
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            _xp += amount;
            OnXPGained?.Invoke(amount);

            // Check for level up
            while (_xp >= GetXPRequiredForLevel(_level + 1))
            {
                _level++;
                OnLevelUp?.Invoke(_level);
                AnalyticsManager.Instance?.TrackLevelUp(_level);
                CheckUnlocks();
            }
        }

        public int GetXPRequiredForLevel(int level)
        {
            if (_config == null) return level * 100;
            return _config.GetXPForLevel(level);
        }

        private float GetLevelProgress()
        {
            int currentLevelXP = GetXPRequiredForLevel(_level);
            int nextLevelXP = GetXPRequiredForLevel(_level + 1);
            int xpInLevel = _xp - currentLevelXP;
            int xpNeeded = nextLevelXP - currentLevelXP;
            return xpNeeded > 0 ? (float)xpInLevel / xpNeeded : 0f;
        }

        private void CheckUnlocks()
        {
            if (_config == null) return;

            // Check theme unlocks
            foreach (var unlock in _config.ThemeUnlocks)
            {
                if (_level >= unlock.RequiredLevel && !_unlockedThemes.Contains(unlock.Id))
                {
                    _unlockedThemes.Add(unlock.Id);
                    OnUnlock?.Invoke("theme", unlock.Id);
                    AnalyticsManager.Instance?.TrackUnlock("theme", unlock.Id);
                }
            }

            // Check board unlocks
            foreach (var unlock in _config.BoardUnlocks)
            {
                if (_level >= unlock.RequiredLevel && !_unlockedBoards.Contains(unlock.Id))
                {
                    _unlockedBoards.Add(unlock.Id);
                    OnUnlock?.Invoke("board", unlock.Id);
                    AnalyticsManager.Instance?.TrackUnlock("board", unlock.Id);
                }
            }
        }

        public bool IsThemeUnlocked(string themeId) => _unlockedThemes.Contains(themeId);
        public bool IsBoardUnlocked(string boardId) => _unlockedBoards.Contains(boardId);
        public List<string> GetUnlockedThemes() => new List<string>(_unlockedThemes);
        public List<string> GetUnlockedBoards() => new List<string>(_unlockedBoards);

        public void GetSaveData(SaveData saveData)
        {
            saveData.XP = _xp;
            saveData.PlayerLevel = _level;
            saveData.UnlockedThemes = new List<string>(_unlockedThemes);
            saveData.UnlockedBoards = new List<string>(_unlockedBoards);
        }
    }
}
