using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace STEM.Experiments.Microscope
{
    public class MicroscopeQuizManager : MonoBehaviour
    {
        [SerializeField] List<MicroscopeQuizQuestion> questions = new List<MicroscopeQuizQuestion>();
        [SerializeField] int passScore = 7;
        [SerializeField] string nextSceneName = "";

        [Header("UI")]
        [SerializeField] TMP_Text progressText;
        [SerializeField] TMP_Text questionText;
        [SerializeField] Button[] answerButtons;
        [SerializeField] TMP_Text explanationText;
        [SerializeField] Button nextButton;
        [SerializeField] GameObject resultPanel;
        [SerializeField] TMP_Text resultText;
        [SerializeField] Button continueButton;
        [SerializeField] Button retryButton;

        [Header("Colors")]
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color wrongColor = new Color(0.9f, 0.35f, 0.35f);
        [SerializeField] Color rightColor = new Color(0.4f, 0.85f, 0.45f);

        [Header("Messages")]
        [SerializeField] string passFormat = "Μπράβο! Σκορ: {0} / {1}";
        [SerializeField] string failFormat = "Σκορ: {0} / {1}. Χρειάζεσαι {2} για να συνεχίσεις.";

        int index = -1;
        int score;
        bool firstTry;
        int correctButton;

        void Start()
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int b = i;
                answerButtons[i].onClick.AddListener(() => OnAnswer(b));
            }
            nextButton.onClick.AddListener(NextQuestion);
            continueButton.onClick.AddListener(() => SceneManager.LoadScene(nextSceneName));
            retryButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
            resultPanel.SetActive(false);
            NextQuestion();
        }

        void NextQuestion()
        {
            index++;
            if (index >= questions.Count)
            {
                ShowResult();
                return;
            }

            var q = questions[index];
            progressText.text = $"{index + 1} / {questions.Count}";
            questionText.text = q.question;
            explanationText.text = "";
            nextButton.gameObject.SetActive(false);
            firstTry = true;

            var order = new List<int>();
            for (int i = 0; i < q.answers.Length; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                var btn = answerButtons[i];
                bool used = i < order.Count;
                btn.gameObject.SetActive(used);
                if (!used) continue;

                btn.interactable = true;
                btn.image.color = normalColor;
                btn.GetComponentInChildren<TMP_Text>().text = q.answers[order[i]];
                if (order[i] == 0) correctButton = i;
            }
        }

        void OnAnswer(int button)
        {
            if (button == correctButton)
            {
                if (firstTry) score++;
                answerButtons[button].image.color = rightColor;
                foreach (var b in answerButtons) b.interactable = false;
                explanationText.text = questions[index].explanation;
                nextButton.gameObject.SetActive(true);
            }
            else
            {
                firstTry = false;
                answerButtons[button].image.color = wrongColor;
                answerButtons[button].interactable = false;
            }
        }

        void ShowResult()
        {
            bool passed = score >= passScore;
            resultPanel.SetActive(true);
            resultText.text = passed
                ? string.Format(passFormat, score, questions.Count)
                : string.Format(failFormat, score, questions.Count, passScore);
            continueButton.gameObject.SetActive(passed && !string.IsNullOrEmpty(nextSceneName));
            retryButton.gameObject.SetActive(!passed);
        }
    }
}
