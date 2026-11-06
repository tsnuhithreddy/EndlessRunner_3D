using UnityEngine;

namespace JungleRun
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int   CurrentScore    { get; private set; }
        public int   BestScore       { get; private set; }
        public int   CoinsCollected  { get; private set; }
        public int   GemsCollected   { get; private set; }
        public float DistanceCovered { get; private set; }

        public int CoinMultiplier { get; set; } = 1;

        // Backward compatibility
        public int RelicsCollected => CoinsCollected;

        private const string HIGH_SCORE_KEY = "JungleRun_BestScore";
        private float lastDistanceZ = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BestScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;
            if (GameManager.Instance.Player == null) return;

            float playerZ = GameManager.Instance.Player.transform.position.z;
            if (playerZ > lastDistanceZ)
            {
                float deltaDist = playerZ - lastDistanceZ;
                lastDistanceZ = playerZ;
                DistanceCovered = playerZ;

                // 2 points per meter traveled, doubled if 2X PowerUp active
                CurrentScore += Mathf.RoundToInt(deltaDist * 2.0f);
            }
        }

        public void AddCoin(int amount = 1)
        {
            int collectedAmount = amount * Mathf.Max(1, CoinMultiplier);
            CoinsCollected += collectedAmount;
            CurrentScore += collectedAmount * 20;
        }

        public void AddGem(int amount = 1)
        {
            GemsCollected += amount;
            // 150 points per red gem (doubled if 2X active)
            CurrentScore += amount * 150;
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
        }

        public void SaveBestScore()
        {
            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, BestScore);
                PlayerPrefs.Save();
            }
        }

        public void ResetScore()
        {
            CurrentScore    = 0;
            CoinsCollected  = 0;
            GemsCollected   = 0;
            DistanceCovered = 0f;
            lastDistanceZ   = 0f;
            CoinMultiplier = 1;
        }
    }
}
