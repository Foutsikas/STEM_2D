using UnityEngine;
using UnityEngine.EventSystems;
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

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            world.z = 0f;

            if (draggingEnd == null && draggingKnob == null
                && mouse.leftButton.wasPressedThisFrame && !PointerOverUI())
            {
                CableEnd end = PickCableEnd(world);
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

        // Ends attached to the same node sit on top of each other, so pick deliberately.
        static CableEnd PickCableEnd(Vector3 world)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(world);
            CableEnd best = null;
            float bestDist = float.MaxValue;

            foreach (Collider2D h in hits)
            {
                CableEnd e = h.GetComponent<CableEnd>();
                if (e == null || e.locked) continue;

                float d = Vector3.Distance(world, e.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = e;
                }
            }
            return best;
        }

        static bool PointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}