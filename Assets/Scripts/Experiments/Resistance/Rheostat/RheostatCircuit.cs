using System.Collections.Generic;
using STEM.Experiments.Resistance;

namespace STEM.Experiments.Rheostat
{
    public enum RheostatFault
    {
        None,
        SwitchOpen,
        ShortCircuit,
        PotMissing,
        LampMissing,
        PotBypassed,
        LampBypassed,
        NoLoop,
        VoltmeterNotPlaced,
        Unrecognised
    }

    public struct RheostatResult
    {
        public RheostatFault fault;
        public float resistance;
        public float current;
        public float lampVoltage;
        public float brightness;

        public bool IsValid { get { return fault == RheostatFault.None; } }
    }

    public static class RheostatCircuit
    {
        public static RheostatResult Solve(List<(NodeId, NodeId)> cables, bool switchClosed,
                                           float knob, RheostatSettings s)
        {
            var r = new RheostatResult();

            if (!switchClosed)
            {
                r.fault = RheostatFault.SwitchOpen;
                return r;
            }

            var touched = new HashSet<NodeId>();
            foreach (var c in cables)
            {
                touched.Add(c.Item1);
                touched.Add(c.Item2);
            }

            var d = new Dsu();
            foreach (var c in cables) d.Union(c.Item1, c.Item2);
            d.Union(NodeId.PS_1, NodeId.PS_3);
            d.Union(NodeId.PS_2, NodeId.PS_4);

            int p = d.Find(NodeId.BAT_POS);
            int n = d.Find(NodeId.BAT_NEG);

            if (p == n)
            {
                r.fault = RheostatFault.ShortCircuit;
                return r;
            }

            if (!touched.Contains(NodeId.PS_5) || !touched.Contains(NodeId.PS_6))
            {
                r.fault = RheostatFault.PotMissing;
                return r;
            }
            if (!touched.Contains(NodeId.PS_7) || !touched.Contains(NodeId.PS_8))
            {
                r.fault = RheostatFault.LampMissing;
                return r;
            }

            int pa = d.Find(NodeId.PS_5);
            int pb = d.Find(NodeId.PS_6);
            int la = d.Find(NodeId.PS_7);
            int lb = d.Find(NodeId.PS_8);

            if (pa == pb) { r.fault = RheostatFault.PotBypassed; return r; }
            if (la == lb) { r.fault = RheostatFault.LampBypassed; return r; }

            int mid = SharedNet(pa, pb, la, lb);
            if (mid == -1 || mid == p || mid == n)
            {
                r.fault = RheostatFault.NoLoop;
                return r;
            }

            int potFree = (pa == mid) ? pb : pa;
            int lampFree = (la == mid) ? lb : la;

            bool spans = (potFree == p && lampFree == n) || (potFree == n && lampFree == p);
            if (!spans)
            {
                r.fault = RheostatFault.NoLoop;
                return r;
            }

            int va = d.Find(NodeId.VOLT_A);
            int vb = d.Find(NodeId.VOLT_B);
            bool voltWired = touched.Contains(NodeId.VOLT_A)
                             && touched.Contains(NodeId.VOLT_B)
                             && va != vb;

            if (!voltWired || !SameNets(va, vb, la, lb))
            {
                r.fault = RheostatFault.VoltmeterNotPlaced;
                return r;
            }

            r.resistance = s.Resistance(knob);
            r.current = s.Current(knob);
            r.lampVoltage = s.LampVoltage(knob);
            r.brightness = s.Brightness(knob);
            return r;
        }

        static int SharedNet(int a1, int a2, int b1, int b2)
        {
            if (a1 == b1 || a1 == b2) return a1;
            if (a2 == b1 || a2 == b2) return a2;
            return -1;
        }

        static bool SameNets(int a1, int a2, int b1, int b2)
        {
            return (a1 == b1 && a2 == b2) || (a1 == b2 && a2 == b1);
        }

        class Dsu
        {
            readonly int[] parent;

            public Dsu()
            {
                int count = System.Enum.GetValues(typeof(NodeId)).Length;
                parent = new int[count];
                for (int i = 0; i < count; i++) parent[i] = i;
            }

            public int Find(NodeId id) { return Find((int)id); }

            int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }
                return x;
            }

            public void Union(NodeId a, NodeId b)
            {
                int ra = Find((int)a);
                int rb = Find((int)b);
                if (ra != rb) parent[ra] = rb;
            }
        }
    }
}