using UnityEngine;
using UnityEngine.SceneManagement;

namespace JungleRun
{
    public class StageControls : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "JungleRun_Main";

        public void PressPlay()
        {
            if (UIManager.Instance != null) StartCoroutine(UIManager.Instance.FadeToScene(nextSceneName));
            else SceneManager.LoadScene(nextSceneName);
        }
    }
}
