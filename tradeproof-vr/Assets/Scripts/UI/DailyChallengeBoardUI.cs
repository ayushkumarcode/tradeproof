using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TradeProof.Core;

namespace TradeProof.UI
{
    /// <summary>
    /// World-space canvas that displays the current daily challenge,
    /// progress bar, and XP bonus information.
    /// </summary>
    public class DailyChallengeBoardUI : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject panelRoot;

        // UI elements
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI challengeDescriptionText;
        private Image progressBarBackground;
        private Image progressBarFill;
        private TextMeshProUGUI progressText;
        private TextMeshProUGUI xpBonusText;
        private TextMeshProUGUI completeBannerText;
        private GameObject completeBanner;

        private Camera playerCamera;
        private DailyChallengeSystem challengeSystem;

        private void Awake()
        {
            CreateUI();
            Hide();
        }

        private void Start()
        {
            challengeSystem = DailyChallengeSystem.Instance;
            if (challengeSystem != null)
            {
                challengeSystem.OnChallengeProgressUpdated += OnProgressUpdated;
                challengeSystem.OnChallengeCompleted += OnChallengeCompleted;
            }
        }

        private void OnDestroy()
        {
            if (challengeSystem != null)
            {
                challengeSystem.OnChallengeProgressUpdated -= OnProgressUpdated;
                challengeSystem.OnChallengeCompleted -= OnChallengeCompleted;
            }
        }

        private void CreateUI()
        {
            // Canvas setup: 500x300 world space
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(500f, 300f);
            canvasRT.localScale = Vector3.one * 0.001f;

            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            // Panel background
            panelRoot = new GameObject("DailyChallengePanel");
            panelRoot.transform.SetParent(canvas.transform, false);
            RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.14f, 0.93f);

            // Border accent (top bar)
            GameObject borderBar = new GameObject("TopBorder");
            borderBar.transform.SetParent(panelRoot.transform, false);
            Image borderImg = borderBar.AddComponent<Image>();
            borderImg.color = new Color(0.9f, 0.6f, 0.1f);
            RectTransform borderRT = borderBar.GetComponent<RectTransform>();
            borderRT.anchorMin = new Vector2(0f, 0.93f);
            borderRT.anchorMax = new Vector2(1f, 1f);
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;

            // Title: "DAILY CHALLENGE"
            titleText = CreateText("Title", "DAILY CHALLENGE", 28, FontStyles.Bold,
                new Color(0.9f, 0.7f, 0.1f), TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.92f));

            // Challenge description
            challengeDescriptionText = CreateText("Description", "Loading...", 20, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.75f));

            // Progress bar background
            GameObject pbBgObj = new GameObject("ProgressBarBG");
            pbBgObj.transform.SetParent(panelRoot.transform, false);
            progressBarBackground = pbBgObj.AddComponent<Image>();
            progressBarBackground.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
            RectTransform pbBgRT = pbBgObj.GetComponent<RectTransform>();
            pbBgRT.anchorMin = new Vector2(0.08f, 0.38f);
            pbBgRT.anchorMax = new Vector2(0.92f, 0.48f);
            pbBgRT.offsetMin = Vector2.zero;
            pbBgRT.offsetMax = Vector2.zero;

            // Progress bar fill
            GameObject pbFillObj = new GameObject("ProgressBarFill");
            pbFillObj.transform.SetParent(pbBgObj.transform, false);
            progressBarFill = pbFillObj.AddComponent<Image>();
            progressBarFill.color = new Color(0.9f, 0.6f, 0.1f);
            RectTransform pbFillRT = pbFillObj.GetComponent<RectTransform>();
            pbFillRT.anchorMin = Vector2.zero;
            pbFillRT.anchorMax = new Vector2(0f, 1f); // Width set dynamically
            pbFillRT.offsetMin = Vector2.zero;
            pbFillRT.offsetMax = Vector2.zero;

            // Progress text
            progressText = CreateText("ProgressText", "0/0", 18, FontStyles.Normal,
                new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.38f));

            // XP bonus display
            xpBonusText = CreateText("XPBonus", "Bonus: +0 XP", 22, FontStyles.Bold,
                new Color(0.5f, 0.8f, 1f), TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.25f));

            // Completion banner (hidden by default)
            completeBanner = new GameObject("CompleteBanner");
            completeBanner.transform.SetParent(panelRoot.transform, false);
            RectTransform bannerRT = completeBanner.AddComponent<RectTransform>();
            bannerRT.anchorMin = new Vector2(0.1f, 0.3f);
            bannerRT.anchorMax = new Vector2(0.9f, 0.55f);
            bannerRT.offsetMin = Vector2.zero;
            bannerRT.offsetMax = Vector2.zero;

            Image bannerBg = completeBanner.AddComponent<Image>();
            bannerBg.color = new Color(0.1f, 0.5f, 0.1f, 0.9f);

            GameObject bannerTextObj = new GameObject("BannerText");
            bannerTextObj.transform.SetParent(completeBanner.transform, false);
            completeBannerText = bannerTextObj.AddComponent<TextMeshProUGUI>();
            completeBannerText.text = "CHALLENGE COMPLETE!";
            completeBannerText.fontSize = 24;
            completeBannerText.fontStyle = FontStyles.Bold;
            completeBannerText.color = new Color(0.8f, 1f, 0.8f);
            completeBannerText.alignment = TextAlignmentOptions.Center;
            RectTransform bannerTextRT = bannerTextObj.GetComponent<RectTransform>();
            bannerTextRT.anchorMin = Vector2.zero;
            bannerTextRT.anchorMax = Vector2.one;
            bannerTextRT.offsetMin = Vector2.zero;
            bannerTextRT.offsetMax = Vector2.zero;

            completeBanner.SetActive(false);
        }

        private TextMeshProUGUI CreateText(string name, string text, float fontSize,
            FontStyles style, Color color, TextAlignmentOptions alignment,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(panelRoot.transform, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return tmp;
        }

        // --- Show / Hide ---

        public void Show()
        {
            gameObject.SetActive(true);
            UpdateDisplay();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void UpdateDisplay()
        {
            if (challengeSystem == null)
                challengeSystem = DailyChallengeSystem.Instance;

            DailyChallengeSystem.DailyChallenge challenge = challengeSystem.GetTodaysChallenge();
            if (challenge == null) return;

            // Challenge description
            challengeDescriptionText.text = challenge.description;

            // XP bonus
            xpBonusText.text = $"Bonus: +{challenge.xpBonus} XP";

            // Progress
            string progressStr = challengeSystem.GetProgressText();
            progressText.text = progressStr;

            // Progress bar fill
            float fillAmount = 0f;
            if (challenge.targetValue > 0)
            {
                fillAmount = Mathf.Clamp01((float)challenge.currentProgress / challenge.targetValue);
            }
            if (progressBarFill != null)
            {
                progressBarFill.rectTransform.anchorMax = new Vector2(fillAmount, 1f);
            }

            // Completion state
            if (challenge.isComplete)
            {
                completeBanner.SetActive(true);
                progressBarFill.color = new Color(0.2f, 0.8f, 0.2f); // Green
            }
            else
            {
                completeBanner.SetActive(false);
                progressBarFill.color = new Color(0.9f, 0.6f, 0.1f); // Orange
            }
        }

        // --- Event Handlers ---

        private void OnProgressUpdated(DailyChallengeSystem.DailyChallenge challenge)
        {
            if (gameObject.activeSelf)
            {
                UpdateDisplay();
            }
        }

        private void OnChallengeCompleted(DailyChallengeSystem.DailyChallenge challenge)
        {
            if (gameObject.activeSelf)
            {
                UpdateDisplay();
            }
        }
    }
}
