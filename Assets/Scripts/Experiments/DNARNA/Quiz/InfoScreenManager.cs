using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace STEM.DNA_Quiz
{
    public class InfoScreenManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image infoImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button continueButton;

        public event Action OnInfoDismissed;

        void Awake()
        {
            infoPanel.SetActive(false);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        public void Show(string title, Sprite image, string description)
        {
            titleText.text = title;
            descriptionText.text = description;

            if (image != null)
            {
                infoImage.sprite = image;
                infoImage.gameObject.SetActive(true);
            }
            else
            {
                infoImage.gameObject.SetActive(false);
            }

            infoPanel.SetActive(true);
        }

        public void Show()
        {
            infoPanel.SetActive(true);
        }

        private void OnContinueClicked()
        {
            infoPanel.SetActive(false);
            OnInfoDismissed?.Invoke();
        }
    }
}
