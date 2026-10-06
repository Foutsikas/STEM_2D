using UnityEngine;

namespace STEM.Experiments.Resistance
{
    public class CircuitNode : MonoBehaviour
    {
        public NodeId nodeId;
        public SpriteRenderer highlight;

        void Start()
        {
            SetHighlight(false);
        }

        public void SetHighlight(bool on)
        {
            if (highlight != null) highlight.enabled = on;
        }
    }
}
