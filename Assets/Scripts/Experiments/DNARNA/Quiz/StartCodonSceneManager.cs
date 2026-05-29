using UnityEngine;
using TMPro;

namespace STEM.DNA_Quiz
{
    public class StartCodonSceneManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private LessonPanelManager lessonPanel;
        [SerializeField] private InfoScreenManager methionineInfo;

        [Header("Part A: DNA (ATG)")]
        [SerializeField] private GameObject partAContainer;
        [SerializeField] private TextMeshProUGUI partATitle;

        [Header("Part B: mRNA (AUG)")]
        [SerializeField] private GameObject partBContainer;
        [SerializeField] private TextMeshProUGUI partBTitle;

        [Header("Methionine Info")]
        [SerializeField] private Sprite methionineSprite;
        [SerializeField] private string methionineTitle = "\u039c\u03b5\u03b8\u03b5\u03b9\u03bf\u03bd\u03af\u03bd\u03b7 (Met)";
        [TextArea(2, 4)]
        [SerializeField] private string methionineDescription =
            "\u03a7\u03b7\u03bc\u03b9\u03ba\u03cc\u03c2 \u03c4\u03cd\u03c0\u03bf\u03c2: C\u2085H\u2081\u2081NO\u2082S\n\n" +
            "\u0397 \u039c\u03b5\u03b8\u03b5\u03b9\u03bf\u03bd\u03af\u03bd\u03b7 \u03b5\u03af\u03bd\u03b1\u03b9 \u03c4\u03bf \u03b1\u03bc\u03b9\u03bd\u03bf\u03be\u03cd " +
            "\u03c0\u03bf\u03c5 \u03ba\u03c9\u03b4\u03b9\u03ba\u03bf\u03c0\u03bf\u03b9\u03b5\u03af\u03c4\u03b1\u03b9 " +
            "\u03b1\u03c0\u03cc \u03c4\u03bf \u03ba\u03c9\u03b4\u03b9\u03ba\u03cc\u03bd\u03b9\u03bf \u03ad\u03bd\u03b1\u03c1\u03be\u03b7\u03c2 ATG/AUG.";

        [Header("Tracking")]
        [SerializeField] private int partADropZones = 3;
        [SerializeField] private int partBDropZones = 3;

        private int partACorrect = 0;
        private int partBCorrect = 0;

        void Start()
        {
            partAContainer.SetActive(false);
            partBContainer.SetActive(false);

            lessonPanel.OnLessonComplete += OnLessonDone;
            methionineInfo.OnInfoDismissed += OnInfoDone;
        }

        void OnDestroy()
        {
            if (lessonPanel != null) lessonPanel.OnLessonComplete -= OnLessonDone;
            if (methionineInfo != null) methionineInfo.OnInfoDismissed -= OnInfoDone;
        }

        private void OnLessonDone()
        {
            partAContainer.SetActive(true);
        }

        public void OnPartACorrect()
        {
            partACorrect++;
            if (partACorrect >= partADropZones)
            {
                partAContainer.SetActive(false);
                partBContainer.SetActive(true);
            }
        }

        public void OnPartBCorrect()
        {
            partBCorrect++;
            if (partBCorrect >= partBDropZones)
            {
                partBContainer.SetActive(false);
                methionineInfo.Show(methionineTitle, methionineSprite, methionineDescription);
            }
        }

        private void OnInfoDone()
        {
            if (QuizManager.Instance != null)
                QuizManager.Instance.StartQuiz();
        }

        public void OnIncorrectPlacement()
        {
            // hook up feedback sticker if desired
        }
    }
}
