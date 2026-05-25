using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace STEM.Experiments.DNA
{
    public class DNASceneManager : MonoBehaviour
    {
        public static DNASceneManager Instance { get; private set; }

        [Header("Feedback")]
        public GameObject invalidChoiceSticker;
        public GameObject wellDoneSticker;
        public float feedbackDuration = 2f;

        [Header("Progress")]
        public TextMeshProUGUI progressText;

        private int totalDropZones;
        private int filledDropZones;
        private Coroutine feedbackCoroutine;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            DNADropZone[] zones = FindObjectsByType<DNADropZone>(FindObjectsSortMode.None);
            totalDropZones = 0;
            filledDropZones = 0;

            foreach (var zone in zones)
            {
                if (!zone.IsOccupied)
                    totalDropZones++;
            }

            if (invalidChoiceSticker != null)
                invalidChoiceSticker.SetActive(false);
            if (wellDoneSticker != null)
                wellDoneSticker.SetActive(false);

            UpdateProgress();
        }

        public void OnCorrectPlacement()
        {
            filledDropZones++;
            UpdateProgress();

            if (filledDropZones >= totalDropZones)
            {
                ShowWellDone();
            }
        }

        public void OnIncorrectPlacement()
        {
            ShowInvalidChoice();
        }

        private void ShowInvalidChoice()
        {
            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            if (wellDoneSticker != null)
                wellDoneSticker.SetActive(false);

            if (invalidChoiceSticker != null)
            {
                invalidChoiceSticker.SetActive(true);
                feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay(invalidChoiceSticker));
            }
        }

        private void ShowWellDone()
        {
            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            if (invalidChoiceSticker != null)
                invalidChoiceSticker.SetActive(false);

            if (wellDoneSticker != null)
                wellDoneSticker.SetActive(true);
        }

        private IEnumerator HideFeedbackAfterDelay(GameObject sticker)
        {
            yield return new WaitForSeconds(feedbackDuration);
            sticker.SetActive(false);
            feedbackCoroutine = null;
        }

        private void UpdateProgress()
        {
            if (progressText != null)
                progressText.text = filledDropZones + " / " + totalDropZones;
        }
    }
}
