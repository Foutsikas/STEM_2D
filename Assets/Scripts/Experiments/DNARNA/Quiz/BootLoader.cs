using UnityEngine;
using UnityEngine.SceneManagement;

namespace STEM.DNA_Quiz
{
    public class BootLoader : MonoBehaviour
    {
        void Start()
        {
            if (GameProgressManager.Instance != null &&
                GameProgressManager.Instance.FlowConfig != null)
            {
                SceneManager.LoadScene(
                    GameProgressManager.Instance.FlowConfig.scenes[0].sceneName);
            }
        }
    }
}