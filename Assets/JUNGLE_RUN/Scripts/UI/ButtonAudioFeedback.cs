using UnityEngine;
using UnityEngine.UI;

namespace JungleRun
{
    [RequireComponent(typeof(Button))]
    public class ButtonAudioFeedback : MonoBehaviour
    {
        private Button button;
        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(PlayClick);
        }
        private void PlayClick()
        {
            AudioManager.Instance?.PlaySFX("ButtonSelect");
        }
        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(PlayClick);
        }
    }
}
