using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace STEM.DNA_Quiz
{
    public class LessonPanelManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject lessonPanel;
        [SerializeField] private TextMeshProUGUI slideText;
        [SerializeField] private TextMeshProUGUI slideCounter;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI nextButtonLabel;

        [Header("Slides")]
        [TextArea(3, 6)]
        [SerializeField] private string[] slides;

        [Header("Settings")]
        [SerializeField] private string nextLabel = "Επόμενο";
        [SerializeField] private string startLabel = "Ξεκινήστε";

        private int currentSlide = 0;

        public event Action OnLessonComplete;

        void Awake()
        {
            nextButton.onClick.AddListener(OnNextClicked);
        }

        void Start()
        {
            if (slides.Length > 0)
            {
                lessonPanel.SetActive(true);
                ShowSlide(0);
            }
            else
            {
                lessonPanel.SetActive(false);
                OnLessonComplete?.Invoke();
            }
        }

        private void ShowSlide(int index)
        {
            currentSlide = index;
            slideText.text = slides[index];
            slideCounter.text = (index + 1) + " / " + slides.Length;

            bool isLast = index >= slides.Length - 1;
            nextButtonLabel.text = isLast ? startLabel : nextLabel;
        }

        private void OnNextClicked()
        {
            if (currentSlide < slides.Length - 1)
            {
                ShowSlide(currentSlide + 1);
            }
            else
            {
                lessonPanel.SetActive(false);
                OnLessonComplete?.Invoke();
            }
        }
    }
}
