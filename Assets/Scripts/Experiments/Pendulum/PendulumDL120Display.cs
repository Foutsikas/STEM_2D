using UnityEngine;
using TMPro;

namespace STEM.Experiments.Pendulum
{
    public class PendulumDL120Display : MonoBehaviour
    {
        [Header("DL120 Screen Text References")]
        public TextMeshProUGUI modeText;
        [SerializeField] GameObject screenOff;
        public TextMeshProUGUI periodValueText;
        public TextMeshProUGUI frequencyValueText;
        public TextMeshProUGUI timeValueText;

        [Header("Labels (static)")]
        public TextMeshProUGUI periodLabelText;
        public TextMeshProUGUI frequencyLabelText;
        public TextMeshProUGUI timeLabelText;

        private float elapsedTime;
        private bool running;

        private void Start()
        {
            if (screenOff != null)
                screenOff.SetActive(false);

            modeText.text = "Ανεξάρτητη Μέτρηση";
            SetStaticLabels();
            ResetDisplay();
        }

        private void Update()
        {
            if (running)
            {
                elapsedTime += Time.deltaTime;
                timeValueText.text = elapsedTime.ToString("F2");
            }
        }

        private void SetStaticLabels()
        {
            if (modeText != null)
                modeText.text = "Ανεξάρτητη Μέτρηση";

            if (periodLabelText != null)
                periodLabelText.text = "Περίοδος (s)";

            if (frequencyLabelText != null)
                frequencyLabelText.text = "Συχνότητα (Hz)";

            if (timeLabelText != null)
                timeLabelText.text = "Χρόνος (s)";
        }

        public void StartMeasurement()
        {
            elapsedTime = 0f;
            running = true;
        }

        public void StopMeasurement()
        {
            running = false;
        }

        public void UpdateReadings(float period, float frequency)
        {
            if (periodValueText != null)
                periodValueText.text = period.ToString("F3");

            if (frequencyValueText != null)
                frequencyValueText.text = frequency.ToString("F3");
        }

        private void UpdateTimeDisplay()
        {
            if (timeValueText != null)
                timeValueText.text = elapsedTime.ToString("F2");
        }

        public void ResetDisplay()
        {
            elapsedTime = 0f;
            running = false;

            if (periodValueText != null) periodValueText.text = "-.---";
            if (frequencyValueText != null) frequencyValueText.text = "-.---";
            if (timeValueText != null) timeValueText.text = "0.00";
        }
    }
}
