using UnityEngine;
using UnityEngine.Events;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class Switch : MonoBehaviour, IInteractable
    {
        [Header("Switch Settings")]
        [SerializeField] private string switchId;
        [SerializeField] private bool startOn = false;
        
        [Header("Action Registration")]
        [SerializeField] private string actionIdOnTurnOn;
        [SerializeField] private string actionIdOnTurnOff;
        
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer switchRenderer;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Transform togglePart;
        [SerializeField] private Vector3 onPosition;
        [SerializeField] private Vector3 offPosition;
        
        [Header("Animation")]
        [SerializeField] private float toggleSpeed = 10f;
        
        [Header("Indicator Light")]
        [SerializeField] private SpriteRenderer indicatorLight;
        [SerializeField] private Color onColor = Color.green;
        [SerializeField] private Color offColor = Color.red;
        
        [Header("Audio")]
        [SerializeField] private AudioSource toggleSound;
        
        [Header("Events")]
        public UnityEvent OnSwitchOn;
        public UnityEvent OnSwitchOff;
        public UnityEvent<bool> OnStateChanged;

        private bool isOn = false;
        private bool isInteractable = true;
        private bool isAnimating = false;
        private Vector3 targetPosition;

        public string SwitchId => switchId;
        public bool IsOn => isOn;

        void Awake()
        {
            if (string.IsNullOrEmpty(switchId))
            {
                switchId = gameObject.name;
            }
        }

        void Start()
        {
            isOn = startOn;
            UpdateVisuals(true);
        }

        void Update()
        {
            if (isAnimating && togglePart != null)
            {
                togglePart.localPosition = Vector3.MoveTowards(
                    togglePart.localPosition, 
                    targetPosition, 
                    toggleSpeed * Time.deltaTime
                );

                if (Vector3.Distance(togglePart.localPosition, targetPosition) < 0.001f)
                {
                    togglePart.localPosition = targetPosition;
                    isAnimating = false;
                }
            }
        }

        void OnMouseDown()
        {
            if (!isInteractable) return;
            if (isAnimating) return;

            Toggle();
        }

        public void Toggle()
        {
            SetState(!isOn);
        }

        public void TurnOn()
        {
            SetState(true);
        }

        public void TurnOff()
        {
            SetState(false);
        }

        public void SetState(bool on)
        {
            if (isOn == on) return;

            isOn = on;
            
            if (toggleSound != null)
            {
                toggleSound.Play();
            }

            UpdateVisuals(false);
            RegisterAction();

            if (isOn)
            {
                OnSwitchOn?.Invoke();
            }
            else
            {
                OnSwitchOff?.Invoke();
            }
            
            OnStateChanged?.Invoke(isOn);
            
            Debug.Log($"[Switch] {switchId}: {(isOn ? "ON" : "OFF")}");
        }

        void RegisterAction()
        {
            string actionId = isOn ? actionIdOnTurnOn : actionIdOnTurnOff;
            
            if (!string.IsNullOrEmpty(actionId))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionId);
            }
        }

        void UpdateVisuals(bool instant)
        {
            if (switchRenderer != null)
            {
                if (isOn && onSprite != null)
                {
                    switchRenderer.sprite = onSprite;
                }
                else if (!isOn && offSprite != null)
                {
                    switchRenderer.sprite = offSprite;
                }
            }

            if (togglePart != null)
            {
                targetPosition = isOn ? onPosition : offPosition;
                
                if (instant)
                {
                    togglePart.localPosition = targetPosition;
                    isAnimating = false;
                }
                else
                {
                    isAnimating = true;
                }
            }

            if (indicatorLight != null)
            {
                indicatorLight.color = isOn ? onColor : offColor;
            }
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public bool CanInteract()
        {
            return isInteractable && !isAnimating;
        }

        public void Reset()
        {
            isOn = startOn;
            UpdateVisuals(true);
        }

        void OnDrawGizmosSelected()
        {
            if (togglePart != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.TransformPoint(onPosition), 0.1f);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.TransformPoint(offPosition), 0.1f);
            }
        }
    }
}
