using UnityEngine;

namespace STEM.Experiments.Rheostat
{
    [CreateAssetMenu(fileName = "RheostatSettings", menuName = "STEM/Rheostat/Settings")]
    public class RheostatSettings : ScriptableObject
    {
        public float sourceVoltage = 5f;
        public float potMax = 470f;
        public float lampResistance = 12.4f;
        public float knobCurve = 4f;
        public float glowCurve = 1f;

        public float Resistance(float knob)
        {
            return potMax * Mathf.Pow(Mathf.Clamp01(knob), knobCurve);
        }

        public float Current(float knob)
        {
            return sourceVoltage / (lampResistance + Resistance(knob));
        }

        public float LampVoltage(float knob)
        {
            return Current(knob) * lampResistance;
        }

        // Display only. The voltmeter reads LampVoltage.
        public float Brightness(float knob)
        {
            return Mathf.Pow(LampVoltage(knob) / sourceVoltage, glowCurve);
        }
    }
}