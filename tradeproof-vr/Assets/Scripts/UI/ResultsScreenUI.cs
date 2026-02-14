using UnityEngine;
using TMPro;
using System.Collections;
using TradeProof.Core;
using TradeProof.Data;
using TradeProof.Training;

namespace TradeProof.UI
{
    /// <summary>
    /// Results screen shown after completing a task.
    /// Shows score breakdown, pass/fail, NEC codes violated/correct,
    /// badge earned animation, Retry and Next Task buttons.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas canvas;

        [Header("Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private TextMeshProUGUI passFailText;
        [SerializeField] private TextMeshProUGUI breakdownText;
        [SerializeField] private TextMeshProUGUI necCodesText;
        [SerializeField] private TextMeshProUGUI badgeText;
        [SerializeField] private UnityEngine.UI.Button retryButton;
        [SerializeField] private UnityEngine.UI.Button nextTaskButton;
        [SerializeField] private UnityEngine.UI.Button menuButton;

        [Header("Badge Animation")]
        [SerializeField] private RectTransform badgeIcon;
        [SerializeField] private float badgeAnimDuration = 1.5f;

        [Header("Positioning")]
        [SerializeField] private float distanceFromPlayer = 1.2f;
        [SerializeField] private float heightOffset = 0.15f;

        [Header("Colors")]
        [SerializeField] private Color passColor = new Color(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color failColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color bgColor = new Color(0.04f, 0.06f, 0.12f, 0.95f);

        private Coroutine animationCoroutine;

        private void Awake()
        {
            SetupCanvas();
            SetupElements();
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

            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(900f, 700f);
            rt.localScale = Vector3.one * 0.001f;

            UnityEngine.UI.Image bg = canvas.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (bg == null)
            {
                bg = canvas.gameObject.AddComponent<UnityEngine.UI.Image>();
            }
            bg.color = bgColor;
        }

        private void SetupElements()
        {
            // Title
            titleText = CreateText("Title", "Results", 48, FontStyles.Bold,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), TextAlignmentOptions.Center, Color.white);

            // Score (large)
            scoreText = CreateText("Score", "0%", 72, FontStyles.Bold,
                new Vector2(0.1f, 0.72f), new Vector2(0.5f, 0.88f), TextAlignmentOptions.Center, Color.white);

            // Grade letter
            gradeText = CreateText("Grade", "F", 48, FontStyles.Bold,
                new Vector2(0.55f, 0.72f), new Vector2(0.9f, 0.88f), TextAlignmentOptions.Center, Color.white);

            // Pass/Fail
            passFailText = CreateText("PassFail", "PENDING", 32, FontStyles.Bold,
                new Vector2(0.1f, 0.64f), new Vector2(0.9f, 0.74f), TextAlignmentOptions.Center, Color.white);

            // Breakdown
            breakdownText = CreateText("Breakdown", "", 18, FontStyles.Normal,
                new Vector2(0.05f, 0.38f), new Vector2(0.5f, 0.62f), TextAlignmentOptions.TopLeft,
                new Color(0.8f, 0.8f, 0.8f));

            // NEC codes
            necCodesText = CreateText("NECCodes", "", 16, FontStyles.Normal,
                new Vector2(0.5f, 0.25f), new Vector2(0.95f, 0.62f), TextAlignmentOptions.TopLeft,
                new Color(0.7f, 0.8f, 0.9f));

            // Badge earned
            badgeText = CreateText("Badge", "", 22, FontStyles.Bold,
                new Vector2(0.05f, 0.2f), new Vector2(0.5f, 0.36f), TextAlignmentOptions.Center,
                new Color(1f, 0.85f, 0.3f));

            // Badge icon placeholder
            GameObject badgeIconObj = new GameObject("BadgeIcon");
            badgeIconObj.transform.SetParent(canvas.transform, false);
            badgeIcon = badgeIconObj.AddComponent<RectTransform>();
            badgeIcon.anchorMin = new Vector2(0.15f, 0.22f);
            badgeIcon.anchorMax = new Vector2(0.35f, 0.34f);
            badgeIcon.offsetMin = Vector2.zero;
            badgeIcon.offsetMax = Vector2.zero;

            UnityEngine.UI.Image badgeIconImg = badgeIconObj.AddComponent<UnityEngine.UI.Image>();
            badgeIconImg.color = new Color(1f, 0.85f, 0.3f, 0f); // Start invisible
            badgeIcon.gameObject.SetActive(false);

            // Buttons
            CreateActionButton("RetryButton", "RETRY", new Color(0.9f, 0.5f, 0.1f),
                new Vector2(0.05f, 0.03f), new Vector2(0.32f, 0.15f), OnRetryClicked, out retryButton);

            CreateActionButton("NextButton", "NEXT TASK", new Color(0.2f, 0.6f, 0.9f),
                new Vector2(0.35f, 0.03f), new Vector2(0.65f, 0.15f), OnNextTaskClicked, out nextTaskButton);

            CreateActionButton("MenuButton", "MAIN MENU", new Color(0.3f, 0.3f, 0.4f),
                new Vector2(0.68f, 0.03f), new Vector2(0.95f, 0.15f), OnMenuClicked, out menuButton);
        }

        private TextMeshProUGUI CreateText(string name, string text, float fontSize, FontStyles style,
            Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(canvas.transform, false);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableWordWrapping = true;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return tmp;
        }

        private void CreateActionButton(string name, string text, Color color,
            Vector2 anchorMin, Vector2 anchorMax,
            UnityEngine.Events.UnityAction action, out UnityEngine.UI.Button button)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(canvas.transform, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.anchorMin = anchorMin;
            btnRT.anchorMax = anchorMax;
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            UnityEngine.UI.Image btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = color;

            button = btnObj.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(action);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 22;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
        }

        // --- Display Results ---

        public void DisplayResults(TaskResult result)
        {
            if (result == null)
            {
                Debug.LogError("[ResultsScreenUI] No result data to display");
                return;
            }

            PositionInFrontOfPlayer();

            // Title
            titleText.text = $"{result.taskName} — Results";

            // Score
            scoreText.text = $"{result.score:F0}%";
            scoreText.color = ScoreManager.Instance.GetGradeColor(result.score);

            // Grade
            string grade = ScoreManager.Instance.GetGradeFromScore(result.score);
            gradeText.text = grade;
            gradeText.color = ScoreManager.Instance.GetGradeColor(result.score);

            // Pass/Fail
            passFailText.text = result.passed ? "PASSED" : "FAILED";
            passFailText.color = result.passed ? passColor : failColor;

            // Breakdown
            if (result.breakdown != null)
            {
                string breakdown = "";
                ScoreBreakdown b = result.breakdown;

                if (b.totalViolations > 0)
                {
                    breakdown += $"Violations Found: {b.violationsFound}/{b.totalViolations}\n";
                    breakdown += $"False Positives: {b.falsePositives}\n";
                }

                if (b.totalSteps > 0)
                {
                    breakdown += $"Steps Completed: {b.stepsCompleted}/{b.totalSteps}\n";
                    breakdown += $"Correct Order: {b.stepsInCorrectOrder}/{b.totalSteps}\n";
                    breakdown += $"Wire Gauge: {(b.wireGaugeCorrect ? "Correct" : "INCORRECT")}\n";
                    breakdown += $"Connection Quality: {b.connectionQuality}\n";
                }

                breakdown += $"\nTime Used: {b.timeUsed:F1}s";
                if (b.timeBonus > 0)
                {
                    breakdown += $"\nTime Bonus: +{b.timeBonus:F0}%";
                }

                breakdown += $"\nMode: {result.mode}";
                breakdownText.text = breakdown;
            }

            // NEC Codes
            if (result.necCodeResults != null && result.necCodeResults.Count > 0)
            {
                string necText = "NEC Code Results:\n\n";
                foreach (var codeResult in result.necCodeResults)
                {
                    string status = codeResult.identified ? "[OK]" : "[MISSED]";
                    string severity = codeResult.severity == "critical" ? " (!)" : "";
                    necText += $"{status} NEC {codeResult.necCode}{severity}\n";
                    necText += $"  {codeResult.description}\n\n";
                }
                necCodesText.text = necText;
            }
            else
            {
                necCodesText.text = "";
            }

            // Badge
            if (result.passed && !string.IsNullOrEmpty(result.badgeId))
            {
                badgeText.text = $"Badge Earned!\n{result.badgeId}";
                badgeText.gameObject.SetActive(true);

                // Play badge animation
                if (animationCoroutine != null) StopCoroutine(animationCoroutine);
                animationCoroutine = StartCoroutine(BadgeEarnedAnimation());
            }
            else
            {
                badgeText.text = result.passed ? "" : "Score 80% or higher to earn the badge.";
                badgeIcon.gameObject.SetActive(false);
            }
        }

        private IEnumerator BadgeEarnedAnimation()
        {
            AudioManager.Instance.PlayBadgeEarnedSound();

            badgeIcon.gameObject.SetActive(true);
            UnityEngine.UI.Image iconImg = badgeIcon.GetComponent<UnityEngine.UI.Image>();

            float elapsed = 0f;
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;

            while (elapsed < badgeAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / badgeAnimDuration;

                // Ease out elastic
                float p = 0.3f;
                float s = p / 4f;
                float elasticT = Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;

                badgeIcon.localScale = Vector3.Lerp(startScale, endScale, elasticT);

                if (iconImg != null)
                {
                    Color c = new Color(1f, 0.85f, 0.3f, Mathf.Clamp01(t * 3f));
                    iconImg.color = c;
                }

                yield return null;
            }

            badgeIcon.localScale = endScale;
        }

        // --- Button Handlers ---

        private void OnRetryClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.StartTask(
                GameManager.Instance.CurrentTaskId,
                GameManager.Instance.CurrentTaskMode
            );
        }

        private void OnNextTaskClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.ReturnToTaskSelection();
        }

        private void OnMenuClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.ReturnToMenu();
        }

        // --- Show/Hide ---

        public void Show()
        {
            gameObject.SetActive(true);
            PositionInFrontOfPlayer();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
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
    }
}
