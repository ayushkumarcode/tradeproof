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
    /// GFCI Testing & Troubleshooting training task. Player tests 3 daisy-chained GFCI outlets,
    /// finds the faulty one, and replaces it. One GFCI is randomly set as faulty.
    /// Difficulty: Intermediate. 7 steps. Time limit (test): 240s.
    /// </summary>
    public class GFCITestingTask : TrainingTask
    {
        public override string TaskId => "gfci-testing-residential";
        public override string TaskName => "GFCI Testing & Troubleshooting";
        public override string Description =>
            "Test three daisy-chained GFCI outlets, identify the faulty one, and replace it.";

        [Header("Workbench Components")]
        [SerializeField] private Transform workbenchTransform;

        [Header("GFCI Outlets")]
        [SerializeField] private GFCIOutlet gfci1;
        [SerializeField] private GFCIOutlet gfci2;
        [SerializeField] private GFCIOutlet gfci3;
        [SerializeField] private int faultyGFCIIndex = -1; // Set during setup

        [Header("Tools")]
        [SerializeField] private GrabInteractable voltageTester;
        [SerializeField] private GrabInteractable multimeter;

        [Header("Replacement GFCI")]
        [SerializeField] private GFCIOutlet replacementGFCI;

        [Header("Task State")]
        private TaskDefinition taskDefinition;
        private List<string> completedStepIds = new List<string>();
        private bool powerConfirmed;
        private bool gfciTested;
        private bool downstreamVerified;
        private bool gfciReset;
        private bool daisyChainTraced;
        private bool faultyGFCIFound;
        private bool faultyGFCIReplaced;
        private int identifiedFaultyIndex = -1;

        [Header("Learn Mode")]
        [SerializeField] private float demoStepDelay = 5f;
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
                Debug.LogError("[GFCITestingTask] Failed to load task definition");
            }
        }

        private void SetupWorkbench()
        {
            if (workbenchTransform == null)
            {
                workbenchTransform = transform;
            }

            SetupGFCIOutlets();
            SetupTools();
            SetupReplacementGFCI();
            SetupInstructionLabel();
        }

        private void SetupGFCIOutlets()
        {
            // Randomly choose which GFCI is faulty
            faultyGFCIIndex = Random.Range(0, 3);

            // GFCI 1
            if (gfci1 == null)
            {
                gfci1 = CreateGFCIOutlet("GFCI_1", new Vector3(-0.25f, 1.0f, 0.3f), faultyGFCIIndex == 0);
            }

            // GFCI 2
            if (gfci2 == null)
            {
                gfci2 = CreateGFCIOutlet("GFCI_2", new Vector3(0f, 1.0f, 0.3f), faultyGFCIIndex == 1);
            }

            // GFCI 3
            if (gfci3 == null)
            {
                gfci3 = CreateGFCIOutlet("GFCI_3", new Vector3(0.25f, 1.0f, 0.3f), faultyGFCIIndex == 2);
            }

            // Add labels
            AddOutletLabel(gfci1, "GFCI #1\n(Kitchen Counter)");
            AddOutletLabel(gfci2, "GFCI #2\n(Kitchen Sink)");
            AddOutletLabel(gfci3, "GFCI #3\n(Dishwasher)");

            Debug.Log($"[GFCITestingTask] Faulty GFCI index: {faultyGFCIIndex}");
        }

        private GFCIOutlet CreateGFCIOutlet(string name, Vector3 localPos, bool isFaulty)
        {
            GameObject gfciObj = new GameObject(name);
            gfciObj.transform.SetParent(workbenchTransform, false);
            gfciObj.transform.localPosition = localPos;

            // Visual body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "GFCIBody";
            body.transform.SetParent(gfciObj.transform, false);
            body.transform.localScale = new Vector3(0.070f, 0.114f, 0.030f);

            Material bodyMat = new Material(Shader.Find("Standard"));
            bodyMat.color = new Color(0.92f, 0.90f, 0.85f, 1f);
            body.GetComponent<MeshRenderer>().material = bodyMat;

            // TEST button (red)
            GameObject testBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testBtn.name = "TestButton";
            testBtn.transform.SetParent(gfciObj.transform, false);
            testBtn.transform.localPosition = new Vector3(0f, 0.015f, -0.016f);
            testBtn.transform.localScale = new Vector3(0.012f, 0.008f, 0.005f);
            Material testMat = new Material(Shader.Find("Standard"));
            testMat.color = Color.red;
            testBtn.GetComponent<MeshRenderer>().material = testMat;

            // RESET button (black)
            GameObject resetBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            resetBtn.name = "ResetButton";
            resetBtn.transform.SetParent(gfciObj.transform, false);
            resetBtn.transform.localPosition = new Vector3(0f, -0.015f, -0.016f);
            resetBtn.transform.localScale = new Vector3(0.012f, 0.008f, 0.005f);
            Material resetMat = new Material(Shader.Find("Standard"));
            resetMat.color = Color.black;
            resetBtn.GetComponent<MeshRenderer>().material = resetMat;

            GFCIOutlet gfci = gfciObj.AddComponent<GFCIOutlet>();
            gfci.Initialize(isFaulty);

            return gfci;
        }

        private void AddOutletLabel(GFCIOutlet gfci, string text)
        {
            if (gfci == null) return;

            GameObject labelObj = new GameObject("GFCILabel");
            labelObj.transform.SetParent(gfci.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 0.08f, 0f);

            FloatingLabel label = labelObj.AddComponent<FloatingLabel>();
            label.SetText(text);
            label.SetFontSize(1.5f);
        }

        private void SetupTools()
        {
            if (voltageTester == null)
            {
                GameObject testerObj = new GameObject("VoltageTester");
                testerObj.transform.SetParent(workbenchTransform, false);
                testerObj.transform.localPosition = new Vector3(-0.4f, 0.85f, 0.1f);

                GameObject testerBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testerBody.transform.SetParent(testerObj.transform, false);
                testerBody.transform.localScale = new Vector3(0.03f, 0.12f, 0.02f);
                Material testerMat = new Material(Shader.Find("Standard"));
                testerMat.color = new Color(0.9f, 0.7f, 0.1f, 1f);
                testerBody.GetComponent<MeshRenderer>().material = testerMat;
                Collider testerCol = testerBody.GetComponent<Collider>();
                if (testerCol != null) Destroy(testerCol);

                voltageTester = testerObj.AddComponent<GrabInteractable>();
                voltageTester.SetToolType("voltage-tester");
                voltageTester.SetGripOffset(new Vector3(0f, 0f, 0.06f));
            }

            if (multimeter == null)
            {
                GameObject meterObj = new GameObject("Multimeter");
                meterObj.transform.SetParent(workbenchTransform, false);
                meterObj.transform.localPosition = new Vector3(-0.35f, 0.85f, 0.1f);

                GameObject meterBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                meterBody.transform.SetParent(meterObj.transform, false);
                meterBody.transform.localScale = new Vector3(0.05f, 0.09f, 0.025f);
                Material meterMat = new Material(Shader.Find("Standard"));
                meterMat.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                meterBody.GetComponent<MeshRenderer>().material = meterMat;
                Collider meterCol = meterBody.GetComponent<Collider>();
                if (meterCol != null) Destroy(meterCol);

                multimeter = meterObj.AddComponent<GrabInteractable>();
                multimeter.SetToolType("multimeter");
                multimeter.SetGripOffset(new Vector3(0f, 0f, 0.04f));
            }
        }

        private void SetupReplacementGFCI()
        {
            if (replacementGFCI == null)
            {
                replacementGFCI = CreateGFCIOutlet("ReplacementGFCI", new Vector3(0.4f, 0.85f, 0.1f), false);

                GrabInteractable replGrab = replacementGFCI.gameObject.AddComponent<GrabInteractable>();
                replGrab.SetGripOffset(new Vector3(0f, 0f, 0.02f));
                replGrab.SetToolType("gfci-replacement");

                // Label
                AddOutletLabel(replacementGFCI, "NEW GFCI\n(Replacement)");
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
            Debug.Log("[GFCITestingTask] Learn mode started - demonstration beginning");
        }

        public override void StartPracticeMode()
        {
            SetupMode(TaskMode.Practice, 0f);
            ResetTaskState();
            ShowCurrentStepInstruction();
            Debug.Log("[GFCITestingTask] Practice mode started");
        }

        public override void StartTestMode()
        {
            float timeLimitSeconds = taskDefinition != null ? taskDefinition.timeLimit : 240f;
            SetupMode(TaskMode.Test, timeLimitSeconds);
            ResetTaskState();
            Debug.Log($"[GFCITestingTask] Test mode started - {timeLimitSeconds}s time limit");
        }

        private void ResetTaskState()
        {
            completedStepIds.Clear();
            powerConfirmed = false;
            gfciTested = false;
            downstreamVerified = false;
            gfciReset = false;
            daisyChainTraced = false;
            faultyGFCIFound = false;
            faultyGFCIReplaced = false;
            identifiedFaultyIndex = -1;

            // Re-randomize the faulty GFCI
            faultyGFCIIndex = Random.Range(0, 3);
            if (gfci1 != null) gfci1.Initialize(faultyGFCIIndex == 0);
            if (gfci2 != null) gfci2.Initialize(faultyGFCIIndex == 1);
            if (gfci3 != null) gfci3.Initialize(faultyGFCIIndex == 2);

            Debug.Log($"[GFCITestingTask] Reset - faulty GFCI index: {faultyGFCIIndex}");
        }

        // --- Demonstration (Learn Mode) ---

        private IEnumerator DemonstrationCoroutine()
        {
            if (taskDefinition == null || taskDefinition.steps == null)
            {
                Debug.LogError("[GFCITestingTask] No steps defined for demonstration");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Watch the demonstration of GFCI testing and troubleshooting.\n" +
                    "GFCI outlets protect against electrocution by detecting ground faults.", 5f);
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

                AudioManager.Instance.PlayHintSound();
                yield return new WaitForSeconds(demoStepDelay + 2f);
            }

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Demonstration complete!\nSwitch to Practice mode to try it yourself.", 5f);
            }
        }

        // --- GFCI Interaction ---

        /// <summary>
        /// Test a GFCI outlet by pressing its TEST button.
        /// Returns the test result.
        /// </summary>
        public GFCITestResult TestGFCI(int gfciIndex)
        {
            GFCIOutlet gfci = GetGFCIByIndex(gfciIndex);
            if (gfci == null) return GFCITestResult.InvalidOutlet;

            GFCITestResult result = gfci.PressTestButton();

            if (CurrentMode == TaskMode.Practice)
            {
                string resultText = result == GFCITestResult.TripSuccess
                    ? $"GFCI #{gfciIndex + 1} tripped successfully"
                    : $"GFCI #{gfciIndex + 1} FAILED to trip - this outlet may be faulty!";

                if (hintPanel != null)
                {
                    hintPanel.ShowHint(resultText, 3f);
                }
            }

            Debug.Log($"[GFCITestingTask] GFCI #{gfciIndex + 1} test result: {result}");
            return result;
        }

        /// <summary>
        /// Reset a GFCI outlet by pressing its RESET button.
        /// </summary>
        public bool ResetGFCI(int gfciIndex)
        {
            GFCIOutlet gfci = GetGFCIByIndex(gfciIndex);
            if (gfci == null) return false;
            return gfci.PressResetButton();
        }

        /// <summary>
        /// Use the voltage tester on a GFCI outlet.
        /// Returns the voltage reading.
        /// </summary>
        public float TestVoltage(int gfciIndex)
        {
            GFCIOutlet gfci = GetGFCIByIndex(gfciIndex);
            if (gfci == null) return 0f;
            return gfci.GetVoltageReading();
        }

        /// <summary>
        /// Player identifies which GFCI is faulty.
        /// </summary>
        public bool IdentifyFaultyGFCI(int gfciIndex)
        {
            identifiedFaultyIndex = gfciIndex;

            if (gfciIndex == faultyGFCIIndex)
            {
                faultyGFCIFound = true;
                AudioManager.Instance.PlayCorrectSound();

                if (CurrentMode == TaskMode.Practice && hintPanel != null)
                {
                    hintPanel.ShowHint($"Correct! GFCI #{gfciIndex + 1} is the faulty outlet.", 3f);
                }

                Debug.Log($"[GFCITestingTask] Correctly identified faulty GFCI: #{gfciIndex + 1}");
                return true;
            }
            else
            {
                AudioManager.Instance.PlayIncorrectSound();

                if (CurrentMode == TaskMode.Practice && hintPanel != null)
                {
                    hintPanel.ShowHint($"Incorrect. GFCI #{gfciIndex + 1} is not the faulty one. Keep testing.", 3f);
                }

                Debug.Log($"[GFCITestingTask] Incorrect identification: #{gfciIndex + 1} (actual: #{faultyGFCIIndex + 1})");
                return false;
            }
        }

        private GFCIOutlet GetGFCIByIndex(int index)
        {
            switch (index)
            {
                case 0: return gfci1;
                case 1: return gfci2;
                case 2: return gfci3;
                default: return null;
            }
        }

        // --- Step Completion ---

        public void OnPowerConfirmed()
        {
            powerConfirmed = true;
            TryCompleteStep("confirm-power");
        }

        public void OnGFCITested()
        {
            gfciTested = true;
            TryCompleteStep("test-gfci");
        }

        public void OnDownstreamVerified()
        {
            downstreamVerified = true;
            TryCompleteStep("verify-downstream-dead");
        }

        public void OnGFCIReset()
        {
            gfciReset = true;
            TryCompleteStep("reset-gfci");
        }

        public void OnDaisyChainTraced()
        {
            daisyChainTraced = true;
            TryCompleteStep("trace-daisy-chain");
        }

        public void OnFaultyGFCIFound()
        {
            if (faultyGFCIFound)
            {
                TryCompleteStep("find-faulty-gfci");
            }
        }

        public void OnFaultyGFCIReplaced()
        {
            if (faultyGFCIFound)
            {
                faultyGFCIReplaced = true;
                TryCompleteStep("replace-faulty-gfci");
            }
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
                    Debug.Log("[GFCITestingTask] All steps completed!");
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
                    { "faultFound", faultyGFCIFound ? 5f : 0f },
                    { "faultReplaced", faultyGFCIReplaced ? 5f : 0f }
                }
            );

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

    // --- Supporting Types ---

    public enum GFCITestResult
    {
        TripSuccess,    // GFCI tripped correctly
        TripFailed,     // GFCI failed to trip (faulty)
        AlreadyTripped, // GFCI was already tripped
        InvalidOutlet   // Not a valid GFCI
    }

    /// <summary>
    /// Represents a GFCI outlet with TEST/RESET buttons and fault detection.
    /// </summary>
    public class GFCIOutlet : MonoBehaviour
    {
        [SerializeField] private bool isFaulty;
        [SerializeField] private bool isTripped;
        [SerializeField] private bool hasPower = true;
        [SerializeField] private float voltage = 120f;

        public bool IsFaulty => isFaulty;
        public bool IsTripped => isTripped;
        public bool HasPower => hasPower && !isTripped;

        public void Initialize(bool faulty)
        {
            isFaulty = faulty;
            isTripped = false;
            hasPower = true;
            voltage = 120f;
        }

        /// <summary>
        /// Press the TEST button. Returns the test result.
        /// A faulty GFCI will fail to trip.
        /// </summary>
        public GFCITestResult PressTestButton()
        {
            if (isTripped) return GFCITestResult.AlreadyTripped;

            if (isFaulty)
            {
                // Faulty GFCI does NOT trip
                Debug.Log($"[GFCIOutlet] {name} TEST pressed - FAILED to trip (faulty)");
                return GFCITestResult.TripFailed;
            }
            else
            {
                // Normal GFCI trips
                isTripped = true;
                Debug.Log($"[GFCIOutlet] {name} TEST pressed - tripped successfully");
                return GFCITestResult.TripSuccess;
            }
        }

        /// <summary>
        /// Press the RESET button.
        /// Returns true if successfully reset, false if faulty.
        /// </summary>
        public bool PressResetButton()
        {
            if (isFaulty)
            {
                Debug.Log($"[GFCIOutlet] {name} RESET pressed - FAILED (faulty, won't hold reset)");
                return false;
            }

            isTripped = false;
            Debug.Log($"[GFCIOutlet] {name} RESET pressed - reset successfully");
            return true;
        }

        /// <summary>
        /// Get the voltage reading at this outlet.
        /// </summary>
        public float GetVoltageReading()
        {
            if (isTripped) return 0f;
            if (!hasPower) return 0f;
            return voltage;
        }

        /// <summary>
        /// Set whether this outlet has incoming power (for daisy-chain testing).
        /// </summary>
        public void SetHasPower(bool power)
        {
            hasPower = power;
        }
    }
}
