using UnityEngine;
using TMPro;
using TradeProof.Core;

namespace TradeProof.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private TextMeshProUGUI statsText;

        // Buttons
        private UnityEngine.UI.Button startDayButton;
        private UnityEngine.UI.Button freeTrainingButton;
        private UnityEngine.UI.Button careerProgressButton;
        private UnityEngine.UI.Button badgesButton;

        [Header("Positioning")]
        [SerializeField] private float distanceFromPlayer = 1.5f;
        [SerializeField] private float heightOffset = 0.2f;
        [SerializeField] private float canvasWidth = 1.2f;
        [SerializeField] private float canvasHeight = 0.9f;

        [Header("Visual")]
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.08f, 0.15f, 0.9f);
        [SerializeField] private Color accentColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color dayButtonColor = new Color(0.9f, 0.6f, 0.1f, 1f);
        [SerializeField] private Color trainingButtonColor = new Color(0.2f, 0.6f, 1f, 1f);

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
                    canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(canvasWidth * 1000f, canvasHeight * 1000f);
            rt.localScale = Vector3.one * 0.001f;

            UnityEngine.UI.Image bgImage = canvas.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (bgImage == null)
                bgImage = canvas.gameObject.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = backgroundColor;
        }

        private void SetupUIElements()
        {
            // Title
            if (titleText == null)
            {
                titleText = CreateText("Title", "TradeProof", 72, FontStyles.Bold, Color.white,
                    new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.95f));
            }

            // Subtitle
            if (subtitleText == null)
            {
                subtitleText = CreateText("Subtitle", "Day in the Life of an Electrician", 28, FontStyles.Normal, accentColor,
                    new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.78f));
            }

            // Stats
            if (statsText == null)
            {
                statsText = CreateText("Stats", "", 18, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f),
                    new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.68f));
            }

            // Version
            if (versionText == null)
            {
                versionText = CreateText("Version", "v2.0 | Meta Quest 2", 14, FontStyles.Normal, new Color(0.4f, 0.4f, 0.4f),
                    new Vector2(0.2f, 0.02f), new Vector2(0.8f, 0.07f));
            }
        }

        private void SetupButtons()
        {
            // Start Day button (primary action)
            startDayButton = CreateButton("StartDayButton", "START YOUR DAY", dayButtonColor,
                new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.53f), 32);
            startDayButton.onClick.AddListener(OnStartDayClicked);

            // Free Training button
            freeTrainingButton = CreateButton("FreeTrainingButton", "FREE TRAINING", trainingButtonColor,
                new Vector2(0.15f, 0.24f), new Vector2(0.48f, 0.35f), 22);
            freeTrainingButton.onClick.AddListener(OnFreeTrainingClicked);

            // Career Progress button
            careerProgressButton = CreateButton("CareerProgressButton", "CAREER PROGRESS", new Color(0.2f, 0.7f, 0.4f),
                new Vector2(0.52f, 0.24f), new Vector2(0.85f, 0.35f), 22);
            careerProgressButton.onClick.AddListener(OnCareerProgressClicked);

            // Badges button
            badgesButton = CreateButton("BadgesButton", "VIEW BADGES", new Color(0.15f, 0.15f, 0.3f, 0.8f),
                new Vector2(0.25f, 0.1f), new Vector2(0.75f, 0.2f), 20);
            badgesButton.onClick.AddListener(OnBadgesClicked);
        }

        private TextMeshProUGUI CreateText(string name, string text, float fontSize, FontStyles style, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(canvas.transform, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return tmp;
        }

        private UnityEngine.UI.Button CreateButton(string name, string text, Color bgColor,
            Vector2 anchorMin, Vector2 anchorMax, float fontSize)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(canvas.transform, false);

            UnityEngine.UI.Image btnImage = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImage.color = bgColor;

            UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();

            RectTransform btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.anchorMin = anchorMin;
            btnRT.anchorMax = anchorMax;
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = fontSize;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return btn;
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
                CareerLevel level = GameManager.Instance.CurrentCareerLevel;
                int xp = progress.totalXP;
                int days = progress.daysCompleted;

                statsText.text = $"Career: {level} | XP: {xp} | Days: {days} | Badges: {badges} | Time: {practiceTime}";
            }
            else
            {
                statsText.text = "Welcome, apprentice. Begin your training.";
            }
        }

        // --- Button Handlers ---

        private void OnStartDayClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.StartDay();
        }

        private void OnFreeTrainingClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.TransitionToState(GameState.TaskSelection);
        }

        private void OnCareerProgressClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            CareerProgressUI careerUI = FindObjectOfType<CareerProgressUI>(true);
            if (careerUI != null)
            {
                careerUI.Show();
            }
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
