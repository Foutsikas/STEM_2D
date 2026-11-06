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

        // Hides every cable, then shows and attaches the ones listed in the phase.
        public void ApplyPresets(IEnumerable<CablePreset> presets)
        {
            foreach (Cable c in cables)
            {
                c.endA.Detach();
                c.endB.Detach();
                c.endA.locked = true;
                c.endB.locked = true;
                c.gameObject.SetActive(false);
            }

            foreach (CablePreset p in presets)
            {
                Cable c = cables[p.cableIndex];
                c.gameObject.SetActive(true);

                if (p.endAAttached) c.endA.AttachTo(RequireNode(p.endANode, p.cableIndex, "A"));
                if (p.endBAttached) c.endB.AttachTo(RequireNode(p.endBNode, p.cableIndex, "B"));

                c.endA.locked = p.endALocked;
                c.endB.locked = p.endBLocked;
            }
        }

        CircuitNode RequireNode(NodeId id, int cableIndex, string end)
        {
            CircuitNode node = FindNode(id);
            if (node == null)
                Debug.LogError($"Cable preset: no node {id} for cable {cableIndex} end {end}", this);
            return node;
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

        public virtual void Evaluate()
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
