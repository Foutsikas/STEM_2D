using UnityEngine;

namespace STEM.DNA_Quiz
{
    public class ActivityQuizBridge : MonoBehaviour
    {
        public void OnActivityComplete()
        {
            if (QuizManager.Instance != null)
                QuizManager.Instance.StartQuiz();
            else
                Debug.LogWarning("QuizManager not found in scene.");
        }
    }
}
