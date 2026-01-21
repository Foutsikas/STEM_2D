using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    public class CircuitManager : MonoBehaviour
    {
        [Header("Circuit Settings")]
        [SerializeField] private string circuitId;
        
        [Header("Required Connections")]
        [Tooltip("All wires that must be connected for circuit to be complete")]
        [SerializeField] private List<DraggableWire> requiredWires = new List<DraggableWire>();
        
        [Header("Required Switches")]
        [Tooltip("Switches that must be ON for circuit to be active")]
        [SerializeField] private List<Switch> requiredSwitches = new List<Switch>();
        
        [Header("Action Registration")]
        [SerializeField] private string actionIdOnCircuitComplete;
        [SerializeField] private string actionIdOnCircuitActive;
        
        [Header("Controlled Elements")]
        [Tooltip("Elements that turn on when circuit is active")]
        [SerializeField] private List<GameObject> controlledElements = new List<GameObject>();
        
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer circuitIndicator;
        [SerializeField] private Color incompleteColor = Color.gray;
        [SerializeField] private Color completeColor = Color.yellow;
        [SerializeField] private Color activeColor = Color.green;
        
        [Header("Events")]
        public UnityEvent OnCircuitComplete;
        public UnityEvent OnCircuitIncomplete;
        public UnityEvent OnCircuitActive;
        public UnityEvent OnCircuitInactive;

        private bool isComplete = false;
        private bool isActive = false;
        private bool hasRegisteredComplete = false;
        private bool hasRegisteredActive = false;

        public string CircuitId => circuitId;
        public bool IsComplete => isComplete;
        public bool IsActive => isActive;

        void Awake()
        {
            if (string.IsNullOrEmpty(circuitId))
            {
                circuitId = gameObject.name;
            }
        }

        void Start()
        {
            SubscribeToEvents();
            CheckCircuitState();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        void SubscribeToEvents()
        {
            foreach (DraggableWire wire in requiredWires)
            {
                if (wire != null)
                {
                    wire.OnConnected.AddListener(OnWireConnectionChanged);
                    wire.OnDisconnected.AddListener(OnWireDisconnected);
                }
            }

            foreach (Switch sw in requiredSwitches)
            {
                if (sw != null)
                {
                    sw.OnStateChanged.AddListener(OnSwitchStateChanged);
                }
            }
        }

        void UnsubscribeFromEvents()
        {
            foreach (DraggableWire wire in requiredWires)
            {
                if (wire != null)
                {
                    wire.OnConnected.RemoveListener(OnWireConnectionChanged);
                    wire.OnDisconnected.RemoveListener(OnWireDisconnected);
                }
            }

            foreach (Switch sw in requiredSwitches)
            {
                if (sw != null)
                {
                    sw.OnStateChanged.RemoveListener(OnSwitchStateChanged);
                }
            }
        }

        void OnWireConnectionChanged(ConnectionPoint point)
        {
            CheckCircuitState();
        }

        void OnWireDisconnected()
        {
            CheckCircuitState();
        }

        void OnSwitchStateChanged(bool state)
        {
            CheckCircuitState();
        }

        void CheckCircuitState()
        {
            bool allWiresConnected = true;
            foreach (DraggableWire wire in requiredWires)
            {
                if (wire != null && !wire.IsConnected)
                {
                    allWiresConnected = false;
                    break;
                }
            }

            bool previousComplete = isComplete;
            isComplete = allWiresConnected;

            if (isComplete && !previousComplete)
            {
                OnCircuitComplete?.Invoke();
                
                if (!hasRegisteredComplete && !string.IsNullOrEmpty(actionIdOnCircuitComplete))
                {
                    ExperimentManager.Instance?.RegisterActionComplete(actionIdOnCircuitComplete);
                    hasRegisteredComplete = true;
                }
                
                Debug.Log($"[Circuit] {circuitId}: Complete");
            }
            else if (!isComplete && previousComplete)
            {
                OnCircuitIncomplete?.Invoke();
                Debug.Log($"[Circuit] {circuitId}: Incomplete");
            }

            bool allSwitchesOn = true;
            foreach (Switch sw in requiredSwitches)
            {
                if (sw != null && !sw.IsOn)
                {
                    allSwitchesOn = false;
                    break;
                }
            }

            bool previousActive = isActive;
            isActive = isComplete && allSwitchesOn;

            if (isActive && !previousActive)
            {
                OnCircuitActive?.Invoke();
                SetControlledElementsActive(true);
                
                if (!hasRegisteredActive && !string.IsNullOrEmpty(actionIdOnCircuitActive))
                {
                    ExperimentManager.Instance?.RegisterActionComplete(actionIdOnCircuitActive);
                    hasRegisteredActive = true;
                }
                
                Debug.Log($"[Circuit] {circuitId}: Active");
            }
            else if (!isActive && previousActive)
            {
                OnCircuitInactive?.Invoke();
                SetControlledElementsActive(false);
                Debug.Log($"[Circuit] {circuitId}: Inactive");
            }

            UpdateVisuals();
        }

        void SetControlledElementsActive(bool active)
        {
            foreach (GameObject element in controlledElements)
            {
                if (element != null)
                {
                    element.SetActive(active);
                }
            }
        }

        void UpdateVisuals()
        {
            if (circuitIndicator == null) return;

            if (isActive)
            {
                circuitIndicator.color = activeColor;
            }
            else if (isComplete)
            {
                circuitIndicator.color = completeColor;
            }
            else
            {
                circuitIndicator.color = incompleteColor;
            }
        }

        public void ResetCircuit()
        {
            foreach (DraggableWire wire in requiredWires)
            {
                if (wire != null)
                {
                    wire.Disconnect();
                }
            }

            foreach (Switch sw in requiredSwitches)
            {
                if (sw != null)
                {
                    sw.Reset();
                }
            }

            hasRegisteredComplete = false;
            hasRegisteredActive = false;
            CheckCircuitState();
        }

        public float GetCompletionPercentage()
        {
            if (requiredWires.Count == 0) return 1f;

            int connected = 0;
            foreach (DraggableWire wire in requiredWires)
            {
                if (wire != null && wire.IsConnected)
                {
                    connected++;
                }
            }

            return (float)connected / requiredWires.Count;
        }
    }
}
