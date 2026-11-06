using UnityEngine;

namespace STEM.Experiments.Pendulum
{
    [CreateAssetMenu(fileName = "PendulumStep", menuName = "STEM/Pendulum/Step")]
    public class PendulumStep : ScriptableObject
    {
        [TextArea(3, 8)] public string text;

        [Header("Panels")]
        public bool showWaveform;
        public bool showLT2;
        public bool showSave;

        [Header("Locks")]
        public bool lengthLocked;
        public bool massLocked;
        public bool amplitudeLocked;
        public bool playLocked;
        public bool stopLocked;
        public bool saveLocked;

        [Header("On enter")]
        public bool rotatePhotogate;

        [Tooltip("Go to the next step after this many saved measurements. 0 = only with the Next button.")]
        public int autoAdvanceAfterSaves;
    }
}
