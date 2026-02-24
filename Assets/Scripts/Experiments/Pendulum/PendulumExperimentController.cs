using UnityEngine;

namespace STEM.Experiments.Pendulum
{
    public class PendulumExperimentController : MonoBehaviour
    {
        [Header("Scene Components")]
        public PendulumApparatus apparatus;
        public RS108PhotogateSensor photogate;
        public PendulumDL120Display dl120Display;
        public PendulumWaveformGraph waveformGraph;
        public PendulumLT2Graph lt2Graph;
        public PendulumInstructionManager instructionManager;

        private bool experimentRunning;
        private int crossingCount;
        private float firstCrossingTime;
        private bool timingStarted;

        private float lastRecordedPeriod;
        private float lastRecordedFrequency;

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
            if (!experimentRunning) return;

            crossingCount++;

            if (crossingCount == 1)
            {
                firstCrossingTime = Time.time;
                timingStarted = true;
            }

            if (timingStarted && crossingCount > 1)
            {
                float elapsed = Time.time - firstCrossingTime;
                int halfOscillations = crossingCount - 1;
                float period = (elapsed / halfOscillations) * 2f;
                float frequency = 1f / period;

                lastRecordedPeriod = period;
                lastRecordedFrequency = frequency;

                dl120Display?.UpdateReadings(period, frequency);
            }
        }

        public void StartExperiment()
        {
            if (experimentRunning) return;

            experimentRunning = true;
            crossingCount = 0;
            timingStarted = false;
            lastRecordedPeriod = 0f;
            lastRecordedFrequency = 0f;

            apparatus?.StartSwinging();
            waveformGraph?.StartRecording();
            dl120Display?.StartMeasurement();
            dl120Display?.ResetDisplay();
            photogate?.ResetSensor();
        }

        public void StopExperiment()
        {
            if (!experimentRunning) return;

            experimentRunning = false;
            apparatus?.StopSwinging();
            waveformGraph?.StopRecording();
            dl120Display?.StopMeasurement();
        }

        public void SaveCurrentMeasurement(float lengthMetres, float massGrams)
        {
            if (lastRecordedPeriod <= 0f)
            {
                Debug.Log("No valid period measured yet. Let the pendulum swing for at least one full oscillation.");
                return;
            }

            lt2Graph?.AddPoint(lengthMetres, lastRecordedPeriod);
            instructionManager?.OnMeasurementSaved();
        }

        public void ApplyLength(float metres)
        {
            if (experimentRunning) StopExperiment();
            apparatus?.SetLength(metres);
            waveformGraph?.ClearGraph();
            dl120Display?.ResetDisplay();
        }

        public void ApplyAmplitude(float metres)
        {
            if (experimentRunning) StopExperiment();
            apparatus?.SetAmplitude(metres);
            waveformGraph?.ClearGraph();
            dl120Display?.ResetDisplay();
        }

        public float LastPeriod => lastRecordedPeriod;
        public float LastFrequency => lastRecordedFrequency;
    }
}
