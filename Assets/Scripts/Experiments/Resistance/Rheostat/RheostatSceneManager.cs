using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using STEM.Experiments.Resistance;

namespace STEM.Experiments.Rheostat
{
    public class RheostatSceneManager : MonoBehaviour
    {
        public RheostatPhase phase;
        public RheostatConnectionManager manager;
        public Knob knob;
        public Bulb bulb;

        public TMP_Text instructionText;
        public TMP_Text statusText;
        public TMP_Text voltageText;
        public TMP_Text currentText;

        public GameObject resultPanel;
        public TMP_Text resultText;
        public Button nextButton;

        public float sweepNeeded = 0.6f;
        public string nextSceneName;

        float minKnob;
        float maxKnob;
        bool done;

        void Start()
        {
            manager.OnRheostatChanged += HandleCircuit;
            nextButton.onClick.AddListener(Next);
            LoadPhase();
        }

        void OnDestroy()
        {
            if (manager != null) manager.OnRheostatChanged -= HandleCircuit;
        }

        void LoadPhase()
        {
            done = false;
            ResetSweep();

            instructionText.text = phase.instruction;
            statusText.text = "";
            resultPanel.SetActive(false);

            knob.value = Mathf.Clamp01(phase.startKnob);
            ClearReadout();
            ApplyPresets();

            manager.circuitSwitch.SetClosed(phase.switchStartsClosed);
            manager.Evaluate();
        }

        void ApplyPresets()
        {
            foreach (Cable c in manager.cables)
            {
                c.endA.Detach();
                c.endB.Detach();
                c.endA.locked = true;
                c.endB.locked = true;
                c.gameObject.SetActive(false);
            }

            foreach (CablePreset p in phase.presets)
            {
                Cable c = manager.cables[p.cableIndex];
                c.gameObject.SetActive(true);

                if (p.endAAttached) c.endA.AttachTo(RequireNode(p.endANode, p.cableIndex, "A"));
                if (p.endBAttached) c.endB.AttachTo(RequireNode(p.endBNode, p.cableIndex, "B"));

                c.endA.locked = p.endALocked;
                c.endB.locked = p.endBLocked;
            }
        }

        CircuitNode RequireNode(NodeId id, int cableIndex, string end)
        {
            CircuitNode node = manager.FindNode(id);
            if (node == null)
                Debug.LogError($"Rheostat preset: no node {id} for cable {cableIndex} end {end}", this);
            return node;
        }

        void HandleCircuit(RheostatResult r)
        {
            if (done) return;

            if (!r.IsValid)
            {
                statusText.text = FaultMessage(r.fault);
                ClearReadout();
                if (bulb != null) bulb.SetBrightness(0f);
                ResetSweep();
                return;
            }

            voltageText.text = r.lampVoltage.ToString("0.00");
            currentText.text = r.current.ToString("0.000");
            if (bulb != null) bulb.SetBrightness(r.brightness);

            minKnob = Mathf.Min(minKnob, knob.value);
            maxKnob = Mathf.Max(maxKnob, knob.value);

            if (maxKnob - minKnob >= sweepNeeded) Finish(r);
            else statusText.text = "Circuit is correct. Turn the knob and watch the lamp.";
        }

        void Finish(RheostatResult r)
        {
            done = true;
            statusText.text = "";

            resultText.text = phase.resultText
                .Replace("{V}", r.lampVoltage.ToString("0.00"))
                .Replace("{I}", r.current.ToString("0.000"))
                .Replace("{R}", r.resistance.ToString("0.#"));

            resultPanel.SetActive(true);
        }

        void Next()
        {
            if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
        }

        void ResetSweep()
        {
            minKnob = 1f;
            maxKnob = 0f;
        }

        void ClearReadout()
        {
            if (voltageText != null) voltageText.text = "---";
            if (currentText != null) currentText.text = "---";
        }

        string FaultMessage(RheostatFault f)
        {
            switch (f)
            {
                case RheostatFault.SwitchOpen:
                    return "The switch is open. Close it to let current flow.";
                case RheostatFault.ShortCircuit:
                    return "Short circuit. The source terminals are connected directly.";
                case RheostatFault.PotMissing:
                    return "The rheostat is not in the circuit. Use sockets 5 and 6.";
                case RheostatFault.LampMissing:
                    return "The lamp is not in the circuit. Use sockets 7 and 8.";
                case RheostatFault.PotBypassed:
                    return "The rheostat is bypassed. Its two terminals are joined.";
                case RheostatFault.LampBypassed:
                    return "The lamp is bypassed. Its two terminals are joined.";
                case RheostatFault.VoltmeterNotPlaced:
                    return "Connect the voltmeter in parallel across the lamp.";
                default:
                    return "The circuit is not closed.";
            }
        }
    }
}