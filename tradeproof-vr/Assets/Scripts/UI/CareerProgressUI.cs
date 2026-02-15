using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TradeProof.Core;
using TradeProof.Data;
using System.Collections.Generic;

namespace TradeProof.UI
{
    /// <summary>
    /// World-space canvas displaying career level, XP progress, stats, and skill tree.
    /// 800x600 canvas at 0.001 scale.
    /// </summary>
    public class CareerProgressUI : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject panelRoot;

        // Career level section
        private TextMeshProUGUI levelTitleText;
        private TextMeshProUGUI levelNameText;
        private Image xpBarBackground;
        private Image xpBarFill;
        private TextMeshProUGUI xpNumberText;

        // Stats section
        private TextMeshProUGUI daysCompletedText;
        private TextMeshProUGUI tasksPassedText;
        private TextMeshProUGUI passRateText;
        private TextMeshProUGUI badgesEarnedText;

        // Skill tree section
        private GameObject skillTreeContainer;
        private List<GameObject> taskEntries = new List<GameObject>();

        // All known task IDs and display names
        private static readonly (string id, string name, string tier)[] AllTasks = {
            ("panel-inspection-residential", "Panel Inspection", "Apprentice"),
            ("circuit-wiring-20a", "Circuit Wiring (20A)", "Apprentice"),
            ("outlet-installation-duplex", "Outlet Installation", "Apprentice"),
            ("switch-wiring-3way", "3-Way Switch Wiring", "Journeyman"),
            ("gfci-testing-residential", "GFCI Testing", "Journeyman"),
            ("conduit-bending-emt", "Conduit Bending (EMT)", "Journeyman"),
            ("troubleshooting-residential", "Troubleshooting", "Master"),
        };

        private Camera playerCamera;

        private void Awake()
        {
            CreateUI();
            Hide();
        }

        private void CreateUI()
        {
            // Canvas setup: 800x600, 0.001 scale
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(800f, 600f);
            canvasRT.localScale = Vector3.one * 0.001f;

            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            // Panel background
            panelRoot = new GameObject("CareerProgressPanel");
            panelRoot.transform.SetParent(canvas.transform, false);
            RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.12f, 0.95f);

            // Title: "CAREER PROGRESS"
            CreateTextElement("Title", "CAREER PROGRESS", 32, FontStyles.Bold,
                new Color(0.9f, 0.7f, 0.1f), TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f));

            // Career level section
            levelTitleText = CreateTextElement("LevelTitle", "CAREER LEVEL", 16, FontStyles.Bold,
                new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.79f), new Vector2(0.5f, 0.85f));

            levelNameText = CreateTextElement("LevelName", "Apprentice", 36, FontStyles.Bold,
                new Color(0.3f, 0.8f, 0.3f), TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.7f), new Vector2(0.5f, 0.8f));

            // XP bar
            CreateXPBar();

            // XP number
            xpNumberText = CreateTextElement("XPNumber", "0 / 500 XP", 18, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Right,
                new Vector2(0.55f, 0.66f), new Vector2(0.95f, 0.72f));

            // Stats section
            CreateTextElement("StatsTitle", "STATISTICS", 16, FontStyles.Bold,
                new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.Left,
                new Vector2(0.55f, 0.79f), new Vector2(0.95f, 0.85f));

            daysCompletedText = CreateTextElement("DaysCompleted", "Days: 0", 18, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Left,
                new Vector2(0.55f, 0.73f), new Vector2(0.95f, 0.79f));

            tasksPassedText = CreateTextElement("TasksPassed", "Tasks Passed: 0", 18, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Left,
                new Vector2(0.55f, 0.67f), new Vector2(0.95f, 0.73f));

            passRateText = CreateTextElement("PassRate", "Pass Rate: --", 18, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Left,
                new Vector2(0.55f, 0.61f), new Vector2(0.95f, 0.67f));

            badgesEarnedText = CreateTextElement("BadgesEarned", "Badges: 0", 18, FontStyles.Normal,
                Color.white, TextAlignmentOptions.Left,
                new Vector2(0.55f, 0.55f), new Vector2(0.95f, 0.61f));

            // Skill tree section
            CreateTextElement("SkillTreeTitle", "SKILL TREE", 16, FontStyles.Bold,
                new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.56f));

            CreateSkillTree();
        }

        private void CreateXPBar()
        {
            // XP bar background
            GameObject xpBgObj = new GameObject("XPBarBG");
            xpBgObj.transform.SetParent(panelRoot.transform, false);
            xpBarBackground = xpBgObj.AddComponent<Image>();
            xpBarBackground.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
            RectTransform bgRT = xpBgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.05f, 0.64f);
            bgRT.anchorMax = new Vector2(0.5f, 0.68f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // XP bar fill
            GameObject xpFillObj = new GameObject("XPBarFill");
            xpFillObj.transform.SetParent(xpBgObj.transform, false);
            xpBarFill = xpFillObj.AddComponent<Image>();
            xpBarFill.color = new Color(0.3f, 0.8f, 0.3f); // Green for Apprentice
            RectTransform fillRT = xpFillObj.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0f, 1f); // Width set dynamically
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
        }

        private void CreateSkillTree()
        {
            skillTreeContainer = new GameObject("SkillTreeContainer");
            skillTreeContainer.transform.SetParent(panelRoot.transform, false);
            RectTransform containerRT = skillTreeContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.05f, 0.05f);
            containerRT.anchorMax = new Vector2(0.95f, 0.5f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            float entryHeight = 1f / AllTasks.Length;

            for (int i = 0; i < AllTasks.Length; i++)
            {
                var (taskId, taskName, tier) = AllTasks[i];
                GameObject entry = CreateSkillTreeEntry(taskId, taskName, tier, i, entryHeight);
                taskEntries.Add(entry);
            }
        }

        private GameObject CreateSkillTreeEntry(string taskId, string taskName, string tier, int index, float height)
        {
            float yMin = 1f - (index + 1) * height;
            float yMax = 1f - index * height;

            GameObject entry = new GameObject($"Task_{taskId}");
            entry.transform.SetParent(skillTreeContainer.transform, false);
            RectTransform entryRT = entry.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(0f, yMin);
            entryRT.anchorMax = new Vector2(1f, yMax);
            entryRT.offsetMin = new Vector2(0f, 2f); // Small gap
            entryRT.offsetMax = new Vector2(0f, -2f);

            Image entryBg = entry.AddComponent<Image>();
            entryBg.color = new Color(0.08f, 0.1f, 0.18f, 0.8f);

            // Status icon area (left)
            GameObject iconObj = new GameObject("StatusIcon");
            iconObj.transform.SetParent(entry.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.01f, 0.15f);
            iconRT.anchorMax = new Vector2(0.06f, 0.85f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            // These will be updated in UpdateDisplay
            iconImg.color = new Color(0.4f, 0.4f, 0.4f); // Default gray (locked)

            // Task name
            GameObject nameObj = new GameObject("TaskName");
            nameObj.transform.SetParent(entry.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = taskName;
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.08f, 0f);
            nameRT.anchorMax = new Vector2(0.55f, 1f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            // Tier label
            GameObject tierObj = new GameObject("Tier");
            tierObj.transform.SetParent(entry.transform, false);
            TextMeshProUGUI tierText = tierObj.AddComponent<TextMeshProUGUI>();
            tierText.text = tier;
            tierText.fontSize = 14;
            tierText.color = GetTierColor(tier);
            tierText.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform tierRT = tierObj.GetComponent<RectTransform>();
            tierRT.anchorMin = new Vector2(0.55f, 0f);
            tierRT.anchorMax = new Vector2(0.72f, 1f);
            tierRT.offsetMin = Vector2.zero;
            tierRT.offsetMax = Vector2.zero;

            // Status text (locked / unlocked / best score)
            GameObject statusObj = new GameObject("Status");
            statusObj.transform.SetParent(entry.transform, false);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "LOCKED";
            statusText.fontSize = 16;
            statusText.color = new Color(0.5f, 0.5f, 0.5f);
            statusText.alignment = TextAlignmentOptions.MidlineRight;
            RectTransform statusRT = statusObj.GetComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0.72f, 0f);
            statusRT.anchorMax = new Vector2(0.98f, 1f);
            statusRT.offsetMin = Vector2.zero;
            statusRT.offsetMax = Vector2.zero;

            return entry;
        }

        private Color GetTierColor(string tier)
        {
            switch (tier)
            {
                case "Apprentice": return new Color(0.3f, 0.8f, 0.3f);
                case "Journeyman": return new Color(0.3f, 0.5f, 0.9f);
                case "Master": return new Color(0.9f, 0.7f, 0.1f);
                default: return Color.white;
            }
        }

        private Color GetLevelColor(CareerLevel level)
        {
            switch (level)
            {
                case CareerLevel.Apprentice: return new Color(0.3f, 0.8f, 0.3f);  // Green
                case CareerLevel.Journeyman: return new Color(0.3f, 0.5f, 0.9f);  // Blue
                case CareerLevel.Master: return new Color(0.9f, 0.75f, 0.1f);     // Gold
                default: return Color.white;
            }
        }

        private TextMeshProUGUI CreateTextElement(string name, string text, float fontSize,
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
            PositionInFrontOfPlayer();
            UpdateDisplay();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void PositionInFrontOfPlayer()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = GameManager.Instance.MainCamera;
            if (playerCamera == null) return;

            Vector3 forward = playerCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            transform.position = playerCamera.transform.position + forward * 1.2f + Vector3.up * 0.1f;
            transform.rotation = Quaternion.LookRotation(forward);
        }

        private void UpdateDisplay()
        {
            PlayerProgress progress = GameManager.Instance.Progress;
            if (progress == null) return;

            CareerLevel level = GameManager.Instance.CurrentCareerLevel;

            // Career level name + color
            levelNameText.text = level.ToString();
            Color levelColor = GetLevelColor(level);
            levelNameText.color = levelColor;

            // XP bar
            float xpProgress = progress.GetXPProgressToNextLevel();
            if (xpBarFill != null)
            {
                xpBarFill.color = levelColor;
                xpBarFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(xpProgress), 1f);
            }

            // XP number text
            int currentThreshold = PlayerProgress.CareerXPThresholds[progress.currentCareerLevel];
            int nextThreshold = progress.currentCareerLevel < PlayerProgress.CareerXPThresholds.Length - 1
                ? PlayerProgress.CareerXPThresholds[progress.currentCareerLevel + 1]
                : progress.totalXP;
            xpNumberText.text = $"{progress.totalXP} / {nextThreshold} XP";

            // Stats
            daysCompletedText.text = $"Days Completed: {progress.daysCompleted}";
            tasksPassedText.text = $"Tasks Passed: {progress.totalTasksPassed}";

            float passRate = progress.GetOverallPassRate();
            passRateText.text = $"Pass Rate: {passRate:F0}%";
            passRateText.color = passRate >= 80f ? new Color(0.3f, 0.8f, 0.3f) :
                                 passRate >= 60f ? new Color(0.9f, 0.7f, 0.1f) :
                                 new Color(0.9f, 0.3f, 0.2f);

            badgesEarnedText.text = $"Badges: {BadgeSystem.Instance.GetBadgeCount()}";

            // Skill tree entries
            for (int i = 0; i < AllTasks.Length && i < taskEntries.Count; i++)
            {
                UpdateSkillTreeEntry(taskEntries[i], AllTasks[i].id, progress);
            }
        }

        private void UpdateSkillTreeEntry(GameObject entry, string taskId, PlayerProgress progress)
        {
            bool unlocked = progress.IsTaskUnlocked(taskId);
            float bestScore = progress.GetBestScore(taskId, "Test");
            if (bestScore <= 0f) bestScore = progress.GetBestScore(taskId, "Practice");

            // Status icon
            Image iconImg = entry.transform.Find("StatusIcon")?.GetComponent<Image>();
            if (iconImg != null)
            {
                if (unlocked)
                {
                    if (bestScore >= 80f)
                        iconImg.color = new Color(0.2f, 0.8f, 0.2f); // Green check
                    else if (bestScore > 0f)
                        iconImg.color = new Color(0.9f, 0.7f, 0.1f); // Yellow in-progress
                    else
                        iconImg.color = new Color(0.3f, 0.5f, 0.9f); // Blue unlocked
                }
                else
                {
                    iconImg.color = new Color(0.3f, 0.3f, 0.3f); // Gray locked
                }
            }

            // Task name color
            TextMeshProUGUI nameText = entry.transform.Find("TaskName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // Status text
            TextMeshProUGUI statusText = entry.transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                if (!unlocked)
                {
                    statusText.text = "LOCKED";
                    statusText.color = new Color(0.5f, 0.5f, 0.5f);
                }
                else if (bestScore >= 80f)
                {
                    statusText.text = $"PASSED ({bestScore:F0}%)";
                    statusText.color = new Color(0.3f, 0.8f, 0.3f);
                }
                else if (bestScore > 0f)
                {
                    statusText.text = $"Best: {bestScore:F0}%";
                    statusText.color = new Color(0.9f, 0.7f, 0.1f);
                }
                else
                {
                    statusText.text = "AVAILABLE";
                    statusText.color = new Color(0.3f, 0.5f, 0.9f);
                }
            }
        }
    }
}
