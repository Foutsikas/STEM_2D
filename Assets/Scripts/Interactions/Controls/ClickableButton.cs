using UnityEngine;
using UnityEngine.Events;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class ClickableButton : MonoBehaviour, IInteractable
    {
        [Header("Button Settings")]
        [SerializeField] private string buttonId;
        
        [Header("Action Registration")]
        [SerializeField] private string actionIdOnClick;
        
        [Header("Behavior")]
        [SerializeField] private bool singleUse = false;
        [SerializeField] private bool toggleMode = false;
        
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer buttonRenderer;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] private Sprite toggledOnSprite;
        [SerializeField] private Color hoverTint = new Color(1.1f, 1.1f, 1.1f, 1f);
        
        [Header("Animation")]
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float animationSpeed = 10f;
        
        [Header("Audio")]
        [SerializeField] private AudioSource clickSound;
        
        [Header("Events")]
        public UnityEvent OnClicked;
        public UnityEvent<bool> OnToggled;

        private bool isInteractable = true;
        private bool hasBeenUsed = false;
        private bool isToggled = false;
        private bool isHovered = false;
        private bool isPressed = false;
        private Vector3 originalScale;
        private Color originalColor;

        public string ButtonId => buttonId;
        public bool IsToggled => isToggled;

        void Awake()
        {
            if (string.IsNullOrEmpty(buttonId))
            {
                buttonId = gameObject.name;
            }

            originalScale = transform.localScale;
            
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (buttonRenderer != null)
            {
                originalColor = buttonRenderer.color;
            }
        }

        void Update()
        {
            float targetScale = isPressed ? pressScale : 1f;
            transform.localScale = Vector3.Lerp(
                transform.localScale, 
                originalScale * targetScale, 
                animationSpeed * Time.deltaTime
            );
        }

        void OnMouseEnter()
        {
            if (!isInteractable) return;
            if (singleUse && hasBeenUsed) return;

            isHovered = true;
            
            if (buttonRenderer != null)
            {
                buttonRenderer.color = originalColor * hoverTint;
            }
        }

        void OnMouseExit()
        {
            isHovered = false;
            isPressed = false;
            
            if (buttonRenderer != null)
            {
                buttonRenderer.color = originalColor;
            }
            
            UpdateSprite();
        }

        void OnMouseDown()
        {
            if (!isInteractable) return;
            if (singleUse && hasBeenUsed) return;

            isPressed = true;
            
            if (pressedSprite != null && buttonRenderer != null)
            {
                buttonRenderer.sprite = pressedSprite;
            }
        }

        void OnMouseUp()
        {
            if (!isPressed) return;
            
            isPressed = false;
            
            if (isHovered)
            {
                ExecuteClick();
            }
            
            UpdateSprite();
        }

        void ExecuteClick()
        {
            if (singleUse && hasBeenUsed) return;

            hasBeenUsed = true;
            
            if (toggleMode)
            {
                isToggled = !isToggled;
                OnToggled?.Invoke(isToggled);
            }
            
            if (clickSound != null)
            {
                clickSound.Play();
            }

            OnClicked?.Invoke();
            
            if (!string.IsNullOrEmpty(actionIdOnClick))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionIdOnClick);
            }
            
            Debug.Log($"[Button] {buttonId}: Clicked" + (toggleMode ? $" (Toggled: {isToggled})" : ""));
        }

        void UpdateSprite()
        {
            if (buttonRenderer == null) return;

            if (toggleMode && isToggled && toggledOnSprite != null)
            {
                buttonRenderer.sprite = toggledOnSprite;
            }
            else if (normalSprite != null)
            {
                buttonRenderer.sprite = normalSprite;
            }
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public bool CanInteract()
        {
            if (singleUse && hasBeenUsed) return false;
            return isInteractable;
        }

        public void ResetButton()
        {
            hasBeenUsed = false;
            isToggled = false;
            isPressed = false;
            isHovered = false;
            UpdateSprite();
        }
    }
}
