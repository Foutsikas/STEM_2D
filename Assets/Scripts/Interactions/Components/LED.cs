using UnityEngine;

namespace STEM2D.Interactions
{
    public class LED : MonoBehaviour
    {
        [Header("LED Settings")]
        [SerializeField] private string ledId;
        [SerializeField] private Color ledColor = Color.red;
        
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer ledRenderer;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite onSprite;
        
        [Header("Brightness")]
        [SerializeField] private float currentBrightness = 0f;
        [SerializeField] private float minBrightness = 0.2f;
        [SerializeField] private float maxBrightness = 1f;
        
        [Header("Glow Effect")]
        [SerializeField] private bool useGlow = true;
        [SerializeField] private float glowScale = 1.5f;
        [SerializeField] private float glowAlpha = 0.5f;
        
        [Header("Animation")]
        [SerializeField] private float brightnessChangeSpeed = 5f;

        private float targetBrightness = 0f;
        private Color baseColor;
        private bool isOn = false;

        public string LEDId => ledId;
        public bool IsOn => isOn;
        public float Brightness => currentBrightness;

        void Awake()
        {
            if (string.IsNullOrEmpty(ledId))
            {
                ledId = gameObject.name;
            }

            if (ledRenderer == null)
            {
                ledRenderer = GetComponent<SpriteRenderer>();
            }

            baseColor = ledColor;
        }

        void Start()
        {
            SetBrightness(0f);
        }

        void Update()
        {
            // Smooth brightness transition
            if (!Mathf.Approximately(currentBrightness, targetBrightness))
            {
                currentBrightness = Mathf.MoveTowards(currentBrightness, targetBrightness, brightnessChangeSpeed * Time.deltaTime);
                UpdateVisuals();
            }
        }

        public void TurnOn()
        {
            isOn = true;
            SetBrightness(maxBrightness);
        }

        public void TurnOff()
        {
            isOn = false;
            SetBrightness(0f);
        }

        public void Toggle()
        {
            if (isOn)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        public void SetBrightness(float brightness)
        {
            targetBrightness = Mathf.Clamp01(brightness);
            isOn = targetBrightness > 0.01f;
        }

        public void SetBrightnessImmediate(float brightness)
        {
            currentBrightness = Mathf.Clamp01(brightness);
            targetBrightness = currentBrightness;
            isOn = currentBrightness > 0.01f;
            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            if (ledRenderer == null) return;

            // Calculate color based on brightness
            float effectiveBrightness = currentBrightness * (maxBrightness - minBrightness) + (currentBrightness > 0.01f ? minBrightness : 0f);
            
            Color currentColor = baseColor * effectiveBrightness;
            currentColor.a = 1f;
            
            // Apply to LED
            if (currentBrightness > 0.01f)
            {
                ledRenderer.color = currentColor;
                if (onSprite != null) ledRenderer.sprite = onSprite;
            }
            else
            {
                ledRenderer.color = new Color(baseColor.r * 0.3f, baseColor.g * 0.3f, baseColor.b * 0.3f, 1f);
                if (offSprite != null) ledRenderer.sprite = offSprite;
            }

            // Update glow
            if (glowRenderer != null && useGlow)
            {
                if (currentBrightness > 0.1f)
                {
                    glowRenderer.enabled = true;
                    Color glowColor = baseColor;
                    glowColor.a = glowAlpha * currentBrightness;
                    glowRenderer.color = glowColor;
                    glowRenderer.transform.localScale = Vector3.one * (1f + (glowScale - 1f) * currentBrightness);
                }
                else
                {
                    glowRenderer.enabled = false;
                }
            }
        }

        public void SetColor(Color color)
        {
            baseColor = color;
            ledColor = color;
            UpdateVisuals();
        }

        public void Pulse(float duration, float intensity = 1f)
        {
            StartCoroutine(PulseCoroutine(duration, intensity));
        }

        private System.Collections.IEnumerator PulseCoroutine(float duration, float intensity)
        {
            float originalBrightness = currentBrightness;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float pulse = Mathf.Sin(t * Mathf.PI) * intensity;
                SetBrightnessImmediate(originalBrightness + pulse);
                yield return null;
            }
            
            SetBrightness(originalBrightness);
        }
    }
}
