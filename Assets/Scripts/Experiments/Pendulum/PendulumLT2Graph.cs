using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace STEM.Experiments.Pendulum
{
    public class PendulumLT2Graph : MonoBehaviour
    {
        [Header("Graph Container")]
        public RectTransform graphRect;
        public GameObject dotPrefab;

        [Header("Axis Labels")]
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI xAxisLabel;
        public TextMeshProUGUI yAxisLabel;

        [Header("Axis Range")]
        public float maxLengthM = 0.45f;
        public float maxT2 = 2.0f;

        private readonly List<GameObject> dots = new List<GameObject>();

        private void Start()
        {
            if (titleLabel != null) titleLabel.text = "Γράφημα L - T²";
            if (xAxisLabel != null) xAxisLabel.text = "L (m)";
            if (yAxisLabel != null) yAxisLabel.text = "T² (s²)";
        }

        public void AddPoint(float lengthMetres, float period)
        {
            if (graphRect == null || dotPrefab == null) return;

            float T2 = period * period;
            float normX = Mathf.Clamp01(lengthMetres / maxLengthM);
            float normY = Mathf.Clamp01(T2 / maxT2);

            Vector2 anchoredPos = new Vector2(
                normX * graphRect.rect.width,
                normY * graphRect.rect.height
            );

            GameObject dot = Instantiate(dotPrefab, graphRect);
            dot.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            dots.Add(dot);
        }

        public void ClearGraph()
        {
            foreach (var dot in dots)
            {
                if (dot != null)
                    Destroy(dot);
            }
            dots.Clear();
        }
    }
}
