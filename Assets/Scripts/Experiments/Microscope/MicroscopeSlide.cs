using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [RequireComponent(typeof(Collider2D))]
    public class MicroscopeSlide : MonoBehaviour, IMicroscopeInteractable
    {
        [SerializeField] SlideData data;
        [SerializeField] MicroscopeState state;
        [SerializeField] Collider2D stageZone;
        [SerializeField] Transform stageAnchor;
        [Tooltip("Side view: the slide is seen edge-on when it lies on the stage.")]
        [SerializeField] Vector3 stageScale = new Vector3(0.3f, 0.04f, 1f);
        [SerializeField] GameObject trayLabel;

        SpriteRenderer sr;
        Vector3 trayPos;
        Vector3 trayScale;
        int baseOrder;
        Vector2 grabOffset;

        public SlideData Data => data;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            trayPos = transform.position;
            trayScale = transform.localScale;
            baseOrder = sr != null ? sr.sortingOrder : 0;
        }

        public void OnPress(Vector2 worldPos)
        {
            if (state.PlacedSlide == this)
                state.SetSlide(null);

            transform.localScale = trayScale;
            grabOffset = (Vector2)transform.position - worldPos;
            if (sr != null) sr.sortingOrder = baseOrder + 50;
            if (trayLabel != null) trayLabel.SetActive(false);
        }

        public void OnDrag(Vector2 worldPos)
        {
            transform.position = worldPos + grabOffset;
        }

        public void OnRelease(Vector2 worldPos)
        {
            if (stageZone.OverlapPoint(worldPos))
                PlaceOnStage();
            else
                ReturnToTray();
        }

        void PlaceOnStage()
        {
            if (state.PlacedSlide != null && state.PlacedSlide != this)
                state.PlacedSlide.ReturnToTray();

            transform.position = stageAnchor.position;
            transform.localScale = stageScale;
            if (sr != null) sr.sortingOrder = baseOrder + 5;
            state.SetSlide(this);
        }

        public void ReturnToTray()
        {
            transform.position = trayPos;
            transform.localScale = trayScale;
            if (sr != null) sr.sortingOrder = baseOrder;
            if (trayLabel != null) trayLabel.SetActive(true);
        }
    }
}
