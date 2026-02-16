using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    [RequireComponent(typeof(LineRenderer))]
    public class DischargeGraph : MonoBehaviour
    {
        [Header("Graph Settings")]
        [SerializeField] private float graphWidth = 4f;
        [SerializeField] private float graphHeight = 3f;

        [Header("Discharge Simulation")]
        [SerializeField] private float timeConstant = 5f;
        [SerializeField] private float simulationSpeed = 1f;
        [SerializeField] private float maxTime = 40f;

        [Header("Action Registration")]
        [SerializeField] private string actionIdOnRecordingComplete;

        [Header("Sampling")]
        [SerializeField] private float sampleInterval = 0.1f;

        [Header("Visual Settings")]
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private Color lineColor = Color.blue;

        [Header("Axis References")]
        [SerializeField] private LineRenderer xAxisLine;
        [SerializeField] private LineRenderer yAxisLine;
        [SerializeField] private Transform graphOrigin;

        [Header("Axis Labels")]
        [SerializeField] private TMP_Text xAxisLabel;
        [SerializeField] private TMP_Text yAxisLabel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private string xAxisText = "Time (s)";
        [SerializeField] private string yAxisText = "Voltage (V)";
        [SerializeField] private string titleText = "Capacitor Discharge";

        [Header("Axis Number Labels")]
        [SerializeField] private Transform xAxisLabelsParent;
        [SerializeField] private Transform yAxisLabelsParent;
        [SerializeField] private TMP_Text labelPrefab;
        [SerializeField] private float labelOffset = 0.15f;

        [Header("Y-Axis Config")]
        [SerializeField] private float maxVoltageDisplay = 6f;
        [SerializeField] private float yAxisStep = 2f;

        [Header("X-Axis Config")]
        [SerializeField] private int[] xAxisValues = { 1, 3, 5, 7, 9, 11, 14, 17, 20, 23, 26, 29, 32, 35, 38 };

        [Header("Events")]
        public UnityEvent OnRecordingStarted;
        public UnityEvent OnRecordingStopped;
        public UnityEvent<float, float> OnDataPointAdded;

        private LineRenderer lineRenderer;
        private List<Vector2> dataPoints = new List<Vector2>();
        private bool isRecording = false;
        private float currentSimulationTime = 0f;
        private float initialVoltage = 0f;
        private float currentVoltage = 0f;
        private float timeSinceLastSample = 0f;
        private List<TMP_Text> spawnedLabels = new List<TMP_Text>();

        public bool IsRecording => isRecording;
        public float CurrentVoltage => currentVoltage;
        public int DataPointCount => dataPoints.Count;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            SetupLineRenderer();
        }

        void Start()
        {
            SetupLabels();
            GenerateAxisNumbers();
            ClearGraph();
        }

        void SetupLineRenderer()
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 0;
        }

        void SetupLabels()
        {
            if (xAxisLabel != null) xAxisLabel.text = xAxisText;
            if (yAxisLabel != null) yAxisLabel.text = yAxisText;
            if (titleLabel != null) titleLabel.text = titleText;
        }

        void GenerateAxisNumbers()
        {
            if (labelPrefab == null) return;

            Vector3 origin = graphOrigin != null ? graphOrigin.localPosition : Vector3.zero;

            // Generate Y-axis labels (0, 2, 4, 6)
            if (yAxisLabelsParent != null)
            {
                for (float v = 0; v <= maxVoltageDisplay; v += yAxisStep)
                {
                    float normalizedY = (v / maxVoltageDisplay) * graphHeight;

                    TMP_Text label = Instantiate(labelPrefab, yAxisLabelsParent);
                    label.text = v.ToString("0");
                    label.alignment = TextAlignmentOptions.MidlineRight;

                    label.transform.localPosition = new Vector3(
                        origin.x - labelOffset,
                        origin.y + normalizedY,
                        0f
                    );

                    spawnedLabels.Add(label);
                }
            }

            // Generate X-axis labels (1, 3, 5, 7, 9, 11, 14, 17, 20, 23, 26, 29, 32, 35, 38)
            if (xAxisLabelsParent != null)
            {
                foreach (int t in xAxisValues)
                {
                    float normalizedX = ((float)t / maxTime) * graphWidth;

                    TMP_Text label = Instantiate(labelPrefab, xAxisLabelsParent);
                    label.text = t.ToString();
                    label.alignment = TextAlignmentOptions.Top;

                    label.transform.localPosition = new Vector3(
                        origin.x + normalizedX,
                        origin.y - labelOffset,
                        0f
                    );

                    spawnedLabels.Add(label);
                }
            }
        }

        void Update()
        {
            if (isRecording)
            {
                UpdateSimulation();
            }
        }

        void UpdateSimulation()
        {
            float deltaTime = Time.deltaTime * simulationSpeed;
            currentSimulationTime += deltaTime;
            timeSinceLastSample += deltaTime;

            if (currentSimulationTime >= maxTime)
            {
                currentVoltage = initialVoltage * Mathf.Exp(-maxTime / timeConstant);
                AddDataPoint(maxTime, currentVoltage);
                StopRecording();
                return;
            }

            currentVoltage = initialVoltage * Mathf.Exp(-currentSimulationTime / timeConstant);

            if (timeSinceLastSample >= sampleInterval)
            {
                AddDataPoint(currentSimulationTime, currentVoltage);
                timeSinceLastSample = 0f;
            }
        }

        public void StartRecording(float startVoltage)
        {
            if (isRecording) return;

            initialVoltage = startVoltage;
            currentVoltage = startVoltage;
            currentSimulationTime = 0f;
            timeSinceLastSample = 0f;

            ClearGraph();

            AddDataPoint(0f, initialVoltage);

            isRecording = true;
            OnRecordingStarted?.Invoke();

            Debug.Log($"[Graph] Recording started at {initialVoltage}V");
        }

        public void StopRecording()
        {
            if (!isRecording) return;

            isRecording = false;
            OnRecordingStopped?.Invoke();

            // Register action for step progression
            if (!string.IsNullOrEmpty(actionIdOnRecordingComplete))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnRecordingComplete);
            }

            Debug.Log($"[Graph] Recording stopped. {dataPoints.Count} data points");
        }

        public void PauseRecording()
        {
            isRecording = false;
        }

        public void ResumeRecording()
        {
            if (initialVoltage > 0)
            {
                isRecording = true;
            }
        }

        void AddDataPoint(float time, float voltage)
        {
            float normalizedX = (time / maxTime) * graphWidth;
            float normalizedY = (voltage / maxVoltageDisplay) * graphHeight;

            Vector2 point = new Vector2(normalizedX, normalizedY);
            dataPoints.Add(point);

            UpdateLineRenderer();
            OnDataPointAdded?.Invoke(time, voltage);
        }

        void UpdateLineRenderer()
        {
            lineRenderer.positionCount = dataPoints.Count;

            Vector3 originOffset = graphOrigin != null ? graphOrigin.localPosition : Vector3.zero;

            for (int i = 0; i < dataPoints.Count; i++)
            {
                Vector3 pos = new Vector3(
                    dataPoints[i].x + originOffset.x,
                    dataPoints[i].y + originOffset.y,
                    0f
                );
                lineRenderer.SetPosition(i, pos);
            }
        }

        public void ClearGraph()
        {
            dataPoints.Clear();
            lineRenderer.positionCount = 0;
            currentSimulationTime = 0f;
            currentVoltage = 0f;
            timeSinceLastSample = 0f;
            isRecording = false;

            Debug.Log("[Graph] Cleared");
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
            maxVoltageDisplay = voltage;
        }

        public void SetLineColor(Color color)
        {
            lineColor = color;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }

        public List<Vector2> GetDataPoints()
        {
            return new List<Vector2>(dataPoints);
        }

        public float GetVoltageAtTime(float time)
        {
            if (initialVoltage <= 0) return 0f;
            return initialVoltage * Mathf.Exp(-time / timeConstant);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 origin = graphOrigin != null ? graphOrigin.position : transform.position;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + Vector3.right * graphWidth);
            Gizmos.DrawLine(origin, origin + Vector3.up * graphHeight);
            Gizmos.DrawLine(origin + Vector3.right * graphWidth, origin + Vector3.right * graphWidth + Vector3.up * graphHeight);
            Gizmos.DrawLine(origin + Vector3.up * graphHeight, origin + Vector3.right * graphWidth + Vector3.up * graphHeight);
        }
    }
}