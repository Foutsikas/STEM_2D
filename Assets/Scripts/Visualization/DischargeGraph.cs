using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace STEM2D.Interactions
{
    [RequireComponent(typeof(LineRenderer))]
    public class DischargeGraph : MonoBehaviour
    {
        [Header("Graph Settings")]
        [SerializeField] private float graphWidth = 4f;
        [SerializeField] private float graphHeight = 3f;
        [SerializeField] private int maxDataPoints = 100;
        
        [Header("Discharge Simulation")]
        [SerializeField] private float timeConstant = 5f;
        [SerializeField] private float simulationSpeed = 1f;
        [SerializeField] private float maxTime = 40f;
        
        [Header("Visual Settings")]
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private Color lineColor = Color.blue;
        [SerializeField] private Color gridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        
        [Header("Axis References")]
        [SerializeField] private LineRenderer xAxisLine;
        [SerializeField] private LineRenderer yAxisLine;
        [SerializeField] private Transform graphOrigin;
        
        [Header("Labels")]
        [SerializeField] private TMPro.TMP_Text xAxisLabel;
        [SerializeField] private TMPro.TMP_Text yAxisLabel;
        [SerializeField] private TMPro.TMP_Text titleLabel;
        [SerializeField] private string xAxisText = "Time (s)";
        [SerializeField] private string yAxisText = "Voltage (V)";
        [SerializeField] private string titleText = "Capacitor Discharge";
        
        [Header("Events")]
        public UnityEvent OnRecordingStarted;
        public UnityEvent OnRecordingStopped;
        public UnityEvent<float, float> OnDataPointAdded;

        private LineRenderer lineRenderer;
        private List<Vector2> dataPoints = new List<Vector2>();
        private bool isRecording = false;
        private float recordingStartTime = 0f;
        private float currentSimulationTime = 0f;
        private float initialVoltage = 0f;
        private float currentVoltage = 0f;

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

        void Update()
        {
            if (isRecording)
            {
                UpdateSimulation();
            }
        }

        void UpdateSimulation()
        {
            currentSimulationTime += Time.deltaTime * simulationSpeed;
            
            if (currentSimulationTime >= maxTime)
            {
                StopRecording();
                return;
            }

            // Calculate voltage using exponential decay: V(t) = V0 * e^(-t/RC)
            currentVoltage = initialVoltage * Mathf.Exp(-currentSimulationTime / timeConstant);
            
            // Add data point
            AddDataPoint(currentSimulationTime, currentVoltage);
        }

        public void StartRecording(float startVoltage)
        {
            if (isRecording) return;

            initialVoltage = startVoltage;
            currentVoltage = startVoltage;
            currentSimulationTime = 0f;
            recordingStartTime = Time.time;
            
            ClearGraph();
            
            // Add initial point
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
            // Normalize to graph coordinates
            float normalizedX = (time / maxTime) * graphWidth;
            float normalizedY = (voltage / GetMaxVoltageScale()) * graphHeight;
            
            Vector2 point = new Vector2(normalizedX, normalizedY);
            dataPoints.Add(point);
            
            // Limit data points
            if (dataPoints.Count > maxDataPoints)
            {
                dataPoints.RemoveAt(0);
            }
            
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

        float GetMaxVoltageScale()
        {
            // Round up to nearest 1.5V increment for nice axis labels
            float maxV = Mathf.Max(initialVoltage, 1.5f);
            return Mathf.Ceil(maxV / 1.5f) * 1.5f;
        }

        public void ClearGraph()
        {
            dataPoints.Clear();
            lineRenderer.positionCount = 0;
            currentSimulationTime = 0f;
            currentVoltage = 0f;
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
            
            // Draw graph bounds
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + Vector3.right * graphWidth);
            Gizmos.DrawLine(origin, origin + Vector3.up * graphHeight);
            Gizmos.DrawLine(origin + Vector3.right * graphWidth, origin + Vector3.right * graphWidth + Vector3.up * graphHeight);
            Gizmos.DrawLine(origin + Vector3.up * graphHeight, origin + Vector3.right * graphWidth + Vector3.up * graphHeight);
        }
    }
}
