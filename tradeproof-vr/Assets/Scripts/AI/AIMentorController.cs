using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TradeProof.Core;
using TradeProof.Data;
using TradeProof.UI;

namespace TradeProof.AI
{
    /// <summary>
    /// Floating AI mentor that follows the player and provides progressive hints.
    /// Visual: yellow hardhat icon (sphere + brim) with a world-space speech bubble.
    /// Adapts hint frequency based on PlayerProgress pass rate.
    /// </summary>
    public class AIMentorController : MonoBehaviour
    {
        private static AIMentorController _instance;
        public static AIMentorController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AIMentorController");
                    _instance = go.AddComponent<AIMentorController>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Follow Settings")]
        [SerializeField] private float followLerpSpeed = 3f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0.4f, 0.3f, 0.8f);

        [Header("Idle Hint Thresholds (seconds)")]
        [SerializeField] private float idleThresholdLevel0 = 5f;
        [SerializeField] private float idleThresholdLevel1 = 15f;
        [SerializeField] private float idleThresholdLevel2 = 30f;

        [Header("State")]
        private float idleTimer;
        private int lastHintLevelShown = -1;
        private string currentTaskId;
        private string currentStepId;
        private bool isActive;

        // Visual components
        private GameObject hardhatGroup;
        private GameObject hardhatSphere;
        private GameObject hardhatBrim;
        private MentorDialogueUI dialogueUI;

        // Greeting and encouragement messages
        private static readonly string[] Greetings = {
            "Ready to learn? Let's get started!",
            "Welcome back! Let's tackle this task.",
            "Good to see you! Time to build some skills.",
            "Let's do this! I'll be here if you need help."
        };

        private static readonly string[] CorrectActionMessages = {
            "Nice work!",
            "That's the way to do it!",
            "Spot on!",
            "You're getting the hang of this!",
            "Well done!"
        };

        private static readonly string[] MistakeConsolations = {
            "Don't worry, that's how we learn.",
            "Close! Let me explain what happened.",
            "Good attempt. Let's review that step.",
            "Almost there. Try a different approach.",
            "No problem, let's figure this out together."
        };

        private Camera playerCamera;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            CreateVisuals();
            CreateDialogueUI();
            Hide();
        }

        private void CreateVisuals()
        {
            hardhatGroup = new GameObject("HardhatIcon");
            hardhatGroup.transform.SetParent(transform, false);

            // Yellow sphere (hardhat dome) -- 0.06m radius
            hardhatSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hardhatSphere.name = "HardhatDome";
            hardhatSphere.transform.SetParent(hardhatGroup.transform, false);
            hardhatSphere.transform.localPosition = Vector3.zero;
            hardhatSphere.transform.localScale = Vector3.one * 0.12f; // diameter 0.12m = radius 0.06m
            Renderer sphereRend = hardhatSphere.GetComponent<Renderer>();
            Material hardhatMat = new Material(Shader.Find("Standard"));
            hardhatMat.color = new Color(1f, 0.85f, 0.1f); // bright yellow
            hardhatMat.SetFloat("_Glossiness", 0.7f);
            sphereRend.material = hardhatMat;

            // Remove collider from visual
            Collider sphereCol = hardhatSphere.GetComponent<Collider>();
            if (sphereCol != null) Object.Destroy(sphereCol);

            // Flat cylinder (brim)
            hardhatBrim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hardhatBrim.name = "HardhatBrim";
            hardhatBrim.transform.SetParent(hardhatGroup.transform, false);
            hardhatBrim.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            hardhatBrim.transform.localScale = new Vector3(0.18f, 0.005f, 0.18f);
            Renderer brimRend = hardhatBrim.GetComponent<Renderer>();
            Material brimMat = new Material(Shader.Find("Standard"));
            brimMat.color = new Color(0.95f, 0.8f, 0.05f);
            brimMat.SetFloat("_Glossiness", 0.6f);
            brimRend.material = brimMat;

            // Remove collider from brim
            Collider brimCol = hardhatBrim.GetComponent<Collider>();
            if (brimCol != null) Object.Destroy(brimCol);
        }

        private void CreateDialogueUI()
        {
            GameObject dialogueObj = new GameObject("MentorDialogueUI");
            dialogueObj.transform.SetParent(transform, false);
            dialogueObj.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            dialogueUI = dialogueObj.AddComponent<MentorDialogueUI>();
        }

        private void LateUpdate()
        {
            if (!isActive) return;

            UpdateCameraFollow();
            UpdateIdleTimer();
            AnimateHardhat();
        }

        private void UpdateCameraFollow()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            // Target position: upper-right of player's view
            Vector3 camPos = playerCamera.transform.position;
            Vector3 camForward = playerCamera.transform.forward;
            Vector3 camRight = playerCamera.transform.right;
            Vector3 camUp = playerCamera.transform.up;

            Vector3 targetPos = camPos
                + camForward * cameraOffset.z
                + camRight * cameraOffset.x
                + camUp * cameraOffset.y;

            // Smooth follow via lerp
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followLerpSpeed);

            // Face the camera
            Vector3 lookDir = playerCamera.transform.position - transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(-lookDir.normalized, Vector3.up);
            }
        }

        private void AnimateHardhat()
        {
            if (hardhatGroup == null) return;

            // Subtle bob animation
            float bob = Mathf.Sin(Time.time * 2f) * 0.005f;
            hardhatGroup.transform.localPosition = new Vector3(0f, bob, 0f);
        }

        private void UpdateIdleTimer()
        {
            idleTimer += Time.deltaTime;

            float hintMultiplier = GetHintDelayMultiplier();

            float threshold0 = idleThresholdLevel0 * hintMultiplier;
            float threshold1 = idleThresholdLevel1 * hintMultiplier;
            float threshold2 = idleThresholdLevel2 * hintMultiplier;

            if (idleTimer >= threshold2 && lastHintLevelShown < 2)
            {
                OnPlayerIdle(idleTimer);
                ShowProgressiveHint(2);
            }
            else if (idleTimer >= threshold1 && lastHintLevelShown < 1)
            {
                OnPlayerIdle(idleTimer);
                ShowProgressiveHint(1);
            }
            else if (idleTimer >= threshold0 && lastHintLevelShown < 0)
            {
                OnPlayerIdle(idleTimer);
                ShowProgressiveHint(0);
            }
        }

        /// <summary>
        /// Returns a multiplier for hint delay based on the player's pass rate.
        /// Players with >80% pass rate get doubled thresholds (fewer hints).
        /// </summary>
        private float GetHintDelayMultiplier()
        {
            PlayerProgress progress = GameManager.Instance.Progress;
            if (progress != null && progress.GetOverallPassRate() > 80f)
            {
                return 2f;
            }
            return 1f;
        }

        private void ShowProgressiveHint(int level)
        {
            lastHintLevelShown = level;

            string hintText = MentorHintDatabase.GetHint(currentTaskId, currentStepId, level);
            if (!string.IsNullOrEmpty(hintText))
            {
                ShowMessage(hintText, 6f);
                AudioManager.Instance.PlayHintSound();
            }

            Debug.Log($"[AIMentor] Showing level {level} hint for task={currentTaskId}, step={currentStepId}");
        }

        // --- Public API ---

        /// <summary>
        /// Called when the player performs any action. Resets the idle timer.
        /// </summary>
        public void OnPlayerAction(string actionType)
        {
            idleTimer = 0f;
            lastHintLevelShown = -1;

            Debug.Log($"[AIMentor] Player action: {actionType}");
        }

        /// <summary>
        /// Called when player has been idle for the given number of seconds.
        /// </summary>
        public void OnPlayerIdle(float seconds)
        {
            Debug.Log($"[AIMentor] Player idle for {seconds:F1}s");
        }

        /// <summary>
        /// Displays a message in the speech bubble for the given duration.
        /// </summary>
        public void ShowMessage(string message, float duration = 5f)
        {
            if (dialogueUI != null)
            {
                dialogueUI.ShowMessage(message, duration);
            }
        }

        /// <summary>
        /// Activates the mentor for a new task. Shows a greeting.
        /// </summary>
        public void ActivateForTask(string taskId, string initialStepId = "")
        {
            currentTaskId = taskId;
            currentStepId = initialStepId;
            idleTimer = 0f;
            lastHintLevelShown = -1;

            Show();

            // Greeting message
            string greeting = Greetings[Random.Range(0, Greetings.Length)];
            ShowMessage(greeting, 4f);
        }

        /// <summary>
        /// Updates the current step being tracked for hint context.
        /// </summary>
        public void SetCurrentStep(string stepId)
        {
            if (currentStepId != stepId)
            {
                currentStepId = stepId;
                lastHintLevelShown = -1;
                idleTimer = 0f;
            }
        }

        /// <summary>
        /// Called when the player performs a correct action. Shows encouragement.
        /// </summary>
        public void OnCorrectAction()
        {
            OnPlayerAction("correct");
            string msg = MentorHintDatabase.GetEncouragement();
            ShowMessage(msg, 3f);
            AudioManager.Instance.PlayCorrectSound();
        }

        /// <summary>
        /// Called when the player makes a mistake. Shows consolation and correction info.
        /// </summary>
        public void OnMistake(string taskId, string stepId)
        {
            OnPlayerAction("mistake");
            string consolation = MistakeConsolations[Random.Range(0, MistakeConsolations.Length)];
            string correction = MentorHintDatabase.GetCorrection(taskId, stepId);

            if (!string.IsNullOrEmpty(correction))
            {
                ShowMessage($"{consolation}\n{correction}", 6f);
            }
            else
            {
                ShowMessage(consolation, 4f);
            }

            AudioManager.Instance.PlayIncorrectSound();
        }

        /// <summary>
        /// Called when a task is completed successfully.
        /// </summary>
        public void OnTaskComplete(bool passed, float score)
        {
            if (passed)
            {
                if (score >= 95f)
                    ShowMessage("Outstanding work! Nearly perfect!", 5f);
                else if (score >= 85f)
                    ShowMessage("Great job! You really know your stuff!", 5f);
                else
                    ShowMessage("Good work! You passed! Keep practicing to improve.", 5f);
            }
            else
            {
                ShowMessage("Don't give up! Review the results and try again.", 5f);
            }
        }

        public void Show()
        {
            isActive = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            isActive = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Deactivates the mentor. Called when leaving a task.
        /// </summary>
        public void Deactivate()
        {
            currentTaskId = null;
            currentStepId = null;
            Hide();
        }
    }
}
