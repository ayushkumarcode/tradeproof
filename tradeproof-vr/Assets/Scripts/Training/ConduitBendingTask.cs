using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Data;
using TradeProof.Interaction;
using TradeProof.UI;

namespace TradeProof.Training
{
    /// <summary>
    /// EMT Conduit Bending training task. Player measures, marks, and bends EMT conduit
    /// to make various bend types (90-degree, offset, saddle). Scored on measurement accuracy.
    /// Difficulty: Intermediate. 8 steps. Time limit (test): 360s.
    /// Angle tolerance: +/-5 degrees.
    /// </summary>
    public class ConduitBendingTask : TrainingTask
    {
        public override string TaskId => "conduit-bending-emt";
        public override string TaskName => "EMT Conduit Bending";
        public override string Description =>
            "Measure, mark, and bend EMT conduit to create 90-degree bends, offsets, and saddle bends.";

        [Header("Workbench Components")]
        [SerializeField] private Transform workbenchTransform;
        [SerializeField] private Conduit conduit;
        [SerializeField] private Transform conduitBender;
        [SerializeField] private MeasureTool measureTool;
        [SerializeField] private ReamingTool reamingTool;

        [Header("Conduit Bender Visual")]
        [SerializeField] private Transform benderHandle;
        [SerializeField] private Transform benderShoe;
        [SerializeField] private FloatingLabel benderAngleLabel;

        [Header("Measurement Targets")]
        [SerializeField] private float stubUpTarget = 0.254f;       // 10 inches in meters
        [SerializeField] private float ninetyDegreeTarget = 90f;    // degrees
        [SerializeField] private float offsetAngleTarget = 30f;     // degrees
        [SerializeField] private float saddleBendTarget = 45f;      // degrees (center bend)
        [SerializeField] private float angleTolerance = 5f;         // +/- degrees

        [Header("Task State")]
        private TaskDefinition taskDefinition;
        private List<string> completedStepIds = new List<string>();
        private List<MeasurementResult> measurementResults = new List<MeasurementResult>();
        private bool stubUpMeasured;
        private bool conduitMarked;
        private bool ninetyBendComplete;
        private bool ninetyAngleChecked;
        private bool offsetBendComplete;
        private bool offsetAngleChecked;
        private bool saddleBendComplete;
        private bool cutEndReamed;
        private float currentBendAngle;

        [Header("Learn Mode")]
        [SerializeField] private float demoStepDelay = 5f;
        private Coroutine demoCoroutine;

        [Header("Feedback")]
        [SerializeField] private FloatingLabel stepInstructionLabel;
        [SerializeField] private FloatingLabel measurementDisplay;
        private HintPanel hintPanel;

        private void Start()
        {
            LoadTaskDefinition();
            hintPanel = FindObjectOfType<HintPanel>();
            SetupWorkbench();
        }

        private void LoadTaskDefinition()
        {
            taskDefinition = TaskManager.Instance.GetTaskDefinition(TaskId);
            if (taskDefinition == null)
            {
                Debug.LogError("[ConduitBendingTask] Failed to load task definition");
            }
        }

        private void SetupWorkbench()
        {
            if (workbenchTransform == null)
            {
                workbenchTransform = transform;
            }

            SetupConduit();
            SetupBender();
            SetupTools();
            SetupLabels();
        }

        private void SetupConduit()
        {
            if (conduit == null)
            {
                GameObject conduitObj = new GameObject("EMT_Conduit");
                conduitObj.transform.SetParent(workbenchTransform, false);
                conduitObj.transform.localPosition = new Vector3(0f, 0.85f, 0.2f);

                conduit = conduitObj.AddComponent<Conduit>();

                GrabInteractable conduitGrab = conduitObj.AddComponent<GrabInteractable>();
                conduitGrab.SetToolType("conduit");
                conduitGrab.SetGripOffset(new Vector3(0f, 0f, 0f));
            }
        }

        private void SetupBender()
        {
            if (conduitBender == null)
            {
                GameObject benderObj = new GameObject("ConduitBender");
                benderObj.transform.SetParent(workbenchTransform, false);
                benderObj.transform.localPosition = new Vector3(0.5f, 0f, 0.3f);

                // Bender shoe (the curved part)
                GameObject shoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shoe.name = "BenderShoe";
                shoe.transform.SetParent(benderObj.transform, false);
                shoe.transform.localPosition = Vector3.zero;
                shoe.transform.localScale = new Vector3(0.08f, 0.05f, 0.25f);

                Material shoeMat = new Material(Shader.Find("Standard"));
                shoeMat.color = new Color(0.3f, 0.3f, 0.35f, 1f);
                shoeMat.SetFloat("_Metallic", 0.7f);
                shoe.GetComponent<MeshRenderer>().material = shoeMat;

                benderShoe = shoe.transform;

                // Handle
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "BenderHandle";
                handle.transform.SetParent(benderObj.transform, false);
                handle.transform.localPosition = new Vector3(0f, 0.5f, -0.1f);
                handle.transform.localScale = new Vector3(0.025f, 0.5f, 0.025f);

                Material handleMat = new Material(Shader.Find("Standard"));
                handleMat.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                handle.GetComponent<MeshRenderer>().material = handleMat;

                benderHandle = handle.transform;

                GrabInteractable benderGrab = benderObj.AddComponent<GrabInteractable>();
                benderGrab.SetToolType("conduit-bender");
                benderGrab.SetGripOffset(new Vector3(0f, 0.3f, 0f));

                conduitBender = benderObj.transform;

                // Angle label
                GameObject angleLabelObj = new GameObject("BenderAngleLabel");
                angleLabelObj.transform.SetParent(benderObj.transform, false);
                angleLabelObj.transform.localPosition = new Vector3(0.06f, 0f, 0f);
                benderAngleLabel = angleLabelObj.AddComponent<FloatingLabel>();
                benderAngleLabel.SetText("0°");
                benderAngleLabel.SetFontSize(2.0f);
                benderAngleLabel.SetColor(Color.cyan);
            }
        }

        private void SetupTools()
        {
            if (measureTool == null)
            {
                GameObject measureObj = new GameObject("TapeMeasure");
                measureObj.transform.SetParent(workbenchTransform, false);
                measureObj.transform.localPosition = new Vector3(-0.3f, 0.85f, 0.1f);
                measureTool = measureObj.AddComponent<MeasureTool>();
            }

            if (reamingTool == null)
            {
                GameObject reamObj = new GameObject("ReamingTool");
                reamObj.transform.SetParent(workbenchTransform, false);
                reamObj.transform.localPosition = new Vector3(-0.2f, 0.85f, 0.1f);
                reamingTool = reamObj.AddComponent<ReamingTool>();
            }
        }

        private void SetupLabels()
        {
            if (stepInstructionLabel == null)
            {
                GameObject labelObj = new GameObject("StepInstructionLabel");
                labelObj.transform.SetParent(workbenchTransform, false);
                labelObj.transform.localPosition = new Vector3(0f, 1.4f, 0.3f);
                stepInstructionLabel = labelObj.AddComponent<FloatingLabel>();
                stepInstructionLabel.SetFontSize(2.0f);
                stepInstructionLabel.SetText("");
            }

            if (measurementDisplay == null)
            {
                GameObject displayObj = new GameObject("MeasurementDisplay");
                displayObj.transform.SetParent(workbenchTransform, false);
                displayObj.transform.localPosition = new Vector3(0.4f, 1.2f, 0.3f);
                measurementDisplay = displayObj.AddComponent<FloatingLabel>();
                measurementDisplay.SetFontSize(1.8f);
                measurementDisplay.SetColor(Color.green);
                measurementDisplay.SetText("");
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            if (CurrentMode == TaskMode.Practice)
            {
                ShowCurrentStepInstruction();
            }

            // Update bend angle display
            if (benderAngleLabel != null)
            {
                benderAngleLabel.SetText($"{currentBendAngle:F1}°");
            }
        }

        // --- Mode Setup ---

        public override void StartLearnMode()
        {
            SetupMode(TaskMode.Learn, 0f);
            ResetTaskState();
            demoCoroutine = StartCoroutine(DemonstrationCoroutine());
            Debug.Log("[ConduitBendingTask] Learn mode started - demonstration beginning");
        }

        public override void StartPracticeMode()
        {
            SetupMode(TaskMode.Practice, 0f);
            ResetTaskState();
            ShowCurrentStepInstruction();
            Debug.Log("[ConduitBendingTask] Practice mode started");
        }

        public override void StartTestMode()
        {
            float timeLimitSeconds = taskDefinition != null ? taskDefinition.timeLimit : 360f;
            SetupMode(TaskMode.Test, timeLimitSeconds);
            ResetTaskState();
            Debug.Log($"[ConduitBendingTask] Test mode started - {timeLimitSeconds}s time limit");
        }

        private void ResetTaskState()
        {
            completedStepIds.Clear();
            measurementResults.Clear();
            stubUpMeasured = false;
            conduitMarked = false;
            ninetyBendComplete = false;
            ninetyAngleChecked = false;
            offsetBendComplete = false;
            offsetAngleChecked = false;
            saddleBendComplete = false;
            cutEndReamed = false;
            currentBendAngle = 0f;

            if (conduit != null) conduit.ResetConduit();
        }

        // --- Demonstration (Learn Mode) ---

        private IEnumerator DemonstrationCoroutine()
        {
            if (taskDefinition == null || taskDefinition.steps == null)
            {
                Debug.LogError("[ConduitBendingTask] No steps defined for demonstration");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Watch the demonstration of EMT conduit bending.\n" +
                    "Accurate measurement is critical - angles must be within +/-5 degrees.", 5f);
            }
            yield return new WaitForSeconds(5f);

            for (int i = 0; i < taskDefinition.steps.Length; i++)
            {
                StepDefinition step = taskDefinition.steps[i];

                NECCodeEntry codeEntry = null;
                if (!string.IsNullOrEmpty(step.necCode))
                {
                    codeEntry = NECDatabase.GetCode(step.necCode);
                }

                string explanation = $"Step {i + 1}/{taskDefinition.steps.Length}: {step.description}";
                if (codeEntry != null)
                {
                    explanation += $"\n\nNEC {step.necCode}: {codeEntry.title}\n{codeEntry.simplifiedExplanation}";
                }

                // Show measurement target info if applicable
                if (taskDefinition.measurements != null)
                {
                    foreach (var m in taskDefinition.measurements)
                    {
                        if (m.id == step.id)
                        {
                            explanation += $"\n\nTarget: {m.targetValue} {m.unit} (tolerance: +/-{m.tolerance} {m.unit})";
                        }
                    }
                }

                if (hintPanel != null)
                {
                    hintPanel.ShowHint(explanation, demoStepDelay + 2f);
                }

                AudioManager.Instance.PlayHintSound();
                yield return new WaitForSeconds(demoStepDelay + 2f);
            }

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Demonstration complete!\nSwitch to Practice mode to try it yourself.", 5f);
            }
        }

        // --- Measurement and Bending ---

        /// <summary>
        /// Measure the stub-up distance. Player uses the tape measure.
        /// </summary>
        public MeasurementResult MeasureStubUp(float measuredValue)
        {
            float targetInches = stubUpTarget * 39.3701f; // Convert to inches
            float accuracy = CalculateAccuracy(measuredValue, targetInches, 0.5f); // 0.5 inch tolerance

            MeasurementResult result = new MeasurementResult
            {
                measurementId = "measure-stub-up",
                actualValue = measuredValue,
                targetValue = targetInches,
                accuracy = accuracy
            };
            measurementResults.Add(result);

            stubUpMeasured = true;

            if (measurementDisplay != null)
            {
                measurementDisplay.SetText($"Stub-up: {measuredValue:F1}\" (target: {targetInches:F1}\")\n" +
                    $"Accuracy: {accuracy * 100f:F0}%");
            }

            Debug.Log($"[ConduitBendingTask] Stub-up measured: {measuredValue:F1}\" (target: {targetInches:F1}\", accuracy: {accuracy:F2})");
            return result;
        }

        /// <summary>
        /// Apply a bend to the conduit at the current bender angle.
        /// </summary>
        public MeasurementResult ApplyBend(float angle, float distanceFromEnd, string bendType)
        {
            if (conduit == null) return null;

            conduit.ApplyBend(angle, distanceFromEnd, bendType);
            currentBendAngle = angle;

            float targetAngle;
            float tolerance = angleTolerance;

            switch (bendType)
            {
                case "90-degree":
                    targetAngle = ninetyDegreeTarget;
                    break;
                case "offset":
                    targetAngle = offsetAngleTarget;
                    break;
                case "saddle":
                    targetAngle = saddleBendTarget;
                    break;
                default:
                    targetAngle = angle;
                    break;
            }

            float accuracy = CalculateAccuracy(angle, targetAngle, tolerance);

            MeasurementResult result = new MeasurementResult
            {
                measurementId = $"bend-{bendType}",
                actualValue = angle,
                targetValue = targetAngle,
                accuracy = accuracy
            };
            measurementResults.Add(result);

            bool withinTolerance = Mathf.Abs(angle - targetAngle) <= tolerance;

            if (measurementDisplay != null)
            {
                string status = withinTolerance ? "PASS" : "FAIL";
                Color statusColor = withinTolerance ? Color.green : Color.red;
                measurementDisplay.SetColor(statusColor);
                measurementDisplay.SetText($"Bend: {angle:F1}° (target: {targetAngle:F1}°)\n" +
                    $"Tolerance: +/-{tolerance}° [{status}]");
            }

            if (withinTolerance)
            {
                AudioManager.Instance.PlayCorrectSound();
            }
            else
            {
                AudioManager.Instance.PlayIncorrectSound();
            }

            Debug.Log($"[ConduitBendingTask] Bend applied: {angle:F1}° ({bendType}), " +
                      $"target: {targetAngle:F1}°, accuracy: {accuracy:F2}, pass: {withinTolerance}");

            return result;
        }

        /// <summary>
        /// Check a bend angle with a protractor or angle finder.
        /// </summary>
        public bool CheckBendAngle(int bendIndex, float expectedAngle)
        {
            if (conduit == null) return false;

            return conduit.CheckBendAccuracy(bendIndex, expectedAngle, angleTolerance);
        }

        private float CalculateAccuracy(float actual, float target, float maxTolerance)
        {
            float difference = Mathf.Abs(actual - target);
            if (difference <= 0.01f) return 1f; // Perfect
            if (difference >= maxTolerance * 2f) return 0f; // Way off
            return Mathf.Clamp01(1f - (difference / (maxTolerance * 2f)));
        }

        // --- Step Completion ---

        public void OnStubUpMeasured()
        {
            if (stubUpMeasured)
                TryCompleteStep("measure-stub-up");
        }

        public void OnConduitMarked()
        {
            conduitMarked = true;
            TryCompleteStep("mark-conduit");
        }

        public void OnNinetyDegreeBendComplete()
        {
            ninetyBendComplete = true;
            TryCompleteStep("bend-90-degrees");
        }

        public void OnNinetyAngleChecked()
        {
            ninetyAngleChecked = true;
            TryCompleteStep("check-90-angle");
        }

        public void OnOffsetBendComplete()
        {
            offsetBendComplete = true;
            TryCompleteStep("make-offset-bend");
        }

        public void OnOffsetAngleChecked()
        {
            offsetAngleChecked = true;
            TryCompleteStep("check-offset-angle");
        }

        public void OnSaddleBendComplete()
        {
            saddleBendComplete = true;
            TryCompleteStep("make-saddle-bend");
        }

        public void OnCutEndReamed()
        {
            if (reamingTool != null && conduit != null)
            {
                reamingTool.ApplyToConduit(conduit);
            }
            cutEndReamed = true;
            TryCompleteStep("ream-cut-end");
        }

        private void TryCompleteStep(string stepId)
        {
            if (completedStepIds.Contains(stepId)) return;

            bool success = TaskManager.Instance.CompleteStep(stepId);
            if (success)
            {
                completedStepIds.Add(stepId);
                ShowCurrentStepInstruction();

                if (taskDefinition != null && taskDefinition.steps != null &&
                    completedStepIds.Count >= taskDefinition.steps.Length)
                {
                    Debug.Log("[ConduitBendingTask] All steps completed!");
                }
            }
        }

        // --- Step Instructions ---

        private void ShowCurrentStepInstruction()
        {
            if (CurrentMode == TaskMode.Test) return;

            StepDefinition currentStep = TaskManager.Instance.GetCurrentStep();
            if (currentStep == null)
            {
                if (stepInstructionLabel != null)
                    stepInstructionLabel.SetText("All steps completed!");
                return;
            }

            string instruction = $"Step {currentStep.order}: {currentStep.description}";
            if (stepInstructionLabel != null)
            {
                stepInstructionLabel.SetText(instruction);
            }
        }

        // --- Hint ---

        protected override void ShowHint()
        {
            StepDefinition currentStep = TaskManager.Instance.GetCurrentStep();
            if (currentStep != null)
            {
                string hint = currentStep.hintText;
                if (string.IsNullOrEmpty(hint))
                {
                    hint = $"Next step: {currentStep.description}";
                }

                if (!string.IsNullOrEmpty(currentStep.necCode))
                {
                    NECCodeEntry code = NECDatabase.GetCode(currentStep.necCode);
                    if (code != null)
                    {
                        hint += $"\n\nNEC {currentStep.necCode}: {code.visualHintText}";
                    }
                }

                // Add measurement info from definition
                if (taskDefinition != null && taskDefinition.measurements != null)
                {
                    foreach (var m in taskDefinition.measurements)
                    {
                        if (m.id == currentStep.id)
                        {
                            hint += $"\n\nTarget: {m.targetValue} {m.unit} (tolerance: +/-{m.tolerance} {m.unit})";
                        }
                    }
                }

                if (hintPanel != null)
                {
                    hintPanel.ShowHint(hint);
                }
            }
        }

        // --- Completion ---

        public override float GetCompletionPercentage()
        {
            if (taskDefinition == null || taskDefinition.steps == null || taskDefinition.steps.Length == 0)
                return 0f;
            return (float)completedStepIds.Count / taskDefinition.steps.Length * 100f;
        }

        public override TaskResult EvaluatePerformance()
        {
            TaskResult result = ScoreManager.Instance.CalculateMeasurementScore(
                measurements: measurementResults,
                allMeasurements: taskDefinition != null ? taskDefinition.measurements : null,
                timeUsed: elapsedTime,
                timeLimit: timeLimit,
                mode: CurrentMode
            );

            // Also factor in step completion
            int stepsCompleted = completedStepIds.Count;
            int totalSteps = taskDefinition != null && taskDefinition.steps != null ? taskDefinition.steps.Length : 0;
            float stepBonus = totalSteps > 0 ? ((float)stepsCompleted / totalSteps) * 10f : 0f;
            result.score = Mathf.Clamp(result.score + stepBonus, 0f, 100f);
            result.passed = result.score >= ScoreManager.PASSING_THRESHOLD;

            // Fill in step data for breakdown
            result.breakdown.stepsCompleted = stepsCompleted;
            result.breakdown.totalSteps = totalSteps;

            if (result.passed)
                AudioManager.Instance.PlayTaskComplete();
            else
                AudioManager.Instance.PlayTaskFail();

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

            if (demoCoroutine != null)
            {
                StopCoroutine(demoCoroutine);
                demoCoroutine = null;
            }

            ResetTaskState();
        }
    }
}
