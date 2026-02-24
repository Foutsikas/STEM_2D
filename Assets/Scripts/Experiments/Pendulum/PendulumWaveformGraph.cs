using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace STEM.Experiments.Pendulum
{
    public class PendulumWaveformGraph : MonoBehaviour
    {
        [Header("References")]
        public PendulumApparatus apparatus;
        public LineRenderer waveformLine;
        public RectTransform graphArea;

        [Header("Axis Label References")]
        public TextMeshProUGUI xAxisLabel;
        public TextMeshProUGUI yAxisLabel;
        public TextMeshProUGUI titleLabel;

        [Header("Graph Settings")]
        [Tooltip("How many seconds of data the graph shows before scrolling.")]
        public float timeWindowSeconds = 10f;
        [Tooltip("Samples recorded per second.")]
        public float sampleRate = 60f;

        [Header("Axis Scale")]
        public float graphWidth = 8f;
        public float graphHeight = 3f;

        private List<Vector3> points = new List<Vector3>();
        private float sampleTimer;
        private float runningTime;
        private bool recording;

        private void Start()
        {
            SetupLabels();
            SetupLineRenderer();
        }

        private void SetupLabels()
        {
            if (titleLabel != null) titleLabel.text = "Μετατόπιση - Χρόνος";
            if (xAxisLabel != null) xAxisLabel.text = "t (s)";
            if (yAxisLabel != null) yAxisLabel.text = "x (cm)";
        }

        private void SetupLineRenderer()
        {
            if (waveformLine == null) return;
            waveformLine.positionCount = 0;
            waveformLine.startWidth = 0.03f;
            waveformLine.endWidth = 0.03f;
            waveformLine.useWorldSpace = true;
        }

        private void Update()
        {
            if (!recording || apparatus == null) return;

            sampleTimer += Time.deltaTime;
            runningTime += Time.deltaTime;

            if (sampleTimer >= 1f / sampleRate)
            {
                sampleTimer = 0f;
                RecordSample();
            }
        }

        private void RecordSample()
        {
            float displacement = apparatus.CurrentDisplacementMetres * 100f;

            float tNorm = Mathf.Clamp01(runningTime / timeWindowSeconds);
            float dNorm = Mathf.InverseLerp(-15f, 15f, displacement);

            Vector3 graphOrigin = transform.position;

            Vector3 point = new Vector3(
                graphOrigin.x + tNorm * graphWidth,
                graphOrigin.y + (dNorm - 0.5f) * graphHeight,
                graphOrigin.z
            );

            if (runningTime > timeWindowSeconds)
            {
                points.RemoveAt(0);
                ShiftPointsLeft();
            }

            points.Add(point);
            RefreshLine();
        }

        private void ShiftPointsLeft()
        {
            float shiftAmount = (graphWidth / (timeWindowSeconds * sampleRate));
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Vector3(
                    points[i].x - shiftAmount,
                    points[i].y,
                    points[i].z
                );
            }
        }

        private void RefreshLine()
        {
            if (waveformLine == null) return;
            waveformLine.positionCount = points.Count;
            waveformLine.SetPositions(points.ToArray());
        }

        public void StartRecording()
        {
            points.Clear();
            runningTime = 0f;
            sampleTimer = 0f;
            recording = true;
            if (waveformLine != null)
                waveformLine.positionCount = 0;
        }

        public void StopRecording()
        {
            recording = false;
        }

        public void ClearGraph()
        {
            points.Clear();
            runningTime = 0f;
            if (waveformLine != null)
                waveformLine.positionCount = 0;
        }
    }
}
