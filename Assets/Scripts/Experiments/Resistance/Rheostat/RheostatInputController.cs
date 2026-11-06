using UnityEngine;
using UnityEngine.InputSystem;
using STEM.Experiments.Resistance;

namespace STEM.Experiments.Rheostat
{
    public class RheostatInputController : MonoBehaviour
    {
        Camera cam;
        CableEnd draggingEnd;
        Knob draggingKnob;

        void Awake()
        {
            cam = Camera.main;
        }

        void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || cam == null) return;

            Vector3 world = CircuitPointer.ToWorld(cam, mouse.position.ReadValue());

            if (draggingEnd == null && draggingKnob == null
                && mouse.leftButton.wasPressedThisFrame && !CircuitPointer.OverUI())
            {
                CableEnd end = CircuitPointer.PickCableEnd(world);
                if (end != null)
                {
                    draggingEnd = end;
                    end.BeginDrag(world);
                    return;
                }

                Collider2D hit = Physics2D.OverlapPoint(world);
                if (hit != null)
                {
                    Knob k = hit.GetComponent<Knob>();
                    if (k != null)
                    {
                        draggingKnob = k;
                        k.BeginDrag(world);
                        return;
                    }

                    CircuitSwitch sw = hit.GetComponent<CircuitSwitch>();
                    if (sw != null) { sw.Toggle(); return; }
                }
            }

            if (draggingKnob != null)
            {
                if (mouse.leftButton.isPressed)
                {
                    draggingKnob.Drag(world);
                }
                else
                {
                    draggingKnob.EndDrag();
                    draggingKnob = null;
                }
                return;
            }

            if (draggingEnd != null)
            {
                if (mouse.leftButton.isPressed)
                {
                    draggingEnd.Drag(world);
                }
                else
                {
                    draggingEnd.EndDrag();
                    draggingEnd = null;
                }
            }
        }
    }
}