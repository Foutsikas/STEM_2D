using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace STEM.Experiments.Pendulum
{
    public class PendulumWaveformGraph : MonoBehaviour
    {
        [Header("References")]
        public PendulumExperimentController experimentController;
        public LineRenderer periodLine;
        public LineRenderer frequencyLine;
        public RectTransform graphArea;

        [Header("Axis Label References")]
        public TextMeshProUGUI xAxisLabel;
        public TextMeshProUGUI yAxisLabel;
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI periodLegendLabel;
        public TextMeshProUGUI frequencyLegendLabel;

        [Header("Graph Settings")]
        public float timeWindowSeconds = 20f;
        public float maxPeriod = 2.0f;
        public float maxFrequency = 2.0f;

        private List<Vector3> periodPoints = new List<Vector3>();
        private List<Vector3> frequencyPoints = new List<Vector3>();

        private float runningTime;
        private bool recording;

        private void Start()
        {
            SetupLabels();
            SetupLineRenderers();
        }

        private void SetupLabels()
        {
            if (titleLabel != null) titleLabel.text = "Περίοδος & Συχνότητα - Χρόνος";
            if (xAxisLabel != null) xAxisLabel.text = "t (s)";
            if (yAxisLabel != null) yAxisLabel.text = "T(s) / f(Hz)";
            if (periodLegendLabel != null) periodLegendLabel.text = "— Περίοδος";
            if (frequencyLegendLabel != null) frequencyLegendLabel.text = "— Συχνότητα";
        }

        private void SetupLineRenderers()
        {
            if (periodLine != null)
            {
                periodLine.startWidth = 0.025f;
                periodLine.endWidth = 0.025f;
                periodLine.startColor = new Color(0.3f, 0.5f, 1f);
                periodLine.endColor = new Color(0.3f, 0.5f, 1f);
            }

            if (frequencyLine != null)
            {
                frequencyLine.startWidth = 0.025f;
                frequencyLine.endWidth = 0.025f;
                frequencyLine.startColor = new Color(0.2f, 1f, 0.4f);
                frequencyLine.endColor = new Color(0.2f, 1f, 0.4f);
            }
        }

        private void Update()
        {
            if (!recording) return;
            runningTime += Time.deltaTime;
        }

        public void AddReading(float period, float frequency)
        {
            if (!recording) return;

            Vector3[] corners = new Vector3[4];
            graphArea.GetWorldCorners(corners);

            Vector3 originWorld = corners[0];
            Vector3 topRightWorld = corners[2];

            float worldWidth = topRightWorld.x - originWorld.x;
            float worldHeight = topRightWorld.y - originWorld.y;

            float tNorm = Mathf.Clamp01(runningTime / timeWindowSeconds);

            float periodNorm = Mathf.Clamp01(period / maxPeriod);
            float frequencyNorm = Mathf.Clamp01(frequency / maxFrequency);

            Vector3 periodPoint = new Vector3(
                originWorld.x + tNorm * worldWidth,
                originWorld.y + periodNorm * worldHeight,
                0f
            );

            Vector3 frequencyPoint = new Vector3(
                originWorld.x + tNorm * worldWidth,
                originWorld.y + frequencyNorm * worldHeight,
                0f
            );

            if (runningTime > timeWindowSeconds)
            {
                if (periodPoints.Count > 0) periodPoints.RemoveAt(0);
                if (frequencyPoints.Count > 0) frequencyPoints.RemoveAt(0);

                float shiftAmount = worldWidth / (timeWindowSeconds * 10f);
                ShiftLeft(periodPoints, shiftAmount);
                ShiftLeft(frequencyPoints, shiftAmount);
            }

            periodPoints.Add(periodPoint);
            frequencyPoints.Add(frequencyPoint);

            RefreshLines();
        }

        private void ShiftLeft(List<Vector3> points, float amount)
        {
            for (int i = 0; i < points.Count; i++)
                points[i] = new Vector3(points[i].x - amount, points[i].y, points[i].z);
        }

        private void RefreshLines()
        {
            if (periodLine != null)
            {
                periodLine.positionCount = periodPoints.Count;
                periodLine.SetPositions(periodPoints.ToArray());
            }

            if (frequencyLine != null)
            {
                frequencyLine.positionCount = frequencyPoints.Count;
                frequencyLine.SetPositions(frequencyPoints.ToArray());
            }
        }

        public void StartRecording()
        {
            periodPoints.Clear();
            frequencyPoints.Clear();
            runningTime = 0f;
            recording = true;

            if (periodLine != null) periodLine.positionCount = 0;
            if (frequencyLine != null) frequencyLine.positionCount = 0;
        }

        public void StopRecording()
        {
            recording = false;
        }

        public void ClearGraph()
        {
            periodPoints.Clear();
            frequencyPoints.Clear();
            runningTime = 0f;

            if (periodLine != null) periodLine.positionCount = 0;
            if (frequencyLine != null) frequencyLine.positionCount = 0;
        }
    }
}