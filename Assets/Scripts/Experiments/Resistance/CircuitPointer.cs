using UnityEngine;
using UnityEngine.EventSystems;

namespace STEM.Experiments.Resistance
{
    // Pointer helpers shared by the resistance and rheostat input controllers.
    public static class CircuitPointer
    {
        public static Vector3 ToWorld(Camera cam, Vector2 screen)
        {
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            world.z = 0f;
            return world;
        }

        public static bool OverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        // Ends attached to the same node sit on top of each other, so choose the nearest free one.
        public static CableEnd PickCableEnd(Vector3 world)
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
    }
}
