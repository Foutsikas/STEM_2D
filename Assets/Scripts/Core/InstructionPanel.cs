using UnityEngine;
using UnityEngine.UI;
using TMPro;
using STEM2D.Core;

namespace STEM2D.Core
{
    public class InstructionPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private Image infographicImage;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextButtonText;
        
        [Header("Panel Settings")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Next Button Text")]
        [SerializeField] private string defaultNextText = "Next";
        [SerializeField] private string continueText = "Continue";
        
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;

        private bool isVisible = true;
        private float targetAlpha = 1f;

        void Awake()
        {
            if (canvasGroup == null && panelRoot != null)
            {
                canvasGroup = panelRoot.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = panelRoot.AddComponent<CanvasGroup>();
                }
            }
            
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
            }
        }

        void Update()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                canvasGroup.interactable = canvasGroup.alpha > 0.5f;
                canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.5f;
            }
        }

        public void ShowInstruction(string text, Sprite infographic, bool showNext, bool isInfoOnly)
        {
            Show();
            
            if (instructionText != null)
            {
                instructionText.text = text;
            }

            if (infographicImage != null)
            {
                if (infographic != null)
                {
                    infographicImage.sprite = infographic;
                    infographicImage.gameObject.SetActive(true);
                    infographicImage.preserveAspect = true;
                }
                else
                {
                    infographicImage.gameObject.SetActive(false);
                }
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(showNext);
                
                if (nextButtonText != null)
                {
                    nextButtonText.text = isInfoOnly ? continueText : defaultNextText;
                }
            }
        }

        public void SetInstructionText(string text)
        {
            if (instructionText != null)
            {
                instructionText.text = text;
            }
        }

        public void SetInfographic(Sprite sprite)
        {
            if (infographicImage != null)
            {
                if (sprite != null)
                {
                    infographicImage.sprite = sprite;
                    infographicImage.gameObject.SetActive(true);
                }
                else
                {
                    infographicImage.gameObject.SetActive(false);
                }
            }
        }

        public void SetNextButtonVisible(bool visible)
        {
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(visible);
            }
        }

        public void Show()
        {
            isVisible = true;
            targetAlpha = 1f;
            
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            isVisible = false;
            targetAlpha = 0f;
        }

        void OnNextClicked()
        {
            ExperimentManager.Instance?.OnNextButtonPressed();
        }

        public void UpdateProgress(int current, int total)
        {
            // Override this if you want progress display
        }
    }
}
