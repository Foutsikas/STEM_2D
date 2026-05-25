using UnityEngine;
using UnityEngine.UI;

namespace STEM.Experiments.DNA
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class DNABackboneLine : Graphic
    {
        public RectTransform pointA;
        public RectTransform pointB;
        public float thickness = 3f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (pointA == null || pointB == null) return;

            Vector2 centerA = GetLocalPoint(pointA);
            Vector2 centerB = GetLocalPoint(pointB);

            Vector2 halfA = pointA.rect.size * 0.5f;
            Vector2 halfB = pointB.rect.size * 0.5f;

            Vector2 posA = GetEdgePoint(centerA, centerB, halfA);
            Vector2 posB = GetEdgePoint(centerB, centerA, halfB);

            Vector2 dir = (posB - posA).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;

            vert.position = posA - normal;
            vh.AddVert(vert);
            vert.position = posA + normal;
            vh.AddVert(vert);
            vert.position = posB + normal;
            vh.AddVert(vert);
            vert.position = posB - normal;
            vh.AddVert(vert);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }

        private Vector2 GetEdgePoint(Vector2 from, Vector2 to, Vector2 halfSize)
        {
            Vector2 dir = to - from;
            if (dir == Vector2.zero) return from;

            float scaleX = halfSize.x / Mathf.Abs(dir.x);
            float scaleY = halfSize.y / Mathf.Abs(dir.y);
            float t = Mathf.Min(
                dir.x != 0 ? scaleX : float.MaxValue,
                dir.y != 0 ? scaleY : float.MaxValue
            );

            return from + dir * t;
        }

        private Vector2 GetLocalPoint(RectTransform target)
        {
            Vector3 worldPos = target.position;
            return rectTransform.InverseTransformPoint(worldPos);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}