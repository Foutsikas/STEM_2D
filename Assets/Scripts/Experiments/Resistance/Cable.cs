using UnityEngine;

namespace STEM.Experiments.Resistance
{
    public class Cable : MonoBehaviour
    {
        public CableEnd endA;
        public CableEnd endB;
        public LineRenderer line;

        void Awake()
        {
            if (endA != null) endA.cable = this;
            if (endB != null) endB.cable = this;
        }

        public bool IsComplete => endA.Node != null && endB.Node != null;

        public (NodeId, NodeId) Pair => (endA.Node.nodeId, endB.Node.nodeId);

        void LateUpdate()
        {
            if (line == null) return;
            line.positionCount = 2;
            line.SetPosition(0, endA.transform.position);
            line.SetPosition(1, endB.transform.position);
        }
    }
}
