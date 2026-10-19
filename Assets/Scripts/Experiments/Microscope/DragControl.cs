using UnityEngine;

namespace STEM.Experiments.Microscope
{
    // Vertical drag. Up = positive.
    [RequireComponent(typeof(Collider2D))]
    public class DragControl : MonoBehaviour, IMicroscopeInteractable
    {
        [SerializeField] MicroscopeState state;
        [SerializeField] ControlType control;
        [SerializeField] float sensitivity = 1f;

        [Tooltip("Optional. Sits at the knob centre, the visible tick is an offset child.")]
        [SerializeField] Transform pointer;
        [SerializeField] float degreesPerUnit = 120f;

        float lastY;

        public void OnPress(Vector2 worldPos) => lastY = worldPos.y;

        public void OnDrag(Vector2 worldPos)
        {
            float delta = worldPos.y - lastY;
            lastY = worldPos.y;
            if (Mathf.Approximately(delta, 0f)) return;

            state.Apply(control, delta * sensitivity);
            if (pointer != null)
                pointer.Rotate(0f, 0f, -delta * degreesPerUnit);
        }

        public void OnRelease(Vector2 worldPos) { }
    }
}
