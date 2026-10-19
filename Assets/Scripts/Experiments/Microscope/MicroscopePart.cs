using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [RequireComponent(typeof(Collider2D))]
    public class MicroscopePart : MonoBehaviour
    {
        [SerializeField] PartId id;
        [SerializeField] string displayName;

        public PartId Id => id;
        public string DisplayName => displayName;
    }
}
