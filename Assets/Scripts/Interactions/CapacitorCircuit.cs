using UnityEngine;
using UnityEngine.Events;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class CapacitorCircuit : MonoBehaviour
    {
        [Header("Circuit Components")]
        [SerializeField] private PowerSupplyController powerSupply;
        [SerializeField] private Switch circuitSwitch;
        [SerializeField] private Lamp led;
        [SerializeField] private VoltageSensor voltageSensor;
        [SerializeField] private DischargeGraph graph;
        
        [Header("Capacitor Settings")]
        [SerializeField] private float capacitance = 0.001f;
        [SerializeField] private float resistance = 5000f;
        
        [Header("State")]
        [SerializeField] private float capacitorVoltage = 0f;
        [SerializeField] private bool isCharging = false;
        [SerializeField] private bool isDischarging = false;
        
        [Header("Simulation")]
        [SerializeField] private float chargeSpeed = 2f;
        [SerializeField] private float dischargeTimeConstant = 5f;
        
        [Header("Action Registration")]
        [SerializeField] private string actionIdOnCircuitClosed;
        [SerializeField] private string actionIdOnDischargeStarted;
        
        [Header("Events")]
        public UnityEvent OnCircuitClosed;
        public UnityEvent OnCircuitOpened;
        public UnityEvent OnCapacitorCharged;
        public UnityEvent OnDischargeStarted;
        public UnityEvent OnDischargeComplete;
        public UnityEvent<float> OnVoltageChanged;

        private float targetVoltage = 0f;
        private float dischargeStartTime = 0f;
        private float initialDischargeVoltage = 0f;
        private bool circuitComplete = false;

        public float CapacitorVoltage => capacitorVoltage;
        public bool IsCharging => isCharging;
        public bool IsDischarging => isDischarging;
        public float TimeConstant => resistance * capacitance;

        void Start()
        {
            SubscribeToEvents();
            
            // Calculate time constant from R and C
            dischargeTimeConstant = resistance * capacitance;
            
            if (graph != null)
            {
                graph.SetTimeConstant(dischargeTimeConstant);
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        void SubscribeToEvents()
        {
            if (powerSupply != null)
            {
                powerSupply.OnVoltageChanged.AddListener(OnPowerSupplyVoltageChanged);
            }
            
            if (circuitSwitch != null)
            {
                circuitSwitch.OnStateChanged.AddListener(OnSwitchStateChanged);
            }
        }

        void UnsubscribeFromEvents()
        {
            if (powerSupply != null)
            {
                powerSupply.OnVoltageChanged.RemoveListener(OnPowerSupplyVoltageChanged);
            }
            
            if (circuitSwitch != null)
            {
                circuitSwitch.OnStateChanged.RemoveListener(OnSwitchStateChanged);
            }
        }

        void OnPowerSupplyVoltageChanged(float voltage)
        {
            targetVoltage = voltage;
            
            if (circuitComplete && voltage > 0)
            {
                StartCharging();
            }
            
            Debug.Log($"[Circuit] Power supply voltage: {voltage}V");
        }

        void OnSwitchStateChanged(bool switchOn)
        {
            if (switchOn)
            {
                CloseCircuit();
            }
            else
            {
                OpenCircuit();
            }
        }

        void CloseCircuit()
        {
            circuitComplete = true;
            OnCircuitClosed?.Invoke();
            
            if (!string.IsNullOrEmpty(actionIdOnCircuitClosed))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnCircuitClosed);
            }
            
            // If power supply is on, start charging
            if (powerSupply != null && powerSupply.IsPoweredOn)
            {
                targetVoltage = powerSupply.CurrentVoltage;
                StartCharging();
            }
            
            Debug.Log("[Circuit] Circuit CLOSED");
        }

        void OpenCircuit()
        {
            circuitComplete = false;
            OnCircuitOpened?.Invoke();
            
            // If capacitor has charge, start discharging
            if (capacitorVoltage > 0.1f)
            {
                StartDischarging();
            }
            
            Debug.Log("[Circuit] Circuit OPENED");
        }

        void StartCharging()
        {
            if (isDischarging)
            {
                StopDischarging();
            }
            
            isCharging = true;
            
            // Turn on LED
            if (led != null)
            {
                led.TurnOn();
            }
            
            Debug.Log($"[Circuit] Charging capacitor to {targetVoltage}V");
        }

        void StopCharging()
        {
            isCharging = false;
        }

        void StartDischarging()
        {
            if (isCharging)
            {
                StopCharging();
            }
            
            isDischarging = true;
            dischargeStartTime = Time.time;
            initialDischargeVoltage = capacitorVoltage;
            
            // Start graph recording
            if (graph != null)
            {
                graph.StartRecording(initialDischargeVoltage);
            }
            
            // Turn off LED (or dim based on voltage)
            if (led != null)
            {
                led.TurnOff();
            }
            
            OnDischargeStarted?.Invoke();
            
            if (!string.IsNullOrEmpty(actionIdOnDischargeStarted))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnDischargeStarted);
            }
            
            Debug.Log($"[Circuit] Discharging from {initialDischargeVoltage}V");
        }

        void StopDischarging()
        {
            isDischarging = false;
            
            if (graph != null)
            {
                graph.StopRecording();
            }
        }

        void Update()
        {
            if (isCharging)
            {
                UpdateCharging();
            }
            else if (isDischarging)
            {
                UpdateDischarging();
            }
            
            // Update LED brightness based on capacitor voltage
            UpdateLED();
            
            // Update voltage sensor reading
            if (voltageSensor != null && voltageSensor.IsConnected)
            {
                voltageSensor.SetVoltageSource(capacitorVoltage);
            }
        }

        void UpdateCharging()
        {
            if (capacitorVoltage < targetVoltage)
            {
                // Simple linear charging for visual feedback
                capacitorVoltage += chargeSpeed * Time.deltaTime;
                capacitorVoltage = Mathf.Min(capacitorVoltage, targetVoltage);
                
                OnVoltageChanged?.Invoke(capacitorVoltage);
                
                if (Mathf.Approximately(capacitorVoltage, targetVoltage))
                {
                    OnCapacitorCharged?.Invoke();
                    Debug.Log($"[Circuit] Capacitor fully charged: {capacitorVoltage}V");
                }
            }
        }

        void UpdateDischarging()
        {
            float elapsedTime = Time.time - dischargeStartTime;
            
            // Exponential decay: V(t) = V0 * e^(-t/RC)
            capacitorVoltage = initialDischargeVoltage * Mathf.Exp(-elapsedTime / dischargeTimeConstant);
            
            OnVoltageChanged?.Invoke(capacitorVoltage);
            
            // Check if discharge is essentially complete
            if (capacitorVoltage < 0.01f)
            {
                capacitorVoltage = 0f;
                StopDischarging();
                OnDischargeComplete?.Invoke();
                Debug.Log("[Circuit] Discharge complete");
            }
        }

        void UpdateLED()
        {
            if (led == null) return;

            if (circuitComplete && capacitorVoltage > 0.1f)
            {
                // LED brightness proportional to voltage
                float brightness = capacitorVoltage / targetVoltage;
                led.SetBrightness(brightness);
            }
            else if (isDischarging)
            {
                // During discharge, LED dims with voltage
                float brightness = capacitorVoltage / initialDischargeVoltage;
                led.SetBrightness(brightness);
            }
        }

        public void SetCapacitance(float c)
        {
            capacitance = c;
            dischargeTimeConstant = resistance * capacitance;
            
            if (graph != null)
            {
                graph.SetTimeConstant(dischargeTimeConstant);
            }
        }

        public void SetResistance(float r)
        {
            resistance = r;
            dischargeTimeConstant = resistance * capacitance;
            
            if (graph != null)
            {
                graph.SetTimeConstant(dischargeTimeConstant);
            }
        }

        public void Reset()
        {
            capacitorVoltage = 0f;
            isCharging = false;
            isDischarging = false;
            circuitComplete = false;
            
            if (led != null)
            {
                led.TurnOff();
            }
            
            if (graph != null)
            {
                graph.ClearGraph();
            }
        }
    }
}
