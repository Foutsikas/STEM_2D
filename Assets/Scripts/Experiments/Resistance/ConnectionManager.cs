using System;
using System.Collections.Generic;
using UnityEngine;

namespace STEM.Experiments.Resistance
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }

        public List<Cable> cables = new List<Cable>();
        public List<CircuitNode> nodes = new List<CircuitNode>();
        public CircuitSwitch circuitSwitch;

        public CircuitResult Result { get; private set; }
        public event Action<CircuitResult> OnCircuitChanged;

        void Awake()
        {
            Instance = this;
        }

        public CircuitNode FindNode(NodeId id)
        {
            foreach (var n in nodes)
                if (n.nodeId == id) return n;
            return null;
        }

        public CircuitNode NearestNode(Vector3 position, float radius)
        {
            CircuitNode best = null;
            float bestDist = radius;

            foreach (var n in nodes)
            {
                float d = Vector3.Distance(position, n.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = n;
                }
            }
            return best;
        }

        public void HighlightNodes(bool on)
        {
            foreach (var n in nodes) n.SetHighlight(on);
        }

        public void Evaluate()
        {
            var pairs = new List<(NodeId, NodeId)>();
            foreach (var c in cables)
                if (c.gameObject.activeInHierarchy && c.IsComplete) pairs.Add(c.Pair);

            bool closed = circuitSwitch != null && circuitSwitch.closed;
            Result = ResistanceCircuit.Solve(pairs, closed);
            OnCircuitChanged?.Invoke(Result);
        }
    }
}
