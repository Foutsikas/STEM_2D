using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace STEM.Experiments.Resistance
{
    public class CircuitInputController : MonoBehaviour
    {
        Camera cam;
        CableEnd dragging;

        void Awake()
        {
            cam = Camera.main;
        }

        void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || cam == null) return;

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            world.z = 0f;

            if (dragging == null && mouse.leftButton.wasPressedThisFrame && !PointerOverUI())
            {
                Collider2D hit = Physics2D.OverlapPoint(world);
                if (hit != null)
                {
                    CableEnd end = hit.GetComponent<CableEnd>();
                    if (end != null && !end.locked)
                    {
                        dragging = end;
                        end.BeginDrag(world);
                        return;
                    }

                    CircuitSwitch sw = hit.GetComponent<CircuitSwitch>();
                    if (sw != null) sw.Toggle();
                }
            }

            if (dragging != null)
            {
                if (mouse.leftButton.isPressed)
                {
                    dragging.Drag(world);
                }
                else
                {
                    dragging.EndDrag();
                    dragging = null;
                }
            }
        }

        static bool PointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}