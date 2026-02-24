using UnityEngine;

namespace STEM.Experiments.Pendulum
{
    public class PendulumApparatus : MonoBehaviour
    {
        [Header("Pivot and Bob")]
        public Transform pivotTransform;
        public Transform bobTransform;

        [Header("Rod Visual")]
        public LineRenderer rodRenderer;
        [Tooltip("Rod width in world units. Keep between 0.04 and 0.08 for a solid rod look.")]
        public float rodWidth = 0.05f;
        public Color rodColor = new Color(0.45f, 0.45f, 0.45f);

        [Header("Scale")]
        [Tooltip("How many Unity units equal one metre. Match this to your scene layout.")]
        public float unitsPerMetre = 3.5f;

        private float lengthMetres = 0.4f;
        private float amplitudeMetres = 0.05f;
        private float angularFrequency;
        private float elapsedTime;
        private bool swinging;

        private bool lastSidePositive;
        private bool firstFrame;

        public float Period => angularFrequency > 0f ? (2f * Mathf.PI / angularFrequency) : 0f;
        public float Frequency => Period > 0f ? 1f / Period : 0f;
        public float CurrentAngleRadians { get; private set; }
        public float CurrentDisplacementMetres => amplitudeMetres * Mathf.Cos(angularFrequency * elapsedTime);

        public event System.Action OnEquilibriumCrossing;

        private void Start()
        {
            ConfigureRod();
            RecalculatePhysics();
            PlaceBobAtRest();
        }

        private void Update()
        {
            if (!swinging) return;

            elapsedTime += Time.deltaTime;

            float angle = (amplitudeMetres / lengthMetres) * Mathf.Cos(angularFrequency * elapsedTime);
            CurrentAngleRadians = angle;

            float rodLengthUnits = lengthMetres * unitsPerMetre;
            float bobX = pivotTransform.position.x + rodLengthUnits * Mathf.Sin(angle);
            float bobY = pivotTransform.position.y - rodLengthUnits * Mathf.Cos(angle);

            bobTransform.position = new Vector3(bobX, bobY, bobTransform.position.z);
            UpdateRodVisual();
            DetectEquilibriumCrossing(angle);
        }

        private void DetectEquilibriumCrossing(float angle)
        {
            bool isPositive = angle >= 0f;

            if (!firstFrame && isPositive != lastSidePositive)
                OnEquilibriumCrossing?.Invoke();

            firstFrame = false;
            lastSidePositive = isPositive;
        }

        private void UpdateRodVisual()
        {
            if (rodRenderer == null) return;
            rodRenderer.SetPosition(0, pivotTransform.position);
            rodRenderer.SetPosition(1, bobTransform.position);
        }

        private void ConfigureRod()
        {
            if (rodRenderer == null) return;
            rodRenderer.positionCount = 2;
            rodRenderer.startWidth = rodWidth;
            rodRenderer.endWidth = rodWidth;
            rodRenderer.startColor = rodColor;
            rodRenderer.endColor = rodColor;
            rodRenderer.useWorldSpace = true;
        }

        private void RecalculatePhysics()
        {
            angularFrequency = Mathf.Sqrt(9.81f / lengthMetres);
        }

        private void PlaceBobAtRest()
        {
            if (bobTransform == null || pivotTransform == null) return;
            bobTransform.position = new Vector3(
                pivotTransform.position.x,
                pivotTransform.position.y - lengthMetres * unitsPerMetre,
                bobTransform.position.z
            );
            UpdateRodVisual();
        }

        public void SetLength(float metres)
        {
            lengthMetres = metres;
            RecalculatePhysics();
            if (!swinging) PlaceBobAtRest();
        }

        public void SetAmplitude(float metres)
        {
            amplitudeMetres = metres;
        }

        public void StartSwinging()
        {
            elapsedTime = 0f;
            firstFrame = true;
            swinging = true;
        }

        public void StopSwinging()
        {
            swinging = false;
            PlaceBobAtRest();
        }

        public bool IsSwinging => swinging;
    }
}
