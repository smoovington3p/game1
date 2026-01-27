using System;
using System.IO;
using UnityEngine;
using BlockPuzzle.Core;

namespace BlockPuzzle.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;
        public event Action<string> OnSaveError;

        private const string SAVE_FILENAME = "save.json";
        private const string BACKUP_FILENAME = "save_backup.json";
        private const string TEMP_FILENAME = "save_temp.json";

        private SaveData _currentData;
        private string _savePath;
        private string _backupPath;
        private string _tempPath;

        public SaveData CurrentData => _currentData;
        public bool HasSaveData => _currentData != null && _currentData.GridState != null && _currentData.GridState.Length > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
            _backupPath = Path.Combine(Application.persistentDataPath, BACKUP_FILENAME);
            _tempPath = Path.Combine(Application.persistentDataPath, TEMP_FILENAME);

            LoadGame();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        public void SaveGame()
        {
            try
            {
                if (_currentData == null)
                {
                    _currentData = new SaveData();
                }

                // Get current game state
                var gameLoop = FindObjectOfType<GameLoopController>();
                if (gameLoop != null && GameManager.Instance?.CurrentState == GameState.Playing)
                {
                    gameLoop.GetSaveData(_currentData);
                }

                // Get progression data
                var progression = FindObjectOfType<Progression.ProgressionManager>();
                if (progression != null)
                {
                    progression.GetSaveData(_currentData);
                }

                // Get economy data
                var economy = FindObjectOfType<Economy.EconomyManager>();
                if (economy != null)
                {
                    economy.GetSaveData(_currentData);
                }

                _currentData.PrepareForSave();

                // Atomic write: temp -> backup -> main
                string json = JsonUtility.ToJson(_currentData, true);

                // Write to temp file
                File.WriteAllText(_tempPath, json);

                // Backup current save if it exists
                if (File.Exists(_savePath))
                {
                    File.Copy(_savePath, _backupPath, true);
                }

                // Move temp to main (Unity 2022 doesn't support 3-arg overwrite)
                if (File.Exists(_savePath))
                {
                    File.Delete(_savePath);
                }
                File.Move(_tempPath, _savePath);

                OnSaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
                OnSaveError?.Invoke(e.Message);
            }
        }

        public void LoadGame()
        {
            try
            {
                _currentData = null;

                // Try main save first
                if (File.Exists(_savePath))
                {
                    string json = File.ReadAllText(_savePath);
                    _currentData = JsonUtility.FromJson<SaveData>(json);

                    // Validate checksum
                    if (_currentData != null && !_currentData.ValidateChecksum())
                    {
                        Debug.LogWarning("Save checksum invalid, trying backup...");
                        _currentData = null;
                    }
                }

                // Try backup if main failed
                if (_currentData == null && File.Exists(_backupPath))
                {
                    string json = File.ReadAllText(_backupPath);
                    _currentData = JsonUtility.FromJson<SaveData>(json);

                    if (_currentData != null && !_currentData.ValidateChecksum())
                    {
                        Debug.LogWarning("Backup checksum also invalid, starting fresh");
                        _currentData = null;
                    }
                }

                // Migrate if needed
                if (_currentData != null)
                {
                    MigrateIfNeeded(_currentData);
                }

                // Create new save data if none exists
                if (_currentData == null)
                {
                    _currentData = new SaveData();
                }

                OnLoadCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                _currentData = new SaveData();
            }
        }

        private void MigrateIfNeeded(SaveData data)
        {
            if (data.Version < SaveData.CURRENT_VERSION)
            {
                // Add migration logic here for future versions
                // Example:
                // if (data.Version < 2) { MigrateV1ToV2(data); }
                // if (data.Version < 3) { MigrateV2ToV3(data); }

                data.Version = SaveData.CURRENT_VERSION;
                Debug.Log($"Migrated save data to version {SaveData.CURRENT_VERSION}");
            }
        }

        public void ResetProgress()
        {
            _currentData = new SaveData();
            SaveGame();
        }

        public void DeleteSaveFiles()
        {
            try
            {
                if (File.Exists(_savePath)) File.Delete(_savePath);
                if (File.Exists(_backupPath)) File.Delete(_backupPath);
                if (File.Exists(_tempPath)) File.Delete(_tempPath);
                _currentData = new SaveData();
            }
            catch (Exception e)
            {
                Debug.LogError($"Delete save failed: {e.Message}");
            }
        }

        // Utility methods for quick access
        public int GetCoins() => _currentData?.Coins ?? 0;
        public int GetXP() => _currentData?.XP ?? 0;
        public int GetPlayerLevel() => _currentData?.PlayerLevel ?? 1;
        public int GetHighScore() => _currentData?.HighScore ?? 0;
        public bool IsAdsRemoved() => _currentData?.AdsRemoved ?? false;

        public void SetCoins(int value)
        {
            if (_currentData != null) _currentData.Coins = value;
        }

        public void SetHighScore(int value)
        {
            if (_currentData != null && value > _currentData.HighScore)
                _currentData.HighScore = value;
        }
    }
}
