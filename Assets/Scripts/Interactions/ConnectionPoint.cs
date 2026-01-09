using UnityEngine;
using UnityEngine.Events;

namespace STEM2D.Interactions
{
    public enum WireColor
    {
        Red,
        Blue,
        White,
        Black,
        Yellow,
        Green
    }

    public class ConnectionPoint : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] private string pointId;
        [SerializeField] private WireColor acceptedColor;
        [SerializeField] private bool isSource = false;
        
        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer dotRenderer;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;
        
        [Header("Connection State")]
        [SerializeField] private bool isConnected = false;
        
        [Header("Events")]
        public UnityEvent<ConnectionPoint> OnWireConnected;
        public UnityEvent<ConnectionPoint> OnWireDisconnected;

        private Color baseColor;
        private bool isPulsing = false;
        private DraggableWire connectedWire;

        public string PointId => pointId;
        public WireColor AcceptedColor => acceptedColor;
        public bool IsSource => isSource;
        public bool IsConnected => isConnected;
        public DraggableWire ConnectedWire => connectedWire;

        void Awake()
        {
            if (dotRenderer == null)
            {
                dotRenderer = GetComponent<SpriteRenderer>();
            }

            if (dotRenderer != null)
            {
                baseColor = GetColorForWireType(acceptedColor);
                dotRenderer.color = baseColor;
            }

            if (string.IsNullOrEmpty(pointId))
            {
                pointId = gameObject.name;
            }
        }

        void Update()
        {
            if (isPulsing && dotRenderer != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                dotRenderer.color = baseColor * pulse;
            }
        }

        public void StartPulsing()
        {
            isPulsing = true;
        }

        public void StopPulsing()
        {
            isPulsing = false;
            if (dotRenderer != null)
            {
                dotRenderer.color = baseColor;
            }
        }

        public bool CanAcceptWire(WireColor wireColor)
        {
            if (isConnected) return false;
            return wireColor == acceptedColor;
        }

        public bool TryConnect(DraggableWire wire)
        {
            if (wire == null) return false;
            if (isConnected) return false;
            if (wire.Color != acceptedColor) return false;

            isConnected = true;
            connectedWire = wire;
            StopPulsing();
            
            OnWireConnected?.Invoke(this);
            
            Debug.Log($"[ConnectionPoint] {pointId}: Wire connected");
            return true;
        }

        public void Disconnect()
        {
            if (!isConnected) return;

            isConnected = false;
            connectedWire = null;
            
            OnWireDisconnected?.Invoke(this);
            
            Debug.Log($"[ConnectionPoint] {pointId}: Wire disconnected");
        }

        public static Color GetColorForWireType(WireColor wireColor)
        {
            switch (wireColor)
            {
                case WireColor.Red: return new Color(0.9f, 0.2f, 0.2f);
                case WireColor.Blue: return new Color(0.2f, 0.4f, 0.9f);
                case WireColor.White: return new Color(0.95f, 0.95f, 0.95f);
                case WireColor.Black: return new Color(0.15f, 0.15f, 0.15f);
                case WireColor.Yellow: return new Color(0.95f, 0.9f, 0.2f);
                case WireColor.Green: return new Color(0.2f, 0.8f, 0.3f);
                default: return Color.gray;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = GetColorForWireType(acceptedColor);
            Gizmos.DrawWireSphere(transform.position, 0.15f);
            
            if (isSource)
            {
                Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.3f);
            }
        }
    }
}
