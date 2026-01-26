using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlockPuzzle.UI
{
    /// <summary>
    /// Minimal tutorial - 3 steps max, hands-on, first session only.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        public event Action OnTutorialComplete;

        [Header("Tutorial Steps")]
        [SerializeField] private GameObject[] _tutorialSteps;
        [SerializeField] private GameObject _highlightOverlay;
        [SerializeField] private RectTransform _highlightTarget;

        private int _currentStep = 0;
        private bool _tutorialComplete = false;
        private const string TUTORIAL_COMPLETE_KEY = "tutorial_complete";

        public bool IsTutorialComplete => _tutorialComplete;
        public bool IsShowingTutorial => _currentStep > 0 && _currentStep <= _tutorialSteps.Length;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _tutorialComplete = PlayerPrefs.GetInt(TUTORIAL_COMPLETE_KEY, 0) == 1;
        }

        private void Start()
        {
            HideAllSteps();
        }

        public bool ShouldShowTutorial()
        {
            return !_tutorialComplete;
        }

        public void StartTutorial()
        {
            if (_tutorialComplete) return;

            _currentStep = 0;
            ShowNextStep();
        }

        public void ShowNextStep()
        {
            // Hide current step
            if (_currentStep > 0 && _currentStep <= _tutorialSteps.Length)
            {
                _tutorialSteps[_currentStep - 1].SetActive(false);
            }

            _currentStep++;

            // Show next step or complete
            if (_currentStep <= _tutorialSteps.Length)
            {
                _tutorialSteps[_currentStep - 1].SetActive(true);
            }
            else
            {
                CompleteTutorial();
            }
        }

        public void SkipTutorial()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            HideAllSteps();
            _tutorialComplete = true;
            PlayerPrefs.SetInt(TUTORIAL_COMPLETE_KEY, 1);
            PlayerPrefs.Save();
            OnTutorialComplete?.Invoke();
        }

        private void HideAllSteps()
        {
            if (_tutorialSteps == null) return;

            foreach (var step in _tutorialSteps)
            {
                if (step != null) step.SetActive(false);
            }

            if (_highlightOverlay != null)
                _highlightOverlay.SetActive(false);
        }

        public void HighlightElement(RectTransform target)
        {
            if (_highlightOverlay == null || _highlightTarget == null) return;

            _highlightOverlay.SetActive(true);
            _highlightTarget.position = target.position;
            _highlightTarget.sizeDelta = target.sizeDelta * 1.2f;
        }

        public void ClearHighlight()
        {
            if (_highlightOverlay != null)
                _highlightOverlay.SetActive(false);
        }

        // Called when user successfully places first piece
        public void OnFirstPiecePlaced()
        {
            if (_currentStep == 1)
            {
                ShowNextStep();
            }
        }

        // Called when user clears first line
        public void OnFirstClear()
        {
            if (_currentStep == 2)
            {
                ShowNextStep();
            }
        }

        // Called when user understands game over
        public void OnGameOverUnderstood()
        {
            if (_currentStep == 3)
            {
                ShowNextStep();
            }
        }

        // Debug: Reset tutorial
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(TUTORIAL_COMPLETE_KEY);
            _tutorialComplete = false;
            _currentStep = 0;
        }
    }
}
