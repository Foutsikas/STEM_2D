using System.Collections.Generic;

namespace STEM.Experiments.Resistance
{
    public enum NodeId
    {
        BAT_POS,
        BAT_NEG,
        SW_A,
        SW_B,
        AMM_A,
        AMM_B,
        R1_A,
        R1_B,
        R2_A,
        R2_B,
        VOLT_A,
        VOLT_B
    }

    public enum Topology
    {
        Open,
        SingleR2,
        Series,
        Parallel,
        Invalid
    }

    public enum VoltmeterTarget
    {
        None,
        AcrossR1,
        AcrossR2
    }

    public enum CircuitFault
    {
        None,
        SwitchOpen,
        NoLoop,
        AmmeterBypassed,
        ShortCircuit,
        VoltmeterNotPlaced,
        Unrecognised
    }

    public struct CircuitResult
    {
        public Topology topology;
        public VoltmeterTarget voltmeter;
        public CircuitFault fault;
        public float current;
        public float voltage;
        public float loadResistance;

        public bool IsValid => fault == CircuitFault.None;
    }

    public static class ResistanceCircuit
    {
        public const float SourceVoltage = 5f;
        public const float R1 = 100f;
        public const float R2 = 50f;

        public static CircuitResult Solve(List<(NodeId, NodeId)> cables, bool switchClosed)
        {
            var result = new CircuitResult();

            if (!switchClosed)
            {
                result.topology = Topology.Open;
                result.fault = CircuitFault.SwitchOpen;
                return result;
            }

            var touched = new HashSet<NodeId>();
            foreach (var c in cables)
            {
                touched.Add(c.Item1);
                touched.Add(c.Item2);
            }

            // Pass one: nets without the ammeter joined, so we can tell if it is bypassed.
            var probe = new Dsu();
            foreach (var c in cables) probe.Union(c.Item1, c.Item2);
            probe.Union(NodeId.SW_A, NodeId.SW_B);

            if (probe.Same(NodeId.AMM_A, NodeId.AMM_B))
            {
                result.fault = CircuitFault.AmmeterBypassed;
                result.topology = Topology.Invalid;
                return result;
            }

            // Pass two: ammeter treated as an ideal wire.
            var d = new Dsu();
            foreach (var c in cables) d.Union(c.Item1, c.Item2);
            d.Union(NodeId.SW_A, NodeId.SW_B);
            d.Union(NodeId.AMM_A, NodeId.AMM_B);

            int p = d.Find(NodeId.BAT_POS);
            int n = d.Find(NodeId.BAT_NEG);

            if (p == n)
            {
                result.fault = CircuitFault.ShortCircuit;
                result.topology = Topology.Invalid;
                return result;
            }

            int r1a = d.Find(NodeId.R1_A);
            int r1b = d.Find(NodeId.R1_B);
            int r2a = d.Find(NodeId.R2_A);
            int r2b = d.Find(NodeId.R2_B);

            bool r1Live = Wired(touched, NodeId.R1_A, NodeId.R1_B) && r1a != r1b;
            bool r2Live = Wired(touched, NodeId.R2_A, NodeId.R2_B) && r2a != r2b;

            bool r1Across = r1Live && Bridges(r1a, r1b, p, n);
            bool r2Across = r2Live && Bridges(r2a, r2b, p, n);

            result.topology = Classify(r1a, r1b, r2a, r2b, p, n, r1Across, r2Across, r1Live, r2Live);

            if (result.topology == Topology.Invalid)
            {
                result.fault = CircuitFault.Unrecognised;
                return result;
            }
            if (result.topology == Topology.Open)
            {
                result.fault = CircuitFault.NoLoop;
                return result;
            }

            int va = d.Find(NodeId.VOLT_A);
            int vb = d.Find(NodeId.VOLT_B);
            bool voltWired = Wired(touched, NodeId.VOLT_A, NodeId.VOLT_B) && va != vb;

            // In the parallel case R1 and R2 span the same pair of nets, so this
            // reports AcrossR1. The voltage is identical either way.
            if (voltWired && SameNets(va, vb, r1a, r1b)) result.voltmeter = VoltmeterTarget.AcrossR1;
            else if (voltWired && SameNets(va, vb, r2a, r2b)) result.voltmeter = VoltmeterTarget.AcrossR2;
            else
            {
                result.voltmeter = VoltmeterTarget.None;
                result.fault = CircuitFault.VoltmeterNotPlaced;
                return result;
            }

            result.loadResistance = LoadResistance(result.topology);
            result.current = SourceVoltage / result.loadResistance;

            if (result.topology == Topology.Series)
                result.voltage = result.current * (result.voltmeter == VoltmeterTarget.AcrossR1 ? R1 : R2);
            else
                result.voltage = SourceVoltage;

            return result;
        }

        public static float LoadResistance(Topology t)
        {
            switch (t)
            {
                case Topology.SingleR2: return R2;
                case Topology.Series: return R1 + R2;
                case Topology.Parallel: return (R1 * R2) / (R1 + R2);
                default: return float.PositiveInfinity;
            }
        }

        static Topology Classify(int r1a, int r1b, int r2a, int r2b, int p, int n,
                                 bool r1Across, bool r2Across, bool r1Live, bool r2Live)
        {
            if (r1Across && r2Across) return Topology.Parallel;

            if (r2Across && !r1Live) return Topology.SingleR2;
            if (r1Across && !r2Live) return Topology.Invalid;

            if (r1Live && r2Live)
            {
                int mid = SharedNet(r1a, r1b, r2a, r2b);
                if (mid == -1 || mid == p || mid == n) return Topology.Invalid;

                int r1Free = (r1a == mid) ? r1b : r1a;
                int r2Free = (r2a == mid) ? r2b : r2a;

                bool spansSource = (r1Free == p && r2Free == n) || (r1Free == n && r2Free == p);
                return spansSource ? Topology.Series : Topology.Invalid;
            }

            return Topology.Open;
        }

        static bool Wired(HashSet<NodeId> touched, NodeId a, NodeId b)
        {
            return touched.Contains(a) && touched.Contains(b);
        }

        static int SharedNet(int a1, int a2, int b1, int b2)
        {
            if (a1 == b1 || a1 == b2) return a1;
            if (a2 == b1 || a2 == b2) return a2;
            return -1;
        }

        static bool Bridges(int a, int b, int p, int n)
        {
            return (a == p && b == n) || (a == n && b == p);
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

            public int Find(NodeId id) => Find((int)id);

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

            public bool Same(NodeId a, NodeId b) => Find(a) == Find(b);
        }
    }
}
