using UnityEngine;

namespace STEM2D.Interactions
{
    public class Lamp : MonoBehaviour
    {
        [Header("Lamp Settings")]
        [SerializeField] private string lampId;
        
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer lampRenderer;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Color offColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color onColor = Color.yellow;
        
        [Header("Glow Effect")]
        [SerializeField] private bool useGlow = true;
        [SerializeField] private float glowPulseSpeed = 2f;
        [SerializeField] private float glowPulseAmount = 0.2f;
        
        [Header("State")]
        [SerializeField] private bool startOn = false;

        private bool isOn = false;
        private Color baseGlowColor;

        public string LampId => lampId;
        public bool IsOn => isOn;

        void Awake()
        {
            if (string.IsNullOrEmpty(lampId))
            {
                lampId = gameObject.name;
            }

            if (lampRenderer == null)
            {
                lampRenderer = GetComponent<SpriteRenderer>();
            }

            if (glowRenderer != null)
            {
                baseGlowColor = onColor;
                baseGlowColor.a = 0.5f;
            }
        }

        void Start()
        {
            SetState(startOn);
        }

        void Update()
        {
            if (isOn && useGlow && glowRenderer != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * glowPulseSpeed) * glowPulseAmount;
                Color glowColor = baseGlowColor * pulse;
                glowColor.a = baseGlowColor.a * pulse;
                glowRenderer.color = glowColor;
            }
        }

        public void TurnOn()
        {
            SetState(true);
        }

        public void TurnOff()
        {
            SetState(false);
        }

        public void Toggle()
        {
            SetState(!isOn);
        }

        public void SetState(bool on)
        {
            isOn = on;
            UpdateVisuals();
            Debug.Log($"[Lamp] {lampId}: {(isOn ? "ON" : "OFF")}");
        }

        void UpdateVisuals()
        {
            if (lampRenderer != null)
            {
                if (isOn)
                {
                    lampRenderer.color = onColor;
                    if (onSprite != null) lampRenderer.sprite = onSprite;
                }
                else
                {
                    lampRenderer.color = offColor;
                    if (offSprite != null) lampRenderer.sprite = offSprite;
                }
            }

            if (glowRenderer != null)
            {
                glowRenderer.enabled = isOn && useGlow;
            }
        }

        public void SetBrightness(float brightness)
        {
            brightness = Mathf.Clamp01(brightness);
            
            if (lampRenderer != null)
            {
                Color targetColor = Color.Lerp(offColor, onColor, brightness);
                lampRenderer.color = targetColor;
            }

            if (glowRenderer != null)
            {
                Color glowColor = baseGlowColor;
                glowColor.a = baseGlowColor.a * brightness;
                glowRenderer.color = glowColor;
                glowRenderer.enabled = brightness > 0.1f && useGlow;
            }

            isOn = brightness > 0.5f;
        }
    }
}
