using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using BlockPuzzle.Core;

namespace BlockPuzzle.UI
{
    /// <summary>
    /// Simple Game UI - Score display + Back button.
    /// </summary>
    public class SimpleGameUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _newGameButton;

        [Header("Game References")]
        [SerializeField] private SimpleGameController _gameController;

        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
            UpdateScoreDisplay(0);
            UpdateComboDisplay(0);
        }

        private void SetupUI()
        {
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_newGameButton != null)
            {
                _newGameButton.onClick.AddListener(OnNewGameClicked);
            }
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
                GameManager.Instance.OnComboChanged += UpdateComboDisplay;
                GameManager.Instance.OnGameOver += OnGameOver;
            }
        }

        private void OnDestroy()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }

            if (_newGameButton != null)
            {
                _newGameButton.onClick.RemoveListener(OnNewGameClicked);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
                GameManager.Instance.OnComboChanged -= UpdateComboDisplay;
                GameManager.Instance.OnGameOver -= OnGameOver;
            }
        }

        private void UpdateScoreDisplay(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }
        }

        private void UpdateComboDisplay(int combo)
        {
            if (_comboText != null)
            {
                _comboText.text = combo > 0 ? $"Combo x{combo}" : "";
            }
        }

        private void OnGameOver()
        {
            Debug.Log("[GameUI] Game Over!");
            // Could show game over panel here
        }

        private void OnBackClicked()
        {
            Debug.Log("[GameUI] Back clicked, loading MainMenu");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnNewGameClicked()
        {
            Debug.Log("[GameUI] New Game clicked");
            if (_gameController != null)
            {
                _gameController.StartNewGame();
            }
        }
    }
}
