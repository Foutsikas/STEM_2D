using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [RequireComponent(typeof(Collider2D))]
    public class PowerButton : MonoBehaviour, IMicroscopeInteractable
    {
        [SerializeField] MicroscopeState state;
        [SerializeField] SpriteRenderer indicator;
        [SerializeField] Color offColor = new Color(0.35f, 0.1f, 0.1f);
        [SerializeField] Color onColor = new Color(0.2f, 0.9f, 0.3f);

        void OnEnable()
        {
            state.Changed += Refresh;
            Refresh();
        }

        void OnDisable() => state.Changed -= Refresh;

        public void OnPress(Vector2 worldPos) => state.TogglePower();
        public void OnDrag(Vector2 worldPos) { }
        public void OnRelease(Vector2 worldPos) { }

        void Refresh()
        {
            if (indicator != null)
                indicator.color = state.Power ? onColor : offColor;
        }
    }
}
