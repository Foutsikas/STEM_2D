using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LampGlow : MonoBehaviour
    {
        [SerializeField] MicroscopeState state;
        [SerializeField] bool lower = true;
        [SerializeField] float maxAlpha = 0.8f;

        SpriteRenderer sr;

        void Awake() => sr = GetComponent<SpriteRenderer>();

        void OnEnable()
        {
            state.Changed += Refresh;
            Refresh();
        }

        void OnDisable() => state.Changed -= Refresh;

        void Refresh()
        {
            float b = state.Power ? (lower ? state.LowerBrightness : state.UpperBrightness) : 0f;
            Color c = sr.color;
            c.a = b * maxAlpha;
            sr.color = c;
        }
    }
}
