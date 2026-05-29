using UnityEngine;

namespace STEM.DNA_Quiz
{
    [CreateAssetMenu(fileName = "NewQuestion", menuName = "STEM/Quiz/Question")]
    public class QuizQuestion : ScriptableObject
    {
        [TextArea(2, 4)]
        public string questionText;

        public string[] answers = new string[4];

        [Tooltip("Zero-based index of the correct answer")]
        [Range(0, 3)]
        public int correctAnswerIndex;
    }
}
