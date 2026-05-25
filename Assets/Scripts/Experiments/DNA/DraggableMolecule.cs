using UnityEngine;
using UnityEngine.EventSystems;

namespace STEM.Experiments.DNA
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableMolecule : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public MoleculeType moleculeType;
        public bool isPoolSource = true;

        [HideInInspector] public bool wasPlaced = false;

        private Canvas rootCanvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        // For pool sources: a visual ghost that follows the cursor
        private GameObject dragGhost;
        private RectTransform ghostRect;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isPoolSource)
            {
                dragGhost = Instantiate(gameObject, rootCanvas.transform);
                dragGhost.name = moleculeType.ToString() + "_Ghost";

                // Disable the ghost's own drag handling so it doesn't interfere
                var ghostDM = dragGhost.GetComponent<DraggableMolecule>();
                Destroy(ghostDM);

                ghostRect = dragGhost.GetComponent<RectTransform>();
                ghostRect.position = rectTransform.position;

                var ghostCG = dragGhost.GetComponent<CanvasGroup>();
                ghostCG.alpha = 0.7f;
                ghostCG.blocksRaycasts = false;

                // Pool source itself must not block raycasts so drop zones can receive
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                canvasGroup.alpha = 0.7f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isPoolSource && ghostRect != null)
            {
                ghostRect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
            }
            else if (!isPoolSource)
            {
                rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isPoolSource)
            {
                if (dragGhost != null)
                    Destroy(dragGhost);
                dragGhost = null;
                ghostRect = null;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                if (!wasPlaced)
                    Destroy(gameObject);
            }
        }

        public void PlaceInZone(RectTransform zoneRect)
        {
            wasPlaced = true;
            transform.SetParent(zoneRect);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = zoneRect.sizeDelta;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;
        }
    }
}
