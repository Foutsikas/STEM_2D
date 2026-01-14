using UnityEngine;
using UnityEngine.Events;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class VoltageSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        [SerializeField] private string sensorId = "PTS101";
        [SerializeField] private int dl120Channel = 1;
        
        [Header("Wire References")]
        [SerializeField] private DraggableWire redWire;
        [SerializeField] private DraggableWire blackWire;
        
        [Header("Measurement")]
        [SerializeField] private float currentReading = 0f;
        [SerializeField] private float noiseAmount = 0.01f;
        [SerializeField] private float updateInterval = 0.1f;
        
        [Header("Connected Equipment")]
        [SerializeField] private DL120Controller dl120;
        [SerializeField] private DischargeGraph graph;
        
        [Header("Action Registration")]
        [SerializeField] private string actionIdOnBothWiresConnected;
        
        [Header("Events")]
        public UnityEvent OnSensorConnected;
        public UnityEvent OnSensorDisconnected;
        public UnityEvent<float> OnVoltageChanged;

        private bool isConnected = false;
        private bool redConnected = false;
        private bool blackConnected = false;
        private float lastUpdateTime = 0f;
        private float measuredVoltage = 0f;

        public bool IsConnected => isConnected;
        public float CurrentReading => currentReading;
        public float MeasuredVoltage => measuredVoltage;

        void Start()
        {
            SubscribeToWireEvents();
        }

        void OnDestroy()
        {
            UnsubscribeFromWireEvents();
        }

        void SubscribeToWireEvents()
        {
            if (redWire != null)
            {
                redWire.OnConnected.AddListener(OnRedWireConnected);
                redWire.OnDisconnected.AddListener(OnRedWireDisconnected);
            }
            
            if (blackWire != null)
            {
                blackWire.OnConnected.AddListener(OnBlackWireConnected);
                blackWire.OnDisconnected.AddListener(OnBlackWireDisconnected);
            }
        }

        void UnsubscribeFromWireEvents()
        {
            if (redWire != null)
            {
                redWire.OnConnected.RemoveListener(OnRedWireConnected);
                redWire.OnDisconnected.RemoveListener(OnRedWireDisconnected);
            }
            
            if (blackWire != null)
            {
                blackWire.OnConnected.RemoveListener(OnBlackWireConnected);
                blackWire.OnDisconnected.RemoveListener(OnBlackWireDisconnected);
            }
        }

        void OnRedWireConnected(ConnectionPoint point)
        {
            redConnected = true;
            CheckFullConnection();
            Debug.Log($"[VoltageSensor] Red wire connected to {point.PointId}");
        }

        void OnRedWireDisconnected()
        {
            redConnected = false;
            CheckFullConnection();
            Debug.Log("[VoltageSensor] Red wire disconnected");
        }

        void OnBlackWireConnected(ConnectionPoint point)
        {
            blackConnected = true;
            CheckFullConnection();
            Debug.Log($"[VoltageSensor] Black wire connected to {point.PointId}");
        }

        void OnBlackWireDisconnected()
        {
            blackConnected = false;
            CheckFullConnection();
            Debug.Log("[VoltageSensor] Black wire disconnected");
        }

        void CheckFullConnection()
        {
            bool wasConnected = isConnected;
            isConnected = redConnected && blackConnected;

            if (isConnected && !wasConnected)
            {
                OnSensorConnected?.Invoke();
                
                if (!string.IsNullOrEmpty(actionIdOnBothWiresConnected))
                {
                    ExperimentManager.Instance?.RegisterActionComplete(actionIdOnBothWiresConnected);
                }
                
                Debug.Log("[VoltageSensor] Fully connected - ready to measure");
            }
            else if (!isConnected && wasConnected)
            {
                OnSensorDisconnected?.Invoke();
                SetReading(0f);
                Debug.Log("[VoltageSensor] Disconnected");
            }
        }

        void Update()
        {
            if (!isConnected) return;

            if (Time.time - lastUpdateTime >= updateInterval)
            {
                lastUpdateTime = Time.time;
                UpdateReading();
            }
        }

        void UpdateReading()
        {
            // Add small noise to make it look realistic
            float noise = Random.Range(-noiseAmount, noiseAmount);
            currentReading = measuredVoltage + noise;
            currentReading = Mathf.Max(0f, currentReading);
            
            // Update DL120 display
            if (dl120 != null && dl120.IsInMeterMode)
            {
                dl120.SetChannelValue(dl120Channel, currentReading);
            }
            
            OnVoltageChanged?.Invoke(currentReading);
        }

        public void SetReading(float voltage)
        {
            measuredVoltage = voltage;
            currentReading = voltage;
            
            if (dl120 != null && dl120.IsInMeterMode)
            {
                dl120.SetChannelValue(dl120Channel, currentReading);
            }
            
            OnVoltageChanged?.Invoke(currentReading);
        }

        public void SetVoltageSource(float voltage)
        {
            measuredVoltage = voltage;
        }

        public void StartMeasuring(float initialVoltage)
        {
            if (!isConnected)
            {
                Debug.LogWarning("[VoltageSensor] Cannot measure - not connected");
                return;
            }

            measuredVoltage = initialVoltage;
            
            // Start graph recording
            if (graph != null)
            {
                graph.StartRecording(initialVoltage);
            }
            
            Debug.Log($"[VoltageSensor] Started measuring at {initialVoltage}V");
        }

        public void StopMeasuring()
        {
            if (graph != null)
            {
                graph.StopRecording();
            }
            
            Debug.Log("[VoltageSensor] Stopped measuring");
        }

        public void SetDL120Channel(int channel)
        {
            dl120Channel = Mathf.Clamp(channel, 1, 4);
        }

        public void ConnectToDL120(DL120Controller controller)
        {
            dl120 = controller;
        }

        public void ConnectToGraph(DischargeGraph graphDisplay)
        {
            graph = graphDisplay;
        }
    }
}
