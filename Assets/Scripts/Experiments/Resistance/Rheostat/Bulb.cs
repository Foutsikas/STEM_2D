using UnityEngine;

namespace STEM.Experiments.Rheostat
{
    public class Bulb : MonoBehaviour
    {
        public SpriteRenderer glow;
        public Color litColor = new Color(1f, 0.92f, 0.55f, 1f);
        public float minScale = 0.9f;
        public float maxScale = 1.8f;

        public void SetBrightness(float b)
        {
            if (glow == null) return;

            b = Mathf.Clamp01(b);
            glow.enabled = b > 0.01f;

            Color c = litColor;
            c.a = b;
            glow.color = c;
            glow.transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, b);
        }
    }
}