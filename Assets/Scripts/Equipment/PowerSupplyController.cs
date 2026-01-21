using UnityEngine;
using UnityEngine.Events;
using TMPro;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class PowerSupplyController : MonoBehaviour
    {
        [Header("Power Settings")]
        [SerializeField] private bool isPoweredOn = false;
        [SerializeField] private float currentVoltage = 1.5f;
        
        [Header("Voltage Configuration")]
        [SerializeField] private float minVoltage = 0f;
        [SerializeField] private float maxVoltage = 9f;
        [SerializeField] private float voltageStep = 1.5f;
        
        [Header("Action Registration")]
        [SerializeField] private string actionIdOnPowerOn;
        [SerializeField] private string actionIdOnPowerOff;
        [SerializeField] private string actionIdOnVoltageChanged;
        
        [Header("UI References")]
        [SerializeField] private TMP_Text voltageDisplayText;
        [SerializeField] private string voltageFormat = "{0:F1} V";
        
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer powerIndicator;
        [SerializeField] private Color powerOnColor = Color.green;
        [SerializeField] private Color powerOffColor = Color.red;
        [SerializeField] private GameObject batteryVisual;
        
        [Header("Button References")]
        [SerializeField] private EquipmentButton powerSwitch;
        [SerializeField] private EquipmentButton plusButton;
        [SerializeField] private EquipmentButton minusButton;
        
        [Header("Audio")]
        [SerializeField] private AudioSource switchSound;
        [SerializeField] private AudioSource adjustSound;
        
        [Header("Events")]
        public UnityEvent OnPowerOn;
        public UnityEvent OnPowerOff;
        public UnityEvent<bool> OnPowerStateChanged;
        public UnityEvent<float> OnVoltageChanged;

        private bool isInteractable = true;

        public bool IsPoweredOn => isPoweredOn;
        public float CurrentVoltage => currentVoltage;
        public float OutputVoltage => isPoweredOn ? currentVoltage : 0f;

        void Start()
        {
            InitializeButtons();
            UpdateDisplay();
        }

        void InitializeButtons()
        {
            if (powerSwitch != null)
                powerSwitch.OnButtonPressed.AddListener(TogglePower);
            if (plusButton != null)
                plusButton.OnButtonPressed.AddListener(IncreaseVoltage);
            if (minusButton != null)
                minusButton.OnButtonPressed.AddListener(DecreaseVoltage);
        }

        public void TogglePower()
        {
            if (!isInteractable) return;

            if (isPoweredOn)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        public void TurnOn()
        {
            if (isPoweredOn) return;

            isPoweredOn = true;
            
            if (switchSound != null) switchSound.Play();
            
            UpdateDisplay();
            
            OnPowerOn?.Invoke();
            OnPowerStateChanged?.Invoke(true);
            OnVoltageChanged?.Invoke(currentVoltage);
            
            if (!string.IsNullOrEmpty(actionIdOnPowerOn))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnPowerOn);
            }
            
            Debug.Log($"[PowerSupply] ON - Output: {currentVoltage}V");
        }

        public void TurnOff()
        {
            if (!isPoweredOn) return;

            isPoweredOn = false;
            
            if (switchSound != null) switchSound.Play();
            
            UpdateDisplay();
            
            OnPowerOff?.Invoke();
            OnPowerStateChanged?.Invoke(false);
            OnVoltageChanged?.Invoke(0f);
            
            if (!string.IsNullOrEmpty(actionIdOnPowerOff))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnPowerOff);
            }
            
            Debug.Log("[PowerSupply] OFF");
        }

        public void IncreaseVoltage()
        {
            if (!isInteractable) return;

            float newVoltage = currentVoltage + voltageStep;
            if (newVoltage <= maxVoltage)
            {
                SetVoltage(newVoltage);
                if (adjustSound != null) adjustSound.Play();
            }
        }

        public void DecreaseVoltage()
        {
            if (!isInteractable) return;

            float newVoltage = currentVoltage - voltageStep;
            if (newVoltage >= minVoltage)
            {
                SetVoltage(newVoltage);
                if (adjustSound != null) adjustSound.Play();
            }
        }

        public void SetVoltage(float voltage)
        {
            voltage = Mathf.Clamp(voltage, minVoltage, maxVoltage);
            
            if (Mathf.Approximately(currentVoltage, voltage)) return;

            currentVoltage = voltage;
            
            UpdateDisplay();
            
            if (isPoweredOn)
            {
                OnVoltageChanged?.Invoke(currentVoltage);
            }
            
            if (!string.IsNullOrEmpty(actionIdOnVoltageChanged))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnVoltageChanged);
            }
            
            Debug.Log($"[PowerSupply] Voltage set to {currentVoltage}V");
        }

        void UpdateDisplay()
        {
            // Update voltage text
            if (voltageDisplayText != null)
            {
                voltageDisplayText.text = string.Format(voltageFormat, currentVoltage);
            }

            // Update power indicator
            if (powerIndicator != null)
            {
                powerIndicator.color = isPoweredOn ? powerOnColor : powerOffColor;
            }

            // Update battery visual (show/hide based on voltage level)
            UpdateBatteryVisual();
        }

        void UpdateBatteryVisual()
        {
            if (batteryVisual == null) return;

            // Calculate how many "battery units" based on voltage
            // Assuming 1.5V per battery
            int batteryCount = Mathf.RoundToInt(currentVoltage / 1.5f);
            
            // You can expand this to show multiple battery sprites
            batteryVisual.SetActive(batteryCount > 0);
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public void Reset()
        {
            isPoweredOn = false;
            currentVoltage = 1.5f;
            UpdateDisplay();
        }

        public int GetBatteryCount()
        {
            return Mathf.RoundToInt(currentVoltage / 1.5f);
        }
    }
}
