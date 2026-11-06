using UnityEngine;
using UnityEngine.SceneManagement;

namespace JungleRun
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsGameActive { get; private set; }
        public bool IsPaused { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private TrackSpawner trackSpawner;
        [SerializeField] private CameraController camController;

        public PlayerController Player => player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            IsGameActive = true;
            IsPaused = false;

            if (player == null)
                player = Object.FindAnyObjectByType<PlayerController>();

            if (trackSpawner == null)
                trackSpawner = Object.FindAnyObjectByType<TrackSpawner>();

            if (camController == null)
                camController = Object.FindAnyObjectByType<CameraController>();

            ScoreManager.Instance?.ResetScore();
            player?.ResetPlayer();
            trackSpawner?.ResetTrack();
            JungleEnvironment.Instance?.ResetEnvironment();
            UIManager.Instance?.ShowHUD();
            AudioManager.Instance?.PlayBGM("JungleExpedition");
        }

        public void GameOver()
        {
            if (!IsGameActive) return;
            IsGameActive = false;

            camController?.TriggerShake(0.4f, 0.4f);
            UIManager.Instance?.TriggerCollisionFeedback();
            ScoreManager.Instance?.SaveBestScore();
            // Trigger the impact before changing the UI or stopping the music.
            AudioManager.Instance?.PlaySFX("PlayerCrash");
            UIManager.Instance?.ShowGameOver();
            AudioManager.Instance?.StopBGM();
        }

        public void TogglePause()
        {
            if (!IsGameActive) return;

            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;

            if (IsPaused)
                AudioManager.Instance?.PauseBGM();
            else
                AudioManager.Instance?.ResumeBGM();
        }

        // ── Single Retry flow: cleanly resets track, environment, player and score ──
        public void RestartGame()
        {
            Time.timeScale = 1f;
            StartGame();
        }

        public void LoadMainMenu()
        {
            RestartGame();
        }
    }
}
