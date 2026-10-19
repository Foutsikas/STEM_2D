using System;
using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [Serializable]
    public class SlideKind
    {
        public string id;
        public string displayName;
        [Tooltip("0 = red channel (cells), 1 = green channel (regions)")]
        [Range(0, 1)] public int channel;
        [Range(0, 255)] public int value;
    }

    [CreateAssetMenu(menuName = "STEM/Microscope/Slide", fileName = "Slide")]
    public class SlideData : ScriptableObject
    {
        public string displayName;
        public Sprite sharp;
        public Sprite blurred;

        [Tooltip("Same layout as the image. Needs Read/Write, no compression, Point filter.")]
        public Texture2D labelMap;
        public SlideKind[] kinds;
        public int valueTolerance = 10;

        public SlideKind FindKind(string id)
        {
            foreach (var k in kinds)
                if (k.id == id) return k;
            return null;
        }

        public bool HasKind(Vector2 uv, string id)
        {
            var k = FindKind(id);
            return k != null && Matches(Sample(uv), k);
        }

        // First cell kind (red channel) under the point, or null.
        public SlideKind CellKindAt(Vector2 uv)
        {
            Color32 c = Sample(uv);
            foreach (var k in kinds)
                if (k.channel == 0 && Matches(c, k)) return k;
            return null;
        }

        bool Matches(Color32 c, SlideKind k)
        {
            int v = k.channel == 0 ? c.r : c.g;
            return v > 0 && Mathf.Abs(v - k.value) <= valueTolerance;
        }

        Color32 Sample(Vector2 uv)
        {
            if (labelMap == null || uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f)
                return new Color32(0, 0, 0, 0);
            int x = Mathf.Clamp((int)(uv.x * labelMap.width), 0, labelMap.width - 1);
            int y = Mathf.Clamp((int)(uv.y * labelMap.height), 0, labelMap.height - 1);
            return labelMap.GetPixel(x, y);
        }
    }
}
