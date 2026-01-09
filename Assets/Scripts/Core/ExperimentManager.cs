using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace STEM2D.Core
{
    public class ExperimentManager : MonoBehaviour
    {
        public static ExperimentManager Instance { get; private set; }

        [System.Serializable]
        public class ExperimentStep
        {
            [Header("Step Info")]
            public string stepId;
            [TextArea(2, 4)]
            public string instruction;
            
            [Header("Instruction Display")]
            public Sprite infographic;
            public bool showNextButton = false;
            
            [Header("Required Actions")]
            [Tooltip("IDs of actions that must be completed to advance")]
            public List<string> requiredActionIds = new List<string>();
            
            [Header("Events")]
            public UnityEvent onStepEnter;
            public UnityEvent onStepComplete;
        }

        [Header("Experiment Configuration")]
        [SerializeField] private string experimentTitle;
        [SerializeField] private List<ExperimentStep> steps = new List<ExperimentStep>();
        
        [Header("UI References")]
        [SerializeField] private InstructionPanel instructionPanel;
        
        [Header("Settings")]
        [SerializeField] private bool autoStartOnAwake = true;

        private int currentStepIndex = -1;
        private HashSet<string> completedActions = new HashSet<string>();
        
        public int CurrentStepIndex => currentStepIndex;
        public ExperimentStep CurrentStep => 
            (currentStepIndex >= 0 && currentStepIndex < steps.Count) ? steps[currentStepIndex] : null;
        public bool IsRunning => currentStepIndex >= 0 && currentStepIndex < steps.Count;

        public event System.Action<int> OnStepChanged;
        public event System.Action OnExperimentComplete;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (autoStartOnAwake)
            {
                StartExperiment();
            }
        }

        public void StartExperiment()
        {
            currentStepIndex = -1;
            completedActions.Clear();
            GoToNextStep();
        }

        public void GoToNextStep()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
            {
                steps[currentStepIndex].onStepComplete?.Invoke();
            }

            currentStepIndex++;
            completedActions.Clear();

            if (currentStepIndex >= steps.Count)
            {
                OnExperimentComplete?.Invoke();
                Debug.Log("[Experiment] Complete!");
                return;
            }

            EnterCurrentStep();
        }

        void EnterCurrentStep()
        {
            ExperimentStep step = CurrentStep;
            if (step == null) return;

            Debug.Log($"[Experiment] Step {currentStepIndex}: {step.stepId}");

            step.onStepEnter?.Invoke();
            OnStepChanged?.Invoke(currentStepIndex);

            UpdateInstructionPanel();
            
            if (step.requiredActionIds.Count == 0 && !step.showNextButton)
            {
                step.showNextButton = true;
            }
            
            CheckStepCompletion();
        }

        void UpdateInstructionPanel()
        {
            if (instructionPanel == null) return;

            ExperimentStep step = CurrentStep;
            if (step == null) return;

            instructionPanel.ShowInstruction(
                step.instruction,
                step.infographic,
                step.showNextButton,
                step.requiredActionIds.Count == 0
            );
        }

        public void RegisterActionComplete(string actionId)
        {
            if (string.IsNullOrEmpty(actionId)) return;
            
            completedActions.Add(actionId);
            Debug.Log($"[Experiment] Action completed: {actionId}");
            
            CheckStepCompletion();
        }

        void CheckStepCompletion()
        {
            ExperimentStep step = CurrentStep;
            if (step == null) return;

            if (step.requiredActionIds.Count == 0)
            {
                return;
            }

            foreach (string requiredId in step.requiredActionIds)
            {
                if (!completedActions.Contains(requiredId))
                {
                    return;
                }
            }

            Debug.Log($"[Experiment] All actions complete for step {currentStepIndex}");
            GoToNextStep();
        }

        public void OnNextButtonPressed()
        {
            ExperimentStep step = CurrentStep;
            if (step == null) return;

            if (step.showNextButton && step.requiredActionIds.Count == 0)
            {
                GoToNextStep();
            }
        }

        public bool IsStepActive(int stepIndex)
        {
            return currentStepIndex == stepIndex;
        }

        public bool IsStepActiveById(string stepId)
        {
            if (CurrentStep == null) return false;
            return CurrentStep.stepId == stepId;
        }

        public bool IsActionRequired(string actionId)
        {
            if (CurrentStep == null) return false;
            return CurrentStep.requiredActionIds.Contains(actionId);
        }

        public bool IsActionCompleted(string actionId)
        {
            return completedActions.Contains(actionId);
        }

        public void RestartExperiment()
        {
            StartExperiment();
        }

        public void JumpToStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= steps.Count) return;
            
            currentStepIndex = stepIndex - 1;
            completedActions.Clear();
            GoToNextStep();
        }
    }
}
