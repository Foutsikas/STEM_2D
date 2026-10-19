using System;
using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [Serializable]
    public class Objective
    {
        public string label = "4x";
        public float zoom = 1f;
        [Range(0f, 1f)] public float focusHeight = 0.52f;
        public float tolerance = 0.02f;
        public float blurRange = 0.08f;
        public Color ringColor = Color.red;
    }

    [CreateAssetMenu(menuName = "STEM/Microscope/Settings", fileName = "MicroscopeSettings")]
    public class MicroscopeSettings : ScriptableObject
    {
        public Objective[] objectives;
        public int startObjective = 1;
        [Range(0f, 1f)] public float startHeight = 0.15f;

        [Tooltip("Height / width of the visible screen area.")]
        public float viewAspect = 0.537f;

        [Header("Drag rates, per world unit of vertical drag")]
        public float coarseRate = 0.4f;
        public float fineRate = 0.04f;
        public float panRate = 0.15f;
        public float brightnessRate = 0.6f;

        [Header("Rules")]
        public bool lockCoarseAtHighest = true;
        [Tooltip("At the highest objective the stage cannot rise more than this above focus.")]
        public float crashMargin = 0.03f;
        [Tooltip("How much the upper light helps a transparent slide.")]
        [Range(0f, 1f)] public float upperLightOnSlide = 0.25f;
        [Range(0f, 1f)] public float lightOnThreshold = 0.3f;
    }
}
