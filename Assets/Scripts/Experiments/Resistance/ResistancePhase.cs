using System.Collections.Generic;
using UnityEngine;

namespace STEM.Experiments.Resistance
{
    [System.Serializable]
    public class CablePreset
    {
        public int cableIndex;

        public bool endAAttached = true;
        public NodeId endANode;
        public bool endALocked = true;

        public bool endBAttached = true;
        public NodeId endBNode;
        public bool endBLocked = true;
    }

    [CreateAssetMenu(fileName = "ResistancePhase", menuName = "STEM/Resistance/Phase")]
    public class ResistancePhase : ScriptableObject
    {
        [TextArea(3, 8)] public string instruction;
        [TextArea(4, 12)] public string resultText;

        public Topology requiredTopology;
        public VoltmeterTarget requiredVoltmeter;
        public bool switchStartsClosed;

        public List<CablePreset> presets = new List<CablePreset>();
    }
}
