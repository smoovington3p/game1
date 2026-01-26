using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockPuzzle.Core;
using BlockPuzzle.Grid;
using BlockPuzzle.Economy;

namespace BlockPuzzle.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private GameObject _comboContainer;

        [Header("Buttons")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _bombButton;

        [Header("Booster Costs")]
        [SerializeField] private TextMeshProUGUI _undoCostText;
        [SerializeField] private TextMeshProUGUI _bombCostText;

        [Header("Pause Panel")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private Button _continueAdButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _gameOverMainMenuButton;

        private void Start()
        {
            SetupButtons();
            UpdateBoosterCosts();
            HideAllPanels();

            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnComboChanged += UpdateCombo;
                GameManager.Instance.OnGameOver += ShowGameOverPanel;
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnComboChanged -= UpdateCombo;
                GameManager.Instance.OnGameOver -= ShowGameOverPanel;
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void SetupButtons()
        {
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPauseClicked);

            if (_undoButton != null)
                _undoButton.onClick.AddListener(OnUndoClicked);

            if (_bombButton != null)
                _bombButton.onClick.AddListener(OnBombClicked);

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumeClicked);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (_continueAdButton != null)
                _continueAdButton.onClick.AddListener(OnContinueAdClicked);

            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(OnNewGameClicked);

            if (_gameOverMainMenuButton != null)
                _gameOverMainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void HideAllPanels()
        {
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboContainer != null) _comboContainer.SetActive(false);
        }

        private void UpdateBoosterCosts()
        {
            var economy = EconomyManager.Instance;
            if (economy == null) return;

            if (_undoCostText != null)
                _undoCostText.text = economy.GetUndoCost().ToString();

            if (_bombCostText != null)
                _bombCostText.text = economy.GetBombCost().ToString();
        }

        private void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = score.ToString("N0");
        }

        private void UpdateCombo(int combo)
        {
            if (_comboContainer != null)
                _comboContainer.SetActive(combo > 0);

            if (_comboText != null)
                _comboText.text = $"x{combo}";
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    HideAllPanels();
                    break;
                case GameState.Paused:
                    ShowPausePanel();
                    break;
                case GameState.GameOver:
                    ShowGameOverPanel();
                    break;
            }
        }

        // === Panel Methods ===

        private void ShowPausePanel()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(true);
        }

        private void HidePausePanel()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(false);
        }

        private void ShowGameOverPanel()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);

            if (_finalScoreText != null)
                _finalScoreText.text = (GameManager.Instance?.Score ?? 0).ToString("N0");

            if (_highScoreText != null)
            {
                int highScore = Save.SaveManager.Instance?.GetHighScore() ?? 0;
                _highScoreText.text = $"Best: {highScore:N0}";
            }

            // Update high score if needed
            int currentScore = GameManager.Instance?.Score ?? 0;
            Save.SaveManager.Instance?.SetHighScore(currentScore);
        }

        // === Button Handlers ===

        private void OnPauseClicked()
        {
            GameManager.Instance?.SetState(GameState.Paused);
        }

        private void OnResumeClicked()
        {
            HidePausePanel();
            GameManager.Instance?.SetState(GameState.Playing);
        }

        private void OnMainMenuClicked()
        {
            UIManager.Instance?.LoadMainMenu();
        }

        private void OnUndoClicked()
        {
            if (EconomyManager.Instance?.TryUseUndo() ?? false)
            {
                // TODO: Implement undo in game loop
                Debug.Log("Undo used");
            }
        }

        private void OnBombClicked()
        {
            if (EconomyManager.Instance?.TryUseBomb() ?? false)
            {
                // TODO: Implement bomb in game loop
                Debug.Log("Bomb used");
            }
        }

        private void OnContinueAdClicked()
        {
            Ads.AdManager.Instance?.ShowContinueAd(
                () =>
                {
                    // Continue game - TODO: implement continue logic
                    if (_gameOverPanel != null)
                        _gameOverPanel.SetActive(false);
                    GameManager.Instance?.SetState(GameState.Playing);
                },
                () =>
                {
                    Debug.Log("Continue ad failed or declined");
                }
            );
        }

        private void OnNewGameClicked()
        {
            // Show interstitial if appropriate
            Ads.AdManager.Instance?.OnGameComplete();
            Ads.AdManager.Instance?.ShowInterstitialIfReady(() =>
            {
                var gameLoop = FindObjectOfType<GameLoopController>();
                gameLoop?.StartNewGame();

                if (_gameOverPanel != null)
                    _gameOverPanel.SetActive(false);
            });
        }
    }
}
