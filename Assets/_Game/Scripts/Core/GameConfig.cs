using UnityEngine;

namespace BlockPuzzle.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BlockPuzzle/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        public int GridWidth = 9;
        public int GridHeight = 9;
        public bool Enable3x3BlockClears = true;

        [Header("Scoring")]
        public int PointsPerTile = 1;
        public int PointsPerClear = 10;
        public int PerfectClearBonus = 100;
        public float ComboMultiplierIncrement = 0.1f;
        public float MaxComboMultiplier = 3f;

        [Header("Piece Generation")]
        public int PiecesPerSet = 3;
        public int DifficultyScalingStartLevel = 10;
        [Range(0f, 1f)] public float SmallPieceBaseWeight = 0.6f;
        [Range(0f, 1f)] public float LargePieceMaxWeight = 0.5f;

        [Header("Economy")]
        public int CoinsPerClear = 1;
        public int CoinsPerCombo = 2;
        public int CoinsPerGameComplete = 10;
        public int UndoCost = 50;
        public int ExtraPieceCost = 30;
        public int BombCost = 100;

        [Header("Ads")]
        public int GamesBeforeInterstitial = 3;
        public int SessionsBeforeInterstitial = 3;
        public float MinTimeBetweenInterstitials = 120f;

        [Header("Daily Rewards")]
        public int[] DailyRewardCoins = { 10, 20, 30, 40, 50, 75, 100 };
        public int DailyChallengeReward = 50;
    }
}
