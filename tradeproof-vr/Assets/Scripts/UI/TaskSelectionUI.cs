using UnityEngine;
using TMPro;
using TradeProof.Core;
using TradeProof.Training;

namespace TradeProof.UI
{
    /// <summary>
    /// Task selection screen with floating cards in VR space.
    /// Shows Panel Inspection (Beginner) and Circuit Wiring (Intermediate) cards.
    /// Each card shows description, difficulty, badge awarded.
    /// </summary>
    public class TaskSelectionUI : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas canvas;

        [Header("Cards")]
        [SerializeField] private RectTransform panelInspectionCard;
        [SerializeField] private RectTransform circuitWiringCard;

        [Header("Mode Selection")]
        [SerializeField] private RectTransform modePanel;
        private string selectedTaskId;

        [Header("Positioning")]
        [SerializeField] private float distanceFromPlayer = 1.5f;
        [SerializeField] private float heightOffset = 0.2f;
        [SerializeField] private float cardSpacing = 0.55f;

        [Header("Colors")]
        [SerializeField] private Color cardBackground = new Color(0.08f, 0.1f, 0.18f, 0.95f);
        [SerializeField] private Color beginnerColor = new Color(0.2f, 0.8f, 0.4f, 1f);
        [SerializeField] private Color intermediateColor = new Color(0.9f, 0.6f, 0.1f, 1f);
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.5f, 0.9f, 1f);

        private void Awake()
        {
            SetupCanvas();
            CreateTaskCards();
            CreateModePanel();
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
            rt.sizeDelta = new Vector2(1400f, 700f);
            rt.localScale = Vector3.one * 0.001f;
        }

        private void CreateTaskCards()
        {
            // Panel Inspection Card
            panelInspectionCard = CreateCard(
                "PanelInspectionCard",
                "Panel Inspection",
                "Residential Panel Inspection",
                "Identify NEC code violations in a residential electrical panel.\n\n" +
                "Learn to spot double-tapped breakers, missing knockout covers, " +
                "incorrect wire gauges, and other common violations.",
                "Beginner",
                beginnerColor,
                "Panel Inspection - Level 1",
                "panel-inspection-residential",
                new Vector2(-350f, 0f)
            );

            // Circuit Wiring Card
            circuitWiringCard = CreateCard(
                "CircuitWiringCard",
                "Circuit Wiring",
                "20A Circuit Wiring",
                "Wire a complete 20-amp circuit from the electrical panel to an outlet box.\n\n" +
                "Select correct wire gauge, route wires, make proper connections " +
                "at panel and outlet, and label the circuit.",
                "Intermediate",
                intermediateColor,
                "Circuit Wiring - Level 1",
                "circuit-wiring-20a",
                new Vector2(350f, 0f)
            );
        }

        private RectTransform CreateCard(string objName, string title, string subtitle,
            string description, string difficulty, Color diffColor,
            string badge, string taskId, Vector2 anchoredPos)
        {
            GameObject cardObj = new GameObject(objName);
            cardObj.transform.SetParent(canvas.transform, false);

            RectTransform cardRT = cardObj.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(500f, 600f);
            cardRT.anchoredPosition = anchoredPos;

            // Card background
            UnityEngine.UI.Image cardBg = cardObj.AddComponent<UnityEngine.UI.Image>();
            cardBg.color = cardBackground;

            // Difficulty badge at top
            GameObject diffBadge = new GameObject("DifficultyBadge");
            diffBadge.transform.SetParent(cardObj.transform, false);

            RectTransform diffRT = diffBadge.AddComponent<RectTransform>();
            diffRT.anchorMin = new Vector2(0.15f, 0.88f);
            diffRT.anchorMax = new Vector2(0.85f, 0.96f);
            diffRT.offsetMin = Vector2.zero;
            diffRT.offsetMax = Vector2.zero;

            UnityEngine.UI.Image diffBg = diffBadge.AddComponent<UnityEngine.UI.Image>();
            diffBg.color = diffColor;

            TextMeshProUGUI diffText = CreateTextElement(diffBadge.transform, difficulty.ToUpper(), 18,
                TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetFullAnchor(diffText.rectTransform);

            // Title
            TextMeshProUGUI titleTMP = CreateTextElement(cardObj.transform, title, 42,
                TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            RectTransform titleRT = titleTMP.rectTransform;
            titleRT.anchorMin = new Vector2(0.05f, 0.74f);
            titleRT.anchorMax = new Vector2(0.95f, 0.88f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // Subtitle
            TextMeshProUGUI subTMP = CreateTextElement(cardObj.transform, subtitle, 22,
                TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.7f), FontStyles.Normal);
            RectTransform subRT = subTMP.rectTransform;
            subRT.anchorMin = new Vector2(0.05f, 0.68f);
            subRT.anchorMax = new Vector2(0.95f, 0.76f);
            subRT.offsetMin = Vector2.zero;
            subRT.offsetMax = Vector2.zero;

            // Description
            TextMeshProUGUI descTMP = CreateTextElement(cardObj.transform, description, 18,
                TextAlignmentOptions.TopLeft, new Color(0.8f, 0.8f, 0.8f), FontStyles.Normal);
            RectTransform descRT = descTMP.rectTransform;
            descRT.anchorMin = new Vector2(0.08f, 0.32f);
            descRT.anchorMax = new Vector2(0.92f, 0.66f);
            descRT.offsetMin = Vector2.zero;
            descRT.offsetMax = Vector2.zero;

            // Badge info
            TextMeshProUGUI badgeTMP = CreateTextElement(cardObj.transform,
                $"Badge: {badge}", 16,
                TextAlignmentOptions.Center, diffColor, FontStyles.Italic);
            RectTransform badgeRT = badgeTMP.rectTransform;
            badgeRT.anchorMin = new Vector2(0.05f, 0.24f);
            badgeRT.anchorMax = new Vector2(0.95f, 0.32f);
            badgeRT.offsetMin = Vector2.zero;
            badgeRT.offsetMax = Vector2.zero;

            // Best Score
            float bestScore = GameManager.Instance?.Progress?.GetBestScore(taskId, "Test") ?? 0f;
            string scoreStr = bestScore > 0 ? $"Best Score: {bestScore:F0}%" : "Not attempted";
            TextMeshProUGUI scoreTMP = CreateTextElement(cardObj.transform, scoreStr, 16,
                TextAlignmentOptions.Center, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal);
            RectTransform scoreRT = scoreTMP.rectTransform;
            scoreRT.anchorMin = new Vector2(0.05f, 0.17f);
            scoreRT.anchorMax = new Vector2(0.95f, 0.24f);
            scoreRT.offsetMin = Vector2.zero;
            scoreRT.offsetMax = Vector2.zero;

            // Select button
            CreateCardButton(cardObj.transform, "SELECT", buttonColor,
                new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.15f),
                () => OnTaskSelected(taskId));

            return cardRT;
        }

        private void CreateModePanel()
        {
            GameObject modePanelObj = new GameObject("ModePanel");
            modePanelObj.transform.SetParent(canvas.transform, false);

            modePanel = modePanelObj.AddComponent<RectTransform>();
            modePanel.sizeDelta = new Vector2(600f, 300f);
            modePanel.anchoredPosition = Vector2.zero;

            UnityEngine.UI.Image modeBg = modePanelObj.AddComponent<UnityEngine.UI.Image>();
            modeBg.color = new Color(0.06f, 0.08f, 0.14f, 0.98f);

            // Title
            TextMeshProUGUI modeTitle = CreateTextElement(modePanelObj.transform,
                "Select Training Mode", 32, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            RectTransform modeTitleRT = modeTitle.rectTransform;
            modeTitleRT.anchorMin = new Vector2(0.05f, 0.75f);
            modeTitleRT.anchorMax = new Vector2(0.95f, 0.95f);
            modeTitleRT.offsetMin = Vector2.zero;
            modeTitleRT.offsetMax = Vector2.zero;

            // Learn button
            CreateCardButton(modePanelObj.transform, "LEARN\nGuided Tour", new Color(0.2f, 0.7f, 0.3f),
                new Vector2(0.03f, 0.25f), new Vector2(0.32f, 0.70f),
                () => OnModeSelected(TaskMode.Learn));

            // Practice button
            CreateCardButton(modePanelObj.transform, "PRACTICE\nWith Hints", new Color(0.2f, 0.5f, 0.9f),
                new Vector2(0.35f, 0.25f), new Vector2(0.65f, 0.70f),
                () => OnModeSelected(TaskMode.Practice));

            // Test button
            CreateCardButton(modePanelObj.transform, "TEST\nTimed, No Help", new Color(0.9f, 0.3f, 0.2f),
                new Vector2(0.68f, 0.25f), new Vector2(0.97f, 0.70f),
                () => OnModeSelected(TaskMode.Test));

            // Back button
            CreateCardButton(modePanelObj.transform, "BACK", new Color(0.3f, 0.3f, 0.3f),
                new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.22f),
                OnBackFromModeSelection);

            modePanel.gameObject.SetActive(false);
        }

        // --- Helpers ---

        private TextMeshProUGUI CreateTextElement(Transform parent, string text, float fontSize,
            TextAlignmentOptions alignment, Color color, FontStyles style)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Truncate;

            return tmp;
        }

        private void SetFullAnchor(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void CreateCardButton(Transform parent, string text, Color bgColor,
            Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            GameObject btnObj = new GameObject("Button");
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.anchorMin = anchorMin;
            btnRT.anchorMax = anchorMax;
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            UnityEngine.UI.Image btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = bgColor;

            UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(action);

            TextMeshProUGUI btnText = CreateTextElement(btnObj.transform, text, 22,
                TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetFullAnchor(btnText.rectTransform);
        }

        // --- Events ---

        private void OnTaskSelected(string taskId)
        {
            AudioManager.Instance.PlayButtonClick();
            selectedTaskId = taskId;

            // Show mode selection panel, hide task cards
            if (panelInspectionCard != null) panelInspectionCard.gameObject.SetActive(false);
            if (circuitWiringCard != null) circuitWiringCard.gameObject.SetActive(false);
            modePanel.gameObject.SetActive(true);
        }

        private void OnModeSelected(TaskMode mode)
        {
            AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.StartTask(selectedTaskId, mode);
        }

        private void OnBackFromModeSelection()
        {
            AudioManager.Instance.PlayButtonClick();
            modePanel.gameObject.SetActive(false);
            if (panelInspectionCard != null) panelInspectionCard.gameObject.SetActive(true);
            if (circuitWiringCard != null) circuitWiringCard.gameObject.SetActive(true);
        }

        // --- Show/Hide ---

        public void Show()
        {
            gameObject.SetActive(true);
            PositionInFrontOfPlayer();

            // Reset to card view
            if (panelInspectionCard != null) panelInspectionCard.gameObject.SetActive(true);
            if (circuitWiringCard != null) circuitWiringCard.gameObject.SetActive(true);
            if (modePanel != null) modePanel.gameObject.SetActive(false);
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
    }
}
