using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace JungleRun
{
    public class LoadToStage : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float waitTime = 3.0f;
        [SerializeField] private string targetSceneName = "JungleRun_Main";
        [SerializeField] private TextMeshProUGUI loadingText;

        private void Start()
        {
            StartCoroutine(WaitAndLoadRoutine());
        }

        private IEnumerator WaitAndLoadRoutine()
        {
            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                elapsed += Time.deltaTime;
                if (loadingText != null)
                {
                    int dots = (Mathf.FloorToInt(elapsed * 2f) % 3) + 1;
                    loadingText.text = "LOADING" + new string('.', dots);
                }
                yield return null;
            }

            SceneManager.LoadScene(targetSceneName);
        }
    }
}
