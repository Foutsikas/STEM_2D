using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace STEM.Experiments.Resistance
{
    // World-space DL120 device. Readouts sit in a World Space Canvas over the LCD.
    // A reading is taken by clicking the OK button (or pressing F6).
    public class DL120Panel : MonoBehaviour
    {
        public TMP_Text voltageText;
        public TMP_Text currentText;
        public SpriteRenderer okGlow;   // pulses when a reading is available

        [Range(0f, 0.05f)] public float noise = 0.01f;

        public event Action<float, float> OnMeasured;

        bool canMeasure;

        void Start()
        {
            Clear();
        }

        void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.f6Key.wasPressedThisFrame) TryMeasure();

            if (okGlow != null && okGlow.enabled)
            {
                float a = 0.4f + 0.3f * Mathf.Abs(Mathf.Sin(Time.time * 3f));
                Color c = okGlow.color;
                c.a = a;
                okGlow.color = c;
            }
        }

        public void Clear()
        {
            voltageText.text = "---";
            currentText.text = "---";
        }

        public void SetStartEnabled(bool value)
        {
            canMeasure = value;
            if (okGlow != null) okGlow.enabled = value;
        }

        public void TryMeasure()
        {
            if (!canMeasure) return;

            CircuitResult r = ConnectionManager.Instance.Result;
            if (!r.IsValid) return;

            float v = r.voltage * (1f + UnityEngine.Random.Range(-noise, noise));
            float i = r.current * (1f + UnityEngine.Random.Range(-noise, noise));

            voltageText.text = v.ToString("0.00");
            currentText.text = i.ToString("0.000");

            SetStartEnabled(false);
            OnMeasured?.Invoke(v, i);
        }
    }
}