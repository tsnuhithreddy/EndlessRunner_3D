using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JungleRun
{
    /// <summary>
    /// Auto-corrects HUD layout positions and GameOver modal styling at runtime.
    /// Runs on Start and also whenever GameOver panel is shown.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class HUDLayout : MonoBehaviour
    {
        private void Start()
        {
            FixHUDPositions();
        }

        private void OnEnable()
        {
            FixHUDPositions();
        }

        public void FixHUDPositions()
        {
            // ── HUD Panel (Full Stretch) ──
            RectTransform hudPanel = FindRectTransform("HUD_Panel");
            if (hudPanel != null)
            {
                hudPanel.anchorMin = Vector2.zero;
                hudPanel.anchorMax = Vector2.one;
                hudPanel.offsetMin = Vector2.zero;
                hudPanel.offsetMax = Vector2.zero;
            }

            // ── Live Score Text (Top-Left) ──
            RectTransform scoreRT = FindRectTransform("ScoreText");
            if (scoreRT != null)
            {
                SetTopLeft(scoreRT, 30f, -30f, 320f, 50f);
                var tmp = scoreRT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize  = 32;
                    tmp.alignment = TextAlignmentOptions.TopLeft;
                    tmp.color     = Color.white;
                }
            }

            // ── Live Distance Text (Below Score, Top-Left) ──
            RectTransform distRT = FindRectTransform("DistanceText");
            if (distRT != null)
            {
                SetTopLeft(distRT, 30f, -85f, 250f, 40f);
                var tmp = distRT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize  = 24;
                    tmp.alignment = TextAlignmentOptions.TopLeft;
                    tmp.color     = new Color(0.85f, 1f, 0.7f);
                }
            }

            // ── Coins Text (Top-Right) ──
            RectTransform coinsRT = FindRectTransform("CoinsText");
            if (coinsRT != null)
            {
                SetTopRight(coinsRT, -30f, -30f, 180f, 45f);
                var tmp = coinsRT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize  = 28;
                    tmp.alignment = TextAlignmentOptions.TopRight;
                    tmp.color     = new Color(1f, 0.84f, 0f);
                }
            }

            // ── Gems Text (Top-Right below Coins) ──
            RectTransform gemsRT = FindRectTransform("GemsText");
            if (gemsRT != null)
            {
                SetTopRight(gemsRT, -30f, -80f, 180f, 45f);
                var tmp = gemsRT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize  = 26;
                    tmp.alignment = TextAlignmentOptions.TopRight;
                    tmp.color     = new Color(0.3f, 0.9f, 1f);
                }
            }

            // ── Game Over Panel (Centered Modal Window) ──
            RectTransform goPanel = FindRectTransform("GameOver_Panel");
            if (goPanel != null)
            {
                goPanel.anchorMin        = new Vector2(0.5f, 0.5f);
                goPanel.anchorMax        = new Vector2(0.5f, 0.5f);
                goPanel.pivot            = new Vector2(0.5f, 0.5f);
                goPanel.anchoredPosition = Vector2.zero;
                goPanel.sizeDelta        = new Vector2(500f, 360f);

                Image img = goPanel.GetComponent<Image>();
                if (img == null) img = goPanel.gameObject.AddComponent<Image>();
                img.color = new Color(0.04f, 0.12f, 0.05f, 0.94f); // Rich dark jungle backdrop
            }

            // ── Game Over Texts ──
            RectTransform finalScore = FindRectTransform("FinalScoreText");
            if (finalScore != null)
            {
                CenterAnchor(finalScore, 0f, 140f, 400f, 60f);
                var tmp = finalScore.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.fontSize = 46; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white; }
            }

            RectTransform bestScore = FindRectTransform("BestScoreText");
            if (bestScore != null)
            {
                CenterAnchor(bestScore, 0f, 85f, 400f, 40f);
                var tmp = bestScore.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.fontSize = 24; tmp.alignment = TextAlignmentOptions.Center; tmp.color = new Color(1f, 0.85f, 0.2f); }
            }

            RectTransform finalDist = FindRectTransform("FinalDistanceText");
            if (finalDist != null)
            {
                CenterAnchor(finalDist, 0f, 112f, 450f, 48f);
                var tmp = finalDist.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.fontSize = 32; tmp.alignment = TextAlignmentOptions.Center; tmp.color = new Color(0.8f, 0.9f, 0.8f); }
            }

            // ── Final collected items: separate rows with generous spacing ──
            RectTransform finalCoins = FindRectTransform("FinalRelicsText");
            if (finalCoins != null)
            {
                CenterAnchor(finalCoins, 0f, 43f, 450f, 42f);
                var tmp = finalCoins.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.fontSize = 25; tmp.alignment = TextAlignmentOptions.Center; tmp.enableWordWrapping = false; }
            }

            RectTransform finalGems = FindRectTransform("FinalGemsText");
            if (finalGems != null)
            {
                CenterAnchor(finalGems, 0f, -24f, 450f, 42f);
                var tmp = finalGems.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.fontSize = 25; tmp.alignment = TextAlignmentOptions.Center; tmp.enableWordWrapping = false; }
            }

            // ── Game Over Buttons (Proper vertical stacking & margins) ──
            RectTransform restartBtn = FindRectTransform("RestartButton");
            RectTransform menuBtn    = FindRectTransform("MainMenuButton");

            if (restartBtn != null)
            {
                CenterAnchor(restartBtn, 0f, -112f, 320f, 58f);
                StyleButton(restartBtn, "RE-ENTER JUNGLE", new Color(0.12f, 0.65f, 0.24f));
            }

            if (menuBtn != null)
            {
                CenterAnchor(menuBtn, 0f, -160f, 340f, 60f);
                StyleButton(menuBtn, "BASE CAMP", new Color(0.55f, 0.35f, 0.12f));
            }
        }

        private void SetTopLeft(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta        = new Vector2(w, h);
        }

        private void SetTopRight(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin        = new Vector2(1f, 1f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta        = new Vector2(w, h);
        }

        private void CenterAnchor(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta        = new Vector2(w, h);
        }

        private void StyleButton(RectTransform rt, string label, Color bgColor)
        {
            Image img = rt.GetComponent<Image>();
            if (img == null) img = rt.gameObject.AddComponent<Image>();
            img.color = bgColor;

            var tmp = rt.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text      = label;
                tmp.fontSize  = 22;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color     = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        private RectTransform FindRectTransform(string goName)
        {
            Transform found = FindDeep(transform, goName);
            return found != null ? found.GetComponent<RectTransform>() : null;
        }

        private Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
