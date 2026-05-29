using UnityEngine;
using UnityEngine.SceneManagement;

namespace STEM.DNA_Quiz
{
    public class GameProgressManager : MonoBehaviour
    {
        public static GameProgressManager Instance { get; private set; }

        [SerializeField] private SceneFlowConfig flowConfig;

        private int currentSceneIndex = 0;
        private bool[] completedScenes;

        public SceneFlowConfig FlowConfig => flowConfig;
        public int CurrentSceneIndex => currentSceneIndex;
        public int TotalScenes => flowConfig != null ? flowConfig.scenes.Length : 0;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (flowConfig != null)
                completedScenes = new bool[flowConfig.scenes.Length];
        }

        public SceneFlowConfig.SceneEntry GetCurrentEntry()
        {
            if (flowConfig == null || currentSceneIndex >= flowConfig.scenes.Length)
                return null;
            return flowConfig.scenes[currentSceneIndex];
        }

        public QuizSet GetCurrentQuiz()
        {
            var entry = GetCurrentEntry();
            return entry?.quiz;
        }

        public void MarkCurrentCompleted()
        {
            if (completedScenes != null && currentSceneIndex < completedScenes.Length)
                completedScenes[currentSceneIndex] = true;
        }

        public void LoadNextScene()
        {
            currentSceneIndex++;

            if (currentSceneIndex >= flowConfig.scenes.Length)
            {
                Debug.Log("All scenes completed!");
                return;
            }

            string nextScene = flowConfig.scenes[currentSceneIndex].sceneName;
            SceneManager.LoadScene(nextScene);
        }

        public void RestartFlow()
        {
            currentSceneIndex = 0;
            completedScenes = new bool[flowConfig.scenes.Length];
            SceneManager.LoadScene(flowConfig.scenes[0].sceneName);
        }

        public void SetSceneIndex(int index)
        {
            if (index >= 0 && index < flowConfig.scenes.Length)
            {
                currentSceneIndex = index;
                SceneManager.LoadScene(flowConfig.scenes[index].sceneName);
            }
        }
    }
}
