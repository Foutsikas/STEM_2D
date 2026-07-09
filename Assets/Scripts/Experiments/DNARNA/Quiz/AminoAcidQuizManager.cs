using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace STEM.DNA_Quiz
{
    public class AminoAcidQuizManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject activityPanel;
        [SerializeField] private Image structureImage;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI formulaText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TextMeshProUGUI[] answerLabels;

        [Header("Scene References")]
        [SerializeField] private LessonPanelManager lessonPanel;

        [Header("Amino Acids")]
        [SerializeField] private AminoAcidEntry[] aminoAcids;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color correctColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color wrongColor = new Color(0.8f, 0.3f, 0.3f, 1f);

        private int currentIndex = 0;
        private int correctCount = 0;

        [System.Serializable]
        public class AminoAcidEntry
        {
            public string aminoAcidName;
            public string chemicalFormula;
            public Sprite structureSprite;
            [Tooltip("4 answer options. One must match aminoAcidName exactly.")]
            public string[] answerOptions = new string[4];
            [Range(0, 3)]
            public int correctAnswerIndex;
        }

        void Start()
        {
            activityPanel.SetActive(false);

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }

            lessonPanel.OnLessonComplete += OnLessonDone;
        }

        void OnDestroy()
        {
            if (lessonPanel != null)
                lessonPanel.OnLessonComplete -= OnLessonDone;
        }

        private void OnLessonDone()
        {
            activityPanel.SetActive(true);
            currentIndex = 0;
            correctCount = 0;
            ShowAminoAcid();
        }

        private void ShowAminoAcid()
        {
            if (currentIndex >= aminoAcids.Length)
            {
                OnAllComplete();
                return;
            }

            AminoAcidEntry entry = aminoAcids[currentIndex];

            structureImage.sprite = entry.structureSprite;
            structureImage.preserveAspect = true;
            progressText.text = (currentIndex + 1) + " / " + aminoAcids.Length;
            feedbackText.text = "";

            if (formulaText != null)
                formulaText.text = entry.chemicalFormula;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < entry.answerOptions.Length && !string.IsNullOrEmpty(entry.answerOptions[i]))
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerLabels[i].text = entry.answerOptions[i];
                    answerButtons[i].image.color = normalColor;
                    answerButtons[i].interactable = true;
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnAnswerSelected(int index)
        {
            AminoAcidEntry entry = aminoAcids[currentIndex];

            if (index == entry.correctAnswerIndex)
            {
                answerButtons[index].image.color = correctColor;
                feedbackText.text = "Correct!";
                feedbackText.color = correctColor;
                correctCount++;

                SetAllButtonsInteractable(false);
                StartCoroutine(NextAminoAcidDelay());
            }
            else
            {
                answerButtons[index].image.color = wrongColor;
                answerButtons[index].interactable = false;
                feedbackText.text = "Wrong. Try again.";
                feedbackText.color = wrongColor;
            }
        }

        private IEnumerator NextAminoAcidDelay()
        {
            yield return new WaitForSeconds(1.5f);
            currentIndex++;
            ShowAminoAcid();
        }

        private void OnAllComplete()
        {
            activityPanel.SetActive(false);

            if (QuizManager.Instance != null)
                QuizManager.Instance.StartQuiz();
        }

        private void SetAllButtonsInteractable(bool interactable)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i].gameObject.activeSelf)
                    answerButtons[i].interactable = interactable;
            }
        }
    }
}