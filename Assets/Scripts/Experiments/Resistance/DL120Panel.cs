using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STEM.Experiments.Resistance
{
    public class DL120Panel : MonoBehaviour
    {
        public TMP_Text voltageText;
        public TMP_Text currentText;
        public Button startButton;

        [Range(0f, 0.05f)] public float noise = 0.01f;

        public event Action<float, float> OnMeasured;

        void Start()
        {
            startButton.onClick.AddListener(Measure);
            Clear();
        }

        public void Clear()
        {
            voltageText.text = "---";
            currentText.text = "---";
        }

        public void SetStartEnabled(bool value)
        {
            startButton.interactable = value;
        }

        void Measure()
        {
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
