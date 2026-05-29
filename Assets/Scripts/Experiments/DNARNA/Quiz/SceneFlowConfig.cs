using UnityEngine;

namespace STEM.DNA_Quiz
{
    [CreateAssetMenu(fileName = "SceneFlowConfig", menuName = "STEM/Quiz/SceneFlowConfig")]
    public class SceneFlowConfig : ScriptableObject
    {
        public SceneEntry[] scenes;

        [System.Serializable]
        public class SceneEntry
        {
            public string sceneName;
            public string displayTitle;
            [Tooltip("Quiz shown after the activity. Leave null if no quiz for this scene.")]
            public QuizSet quiz;
        }
    }
}
