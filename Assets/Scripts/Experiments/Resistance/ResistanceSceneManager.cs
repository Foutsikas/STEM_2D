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
        public string quizSceneName = "ResistanceQuizScene";

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

                if (p.endAAttached) c.endA.AttachTo(cm.FindNode(p.endANode));
                if (p.endBAttached) c.endB.AttachTo(cm.FindNode(p.endBNode));

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

            statusText.text = ok ? "Το κύκλωμα είναι σωστό. Πάτησε Start (F6)." : FaultMessage(r, phase);

            dl120.SetStartEnabled(ok);
            if (!ok) dl120.Clear();
        }

        void HandleMeasured(float v, float i)
        {
            measured = true;

            ResistancePhase phase = phases[index];
            float r = ConnectionManager.Instance.Result.loadResistance;
            if (phase.requiredTopology == Topology.Series)
                r = ResistanceCircuit.R1;

            string body = phase.resultText
                .Replace("{V}", v.ToString("0.00"))
                .Replace("{I}", i.ToString("0.000"))
                .Replace("{R}", r.ToString("0.##"));

            resultText.text = body;
            resultPanel.SetActive(true);
            nextButton.gameObject.SetActive(true);
            statusText.text = "";
        }

        void NextPhase()
        {
            if (index + 1 < phases.Length) LoadPhase(index + 1);
            else SceneManager.LoadScene(quizSceneName);
        }

        string FaultMessage(CircuitResult r, ResistancePhase phase)
        {
            switch (r.fault)
            {
                case CircuitFault.SwitchOpen:
                    return "Ο διακόπτης είναι ανοιχτός. Κλείσε τον για να διαρρεύσει ρεύμα.";
                case CircuitFault.NoLoop:
                    return "Το κύκλωμα δεν είναι κλειστό.";
                case CircuitFault.AmmeterBypassed:
                    return "Το αμπερόμετρο παρακάμπτεται. Πρέπει να συνδεθεί σε σειρά.";
                case CircuitFault.ShortCircuit:
                    return "Βραχυκύκλωμα. Οι πόλοι της πηγής ενώνονται απευθείας.";
                case CircuitFault.VoltmeterNotPlaced:
                    return "Σύνδεσε το βολτόμετρο παράλληλα στα άκρα της αντίστασης.";
                default:
                    if (r.voltmeter != phase.requiredVoltmeter && phase.requiredVoltmeter != VoltmeterTarget.None)
                        return "Το βολτόμετρο μετρά σε λάθος αντίσταση.";
                    return "Η συνδεσμολογία δεν είναι η ζητούμενη. Έλεγξε τα καλώδια.";
            }
        }
    }
}
