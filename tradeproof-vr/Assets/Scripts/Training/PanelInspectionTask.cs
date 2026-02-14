using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Data;
using TradeProof.UI;

namespace TradeProof.Training
{
    /// <summary>
    /// Panel Inspection training task. User identifies NEC code violations in a residential electrical panel.
    /// All violation markers are child objects of the panel with LOCAL position offsets.
    /// </summary>
    public class PanelInspectionTask : TrainingTask
    {
        public override string TaskId => "panel-inspection-residential";
        public override string TaskName => "Residential Panel Inspection";
        public override string Description => "Identify NEC code violations in a residential electrical panel";

        [Header("Panel Reference")]
        [SerializeField] private Electrical.ElectricalPanel electricalPanel;
        [SerializeField] private Transform panelTransform;

        [Header("Violation Markers")]
        private List<ViolationMarker> violationMarkers = new List<ViolationMarker>();
        private int totalViolationCount;

        [Header("Learn Mode")]
        [SerializeField] private float guidedTourDelay = 3f;
        private int currentGuidedIndex;
        private Coroutine guidedTourCoroutine;

        [Header("Task Definition")]
        private TaskDefinition taskDefinition;

        [Header("Interaction")]
        [SerializeField] private LayerMask violationLayer;
        [SerializeField] private float selectionRayLength = 3f;
        private Camera playerCamera;
        private OVRHand[] cachedHands;
        private OVRCameraRig cachedCameraRig;

        private void Start()
        {
            playerCamera = GameManager.Instance.MainCamera;
            if (playerCamera == null)
                playerCamera = Camera.main;

            cachedHands = FindObjectsOfType<OVRHand>();
            cachedCameraRig = FindObjectOfType<OVRCameraRig>();

            LoadTaskDefinition();
        }

        private void LoadTaskDefinition()
        {
            taskDefinition = TaskManager.Instance.GetTaskDefinition(TaskId);
            if (taskDefinition == null)
            {
                Debug.LogError("[PanelInspectionTask] Failed to load task definition");
                return;
            }

            if (panelTransform == null && electricalPanel != null)
            {
                panelTransform = electricalPanel.transform;
            }

            if (panelTransform == null)
            {
                panelTransform = transform;
                Debug.LogWarning("[PanelInspectionTask] Panel transform not assigned, using self");
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            // Handle user pointing/selecting in Practice and Test modes
            if (CurrentMode == TaskMode.Practice || CurrentMode == TaskMode.Test)
            {
                HandleViolationSelection();
            }
        }

        // --- Mode Setup ---

        public override void StartLearnMode()
        {
            SetupMode(TaskMode.Learn, 0f);

            SpawnViolationMarkers();

            // Start guided tour
            guidedTourCoroutine = StartCoroutine(GuidedTourCoroutine());

            Debug.Log("[PanelInspectionTask] Learn mode started — guided tour beginning");
        }

        public override void StartPracticeMode()
        {
            SetupMode(TaskMode.Practice, 0f);

            SpawnViolationMarkers();

            // Hide all highlights — user must find them
            foreach (var marker in violationMarkers)
            {
                marker.HideHighlight();
            }

            Debug.Log("[PanelInspectionTask] Practice mode started — find the violations");
        }

        public override void StartTestMode()
        {
            float timeLimitSeconds = taskDefinition != null ? taskDefinition.timeLimit : 180f;
            SetupMode(TaskMode.Test, timeLimitSeconds);

            SpawnViolationMarkers();

            // Hide all highlights — no hints in test mode
            foreach (var marker in violationMarkers)
            {
                marker.HideHighlight();
            }

            Debug.Log($"[PanelInspectionTask] Test mode started — {timeLimitSeconds}s time limit");
        }

        // --- Violation Marker Spawning ---

        private void SpawnViolationMarkers()
        {
            // Clear existing markers
            foreach (var marker in violationMarkers)
            {
                if (marker != null)
                    Destroy(marker.gameObject);
            }
            violationMarkers.Clear();

            if (taskDefinition == null || taskDefinition.violations == null)
            {
                Debug.LogError("[PanelInspectionTask] No violations defined");
                return;
            }

            totalViolationCount = taskDefinition.violations.Length;

            foreach (var violationDef in taskDefinition.violations)
            {
                // Create marker as CHILD of panel — local position relative to panel
                GameObject markerObj = new GameObject($"Violation_{violationDef.id}");
                markerObj.transform.SetParent(panelTransform, false);
                // Use first active layer from violationLayer mask, fall back to Default
                int layerIndex = 0;
                int mask = violationLayer.value;
                if (mask != 0)
                {
                    // Find the first set bit in the layer mask
                    for (int bit = 0; bit < 32; bit++)
                    {
                        if ((mask & (1 << bit)) != 0)
                        {
                            layerIndex = bit;
                            break;
                        }
                    }
                }
                markerObj.layer = layerIndex;

                ViolationMarker marker = markerObj.AddComponent<ViolationMarker>();
                marker.Initialize(violationDef);

                violationMarkers.Add(marker);
            }

            Debug.Log($"[PanelInspectionTask] Spawned {violationMarkers.Count} violation markers");
        }

        // --- Guided Tour (Learn Mode) ---

        private IEnumerator GuidedTourCoroutine()
        {
            // Initial pause to let user orient
            yield return new WaitForSeconds(2f);

            // Show introductory message
            HintPanel hintPanel = FindObjectOfType<HintPanel>();
            if (hintPanel != null)
            {
                hintPanel.ShowHint("Welcome to Panel Inspection Training.\nI will highlight each violation and explain the NEC code.", 5f);
            }
            yield return new WaitForSeconds(5f);

            // Tour each violation
            for (int i = 0; i < violationMarkers.Count; i++)
            {
                currentGuidedIndex = i;

                // Hide previous
                if (i > 0)
                {
                    violationMarkers[i - 1].HideHighlight();
                }

                // Highlight current violation
                ViolationMarker marker = violationMarkers[i];
                marker.ShowLearnHighlight();

                // Show NEC explanation
                if (hintPanel != null)
                {
                    NECCodeEntry codeEntry = NECDatabase.GetCode(marker.NecCode);
                    string explanation = codeEntry != null
                        ? $"Violation {i + 1}/{totalViolationCount}\n\nNEC {marker.NecCode}: {codeEntry.title}\n\n{codeEntry.simplifiedExplanation}"
                        : $"Violation {i + 1}/{totalViolationCount}\n\nNEC {marker.NecCode}\n\n{marker.Description}";
                    hintPanel.ShowHint(explanation, guidedTourDelay + 2f);
                }

                AudioManager.Instance.PlayHintSound();

                yield return new WaitForSeconds(guidedTourDelay + 2f);
            }

            // Final summary
            if (hintPanel != null)
            {
                hintPanel.ShowHint($"Tour complete! {totalViolationCount} violations identified.\nSwitch to Practice mode to test yourself.", 5f);
            }

            // Show all violations at once for review
            foreach (var marker in violationMarkers)
            {
                marker.ShowLearnHighlight();
            }
        }

        // --- User Interaction ---

        private void HandleViolationSelection()
        {
            if (playerCamera == null) return;

            // Check for controller trigger or hand pinch
            bool triggerPressed = false;

            // Controller input
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                triggerPressed = true;
            }

            // Hand tracking pinch detection (simplified — full implementation in HandInteraction.cs)
            if (cachedHands == null) cachedHands = FindObjectsOfType<OVRHand>();
            foreach (var hand in cachedHands)
            {
                if (hand != null && hand.IsTracked && hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                {
                    triggerPressed = true;
                    break;
                }
            }

            if (!triggerPressed) return;

            // Raycast from controller or dominant hand
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            // Also try from right controller
            Vector3 controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            if (controllerPos != Vector3.zero)
            {
                if (cachedCameraRig == null) cachedCameraRig = FindObjectOfType<OVRCameraRig>();
                Transform trackingSpace = cachedCameraRig?.trackingSpace;
                if (trackingSpace != null)
                {
                    Vector3 worldPos = trackingSpace.TransformPoint(controllerPos);
                    Vector3 worldFwd = trackingSpace.TransformDirection(controllerRot * Vector3.forward);
                    ray = new Ray(worldPos, worldFwd);
                }
            }

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, selectionRayLength))
            {
                ViolationMarker marker = hit.collider.GetComponent<ViolationMarker>();
                if (marker == null)
                {
                    marker = hit.collider.GetComponentInParent<ViolationMarker>();
                }

                if (marker != null)
                {
                    marker.TryIdentify();
                }
                else
                {
                    // User pointed at non-violation area — register as false positive in test mode
                    if (CurrentMode == TaskMode.Test)
                    {
                        string falseId = $"false_{hit.collider.gameObject.name}_{Time.time:F0}";
                        TaskManager.Instance.IdentifyViolation(falseId);
                    }
                }
            }
        }

        // --- Hint ---

        protected override void ShowHint()
        {
            ViolationDefinition nextViolation = TaskManager.Instance.GetNextHint();
            if (nextViolation != null)
            {
                // Find corresponding marker and make it glow
                foreach (var marker in violationMarkers)
                {
                    if (marker.ViolationId == nextViolation.id)
                    {
                        marker.StartHintGlow();
                        break;
                    }
                }

                // Show hint text
                HintPanel hintPanel = FindObjectOfType<HintPanel>();
                if (hintPanel != null)
                {
                    hintPanel.ShowHint(nextViolation.hintText);
                }
            }
            else
            {
                HintPanel hintPanel = FindObjectOfType<HintPanel>();
                if (hintPanel != null)
                {
                    hintPanel.ShowHint("All violations have been identified!");
                }
            }
        }

        // --- Completion ---

        public override float GetCompletionPercentage()
        {
            if (totalViolationCount == 0) return 0f;
            return (float)TaskManager.Instance.GetIdentifiedViolationCount() / totalViolationCount * 100f;
        }

        public override TaskResult EvaluatePerformance()
        {
            // Show missed violations
            List<string> identifiedIds = TaskManager.Instance.GetIdentifiedViolationIds();
            foreach (var marker in violationMarkers)
            {
                if (!identifiedIds.Contains(marker.ViolationId))
                {
                    marker.MarkAsMissed();
                }
            }

            // Calculate score
            TaskResult result = ScoreManager.Instance.CalculatePanelInspectionScore(
                violationsFound: TaskManager.Instance.GetIdentifiedViolationCount(),
                totalViolations: totalViolationCount,
                falsePositives: TaskManager.Instance.GetFalsePositiveCount(),
                identifiedViolationIds: identifiedIds,
                allViolations: taskDefinition != null
                    ? new List<ViolationDefinition>(taskDefinition.violations)
                    : new List<ViolationDefinition>(),
                timeUsed: elapsedTime,
                timeLimit: timeLimit,
                mode: CurrentMode
            );

            if (result.passed)
            {
                AudioManager.Instance.PlayTaskComplete();
            }
            else
            {
                AudioManager.Instance.PlayTaskFail();
            }

            isActive = false;
            return result;
        }

        protected override void OnTimeExpired()
        {
            base.OnTimeExpired();
            TaskManager.Instance.ForceFinishTask();
        }

        public override void ResetTask()
        {
            base.ResetTask();

            if (guidedTourCoroutine != null)
            {
                StopCoroutine(guidedTourCoroutine);
                guidedTourCoroutine = null;
            }

            foreach (var marker in violationMarkers)
            {
                if (marker != null)
                    Destroy(marker.gameObject);
            }
            violationMarkers.Clear();
        }
    }
}
