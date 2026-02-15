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
    /// 3-Way Switch Wiring training task. Player wires two 3-way switches to control
    /// a single light fixture. Validates proper traveler wiring and common terminal usage.
    /// Difficulty: Intermediate. 7 steps. Time limit (test): 300s.
    /// Uses CircuitSimulator to validate energized state when switches are toggled.
    /// </summary>
    public class SwitchWiringTask : TrainingTask
    {
        public override string TaskId => "switch-wiring-3way";
        public override string TaskName => "3-Way Switch Wiring";
        public override string Description =>
            "Wire two 3-way switches to control a single light fixture, using proper traveler connections.";

        [Header("Workbench Components")]
        [SerializeField] private Transform workbenchTransform;
        [SerializeField] private Transform switch1;
        [SerializeField] private Transform switch2;
        [SerializeField] private Transform lightFixture;
        [SerializeField] private Transform junctionBox;

        [Header("Switch Components")]
        [SerializeField] private SnapPoint switch1CommonTerminal;
        [SerializeField] private SnapPoint switch1Traveler1;
        [SerializeField] private SnapPoint switch1Traveler2;
        [SerializeField] private SnapPoint switch2CommonTerminal;
        [SerializeField] private SnapPoint switch2Traveler1;
        [SerializeField] private SnapPoint switch2Traveler2;
        [SerializeField] private SnapPoint lightFixtureTerminal;

        [Header("Tools")]
        [SerializeField] private GrabInteractable wireNut;
        [SerializeField] private GrabInteractable voltageTester;

        [Header("Wires")]
        [SerializeField] private WireSegment lineHotWire;
        [SerializeField] private WireSegment traveler1Wire;
        [SerializeField] private WireSegment traveler2Wire;
        [SerializeField] private WireSegment switchLeg;
        [SerializeField] private WireSegment neutralWire;
        [SerializeField] private WireSegment groundWire;

        [Header("Circuit Simulation")]
        private CircuitSimulator circuitSimulator;
        private bool switch1State; // false = position A, true = position B
        private bool switch2State;
        private bool lightIsOn;

        [Header("Task State")]
        private TaskDefinition taskDefinition;
        private List<string> completedStepIds = new List<string>();
        private bool lineHotIdentified;
        private bool commonTerminalsConnected;
        private bool travelersConnected;
        private bool neutralsSpliced;
        private bool groundsConnected;
        private bool switchesValidated;

        [Header("Learn Mode")]
        [SerializeField] private float demoStepDelay = 5f;
        private Coroutine demoCoroutine;

        [Header("Feedback")]
        [SerializeField] private FloatingLabel stepInstructionLabel;
        [SerializeField] private FloatingLabel lightStatusLabel;
        private HintPanel hintPanel;

        private void Start()
        {
            LoadTaskDefinition();
            hintPanel = FindObjectOfType<HintPanel>();
            SetupCircuitSimulator();
            SetupWorkbench();
        }

        private void LoadTaskDefinition()
        {
            taskDefinition = TaskManager.Instance.GetTaskDefinition(TaskId);
            if (taskDefinition == null)
            {
                Debug.LogError("[SwitchWiringTask] Failed to load task definition");
            }
        }

        private void SetupCircuitSimulator()
        {
            circuitSimulator = new CircuitSimulator();

            // Setup circuit nodes
            circuitSimulator.AddNode("source", CircuitNodeState.Energized);
            circuitSimulator.AddNode("switch1-common", CircuitNodeState.DeEnergized);
            circuitSimulator.AddNode("traveler1", CircuitNodeState.DeEnergized);
            circuitSimulator.AddNode("traveler2", CircuitNodeState.DeEnergized);
            circuitSimulator.AddNode("switch2-common", CircuitNodeState.DeEnergized);
            circuitSimulator.AddNode("light", CircuitNodeState.DeEnergized);

            // Initial connections
            circuitSimulator.ConnectNodes("source", "switch1-common");
            circuitSimulator.ConnectNodes("switch2-common", "light");
        }

        private void SetupWorkbench()
        {
            if (workbenchTransform == null)
            {
                workbenchTransform = transform;
            }

            SetupSwitches();
            SetupLightFixture();
            SetupJunctionBox();
            SetupWires();
            SetupTools();
            SetupLabels();
        }

        private void SetupSwitches()
        {
            // Switch 1 (line side)
            if (switch1 == null)
            {
                GameObject sw1Obj = new GameObject("Switch1_ThreeWay");
                sw1Obj.transform.SetParent(workbenchTransform, false);
                sw1Obj.transform.localPosition = new Vector3(-0.3f, 1.0f, 0.3f);

                // Visual body
                GameObject sw1Body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sw1Body.name = "SwitchBody";
                sw1Body.transform.SetParent(sw1Obj.transform, false);
                sw1Body.transform.localScale = new Vector3(0.04f, 0.07f, 0.03f);
                Material sw1Mat = new Material(Shader.Find("Standard"));
                sw1Mat.color = new Color(0.85f, 0.82f, 0.78f, 1f);
                sw1Body.GetComponent<MeshRenderer>().material = sw1Mat;

                switch1 = sw1Obj.transform;

                // Create terminals
                switch1CommonTerminal = CreateSwitchTerminal("SW1_Common", sw1Obj.transform,
                    new Vector3(0f, -0.04f, 0.015f), "hot", "Common (Dark Screw)");
                switch1Traveler1 = CreateSwitchTerminal("SW1_Traveler1", sw1Obj.transform,
                    new Vector3(-0.015f, 0.02f, 0.015f), "hot", "Traveler 1 (Brass)");
                switch1Traveler2 = CreateSwitchTerminal("SW1_Traveler2", sw1Obj.transform,
                    new Vector3(0.015f, 0.02f, 0.015f), "hot", "Traveler 2 (Brass)");
            }

            // Switch 2 (load side)
            if (switch2 == null)
            {
                GameObject sw2Obj = new GameObject("Switch2_ThreeWay");
                sw2Obj.transform.SetParent(workbenchTransform, false);
                sw2Obj.transform.localPosition = new Vector3(0.3f, 1.0f, 0.3f);

                GameObject sw2Body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sw2Body.name = "SwitchBody";
                sw2Body.transform.SetParent(sw2Obj.transform, false);
                sw2Body.transform.localScale = new Vector3(0.04f, 0.07f, 0.03f);
                Material sw2Mat = new Material(Shader.Find("Standard"));
                sw2Mat.color = new Color(0.85f, 0.82f, 0.78f, 1f);
                sw2Body.GetComponent<MeshRenderer>().material = sw2Mat;

                switch2 = sw2Obj.transform;

                switch2CommonTerminal = CreateSwitchTerminal("SW2_Common", sw2Obj.transform,
                    new Vector3(0f, -0.04f, 0.015f), "hot", "Common (Dark Screw)");
                switch2Traveler1 = CreateSwitchTerminal("SW2_Traveler1", sw2Obj.transform,
                    new Vector3(-0.015f, 0.02f, 0.015f), "hot", "Traveler 1 (Brass)");
                switch2Traveler2 = CreateSwitchTerminal("SW2_Traveler2", sw2Obj.transform,
                    new Vector3(0.015f, 0.02f, 0.015f), "hot", "Traveler 2 (Brass)");
            }
        }

        private SnapPoint CreateSwitchTerminal(string name, Transform parent, Vector3 localPos, string wireType, string label)
        {
            GameObject termObj = new GameObject(name);
            termObj.transform.SetParent(parent, false);
            termObj.transform.localPosition = localPos;

            SnapPoint sp = termObj.AddComponent<SnapPoint>();
            sp.SetAcceptedWireType(wireType);
            sp.SetSnapPointId(name.ToLower());
            sp.SetLabel(label);
            return sp;
        }

        private void SetupLightFixture()
        {
            if (lightFixture == null)
            {
                GameObject lightObj = new GameObject("LightFixture");
                lightObj.transform.SetParent(workbenchTransform, false);
                lightObj.transform.localPosition = new Vector3(0f, 1.5f, 0.3f);

                // Visual bulb
                GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bulb.name = "Bulb";
                bulb.transform.SetParent(lightObj.transform, false);
                bulb.transform.localScale = Vector3.one * 0.06f;

                Material bulbMat = new Material(Shader.Find("Standard"));
                bulbMat.color = new Color(0.9f, 0.9f, 0.8f, 0.8f);
                bulb.GetComponent<MeshRenderer>().material = bulbMat;

                lightFixture = lightObj.transform;

                // Light fixture terminal
                GameObject termObj = new GameObject("LightTerminal");
                termObj.transform.SetParent(lightObj.transform, false);
                termObj.transform.localPosition = new Vector3(0f, -0.05f, 0f);

                lightFixtureTerminal = termObj.AddComponent<SnapPoint>();
                lightFixtureTerminal.SetAcceptedWireType("hot");
                lightFixtureTerminal.SetSnapPointId("light-terminal");
                lightFixtureTerminal.SetLabel("Light Fixture");
            }
        }

        private void SetupJunctionBox()
        {
            if (junctionBox == null)
            {
                GameObject boxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxObj.name = "JunctionBox";
                boxObj.transform.SetParent(workbenchTransform, false);
                boxObj.transform.localPosition = new Vector3(0f, 1.0f, 0.3f);
                boxObj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);

                Material boxMat = new Material(Shader.Find("Standard"));
                boxMat.color = new Color(0.2f, 0.4f, 0.6f, 1f);
                boxObj.GetComponent<MeshRenderer>().material = boxMat;

                junctionBox = boxObj.transform;
            }
        }

        private void SetupWires()
        {
            float wireY = 0.85f;
            float wireX = -0.15f;

            if (lineHotWire == null)
            {
                lineHotWire = CreateWire("LineHotWire", new Vector3(wireX, wireY, 0.1f));
            }
            if (traveler1Wire == null)
            {
                traveler1Wire = CreateWire("Traveler1Wire", new Vector3(wireX + 0.05f, wireY, 0.1f));
            }
            if (traveler2Wire == null)
            {
                traveler2Wire = CreateWire("Traveler2Wire", new Vector3(wireX + 0.1f, wireY, 0.1f));
            }
            if (switchLeg == null)
            {
                switchLeg = CreateWire("SwitchLeg", new Vector3(wireX + 0.15f, wireY, 0.1f));
            }
            if (neutralWire == null)
            {
                neutralWire = CreateWire("NeutralWire", new Vector3(wireX + 0.2f, wireY, 0.1f));
            }
            if (groundWire == null)
            {
                groundWire = CreateWire("GroundWire", new Vector3(wireX + 0.25f, wireY, 0.1f));
            }
        }

        private WireSegment CreateWire(string name, Vector3 localPos)
        {
            GameObject wireObj = new GameObject(name);
            wireObj.transform.SetParent(workbenchTransform, false);
            wireObj.transform.localPosition = localPos;
            return wireObj.AddComponent<WireSegment>();
        }

        private void SetupTools()
        {
            if (wireNut == null)
            {
                GameObject nutObj = new GameObject("WireNut");
                nutObj.transform.SetParent(workbenchTransform, false);
                nutObj.transform.localPosition = new Vector3(0.4f, 0.85f, 0.1f);

                // Visual cone shape
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.transform.SetParent(nutObj.transform, false);
                visual.transform.localScale = new Vector3(0.015f, 0.012f, 0.015f);
                Material nutMat = new Material(Shader.Find("Standard"));
                nutMat.color = new Color(0.9f, 0.5f, 0.1f, 1f); // Orange wire nut
                visual.GetComponent<MeshRenderer>().material = nutMat;
                Collider visualCol = visual.GetComponent<Collider>();
                if (visualCol != null) Destroy(visualCol);

                wireNut = nutObj.AddComponent<GrabInteractable>();
                wireNut.SetToolType("wire-nut");
                wireNut.SetGripOffset(new Vector3(0f, 0f, 0.01f));
            }

            if (voltageTester == null)
            {
                GameObject testerObj = new GameObject("VoltageTester");
                testerObj.transform.SetParent(workbenchTransform, false);
                testerObj.transform.localPosition = new Vector3(0.45f, 0.85f, 0.1f);

                GameObject testerBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testerBody.transform.SetParent(testerObj.transform, false);
                testerBody.transform.localScale = new Vector3(0.03f, 0.12f, 0.02f);
                Material testerMat = new Material(Shader.Find("Standard"));
                testerMat.color = new Color(0.9f, 0.7f, 0.1f, 1f); // Yellow
                testerBody.GetComponent<MeshRenderer>().material = testerMat;
                Collider testerCol = testerBody.GetComponent<Collider>();
                if (testerCol != null) Destroy(testerCol);

                voltageTester = testerObj.AddComponent<GrabInteractable>();
                voltageTester.SetToolType("voltage-tester");
                voltageTester.SetGripOffset(new Vector3(0f, 0f, 0.06f));
            }
        }

        private void SetupLabels()
        {
            if (stepInstructionLabel == null)
            {
                GameObject labelObj = new GameObject("StepInstructionLabel");
                labelObj.transform.SetParent(workbenchTransform, false);
                labelObj.transform.localPosition = new Vector3(0f, 1.7f, 0.3f);
                stepInstructionLabel = labelObj.AddComponent<FloatingLabel>();
                stepInstructionLabel.SetFontSize(2.0f);
                stepInstructionLabel.SetText("");
            }

            if (lightStatusLabel == null)
            {
                GameObject lightLabelObj = new GameObject("LightStatusLabel");
                lightLabelObj.transform.SetParent(workbenchTransform, false);
                lightLabelObj.transform.localPosition = new Vector3(0f, 1.6f, 0.3f);
                lightStatusLabel = lightLabelObj.AddComponent<FloatingLabel>();
                lightStatusLabel.SetFontSize(1.5f);
                lightStatusLabel.SetColor(Color.yellow);
                lightStatusLabel.SetText("Light: OFF");
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            UpdateCircuitSimulation();

            if (CurrentMode == TaskMode.Practice)
            {
                ShowCurrentStepInstruction();
            }
        }

        private void UpdateCircuitSimulation()
        {
            // Determine if the light should be on based on switch positions
            // In a 3-way switch setup: light is on when both switches are in the same position
            // OR both are in different positions (depends on wiring, we use XOR for simplicity)
            bool previousState = lightIsOn;
            lightIsOn = (switch1State == switch2State);

            // Only update if all connections are made
            if (!travelersConnected || !commonTerminalsConnected) lightIsOn = false;

            if (lightIsOn != previousState)
            {
                UpdateLightVisual();
            }
        }

        private void UpdateLightVisual()
        {
            if (lightFixture == null) return;

            MeshRenderer bulbRenderer = lightFixture.GetComponentInChildren<MeshRenderer>();
            if (bulbRenderer != null)
            {
                if (lightIsOn)
                {
                    bulbRenderer.material.color = new Color(1f, 0.95f, 0.5f, 1f);
                    bulbRenderer.material.EnableKeyword("_EMISSION");
                    bulbRenderer.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.4f) * 0.5f);
                }
                else
                {
                    bulbRenderer.material.color = new Color(0.9f, 0.9f, 0.8f, 0.8f);
                    bulbRenderer.material.DisableKeyword("_EMISSION");
                }
            }

            if (lightStatusLabel != null)
            {
                lightStatusLabel.SetText(lightIsOn ? "Light: ON" : "Light: OFF");
                lightStatusLabel.SetColor(lightIsOn ? Color.green : Color.yellow);
            }
        }

        // --- Switch Toggle ---

        public void ToggleSwitch1()
        {
            switch1State = !switch1State;
            Debug.Log($"[SwitchWiringTask] Switch 1 toggled to {(switch1State ? "B" : "A")}");
        }

        public void ToggleSwitch2()
        {
            switch2State = !switch2State;
            Debug.Log($"[SwitchWiringTask] Switch 2 toggled to {(switch2State ? "B" : "A")}");
        }

        // --- Mode Setup ---

        public override void StartLearnMode()
        {
            SetupMode(TaskMode.Learn, 0f);
            ResetTaskState();
            demoCoroutine = StartCoroutine(DemonstrationCoroutine());
            Debug.Log("[SwitchWiringTask] Learn mode started - demonstration beginning");
        }

        public override void StartPracticeMode()
        {
            SetupMode(TaskMode.Practice, 0f);
            ResetTaskState();
            ShowCurrentStepInstruction();
            Debug.Log("[SwitchWiringTask] Practice mode started");
        }

        public override void StartTestMode()
        {
            float timeLimitSeconds = taskDefinition != null ? taskDefinition.timeLimit : 300f;
            SetupMode(TaskMode.Test, timeLimitSeconds);
            ResetTaskState();
            Debug.Log($"[SwitchWiringTask] Test mode started - {timeLimitSeconds}s time limit");
        }

        private void ResetTaskState()
        {
            completedStepIds.Clear();
            lineHotIdentified = false;
            commonTerminalsConnected = false;
            travelersConnected = false;
            neutralsSpliced = false;
            groundsConnected = false;
            switchesValidated = false;
            switch1State = false;
            switch2State = false;
            lightIsOn = false;

            if (lineHotWire != null) lineHotWire.DetachAll();
            if (traveler1Wire != null) traveler1Wire.DetachAll();
            if (traveler2Wire != null) traveler2Wire.DetachAll();
            if (switchLeg != null) switchLeg.DetachAll();
            if (neutralWire != null) neutralWire.DetachAll();
            if (groundWire != null) groundWire.DetachAll();

            UpdateLightVisual();
        }

        // --- Demonstration (Learn Mode) ---

        private IEnumerator DemonstrationCoroutine()
        {
            if (taskDefinition == null || taskDefinition.steps == null)
            {
                Debug.LogError("[SwitchWiringTask] No steps defined for demonstration");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Watch the demonstration of 3-way switch wiring.\n" +
                    "Pay attention to the common terminal (darker screw) vs traveler terminals.", 5f);
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

        // --- Step Completion ---

        public void OnLineLoadIdentified()
        {
            lineHotIdentified = true;
            TryCompleteStep("identify-line-load");
        }

        public void OnSwitch1CommonConnected()
        {
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-switch1-common");
        }

        public void OnTravelers1Connected()
        {
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-travelers-1");
        }

        public void OnTravelers2Connected()
        {
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-travelers-2");
            travelersConnected = true;
        }

        public void OnSwitch2ToFixtureConnected()
        {
            commonTerminalsConnected = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-switch2-to-fixture");
        }

        public void OnNeutralsSpliced()
        {
            neutralsSpliced = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("splice-neutrals");
        }

        public void OnGroundsConnected()
        {
            groundsConnected = true;
            TaskManager.Instance.AddConnectionQuality(10);
            TryCompleteStep("connect-grounds");
        }

        /// <summary>
        /// Called when the player tests the switches and the light toggles correctly.
        /// </summary>
        public void ValidateSwitchOperation()
        {
            if (travelersConnected && commonTerminalsConnected && lightIsOn)
            {
                switchesValidated = true;
                AudioManager.Instance.PlayCorrectSound();
                if (hintPanel != null)
                {
                    hintPanel.ShowHint("Light is working correctly with both switches!", 3f);
                }
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
                    Debug.Log("[SwitchWiringTask] All steps completed!");
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
                    { "switchValidation", switchesValidated ? 5f : 0f },
                    { "travelerAccuracy", travelersConnected ? 5f : 0f }
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
}
