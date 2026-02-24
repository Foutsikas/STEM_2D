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

        [Header("Playback")]
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
            SetSelectionsInteractable(false);
        }

        private void OnStop()
        {
            experimentController?.StopExperiment();
            SetSelectionsInteractable(true);
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
                $"Βαρίδιο: {selectedMassGrams:0} g   " +
                $"Πλάτος: {selectedAmplitude * 100:0.0} cm";
        }

        private void SetSelectionsInteractable(bool interactable)
        {
            if (length40Toggle != null) length40Toggle.interactable = interactable;
            if (length30Toggle != null) length30Toggle.interactable = interactable;
            if (length20Toggle != null) length20Toggle.interactable = interactable;
            if (mass50Toggle != null) mass50Toggle.interactable = interactable;
            if (mass100Toggle != null) mass100Toggle.interactable = interactable;
            if (amplitude5Toggle != null) amplitude5Toggle.interactable = interactable;
            if (amplitude75Toggle != null) amplitude75Toggle.interactable = interactable;
            if (amplitude10Toggle != null) amplitude10Toggle.interactable = interactable;
        }
    }
}
