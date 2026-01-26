using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockPuzzle.Core;
using BlockPuzzle.Economy;
using BlockPuzzle.Progression;
using BlockPuzzle.Save;

namespace BlockPuzzle.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _dailyChallengeButton;
        [SerializeField] private Button _settingsButton;

        [Header("Displays")]
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Slider _levelProgressSlider;

        [Header("Daily Reward")]
        [SerializeField] private Button _dailyRewardButton;
        [SerializeField] private GameObject _dailyRewardAvailableIndicator;

        private void Start()
        {
            SetupButtons();
            UpdateDisplays();

            // Subscribe to events
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.OnCoinsChanged += UpdateCoinsDisplay;

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnLevelUp += OnLevelUp;
                ProgressionManager.Instance.OnXPGained += OnXPGained;
            }
        }

        private void OnDestroy()
        {
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.OnCoinsChanged -= UpdateCoinsDisplay;

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnLevelUp -= OnLevelUp;
                ProgressionManager.Instance.OnXPGained -= OnXPGained;
            }
        }

        private void SetupButtons()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);

            if (_dailyChallengeButton != null)
                _dailyChallengeButton.onClick.AddListener(OnDailyChallengeClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_dailyRewardButton != null)
                _dailyRewardButton.onClick.AddListener(OnDailyRewardClicked);
        }

        private void UpdateDisplays()
        {
            UpdateCoinsDisplay(EconomyManager.Instance?.Coins ?? 0);
            UpdateLevelDisplay();
            UpdateDailyRewardIndicator();
        }

        private void UpdateCoinsDisplay(int coins)
        {
            if (_coinsText != null)
                _coinsText.text = coins.ToString("N0");
        }

        private void UpdateLevelDisplay()
        {
            var progression = ProgressionManager.Instance;
            if (progression == null) return;

            if (_levelText != null)
                _levelText.text = $"Level {progression.Level}";

            if (_levelProgressSlider != null)
                _levelProgressSlider.value = progression.LevelProgress;
        }

        private void UpdateDailyRewardIndicator()
        {
            bool canClaim = DailyRewardManager.Instance?.CanClaimToday ?? false;

            if (_dailyRewardAvailableIndicator != null)
                _dailyRewardAvailableIndicator.SetActive(canClaim);

            if (_dailyRewardButton != null)
                _dailyRewardButton.interactable = canClaim;
        }

        private void OnLevelUp(int newLevel)
        {
            UpdateLevelDisplay();
        }

        private void OnXPGained(int amount)
        {
            UpdateLevelDisplay();
        }

        // === Button Handlers ===

        private void OnPlayClicked()
        {
            UIManager.Instance?.LoadGame();
        }

        private void OnDailyChallengeClicked()
        {
            UIManager.Instance?.LoadDailyChallenge();
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance?.LoadSettings();
        }

        private void OnDailyRewardClicked()
        {
            if (DailyRewardManager.Instance?.TryClaimReward() ?? false)
            {
                UpdateDailyRewardIndicator();
                UpdateCoinsDisplay(EconomyManager.Instance?.Coins ?? 0);
                // TODO: Show reward popup
            }
        }
    }
}
