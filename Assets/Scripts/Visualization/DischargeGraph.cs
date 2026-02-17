using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class DischargeGraph : MonoBehaviour
    {
        [Header("Plot Area Definition")]
        [Tooltip("Bottom-left corner of the plot area")]
        [SerializeField] private Transform plotBottomLeft;
        [Tooltip("Top-right corner of the plot area")]
        [SerializeField] private Transform plotTopRight;

        [Header("Data Range")]
        [SerializeField] private float maxTime = 40f;
        [SerializeField] private float maxVoltage = 6f;

        [Header("Discharge Simulation")]
        [SerializeField] private float timeConstant = 5f;
        [SerializeField] private float simulationSpeed = 1f;
        [SerializeField] private float sampleInterval = 0.1f;

        [Header("Line Appearance")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth = 0.03f;
        [SerializeField] private Color lineColor = Color.blue;

        [Header("Action Registration")]
        [SerializeField] private string actionIdOnComplete = "graph_complete";

        [Header("Events")]
        public UnityEvent OnRecordingStarted;
        public UnityEvent OnRecordingStopped;
        public UnityEvent<float, float> OnDataPointAdded;

        private List<Vector3> points = new List<Vector3>();
        private bool isRecording = false;
        private float currentTime = 0f;
        private float initialVoltage = 0f;
        private float timeSinceLastSample = 0f;

        private Vector3 plotOrigin;
        private float plotWidth;
        private float plotHeight;

        public bool IsRecording => isRecording;
        public float CurrentVoltage { get; private set; }
        public int DataPointCount => points.Count;

        void Awake()
        {
            SetupLineRenderer();
            CalculatePlotArea();
        }

        void SetupLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 0;

            if (lineRenderer.material == null)
            {
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
            lineRenderer.material.color = lineColor;
        }

        void CalculatePlotArea()
        {
            if (plotBottomLeft == null || plotTopRight == null)
            {
                Debug.LogError("[Graph] Assign plotBottomLeft and plotTopRight!");
                return;
            }

            plotOrigin = plotBottomLeft.position;
            plotWidth = plotTopRight.position.x - plotBottomLeft.position.x;
            plotHeight = plotTopRight.position.y - plotBottomLeft.position.y;

            Debug.Log($"[Graph] Plot: origin={plotOrigin}, size={plotWidth}x{plotHeight}");
        }

        void Update()
        {
            if (isRecording)
            {
                RunSimulation();
            }
        }

        void RunSimulation()
        {
            float dt = Time.deltaTime * simulationSpeed;
            currentTime += dt;
            timeSinceLastSample += dt;

            CurrentVoltage = initialVoltage * Mathf.Exp(-currentTime / timeConstant);

            if (timeSinceLastSample >= sampleInterval)
            {
                AddPoint(currentTime, CurrentVoltage);
                timeSinceLastSample = 0f;
            }

            if (currentTime >= maxTime)
            {
                AddPoint(maxTime, CurrentVoltage);
                StopRecording();
            }
        }

        void AddPoint(float time, float voltage)
        {
            float x = plotOrigin.x + (time / maxTime) * plotWidth;
            float y = plotOrigin.y + (voltage / maxVoltage) * plotHeight;

            Vector3 worldPos = new Vector3(x, y, plotOrigin.z);
            points.Add(worldPos);

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());

            OnDataPointAdded?.Invoke(time, voltage);
        }

        public void StartRecording(float startVoltage)
        {
            if (isRecording) return;

            CalculatePlotArea();

            initialVoltage = startVoltage;
            CurrentVoltage = startVoltage;
            currentTime = 0f;
            timeSinceLastSample = 0f;

            points.Clear();
            lineRenderer.positionCount = 0;

            AddPoint(0f, initialVoltage);

            isRecording = true;
            OnRecordingStarted?.Invoke();

            Debug.Log($"[Graph] Started at {initialVoltage}V");
        }

        public void StopRecording()
        {
            if (!isRecording) return;

            isRecording = false;
            OnRecordingStopped?.Invoke();

            if (!string.IsNullOrEmpty(actionIdOnComplete) && ExperimentManager.Instance != null)
            {
                ExperimentManager.Instance.RegisterActionComplete(actionIdOnComplete);
                Debug.Log($"[Graph] Action: {actionIdOnComplete}");
            }

            Debug.Log($"[Graph] Stopped. Points: {points.Count}");
        }

        public void ClearGraph()
        {
            points.Clear();
            lineRenderer.positionCount = 0;
            currentTime = 0f;
            CurrentVoltage = 0f;
            isRecording = false;
        }

        public void SetTimeConstant(float rc)
        {
            timeConstant = rc;
        }

        public void SetSimulationSpeed(float speed)
        {
            simulationSpeed = speed;
        }

        public void SetMaxTime(float time)
        {
            maxTime = time;
        }

        public void SetMaxVoltageDisplay(float voltage)
        {
            maxVoltage = voltage;
        }

        public void SetLineColor(Color color)
        {
            lineColor = color;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                if (lineRenderer.material != null)
                    lineRenderer.material.color = color;
            }
        }

        void OnDrawGizmos()
        {
            if (plotBottomLeft == null || plotTopRight == null) return;

            Vector3 bl = plotBottomLeft.position;
            Vector3 tr = plotTopRight.position;
            Vector3 tl = new Vector3(bl.x, tr.y, bl.z);
            Vector3 br = new Vector3(tr.x, bl.y, bl.z);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(bl, 0.08f);
            Gizmos.DrawSphere(tr, 0.08f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tr, br);
        }
    }
}