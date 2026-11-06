using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace STEM.Experiments.Resistance
{
    public class ResistanceSceneManager : MonoBehaviour
    {
        public static ResistanceSceneManager Instance { get; private set; }

        public ResistancePhase[] phases;

        public TMP_Text instructionText;
        public TMP_Text statusText;

        public GameObject resultPanel;
        public TMP_Text resultText;
        public Button nextButton;

        public DL120Panel dl120;
        public string nextSceneName;

        int index;
        bool measured;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            ConnectionManager.Instance.OnCircuitChanged += HandleCircuit;
            dl120.OnMeasured += HandleMeasured;
            nextButton.onClick.AddListener(NextPhase);
            LoadPhase(0);
        }

        void OnDestroy()
        {
            if (ConnectionManager.Instance != null)
                ConnectionManager.Instance.OnCircuitChanged -= HandleCircuit;
            if (dl120 != null)
                dl120.OnMeasured -= HandleMeasured;
        }

        void LoadPhase(int i)
        {
            index = i;
            measured = false;

            ResistancePhase phase = phases[i];
            instructionText.text = phase.instruction;

            resultPanel.SetActive(false);
            nextButton.gameObject.SetActive(false);

            dl120.Clear();
            dl120.SetStartEnabled(false);

            ApplyPresets(phase);
            ConnectionManager.Instance.circuitSwitch.SetClosed(phase.switchStartsClosed);
            ConnectionManager.Instance.Evaluate();
        }

        void ApplyPresets(ResistancePhase phase)
        {
            ConnectionManager cm = ConnectionManager.Instance;

            foreach (Cable c in cm.cables)
            {
                c.endA.Detach();
                c.endB.Detach();
                c.endA.locked = true;
                c.endB.locked = true;
                c.gameObject.SetActive(false);
            }

            foreach (CablePreset p in phase.presets)
            {
                Cable c = cm.cables[p.cableIndex];
                c.gameObject.SetActive(true);

                if (p.endAAttached)
                {
                    CircuitNode node = cm.FindNode(p.endANode);
                    if (node == null)
                        Debug.LogError($"Phase preset: no node {p.endANode} for cable {p.cableIndex} end A", this);
                    c.endA.AttachTo(node);
                }

                if (p.endBAttached)
                {
                    CircuitNode node = cm.FindNode(p.endBNode);
                    if (node == null)
                        Debug.LogError($"Phase preset: no node {p.endBNode} for cable {p.cableIndex} end B", this);
                    c.endB.AttachTo(node);
                }

                c.endA.locked = p.endALocked;
                c.endB.locked = p.endBLocked;
            }
        }

        void HandleCircuit(CircuitResult r)
        {
            if (measured) return;

            ResistancePhase phase = phases[index];
            bool ok = r.IsValid
                      && r.topology == phase.requiredTopology
                      && (phase.requiredVoltmeter == VoltmeterTarget.None || r.voltmeter == phase.requiredVoltmeter);

            statusText.text = ok ? "Το κύκλωμα είναι σωστό. Πατήστε OK στο DL120." : FaultMessage(r, phase);

            dl120.SetStartEnabled(ok);
            if (!ok) dl120.Clear();
        }

        void HandleMeasured(float v, float i)
        {
            if (measured) return;
            measured = true;
            dl120.SetStartEnabled(false);

            ResistancePhase phase = phases[index];
            bool series = phase.requiredTopology == Topology.Series;

            // Clean nominal values for the calculation lines (like the pptx worksheet).
            float loadR = ConnectionManager.Instance.Result.loadResistance;
            float inom = ResistanceCircuit.SourceVoltage / loadR;
            float vnom = series ? inom * ResistanceCircuit.R1 : ResistanceCircuit.SourceVoltage;
            float rShown = series ? ResistanceCircuit.R1 : loadR;

            string body = phase.resultText
                .Replace("{V}", v.ToString("0.00"))
                .Replace("{I}", i.ToString("0.000"))
                .Replace("{Vn}", vnom.ToString("0.##"))
                .Replace("{In}", inom.ToString("0.####"))
                .Replace("{R}", rShown.ToString("0.##"));

            resultText.text = body;
            resultPanel.SetActive(true);
            nextButton.gameObject.SetActive(true);
            statusText.text = "";
        }

        void NextPhase()
        {
            if (index + 1 < phases.Length) LoadPhase(index + 1);
            else SceneManager.LoadScene(nextSceneName);
        }

        string FaultMessage(CircuitResult r, ResistancePhase phase)
        {
            switch (r.fault)
            {
                case CircuitFault.SwitchOpen:
                    return "Ο διακόπτης είναι ανοιχτός. Κλείστε τον για να περάσει ρεύμα.";
                case CircuitFault.NoLoop:
                    return "Το κύκλωμα δεν είναι κλειστό.";
                case CircuitFault.AmmeterBypassed:
                    return "Το αμπερόμετρο παρακάμπτεται. Πρέπει να συνδεθεί σε σειρά.";
                case CircuitFault.ShortCircuit:
                    return "Βραχυκύκλωμα. Οι πόλοι της πηγής συνδέονται απευθείας.";
                case CircuitFault.VoltmeterNotPlaced:
                    return "Συνδέστε το βολτόμετρο παράλληλα στα άκρα της αντίστασης.";
                case CircuitFault.JunctionAtSource:
                    return "Οι αντιστάσεις ενώνονται σε ακροδέκτη που είναι ήδη συνδεδεμένος με την πηγή. Χρησιμοποιήστε τον ελεύθερο ακροδέκτη.";
                case CircuitFault.ResistorNotBridged:
                    if (phase.requiredTopology == Topology.Parallel)
                        return "Η μία αντίσταση είναι συνδεδεμένη στους πόλους της πηγής. Συνδέστε την άλλη στα ίδια δύο σημεία.";
                    if (phase.requiredTopology == Topology.Series)
                        return "Και οι δύο αντιστάσεις πρέπει να βρίσκονται στη διαδρομή του ρεύματος, η μία μετά την άλλη.";
                    return "Η συνδεσμολογία δεν είναι σωστή. Ελέγξτε τα καλώδια.";
                default:
                    if (r.voltmeter != phase.requiredVoltmeter && phase.requiredVoltmeter != VoltmeterTarget.None)
                        return "Το βολτόμετρο μετρά στα άκρα λάθος αντίστασης.";
                    return "Η συνδεσμολογία δεν είναι σωστή. Ελέγξτε τα καλώδια.";
            }
        }
    }
}