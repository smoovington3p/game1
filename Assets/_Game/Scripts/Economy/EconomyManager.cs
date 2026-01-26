using System;
using UnityEngine;
using BlockPuzzle.Save;
using BlockPuzzle.Analytics;
using BlockPuzzle.Core;

namespace BlockPuzzle.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        public event Action<int> OnCoinsChanged;
        public event Action<int, string> OnCoinsEarned; // amount, source
        public event Action<int, string> OnCoinsSpent; // amount, item
        public event Action<string> OnInsufficientFunds; // item

        private int _coins;

        public int Coins => _coins;

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
                _coins = saveData.Coins;
            }
        }

        public void AddCoins(int amount, string source)
        {
            if (amount <= 0) return;

            _coins += amount;
            OnCoinsChanged?.Invoke(_coins);
            OnCoinsEarned?.Invoke(amount, source);
            AnalyticsManager.Instance?.TrackCoinEarned(amount, source);
        }

        public bool TrySpendCoins(int amount, string item)
        {
            if (amount <= 0) return true;

            if (_coins < amount)
            {
                OnInsufficientFunds?.Invoke(item);
                return false;
            }

            _coins -= amount;
            OnCoinsChanged?.Invoke(_coins);
            OnCoinsSpent?.Invoke(amount, item);
            AnalyticsManager.Instance?.TrackCoinSpent(amount, item);
            return true;
        }

        public bool CanAfford(int amount)
        {
            return _coins >= amount;
        }

        public void GetSaveData(SaveData saveData)
        {
            saveData.Coins = _coins;
        }

        // === Booster Methods ===

        public bool TryUseUndo()
        {
            var config = GameManager.Instance?.Config;
            int cost = config?.UndoCost ?? 50;

            if (TrySpendCoins(cost, "undo"))
            {
                AnalyticsManager.Instance?.TrackBoosterUsed("undo");
                return true;
            }
            return false;
        }

        public bool TryUseExtraPiece()
        {
            var config = GameManager.Instance?.Config;
            int cost = config?.ExtraPieceCost ?? 30;

            if (TrySpendCoins(cost, "extra_piece"))
            {
                AnalyticsManager.Instance?.TrackBoosterUsed("extra_piece");
                return true;
            }
            return false;
        }

        public bool TryUseBomb()
        {
            var config = GameManager.Instance?.Config;
            int cost = config?.BombCost ?? 100;

            if (TrySpendCoins(cost, "bomb"))
            {
                AnalyticsManager.Instance?.TrackBoosterUsed("bomb");
                return true;
            }
            return false;
        }

        public int GetUndoCost() => GameManager.Instance?.Config?.UndoCost ?? 50;
        public int GetExtraPieceCost() => GameManager.Instance?.Config?.ExtraPieceCost ?? 30;
        public int GetBombCost() => GameManager.Instance?.Config?.BombCost ?? 100;
    }
}
