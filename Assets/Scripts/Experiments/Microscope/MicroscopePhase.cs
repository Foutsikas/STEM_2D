using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [CreateAssetMenu(menuName = "STEM/Microscope/Phase", fileName = "Phase")]
    public class MicroscopePhase : ScriptableObject
    {
        [Tooltip("Max 3 lines on screen.")]
        [TextArea(2, 3)] public string instruction;
        public PhaseGoal goal;

        [Header("ClickPart")]
        public PartId part;

        [Header("PlaceSlide")]
        public SlideData slide;

        [Header("SelectObjective / Focus")]
        public int objectiveIndex;

        [Header("FindTarget")]
        public string targetKind;
        public int minObjective;

        [Header("Shown when done")]
        [TextArea(2, 4)] public string successText;
    }
}
