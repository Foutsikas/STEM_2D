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

                if (molecule.isPoolSource)
                {
                    // Pool source: create a placed copy
                    GameObject placed = Instantiate(dragged, transform);
                    var placedMol = placed.GetComponent<DraggableMolecule>();
                    placedMol.isPoolSource = false;
                    placedMol.PlaceInZone(GetComponent<RectTransform>());
                }
                else
                {
                    // Loose clone: place it directly
                    molecule.PlaceInZone(GetComponent<RectTransform>());
                }

                HideQuestionMark();
                if (backgroundImage != null)
                    backgroundImage.color = Color.clear;

                DNASceneManager.Instance.OnCorrectPlacement();
            }
            else
            {
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
