using UnityEngine;

namespace STEM.Experiments.Microscope
{
    // Serialized as int. Append only, never reorder.
    public enum PartId
    {
        None,
        Screen,
        PowerButton,
        Nosepiece,
        Stage,
        SlideClip,
        StageKnobs,
        CoarseFocus,
        FineFocus,
        UpperLight,
        LowerLight,
        UpperBrightness,
        LowerBrightness,
        Arm,
        Base
    }

    public enum ControlType
    {
        CoarseFocus,
        FineFocus,
        StageX,
        StageY,
        UpperBrightness,
        LowerBrightness
    }

    public enum PhaseGoal
    {
        ClickPart,
        PowerOn,
        LowerLightOn,
        SelectObjective,
        PlaceSlide,
        Focus,
        FindTarget
    }

    public interface IMicroscopeInteractable
    {
        void OnPress(Vector2 worldPos);
        void OnDrag(Vector2 worldPos);
        void OnRelease(Vector2 worldPos);
    }
}
