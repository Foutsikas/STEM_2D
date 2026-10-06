using UnityEngine;

namespace STEM.Experiments.Resistance
{
    public class CircuitSwitch : MonoBehaviour
    {
        public bool closed;
        public SpriteRenderer graphic;
        public Sprite openSprite;
        public Sprite closedSprite;

        public void Toggle()
        {
            closed = !closed;
            Apply();
            ConnectionManager.Instance.Evaluate();
        }

        public void SetClosed(bool value)
        {
            closed = value;
            Apply();
        }

        void Apply()
        {
            if (graphic == null) return;
            if (openSprite != null && closedSprite != null)
                graphic.sprite = closed ? closedSprite : openSprite;

            graphic.color = closed ? new Color(0.25f, 0.75f, 0.35f) : new Color(0.75f, 0.2f, 0.2f);
        }
    }
}