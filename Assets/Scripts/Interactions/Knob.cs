using UnityEngine;
using UnityEngine.Events;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class Knob : MonoBehaviour, IInteractable
    {
        [Header("Knob Settings")]
        [SerializeField] private string knobId;
        
        [Header("Rotation Presets")]
        [Tooltip("Predefined rotation angles the knob can be set to")]
        [SerializeField] private float[] presetAngles = { 0f, 45f, 90f, 135f, 180f };
        [SerializeField] private int startingPresetIndex = 0;
        
        [Header("Action Registration")]
        [Tooltip("Maps preset index to action ID. Leave empty for no action.")]
        [SerializeField] private string[] actionIdPerPreset;
        
        [Header("Value Output")]
        [Tooltip("Optional: values corresponding to each preset angle")]
        [SerializeField] private float[] presetValues;
        
        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private Transform rotatingPart;
        
        [Header("Audio")]
        [SerializeField] private AudioSource clickSound;
        
        [Header("Events")]
        public UnityEvent<int> OnPresetChanged;
        public UnityEvent<float> OnValueChanged;
        public UnityEvent<float> OnAngleChanged;

        private int currentPresetIndex;
        private float currentAngle;
        private float targetAngle;
        private bool isInteractable = true;
        private bool isRotating = false;

        public string KnobId => knobId;
        public int CurrentPresetIndex => currentPresetIndex;
        public float CurrentAngle => currentAngle;
        public float CurrentValue => GetCurrentValue();

        void Awake()
        {
            if (string.IsNullOrEmpty(knobId))
            {
                knobId = gameObject.name;
            }

            if (rotatingPart == null)
            {
                rotatingPart = transform;
            }
        }

        void Start()
        {
            if (presetAngles.Length == 0)
            {
                presetAngles = new float[] { 0f };
            }

            currentPresetIndex = Mathf.Clamp(startingPresetIndex, 0, presetAngles.Length - 1);
            currentAngle = presetAngles[currentPresetIndex];
            targetAngle = currentAngle;
            
            ApplyRotation(currentAngle);
        }

        void Update()
        {
            if (isRotating)
            {
                currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotationSpeed * Time.deltaTime * 100f);
                ApplyRotation(currentAngle);

                if (Mathf.Approximately(currentAngle, targetAngle))
                {
                    isRotating = false;
                    currentAngle = targetAngle;
                    ApplyRotation(currentAngle);
                }
            }
        }

        void OnMouseDown()
        {
            if (!isInteractable) return;
            if (isRotating) return;

            RotateToNextPreset();
        }

        public void RotateToNextPreset()
        {
            int nextIndex = (currentPresetIndex + 1) % presetAngles.Length;
            SetPreset(nextIndex);
        }

        public void RotateToPreviousPreset()
        {
            int prevIndex = currentPresetIndex - 1;
            if (prevIndex < 0) prevIndex = presetAngles.Length - 1;
            SetPreset(prevIndex);
        }

        public void SetPreset(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= presetAngles.Length) return;

            int previousIndex = currentPresetIndex;
            currentPresetIndex = presetIndex;
            targetAngle = presetAngles[presetIndex];
            isRotating = true;

            if (clickSound != null)
            {
                clickSound.Play();
            }

            OnPresetChanged?.Invoke(currentPresetIndex);
            OnAngleChanged?.Invoke(targetAngle);
            
            if (presetValues != null && presetIndex < presetValues.Length)
            {
                OnValueChanged?.Invoke(presetValues[presetIndex]);
            }

            RegisterAction(presetIndex);
            
            Debug.Log($"[Knob] {knobId}: Preset {currentPresetIndex} (Angle: {targetAngle})");
        }

        void RegisterAction(int presetIndex)
        {
            if (actionIdPerPreset == null) return;
            if (presetIndex >= actionIdPerPreset.Length) return;
            
            string actionId = actionIdPerPreset[presetIndex];
            if (!string.IsNullOrEmpty(actionId))
            {
                ExperimentManager.Instance?.RegisterActionComplete(actionId);
            }
        }

        void ApplyRotation(float angle)
        {
            if (rotatingPart != null)
            {
                rotatingPart.localRotation = Quaternion.Euler(0, 0, -angle);
            }
        }

        float GetCurrentValue()
        {
            if (presetValues == null || currentPresetIndex >= presetValues.Length)
            {
                return currentAngle;
            }
            return presetValues[currentPresetIndex];
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public bool CanInteract()
        {
            return isInteractable && !isRotating;
        }

        public void ResetToStart()
        {
            SetPreset(startingPresetIndex);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 pos = transform.position;
            
            for (int i = 0; i < presetAngles.Length; i++)
            {
                float rad = presetAngles[i] * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0);
                
                Gizmos.color = (i == currentPresetIndex) ? Color.green : Color.yellow;
                Gizmos.DrawLine(pos, pos + dir * 0.5f);
            }
        }
    }
}
