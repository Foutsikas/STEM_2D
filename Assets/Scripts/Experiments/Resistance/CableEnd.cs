using UnityEngine;

namespace STEM.Experiments.Resistance
{
    public class CableEnd : MonoBehaviour
    {
        [HideInInspector] public Cable cable;

        public Transform restPoint;
        public float snapRadius = 0.55f;
        public bool locked;

        public CircuitNode Node { get; private set; }

        Vector3 grabOffset;

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

        public void BeginDrag(Vector3 world)
        {
            grabOffset = transform.position - world;
            ConnectionManager.Instance.HighlightNodes(true);
        }

        public void Drag(Vector3 world)
        {
            transform.position = world + grabOffset;
        }

        public void EndDrag()
        {
            ConnectionManager.Instance.HighlightNodes(false);

            CircuitNode target = ConnectionManager.Instance.NearestNode(transform.position, snapRadius);
            if (target != null) AttachTo(target);
            else Detach();

            ConnectionManager.Instance.Evaluate();
        }
    }
}