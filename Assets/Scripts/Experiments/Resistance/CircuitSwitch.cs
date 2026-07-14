using UnityEngine;

namespace STEM.Experiments.Resistance
{
    [RequireComponent(typeof(Collider2D))]
    public class CircuitSwitch : MonoBehaviour
    {
        public bool closed;
        public SpriteRenderer graphic;
        public Sprite openSprite;
        public Sprite closedSprite;

        void OnMouseDown()
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
            graphic.sprite = closed ? closedSprite : openSprite;
        }
    }
}
