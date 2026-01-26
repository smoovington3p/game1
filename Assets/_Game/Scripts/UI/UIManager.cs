using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using BlockPuzzle.Core;

namespace BlockPuzzle.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public event Action<string> OnSceneTransitionStart;
        public event Action<string> OnSceneTransitionComplete;

        [Header("Scene Names")]
        public string MainMenuScene = "MainMenu";
        public string GameScene = "Game";
        public string DailyChallengeScene = "DailyChallenge";
        public string SettingsScene = "Settings";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadMainMenu()
        {
            LoadScene(MainMenuScene);
            GameManager.Instance?.SetState(GameState.MainMenu);
        }

        public void LoadGame()
        {
            LoadScene(GameScene);
        }

        public void LoadDailyChallenge()
        {
            LoadScene(DailyChallengeScene);
        }

        public void LoadSettings()
        {
            LoadScene(SettingsScene);
        }

        private void LoadScene(string sceneName)
        {
            OnSceneTransitionStart?.Invoke(sceneName);
            SceneManager.LoadScene(sceneName);
            OnSceneTransitionComplete?.Invoke(sceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
