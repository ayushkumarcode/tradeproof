using UnityEngine;
using TMPro;
using System.Collections.Generic;
using TradeProof.Core;

namespace TradeProof.UI
{
    /// <summary>
    /// Shows earned badges in a VR panel.
    /// Export button generates JSON for marketplace sync.
    /// </summary>
    public class BadgeDisplayUI : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas canvas;

        [Header("Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI badgeCountText;
        [SerializeField] private RectTransform badgeContainer;
        [SerializeField] private TextMeshProUGUI noBadgesText;
        [SerializeField] private UnityEngine.UI.Button exportButton;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private TextMeshProUGUI exportStatusText;

        [Header("Positioning")]
        [SerializeField] private float distanceFromPlayer = 1.2f;
        [SerializeField] private float heightOffset = 0.15f;

        [Header("Badge Card Settings")]
        [SerializeField] private float cardWidth = 220f;
        [SerializeField] private float cardHeight = 140f;
        [SerializeField] private float cardSpacing = 20f;

        [Header("Colors")]
        [SerializeField] private Color bgColor = new Color(0.04f, 0.06f, 0.12f, 0.95f);
        [SerializeField] private Color badgeCardColor = new Color(0.1f, 0.15f, 0.25f, 0.9f);
        [SerializeField] private Color goldColor = new Color(1f, 0.85f, 0.3f);
        [SerializeField] private Color silverColor = new Color(0.8f, 0.8f, 0.85f);

        private List<GameObject> badgeCards = new List<GameObject>();

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
            rt.sizeDelta = new Vector2(800f, 600f);
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
            titleText = CreateText("Title", "Your Badges", 42, FontStyles.Bold,
                new Vector2(0.05f, 0.85f), new Vector2(0.65f, 0.98f), TextAlignmentOptions.Left, Color.white);

            // Badge count
            badgeCountText = CreateText("BadgeCount", "0 badges earned", 20, FontStyles.Normal,
                new Vector2(0.65f, 0.88f), new Vector2(0.95f, 0.96f), TextAlignmentOptions.Right,
                new Color(0.6f, 0.6f, 0.6f));

            // Badge container
            GameObject containerObj = new GameObject("BadgeContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            badgeContainer = containerObj.AddComponent<RectTransform>();
            badgeContainer.anchorMin = new Vector2(0.03f, 0.18f);
            badgeContainer.anchorMax = new Vector2(0.97f, 0.82f);
            badgeContainer.offsetMin = Vector2.zero;
            badgeContainer.offsetMax = Vector2.zero;

            // No badges text
            noBadgesText = CreateText("NoBadges",
                "No badges earned yet.\nComplete training tasks in Test mode\nwith a score of 80% or higher.",
                22, FontStyles.Italic,
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f), TextAlignmentOptions.Center,
                new Color(0.5f, 0.5f, 0.5f));

            // Export button
            CreateButton("ExportButton", "EXPORT BADGES (JSON)", new Color(0.2f, 0.6f, 0.3f),
                new Vector2(0.05f, 0.03f), new Vector2(0.45f, 0.14f), OnExportClicked, out exportButton);

            // Close button
            CreateButton("CloseButton", "CLOSE", new Color(0.4f, 0.4f, 0.4f),
                new Vector2(0.55f, 0.03f), new Vector2(0.95f, 0.14f), OnCloseClicked, out closeButton);

            // Export status
            exportStatusText = CreateText("ExportStatus", "", 16, FontStyles.Italic,
                new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.18f), TextAlignmentOptions.Center,
                new Color(0.5f, 0.8f, 0.5f));
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

        private void CreateButton(string name, string text, Color color,
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
            btnText.fontSize = 18;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
        }

        // --- Display ---

        public void Show()
        {
            gameObject.SetActive(true);
            PositionInFrontOfPlayer();
            PopulateBadges();
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

        private void PopulateBadges()
        {
            // Clear existing cards
            foreach (var card in badgeCards)
            {
                if (card != null) Destroy(card);
            }
            badgeCards.Clear();

            List<Badge> badges = BadgeSystem.Instance.GetAllBadges();
            int badgeCount = badges.Count;

            badgeCountText.text = $"{badgeCount} badge{(badgeCount != 1 ? "s" : "")} earned";
            noBadgesText.gameObject.SetActive(badgeCount == 0);

            if (badgeCount == 0) return;

            // Calculate layout
            float containerWidth = 750f; // Approximate container width
            int columns = Mathf.Max(1, Mathf.FloorToInt(containerWidth / (cardWidth + cardSpacing)));
            int rows = Mathf.CeilToInt((float)badgeCount / columns);

            for (int i = 0; i < badges.Count; i++)
            {
                Badge badge = badges[i];
                int col = i % columns;
                int row = i / columns;

                float xPos = col * (cardWidth + cardSpacing);
                float yPos = -(row * (cardHeight + cardSpacing));

                CreateBadgeCard(badge, new Vector2(xPos, yPos));
            }
        }

        private void CreateBadgeCard(Badge badge, Vector2 position)
        {
            GameObject cardObj = new GameObject($"Badge_{badge.badgeId}");
            cardObj.transform.SetParent(badgeContainer, false);

            RectTransform cardRT = cardObj.AddComponent<RectTransform>();
            cardRT.anchoredPosition = position;
            cardRT.sizeDelta = new Vector2(cardWidth, cardHeight);
            cardRT.pivot = new Vector2(0f, 1f);

            UnityEngine.UI.Image cardBg = cardObj.AddComponent<UnityEngine.UI.Image>();
            cardBg.color = badgeCardColor;

            // Badge name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);

            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = badge.badgeName;
            nameTmp.fontSize = 18;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.TopLeft;
            nameTmp.color = goldColor;

            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.08f, 0.55f);
            nameRT.anchorMax = new Vector2(0.92f, 0.9f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(cardObj.transform, false);

            TextMeshProUGUI scoreTmp = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreTmp.text = $"Score: {badge.score:F0}%";
            scoreTmp.fontSize = 14;
            scoreTmp.alignment = TextAlignmentOptions.Left;
            scoreTmp.color = silverColor;

            RectTransform scoreRT = scoreObj.GetComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.08f, 0.3f);
            scoreRT.anchorMax = new Vector2(0.92f, 0.55f);
            scoreRT.offsetMin = Vector2.zero;
            scoreRT.offsetMax = Vector2.zero;

            // Date
            GameObject dateObj = new GameObject("Date");
            dateObj.transform.SetParent(cardObj.transform, false);

            TextMeshProUGUI dateTmp = dateObj.AddComponent<TextMeshProUGUI>();
            dateTmp.text = badge.dateEarned;
            dateTmp.fontSize = 12;
            dateTmp.alignment = TextAlignmentOptions.Left;
            dateTmp.color = new Color(0.5f, 0.5f, 0.5f);

            RectTransform dateRT = dateObj.GetComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0.08f, 0.05f);
            dateRT.anchorMax = new Vector2(0.92f, 0.3f);
            dateRT.offsetMin = Vector2.zero;
            dateRT.offsetMax = Vector2.zero;

            // Difficulty indicator line
            GameObject diffLine = new GameObject("DifficultyLine");
            diffLine.transform.SetParent(cardObj.transform, false);

            UnityEngine.UI.Image lineImg = diffLine.AddComponent<UnityEngine.UI.Image>();
            Color lineColor = GetDifficultyColor(badge.difficulty);
            lineImg.color = lineColor;

            RectTransform lineRT = diffLine.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0f, 0.92f);
            lineRT.anchorMax = new Vector2(1f, 1f);
            lineRT.offsetMin = Vector2.zero;
            lineRT.offsetMax = Vector2.zero;

            badgeCards.Add(cardObj);
        }

        private Color GetDifficultyColor(string difficulty)
        {
            switch (difficulty)
            {
                case "beginner":
                    return new Color(0.2f, 0.8f, 0.4f);
                case "intermediate":
                    return new Color(0.9f, 0.6f, 0.1f);
                case "advanced":
                    return new Color(0.9f, 0.2f, 0.2f);
                case "special":
                    return new Color(0.6f, 0.3f, 0.9f);
                default:
                    return Color.gray;
            }
        }

        // --- Button Handlers ---

        private void OnExportClicked()
        {
            AudioManager.Instance.PlayButtonClick();

            string json = BadgeSystem.Instance.ExportBadgesAsJson();

            if (BadgeSystem.Instance.GetBadgeCount() == 0)
            {
                exportStatusText.text = "No badges to export.";
                exportStatusText.color = new Color(0.9f, 0.5f, 0.2f);
                return;
            }

            // Save to persistent data path
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "tradeproof_badges.json");
            System.IO.File.WriteAllText(filePath, json);

            exportStatusText.text = $"Exported to: {filePath}";
            exportStatusText.color = new Color(0.5f, 0.8f, 0.5f);

            Debug.Log($"[BadgeDisplayUI] Badges exported to: {filePath}");
            Debug.Log($"[BadgeDisplayUI] JSON:\n{json}");
        }

        private void OnCloseClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            Hide();
        }
    }
}
