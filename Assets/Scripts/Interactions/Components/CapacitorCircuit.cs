using UnityEngine;
using UnityEngine.Events;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class CapacitorCircuit : MonoBehaviour
    {
        [Header("Circuit Components")]
        [SerializeField] private PowerSupplyController powerSupply;
        [SerializeField] private LED led;
        [SerializeField] private VoltageSensor voltageSensor;
        [SerializeField] private DischargeGraph graph;

        [Header("Capacitor Settings")]
        [SerializeField] private float capacitance = 0.00047f;
        [SerializeField] private float resistance = 10000f;

        [Header("State")]
        [SerializeField] private float capacitorVoltage = 0f;
        [SerializeField] private bool isCharging = false;
        [SerializeField] private bool isDischarging = false;

        [Header("Simulation")]
        [SerializeField] private float chargeSpeed = 2f;
        [SerializeField] private float dischargeTimeConstant = 4.7f;
        [SerializeField] private float minimumVoltageThreshold = 0.3f;

        [Header("Action Registration")]
        [SerializeField] private string actionIdOnDischargeStarted = "circuit_closed";

        [Header("Events")]
        public UnityEvent OnChargingStarted;
        public UnityEvent OnCapacitorCharged;
        public UnityEvent OnDischargeStarted;
        public UnityEvent OnDischargeComplete;
        public UnityEvent<float> OnVoltageChanged;

        private float targetVoltage = 0f;
        private float dischargeStartTime = 0f;
        private float initialDischargeVoltage = 0f;

        public float CapacitorVoltage => capacitorVoltage;
        public bool IsCharging => isCharging;
        public bool IsDischarging => isDischarging;
        public float TimeConstant => resistance * capacitance;

        void Start()
        {
            SubscribeToEvents();

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
                powerSupply.OnPowerOn.AddListener(OnPowerSupplyTurnedOn);
                powerSupply.OnPowerOff.AddListener(OnPowerSupplyTurnedOff);
                powerSupply.OnVoltageChanged.AddListener(OnPowerSupplyVoltageChanged);
            }
        }

        void UnsubscribeFromEvents()
        {
            if (powerSupply != null)
            {
                powerSupply.OnPowerOn.RemoveListener(OnPowerSupplyTurnedOn);
                powerSupply.OnPowerOff.RemoveListener(OnPowerSupplyTurnedOff);
                powerSupply.OnVoltageChanged.RemoveListener(OnPowerSupplyVoltageChanged);
            }
        }

        void OnPowerSupplyTurnedOn()
        {
            if (isDischarging)
            {
                StopDischarging();
            }

            targetVoltage = powerSupply.CurrentVoltage;
            StartCharging();

            Debug.Log($"[Circuit] Power ON - Charging to {targetVoltage}V");
        }

        void OnPowerSupplyTurnedOff()
        {
            if (isCharging)
            {
                StopCharging();
            }

            if (capacitorVoltage > 0.1f)
            {
                StartDischarging();
            }

            Debug.Log("[Circuit] Power OFF - Discharging");
        }

        void OnPowerSupplyVoltageChanged(float voltage)
        {
            if (powerSupply.IsPoweredOn)
            {
                targetVoltage = voltage;
                Debug.Log($"[Circuit] Target voltage changed to {voltage}V");
            }
        }

        void StartCharging()
        {
            isCharging = true;
            isDischarging = false;

            if (led != null)
            {
                led.TurnOn();
            }

            OnChargingStarted?.Invoke();
            Debug.Log($"[Circuit] Charging started to {targetVoltage}V");
        }

        void StopCharging()
        {
            isCharging = false;
        }

        void StartDischarging()
        {
            isCharging = false;
            isDischarging = true;
            dischargeStartTime = Time.time;
            initialDischargeVoltage = capacitorVoltage;

            if (graph != null)
            {
                graph.StartRecording(initialDischargeVoltage);
            }

            OnDischargeStarted?.Invoke();

            if (!string.IsNullOrEmpty(actionIdOnDischargeStarted))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnDischargeStarted);
                Debug.Log($"[Circuit] Action '{actionIdOnDischargeStarted}' registered");
            }

            Debug.Log($"[Circuit] Discharging from {initialDischargeVoltage}V (τ = {dischargeTimeConstant}s)");
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

            UpdateLED();
            UpdateVoltageSensor();
        }

        void UpdateCharging()
        {
            if (capacitorVoltage < targetVoltage)
            {
                capacitorVoltage += chargeSpeed * Time.deltaTime;
                capacitorVoltage = Mathf.Min(capacitorVoltage, targetVoltage);

                OnVoltageChanged?.Invoke(capacitorVoltage);

                if (Mathf.Approximately(capacitorVoltage, targetVoltage))
                {
                    OnCapacitorCharged?.Invoke();
                    Debug.Log($"[Circuit] Fully charged: {capacitorVoltage}V");
                }
            }
            else if (capacitorVoltage > targetVoltage)
            {
                capacitorVoltage = targetVoltage;
                OnVoltageChanged?.Invoke(capacitorVoltage);
            }
        }

        void UpdateDischarging()
        {
            float elapsedTime = Time.time - dischargeStartTime;

            capacitorVoltage = initialDischargeVoltage * Mathf.Exp(-elapsedTime / dischargeTimeConstant);

            OnVoltageChanged?.Invoke(capacitorVoltage);

            // Stop when voltage reaches minimum threshold (never hits 0)
            if (capacitorVoltage < minimumVoltageThreshold)
            {
                capacitorVoltage = minimumVoltageThreshold;
                StopDischarging();
                OnDischargeComplete?.Invoke();
                Debug.Log($"[Circuit] Discharge complete at {minimumVoltageThreshold}V threshold");
            }
        }

        void UpdateLED()
        {
            if (led == null) return;

            if (capacitorVoltage > 0.1f)
            {
                float maxVoltage = Mathf.Max(targetVoltage, initialDischargeVoltage, 1.5f);
                float brightness = capacitorVoltage / maxVoltage;
                led.SetBrightness(brightness);
            }
            else
            {
                led.SetBrightness(0f);
            }
        }

        void UpdateVoltageSensor()
        {
            if (voltageSensor != null && voltageSensor.IsConnected)
            {
                voltageSensor.SetVoltageSource(capacitorVoltage);
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