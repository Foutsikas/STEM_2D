using System;
using UnityEngine;

namespace STEM.Experiments.Microscope
{
    public class MicroscopeState : MonoBehaviour
    {
        [SerializeField] MicroscopeSettings settings;

        public MicroscopeSettings Settings => settings;
        public bool Power { get; private set; }
        public float UpperBrightness { get; private set; }
        public float LowerBrightness { get; private set; }
        public float StageHeight { get; private set; }
        public Vector2 StagePos { get; private set; } = new Vector2(0.5f, 0.5f);
        public int ObjectiveIndex { get; private set; }
        public MicroscopeSlide PlacedSlide { get; private set; }

        public SlideData Slide => PlacedSlide != null ? PlacedSlide.Data : null;
        public Objective CurrentObjective => settings.objectives[ObjectiveIndex];
        public bool AtHighest => ObjectiveIndex == settings.objectives.Length - 1;
        public float FocusError => Mathf.Abs(StageHeight - CurrentObjective.focusHeight);
        public bool InFocus => Slide != null && FocusError <= CurrentObjective.tolerance;
        public bool LowerLightOn => Power && LowerBrightness >= settings.lightOnThreshold;

        // Light reaching the camera through a transparent slide.
        public float ViewBrightness =>
            Power ? Mathf.Clamp01(LowerBrightness + UpperBrightness * settings.upperLightOnSlide) : 0f;

        public event Action Changed;
        public event Action CoarseBlocked;
        public event Action Crashed;

        void Awake()
        {
            ObjectiveIndex = Mathf.Clamp(settings.startObjective, 0, settings.objectives.Length - 1);
            StageHeight = settings.startHeight;
        }

        public void TogglePower()
        {
            Power = !Power;
            Changed?.Invoke();
        }

        public void SetObjective(int index)
        {
            ObjectiveIndex = (index + settings.objectives.Length) % settings.objectives.Length;
            ClampPan();
            Changed?.Invoke();
        }

        public void SetSlide(MicroscopeSlide slide)
        {
            PlacedSlide = slide;
            Changed?.Invoke();
        }

        public void Apply(ControlType control, float drag)
        {
            switch (control)
            {
                case ControlType.CoarseFocus:
                    if (settings.lockCoarseAtHighest && AtHighest)
                    {
                        CoarseBlocked?.Invoke();
                        return;
                    }
                    MoveStage(drag * settings.coarseRate);
                    break;
                case ControlType.FineFocus:
                    MoveStage(drag * settings.fineRate);
                    break;
                case ControlType.StageX:
                    StagePos += new Vector2(drag * settings.panRate, 0f);
                    ClampPan();
                    break;
                case ControlType.StageY:
                    StagePos += new Vector2(0f, drag * settings.panRate);
                    ClampPan();
                    break;
                case ControlType.UpperBrightness:
                    UpperBrightness = Mathf.Clamp01(UpperBrightness + drag * settings.brightnessRate);
                    break;
                case ControlType.LowerBrightness:
                    LowerBrightness = Mathf.Clamp01(LowerBrightness + drag * settings.brightnessRate);
                    break;
            }
            Changed?.Invoke();
        }

        void MoveStage(float delta)
        {
            float h = Mathf.Clamp01(StageHeight + delta);
            if (AtHighest && Slide != null)
            {
                float limit = CurrentObjective.focusHeight + settings.crashMargin;
                if (h > limit)
                {
                    h = limit;
                    Crashed?.Invoke();
                }
            }
            StageHeight = h;
        }

        void ClampPan()
        {
            float zoom = CurrentObjective.zoom;
            float hx = Mathf.Min(0.5f, 0.5f / zoom);
            float hy = Mathf.Min(0.5f, 0.5f * settings.viewAspect / zoom);
            StagePos = new Vector2(
                Mathf.Clamp(StagePos.x, hx, 1f - hx),
                Mathf.Clamp(StagePos.y, hy, 1f - hy));
        }
    }
}
