using UnityEngine;

namespace STEM.Experiments.Resistance
{
    [RequireComponent(typeof(Collider2D))]
    public class CableEnd : MonoBehaviour
    {
        [HideInInspector] public Cable cable;

        public Transform restPoint;
        public float snapRadius = 0.6f;
        public bool locked;

        public CircuitNode Node { get; private set; }

        Camera cam;
        Vector3 grabOffset;
        bool dragging;

        void Awake()
        {
            cam = Camera.main;
        }

        public void AttachTo(CircuitNode node)
        {
            Node = node;
            if (node != null) transform.position = node.transform.position;
        }

        public void Detach()
        {
            Node = null;
            if (restPoint != null) transform.position = restPoint.position;
        }

        void OnMouseDown()
        {
            if (locked) return;
            dragging = true;
            grabOffset = transform.position - MouseWorld();
            ConnectionManager.Instance.HighlightNodes(true);
        }

        void OnMouseDrag()
        {
            if (!dragging) return;
            transform.position = MouseWorld() + grabOffset;
        }

        void OnMouseUp()
        {
            if (!dragging) return;
            dragging = false;
            ConnectionManager.Instance.HighlightNodes(false);

            CircuitNode target = ConnectionManager.Instance.NearestNode(transform.position, snapRadius);
            if (target != null) AttachTo(target);
            else Detach();

            ConnectionManager.Instance.Evaluate();
        }

        Vector3 MouseWorld()
        {
            Vector3 p = cam.ScreenToWorldPoint(Input.mousePosition);
            p.z = transform.position.z;
            return p;
        }
    }
}
