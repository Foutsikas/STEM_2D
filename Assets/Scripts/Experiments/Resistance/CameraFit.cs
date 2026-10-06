using UnityEngine;

namespace STEM.Experiments.Resistance
{
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        public float halfWidth = 8.6f;
        public float baseSize = 5.4f;

        Camera cam;
        float lastAspect;

        void Awake() { cam = GetComponent<Camera>(); }

        void LateUpdate()
        {
            if (Mathf.Approximately(cam.aspect, lastAspect)) return;
            lastAspect = cam.aspect;
            cam.orthographicSize = Mathf.Max(baseSize, halfWidth / cam.aspect);
        }
    }
}