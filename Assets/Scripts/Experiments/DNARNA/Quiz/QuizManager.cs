using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace STEM.DNA_Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TextMeshProUGUI[] answerLabels;
        [SerializeField] private Button nextSceneButton;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color correctColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color wrongColor = new Color(0.8f, 0.3f, 0.3f, 1f);

        private QuizSet currentQuiz;
        private int currentQuestionIndex;
        private int correctCount;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            quizPanel.SetActive(false);
            nextSceneButton.gameObject.SetActive(false);

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }

            nextSceneButton.onClick.AddListener(OnNextScene);
        }

        public void StartQuiz()
        {
            if (GameProgressManager.Instance != null)
                currentQuiz = GameProgressManager.Instance.GetCurrentQuiz();

            if (currentQuiz == null || currentQuiz.questions.Length == 0)
            {
                OnQuizPassed();
                return;
            }

            currentQuestionIndex = 0;
            correctCount = 0;

            quizPanel.SetActive(true);
            nextSceneButton.gameObject.SetActive(false);

            if (titleText != null)
                titleText.text = currentQuiz.quizTitle;

            ShowQuestion();
        }

        private void ShowQuestion()
        {
            if (currentQuestionIndex >= currentQuiz.questions.Length)
            {
                EvaluateResults();
                return;
            }

            QuizQuestion q = currentQuiz.questions[currentQuestionIndex];

            questionText.text = q.questionText;
            progressText.text = (currentQuestionIndex + 1) + " / " + currentQuiz.questions.Length;
            feedbackText.text = "";

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < q.answers.Length && !string.IsNullOrEmpty(q.answers[i]))
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerLabels[i].text = q.answers[i];
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
            QuizQuestion q = currentQuiz.questions[currentQuestionIndex];

            if (index == q.correctAnswerIndex)
            {
                answerButtons[index].image.color = correctColor;
                feedbackText.text = "Correct!";
                feedbackText.color = correctColor;
                correctCount++;

                SetAllButtonsInteractable(false);
                StartCoroutine(NextQuestionDelay());
            }
            else
            {
                answerButtons[index].image.color = wrongColor;
                answerButtons[index].interactable = false;
                feedbackText.text = "Wrong. Try again.";
                feedbackText.color = wrongColor;
            }
        }

        private IEnumerator NextQuestionDelay()
        {
            yield return new WaitForSeconds(1.2f);
            currentQuestionIndex++;
            ShowQuestion();
        }

        private void EvaluateResults()
        {
            if (correctCount >= currentQuiz.requiredCorrect)
                OnQuizPassed();
            else
                OnQuizFailed();
        }

        private void OnQuizPassed()
        {
            questionText.text = "";
            feedbackText.text = "Quiz Passed!";
            feedbackText.color = correctColor;
            progressText.text = correctCount + " / " + currentQuiz.questions.Length;

            HideAnswerButtons();

            if (GameProgressManager.Instance != null)
                GameProgressManager.Instance.MarkCurrentCompleted();

            nextSceneButton.gameObject.SetActive(true);
        }

        private void OnQuizFailed()
        {
            questionText.text = "";
            feedbackText.text = "Quiz Failed. Retrying...";
            feedbackText.color = wrongColor;

            HideAnswerButtons();
            StartCoroutine(RetryQuizDelay());
        }

        private IEnumerator RetryQuizDelay()
        {
            yield return new WaitForSeconds(2f);
            currentQuestionIndex = 0;
            correctCount = 0;
            ShowQuestion();
        }

        private void OnNextScene()
        {
            if (GameProgressManager.Instance != null)
                GameProgressManager.Instance.LoadNextScene();
        }

        private void SetAllButtonsInteractable(bool interactable)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i].gameObject.activeSelf)
                    answerButtons[i].interactable = interactable;
            }
        }

        private void HideAnswerButtons()
        {
            for (int i = 0; i < answerButtons.Length; i++)
                answerButtons[i].gameObject.SetActive(false);
        }
    }
}
