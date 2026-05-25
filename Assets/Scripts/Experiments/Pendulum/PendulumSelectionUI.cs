using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace STEM.Experiments.Pendulum
{
    public class PendulumSelectionUI : MonoBehaviour
    {
        [Header("Core Reference")]
        public PendulumExperimentController experimentController;

        [Header("Length Toggles")]
        public Toggle length40Toggle;
        public Toggle length30Toggle;
        public Toggle length20Toggle;

        [Header("Mass Toggles")]
        public Toggle mass50Toggle;
        public Toggle mass100Toggle;

        [Header("Amplitude Toggles")]
        public Toggle amplitude5Toggle;
        public Toggle amplitude75Toggle;
        public Toggle amplitude10Toggle;

        [Header("Buttons")]
        public Button playButton;
        public Button stopButton;
        public Button saveButton;

        [Header("Current Selection Display")]
        public TextMeshProUGUI selectionSummaryText;

        private float selectedLength = 0.4f;
        private float selectedMassGrams = 50f;
        private float selectedAmplitude = 0.05f;

        private void Start()
        {
            RegisterLengthToggles();
            RegisterMassToggles();
            RegisterAmplitudeToggles();

            playButton?.onClick.AddListener(OnPlay);
            stopButton?.onClick.AddListener(OnStop);
            saveButton?.onClick.AddListener(OnSave);

            UpdateSummary();
        }

        private void RegisterLengthToggles()
        {
            length40Toggle?.onValueChanged.AddListener(on => { if (on) SetLength(0.4f); });
            length30Toggle?.onValueChanged.AddListener(on => { if (on) SetLength(0.3f); });
            length20Toggle?.onValueChanged.AddListener(on => { if (on) SetLength(0.2f); });
        }

        private void RegisterMassToggles()
        {
            mass50Toggle?.onValueChanged.AddListener(on => { if (on) SetMass(50f); });
            mass100Toggle?.onValueChanged.AddListener(on => { if (on) SetMass(100f); });
        }

        private void RegisterAmplitudeToggles()
        {
            amplitude5Toggle?.onValueChanged.AddListener(on => { if (on) SetAmplitude(0.05f); });
            amplitude75Toggle?.onValueChanged.AddListener(on => { if (on) SetAmplitude(0.075f); });
            amplitude10Toggle?.onValueChanged.AddListener(on => { if (on) SetAmplitude(0.10f); });
        }

        private void SetLength(float metres)
        {
            selectedLength = metres;
            experimentController?.ApplyLength(metres);
            UpdateSummary();
        }

        private void SetMass(float grams)
        {
            selectedMassGrams = grams;
            UpdateSummary();
        }

        private void SetAmplitude(float metres)
        {
            selectedAmplitude = metres;
            experimentController?.ApplyAmplitude(metres);
            UpdateSummary();
        }

        private void OnPlay()
        {
            experimentController?.StartExperiment();
        }

        private void OnStop()
        {
            experimentController?.StopExperiment();
        }

        private void OnSave()
        {
            experimentController?.SaveCurrentMeasurement(selectedLength, selectedMassGrams);
        }

        private void UpdateSummary()
        {
            if (selectionSummaryText == null) return;
            selectionSummaryText.text =
                $"Μήκος: {selectedLength * 100:0} cm   " +
                $"Βαρίδιο: {selectedMassGrams:0} g\n" +
                $"Πλάτος: {selectedAmplitude * 100:0.0} cm";
        }

        public void ApplyLocks(
            bool lengthLocked,
            bool massLocked,
            bool amplitudeLocked,
            bool playLocked,
            bool stopLocked,
            bool saveLocked)
        {
            SetToggleGroupInteractable(lengthLocked, length40Toggle, length30Toggle, length20Toggle);
            SetToggleGroupInteractable(massLocked, mass50Toggle, mass100Toggle);
            SetToggleGroupInteractable(amplitudeLocked, amplitude5Toggle, amplitude75Toggle, amplitude10Toggle);

            if (playButton != null) playButton.interactable = !playLocked;
            if (stopButton != null) stopButton.interactable = !stopLocked;
            if (saveButton != null) saveButton.interactable = !saveLocked;
        }

        private void SetToggleGroupInteractable(bool locked, params Toggle[] toggles)
        {
            foreach (Toggle t in toggles)
                if (t != null) t.interactable = !locked;
        }
    }
}