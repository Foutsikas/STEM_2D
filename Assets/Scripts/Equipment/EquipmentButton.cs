using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace STEM2D.Interactions
{
    /// <summary>
    /// Generic clickable button for equipment (Power Supply, DL120, etc.)
    /// Uses new Input System pointer events.
    /// Requires BoxCollider2D and Physics2DRaycaster on camera.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class EquipmentButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Button Settings")]
        [SerializeField] private string buttonId;
        [SerializeField] private bool interactable = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer buttonRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
        [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f);
        
        [Header("Press Animation")]
        [SerializeField] private bool useScaleAnimation = true;
        [SerializeField] private float pressedScale = 0.95f;
        [SerializeField] private float animationSpeed = 15f;
        
        [Header("Audio")]
        [SerializeField] private AudioSource clickSound;
        
        [Header("Events")]
        public UnityEvent OnButtonPressed;
        public UnityEvent OnButtonReleased;

        private Vector3 originalScale;
        private Vector3 targetScale;
        private bool isPressed = false;
        private bool isHovered = false;

        public string ButtonId => buttonId;
        public bool IsInteractable => interactable;

        void Awake()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
            
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (string.IsNullOrEmpty(buttonId))
            {
                buttonId = gameObject.name;
            }
        }

        void Start()
        {
            UpdateVisuals();
        }

        void Update()
        {
            if (useScaleAnimation)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, animationSpeed * Time.deltaTime);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            
            isHovered = true;
            UpdateVisuals();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isPressed = false;
            targetScale = originalScale;
            UpdateVisuals();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;

            isPressed = true;
            targetScale = originalScale * pressedScale;
            
            if (clickSound != null)
            {
                clickSound.Play();
            }
            
            UpdateVisuals();
            OnButtonPressed?.Invoke();
            
            Debug.Log($"[Button] {buttonId}: Pressed");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;

            isPressed = false;
            targetScale = originalScale;
            
            UpdateVisuals();
            OnButtonReleased?.Invoke();
        }

        void UpdateVisuals()
        {
            if (buttonRenderer == null) return;

            if (!interactable)
            {
                buttonRenderer.color = disabledColor;
            }
            else if (isPressed)
            {
                buttonRenderer.color = pressedColor;
            }
            else if (isHovered)
            {
                buttonRenderer.color = hoverColor;
            }
            else
            {
                buttonRenderer.color = normalColor;
            }
        }

        public void SetInteractable(bool value)
        {
            interactable = value;
            UpdateVisuals();
        }

        public void SimulatePress()
        {
            if (!interactable) return;
            
            OnButtonPressed?.Invoke();
        }
    }
}
