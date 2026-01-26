using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzle.Progression
{
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "BlockPuzzle/Progression Config")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("XP Curve")]
        [Tooltip("Base XP needed for level 2")]
        public int BaseXP = 100;

        [Tooltip("XP multiplier per level")]
        public float XPMultiplier = 1.2f;

        [Header("XP Sources")]
        public int XPPerGameComplete = 50;
        public int XPPerClear = 10;
        public int XPPerCombo = 5;
        public int XPPerDailyChallenge = 100;

        [Header("Theme Unlocks")]
        public List<LevelUnlock> ThemeUnlocks = new List<LevelUnlock>();

        [Header("Board Unlocks")]
        public List<LevelUnlock> BoardUnlocks = new List<LevelUnlock>();

        [Serializable]
        public class LevelUnlock
        {
            public string Id;
            public string DisplayName;
            public int RequiredLevel;
        }

        public int GetXPForLevel(int level)
        {
            if (level <= 1) return 0;

            // Cumulative XP needed to reach this level
            float totalXP = 0;
            for (int i = 2; i <= level; i++)
            {
                totalXP += BaseXP * Mathf.Pow(XPMultiplier, i - 2);
            }
            return Mathf.RoundToInt(totalXP);
        }
    }
}
