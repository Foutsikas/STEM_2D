using UnityEngine;

namespace STEM.DNA_Quiz
{
    [CreateAssetMenu(fileName = "NewQuizSet", menuName = "STEM/Quiz/QuizSet")]
    public class QuizSet : ScriptableObject
    {
        public string quizTitle;
        public QuizQuestion[] questions;

        [Tooltip("Minimum correct answers to pass. Set to questions.Length for all correct.")]
        [Min(1)]
        public int requiredCorrect = 1;
    }
}
