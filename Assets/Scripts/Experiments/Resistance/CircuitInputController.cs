using UnityEngine;
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

            Vector3 world = CircuitPointer.ToWorld(cam, mouse.position.ReadValue());

            if (dragging == null && mouse.leftButton.wasPressedThisFrame && !CircuitPointer.OverUI())
            {
                CableEnd end = CircuitPointer.PickCableEnd(world);
                if (end != null)
                {
                    dragging = end;
                    end.BeginDrag(world);
                    return;
                }

                Collider2D hit = Physics2D.OverlapPoint(world);
                if (hit != null)
                {
                    CircuitSwitch sw = hit.GetComponent<CircuitSwitch>();
                    if (sw != null) { sw.Toggle(); return; }

                    DL120Button ok = hit.GetComponent<DL120Button>();
                    if (ok != null) { ok.Press(); return; }
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
    }
}