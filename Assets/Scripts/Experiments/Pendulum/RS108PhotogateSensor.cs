using UnityEngine;
using System.Collections;

namespace STEM.Experiments.Pendulum
{
    public class RS108PhotogateSensor : MonoBehaviour
    {
        [Header("References")]
        public PendulumApparatus apparatus;
        public SpriteRenderer sensorSpriteRenderer;

        [Header("Flash Settings")]
        public Color idleColor = Color.white;
        public Color activeColor = new Color(1f, 0.6f, 0f);
        public float flashDuration = 0.08f;

        private bool flashing;

        private void OnEnable()
        {
            if (apparatus != null)
                apparatus.OnEquilibriumCrossing += HandleCrossing;
        }

        private void OnDisable()
        {
            if (apparatus != null)
                apparatus.OnEquilibriumCrossing -= HandleCrossing;
        }

        private void HandleCrossing()
        {
            if (!flashing)
                StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            flashing = true;

            if (sensorSpriteRenderer != null)
                sensorSpriteRenderer.color = activeColor;

            yield return new WaitForSeconds(flashDuration);

            if (sensorSpriteRenderer != null)
                sensorSpriteRenderer.color = idleColor;

            flashing = false;
        }

        public void ResetSensor()
        {
            StopAllCoroutines();
            flashing = false;
            if (sensorSpriteRenderer != null)
                sensorSpriteRenderer.color = idleColor;
        }
    }
}
