#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BlockPuzzle.Core;
using BlockPuzzle.Economy;
using BlockPuzzle.Progression;
using BlockPuzzle.Save;
using BlockPuzzle.Ads;
using BlockPuzzle.UI;

namespace BlockPuzzle.Editor
{
    public class DebugPanel : EditorWindow
    {
        private Vector2 _scrollPosition;
        private int _coinsToAdd = 100;
        private int _xpToAdd = 50;

        [MenuItem("BlockPuzzle/Debug Panel")]
        public static void ShowWindow()
        {
            GetWindow<DebugPanel>("Debug Panel");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use debug functions", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawGameStateSection();
            EditorGUILayout.Space(10);
            DrawEconomySection();
            EditorGUILayout.Space(10);
            DrawProgressionSection();
            EditorGUILayout.Space(10);
            DrawSaveSection();
            EditorGUILayout.Space(10);
            DrawAdSection();
            EditorGUILayout.Space(10);
            DrawTuningSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawGameStateSection()
        {
            EditorGUILayout.LabelField("Game State", EditorStyles.boldLabel);

            var gm = GameManager.Instance;
            if (gm == null)
            {
                EditorGUILayout.HelpBox("GameManager not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"State: {gm.CurrentState}");
            EditorGUILayout.LabelField($"Score: {gm.Score}");
            EditorGUILayout.LabelField($"Combo: {gm.Combo}");
            EditorGUILayout.LabelField($"Session Duration: {gm.SessionDuration:F1}s");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Game Over"))
            {
                var gameLoop = Object.FindObjectOfType<GameLoopController>();
                gameLoop?.EndGame("debug_forced");
            }
            if (GUILayout.Button("Reset Game"))
            {
                var gameLoop = Object.FindObjectOfType<GameLoopController>();
                gameLoop?.StartNewGame();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEconomySection()
        {
            EditorGUILayout.LabelField("Economy", EditorStyles.boldLabel);

            var economy = EconomyManager.Instance;
            if (economy == null)
            {
                EditorGUILayout.HelpBox("EconomyManager not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Coins: {economy.Coins}");

            EditorGUILayout.BeginHorizontal();
            _coinsToAdd = EditorGUILayout.IntField("Amount", _coinsToAdd);
            if (GUILayout.Button("Add Coins"))
            {
                economy.AddCoins(_coinsToAdd, "debug");
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Give 1000 Coins"))
            {
                economy.AddCoins(1000, "debug");
            }
        }

        private void DrawProgressionSection()
        {
            EditorGUILayout.LabelField("Progression", EditorStyles.boldLabel);

            var progression = ProgressionManager.Instance;
            if (progression == null)
            {
                EditorGUILayout.HelpBox("ProgressionManager not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Level: {progression.Level}");
            EditorGUILayout.LabelField($"XP: {progression.XP}");
            EditorGUILayout.LabelField($"XP to Next: {progression.XPToNextLevel}");

            EditorGUILayout.BeginHorizontal();
            _xpToAdd = EditorGUILayout.IntField("Amount", _xpToAdd);
            if (GUILayout.Button("Add XP"))
            {
                progression.AddXP(_xpToAdd);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Level Up"))
            {
                progression.AddXP(progression.XPToNextLevel + 1);
            }
        }

        private void DrawSaveSection()
        {
            EditorGUILayout.LabelField("Save System", EditorStyles.boldLabel);

            var save = SaveManager.Instance;
            if (save == null)
            {
                EditorGUILayout.HelpBox("SaveManager not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Has Save: {save.HasSaveData}");
            EditorGUILayout.LabelField($"High Score: {save.GetHighScore()}");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Save"))
            {
                save.SaveGame();
            }
            if (GUILayout.Button("Force Load"))
            {
                save.LoadGame();
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("DELETE ALL SAVE DATA"))
            {
                if (EditorUtility.DisplayDialog("Delete Save Data",
                    "Are you sure you want to delete all save data? This cannot be undone.",
                    "Delete", "Cancel"))
                {
                    save.DeleteSaveFiles();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawAdSection()
        {
            EditorGUILayout.LabelField("Ads", EditorStyles.boldLabel);

            var adManager = AdManager.Instance;
            if (adManager == null)
            {
                EditorGUILayout.HelpBox("AdManager not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Ads Removed: {adManager.AdsRemoved}");
            EditorGUILayout.LabelField($"Rewarded Ready: {adManager.IsRewardedAdReady()}");
            EditorGUILayout.LabelField($"Should Show Interstitial: {adManager.ShouldShowInterstitial()}");

            if (GUILayout.Button("Trigger Mock Rewarded Ad"))
            {
                adManager.ShowRewardedAd(AdPlacement.Continue,
                    () => Debug.Log("[Debug] Rewarded ad completed"),
                    () => Debug.Log("[Debug] Rewarded ad failed"));
            }

            if (GUILayout.Button("Trigger Mock Interstitial"))
            {
                adManager.ShowInterstitialIfReady(() => Debug.Log("[Debug] Interstitial done"));
            }
        }

        private void DrawTuningSection()
        {
            EditorGUILayout.LabelField("Tuning Values", EditorStyles.boldLabel);

            var config = GameManager.Instance?.Config;
            if (config == null)
            {
                EditorGUILayout.HelpBox("GameConfig not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Grid", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  Size: {config.GridWidth}x{config.GridHeight}");
            EditorGUILayout.LabelField($"  3x3 Clears: {config.Enable3x3BlockClears}");

            EditorGUILayout.LabelField("Scoring", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  Per Tile: {config.PointsPerTile}");
            EditorGUILayout.LabelField($"  Per Clear: {config.PointsPerClear}");
            EditorGUILayout.LabelField($"  Combo Mult: {config.ComboMultiplierIncrement}");

            EditorGUILayout.LabelField("Economy", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  Undo Cost: {config.UndoCost}");
            EditorGUILayout.LabelField($"  Bomb Cost: {config.BombCost}");

            if (GUILayout.Button("Select GameConfig"))
            {
                Selection.activeObject = config;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif
