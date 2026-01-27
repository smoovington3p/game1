using System;
using UnityEngine;

namespace BlockPuzzle.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        DailyChallenge
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnGameStateChanged;
        public event Action OnGameOver;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnComboChanged;

        [SerializeField] private GameConfig _gameConfig;

        private GameState _currentState = GameState.MainMenu;
        private int _score;
        private int _combo;
        private int _totalClears;
        private float _sessionStartTime;

        public GameState CurrentState => _currentState;
        public int Score => _score;
        public int Combo => _combo;
        public int TotalClears => _totalClears;
        public float SessionDuration => Time.time - _sessionStartTime;
        public GameConfig Config => _gameConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Note: DontDestroyOnLoad is handled by Bootstrap
        }

        /// <summary>
        /// Called by Bootstrap to set the config.
        /// </summary>
        public void SetConfig(GameConfig config)
        {
            _gameConfig = config;
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            var previousState = _currentState;
            _currentState = newState;

            if (newState == GameState.Playing && previousState != GameState.Paused)
            {
                ResetRunStats();
            }

            OnGameStateChanged?.Invoke(_currentState);
        }

        private void ResetRunStats()
        {
            _score = 0;
            _combo = 0;
            _totalClears = 0;
            _sessionStartTime = Time.time;
            OnScoreChanged?.Invoke(_score);
            OnComboChanged?.Invoke(_combo);
        }

        public void AddScore(int points)
        {
            _score += points;
            OnScoreChanged?.Invoke(_score);
        }

        public void IncrementCombo()
        {
            _combo++;
            OnComboChanged?.Invoke(_combo);
        }

        public void ResetCombo()
        {
            _combo = 0;
            OnComboChanged?.Invoke(_combo);
        }

        public void AddClears(int count)
        {
            _totalClears += count;
        }

        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();
        }

        public int CalculateClearBonus(int rowsCleared, int colsCleared, int blocksCleared)
        {
            if (_gameConfig == null) return 0;

            int totalLines = rowsCleared + colsCleared + blocksCleared;
            int basePoints = totalLines * _gameConfig.PointsPerClear;

            // Combo multiplier
            float comboMultiplier = 1f + (_combo * _gameConfig.ComboMultiplierIncrement);
            comboMultiplier = Mathf.Min(comboMultiplier, _gameConfig.MaxComboMultiplier);

            // Multi-clear bonus
            float multiClearMultiplier = 1f;
            if (totalLines >= 2) multiClearMultiplier = 1.5f;
            if (totalLines >= 3) multiClearMultiplier = 2f;
            if (totalLines >= 4) multiClearMultiplier = 3f;

            return Mathf.RoundToInt(basePoints * comboMultiplier * multiClearMultiplier);
        }

        public int CalculatePlacementPoints(int tileCount)
        {
            if (_gameConfig == null) return tileCount;
            return tileCount * _gameConfig.PointsPerTile;
        }
    }
}
