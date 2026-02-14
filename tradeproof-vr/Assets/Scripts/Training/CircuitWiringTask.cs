using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Data;
using TradeProof.Interaction;
using TradeProof.Electrical;
using TradeProof.UI;

namespace TradeProof.Training
{
    /// <summary>
    /// Circuit Wiring training task. User wires a 20A circuit from panel to outlet.
    /// All connection points are child transforms of their parent components.
    /// Wire endpoints use SnapPoint with 0.02m snap radius.
    /// </summary>
    public class CircuitWiringTask : TrainingTask
    {
        public override string TaskId => "circuit-wiring-20a";
        public override string TaskName => "20A Circuit Wiring";
        public override string Description => "Wire a 20-amp circuit from the electrical panel to an outlet box";

        [Header("Workbench Components")]
        [SerializeField] private Transform workbenchTransform;
        [SerializeField] private ElectricalPanel electricalPanel;
        [SerializeField] private Outlet outletBox;

        [Header("Wire Spools")]
        [SerializeField] private Transform wireSpoolArea;
        [SerializeField] private WireSegment wire14AWG;
        [SerializeField] private WireSegment wire12AWG;
        [SerializeField] private WireSegment wire10AWG;

        [Header("Active Wires")]
        private WireSegment activeHotWire;
        private WireSegment activeNeutralWire;
        private WireSegment activeGroundWire;

        [Header("Tools")]
        [SerializeField] private GrabInteractable wireStrippers;
        [SerializeField] private GrabInteractable screwdriver;

        [Header("Snap Points — Panel Side")]
        [SerializeField] private SnapPoint breakerHotTerminal;
        [SerializeField] private SnapPoint neutralBarTerminal;
        [SerializeField] private SnapPoint groundBarTerminal;

        [Header("Snap Points — Outlet Side")]
        [SerializeField] private SnapPoint outletHotTerminal;
        [SerializeField] private SnapPoint outletNeutralTerminal;
        [SerializeField] private SnapPoint outletGroundTerminal;

        [Header("Task State")]
        private TaskDefinition taskDefinition;
        private int selectedWireGauge;
        private bool wireGaugeSelected;
        private bool wiresStripped;
        private bool breakerInserted;
        private bool circuitLabeled;
        private List<string> completedStepIds = new List<string>();

        [Header("Learn Mode")]
        [SerializeField] private float demoStepDelay = 4f;
        private Coroutine demoCoroutine;

        [Header("Feedback")]
        [SerializeField] private FloatingLabel stepInstructionLabel;
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
                Debug.LogError("[CircuitWiringTask] Failed to load task definition");
            }
        }

        private void SetupWorkbench()
        {
            if (workbenchTransform == null)
            {
                workbenchTransform = transform;
            }

            // Set up wire spools at the workbench
            SetupWireSpools();

            // Set up tools
            SetupTools();

            // Set up snap points
            SetupSnapPoints();
        }

        private void SetupWireSpools()
        {
            if (wireSpoolArea == null)
            {
                GameObject spoolArea = new GameObject("WireSpoolArea");
                spoolArea.transform.SetParent(workbenchTransform, false);
                spoolArea.transform.localPosition = new Vector3(-0.3f, 0.8f, 0f);
                wireSpoolArea = spoolArea.transform;
            }

            // Create wire spool objects if not assigned
            if (wire14AWG == null)
            {
                wire14AWG = CreateWireSpool(14, WireSegment.WireColor.Black, new Vector3(-0.15f, 0f, 0f));
            }
            if (wire12AWG == null)
            {
                wire12AWG = CreateWireSpool(12, WireSegment.WireColor.Black, new Vector3(0f, 0f, 0f));
            }
            if (wire10AWG == null)
            {
                wire10AWG = CreateWireSpool(10, WireSegment.WireColor.Black, new Vector3(0.15f, 0f, 0f));
            }

            // Add labels to wire spools
            AddWireSpoolLabel(wire14AWG, "14 AWG\n15A Max");
            AddWireSpoolLabel(wire12AWG, "12 AWG\n20A Max");
            AddWireSpoolLabel(wire10AWG, "10 AWG\n30A Max");
        }

        private WireSegment CreateWireSpool(int gauge, WireSegment.WireColor color, Vector3 localPos)
        {
            GameObject wireObj = new GameObject($"Wire_{gauge}AWG");
            wireObj.transform.SetParent(wireSpoolArea, false);
            wireObj.transform.localPosition = localPos;

            WireSegment wire = wireObj.AddComponent<WireSegment>();
            // Note: gauge and color are set via serialized fields in the editor
            // In code, we initialize them through reflection or initialization method
            return wire;
        }

        private void AddWireSpoolLabel(WireSegment wire, string text)
        {
            if (wire == null) return;

            GameObject labelObj = new GameObject("SpoolLabel");
            labelObj.transform.SetParent(wire.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);

            FloatingLabel label = labelObj.AddComponent<FloatingLabel>();
            label.SetText(text);
        }

        private void SetupTools()
        {
            if (wireStrippers == null)
            {
                GameObject strippersObj = new GameObject("WireStrippers");
                strippersObj.transform.SetParent(workbenchTransform, false);
                strippersObj.transform.localPosition = new Vector3(0.2f, 0.85f, 0.1f);
                wireStrippers = strippersObj.AddComponent<GrabInteractable>();
                wireStrippers.SetGripOffset(new Vector3(0f, 0f, 0.06f)); // Held in fist
            }

            if (screwdriver == null)
            {
                GameObject screwdriverObj = new GameObject("Screwdriver");
                screwdriverObj.transform.SetParent(workbenchTransform, false);
                screwdriverObj.transform.localPosition = new Vector3(0.3f, 0.85f, 0.1f);
                screwdriver = screwdriverObj.AddComponent<GrabInteractable>();
                screwdriver.SetGripOffset(new Vector3(0f, 0f, 0.08f)); // Held in fist
            }
        }

        private void SetupSnapPoints()
        {
            // Panel side snap points — children of the panel
            if (electricalPanel != null)
            {
                Transform panelXform = electricalPanel.transform;

                if (breakerHotTerminal == null)
                {
                    breakerHotTerminal = CreateSnapPoint("BreakerHotTerminal", panelXform,
                        new Vector3(-0.05f, 0.1f, 0.02f), "hot", 20);
                }
                if (neutralBarTerminal == null)
                {
                    neutralBarTerminal = CreateSnapPoint("NeutralBarTerminal", panelXform,
                        new Vector3(0.1f, 0.0f, 0.02f), "neutral", 0);
                }
                if (groundBarTerminal == null)
                {
                    groundBarTerminal = CreateSnapPoint("GroundBarTerminal", panelXform,
                        new Vector3(0.1f, -0.1f, 0.02f), "ground", 0);
                }
            }

            // Outlet side snap points — children of the outlet box
            if (outletBox != null)
            {
                Transform outletXform = outletBox.transform;

                if (outletHotTerminal == null)
                {
                    outletHotTerminal = CreateSnapPoint("OutletHotTerminal", outletXform,
                        new Vector3(-0.02f, 0f, 0.01f), "hot", 20);
                }
                if (outletNeutralTerminal == null)
                {
                    outletNeutralTerminal = CreateSnapPoint("OutletNeutralTerminal", outletXform,
                        new Vector3(0.02f, 0f, 0.01f), "neutral", 0);
                }
                if (outletGroundTerminal == null)
                {
                    outletGroundTerminal = CreateSnapPoint("OutletGroundTerminal", outletXform,
                        new Vector3(0f, -0.02f, 0.01f), "ground", 0);
                }
            }
        }

        private SnapPoint CreateSnapPoint(string name, Transform parent, Vector3 localPos, string wireType, int ampRating)
        {
            GameObject spObj = new GameObject(name);
            spObj.transform.SetParent(parent, false);
            spObj.transform.localPosition = localPos;

            SnapPoint sp = spObj.AddComponent<SnapPoint>();
            sp.SetAcceptedWireType(wireType);
            sp.SetAmpRating(ampRating);
            return sp;
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            // Update step instruction label
            UpdateStepInstruction();
        }

        // --- Mode Setup ---

        public override void StartLearnMode()
        {
            SetupMode(TaskMode.Learn, 0f);
            ResetWiringState();

            // Start demonstration
            demoCoroutine = StartCoroutine(DemonstrationCoroutine());

            Debug.Log("[CircuitWiringTask] Learn mode started — demonstration beginning");
        }

        public override void StartPracticeMode()
        {
            SetupMode(TaskMode.Practice, 0f);
            ResetWiringState();

            // Show first step instruction
            ShowCurrentStepInstruction();

            Debug.Log("[CircuitWiringTask] Practice mode started");
        }

        public override void StartTestMode()
        {
            float timeLimitSeconds = taskDefinition != null ? taskDefinition.timeLimit : 300f;
            SetupMode(TaskMode.Test, timeLimitSeconds);
            ResetWiringState();

            Debug.Log($"[CircuitWiringTask] Test mode started — {timeLimitSeconds}s time limit");
        }

        private void ResetWiringState()
        {
            wireGaugeSelected = false;
            selectedWireGauge = 0;
            wiresStripped = false;
            breakerInserted = false;
            circuitLabeled = false;
            completedStepIds.Clear();

            // Detach any connected wires
            if (activeHotWire != null) activeHotWire.DetachAll();
            if (activeNeutralWire != null) activeNeutralWire.DetachAll();
            if (activeGroundWire != null) activeGroundWire.DetachAll();
        }

        // --- Demonstration (Learn Mode) ---

        private IEnumerator DemonstrationCoroutine()
        {
            if (taskDefinition == null || taskDefinition.steps == null)
            {
                Debug.LogError("[CircuitWiringTask] No steps defined for demonstration");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Watch the demonstration of wiring a 20A circuit.\nEach step will be explained with the NEC code reference.", 5f);
            }
            yield return new WaitForSeconds(5f);

            for (int i = 0; i < taskDefinition.steps.Length; i++)
            {
                StepDefinition step = taskDefinition.steps[i];

                // Show step explanation
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

                if (hintPanel != null)
                {
                    hintPanel.ShowHint(explanation, demoStepDelay + 2f);
                }

                // Highlight relevant components
                HighlightStepComponents(step);

                AudioManager.Instance.PlayHintSound();

                yield return new WaitForSeconds(demoStepDelay + 2f);
            }

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Demonstration complete!\nSwitch to Practice mode to try it yourself.", 5f);
            }
        }

        private void HighlightStepComponents(StepDefinition step)
        {
            HighlightController highlighter = FindObjectOfType<HighlightController>();
            if (highlighter == null) return;

            switch (step.action)
            {
                case "select-wire":
                    if (wire12AWG != null)
                        highlighter.HighlightObject(wire12AWG.gameObject);
                    break;
                case "connect-hot-to-breaker":
                    if (breakerHotTerminal != null)
                        highlighter.HighlightObject(breakerHotTerminal.gameObject);
                    break;
                case "connect-neutral-to-bar":
                    if (neutralBarTerminal != null)
                        highlighter.HighlightObject(neutralBarTerminal.gameObject);
                    break;
                case "connect-ground-to-bar":
                    if (groundBarTerminal != null)
                        highlighter.HighlightObject(groundBarTerminal.gameObject);
                    break;
                case "connect-hot-to-outlet":
                    if (outletHotTerminal != null)
                        highlighter.HighlightObject(outletHotTerminal.gameObject);
                    break;
                case "connect-neutral-to-outlet":
                    if (outletNeutralTerminal != null)
                        highlighter.HighlightObject(outletNeutralTerminal.gameObject);
                    break;
                case "connect-ground-to-outlet":
                    if (outletGroundTerminal != null)
                        highlighter.HighlightObject(outletGroundTerminal.gameObject);
                    break;
                case "connect-outlet-wires":
                    // Highlight all three outlet terminals
                    if (outletHotTerminal != null)
                        highlighter.HighlightObject(outletHotTerminal.gameObject);
                    if (outletNeutralTerminal != null)
                        highlighter.HighlightObject(outletNeutralTerminal.gameObject);
                    if (outletGroundTerminal != null)
                        highlighter.HighlightObject(outletGroundTerminal.gameObject);
                    break;
                case "insert-breaker":
                    if (electricalPanel != null)
                        highlighter.HighlightObject(electricalPanel.gameObject);
                    break;
                case "label-directory":
                    if (electricalPanel != null)
                        highlighter.HighlightObject(electricalPanel.gameObject);
                    break;
            }
        }

        // --- Step Completion ---

        public void OnWireGaugeSelected(int gauge)
        {
            selectedWireGauge = gauge;
            wireGaugeSelected = true;

            bool correct = (gauge == 12); // 12 AWG for 20A circuit per NEC 310.16
            TaskManager.Instance.SetWireGaugeCorrect(correct);

            if (CurrentMode == TaskMode.Practice)
            {
                if (correct)
                {
                    if (hintPanel != null)
                        hintPanel.ShowHint("Correct! 12 AWG is the right gauge for a 20A circuit (NEC 310.16).", 3f);
                    AudioManager.Instance.PlayCorrectSound();
                }
                else
                {
                    if (hintPanel != null)
                        hintPanel.ShowHint($"Incorrect gauge! {gauge} AWG is not rated for 20A.\n12 AWG is required per NEC 310.16.", 4f);
                    AudioManager.Instance.PlayIncorrectSound();
                }
            }

            TryCompleteStep("select-wire-gauge");
        }

        public void OnWireRouted()
        {
            TryCompleteStep("route-wire");
        }

        public void OnWiresStripped()
        {
            wiresStripped = true;
            TryCompleteStep("strip-wire-ends");
        }

        public void OnHotWireConnectedToBreaker()
        {
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-hot-to-breaker");
        }

        public void OnNeutralWireConnectedToBar()
        {
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-neutral-to-bar");
        }

        public void OnGroundWireConnectedToBar()
        {
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-ground-to-bar");
        }

        public void OnOutletWiresConnected()
        {
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-wires-at-outlet");
        }

        public void OnBreakerInserted()
        {
            breakerInserted = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("secure-breaker");
        }

        public void OnCircuitLabeled()
        {
            circuitLabeled = true;
            TryCompleteStep("label-circuit");
        }

        private void TryCompleteStep(string stepId)
        {
            if (completedStepIds.Contains(stepId)) return;

            bool success = TaskManager.Instance.CompleteStep(stepId);
            if (success)
            {
                completedStepIds.Add(stepId);
                ShowCurrentStepInstruction();

                // Check if all steps completed
                if (taskDefinition != null && taskDefinition.steps != null &&
                    completedStepIds.Count >= taskDefinition.steps.Length)
                {
                    Debug.Log("[CircuitWiringTask] All steps completed!");
                }
            }
        }

        // --- Step Instructions ---

        private void ShowCurrentStepInstruction()
        {
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

        private void UpdateStepInstruction()
        {
            if (CurrentMode == TaskMode.Test) return; // No instruction in test mode

            if (CurrentMode == TaskMode.Practice)
            {
                ShowCurrentStepInstruction();
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

                if (hintPanel != null)
                {
                    hintPanel.ShowHint(hint);
                }

                // Highlight the relevant component
                HighlightStepComponents(currentStep);
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
            TaskResult result = ScoreManager.Instance.CalculateCircuitWiringScore(
                completedStepIds: TaskManager.Instance.GetCompletedStepIds(),
                allSteps: taskDefinition != null ? taskDefinition.steps : null,
                wireGaugeCorrect: selectedWireGauge == 12,
                connectionQualityScore: TaskManager.Instance.ConnectionQualityScore,
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

            if (demoCoroutine != null)
            {
                StopCoroutine(demoCoroutine);
                demoCoroutine = null;
            }

            ResetWiringState();
        }
    }
}
