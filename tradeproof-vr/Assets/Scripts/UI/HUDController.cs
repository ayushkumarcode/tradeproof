using UnityEngine;
using TMPro;
using TradeProof.Core;
using TradeProof.Training;

namespace TradeProof.UI
{
    /// <summary>
    /// Heads-up display that follows user gaze slightly.
    /// Shows timer (test mode), violations found count, current mode, hint button (practice mode).
    /// Maintains readable distance from user (0.5-1.5m).
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas canvas;

        [Header("Elements")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI taskNameText;
        [SerializeField] private UnityEngine.UI.Button hintButton;
        [SerializeField] private UnityEngine.UI.Button finishButton;
        [SerializeField] private UnityEngine.UI.Image progressBar;
        [SerializeField] private UnityEngine.UI.Image progressBarBackground;

        [Header("Follow Settings")]
        [SerializeField] private float followDistance = 0.8f;
        [SerializeField] private float followHeightOffset = 0.25f;
        [SerializeField] private float followSpeed = 3f;
        [SerializeField] private float gazeFollowStrength = 0.3f; // How much it follows gaze vs staying centered

        [Header("Visual")]
        [SerializeField] private Color hudBackground = new Color(0.02f, 0.04f, 0.08f, 0.7f);
        [SerializeField] private Color timerNormalColor = Color.white;
        [SerializeField] private Color timerWarningColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color timerCriticalColor = new Color(1f, 0.1f, 0.1f);

        private Camera playerCamera;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool timerWarningTriggered;

        private void Awake()
        {
            SetupCanvas();
            SetupHUDElements();
        }

        private void Start()
        {
            playerCamera = Camera.main;
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
            rt.sizeDelta = new Vector2(600f, 200f);
            rt.localScale = Vector3.one * 0.0008f;

            UnityEngine.UI.Image bg = canvas.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (bg == null)
            {
                bg = canvas.gameObject.AddComponent<UnityEngine.UI.Image>();
            }
            bg.color = hudBackground;
        }

        private void SetupHUDElements()
        {
            // Task name
            taskNameText = CreateText("TaskName", "Training Task", 22, FontStyles.Bold,
                new Vector2(0.02f, 0.72f), new Vector2(0.7f, 0.98f));

            // Mode indicator
            modeText = CreateText("Mode", "LEARN", 18, FontStyles.Bold,
                new Vector2(0.72f, 0.72f), new Vector2(0.98f, 0.98f));
            modeText.alignment = TextAlignmentOptions.Right;

            // Timer
            timerText = CreateText("Timer", "", 28, FontStyles.Bold,
                new Vector2(0.72f, 0.4f), new Vector2(0.98f, 0.7f));
            timerText.alignment = TextAlignmentOptions.Right;
            timerText.color = timerNormalColor;

            // Progress text
            progressText = CreateText("Progress", "0/0 Found", 20, FontStyles.Normal,
                new Vector2(0.02f, 0.4f), new Vector2(0.5f, 0.65f));

            // Progress bar background
            GameObject pbBgObj = new GameObject("ProgressBarBG");
            pbBgObj.transform.SetParent(canvas.transform, false);
            progressBarBackground = pbBgObj.AddComponent<UnityEngine.UI.Image>();
            progressBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform pbBgRT = pbBgObj.GetComponent<RectTransform>();
            pbBgRT.anchorMin = new Vector2(0.02f, 0.28f);
            pbBgRT.anchorMax = new Vector2(0.68f, 0.38f);
            pbBgRT.offsetMin = Vector2.zero;
            pbBgRT.offsetMax = Vector2.zero;

            // Progress bar fill
            GameObject pbObj = new GameObject("ProgressBarFill");
            pbObj.transform.SetParent(pbBgObj.transform, false);
            progressBar = pbObj.AddComponent<UnityEngine.UI.Image>();
            progressBar.color = new Color(0.2f, 0.8f, 0.4f, 1f);
            RectTransform pbRT = pbObj.GetComponent<RectTransform>();
            pbRT.anchorMin = Vector2.zero;
            pbRT.anchorMax = new Vector2(0f, 1f); // Width updated dynamically
            pbRT.offsetMin = Vector2.zero;
            pbRT.offsetMax = Vector2.zero;

            // Hint button
            CreateButton("HintButton", "HINT", new Color(0.9f, 0.7f, 0.1f),
                new Vector2(0.02f, 0.02f), new Vector2(0.32f, 0.24f),
                OnHintClicked, out hintButton);

            // Finish button
            CreateButton("FinishButton", "FINISH", new Color(0.3f, 0.6f, 0.9f),
                new Vector2(0.36f, 0.02f), new Vector2(0.68f, 0.24f),
                OnFinishClicked, out finishButton);
        }

        private TextMeshProUGUI CreateText(string name, string text, float fontSize, FontStyles style,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(canvas.transform, false);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return tmp;
        }

        private void CreateButton(string name, string text, Color bgColor,
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
            btnImg.color = bgColor;

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

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            UpdatePosition();
            UpdateHUDContent();
        }

        private void UpdatePosition()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            // Calculate target position: slightly below and in front of gaze
            Vector3 gazeForward = playerCamera.transform.forward;
            Vector3 levelForward = gazeForward;
            levelForward.y = 0f;
            levelForward.Normalize();

            // Blend between level forward and gaze forward for subtle follow
            Vector3 blendedForward = Vector3.Lerp(levelForward, gazeForward, gazeFollowStrength);
            blendedForward.Normalize();

            targetPosition = playerCamera.transform.position +
                             blendedForward * followDistance +
                             Vector3.up * followHeightOffset;

            targetRotation = Quaternion.LookRotation(
                (targetPosition - playerCamera.transform.position).normalized,
                Vector3.up);

            // Smoothly follow
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
        }

        private void UpdateHUDContent()
        {
            TaskManager tm = TaskManager.Instance;
            GameManager gm = GameManager.Instance;

            if (gm.CurrentState != GameState.Training) return;

            // Task name
            if (taskNameText != null)
            {
                taskNameText.text = gm.CurrentTaskId;
            }

            // Mode
            if (modeText != null)
            {
                modeText.text = gm.CurrentTaskMode.ToString().ToUpper();
                switch (gm.CurrentTaskMode)
                {
                    case TaskMode.Learn:
                        modeText.color = new Color(0.2f, 0.8f, 0.4f);
                        break;
                    case TaskMode.Practice:
                        modeText.color = new Color(0.2f, 0.5f, 0.9f);
                        break;
                    case TaskMode.Test:
                        modeText.color = new Color(0.9f, 0.3f, 0.2f);
                        break;
                }
            }

            // Timer (test mode only)
            if (timerText != null)
            {
                if (gm.CurrentTaskMode == TaskMode.Test && tm.IsTimerRunning)
                {
                    float timeLeft = tm.TimeRemaining;
                    int minutes = Mathf.FloorToInt(timeLeft / 60f);
                    int seconds = Mathf.FloorToInt(timeLeft % 60f);
                    timerText.text = $"{minutes}:{seconds:D2}";

                    // Color changes at thresholds
                    if (timeLeft <= 30f)
                    {
                        timerText.color = timerCriticalColor;
                        if (!timerWarningTriggered && timeLeft <= 30f)
                        {
                            timerWarningTriggered = true;
                            AudioManager.Instance.PlayTimerTickSound();
                        }
                    }
                    else if (timeLeft <= 60f)
                    {
                        timerText.color = timerWarningColor;
                    }
                    else
                    {
                        timerText.color = timerNormalColor;
                    }

                    timerText.gameObject.SetActive(true);
                }
                else
                {
                    timerText.gameObject.SetActive(false);
                }
            }

            // Progress
            if (progressText != null)
            {
                if (tm.TotalViolations > 0)
                {
                    progressText.text = $"{tm.ViolationsFound}/{tm.TotalViolations} Violations Found";
                }
                else if (tm.TotalSteps > 0)
                {
                    progressText.text = $"Step {tm.StepsCompleted}/{tm.TotalSteps}";
                }
            }

            // Progress bar
            if (progressBar != null)
            {
                float progress = 0f;
                if (tm.TotalViolations > 0)
                    progress = (float)tm.ViolationsFound / tm.TotalViolations;
                else if (tm.TotalSteps > 0)
                    progress = (float)tm.StepsCompleted / tm.TotalSteps;

                RectTransform fillRT = progressBar.rectTransform;
                fillRT.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
            }

            // Show/hide hint button
            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(gm.CurrentTaskMode == TaskMode.Practice);
            }
        }

        // --- Button Handlers ---

        private void OnHintClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            TrainingTask activeTask = FindObjectOfType<TrainingTask>();
            if (activeTask != null)
            {
                activeTask.RequestHint();
            }
        }

        private void OnFinishClicked()
        {
            AudioManager.Instance.PlayButtonClick();
            TaskManager.Instance.ForceFinishTask();
        }

        // --- Show/Hide ---

        public void Show()
        {
            gameObject.SetActive(true);
            timerWarningTriggered = false;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
