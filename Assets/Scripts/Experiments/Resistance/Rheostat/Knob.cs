using UnityEngine;

namespace STEM.Experiments.Rheostat
{
    public class Knob : MonoBehaviour
    {
        public float value;
        public float minAngle = 135f;
        public float maxAngle = -135f;
        public float travel = 2f;
        public Transform pointer;

        public System.Action<float> OnValueChanged;

        float lastY;

        void Start() { Apply(); }

        public void BeginDrag(Vector3 world)
        {
            lastY = world.y;
        }

        public void Drag(Vector3 world)
        {
            float dy = world.y - lastY;
            lastY = world.y;

            float v = Mathf.Clamp01(value + dy / travel);
            if (Mathf.Approximately(v, value)) return;

            value = v;
            Apply();
            if (OnValueChanged != null) OnValueChanged(value);
        }

        public void EndDrag() { }

        void Apply()
        {
            float a = Mathf.Lerp(minAngle, maxAngle, value);
            if (pointer != null) pointer.localRotation = Quaternion.Euler(0f, 0f, a);
        }
    }
}