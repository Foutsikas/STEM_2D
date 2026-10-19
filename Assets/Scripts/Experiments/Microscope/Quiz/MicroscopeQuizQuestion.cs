using UnityEngine;

namespace STEM.Experiments.Microscope
{
    [CreateAssetMenu(menuName = "STEM/Microscope/Quiz Question", fileName = "Question")]
    public class MicroscopeQuizQuestion : ScriptableObject
    {
        [TextArea(2, 4)] public string question;
        [Tooltip("The first answer is the correct one. Order is shuffled at runtime.")]
        public string[] answers = new string[4];
        [TextArea(2, 4)] public string explanation;
    }
}
