using UnityEngine;
using UnityEngine.Events;
using STEM2D.Core;

namespace STEM2D.Interactions
{
    [RequireComponent(typeof(LineRenderer))]
    public class DraggableWire : MonoBehaviour, IInteractable
    {
        [Header("Wire Settings")]
        [SerializeField] private string wireId;
        [SerializeField] private WireColor wireColor;
        [SerializeField] private ConnectionPoint sourcePoint;
        
        [Header("Action Registration")]
        [Tooltip("Action ID to register when wire is fully connected")]
        [SerializeField] private string completionActionId;

        [Header("Line Settings")]
        [SerializeField] private float lineWidth = 0.08f;
        [SerializeField] private int curveSegments = 20;
        [SerializeField] private float sagAmount = 0.5f;
        
        [Header("Snap Settings")]
        [SerializeField] private float snapDistance = 0.5f;
        [SerializeField] private LayerMask connectionPointLayer;

        [Header("Events")]
        public UnityEvent OnDragStarted;
        public UnityEvent OnDragEnded;
        public UnityEvent<ConnectionPoint> OnConnected;
        public UnityEvent OnDisconnected;

        private LineRenderer lineRenderer;
        private bool isDragging = false;
        private bool isConnected = false;
        private bool isInteractable = true;
        private Vector3 dragEndPosition;
        private ConnectionPoint targetPoint;
        private Camera mainCamera;

        public string WireId => wireId;
        public WireColor Color => wireColor;
        public bool IsConnected => isConnected;
        public ConnectionPoint SourcePoint => sourcePoint;
        public ConnectionPoint TargetPoint => targetPoint;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            mainCamera = Camera.main;
            
            SetupLineRenderer();
            
            if (string.IsNullOrEmpty(wireId))
            {
                wireId = gameObject.name;
            }
        }

        void Start()
        {
            if (sourcePoint != null)
            {
                transform.position = sourcePoint.transform.position;
                dragEndPosition = sourcePoint.transform.position;
            }
            
            UpdateLinePositions();
        }

        void SetupLineRenderer()
        {
            lineRenderer.positionCount = curveSegments;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;
            
            Color color = ConnectionPoint.GetColorForWireType(wireColor);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        void Update()
        {
            if (isDragging)
            {
                UpdateDragPosition();
                CheckForSnapTarget();
            }
            
            UpdateLinePositions();
        }

        void OnMouseDown()
        {
            if (!isInteractable) return;
            if (isConnected) return;

            isDragging = true;
            OnDragStarted?.Invoke();
            
            HighlightValidTargets(true);
        }

        void OnMouseUp()
        {
            if (!isDragging) return;

            isDragging = false;
            OnDragEnded?.Invoke();
            
            HighlightValidTargets(false);
            
            TryConnect();
        }

        void UpdateDragPosition()
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            dragEndPosition = mousePos;
        }

        void CheckForSnapTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(dragEndPosition, snapDistance, connectionPointLayer);
            
            ConnectionPoint nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                ConnectionPoint point = hit.GetComponent<ConnectionPoint>();
                if (point != null && point.CanAcceptWire(wireColor) && !point.IsSource)
                {
                    float dist = Vector2.Distance(dragEndPosition, point.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = point;
                    }
                }
            }

            if (nearest != null)
            {
                dragEndPosition = nearest.transform.position;
            }
        }

        void TryConnect()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(dragEndPosition, snapDistance, connectionPointLayer);

            foreach (Collider2D hit in hits)
            {
                ConnectionPoint point = hit.GetComponent<ConnectionPoint>();
                if (point != null && point.CanAcceptWire(wireColor) && !point.IsSource)
                {
                    if (point.TryConnect(this))
                    {
                        targetPoint = point;
                        isConnected = true;
                        dragEndPosition = point.transform.position;
                        
                        OnConnected?.Invoke(point);
                        
                        if (!string.IsNullOrEmpty(completionActionId))
                        {
                            ExperimentManager.Instance?.RegisterActionComplete(completionActionId);
                        }
                        
                        Debug.Log($"[Wire] {wireId}: Connected to {point.PointId}");
                        return;
                    }
                }
            }

            ResetToSource();
        }

        void ResetToSource()
        {
            if (sourcePoint != null)
            {
                dragEndPosition = sourcePoint.transform.position;
            }
        }

        public void Disconnect()
        {
            if (!isConnected) return;

            if (targetPoint != null)
            {
                targetPoint.Disconnect();
                targetPoint = null;
            }

            isConnected = false;
            ResetToSource();
            
            OnDisconnected?.Invoke();
            Debug.Log($"[Wire] {wireId}: Disconnected");
        }

        void UpdateLinePositions()
        {
            if (sourcePoint == null) return;

            Vector3 start = sourcePoint.transform.position;
            Vector3 end = dragEndPosition;

            for (int i = 0; i < curveSegments; i++)
            {
                float t = (float)i / (curveSegments - 1);
                Vector3 point = Vector3.Lerp(start, end, t);
                
                float sag = Mathf.Sin(t * Mathf.PI) * sagAmount;
                point.y -= sag;
                
                lineRenderer.SetPosition(i, point);
            }
        }

        void HighlightValidTargets(bool highlight)
        {
            ConnectionPoint[] allPoints = FindObjectsByType<ConnectionPoint>(FindObjectsSortMode.None);
            
            foreach (ConnectionPoint point in allPoints)
            {
                if (point.IsSource) continue;
                if (!point.CanAcceptWire(wireColor)) continue;

                if (highlight)
                {
                    point.StartPulsing();
                }
                else
                {
                    point.StopPulsing();
                }
            }
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public bool CanInteract()
        {
            return isInteractable && !isConnected;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = ConnectionPoint.GetColorForWireType(wireColor);
            
            if (sourcePoint != null)
            {
                Gizmos.DrawLine(sourcePoint.transform.position, transform.position);
            }
            
            Gizmos.DrawWireSphere(transform.position, snapDistance);
        }
    }
}
