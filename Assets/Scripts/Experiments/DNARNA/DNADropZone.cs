using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace STEM.Experiments.DNA
{
    public class DNADropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public MoleculeType expectedType;
        public bool isPreplaced = false;

        public bool IsOccupied { get; private set; }

        private Image backgroundImage;
        private TextMeshProUGUI questionMark;
        private Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private Color highlightColor = new Color(0.7f, 1f, 0.7f, 1f);

        [Header("Events")]
        public UnityEngine.Events.UnityEvent onCorrectDrop;
        public UnityEngine.Events.UnityEvent onIncorrectDrop;

        void Awake()
        {
            backgroundImage = GetComponent<Image>();
            questionMark = GetComponentInChildren<TextMeshProUGUI>();

            if (isPreplaced)
            {
                IsOccupied = true;
                HideQuestionMark();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (IsOccupied) return;

            GameObject dragged = eventData.pointerDrag;
            if (dragged == null) return;

            DraggableMolecule molecule = dragged.GetComponent<DraggableMolecule>();
            if (molecule == null) return;

            MoleculeType droppedType = molecule.moleculeType;

            if (droppedType == expectedType)
            {
                IsOccupied = true;
                RectTransform zoneRect = GetComponent<RectTransform>();

                if (molecule.isPoolSource)
                {
                    GameObject ghost = molecule.DragGhost;
                    if (ghost != null)
                    {
                        ghost.transform.SetParent(transform);
                        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
                        ghostRect.anchorMin = new Vector2(0.5f, 0.5f);
                        ghostRect.anchorMax = new Vector2(0.5f, 0.5f);
                        ghostRect.pivot = new Vector2(0.5f, 0.5f);
                        ghostRect.anchoredPosition = Vector2.zero;
                        ghostRect.sizeDelta = zoneRect.sizeDelta;

                        var cg = ghost.GetComponent<CanvasGroup>();
                        if (cg != null)
                        {
                            cg.alpha = 1f;
                            cg.blocksRaycasts = false;
                        }

                        molecule.ClaimGhost();
                    }

                    molecule.poolUsed = true;
                }
                else
                {
                    molecule.PlaceInZone(zoneRect);
                }

                HideQuestionMark();
                if (backgroundImage != null)
                    backgroundImage.color = Color.clear;

                // was: DNASceneManager.Instance.OnCorrectPlacement();
                if (onCorrectDrop.GetPersistentEventCount() > 0)
                    onCorrectDrop.Invoke();
                else
                    DNASceneManager.Instance.OnCorrectPlacement();
            }
            else
            {
                // was: DNASceneManager.Instance.OnIncorrectPlacement();
                if (onIncorrectDrop.GetPersistentEventCount() > 0)
                    onIncorrectDrop.Invoke();
                else
                    DNASceneManager.Instance.OnIncorrectPlacement();
            }

            ResetHighlight();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsOccupied && eventData.dragging && backgroundImage != null)
                backgroundImage.color = highlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetHighlight();
        }

        private void ResetHighlight()
        {
            if (!IsOccupied && backgroundImage != null)
                backgroundImage.color = normalColor;
        }

        private void HideQuestionMark()
        {
            if (questionMark != null)
                questionMark.gameObject.SetActive(false);
        }
    }
}