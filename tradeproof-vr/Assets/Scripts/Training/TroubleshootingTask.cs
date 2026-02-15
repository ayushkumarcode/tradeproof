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
    /// Circuit Troubleshooting training task. Player responds to a customer complaint
    /// about a dead outlet, diagnoses the fault using tools, identifies and repairs it.
    /// Uses FaultSystem for fault injection and NPCDialogue for customer interaction.
    /// Difficulty: Advanced. 10 steps. Time limit (test): 420s.
    /// </summary>
    public class TroubleshootingTask : TrainingTask
    {
        public override string TaskId => "troubleshooting-residential";
        public override string TaskName => "Circuit Troubleshooting";
        public override string Description =>
            "Respond to a customer complaint, diagnose a circuit fault using testing tools, and repair the issue.";

        [Header("Scene Components")]
        [SerializeField] private Transform sceneRoot;
        [SerializeField] private ElectricalPanel electricalPanel;
        [SerializeField] private Outlet outlet1;
        [SerializeField] private Outlet outlet2;
        [SerializeField] private Outlet outlet3;
        [SerializeField] private Transform junctionBox1;
        [SerializeField] private Transform junctionBox2;

        [Header("Tools")]
        [SerializeField] private GrabInteractable multimeter;
        [SerializeField] private GrabInteractable voltageTester;

        [Header("Customer NPC")]
        [SerializeField] private Transform customerNPC;
        [SerializeField] private MeshRenderer customerRenderer;
        [SerializeField] private FloatingLabel customerNameLabel;
        [SerializeField] private FloatingLabel dialogueLabel;
        [SerializeField] private float npcInteractionDistance = 1.5f;

        [Header("Fault System")]
        private FaultInjector faultInjector;
        private FaultDiagnostic faultDiagnostic;
        private CircuitSimulator circuitSimulator;

        [Header("Dialogue System")]
        private DialogueTree dialogueTree;
        private DialogueRunner dialogueRunner;
        private bool dialogueActive;

        [Header("Task State")]
        private TaskDefinition taskDefinition;
        private List<string> completedStepIds = new List<string>();
        private bool complaintRead;
        private bool diagnosticsAsked;
        private bool panelChecked;
        private bool panelVoltagesTested;
        private bool outletVoltagesTested;
        private bool circuitTraced;
        private bool faultIdentified;
        private bool faultRepaired;
        private bool repairVerified;
        private bool reportedToCustomer;

        [Header("Learn Mode")]
        [SerializeField] private float demoStepDelay = 5f;
        private Coroutine demoCoroutine;

        [Header("Feedback")]
        [SerializeField] private FloatingLabel stepInstructionLabel;
        [SerializeField] private FloatingLabel voltageDisplay;
        private HintPanel hintPanel;

        private void Start()
        {
            LoadTaskDefinition();
            hintPanel = FindObjectOfType<HintPanel>();
            SetupScene();
        }

        private void LoadTaskDefinition()
        {
            taskDefinition = TaskManager.Instance.GetTaskDefinition(TaskId);
            if (taskDefinition == null)
            {
                Debug.LogError("[TroubleshootingTask] Failed to load task definition");
            }
        }

        private void SetupScene()
        {
            if (sceneRoot == null)
            {
                sceneRoot = transform;
            }

            SetupCircuitSimulator();
            SetupFaultSystem();
            SetupDialogueSystem();
            SetupElectricalPanel();
            SetupOutlets();
            SetupJunctionBoxes();
            SetupTools();
            SetupCustomerNPC();
            SetupLabels();
        }

        private void SetupCircuitSimulator()
        {
            circuitSimulator = new CircuitSimulator();

            // Build the circuit graph
            circuitSimulator.AddNode("panel-breaker", CircuitNodeState.Energized);
            circuitSimulator.AddNode("junction-box-1", CircuitNodeState.Energized);
            circuitSimulator.AddNode("junction-box-2", CircuitNodeState.Energized);
            circuitSimulator.AddNode("outlet-1", CircuitNodeState.Energized);
            circuitSimulator.AddNode("outlet-2", CircuitNodeState.Energized);
            circuitSimulator.AddNode("outlet-3", CircuitNodeState.Energized);

            // Connections
            circuitSimulator.ConnectNodes("panel-breaker", "junction-box-1");
            circuitSimulator.ConnectNodes("junction-box-1", "outlet-1");
            circuitSimulator.ConnectNodes("junction-box-1", "junction-box-2");
            circuitSimulator.ConnectNodes("junction-box-2", "outlet-2");
            circuitSimulator.ConnectNodes("junction-box-2", "outlet-3");
        }

        private void SetupFaultSystem()
        {
            faultInjector = new FaultInjector();
            faultDiagnostic = new FaultDiagnostic();

            // Inject the fault from task definition
            if (taskDefinition != null && taskDefinition.faults != null && taskDefinition.faults.Length > 0)
            {
                FaultDefinition faultDef = taskDefinition.faults[0]; // Use the first defined fault
                FaultType faultType = ParseFaultType(faultDef.faultType);
                faultInjector.InjectFault(faultType, faultDef.affectedComponent, circuitSimulator);

                Debug.Log($"[TroubleshootingTask] Fault injected: {faultDef.faultType} at {faultDef.affectedComponent}");
            }
            else
            {
                // Default fault: loose connection at junction box 1
                faultInjector.InjectFault(FaultType.LooseConnection, "junction-box-1", circuitSimulator);
                Debug.Log("[TroubleshootingTask] Default fault injected: LooseConnection at junction-box-1");
            }
        }

        private FaultType ParseFaultType(string typeString)
        {
            switch (typeString)
            {
                case "loose-connection": return FaultType.LooseConnection;
                case "bad-splice": return FaultType.BadSplice;
                case "tripped-gfci": return FaultType.TrippedGFCI;
                case "tripped-breaker": return FaultType.TrippedBreaker;
                case "broken-wire": return FaultType.BrokenWire;
                case "overloaded-circuit": return FaultType.OverloadedCircuit;
                default:
                    Debug.LogWarning($"[TroubleshootingTask] Unknown fault type: {typeString}, using LooseConnection");
                    return FaultType.LooseConnection;
            }
        }

        private void SetupDialogueSystem()
        {
            dialogueTree = new DialogueTree();
            dialogueRunner = new DialogueRunner();

            if (taskDefinition != null && taskDefinition.dialogues != null && taskDefinition.dialogues.Length > 0)
            {
                dialogueTree.BuildFromDefinitions(taskDefinition.dialogues);
                dialogueRunner.Initialize(dialogueTree);
            }
            else
            {
                // Build default dialogue
                DialogueDefinition[] defaultDialogues = CreateDefaultDialogue();
                dialogueTree.BuildFromDefinitions(defaultDialogues);
                dialogueRunner.Initialize(dialogueTree);
            }
        }

        private DialogueDefinition[] CreateDefaultDialogue()
        {
            DialogueDefinition[] dialogues = new DialogueDefinition[3];

            dialogues[0] = new DialogueDefinition
            {
                id = "greeting",
                speakerName = "Customer",
                text = "Hi, thanks for coming! The outlet in the living room suddenly stopped working. I was using my vacuum when it just went dead.",
                choices = new DialogueChoice[]
                {
                    new DialogueChoice
                    {
                        choiceText = "When did this happen?",
                        nextDialogueId = "details",
                        diagnosticPoints = 2,
                        responseText = "It happened about an hour ago. I heard a little pop and then nothing."
                    },
                    new DialogueChoice
                    {
                        choiceText = "Did anything else stop working?",
                        nextDialogueId = "details",
                        diagnosticPoints = 3,
                        responseText = "Actually, yes! The outlet in the hallway seems dead too. But the kitchen is fine."
                    },
                    new DialogueChoice
                    {
                        choiceText = "Let me take a look.",
                        nextDialogueId = "details",
                        diagnosticPoints = 0,
                        responseText = "Sure, it's the outlet behind the couch."
                    }
                },
                nextDialogueId = "details"
            };

            dialogues[1] = new DialogueDefinition
            {
                id = "details",
                speakerName = "Customer",
                text = "Do you think it's something serious? Should I be worried about a fire?",
                choices = new DialogueChoice[]
                {
                    new DialogueChoice
                    {
                        choiceText = "Has the breaker tripped? Have you checked your panel?",
                        nextDialogueId = "end",
                        diagnosticPoints = 3,
                        responseText = "I checked the panel but I'm not sure what I'm looking at. Nothing looked obviously tripped."
                    },
                    new DialogueChoice
                    {
                        choiceText = "I'll check the circuit and let you know what I find.",
                        nextDialogueId = "end",
                        diagnosticPoints = 1,
                        responseText = "Okay, thank you. I'll stay out of your way."
                    }
                },
                nextDialogueId = "end"
            };

            dialogues[2] = new DialogueDefinition
            {
                id = "end",
                speakerName = "Customer",
                text = "Let me know when you find the problem. I'll be in the kitchen.",
                choices = null,
                nextDialogueId = ""
            };

            return dialogues;
        }

        private void SetupElectricalPanel()
        {
            if (electricalPanel == null)
            {
                GameObject panelObj = new GameObject("ElectricalPanel");
                panelObj.transform.SetParent(sceneRoot, false);
                panelObj.transform.localPosition = new Vector3(-2f, 1.0f, 0f);

                electricalPanel = panelObj.AddComponent<ElectricalPanel>();
                electricalPanel.AddBreaker(20, 0);
                electricalPanel.AddBreaker(20, 1);
                electricalPanel.AddBreaker(15, 2);
            }
        }

        private void SetupOutlets()
        {
            if (outlet1 == null)
            {
                GameObject o1 = new GameObject("Outlet_1_LivingRoom");
                o1.transform.SetParent(sceneRoot, false);
                o1.transform.localPosition = new Vector3(1f, 0.3f, 2f);
                outlet1 = o1.AddComponent<Outlet>();
            }

            if (outlet2 == null)
            {
                GameObject o2 = new GameObject("Outlet_2_Hallway");
                o2.transform.SetParent(sceneRoot, false);
                o2.transform.localPosition = new Vector3(0f, 0.3f, 2f);
                outlet2 = o2.AddComponent<Outlet>();
            }

            if (outlet3 == null)
            {
                GameObject o3 = new GameObject("Outlet_3_Bedroom");
                o3.transform.SetParent(sceneRoot, false);
                o3.transform.localPosition = new Vector3(-1f, 0.3f, 2f);
                outlet3 = o3.AddComponent<Outlet>();
            }

            // Add labels
            AddComponentLabel(outlet1.transform, "Living Room Outlet");
            AddComponentLabel(outlet2.transform, "Hallway Outlet");
            AddComponentLabel(outlet3.transform, "Bedroom Outlet");
        }

        private void SetupJunctionBoxes()
        {
            if (junctionBox1 == null)
            {
                GameObject jb1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jb1.name = "JunctionBox_1";
                jb1.transform.SetParent(sceneRoot, false);
                jb1.transform.localPosition = new Vector3(0.5f, 2.2f, 2f);
                jb1.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                Material jb1Mat = new Material(Shader.Find("Standard"));
                jb1Mat.color = new Color(0.2f, 0.4f, 0.6f, 1f);
                jb1.GetComponent<MeshRenderer>().material = jb1Mat;
                junctionBox1 = jb1.transform;
                AddComponentLabel(junctionBox1, "Junction Box 1");
            }

            if (junctionBox2 == null)
            {
                GameObject jb2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jb2.name = "JunctionBox_2";
                jb2.transform.SetParent(sceneRoot, false);
                jb2.transform.localPosition = new Vector3(-0.5f, 2.2f, 2f);
                jb2.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                Material jb2Mat = new Material(Shader.Find("Standard"));
                jb2Mat.color = new Color(0.2f, 0.4f, 0.6f, 1f);
                jb2.GetComponent<MeshRenderer>().material = jb2Mat;
                junctionBox2 = jb2.transform;
                AddComponentLabel(junctionBox2, "Junction Box 2");
            }
        }

        private void AddComponentLabel(Transform target, string text)
        {
            if (target == null) return;
            GameObject labelObj = new GameObject("ComponentLabel");
            labelObj.transform.SetParent(target, false);
            labelObj.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            FloatingLabel label = labelObj.AddComponent<FloatingLabel>();
            label.SetText(text);
            label.SetFontSize(1.5f);
        }

        private void SetupTools()
        {
            if (multimeter == null)
            {
                GameObject meterObj = new GameObject("Multimeter");
                meterObj.transform.SetParent(sceneRoot, false);
                meterObj.transform.localPosition = new Vector3(-1.5f, 0.85f, 0f);

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

            if (voltageTester == null)
            {
                GameObject testerObj = new GameObject("VoltageTester");
                testerObj.transform.SetParent(sceneRoot, false);
                testerObj.transform.localPosition = new Vector3(-1.4f, 0.85f, 0f);

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
        }

        private void SetupCustomerNPC()
        {
            if (customerNPC == null)
            {
                GameObject npcObj = new GameObject("CustomerNPC");
                npcObj.transform.SetParent(sceneRoot, false);
                npcObj.transform.localPosition = new Vector3(2f, 0f, 1f);

                // Simple capsule body
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "NPCBody";
                body.transform.SetParent(npcObj.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                body.transform.localScale = new Vector3(0.4f, 0.9f, 0.4f);

                Material bodyMat = new Material(Shader.Find("Standard"));
                bodyMat.color = new Color(0.3f, 0.5f, 0.7f, 1f); // Blue shirt
                customerRenderer = body.GetComponent<MeshRenderer>();
                customerRenderer.material = bodyMat;

                // Head
                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "NPCHead";
                head.transform.SetParent(npcObj.transform, false);
                head.transform.localPosition = new Vector3(0f, 1.65f, 0f);
                head.transform.localScale = Vector3.one * 0.25f;
                Material headMat = new Material(Shader.Find("Standard"));
                headMat.color = new Color(0.9f, 0.75f, 0.6f, 1f); // Skin tone
                head.GetComponent<MeshRenderer>().material = headMat;

                customerNPC = npcObj.transform;

                // Name label
                GameObject nameObj = new GameObject("CustomerNameLabel");
                nameObj.transform.SetParent(npcObj.transform, false);
                nameObj.transform.localPosition = new Vector3(0f, 2.0f, 0f);
                customerNameLabel = nameObj.AddComponent<FloatingLabel>();
                customerNameLabel.SetText("Mrs. Johnson");
                customerNameLabel.SetFontSize(2.0f);
                customerNameLabel.SetColor(Color.white);

                // Dialogue label
                GameObject dialogueObj = new GameObject("DialogueLabel");
                dialogueObj.transform.SetParent(npcObj.transform, false);
                dialogueObj.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                dialogueLabel = dialogueObj.AddComponent<FloatingLabel>();
                dialogueLabel.SetFontSize(1.5f);
                dialogueLabel.SetColor(new Color(0.9f, 0.9f, 0.7f, 1f));
                dialogueLabel.SetText("");
            }
        }

        private void SetupLabels()
        {
            if (stepInstructionLabel == null)
            {
                GameObject labelObj = new GameObject("StepInstructionLabel");
                labelObj.transform.SetParent(sceneRoot, false);
                labelObj.transform.localPosition = new Vector3(0f, 2.5f, 1f);
                stepInstructionLabel = labelObj.AddComponent<FloatingLabel>();
                stepInstructionLabel.SetFontSize(2.0f);
                stepInstructionLabel.SetText("");
            }

            if (voltageDisplay == null)
            {
                GameObject displayObj = new GameObject("VoltageDisplay");
                displayObj.transform.SetParent(sceneRoot, false);
                displayObj.transform.localPosition = new Vector3(-1.5f, 1.2f, 0f);
                voltageDisplay = displayObj.AddComponent<FloatingLabel>();
                voltageDisplay.SetFontSize(2.5f);
                voltageDisplay.SetColor(Color.green);
                voltageDisplay.SetText("---V");
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            // Check if player is near NPC for dialogue
            CheckNPCProximity();

            if (CurrentMode == TaskMode.Practice)
            {
                ShowCurrentStepInstruction();
            }
        }

        private void CheckNPCProximity()
        {
            if (customerNPC == null) return;

            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            float distance = Vector3.Distance(mainCam.transform.position, customerNPC.position);
            bool inRange = distance <= npcInteractionDistance;

            // Show dialogue when in range and dialogue is active
            if (inRange && dialogueActive && !dialogueRunner.IsComplete)
            {
                DialogueDefinition currentNode = dialogueRunner.GetCurrentNode();
                if (currentNode != null && dialogueLabel != null)
                {
                    string displayText = $"{currentNode.speakerName}: {currentNode.text}";
                    if (currentNode.choices != null && currentNode.choices.Length > 0)
                    {
                        displayText += "\n\n";
                        for (int i = 0; i < currentNode.choices.Length; i++)
                        {
                            displayText += $"[{i + 1}] {currentNode.choices[i].choiceText}\n";
                        }
                    }
                    dialogueLabel.SetText(displayText);
                }
            }
            else if (dialogueLabel != null && !inRange)
            {
                dialogueLabel.SetText("");
            }
        }

        // --- Mode Setup ---

        public override void StartLearnMode()
        {
            SetupMode(TaskMode.Learn, 0f);
            ResetTaskState();
            demoCoroutine = StartCoroutine(DemonstrationCoroutine());
            Debug.Log("[TroubleshootingTask] Learn mode started - demonstration beginning");
        }

        public override void StartPracticeMode()
        {
            SetupMode(TaskMode.Practice, 0f);
            ResetTaskState();
            ShowCurrentStepInstruction();
            Debug.Log("[TroubleshootingTask] Practice mode started");
        }

        public override void StartTestMode()
        {
            float timeLimitSeconds = taskDefinition != null ? taskDefinition.timeLimit : 420f;
            SetupMode(TaskMode.Test, timeLimitSeconds);
            ResetTaskState();
            Debug.Log($"[TroubleshootingTask] Test mode started - {timeLimitSeconds}s time limit");
        }

        private void ResetTaskState()
        {
            completedStepIds.Clear();
            complaintRead = false;
            diagnosticsAsked = false;
            panelChecked = false;
            panelVoltagesTested = false;
            outletVoltagesTested = false;
            circuitTraced = false;
            faultIdentified = false;
            faultRepaired = false;
            repairVerified = false;
            reportedToCustomer = false;
            dialogueActive = false;

            // Reset fault system
            faultInjector.ClearAll();
            faultDiagnostic = new FaultDiagnostic();

            // Re-setup simulator and fault
            SetupCircuitSimulator();
            SetupFaultSystem();
            SetupDialogueSystem();
        }

        // --- Demonstration (Learn Mode) ---

        private IEnumerator DemonstrationCoroutine()
        {
            if (taskDefinition == null || taskDefinition.steps == null)
            {
                Debug.LogError("[TroubleshootingTask] No steps defined for demonstration");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (hintPanel != null)
            {
                hintPanel.ShowHint("Watch the demonstration of circuit troubleshooting.\n" +
                    "A systematic approach is essential: interview, inspect, test, diagnose, repair, verify.", 6f);
            }
            yield return new WaitForSeconds(6f);

            for (int i = 0; i < taskDefinition.steps.Length; i++)
            {
                StepDefinition step = taskDefinition.steps[i];

                string explanation = $"Step {i + 1}/{taskDefinition.steps.Length}: {step.description}";

                if (!string.IsNullOrEmpty(step.hintText))
                {
                    explanation += $"\n\n{step.hintText}";
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

        // --- Dialogue Interaction ---

        /// <summary>
        /// Start a dialogue conversation with the customer NPC.
        /// </summary>
        public void StartDialogue()
        {
            dialogueActive = true;
            dialogueRunner.Initialize(dialogueTree);

            DialogueDefinition firstNode = dialogueRunner.GetCurrentNode();
            if (firstNode != null && dialogueLabel != null)
            {
                dialogueLabel.SetText($"{firstNode.speakerName}: {firstNode.text}");
            }

            Debug.Log("[TroubleshootingTask] Dialogue started with customer");
        }

        /// <summary>
        /// Select a dialogue choice.
        /// </summary>
        public string SelectDialogueChoice(int choiceIndex)
        {
            if (!dialogueActive) return null;

            string response = dialogueRunner.SelectChoice(choiceIndex);

            if (response != null && dialogueLabel != null)
            {
                dialogueLabel.SetText($"Customer: {response}");
            }

            if (dialogueRunner.IsComplete)
            {
                dialogueActive = false;
                diagnosticsAsked = true;
            }

            return response;
        }

        /// <summary>
        /// Advance the dialogue to the next node (for linear dialogue).
        /// </summary>
        public void AdvanceDialogue()
        {
            if (!dialogueActive) return;

            bool advanced = dialogueRunner.AdvanceToNext();
            if (!advanced)
            {
                dialogueActive = false;
            }
            else
            {
                DialogueDefinition currentNode = dialogueRunner.GetCurrentNode();
                if (currentNode != null && dialogueLabel != null)
                {
                    dialogueLabel.SetText($"{currentNode.speakerName}: {currentNode.text}");
                }
            }
        }

        // --- Voltage Testing ---

        /// <summary>
        /// Test voltage at a specific circuit node.
        /// </summary>
        public float TestVoltageAt(string nodeId)
        {
            float voltage = circuitSimulator.MeasureVoltage(nodeId);

            faultDiagnostic.RecordStep("measure-voltage", nodeId, $"{voltage:F1}V");

            if (voltageDisplay != null)
            {
                voltageDisplay.SetText($"{voltage:F1}V");
                voltageDisplay.SetColor(voltage > 100f ? Color.green : (voltage > 0f ? Color.yellow : Color.red));
            }

            Debug.Log($"[TroubleshootingTask] Voltage at '{nodeId}': {voltage:F1}V");
            return voltage;
        }

        /// <summary>
        /// Test continuity between two circuit nodes.
        /// </summary>
        public bool TestContinuity(string nodeA, string nodeB)
        {
            bool continuity = circuitSimulator.CheckContinuity(nodeA, nodeB);

            faultDiagnostic.RecordStep("check-continuity", $"{nodeA}-to-{nodeB}",
                continuity ? "CONTINUOUS" : "OPEN");

            if (voltageDisplay != null)
            {
                voltageDisplay.SetText(continuity ? "CONTINUITY" : "OPEN CIRCUIT");
                voltageDisplay.SetColor(continuity ? Color.green : Color.red);
            }

            Debug.Log($"[TroubleshootingTask] Continuity {nodeA} <-> {nodeB}: {(continuity ? "YES" : "NO")}");
            return continuity;
        }

        // --- Fault Identification ---

        /// <summary>
        /// Player attempts to identify the fault at a specific node.
        /// </summary>
        public bool AttemptFaultIdentification(string nodeId, FaultType faultType)
        {
            bool correct = faultDiagnostic.AttemptIdentification(nodeId, faultType, faultInjector);

            if (correct)
            {
                faultIdentified = true;
                AudioManager.Instance.PlayCorrectSound();

                if (CurrentMode == TaskMode.Practice && hintPanel != null)
                {
                    hintPanel.ShowHint($"Correct! The fault is a {faultType} at {nodeId}.", 3f);
                }
            }
            else
            {
                AudioManager.Instance.PlayIncorrectSound();

                if (CurrentMode == TaskMode.Practice && hintPanel != null)
                {
                    hintPanel.ShowHint("Incorrect identification. Continue testing to narrow down the fault.", 3f);
                }
            }

            return correct;
        }

        /// <summary>
        /// Player repairs the identified fault.
        /// </summary>
        public bool RepairFault(string nodeId)
        {
            if (!faultIdentified)
            {
                Debug.LogWarning("[TroubleshootingTask] Cannot repair - fault not yet identified");
                return false;
            }

            bool repaired = faultInjector.RepairFault(nodeId, circuitSimulator);

            if (repaired)
            {
                faultRepaired = true;
                AudioManager.Instance.PlayCorrectSound();

                if (CurrentMode == TaskMode.Practice && hintPanel != null)
                {
                    hintPanel.ShowHint("Repair complete! Now verify the fix by testing the circuit.", 3f);
                }
            }

            return repaired;
        }

        // --- Step Completion ---

        public void OnComplaintRead()
        {
            complaintRead = true;
            TryCompleteStep("read-complaint");
        }

        public void OnDiagnosticsAsked()
        {
            if (diagnosticsAsked || dialogueRunner.GetDiagnosticScore() > 0)
            {
                diagnosticsAsked = true;
                TryCompleteStep("ask-diagnostics");
            }
        }

        public void OnPanelChecked()
        {
            panelChecked = true;
            TryCompleteStep("check-panel");
        }

        public void OnPanelVoltageTested()
        {
            panelVoltagesTested = true;
            TryCompleteStep("test-panel-voltage");
        }

        public void OnOutletVoltageTested()
        {
            outletVoltagesTested = true;
            TryCompleteStep("test-outlet-voltage");
        }

        public void OnCircuitTraced()
        {
            circuitTraced = true;
            TryCompleteStep("trace-circuit");
        }

        public void OnFaultIdentified()
        {
            if (faultIdentified)
            {
                TryCompleteStep("identify-fault");
            }
        }

        public void OnFaultRepaired()
        {
            if (faultRepaired)
            {
                TryCompleteStep("repair-fault");
            }
        }

        public void OnRepairVerified()
        {
            repairVerified = true;
            TryCompleteStep("verify-repair");
        }

        public void OnReportedToCustomer()
        {
            reportedToCustomer = true;
            TryCompleteStep("report-to-customer");
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
                    Debug.Log("[TroubleshootingTask] All steps completed!");
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
            int maxDiagnosticPoints = dialogueRunner.MaxDiagnosticScore > 0
                ? dialogueRunner.MaxDiagnosticScore
                : 10;

            TaskResult result = ScoreManager.Instance.CalculateTroubleshootingScore(
                diagnosticPoints: dialogueRunner.GetDiagnosticScore(),
                maxDiagnosticPoints: maxDiagnosticPoints,
                faultIdentified: faultIdentified,
                faultRepaired: faultRepaired,
                completedStepIds: TaskManager.Instance.GetCompletedStepIds(),
                allSteps: taskDefinition != null ? taskDefinition.steps : null,
                timeUsed: elapsedTime,
                timeLimit: timeLimit,
                mode: CurrentMode
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
