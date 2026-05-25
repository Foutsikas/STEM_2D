using UnityEngine;
using System.Collections;

namespace STEM.Experiments.Pendulum
{
    public class PhotogateRotator : MonoBehaviour
    {
        private bool hasRotated = false;

        public void RotateToSideView()
        {
            if (hasRotated) return;
            hasRotated = true;
            StartCoroutine(RotateRoutine());
        }

        private IEnumerator RotateRoutine()
        {
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.Euler(0f, 90f, 0f);
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.rotation = Quaternion.Lerp(startRot, endRot, t);
                yield return null;
            }

            transform.rotation = endRot;
        }
    }
}