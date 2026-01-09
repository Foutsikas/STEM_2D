using UnityEngine;
using System.Collections.Generic;

namespace STEM2D.Core
{
    public class InteractionGate : MonoBehaviour
    {
        [Header("Activation Settings")]
        [Tooltip("Step IDs during which this object is interactable")]
        [SerializeField] private List<string> activeOnSteps = new List<string>();
        
        [Tooltip("If true, object is always interactable regardless of step")]
        [SerializeField] private bool alwaysActive = false;

        [Header("Visual Feedback")]
        [SerializeField] private bool dimWhenInactive = true;
        [SerializeField] private float inactiveAlpha = 0.5f;
        [SerializeField] private bool showHighlightWhenActive = true;
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool isCurrentlyActive = false;
        private IInteractable interactable;

        public bool IsActive => isCurrentlyActive;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            interactable = GetComponent<IInteractable>();
        }

        void OnEnable()
        {
            if (ExperimentManager.Instance != null)
            {
                ExperimentManager.Instance.OnStepChanged += OnStepChanged;
                UpdateActiveState();
            }
        }

        void OnDisable()
        {
            if (ExperimentManager.Instance != null)
            {
                ExperimentManager.Instance.OnStepChanged -= OnStepChanged;
            }
        }

        void Start()
        {
            UpdateActiveState();
        }

        void OnStepChanged(int stepIndex)
        {
            UpdateActiveState();
        }

        void UpdateActiveState()
        {
            if (alwaysActive)
            {
                SetActive(true);
                return;
            }

            if (ExperimentManager.Instance == null || ExperimentManager.Instance.CurrentStep == null)
            {
                SetActive(false);
                return;
            }

            string currentStepId = ExperimentManager.Instance.CurrentStep.stepId;
            bool shouldBeActive = activeOnSteps.Contains(currentStepId);
            
            SetActive(shouldBeActive);
        }

        void SetActive(bool active)
        {
            isCurrentlyActive = active;

            if (interactable != null)
            {
                interactable.SetInteractable(active);
            }

            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            if (spriteRenderer == null) return;

            if (isCurrentlyActive)
            {
                if (showHighlightWhenActive)
                {
                    spriteRenderer.color = highlightColor;
                }
                else
                {
                    spriteRenderer.color = originalColor;
                }
            }
            else
            {
                if (dimWhenInactive)
                {
                    Color dimmed = originalColor;
                    dimmed.a = inactiveAlpha;
                    spriteRenderer.color = dimmed;
                }
                else
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        public void AddActiveStep(string stepId)
        {
            if (!activeOnSteps.Contains(stepId))
            {
                activeOnSteps.Add(stepId);
                UpdateActiveState();
            }
        }

        public void RemoveActiveStep(string stepId)
        {
            if (activeOnSteps.Remove(stepId))
            {
                UpdateActiveState();
            }
        }

        public void SetAlwaysActive(bool always)
        {
            alwaysActive = always;
            UpdateActiveState();
        }
    }

    public interface IInteractable
    {
        void SetInteractable(bool interactable);
        bool CanInteract();
    }
}
