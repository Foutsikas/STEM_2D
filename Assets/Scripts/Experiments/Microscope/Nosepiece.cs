using UnityEngine;

namespace STEM.Experiments.Microscope
{
    // Click cycles 4x -> 10x -> 40x -> 4x.
    [RequireComponent(typeof(Collider2D))]
    public class Nosepiece : MonoBehaviour, IMicroscopeInteractable
    {
        [SerializeField] MicroscopeState state;

        public void OnPress(Vector2 worldPos) => state.SetObjective(state.ObjectiveIndex + 1);
        public void OnDrag(Vector2 worldPos) { }
        public void OnRelease(Vector2 worldPos) { }
    }
}
