using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using BlockPuzzle.Core;

namespace BlockPuzzle.UI
{
    /// <summary>
    /// Simple MainMenu UI - Play button loads Game scene.
    /// </summary>
    public class SimpleMainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _playButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        private void Start()
        {
            // Ensure GameManager is in MainMenu state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.MainMenu);
            }

            SetupUI();
        }

        private void SetupUI()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_titleText != null)
            {
                _titleText.text = "Block Puzzle";
            }
        }

        private void OnPlayClicked()
        {
            Debug.Log("[MainMenu] Play clicked, loading Game scene");
            SceneManager.LoadScene("Game");
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayClicked);
            }
        }
    }
}
