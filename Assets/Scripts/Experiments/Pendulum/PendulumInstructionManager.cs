using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace STEM.Experiments.Pendulum
{
    public class PendulumInstructionManager : MonoBehaviour
    {
        [Header("Instruction Panel UI")]
        public GameObject instructionPanel;
        public TextMeshProUGUI stepText;
        public TextMeshProUGUI stepCounterText;
        public Button nextButton;
        public Button togglePanelButton;

        [Header("Panels revealed per step")]
        public GameObject selectionPanel;
        public GameObject waveformGraphPanel;
        public GameObject lt2GraphPanel;
        public GameObject saveButton;
        [Header("Graph Reference")]
        public PendulumLT2Graph lt2Graph;

        [Header("Selection UI Reference")]
        public PendulumSelectionUI selectionUI;

        [Header("Photogate")]
        [SerializeField] private PhotogateRotator photogateRotator;

        private int currentStep = 0;
        private int savedMeasurements = 0;
        private bool panelVisible = false;

        [Header("Steps")]
        [SerializeField] private List<PendulumStep> steps = new List<PendulumStep>();

        private void Start()
        {
            if (nextButton != null) nextButton.onClick.AddListener(NextStep);
            if (togglePanelButton != null) togglePanelButton.onClick.AddListener(TogglePanel);

            SetActive(instructionPanel, false);
            ApplyStep(0);
        }

        private void TogglePanel()
        {
            panelVisible = !panelVisible;
            SetActive(instructionPanel, panelVisible);
        }

        private void NextStep()
        {
            if (currentStep < steps.Count - 1)
            {
                currentStep++;
                ApplyStep(currentStep);
            }
        }

        private void ApplyStep(int index)
        {
            PendulumStep step = steps[index];

            if (stepText != null) stepText.text = step.text;
            if (stepCounterText != null) stepCounterText.text = $"{index + 1} / {steps.Count}";

            SetActive(waveformGraphPanel, step.showWaveform);
            SetActive(lt2GraphPanel, step.showLT2);
            SetActive(saveButton, step.showSave);

            if (nextButton != null)
                nextButton.gameObject.SetActive(index < steps.Count - 1);

            if (step.showLT2 && lt2Graph != null)
                lt2Graph.ShowConnectingLine();

            if (step.rotatePhotogate && photogateRotator != null)
                photogateRotator.RotateToSideView();

            if (selectionUI != null)
                selectionUI.ApplyLocks(
                    step.lengthLocked,
                    step.massLocked,
                    step.amplitudeLocked,
                    step.playLocked,
                    step.stopLocked,
                    step.saveLocked
                );
        }

        public void OnMeasurementSaved()
        {
            savedMeasurements++;

            int needed = steps[currentStep].autoAdvanceAfterSaves;
            if (needed > 0 && savedMeasurements >= needed)
                NextStep();
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}