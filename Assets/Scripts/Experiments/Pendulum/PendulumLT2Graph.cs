using UnityEngine;
using System.Collections.Generic;
using TMPro;

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

        [Header("Connecting Line")]
        public LineRenderer connectingLine;

        private readonly List<GameObject> dots = new List<GameObject>();
        private readonly List<RectTransform> placedRects = new List<RectTransform>();
        private readonly List<Vector2> pendingPoints = new List<Vector2>();

        private void Start()
        {
            if (titleLabel != null) titleLabel.text = "Γράφημα L - T²";
            if (xAxisLabel != null) xAxisLabel.text = "L (m)";
            if (yAxisLabel != null) yAxisLabel.text = "T² (s²)";
        }

        private void Update()
        {
            if (pendingPoints.Count > 0)
                TryPlacePendingPoints();
        }

        public void AddPoint(float lengthMetres, float period)
        {
            if (graphRect == null || dotPrefab == null) return;

            float T2 = period * period;
            float normX = Mathf.Clamp01(lengthMetres / maxLengthM);
            float normY = Mathf.Clamp01(T2 / maxT2);

            pendingPoints.Add(new Vector2(normX, normY));
        }

        private void TryPlacePendingPoints()
        {
            if (graphRect.rect.width <= 0) return;

            foreach (Vector2 p in pendingPoints)
            {
                GameObject dot = Instantiate(dotPrefab, graphRect);
                RectTransform rt = dot.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(
                    (p.x - 0.5f) * graphRect.rect.width,
                    (p.y - 0.5f) * graphRect.rect.height
                );
                dots.Add(dot);
                placedRects.Add(rt);
            }

            pendingPoints.Clear();
        }

        private void UpdateConnectingLine()
        {
            if (connectingLine == null || placedRects.Count < 2) return;

            List<Vector3> worldPositions = new List<Vector3>();
            foreach (RectTransform rt in placedRects)
                worldPositions.Add(rt.position);

            connectingLine.positionCount = worldPositions.Count;
            connectingLine.SetPositions(worldPositions.ToArray());
        }

        public void ShowConnectingLine()
        {
            UpdateConnectingLine();
        }

        public void ClearGraph()
        {
            foreach (var dot in dots)
                if (dot != null) Destroy(dot);

            dots.Clear();
            placedRects.Clear();
            pendingPoints.Clear();

            if (connectingLine != null)
                connectingLine.positionCount = 0;
        }
    }
}