using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Data;
using TMPro;

namespace TradeProof.UI
{
    public class DayBriefingUI : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject panelRoot;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI mentorText;
        private TextMeshProUGUI careerText;
        private List<GameObject> workOrderCards = new List<GameObject>();
        private List<WorkOrder> todaysOrders = new List<WorkOrder>();

        private void Awake()
        {
            CreateUI();
            Hide();
        }

        private void CreateUI()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 800);
            canvas.transform.localScale = Vector3.one * 0.001f;

            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("DayBriefingPanel");
            panelRoot.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelRoot.transform, false);
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "MORNING BRIEFING";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.9f, 0.7f, 0.1f);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Mentor greeting
            GameObject mentorObj = new GameObject("MentorGreeting");
            mentorObj.transform.SetParent(panelRoot.transform, false);
            mentorText = mentorObj.AddComponent<TextMeshProUGUI>();
            mentorText.text = "Good morning! Here are today's jobs...";
            mentorText.fontSize = 24;
            mentorText.alignment = TextAlignmentOptions.Center;
            mentorText.color = new Color(0.8f, 0.8f, 0.8f);
            RectTransform mentorRect = mentorObj.GetComponent<RectTransform>();
            mentorRect.anchorMin = new Vector2(0.1f, 0.78f);
            mentorRect.anchorMax = new Vector2(0.9f, 0.85f);
            mentorRect.offsetMin = Vector2.zero;
            mentorRect.offsetMax = Vector2.zero;

            // Career level
            GameObject careerObj = new GameObject("CareerLevel");
            careerObj.transform.SetParent(panelRoot.transform, false);
            careerText = careerObj.AddComponent<TextMeshProUGUI>();
            careerText.fontSize = 20;
            careerText.alignment = TextAlignmentOptions.Center;
            careerText.color = new Color(0.5f, 0.8f, 1f);
            RectTransform careerRect = careerObj.GetComponent<RectTransform>();
            careerRect.anchorMin = new Vector2(0.1f, 0.72f);
            careerRect.anchorMax = new Vector2(0.9f, 0.78f);
            careerRect.offsetMin = Vector2.zero;
            careerRect.offsetMax = Vector2.zero;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            PositionInFrontOfPlayer();
            GenerateWorkOrders();
            UpdateDisplay();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ShowDayResults()
        {
            gameObject.SetActive(true);
            PositionInFrontOfPlayer();
            titleText.text = "END OF DAY REPORT";
            mentorText.text = "Great work today! Here's your summary.";

            var progress = GameManager.Instance.Progress;
            careerText.text = $"Career: {GameManager.Instance.CurrentCareerLevel} | XP: {progress.totalXP} | Day Streak: {progress.currentDayStreak}";

            ClearWorkOrderCards();
        }

        private void PositionInFrontOfPlayer()
        {
            Camera cam = GameManager.Instance.MainCamera;
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                Vector3 forward = cam.transform.forward;
                forward.y = 0;
                forward.Normalize();
                transform.position = cam.transform.position + forward * 1.5f + Vector3.up * 0.2f;
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        private void GenerateWorkOrders()
        {
            CareerLevel level = GameManager.Instance.CurrentCareerLevel;
            int orderCount = level == CareerLevel.Apprentice ? 3 : (level == CareerLevel.Journeyman ? 4 : 5);
            todaysOrders = WorkOrderGenerator.Generate(orderCount, level);
        }

        private void UpdateDisplay()
        {
            var progress = GameManager.Instance.Progress;
            CareerLevel level = GameManager.Instance.CurrentCareerLevel;

            careerText.text = $"Career: {level} | XP: {progress.totalXP} | Day {progress.daysCompleted + 1}";

            string[] greetings = {
                "Good morning! Here are today's service calls.",
                "Rise and shine! We've got some work lined up.",
                "Morning! Let's see what's on the schedule today.",
                "Another day, another circuit! Here's your lineup."
            };
            mentorText.text = greetings[Random.Range(0, greetings.Length)];

            ClearWorkOrderCards();
            CreateWorkOrderCards();
        }

        private void ClearWorkOrderCards()
        {
            foreach (var card in workOrderCards)
                Destroy(card);
            workOrderCards.Clear();
        }

        private void CreateWorkOrderCards()
        {
            float cardWidth = 300f;
            float cardHeight = 200f;
            float spacing = 20f;
            float startX = -(todaysOrders.Count * (cardWidth + spacing) - spacing) / 2f + cardWidth / 2f;

            for (int i = 0; i < todaysOrders.Count; i++)
            {
                var order = todaysOrders[i];
                GameObject card = CreateWorkOrderCard(order, i);
                RectTransform cardRect = card.GetComponent<RectTransform>();
                cardRect.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), -100f);
                workOrderCards.Add(card);
            }
        }

        private GameObject CreateWorkOrderCard(WorkOrder order, int index)
        {
            GameObject card = new GameObject($"WorkOrderCard_{index}");
            card.transform.SetParent(panelRoot.transform, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(300, 200);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);

            Image cardBg = card.AddComponent<Image>();
            Color priorityColor = order.priority == WorkOrderPriority.Urgent ? new Color(0.3f, 0.1f, 0.1f, 0.9f) :
                                  order.priority == WorkOrderPriority.Normal ? new Color(0.1f, 0.15f, 0.25f, 0.9f) :
                                  new Color(0.1f, 0.2f, 0.1f, 0.9f);
            cardBg.color = priorityColor;

            // Priority bar
            GameObject priorityBar = new GameObject("PriorityBar");
            priorityBar.transform.SetParent(card.transform, false);
            RectTransform barRect = priorityBar.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0, 0.9f);
            barRect.anchorMax = new Vector2(1, 1);
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;
            Image barImage = priorityBar.AddComponent<Image>();
            barImage.color = order.priority == WorkOrderPriority.Urgent ? new Color(0.9f, 0.2f, 0.2f) :
                             order.priority == WorkOrderPriority.Normal ? new Color(0.9f, 0.7f, 0.1f) :
                             new Color(0.3f, 0.7f, 0.3f);

            // Customer name
            GameObject nameObj = new GameObject("CustomerName");
            nameObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = order.customerName;
            nameText.fontSize = 20;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.7f);
            nameRect.anchorMax = new Vector2(0.95f, 0.88f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Address
            GameObject addrObj = new GameObject("Address");
            addrObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI addrText = addrObj.AddComponent<TextMeshProUGUI>();
            addrText.text = order.address;
            addrText.fontSize = 14;
            addrText.color = new Color(0.6f, 0.6f, 0.6f);
            RectTransform addrRect = addrObj.GetComponent<RectTransform>();
            addrRect.anchorMin = new Vector2(0.05f, 0.6f);
            addrRect.anchorMax = new Vector2(0.95f, 0.7f);
            addrRect.offsetMin = Vector2.zero;
            addrRect.offsetMax = Vector2.zero;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = order.description;
            descText.fontSize = 16;
            descText.color = new Color(0.9f, 0.9f, 0.9f);
            descText.enableWordWrapping = true;
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.05f, 0.25f);
            descRect.anchorMax = new Vector2(0.95f, 0.6f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;

            // XP reward
            GameObject xpObj = new GameObject("XPReward");
            xpObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI xpText = xpObj.AddComponent<TextMeshProUGUI>();
            string multiplierStr = order.bonusMultiplier > 1f ? $" (x{order.bonusMultiplier:F1})" : "";
            xpText.text = $"+{order.xpReward} XP{multiplierStr}";
            xpText.fontSize = 16;
            xpText.fontStyle = FontStyles.Bold;
            xpText.color = new Color(0.5f, 0.8f, 1f);
            xpText.alignment = TextAlignmentOptions.Right;
            RectTransform xpRect = xpObj.GetComponent<RectTransform>();
            xpRect.anchorMin = new Vector2(0.5f, 0.05f);
            xpRect.anchorMax = new Vector2(0.95f, 0.2f);
            xpRect.offsetMin = Vector2.zero;
            xpRect.offsetMax = Vector2.zero;

            // Accept button
            GameObject btnObj = new GameObject("AcceptButton");
            btnObj.transform.SetParent(card.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.05f, 0.05f);
            btnRect.anchorMax = new Vector2(0.45f, 0.2f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.5f, 0.8f);
            Button btn = btnObj.AddComponent<Button>();

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "ACCEPT";
            btnText.fontSize = 14;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            WorkOrder capturedOrder = order;
            btn.onClick.AddListener(() => OnAcceptWorkOrder(capturedOrder));

            return card;
        }

        private void OnAcceptWorkOrder(WorkOrder order)
        {
            AudioManager.Instance.PlayButtonClickSound();
            GameManager.Instance.StartWorkOrder(order, Training.TaskMode.Practice);
        }
    }
}
