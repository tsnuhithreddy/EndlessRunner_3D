using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JungleRun
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI gemsText;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private TextMeshProUGUI powerUpStatusText;

        [Header("Optional HUD")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI sectorText;

        [Header("Game Over Elements")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI finalDistanceText;
        [SerializeField] private TextMeshProUGUI finalCoinsText;
        [SerializeField] private TextMeshProUGUI finalGemsText;

        private Canvas rootCanvas;
        private CanvasGroup fadeGroup;
        private Image collisionFlash;
        private GameObject audioSettingsPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            AutoWireHUDTexts();
        }

        private void Start()
        {
            FormatGameOverModal();
            HideScoreDisplays();
            BuildFadeOverlay();
            BuildCollisionFlash();
            BuildAudioSettingsUI();
            AddButtonAudioFeedback();
            ShowHUD();
        }

        private void AutoWireHUDTexts()
        {
            if (scoreText == null && GameObject.Find("ScoreText") != null)
                scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
            if (finalScoreText == null && GameObject.Find("FinalScoreText") != null)
                finalScoreText = GameObject.Find("FinalScoreText").GetComponent<TextMeshProUGUI>();
            if (bestScoreText == null && GameObject.Find("BestScoreText") != null)
                bestScoreText = GameObject.Find("BestScoreText").GetComponent<TextMeshProUGUI>();
            if (distanceText == null && GameObject.Find("DistanceText") != null)
                distanceText = GameObject.Find("DistanceText").GetComponent<TextMeshProUGUI>();

            if (coinsText == null)
                coinsText = FindTextInChildren(hudPanel, "RelicsText");
            if (coinsText == null)
                coinsText = FindTextInChildren(hudPanel, "CoinsText");
            if (coinsText == null)
                coinsText = FindTextInLoadedScene("RelicsText");
            if (coinsText == null)
                coinsText = FindTextInLoadedScene("CoinsText");

            // Game-over panel is normally inactive, so GameObject.Find cannot see its
            // children. Search the panel hierarchy including inactive objects.
            if (finalCoinsText == null)
                finalCoinsText = FindTextInChildren(gameOverPanel, "FinalRelicsText");
            if (finalCoinsText == null)
                finalCoinsText = FindTextInChildren(gameOverPanel, "FinalCoinsText");
            if (finalCoinsText == null && gameOverPanel != null)
                finalCoinsText = CreateFinalStatText("FinalCoinsText", gameOverPanel.transform);

            if (finalDistanceText == null)
            {
                GameObject finalDistanceObject = GameObject.Find("FinalDistanceText");
                if (finalDistanceObject != null) finalDistanceText = finalDistanceObject.GetComponent<TextMeshProUGUI>();
            }

            if (powerUpStatusText == null && hudPanel != null)
            {
                GameObject statusGo = new GameObject("PowerUpStatusText");
                statusGo.transform.SetParent(hudPanel.transform, false);
                RectTransform statusRect = statusGo.AddComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0.5f, 1f);
                statusRect.anchorMax = new Vector2(0.5f, 1f);
                statusRect.pivot = new Vector2(0.5f, 1f);
                statusRect.anchoredPosition = new Vector2(0f, -102f);
                statusRect.sizeDelta = new Vector2(620f, 38f);
                powerUpStatusText = statusGo.AddComponent<TextMeshProUGUI>();
                powerUpStatusText.fontSize = 24;
                powerUpStatusText.fontStyle = FontStyles.Bold;
                powerUpStatusText.alignment = TextAlignmentOptions.Center;
                powerUpStatusText.color = new Color(1f, 0.9f, 0.2f);
                powerUpStatusText.text = "";
            }

            if (gemsText == null && hudPanel != null)
            {
                GameObject go = new GameObject("GemsText");
                go.transform.SetParent(hudPanel.transform, false);
                RectTransform rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-24f, -52f);
                rt.sizeDelta = new Vector2(220f, 34f);
                gemsText = go.AddComponent<TextMeshProUGUI>();
                gemsText.fontSize = 23;
                gemsText.alignment = TextAlignmentOptions.Right;
                gemsText.color = new Color(0.35f, 1f, 0.95f);
            }

            if (finalGemsText == null)
                finalGemsText = FindTextInChildren(gameOverPanel, "FinalGemsText");
            if (finalGemsText == null && gameOverPanel != null)
                finalGemsText = CreateFinalStatText("FinalGemsText", gameOverPanel.transform);
        }

        private TextMeshProUGUI FindTextInChildren(GameObject parent, string objectName)
        {
            if (parent == null) return null;
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == objectName)
                    return child.GetComponent<TextMeshProUGUI>();
            }
            return null;
        }

        private TextMeshProUGUI FindTextInLoadedScene(string objectName)
        {
            TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (TextMeshProUGUI candidate in allTexts)
            {
                if (candidate != null && candidate.gameObject.name == objectName && candidate.gameObject.scene.IsValid())
                    return candidate;
            }
            return null;
        }

        private TextMeshProUGUI CreateFinalStatText(string objectName, Transform parent)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = 25;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.color = Color.white;
            return text;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;
            if (ScoreManager.Instance == null) return;

            if (distanceText != null)
                distanceText.text = ScoreManager.Instance.DistanceCovered.ToString("F0") + " m";

            if (coinsText != null)
            {
                coinsText.text = $"COINS: {ScoreManager.Instance.CoinsCollected}";
                coinsText.fontSize = 26;
                coinsText.color = new Color(1f, 0.88f, 0.2f);
            }

            if (gemsText != null)
                gemsText.text = $"GEMS: {ScoreManager.Instance.GemsCollected}";

            if (powerUpStatusText != null)
            {
                string status = "";
                if (PlayerController.IsMagnetActive)
                    status += $"  MAGNET {PlayerController.MagnetRemaining:F0}s  ";
                if (PlayerController.Is2XCoinsActive)
                    status += $"  2X COINS {PlayerController.DoubleCoinsRemaining:F0}s  ";
                if (PlayerController.HasShield)
                    status += $"  SHIELD {PlayerController.ShieldRemaining:F0}s  ";

                powerUpStatusText.text = status.Trim();
                powerUpStatusText.gameObject.SetActive(!string.IsNullOrEmpty(status));
                powerUpStatusText.fontSize = 20;
                powerUpStatusText.fontStyle = FontStyles.Bold;
                powerUpStatusText.alignment = TextAlignmentOptions.Center;
            }

            if (speedText != null && GameManager.Instance.Player != null)
            {
                float kmh = GameManager.Instance.Player.CurrentSpeed * 3.6f;
                speedText.text = kmh.ToString("F0") + " km/h";
            }

            if (sectorText != null)
                UpdateSector(ScoreManager.Instance.DistanceCovered);
        }

        private void UpdateSector(float dist)
        {
            if (dist < 500f) sectorText.text = "THE SUNLIT CANOPY";
            else if (dist < 1000f) sectorText.text = "FORGOTTEN TEMPLE RUINS";
            else if (dist < 1500f) sectorText.text = "SUNKEN SWAMP BASIN";
            else sectorText.text = "SACRED SERPENT GROTTO";
        }

        public void ShowHUD()
        {
            if (hudPanel != null) hudPanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (fadeGroup != null) StartCoroutine(Fade(1f, 0f, 0.85f));
        }

        public void ShowGameOver()
        {
            AutoWireHUDTexts();
            FormatGameOverModal();

            if (hudPanel != null) hudPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            CanvasGroup panelGroup = gameOverPanel != null ? gameOverPanel.GetComponent<CanvasGroup>() : null;
            if (panelGroup == null && gameOverPanel != null) panelGroup = gameOverPanel.AddComponent<CanvasGroup>();
            if (panelGroup != null) StartCoroutine(FadePanel(panelGroup));

            if (ScoreManager.Instance == null) return;

            if (finalDistanceText != null)
                finalDistanceText.text = "DISTANCE: " + ScoreManager.Instance.DistanceCovered.ToString("F0") + " m";
            if (finalGemsText != null)
                finalGemsText.text = "GEMS COLLECTED: " + ScoreManager.Instance.GemsCollected;
            if (finalCoinsText != null)
                finalCoinsText.text = "COINS COLLECTED: " + ScoreManager.Instance.CoinsCollected;
        }

        private void FormatGameOverModal()
        {
            if (gameOverPanel == null) return;

            RectTransform panelRT = gameOverPanel.GetComponent<RectTransform>();
            if (panelRT != null)
            {
                panelRT.anchorMin = new Vector2(0.5f, 0.5f);
                panelRT.anchorMax = new Vector2(0.5f, 0.5f);
                panelRT.pivot = new Vector2(0.5f, 0.5f);
                panelRT.anchoredPosition = Vector2.zero;
                // Taller card gives each statistic its own clear row without wasting space.
                panelRT.sizeDelta = new Vector2(500f, 360f);

                Image panelImage = gameOverPanel.GetComponent<Image>();
                if (panelImage != null)
                    panelImage.color = new Color(0.035f, 0.105f, 0.045f, 0.96f);
            }

            // Score and best-score are intentionally hidden: the end card has only the
            // three requested run statistics and the retry button.
            if (finalScoreText != null) finalScoreText.gameObject.SetActive(false);
            if (bestScoreText != null) bestScoreText.gameObject.SetActive(false);

            if (finalDistanceText != null)
            {
                SetCenter(finalDistanceText.rectTransform, 0f, 112f, 450f, 48f);
                finalDistanceText.fontSize = 32;
                finalDistanceText.fontStyle = FontStyles.Bold;
                finalDistanceText.alignment = TextAlignmentOptions.Center;
                finalDistanceText.enableWordWrapping = false;
                finalDistanceText.color = Color.white;
                finalDistanceText.margin = Vector4.zero;
            }

            if (finalCoinsText != null)
            {
                finalCoinsText.gameObject.SetActive(true);
                SetCenter(finalCoinsText.rectTransform, 0f, 43f, 450f, 42f);
                finalCoinsText.fontSize = 25;
                finalCoinsText.fontStyle = FontStyles.Bold;
                finalCoinsText.alignment = TextAlignmentOptions.Center;
                finalCoinsText.enableWordWrapping = false;
                finalCoinsText.color = new Color(1f, 0.88f, 0.2f);
                finalCoinsText.margin = Vector4.zero;
            }

            if (finalGemsText != null)
            {
                finalGemsText.gameObject.SetActive(true);
                SetCenter(finalGemsText.rectTransform, 0f, -24f, 450f, 42f);
                finalGemsText.fontSize = 25;
                finalGemsText.fontStyle = FontStyles.Bold;
                finalGemsText.alignment = TextAlignmentOptions.Center;
                finalGemsText.enableWordWrapping = false;
                finalGemsText.color = new Color(0.35f, 1f, 0.95f);
                finalGemsText.margin = Vector4.zero;
            }

            Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
            bool retryConfigured = false;
            foreach (Button btn in buttons)
            {
                if (!retryConfigured)
                {
                    retryConfigured = true;
                    btn.gameObject.SetActive(true);
                    btn.name = "RetryButton";
                    SetCenter(btn.GetComponent<RectTransform>(), 0f, -112f, 320f, 58f);
                    SetButtonColor(btn, new Color(0.12f, 0.65f, 0.24f));
                    TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.text = "RETRY";
                        label.fontSize = 27;
                        label.fontStyle = FontStyles.Bold;
                        label.alignment = TextAlignmentOptions.Center;
                    }
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnRestartClicked);
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        private void HideScoreDisplays()
        {
            if (scoreText != null) scoreText.gameObject.SetActive(false);
            if (speedText != null) speedText.gameObject.SetActive(false);
            if (sectorText != null) sectorText.gameObject.SetActive(false);
            if (powerUpStatusText != null)
            {
                powerUpStatusText.text = "";
                powerUpStatusText.gameObject.SetActive(true);
            }
            if (finalScoreText != null) finalScoreText.gameObject.SetActive(false);
            if (bestScoreText != null) bestScoreText.gameObject.SetActive(false);
        }

        private void BuildFadeOverlay()
        {
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null) rootCanvas = FindFirstObjectByType<Canvas>();
            if (rootCanvas == null) return;
            GameObject overlay = new GameObject("SceneFadeOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            overlay.transform.SetParent(rootCanvas.transform, false);
            overlay.transform.SetAsLastSibling();
            RectTransform rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = Color.black;
            fadeGroup = overlay.GetComponent<CanvasGroup>();
            fadeGroup.alpha = 1f;
            fadeGroup.blocksRaycasts = false;
        }

        private System.Collections.IEnumerator Fade(float from, float to, float duration)
        {
            if (fadeGroup == null) yield break;
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.alpha = from;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            fadeGroup.alpha = to;
            if (to <= 0f) fadeGroup.gameObject.SetActive(false);
        }

        private void BuildCollisionFlash()
        {
            if (rootCanvas == null) return;
            GameObject go = new GameObject("CollisionFlash", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(rootCanvas.transform, false);
            go.transform.SetAsLastSibling();
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            collisionFlash = go.GetComponent<Image>();
            collisionFlash.color = new Color(1f, 0.08f, 0.02f, 0f);
            collisionFlash.raycastTarget = false;
        }

        public void TriggerCollisionFeedback()
        {
            if (collisionFlash != null) StartCoroutine(CollisionFlashRoutine());
        }

        private System.Collections.IEnumerator CollisionFlashRoutine()
        {
            float t = 0f;
            while (t < 0.28f)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Sin((t / 0.28f) * Mathf.PI) * 0.30f;
                collisionFlash.color = new Color(1f, 0.08f, 0.02f, alpha);
                yield return null;
            }
            collisionFlash.color = new Color(1f, 0.08f, 0.02f, 0f);
        }

        private System.Collections.IEnumerator FadePanel(CanvasGroup group)
        {
            group.alpha = 0f;
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(t / 0.35f);
                yield return null;
            }
            group.alpha = 1f;
        }

        public System.Collections.IEnumerator FadeToScene(string sceneName)
        {
            yield return Fade(0f, 1f, 0.35f);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        private void BuildAudioSettingsUI()
        {
            if (rootCanvas == null) return;
            GameObject toggle = new GameObject("AudioSettingsButton", typeof(RectTransform), typeof(Image), typeof(Button));
            toggle.transform.SetParent(rootCanvas.transform, false);
            RectTransform tr = toggle.GetComponent<RectTransform>();
            tr.anchorMin = tr.anchorMax = new Vector2(1f, 0f);
            tr.pivot = new Vector2(1f, 0f);
            tr.anchoredPosition = new Vector2(-22f, 20f);
            tr.sizeDelta = new Vector2(118f, 42f);
            toggle.GetComponent<Image>().color = new Color(0.08f, 0.32f, 0.16f, 0.95f);
            TextMeshProUGUI toggleText = MakeLabel("AUDIO", toggle.transform, 18, Color.white);
            toggleText.alignment = TextAlignmentOptions.Center;
            toggleText.rectTransform.anchorMin = Vector2.zero;
            toggleText.rectTransform.anchorMax = Vector2.one;
            toggleText.rectTransform.offsetMin = Vector2.zero;
            toggleText.rectTransform.offsetMax = Vector2.zero;
            toggle.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (audioSettingsPanel != null) audioSettingsPanel.SetActive(!audioSettingsPanel.activeSelf);
            });

            audioSettingsPanel = new GameObject("AudioSettingsPanel", typeof(RectTransform), typeof(Image));
            audioSettingsPanel.transform.SetParent(rootCanvas.transform, false);
            RectTransform panel = audioSettingsPanel.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(460f, 330f);
            audioSettingsPanel.GetComponent<Image>().color = new Color(0.025f, 0.09f, 0.045f, 0.97f);
            MakeLabel("AUDIO SETTINGS", audioSettingsPanel.transform, 27, Color.white).rectTransform.anchoredPosition = new Vector2(0f, 125f);

            AudioManager audio = AudioManager.Instance;
            CreateVolumeSlider("MASTER", 70f, audio != null ? audio.MasterVolume : 1f, v => audio?.SetMasterVolume(v));
            CreateVolumeSlider("MUSIC", 0f, audio != null ? audio.MusicVolume : 0.7f, v => audio?.SetMusicVolume(v));
            CreateVolumeSlider("SFX", -70f, audio != null ? audio.SfxVolume : 0.8f, v => audio?.SetSfxVolume(v));
            audioSettingsPanel.SetActive(false);
        }

        private void CreateVolumeSlider(string label, float y, float value, System.Action<float> onChanged)
        {
            if (audioSettingsPanel == null) return;
            TextMeshProUGUI title = MakeLabel(label, audioSettingsPanel.transform, 20, Color.white);
            title.alignment = TextAlignmentOptions.Left;
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            title.rectTransform.pivot = new Vector2(0f, 0.5f);
            title.rectTransform.anchoredPosition = new Vector2(35f, y);
            title.rectTransform.sizeDelta = new Vector2(100f, 35f);

            GameObject sliderGo = new GameObject(label + "Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(audioSettingsPanel.transform, false);
            RectTransform sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchorMin = sliderRect.anchorMax = new Vector2(0f, 0.5f);
            sliderRect.pivot = new Vector2(0f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(145f, y);
            sliderRect.sizeDelta = new Vector2(245f, 24f);

            Slider slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            slider.onValueChanged.AddListener(valueChanged => onChanged(valueChanged));

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(sliderGo.transform, false);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.16f, 0.22f, 0.17f);
            slider.targetGraphic = background.GetComponent<Image>();

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-5f, 0f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.2f, 0.75f, 0.3f);
            slider.fillRect = fillRect;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(sliderGo.transform, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 30f);
            handle.GetComponent<Image>().color = Color.white;
            slider.handleRect = handleRect;
            slider.direction = Slider.Direction.LeftToRight;
        }

        private TextMeshProUGUI MakeLabel(string text, Transform parent, float size, Color color)
        {
            GameObject go = new GameObject(text + "Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private void AddButtonAudioFeedback()
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button button in buttons)
            {
                if (button.GetComponent<ButtonAudioFeedback>() == null)
                    button.gameObject.AddComponent<ButtonAudioFeedback>();
            }
        }

        private void SetCenter(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        private void SetButtonColor(Button button, Color color)
        {
            Image image = button.GetComponent<Image>();
            if (image == null) image = button.gameObject.AddComponent<Image>();
            image.color = color;
        }

        public void OnPauseClicked()
        {
            GameManager.Instance?.TogglePause();
            if (pausePanel != null && GameManager.Instance != null)
                pausePanel.SetActive(GameManager.Instance.IsPaused);
        }

        public void OnRestartClicked()
        {
            StartCoroutine(RestartAfterFade());
        }

        private System.Collections.IEnumerator RestartAfterFade()
        {
            yield return Fade(0f, 1f, 0.28f);
            GameManager.Instance?.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            OnRestartClicked();
        }
    }
}
