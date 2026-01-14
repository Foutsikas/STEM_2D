using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class DL120Controller : MonoBehaviour
    {
        public enum DL120State
        {
            Off,
            MainMenu,
            Meter
        }

        public enum MenuOption
        {
            EasyLog = 0,
            Meter = 1,
            Snapshot = 2
        }

        [Header("State")]
        [SerializeField] private DL120State currentState = DL120State.Off;
        
        [Header("Action Registration")]
        [SerializeField] private string actionIdOnPowerOn;
        [SerializeField] private string actionIdOnMeterSelected;
        
        [Header("UI References")]
        [SerializeField] private GameObject screenOff;
        [SerializeField] private GameObject screenMainMenu;
        [SerializeField] private GameObject screenMeter;
        
        [Header("Main Menu UI")]
        [SerializeField] private TMP_Text menuTitleText;
        [SerializeField] private TMP_Text option1Text;
        [SerializeField] private TMP_Text option2Text;
        [SerializeField] private TMP_Text option3Text;
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private float selectionYPositions1 = 0.3f;
        [SerializeField] private float selectionYPositions2 = 0f;
        [SerializeField] private float selectionYPositions3 = -0.3f;
        
        [Header("Meter UI")]
        [SerializeField] private TMP_Text channel1Text;
        [SerializeField] private TMP_Text channel2Text;
        [SerializeField] private TMP_Text channel3Text;
        [SerializeField] private TMP_Text channel4Text;
        
        [Header("Menu Options Text")]
        [SerializeField] private string menuTitle = "Main Menu";
        [SerializeField] private string option1Label = "Easy Log";
        [SerializeField] private string option2Label = "Meter";
        [SerializeField] private string option3Label = "Snapshot";
        
        [Header("Button References")]
        [SerializeField] private DL120Button powerButton;
        [SerializeField] private DL120Button upButton;
        [SerializeField] private DL120Button downButton;
        [SerializeField] private DL120Button confirmButton;
        [SerializeField] private DL120Button cancelButton;
        
        [Header("Audio")]
        [SerializeField] private AudioSource buttonSound;
        [SerializeField] private AudioSource powerSound;
        
        [Header("Events")]
        public UnityEvent OnPowerOn;
        public UnityEvent OnPowerOff;
        public UnityEvent OnMeterModeEntered;
        public UnityEvent<int, float> OnChannelValueChanged;

        private MenuOption selectedOption = MenuOption.Meter;
        private float[] channelValues = new float[4];
        private bool isInteractable = true;

        public DL120State CurrentState => currentState;
        public bool IsPoweredOn => currentState != DL120State.Off;
        public bool IsInMeterMode => currentState == DL120State.Meter;

        void Start()
        {
            InitializeButtons();
            UpdateDisplay();
        }

        void InitializeButtons()
        {
            if (powerButton != null)
                powerButton.OnButtonPressed.AddListener(OnPowerButtonPressed);
            if (upButton != null)
                upButton.OnButtonPressed.AddListener(OnUpButtonPressed);
            if (downButton != null)
                downButton.OnButtonPressed.AddListener(OnDownButtonPressed);
            if (confirmButton != null)
                confirmButton.OnButtonPressed.AddListener(OnConfirmButtonPressed);
            if (cancelButton != null)
                cancelButton.OnButtonPressed.AddListener(OnCancelButtonPressed);
        }

        void OnPowerButtonPressed()
        {
            if (!isInteractable) return;

            if (currentState == DL120State.Off)
            {
                PowerOn();
            }
            else
            {
                PowerOff();
            }
        }

        public void PowerOn()
        {
            if (currentState != DL120State.Off) return;

            currentState = DL120State.MainMenu;
            selectedOption = MenuOption.Meter;
            
            if (powerSound != null) powerSound.Play();
            
            UpdateDisplay();
            OnPowerOn?.Invoke();
            
            if (!string.IsNullOrEmpty(actionIdOnPowerOn))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnPowerOn);
            }
            
            Debug.Log("[DL120] Powered ON");
        }

        public void PowerOff()
        {
            if (currentState == DL120State.Off) return;

            currentState = DL120State.Off;
            
            if (powerSound != null) powerSound.Play();
            
            UpdateDisplay();
            OnPowerOff?.Invoke();
            
            Debug.Log("[DL120] Powered OFF");
        }

        void OnUpButtonPressed()
        {
            if (!isInteractable) return;
            if (currentState != DL120State.MainMenu) return;

            if (buttonSound != null) buttonSound.Play();

            int current = (int)selectedOption;
            current--;
            if (current < 0) current = 2;
            selectedOption = (MenuOption)current;
            
            UpdateSelectionIndicator();
            Debug.Log($"[DL120] Selected: {selectedOption}");
        }

        void OnDownButtonPressed()
        {
            if (!isInteractable) return;
            if (currentState != DL120State.MainMenu) return;

            if (buttonSound != null) buttonSound.Play();

            int current = (int)selectedOption;
            current++;
            if (current > 2) current = 0;
            selectedOption = (MenuOption)current;
            
            UpdateSelectionIndicator();
            Debug.Log($"[DL120] Selected: {selectedOption}");
        }

        void OnConfirmButtonPressed()
        {
            if (!isInteractable) return;

            if (buttonSound != null) buttonSound.Play();

            if (currentState == DL120State.MainMenu)
            {
                if (selectedOption == MenuOption.Meter)
                {
                    EnterMeterMode();
                }
                else
                {
                    Debug.Log($"[DL120] {selectedOption} not available in this experiment");
                }
            }
        }

        void OnCancelButtonPressed()
        {
            if (!isInteractable) return;

            if (buttonSound != null) buttonSound.Play();

            if (currentState == DL120State.Meter)
            {
                ExitMeterMode();
            }
        }

        void EnterMeterMode()
        {
            currentState = DL120State.Meter;
            UpdateDisplay();
            
            OnMeterModeEntered?.Invoke();
            
            if (!string.IsNullOrEmpty(actionIdOnMeterSelected))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnMeterSelected);
            }
            
            Debug.Log("[DL120] Entered Meter mode");
        }

        void ExitMeterMode()
        {
            currentState = DL120State.MainMenu;
            UpdateDisplay();
            
            Debug.Log("[DL120] Exited Meter mode");
        }

        void UpdateDisplay()
        {
            // Hide all screens
            if (screenOff != null) screenOff.SetActive(false);
            if (screenMainMenu != null) screenMainMenu.SetActive(false);
            if (screenMeter != null) screenMeter.SetActive(false);

            // Show appropriate screen
            switch (currentState)
            {
                case DL120State.Off:
                    if (screenOff != null) screenOff.SetActive(true);
                    break;
                    
                case DL120State.MainMenu:
                    if (screenMainMenu != null) screenMainMenu.SetActive(true);
                    UpdateMainMenuText();
                    UpdateSelectionIndicator();
                    break;
                    
                case DL120State.Meter:
                    if (screenMeter != null) screenMeter.SetActive(true);
                    UpdateMeterDisplay();
                    break;
            }
        }

        void UpdateMainMenuText()
        {
            if (menuTitleText != null) menuTitleText.text = menuTitle;
            if (option1Text != null) option1Text.text = $"1) {option1Label}";
            if (option2Text != null) option2Text.text = $"2) {option2Label}";
            if (option3Text != null) option3Text.text = $"3) {option3Label}";
        }

        void UpdateSelectionIndicator()
        {
            if (selectionIndicator == null) return;

            float yPos = selectedOption switch
            {
                MenuOption.EasyLog => selectionYPositions1,
                MenuOption.Meter => selectionYPositions2,
                MenuOption.Snapshot => selectionYPositions3,
                _ => selectionYPositions2
            };

            Vector3 pos = selectionIndicator.transform.localPosition;
            pos.y = yPos;
            selectionIndicator.transform.localPosition = pos;
        }

        void UpdateMeterDisplay()
        {
            if (channel1Text != null)
                channel1Text.text = $"1) {channelValues[0]:F2} V";
            if (channel2Text != null)
                channel2Text.text = $"2) {channelValues[1]:F2} V";
            if (channel3Text != null)
                channel3Text.text = $"3) {channelValues[2]:F2} V";
            if (channel4Text != null)
                channel4Text.text = $"4) {channelValues[3]:F2} V";
        }

        public void SetChannelValue(int channel, float value)
        {
            if (channel < 1 || channel > 4) return;
            
            channelValues[channel - 1] = value;
            
            if (currentState == DL120State.Meter)
            {
                UpdateMeterDisplay();
            }
            
            OnChannelValueChanged?.Invoke(channel, value);
        }

        public float GetChannelValue(int channel)
        {
            if (channel < 1 || channel > 4) return 0f;
            return channelValues[channel - 1];
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public void Reset()
        {
            currentState = DL120State.Off;
            selectedOption = MenuOption.Meter;
            channelValues = new float[4];
            UpdateDisplay();
        }
    }

    public class DL120Button : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer buttonRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private float pressScale = 0.95f;
        
        [Header("Events")]
        public UnityEvent OnButtonPressed;
        public UnityEvent OnButtonReleased;

        private Vector3 originalScale;
        private bool isPressed = false;

        void Awake()
        {
            originalScale = transform.localScale;
            
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            
            transform.localScale = originalScale * pressScale;
            
            if (buttonRenderer != null)
            {
                buttonRenderer.color = pressedColor;
            }
            
            OnButtonPressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            
            isPressed = false;
            
            transform.localScale = originalScale;
            
            if (buttonRenderer != null)
            {
                buttonRenderer.color = normalColor;
            }
            
            OnButtonReleased?.Invoke();
        }
    }
}
