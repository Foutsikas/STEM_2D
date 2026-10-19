using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace STEM.Experiments.Microscope
{
    public class MicroscopeInputController : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] MicroscopeSceneManager manager;

        IMicroscopeInteractable active;

        void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            Vector2 world = cam.ScreenToWorldPoint(pointer.position.ReadValue());

            if (pointer.press.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                Press(world);
            }
            else if (active != null && pointer.press.isPressed)
            {
                active.OnDrag(world);
            }

            if (active != null && pointer.press.wasReleasedThisFrame)
            {
                active.OnRelease(world);
                active = null;
            }
        }

        void Press(Vector2 world)
        {
            // Overlapping colliders (knob cap inside knob ring): the smallest one wins.
            Collider2D best = null;
            float bestArea = float.MaxValue;
            foreach (var hit in Physics2D.OverlapPointAll(world))
            {
                Vector3 s = hit.bounds.size;
                float area = s.x * s.y;
                if (area < bestArea)
                {
                    bestArea = area;
                    best = hit;
                }
            }
            if (best == null) return;

            var part = best.GetComponent<MicroscopePart>();
            if (part != null) manager.OnPartClicked(part);

            var view = best.GetComponent<SpecimenView>();
            if (view != null && view.TryGetUV(world, out Vector2 uv)) manager.OnSpecimenClicked(uv);

            active = best.GetComponent<IMicroscopeInteractable>();
            active?.OnPress(world);
        }
    }
}
