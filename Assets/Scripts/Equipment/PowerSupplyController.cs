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

        [Header("Voltage Configuration")]
        [SerializeField] private float[] allowedVoltages = { 1.5f, 3f, 4.5f, 6f };
        [SerializeField] private int currentVoltageIndex = 3;

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

        [Header("Connected Components")]
        [SerializeField] private LED circuitLED;

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
        public float CurrentVoltage => allowedVoltages[currentVoltageIndex];
        public float OutputVoltage => isPoweredOn ? CurrentVoltage : 0f;

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
                TurnOff();
            else
                TurnOn();
        }

        public void TurnOn()
        {
            if (isPoweredOn) return;

            isPoweredOn = true;

            if (switchSound != null) switchSound.Play();

            UpdateDisplay();

            if (circuitLED != null)
                circuitLED.TurnOn();

            OnPowerOn?.Invoke();
            OnPowerStateChanged?.Invoke(true);
            OnVoltageChanged?.Invoke(CurrentVoltage);

            if (!string.IsNullOrEmpty(actionIdOnPowerOn))
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnPowerOn);

            Debug.Log($"[PowerSupply] ON - Output: {CurrentVoltage}V");
        }

        public void TurnOff()
        {
            if (!isPoweredOn) return;

            isPoweredOn = false;

            if (switchSound != null) switchSound.Play();

            UpdateDisplay();

            if (circuitLED != null)
                circuitLED.TurnOff();

            OnPowerOff?.Invoke();
            OnPowerStateChanged?.Invoke(false);
            OnVoltageChanged?.Invoke(0f);

            if (!string.IsNullOrEmpty(actionIdOnPowerOff))
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnPowerOff);

            Debug.Log("[PowerSupply] OFF");
        }

        public void IncreaseVoltage()
        {
            if (!isInteractable) return;
            if (allowedVoltages.Length == 0) return;

            if (currentVoltageIndex < allowedVoltages.Length - 1)
            {
                currentVoltageIndex++;
                OnVoltageUpdated();
            }
        }

        public void DecreaseVoltage()
        {
            if (!isInteractable) return;
            if (allowedVoltages.Length == 0) return;

            if (currentVoltageIndex > 0)
            {
                currentVoltageIndex--;
                OnVoltageUpdated();
            }
        }

        void OnVoltageUpdated()
        {
            if (adjustSound != null) adjustSound.Play();

            UpdateDisplay();

            if (isPoweredOn)
                OnVoltageChanged?.Invoke(CurrentVoltage);

            if (!string.IsNullOrEmpty(actionIdOnVoltageChanged))
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnVoltageChanged);

            Debug.Log($"[PowerSupply] Voltage: {CurrentVoltage}V");
        }

        public void SetVoltage(float voltage)
        {
            // Find closest allowed voltage
            int closestIndex = 0;
            float closestDiff = Mathf.Abs(allowedVoltages[0] - voltage);

            for (int i = 1; i < allowedVoltages.Length; i++)
            {
                float diff = Mathf.Abs(allowedVoltages[i] - voltage);
                if (diff < closestDiff)
                {
                    closestDiff = diff;
                    closestIndex = i;
                }
            }

            if (currentVoltageIndex != closestIndex)
            {
                currentVoltageIndex = closestIndex;
                OnVoltageUpdated();
            }
        }

        void UpdateDisplay()
        {
            if (voltageDisplayText != null)
                voltageDisplayText.text = string.Format(voltageFormat, CurrentVoltage);

            if (powerIndicator != null)
                powerIndicator.color = isPoweredOn ? powerOnColor : powerOffColor;

            UpdateBatteryVisual();
        }

        void UpdateBatteryVisual()
        {
            if (batteryVisual == null) return;

            int batteryCount = GetBatteryCount();
            batteryVisual.SetActive(batteryCount > 0);
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public void Reset()
        {
            isPoweredOn = false;
            currentVoltageIndex = 3;

            if (circuitLED != null)
                circuitLED.TurnOff();

            UpdateDisplay();
        }

        public int GetBatteryCount()
        {
            return Mathf.RoundToInt(CurrentVoltage / 1.5f);
        }
    }
}