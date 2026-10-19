using System.Collections.Generic;
using UnityEngine;
using STEM.Experiments.Resistance;

namespace STEM.Experiments.Rheostat
{
    [CreateAssetMenu(fileName = "RheostatPhase", menuName = "STEM/Rheostat/Phase")]
    public class RheostatPhase : ScriptableObject
    {
        [TextArea(3, 8)] public string instruction;
        [TextArea(4, 12)] public string resultText;

        public bool switchStartsClosed = true;
        public float startKnob;

        public List<CablePreset> presets = new List<CablePreset>();
    }
}