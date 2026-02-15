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
    /// Outlet Installation training task. Player installs a duplex outlet in a junction box,
    /// connecting hot, neutral, and ground wires with proper NEC-compliant terminations.
    /// Difficulty: Beginner. 9 steps. Time limit (test): 240s.
    /// </summary>
    public class OutletInstallationTask : TrainingTask
    {
        public override string TaskId => "outlet-installation-duplex";
        public override string TaskName => "Outlet Installation";
        public override string Description =>
            "Install a duplex outlet in a junction box, connecting hot, neutral, and ground wires with proper NEC-compliant terminations.";

        [Header("Workbench Components")]
        [SerializeField] private Transform workbenchTransform;
        [SerializeField] private Transform junctionBox;
        [SerializeField] private Outlet outlet;
        [SerializeField] private Transform coverPlate;

        [Header("Wires")]
        [SerializeField] private WireSegment hotWire;
        [SerializeField] private WireSegment neutralWire;
        [SerializeField] private WireSegment groundWire;

        [Header("Tools")]
        [SerializeField] private GrabInteractable wireStrippers;

        [Header("Snap Points")]
        [SerializeField] private SnapPoint outletHotTerminal;
        [SerializeField] private SnapPoint outletNeutralTerminal;
        [SerializeField] private SnapPoint outletGroundTerminal;
        [SerializeField] private SnapPoint coverplateSnap;

        [Header("Task State")]
        private TaskDefinition taskDefinition;
        private List<string> completedStepIds = new List<string>();
        private bool romexStripped;
        private bool wiresIdentified;
        private bool wireLengthVerified;
        private bool hotConnected;
        private bool neutralConnected;
        private bool groundConnected;
        private bool terminationsTightened;
        private bool outletMounted;
        private bool coverplateInstalled;

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
                Debug.LogError("[OutletInstallationTask] Failed to load task definition");
            }
        }

        private void SetupWorkbench()
        {
            if (workbenchTransform == null)
            {
                workbenchTransform = transform;
            }

            SetupJunctionBox();
            SetupOutlet();
            SetupCoverPlate();
            SetupWires();
            SetupTools();
            SetupSnapPoints();
            SetupInstructionLabel();
        }

        private void SetupJunctionBox()
        {
            if (junctionBox == null)
            {
                GameObject boxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxObj.name = "JunctionBox";
                boxObj.transform.SetParent(workbenchTransform, false);
                boxObj.transform.localPosition = new Vector3(0f, 1.0f, 0.3f);
                boxObj.transform.localScale = new Vector3(0.051f, 0.076f, 0.070f);

                Material boxMat = new Material(Shader.Find("Standard"));
                boxMat.color = new Color(0.2f, 0.4f, 0.6f, 1f);
                boxObj.GetComponent<MeshRenderer>().material = boxMat;

                junctionBox = boxObj.transform;
            }
        }

        private void SetupOutlet()
        {
            if (outlet == null)
            {
                GameObject outletObj = new GameObject("Outlet_Duplex");
                outletObj.transform.SetParent(workbenchTransform, false);
                outletObj.transform.localPosition = new Vector3(0.2f, 0.85f, 0.1f);

                outlet = outletObj.AddComponent<Outlet>();

                // Add a GrabInteractable so the player can pick it up
                GrabInteractable outletGrab = outletObj.AddComponent<GrabInteractable>();
                outletGrab.SetGripOffset(new Vector3(0f, 0f, 0.02f));
            }
        }

        private void SetupCoverPlate()
        {
            if (coverPlate == null)
            {
                GameObject cpObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cpObj.name = "CoverPlate";
                cpObj.transform.SetParent(workbenchTransform, false);
                cpObj.transform.localPosition = new Vector3(0.3f, 0.85f, 0.1f);
                cpObj.transform.localScale = new Vector3(0.072f, 0.115f, 0.003f);

                Material cpMat = new Material(Shader.Find("Standard"));
                cpMat.color = new Color(0.92f, 0.90f, 0.85f, 1f); // Ivory
                cpObj.GetComponent<MeshRenderer>().material = cpMat;

                GrabInteractable cpGrab = cpObj.AddComponent<GrabInteractable>();
                cpGrab.SetGripOffset(new Vector3(0f, 0f, 0.01f));

                coverPlate = cpObj.transform;
            }
        }

        private void SetupWires()
        {
            // Hot wire (black)
            if (hotWire == null)
            {
                GameObject hotObj = new GameObject("HotWire_Black");
                hotObj.transform.SetParent(workbenchTransform, false);
                hotObj.transform.localPosition = new Vector3(-0.15f, 0.9f, 0.3f);
                hotWire = hotObj.AddComponent<WireSegment>();
            }

            // Neutral wire (white)
            if (neutralWire == null)
            {
                GameObject neutralObj = new GameObject("NeutralWire_White");
                neutralObj.transform.SetParent(workbenchTransform, false);
                neutralObj.transform.localPosition = new Vector3(-0.1f, 0.9f, 0.3f);
                neutralWire = neutralObj.AddComponent<WireSegment>();
            }

            // Ground wire (bare copper)
            if (groundWire == null)
            {
                GameObject groundObj = new GameObject("GroundWire_Bare");
                groundObj.transform.SetParent(workbenchTransform, false);
                groundObj.transform.localPosition = new Vector3(-0.05f, 0.9f, 0.3f);
                groundWire = groundObj.AddComponent<WireSegment>();
            }
        }

        private void SetupTools()
        {
            if (wireStrippers == null)
            {
                GameObject strippersObj = new GameObject("WireStrippers");
                strippersObj.transform.SetParent(workbenchTransform, false);
                strippersObj.transform.localPosition = new Vector3(-0.25f, 0.85f, 0.1f);
                wireStrippers = strippersObj.AddComponent<GrabInteractable>();
                wireStrippers.SetGripOffset(new Vector3(0f, 0f, 0.06f));
                wireStrippers.SetToolType("wire-strippers");
            }
        }

        private void SetupSnapPoints()
        {
            // Outlet terminal snap points
            if (outlet != null)
            {
                outletHotTerminal = outlet.HotTerminal;
                outletNeutralTerminal = outlet.NeutralTerminal;
                outletGroundTerminal = outlet.GroundTerminal;
            }

            // Cover plate snap point on the junction box
            if (coverplateSnap == null && junctionBox != null)
            {
                GameObject cpSnapObj = new GameObject("CoverplateSnap");
                cpSnapObj.transform.SetParent(junctionBox, false);
                cpSnapObj.transform.localPosition = new Vector3(0f, 0f, -0.04f);

                coverplateSnap = cpSnapObj.AddComponent<SnapPoint>();
                coverplateSnap.SetAcceptedWireType("any");
                coverplateSnap.SetSnapPointId("coverplate-snap");
            }
        }

        private void SetupInstructionLabel()
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
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            if (CurrentMode == TaskMode.Practice)
            {
                ShowCurrentStepInstruction();
            }
        }

        // --- Mode Setup ---

        public override void StartLearnMode()
        {
            SetupMode(TaskMode.Learn, 0f);
            ResetTaskState();

            demoCoroutine = StartCoroutine(DemonstrationCoroutine());

            Debug.Log("[OutletInstallationTask] Learn mode started - demonstration beginning");
        }

        public override void StartPracticeMode()
        {
            SetupMode(TaskMode.Practice, 0f);
            ResetTaskState();

            ShowCurrentStepInstruction();

            Debug.Log("[OutletInstallationTask] Practice mode started");
        }

        public override void StartTestMode()
        {
            float timeLimitSeconds = taskDefinition != null ? taskDefinition.timeLimit : 240f;
            SetupMode(TaskMode.Test, timeLimitSeconds);
            ResetTaskState();

            Debug.Log($"[OutletInstallationTask] Test mode started - {timeLimitSeconds}s time limit");
        }

        private void ResetTaskState()
        {
            completedStepIds.Clear();
            romexStripped = false;
            wiresIdentified = false;
            wireLengthVerified = false;
            hotConnected = false;
            neutralConnected = false;
            groundConnected = false;
            terminationsTightened = false;
            outletMounted = false;
            coverplateInstalled = false;

            // Detach wires
            if (hotWire != null) hotWire.DetachAll();
            if (neutralWire != null) neutralWire.DetachAll();
            if (groundWire != null) groundWire.DetachAll();
        }

        // --- Demonstration (Learn Mode) ---

        private IEnumerator DemonstrationCoroutine()
        {
            if (taskDefinition == null || taskDefinition.steps == null)
            {
                Debug.LogError("[OutletInstallationTask] No steps defined for demonstration");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Watch the demonstration of installing a duplex outlet.\n" +
                    "Each step will be explained with the relevant NEC code.", 5f);
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

                if (hintPanel != null)
                {
                    hintPanel.ShowHint(explanation, demoStepDelay + 2f);
                }

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
                case "strip-romex":
                    if (hotWire != null) highlighter.HighlightObject(hotWire.gameObject);
                    break;
                case "identify-wires":
                    if (hotWire != null) highlighter.HighlightObject(hotWire.gameObject);
                    if (neutralWire != null) highlighter.HighlightObject(neutralWire.gameObject);
                    if (groundWire != null) highlighter.HighlightObject(groundWire.gameObject);
                    break;
                case "measure-wire-length":
                    if (junctionBox != null) highlighter.HighlightObject(junctionBox.gameObject);
                    break;
                case "connect-hot":
                    if (outletHotTerminal != null) highlighter.HighlightObject(outletHotTerminal.gameObject);
                    break;
                case "connect-neutral":
                    if (outletNeutralTerminal != null) highlighter.HighlightObject(outletNeutralTerminal.gameObject);
                    break;
                case "connect-ground":
                    if (outletGroundTerminal != null) highlighter.HighlightObject(outletGroundTerminal.gameObject);
                    break;
                case "tighten-terminations":
                    if (outlet != null) highlighter.HighlightObject(outlet.gameObject);
                    break;
                case "mount-outlet":
                    if (junctionBox != null) highlighter.HighlightObject(junctionBox.gameObject);
                    break;
                case "install-coverplate":
                    if (coverPlate != null) highlighter.HighlightObject(coverPlate.gameObject);
                    break;
            }
        }

        // --- Step Completion ---

        public void OnRomexStripped()
        {
            romexStripped = true;
            TryCompleteStep("strip-romex");
        }

        public void OnWiresIdentified()
        {
            wiresIdentified = true;
            TryCompleteStep("identify-wires");
        }

        public void OnWireLengthVerified()
        {
            wireLengthVerified = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("verify-wire-length");
        }

        public void OnHotWireConnected()
        {
            hotConnected = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-hot");
        }

        public void OnNeutralWireConnected()
        {
            neutralConnected = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-neutral");
        }

        public void OnGroundWireConnected()
        {
            groundConnected = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-ground");
        }

        public void OnTerminationsTightened()
        {
            terminationsTightened = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("tighten-terminations");
        }

        public void OnOutletMounted()
        {
            outletMounted = true;
            TryCompleteStep("mount-outlet");
        }

        public void OnCoverplateInstalled()
        {
            coverplateInstalled = true;
            TryCompleteStep("install-coverplate");
        }

        private void TryCompleteStep(string stepId)
        {
            if (completedStepIds.Contains(stepId)) return;

            bool success = TaskManager.Instance.CompleteStep(stepId);
            if (success)
            {
                completedStepIds.Add(stepId);
                ShowCurrentStepInstruction();

                if (CurrentMode == TaskMode.Practice)
                {
                    AudioManager.Instance.PlayCorrectSound();
                }

                if (taskDefinition != null && taskDefinition.steps != null &&
                    completedStepIds.Count >= taskDefinition.steps.Length)
                {
                    Debug.Log("[OutletInstallationTask] All steps completed!");
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

                if (hintPanel != null)
                {
                    hintPanel.ShowHint(hint);
                }

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
            TaskResult result = ScoreManager.Instance.CalculateGenericStepScore(
                completedStepIds: TaskManager.Instance.GetCompletedStepIds(),
                allSteps: taskDefinition != null ? taskDefinition.steps : null,
                timeUsed: elapsedTime,
                timeLimit: timeLimit,
                mode: CurrentMode,
                bonusScores: new Dictionary<string, float>
                {
                    { "terminations", terminationsTightened ? 5f : 0f },
                    { "wireLength", wireLengthVerified ? 5f : 0f }
                }
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

            ResetTaskState();
        }
    }
}
