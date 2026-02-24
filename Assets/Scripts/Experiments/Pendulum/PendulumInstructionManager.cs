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

        private int currentStep = 0;
        private int savedMeasurements = 0;
        private bool panelVisible = false;

        private readonly List<Step> steps = new List<Step>
        {
            new Step(
                "Βήμα 1\n\nΕπιλέξτε μήκος ράβδου 40 cm και βαρίδιο 50 g.\n" +
                "Τοποθετήστε τη φωτοπύλη RS108 στη θέση ισορροπίας του εκκρεμούς.",
                showSelection: true, showWaveform: false, showLT2: false, showSave: false
            ),
            new Step(
                "Βήμα 2\n\nΕπιλέξτε πλάτος ταλάντωσης 5 cm.\n" +
                "Πατήστε Εκκίνηση και παρατηρήστε το γράφημα μετατόπισης-χρόνου.",
                showSelection: true, showWaveform: true, showLT2: false, showSave: false
            ),
            new Step(
                "Βήμα 3\n\nΠαρατηρήστε τις τιμές Περίοδος και Συχνότητα στο DL120.\n" +
                "Αφήστε το εκκρεμές να εκτελέσει τουλάχιστον 10 ταλαντώσεις.",
                showSelection: true, showWaveform: true, showLT2: false, showSave: true
            ),
            new Step(
                "Βήμα 4\n\nΕπαναλάβετε με πλάτος 7.5 cm και 10 cm.\n" +
                "Παρατηρείτε αλλαγές στην περίοδο;",
                showSelection: true, showWaveform: true, showLT2: false, showSave: true
            ),
            new Step(
                "Βήμα 5\n\nΑλλάξτε βαρίδιο σε 100 g.\n" +
                "Επαναλάβετε τις μετρήσεις. Αλλάζει η περίοδος με τη μάζα;",
                showSelection: true, showWaveform: true, showLT2: false, showSave: true
            ),
            new Step(
                "Βήμα 6\n\nΑλλάξτε μήκος ράβδου σε 30 cm και μετά 20 cm.\n" +
                "Καταγράψτε την περίοδο για κάθε μήκος.",
                showSelection: true, showWaveform: true, showLT2: false, showSave: true
            ),
            new Step(
                "Βήμα 7\n\nΔείτε το γράφημα L - T².\n" +
                "Παρατηρείτε τη γραμμική σχέση;\n\nΤ = 2π√(L/g)",
                showSelection: true, showWaveform: true, showLT2: true, showSave: true
            )
        };

        private void Start()
        {
            nextButton?.onClick.AddListener(NextStep);
            togglePanelButton?.onClick.AddListener(TogglePanel);

            instructionPanel?.SetActive(false);
            ApplyStep(0);
        }

        private void TogglePanel()
        {
            panelVisible = !panelVisible;
            instructionPanel?.SetActive(panelVisible);
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
            Step step = steps[index];

            if (stepText != null) stepText.text = step.Text;
            if (stepCounterText != null) stepCounterText.text = $"{index + 1} / {steps.Count}";

            SetActive(selectionPanel, step.ShowSelection);
            SetActive(waveformGraphPanel, step.ShowWaveform);
            SetActive(lt2GraphPanel, step.ShowLT2);
            SetActive(saveButton, step.ShowSave);

            if (nextButton != null)
                nextButton.gameObject.SetActive(index < steps.Count - 1);
        }

        public void OnMeasurementSaved()
        {
            savedMeasurements++;
            if (savedMeasurements >= 3 && currentStep == 2)
                NextStep();
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        private class Step
        {
            public string Text;
            public bool ShowSelection;
            public bool ShowWaveform;
            public bool ShowLT2;
            public bool ShowSave;

            public Step(string text, bool showSelection, bool showWaveform, bool showLT2, bool showSave)
            {
                Text = text;
                ShowSelection = showSelection;
                ShowWaveform = showWaveform;
                ShowLT2 = showLT2;
                ShowSave = showSave;
            }
        }
    }
}
