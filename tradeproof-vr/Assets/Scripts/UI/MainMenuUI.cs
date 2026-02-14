using UnityEngine;
using TMPro;
using TradeProof.Core;

namespace TradeProof.UI
{
    /// <summary>
    /// Main menu UI displayed in VR space.
    /// Shows "TradeProof VR Training" title and Start button.
    /// Positioned as a world-space canvas facing the player.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private UnityEngine.UI.Button startButton;
        [SerializeField] private TextMeshProUGUI startButtonText;
        [SerializeField] private UnityEngine.UI.Button badgesButton;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Positioning")]
        [SerializeField] private float distanceFromPlayer = 1.5f;
        [SerializeField] private float heightOffset = 0.2f;
        [SerializeField] private float canvasWidth = 1.0f;
        [SerializeField] private float canvasHeight = 0.7f;

        [Header("Visual")]
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.08f, 0.15f, 0.9f);
        [SerializeField] private Color accentColor = new Color(0.2f, 0.6f, 1f, 1f);

        private void Awake()
        {
            SetupCanvas();
            SetupUIElements();
            SetupButtons();
        }

        private void SetupCanvas()
        {
            if (canvas == null)
            {
                canvas = gameObject.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                }
            }

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(canvasWidth * 1000f, canvasHeight * 1000f);
            rt.localScale = Vector3.one * 0.001f; // Scale to meters

            // Add background
            UnityEngine.UI.Image bgImage = canvas.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (bgImage == null)
            {
                bgImage = canvas.gameObject.AddComponent<UnityEngine.UI.Image>();
            }
            bgImage.color = backgroundColor;
        }

        private void SetupUIElements()
        {
            // Title
            if (titleText == null)
            {
                GameObject titleObj = new GameObject("Title");
                titleObj.transform.SetParent(canvas.transform, false);
                titleText = titleObj.AddComponent<TextMeshProUGUI>();
                titleText.text = "TradeProof";
                titleText.fontSize = 72;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = Color.white;
                titleText.fontStyle = FontStyles.Bold;

                RectTransform titleRT = titleObj.GetComponent<RectTransform>();
                titleRT.anchorMin = new Vector2(0.1f, 0.7f);
                titleRT.anchorMax = new Vector2(0.9f, 0.9f);
                titleRT.offsetMin = Vector2.zero;
                titleRT.offsetMax = Vector2.zero;
            }

            // Subtitle
            if (subtitleText == null)
            {
                GameObject subtitleObj = new GameObject("Subtitle");
                subtitleObj.transform.SetParent(canvas.transform, false);
                subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
                subtitleText.text = "VR Electrical Training";
                subtitleText.fontSize = 32;
                subtitleText.alignment = TextAlignmentOptions.Center;
                subtitleText.color = accentColor;

                RectTransform subtitleRT = subtitleObj.GetComponent<RectTransform>();
                subtitleRT.anchorMin = new Vector2(0.1f, 0.6f);
                subtitleRT.anchorMax = new Vector2(0.9f, 0.72f);
                subtitleRT.offsetMin = Vector2.zero;
                subtitleRT.offsetMax = Vector2.zero;
            }

            // Stats
            if (statsText == null)
            {
                GameObject statsObj = new GameObject("Stats");
                statsObj.transform.SetParent(canvas.transform, false);
                statsText = statsObj.AddComponent<TextMeshProUGUI>();
                statsText.fontSize = 20;
                statsText.alignment = TextAlignmentOptions.Center;
                statsText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

                RectTransform statsRT = statsObj.GetComponent<RectTransform>();
                statsRT.anchorMin = new Vector2(0.1f, 0.45f);
                statsRT.anchorMax = new Vector2(0.9f, 0.58f);
                statsRT.offsetMin = Vector2.zero;
                statsRT.offsetMax = Vector2.zero;
            }

            // Version
            if (versionText == null)
            {
                GameObject versionObj = new GameObject("Version");
                versionObj.transform.SetParent(canvas.transform, false);
                versionText = versionObj.AddComponent<TextMeshProUGUI>();
                versionText.text = "v1.0 | Meta Quest 3";
                versionText.fontSize = 14;
                versionText.alignment = TextAlignmentOptions.Center;
                versionText.color = new Color(0.4f, 0.4f, 0.4f, 1f);

                RectTransform versionRT = versionObj.GetComponent<RectTransform>();
                versionRT.anchorMin = new Vector2(0.2f, 0.02f);
                versionRT.anchorMax = new Vector2(0.8f, 0.08f);
                versionRT.offsetMin = Vector2.zero;
                versionRT.offsetMax = Vector2.zero;
            }
        }

        private void SetupButtons()
        {
            // Start button
            if (startButton == null)
            {
                GameObject btnObj = new GameObject("StartButton");
                btnObj.transform.SetParent(canvas.transform, false);

                UnityEngine.UI.Image btnImage = btnObj.AddComponent<UnityEngine.UI.Image>();
                btnImage.color = accentColor;

                startButton = btnObj.AddComponent<UnityEngine.UI.Button>();

                RectTransform btnRT = btnObj.GetComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(0.25f, 0.22f);
                btnRT.anchorMax = new Vector2(0.75f, 0.38f);
                btnRT.offsetMin = Vector2.zero;
                btnRT.offsetMax = Vector2.zero;

                // Button text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                startButtonText = textObj.AddComponent<TextMeshProUGUI>();
                startButtonText.text = "START TRAINING";
                startButtonText.fontSize = 36;
                startButtonText.alignment = TextAlignmentOptions.Center;
                startButtonText.color = Color.white;
                startButtonText.fontStyle = FontStyles.Bold;

                RectTransform textRT = textObj.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;
            }

            startButton.onClick.AddListener(OnStartClicked);

            // Badges button
            if (badgesButton == null)
            {
                GameObject badgeBtnObj = new GameObject("BadgesButton");
                badgeBtnObj.transform.SetParent(canvas.transform, false);

                UnityEngine.UI.Image badgeBtnImage = badgeBtnObj.AddComponent<UnityEngine.UI.Image>();
                badgeBtnImage.color = new Color(0.15f, 0.15f, 0.3f, 0.8f);

                badgesButton = badgeBtnObj.AddComponent<UnityEngine.UI.Button>();

                RectTransform badgeBtnRT = badgeBtnObj.GetComponent<RectTransform>();
                badgeBtnRT.anchorMin = new Vector2(0.3f, 0.1f);
                badgeBtnRT.anchorMax = new Vector2(0.7f, 0.2f);
                badgeBtnRT.offsetMin = Vector2.zero;
                badgeBtnRT.offsetMax = Vector2.zero;

                GameObject badgeTextObj = new GameObject("Text");
                badgeTextObj.transform.SetParent(badgeBtnObj.transform, false);
                TextMeshProUGUI badgeText = badgeTextObj.AddComponent<TextMeshProUGUI>();
                badgeText.text = "View Badges";
                badgeText.fontSize = 24;
                badgeText.alignment = TextAlignmentOptions.Center;
                badgeText.color = Color.white;

                RectTransform badgeTextRT = badgeTextObj.GetComponent<RectTransform>();
                badgeTextRT.anchorMin = Vector2.zero;
                badgeTextRT.anchorMax = Vector2.one;
                badgeTextRT.offsetMin = Vector2.zero;
                badgeTextRT.offsetMax = Vector2.zero;
            }

            badgesButton.onClick.AddListener(OnBadgesClicked);
        }

        // --- Show/Hide ---

        public void Show()
        {
            gameObject.SetActive(true);
            PositionInFrontOfPlayer();
            UpdateStats();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void PositionInFrontOfPlayer()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 forward = cam.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            transform.position = cam.transform.position +
                                 forward * distanceFromPlayer +
                                 Vector3.up * heightOffset;

            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        private void UpdateStats()
        {
            if (statsText == null) return;

            Data.PlayerProgress progress = GameManager.Instance.Progress;
            if (progress != null)
            {
                int badges = BadgeSystem.Instance.GetBadgeCount();
                string practiceTime = progress.GetFormattedPracticeTime();
                float passRate = progress.GetOverallPassRate();

                statsText.text = $"Badges: {badges} | Practice Time: {practiceTime} | Pass Rate: {passRate:F0}%";
            }
            else
            {
                statsText.text = "Welcome, apprentice. Begin your training.";
            }
        }

        // --- Button Handlers ---

        private void OnStartClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.TransitionToState(GameState.TaskSelection);
        }

        private void OnBadgesClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            BadgeDisplayUI badgeUI = FindObjectOfType<BadgeDisplayUI>(true);
            if (badgeUI != null)
            {
                badgeUI.Show();
            }
        }
    }
}
