using UnityEngine;

namespace STEM2D.Interactions
{
    [RequireComponent(typeof(LineRenderer))]
    public class StaticWire : MonoBehaviour
    {
        [Header("Connection Points")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        
        [Header("Wire Appearance")]
        [SerializeField] private Color wireColor = Color.red;
        [SerializeField] private float wireWidth = 0.05f;
        
        private LineRenderer lineRenderer;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            SetupLineRenderer();
        }

        void Start()
        {
            UpdateWirePositions();
        }

        void SetupLineRenderer()
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = wireWidth;
            lineRenderer.endWidth = wireWidth;
            lineRenderer.startColor = wireColor;
            lineRenderer.endColor = wireColor;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingLayerName = "Wires";
            lineRenderer.sortingOrder = 0;
        }

        void UpdateWirePositions()
        {
            if (startPoint == null || endPoint == null) return;

            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, endPoint.position);
        }

        public void SetPoints(Transform start, Transform end)
        {
            startPoint = start;
            endPoint = end;
            UpdateWirePositions();
        }

        public void SetColor(Color color)
        {
            wireColor = color;
            lineRenderer.startColor = wireColor;
            lineRenderer.endColor = wireColor;
        }

        void OnValidate()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();
            
            if (lineRenderer != null)
            {
                lineRenderer.startColor = wireColor;
                lineRenderer.endColor = wireColor;
                lineRenderer.startWidth = wireWidth;
                lineRenderer.endWidth = wireWidth;
            }
        }
    }
}
