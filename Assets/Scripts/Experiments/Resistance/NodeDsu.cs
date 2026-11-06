namespace STEM.Experiments.Resistance
{
    // Union-find over the circuit nodes. Shared by the resistance and rheostat solvers.
    public class NodeDsu
    {
        readonly int[] parent;

        public NodeDsu()
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
