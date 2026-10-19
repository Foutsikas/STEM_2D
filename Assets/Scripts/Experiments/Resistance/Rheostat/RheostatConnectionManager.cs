using System;
using System.Collections.Generic;
using UnityEngine;
using STEM.Experiments.Resistance;

namespace STEM.Experiments.Rheostat
{
    public class RheostatConnectionManager : ConnectionManager
    {
        public Knob knob;
        public RheostatSettings settings;

        public RheostatResult RheostatResult { get; private set; }
        public event Action<RheostatResult> OnRheostatChanged;

        void Start()
        {
            if (knob != null) knob.OnValueChanged += _ => Evaluate();
        }

        public override void Evaluate()
        {
            if (knob == null || settings == null) return;

            var pairs = new List<(NodeId, NodeId)>();
            foreach (var c in cables)
                if (c.gameObject.activeInHierarchy && c.IsComplete) pairs.Add(c.Pair);

            bool closed = circuitSwitch != null && circuitSwitch.closed;
            RheostatResult = RheostatCircuit.Solve(pairs, closed, knob.value, settings);

            if (OnRheostatChanged != null) OnRheostatChanged(RheostatResult);
        }
    }
}