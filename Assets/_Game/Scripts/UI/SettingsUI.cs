using UnityEngine;
using UnityEngine.UI;
using BlockPuzzle.Save;
using BlockPuzzle.IAP;

namespace BlockPuzzle.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Toggles")]
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Toggle _hapticsToggle;

        [Header("Buttons")]
        [SerializeField] private Button _restorePurchasesButton;
        [SerializeField] private Button _privacyPolicyButton;
        [SerializeField] private Button _backButton;

        [Header("Privacy URL")]
        [SerializeField] private string _privacyPolicyUrl = "https://example.com/privacy";

        private void Start()
        {
            LoadSettings();
            SetupListeners();
        }

        private void LoadSettings()
        {
            var saveData = SaveManager.Instance?.CurrentData;
            if (saveData == null) return;

            if (_soundToggle != null)
                _soundToggle.isOn = saveData.SoundEnabled;

            if (_hapticsToggle != null)
                _hapticsToggle.isOn = saveData.HapticsEnabled;
        }

        private void SetupListeners()
        {
            if (_soundToggle != null)
                _soundToggle.onValueChanged.AddListener(OnSoundToggled);

            if (_hapticsToggle != null)
                _hapticsToggle.onValueChanged.AddListener(OnHapticsToggled);

            if (_restorePurchasesButton != null)
                _restorePurchasesButton.onClick.AddListener(OnRestorePurchasesClicked);

            if (_privacyPolicyButton != null)
                _privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnSoundToggled(bool isOn)
        {
            var saveData = SaveManager.Instance?.CurrentData;
            if (saveData != null)
            {
                saveData.SoundEnabled = isOn;
                SaveManager.Instance?.SaveGame();
            }

            // Apply setting
            AudioListener.volume = isOn ? 1f : 0f;
        }

        private void OnHapticsToggled(bool isOn)
        {
            var saveData = SaveManager.Instance?.CurrentData;
            if (saveData != null)
            {
                saveData.HapticsEnabled = isOn;
                SaveManager.Instance?.SaveGame();
            }

            // Haptics would be applied in input handling
        }

        private void OnRestorePurchasesClicked()
        {
            IAPManager.Instance?.RestorePurchases();
            // TODO: Show feedback to user
        }

        private void OnPrivacyPolicyClicked()
        {
            Application.OpenURL(_privacyPolicyUrl);
        }

        private void OnBackClicked()
        {
            UIManager.Instance?.LoadMainMenu();
        }
    }
}
