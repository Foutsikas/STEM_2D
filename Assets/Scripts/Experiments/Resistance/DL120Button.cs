using UnityEngine;

namespace STEM.Experiments.Resistance
{
    // Clickable OK button region on the DL120 sprite. Handled by CircuitInputController.
    [RequireComponent(typeof(Collider2D))]
    public class DL120Button : MonoBehaviour
    {
        public DL120Panel panel;

        public void Press()
        {
            if (panel != null) panel.TryMeasure();
        }
    }
}