using TMPro;
using UnityEngine;

namespace STEM.Experiments.Microscope
{
    // The microscope screen. Zoom = objective, pan = stage, blur = focus error.
    [RequireComponent(typeof(BoxCollider2D))]
    public class SpecimenView : MonoBehaviour
    {
        [SerializeField] MicroscopeState state;
        [SerializeField] Vector2 viewSize = new Vector2(4.3f, 2.31f);
        [SerializeField] SpriteRenderer emptyField;
        [SerializeField] SpriteRenderer blurred;
        [SerializeField] SpriteRenderer sharp;
        [SerializeField] TextMeshPro objectiveLabel;
        [SerializeField] float brightnessGamma = 0.7f;

        float imageWidth;
        float imageHeight;

        void OnEnable()
        {
            state.Changed += Refresh;
            Refresh();
        }

        void OnDisable() => state.Changed -= Refresh;

        void Refresh()
        {
            var obj = state.CurrentObjective;
            var slide = state.Slide;
            float b = Mathf.Pow(state.ViewBrightness, brightnessGamma);
            Color lit = new Color(b, b, b, 1f);

            emptyField.color = state.Power ? lit : Color.black;

            bool show = state.Power && slide != null;
            sharp.enabled = show;
            blurred.enabled = show;

            if (objectiveLabel != null)
            {
                objectiveLabel.enabled = state.Power;
                objectiveLabel.text = obj.label;
                objectiveLabel.color = obj.ringColor;
            }

            if (!show) return;

            sharp.sprite = slide.sharp;
            blurred.sprite = slide.blurred;

            imageWidth = viewSize.x * obj.zoom;
            float scale = imageWidth / slide.sharp.bounds.size.x;
            imageHeight = slide.sharp.bounds.size.y * scale;

            Vector2 offset = state.StagePos - new Vector2(0.5f, 0.5f);
            Vector3 pos = new Vector3(-offset.x * imageWidth, -offset.y * imageHeight, 0f);
            foreach (var r in new[] { sharp, blurred })
            {
                r.transform.localPosition = pos;
                r.transform.localScale = new Vector3(scale, scale, 1f);
            }

            float err = state.FocusError;
            float range = obj.blurRange;
            float sharpAlpha = 1f - Mathf.Clamp01(err / range);
            float blurAlpha = 1f - Mathf.Clamp01((err - range) / (2f * range));

            sharp.color = new Color(b, b, b, sharpAlpha);
            blurred.color = new Color(b, b, b, blurAlpha);
        }

        public bool TryGetUV(Vector2 worldPos, out Vector2 uv)
        {
            uv = default;
            if (state.Slide == null || imageWidth <= 0f) return false;

            Vector2 local = transform.InverseTransformPoint(worldPos);
            if (Mathf.Abs(local.x) > viewSize.x * 0.5f || Mathf.Abs(local.y) > viewSize.y * 0.5f)
                return false;

            uv = state.StagePos + new Vector2(local.x / imageWidth, local.y / imageHeight);
            return true;
        }
    }
}
