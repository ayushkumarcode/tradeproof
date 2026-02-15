using UnityEngine;
using TMPro;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Training;

namespace TradeProof.UI
{
    public class TaskSelectionUI : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas canvas;

        [Header("Mode Selection")]
        [SerializeField] private RectTransform modePanel;
        private string selectedTaskId;

        [Header("Positioning")]
        [SerializeField] private float distanceFromPlayer = 1.5f;
        [SerializeField] private float heightOffset = 0.2f;

        [Header("Colors")]
        [SerializeField] private Color cardBackground = new Color(0.08f, 0.1f, 0.18f, 0.95f);
        [SerializeField] private Color beginnerColor = new Color(0.2f, 0.8f, 0.4f, 1f);
        [SerializeField] private Color intermediateColor = new Color(0.9f, 0.6f, 0.1f, 1f);
        [SerializeField] private Color advancedColor = new Color(0.9f, 0.3f, 0.2f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.5f, 0.9f, 1f);

        private List<GameObject> taskCards = new List<GameObject>();
        private GameObject cardContainer;
        private int currentPage = 0;
        private const int CARDS_PER_PAGE = 4;

        // Task registry
        private struct TaskInfo
        {
            public string taskId;
            public string title;
            public string subtitle;
            public string description;
            public string difficulty;
            public string badge;
        }

        private TaskInfo[] allTasks;

        private void Awake()
        {
            InitializeTaskRegistry();
            SetupCanvas();
            CreateCardContainer();
            CreateTaskCards();
            CreateModePanel();
            CreateNavigationButtons();
        }

        private void InitializeTaskRegistry()
        {
            allTasks = new TaskInfo[]
            {
                new TaskInfo {
                    taskId = "panel-inspection-residential",
                    title = "Panel Inspection",
                    subtitle = "Residential Panel Inspection",
                    description = "Identify NEC code violations in a residential electrical panel. Spot double-tapped breakers, missing covers, and wrong wire gauges.",
                    difficulty = "Beginner",
                    badge = "Panel Inspection - Level 1"
                },
                new TaskInfo {
                    taskId = "outlet-installation-duplex",
                    title = "Outlet Install",
                    subtitle = "Duplex Outlet Installation",
                    description = "Install a duplex outlet: strip romex, connect hot/neutral/ground, verify wire length, tighten terminations, mount and plate.",
                    difficulty = "Beginner",
                    badge = "Outlet Installation - Level 1"
                },
                new TaskInfo {
                    taskId = "circuit-wiring-20a",
                    title = "Circuit Wiring",
                    subtitle = "20A Circuit Wiring",
                    description = "Wire a complete 20-amp circuit from panel to outlet. Select wire gauge, route, connect, and label the circuit.",
                    difficulty = "Intermediate",
                    badge = "Circuit Wiring - Level 1"
                },
                new TaskInfo {
                    taskId = "switch-wiring-3way",
                    title = "Switch Wiring",
                    subtitle = "3-Way Switch Wiring",
                    description = "Wire a 3-way switch circuit: identify line/load, connect common terminals, run travelers, splice neutrals, test operation.",
                    difficulty = "Intermediate",
                    badge = "Switch Wiring - Level 1"
                },
                new TaskInfo {
                    taskId = "gfci-testing-residential",
                    title = "GFCI Testing",
                    subtitle = "GFCI Testing & Replacement",
                    description = "Test GFCI outlets, verify downstream protection, trace daisy chains, identify faulty units, and replace defective GFCIs.",
                    difficulty = "Intermediate",
                    badge = "GFCI Testing - Level 1"
                },
                new TaskInfo {
                    taskId = "conduit-bending-emt",
                    title = "Conduit Bending",
                    subtitle = "EMT Conduit Bending",
                    description = "Measure, mark, and bend EMT conduit. Make 90-degree bends, offsets, saddle bends. Ream ends and verify NEC bend limits.",
                    difficulty = "Intermediate",
                    badge = "Conduit Bending - Level 1"
                },
                new TaskInfo {
                    taskId = "troubleshooting-residential",
                    title = "Troubleshooting",
                    subtitle = "Residential Troubleshooting",
                    description = "Diagnose electrical faults: read complaints, test with multimeter, trace circuits, identify and repair faults, verify fix.",
                    difficulty = "Advanced",
                    badge = "Troubleshooting - Level 1"
                }
            };
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

            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1800f, 800f);
            rt.localScale = Vector3.one * 0.001f;
        }

        private void CreateCardContainer()
        {
            cardContainer = new GameObject("CardContainer");
            cardContainer.transform.SetParent(canvas.transform, false);
            RectTransform containerRT = cardContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;
        }

        private void CreateTaskCards()
        {
            foreach (var card in taskCards)
                if (card != null) Destroy(card);
            taskCards.Clear();

            float cardWidth = 380f;
            float cardHeight = 500f;
            float spacing = 20f;

            int startIdx = currentPage * CARDS_PER_PAGE;
            int endIdx = Mathf.Min(startIdx + CARDS_PER_PAGE, allTasks.Length);
            int visibleCount = endIdx - startIdx;

            float totalWidth = visibleCount * cardWidth + (visibleCount - 1) * spacing;
            float startX = -totalWidth / 2f + cardWidth / 2f;

            for (int i = startIdx; i < endIdx; i++)
            {
                var task = allTasks[i];
                int cardIdx = i - startIdx;
                Vector2 pos = new Vector2(startX + cardIdx * (cardWidth + spacing), 30f);

                bool unlocked = IsTaskUnlocked(task.taskId);
                GameObject card = CreateCard(task, pos, cardWidth, cardHeight, unlocked);
                taskCards.Add(card);
            }
        }

        private bool IsTaskUnlocked(string taskId)
        {
            var progress = GameManager.Instance?.Progress;
            if (progress == null) return true;
            return progress.IsTaskUnlocked(taskId);
        }

        private Color GetDifficultyColor(string difficulty)
        {
            switch (difficulty)
            {
                case "Beginner": return beginnerColor;
                case "Intermediate": return intermediateColor;
                case "Advanced": return advancedColor;
                default: return intermediateColor;
            }
        }

        private GameObject CreateCard(TaskInfo task, Vector2 anchoredPos, float width, float height, bool unlocked)
        {
            Color diffColor = GetDifficultyColor(task.difficulty);

            GameObject cardObj = new GameObject($"Card_{task.taskId}");
            cardObj.transform.SetParent(cardContainer.transform, false);

            RectTransform cardRT = cardObj.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(width, height);
            cardRT.anchoredPosition = anchoredPos;

            UnityEngine.UI.Image cardBg = cardObj.AddComponent<UnityEngine.UI.Image>();
            cardBg.color = unlocked ? cardBackground : lockedColor;

            // Difficulty badge
            GameObject diffBadge = new GameObject("DifficultyBadge");
            diffBadge.transform.SetParent(cardObj.transform, false);
            RectTransform diffRT = diffBadge.AddComponent<RectTransform>();
            diffRT.anchorMin = new Vector2(0.15f, 0.9f);
            diffRT.anchorMax = new Vector2(0.85f, 0.97f);
            diffRT.offsetMin = Vector2.zero;
            diffRT.offsetMax = Vector2.zero;
            UnityEngine.UI.Image diffBg = diffBadge.AddComponent<UnityEngine.UI.Image>();
            diffBg.color = unlocked ? diffColor : new Color(0.3f, 0.3f, 0.3f);

            TextMeshProUGUI diffText = CreateTextElement(diffBadge.transform, task.difficulty.ToUpper(), 16,
                TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetFullAnchor(diffText.rectTransform);

            // Title
            TextMeshProUGUI titleTMP = CreateTextElement(cardObj.transform, task.title, 32,
                TextAlignmentOptions.Center, unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f), FontStyles.Bold);
            RectTransform titleRT = titleTMP.rectTransform;
            titleRT.anchorMin = new Vector2(0.05f, 0.76f);
            titleRT.anchorMax = new Vector2(0.95f, 0.9f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // Subtitle
            TextMeshProUGUI subTMP = CreateTextElement(cardObj.transform, task.subtitle, 18,
                TextAlignmentOptions.Center, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal);
            RectTransform subRT = subTMP.rectTransform;
            subRT.anchorMin = new Vector2(0.05f, 0.7f);
            subRT.anchorMax = new Vector2(0.95f, 0.78f);
            subRT.offsetMin = Vector2.zero;
            subRT.offsetMax = Vector2.zero;

            // Description
            TextMeshProUGUI descTMP = CreateTextElement(cardObj.transform, task.description, 15,
                TextAlignmentOptions.TopLeft, new Color(0.8f, 0.8f, 0.8f), FontStyles.Normal);
            RectTransform descRT = descTMP.rectTransform;
            descRT.anchorMin = new Vector2(0.06f, 0.32f);
            descRT.anchorMax = new Vector2(0.94f, 0.68f);
            descRT.offsetMin = Vector2.zero;
            descRT.offsetMax = Vector2.zero;

            // Badge info
            TextMeshProUGUI badgeTMP = CreateTextElement(cardObj.transform,
                $"Badge: {task.badge}", 14,
                TextAlignmentOptions.Center, diffColor, FontStyles.Italic);
            RectTransform badgeRT = badgeTMP.rectTransform;
            badgeRT.anchorMin = new Vector2(0.05f, 0.24f);
            badgeRT.anchorMax = new Vector2(0.95f, 0.32f);
            badgeRT.offsetMin = Vector2.zero;
            badgeRT.offsetMax = Vector2.zero;

            // Best Score
            float bestScore = GameManager.Instance?.Progress?.GetBestScore(task.taskId, "Test") ?? 0f;
            string scoreStr = bestScore > 0 ? $"Best: {bestScore:F0}%" : "Not attempted";
            TextMeshProUGUI scoreTMP = CreateTextElement(cardObj.transform, scoreStr, 14,
                TextAlignmentOptions.Center, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal);
            RectTransform scoreRT = scoreTMP.rectTransform;
            scoreRT.anchorMin = new Vector2(0.05f, 0.17f);
            scoreRT.anchorMax = new Vector2(0.95f, 0.24f);
            scoreRT.offsetMin = Vector2.zero;
            scoreRT.offsetMax = Vector2.zero;

            // Button
            if (unlocked)
            {
                string capturedId = task.taskId;
                CreateCardButton(cardObj.transform, "SELECT", buttonColor,
                    new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.15f),
                    () => OnTaskSelected(capturedId));
            }
            else
            {
                CreateCardButton(cardObj.transform, "LOCKED", new Color(0.3f, 0.3f, 0.3f),
                    new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.15f), () => { });
            }

            return cardObj;
        }

        private void CreateNavigationButtons()
        {
            int totalPages = Mathf.CeilToInt((float)allTasks.Length / CARDS_PER_PAGE);
            if (totalPages <= 1) return;

            // Previous page
            CreateCardButton(canvas.transform, "<", new Color(0.3f, 0.3f, 0.4f),
                new Vector2(0.01f, 0.4f), new Vector2(0.04f, 0.6f), OnPreviousPage);

            // Next page
            CreateCardButton(canvas.transform, ">", new Color(0.3f, 0.3f, 0.4f),
                new Vector2(0.96f, 0.4f), new Vector2(0.99f, 0.6f), OnNextPage);
        }

        private void OnPreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                CreateTaskCards();
            }
        }

        private void OnNextPage()
        {
            int totalPages = Mathf.CeilToInt((float)allTasks.Length / CARDS_PER_PAGE);
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                CreateTaskCards();
            }
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

            TextMeshProUGUI modeTitle = CreateTextElement(modePanelObj.transform,
                "Select Training Mode", 32, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            RectTransform modeTitleRT = modeTitle.rectTransform;
            modeTitleRT.anchorMin = new Vector2(0.05f, 0.75f);
            modeTitleRT.anchorMax = new Vector2(0.95f, 0.95f);
            modeTitleRT.offsetMin = Vector2.zero;
            modeTitleRT.offsetMax = Vector2.zero;

            CreateCardButton(modePanelObj.transform, "LEARN\nGuided Tour", new Color(0.2f, 0.7f, 0.3f),
                new Vector2(0.03f, 0.25f), new Vector2(0.32f, 0.70f),
                () => OnModeSelected(TaskMode.Learn));

            CreateCardButton(modePanelObj.transform, "PRACTICE\nWith Hints", new Color(0.2f, 0.5f, 0.9f),
                new Vector2(0.35f, 0.25f), new Vector2(0.65f, 0.70f),
                () => OnModeSelected(TaskMode.Practice));

            CreateCardButton(modePanelObj.transform, "TEST\nTimed, No Help", new Color(0.9f, 0.3f, 0.2f),
                new Vector2(0.68f, 0.25f), new Vector2(0.97f, 0.70f),
                () => OnModeSelected(TaskMode.Test));

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

            TextMeshProUGUI btnText = CreateTextElement(btnObj.transform, text, 20,
                TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            SetFullAnchor(btnText.rectTransform);
        }

        // --- Events ---

        private void OnTaskSelected(string taskId)
        {
            AudioManager.Instance.PlayButtonClick();
            selectedTaskId = taskId;

            if (cardContainer != null) cardContainer.SetActive(false);
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
            if (cardContainer != null) cardContainer.SetActive(true);
        }

        // --- Show/Hide ---

        public void Show()
        {
            gameObject.SetActive(true);
            PositionInFrontOfPlayer();

            currentPage = 0;
            CreateTaskCards();
            if (cardContainer != null) cardContainer.SetActive(true);
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
