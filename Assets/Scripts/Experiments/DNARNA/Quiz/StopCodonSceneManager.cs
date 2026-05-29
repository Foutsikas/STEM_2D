using UnityEngine;
using TMPro;

namespace STEM.DNA_Quiz
{
    public class StopCodonSceneManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private LessonPanelManager lessonPanel;

        [Header("Structures")]
        [SerializeField] private GameObject structuresContainer;
        [SerializeField] private TextMeshProUGUI structure1Label;
        [SerializeField] private TextMeshProUGUI structure2Label;
        [SerializeField] private TextMeshProUGUI structure3Label;

        [Header("Feedback")]
        [SerializeField] private GameObject invalidSticker;
        [SerializeField] private GameObject wellDoneSticker;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Settings")]
        [SerializeField] private int totalDropZones = 9;

        private int correctCount = 0;
        private bool activityComplete = false;

        void Start()
        {
            structuresContainer.SetActive(false);
            if (invalidSticker != null) invalidSticker.SetActive(false);
            if (wellDoneSticker != null) wellDoneSticker.SetActive(false);

            lessonPanel.OnLessonComplete += OnLessonDone;
        }

        void OnDestroy()
        {
            if (lessonPanel != null) lessonPanel.OnLessonComplete -= OnLessonDone;
        }

        private void OnLessonDone()
        {
            structuresContainer.SetActive(true);
            UpdateProgress();
        }

        public void OnCorrectPlacement()
        {
            if (activityComplete) return;

            correctCount++;
            UpdateProgress();

            if (correctCount >= totalDropZones)
            {
                activityComplete = true;
                if (wellDoneSticker != null)
                    wellDoneSticker.SetActive(true);
                Invoke(nameof(StartQuiz), 2f);
            }
        }

        public void OnIncorrectPlacement()
        {
            if (invalidSticker != null)
            {
                invalidSticker.SetActive(true);
                CancelInvoke(nameof(HideInvalidSticker));
                Invoke(nameof(HideInvalidSticker), 1.5f);
            }
        }

        private void HideInvalidSticker()
        {
            if (invalidSticker != null)
                invalidSticker.SetActive(false);
        }

        private void UpdateProgress()
        {
            if (progressText != null)
                progressText.text = correctCount + " / " + totalDropZones;
        }

        private void StartQuiz()
        {
            if (wellDoneSticker != null)
                wellDoneSticker.SetActive(false);

            if (QuizManager.Instance != null)
                QuizManager.Instance.StartQuiz();
        }
    }
}
