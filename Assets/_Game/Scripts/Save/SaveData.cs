using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzle.Save
{
    [Serializable]
    public class SaveData
    {
        public const int CURRENT_VERSION = 1;

        // Version for migration
        public int Version = CURRENT_VERSION;

        // Grid state
        public int[] GridState;

        // Current game state
        public int Score;
        public int Combo;
        public List<PieceInfo> CurrentPieces;
        public int PiecesUsedThisSet;

        // Progression
        public int XP;
        public int PlayerLevel;
        public int Coins;

        // Daily systems
        public int DailyStreak;
        public string LastDailyRewardDate;
        public string LastDailyChallengeDate;
        public bool DailyChallengeCompleted;

        // Statistics
        public int TotalGamesPlayed;
        public int HighScore;
        public int TotalClears;

        // Settings
        public bool SoundEnabled = true;
        public bool HapticsEnabled = true;

        // Unlocks
        public List<string> UnlockedThemes;
        public List<string> UnlockedBoards;

        // IAP
        public bool AdsRemoved;

        // Timestamps
        public long LastSaveTimestamp;

        // Checksum for validation
        public string Checksum;

        [Serializable]
        public class PieceInfo
        {
            public int PieceId;
            public int RotationIndex;
        }

        public SaveData()
        {
            CurrentPieces = new List<PieceInfo>();
            UnlockedThemes = new List<string> { "default" };
            UnlockedBoards = new List<string> { "default" };
            LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void UpdateTimestamp()
        {
            LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public string CalculateChecksum()
        {
            // Simple checksum based on critical values
            int hash = 17;
            hash = hash * 31 + Score;
            hash = hash * 31 + Coins;
            hash = hash * 31 + XP;
            hash = hash * 31 + PlayerLevel;
            hash = hash * 31 + HighScore;
            hash = hash * 31 + (AdsRemoved ? 1 : 0);
            return hash.ToString("X8");
        }

        public bool ValidateChecksum()
        {
            return Checksum == CalculateChecksum();
        }

        public void PrepareForSave()
        {
            UpdateTimestamp();
            Checksum = CalculateChecksum();
        }
    }
}
