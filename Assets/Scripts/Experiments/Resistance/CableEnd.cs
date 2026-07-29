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
        SpriteRenderer glow;

        void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            var g = new GameObject("Glow");
            g.transform.SetParent(transform, false);
            g.transform.localScale = Vector3.one * 2.0f;

            glow = g.AddComponent<SpriteRenderer>();
            glow.sprite = sr != null ? sr.sprite : null;
            glow.color = new Color(1f, 0.9f, 0.2f, 0.5f);
            glow.sortingOrder = (sr != null ? sr.sortingOrder : 8) - 1;
            glow.enabled = false;
        }

        // A loose, not-yet-connected end is the one the student must drag.
        void LateUpdate()
        {
            if (glow == null) return;

            bool needsConnect = !locked && Node == null;
            glow.enabled = needsConnect;

            if (needsConnect)
            {
                float a = 0.35f + 0.3f * Mathf.Abs(Mathf.Sin(Time.time * 3f));
                Color c = glow.color;
                c.a = a;
                glow.color = c;
            }
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