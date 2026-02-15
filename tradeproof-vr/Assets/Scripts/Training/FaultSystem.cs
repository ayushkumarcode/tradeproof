using UnityEngine;
using System.Collections.Generic;

namespace TradeProof.Training
{
    /// <summary>
    /// Fault types that can be injected into a circuit for troubleshooting training.
    /// </summary>
    public enum FaultType
    {
        LooseConnection,
        BadSplice,
        TrippedGFCI,
        TrippedBreaker,
        BrokenWire,
        OverloadedCircuit
    }

    /// <summary>
    /// Injects faults into circuit components for troubleshooting exercises.
    /// Works with CircuitSimulator to affect energized states.
    /// </summary>
    public class FaultInjector
    {
        private List<InjectedFault> activeFaults = new List<InjectedFault>();

        public List<InjectedFault> ActiveFaults => activeFaults;

        /// <summary>
        /// Inject a fault into the circuit at the specified node.
        /// </summary>
        public InjectedFault InjectFault(FaultType faultType, string targetNodeId, CircuitSimulator simulator)
        {
            InjectedFault fault = new InjectedFault
            {
                faultType = faultType,
                targetNodeId = targetNodeId,
                isActive = true,
                isIdentified = false,
                isRepaired = false
            };

            activeFaults.Add(fault);

            if (simulator != null)
            {
                ApplyFaultToSimulator(fault, simulator);
            }

            Debug.Log($"[FaultInjector] Injected {faultType} at node '{targetNodeId}'");
            return fault;
        }

        private void ApplyFaultToSimulator(InjectedFault fault, CircuitSimulator simulator)
        {
            switch (fault.faultType)
            {
                case FaultType.LooseConnection:
                    simulator.SetNodeState(fault.targetNodeId, CircuitNodeState.Intermittent);
                    break;

                case FaultType.BadSplice:
                    simulator.SetNodeState(fault.targetNodeId, CircuitNodeState.HighResistance);
                    break;

                case FaultType.TrippedGFCI:
                    simulator.SetNodeState(fault.targetNodeId, CircuitNodeState.Open);
                    break;

                case FaultType.TrippedBreaker:
                    simulator.SetNodeState(fault.targetNodeId, CircuitNodeState.Open);
                    break;

                case FaultType.BrokenWire:
                    simulator.SetNodeState(fault.targetNodeId, CircuitNodeState.Open);
                    break;

                case FaultType.OverloadedCircuit:
                    simulator.SetNodeState(fault.targetNodeId, CircuitNodeState.Overloaded);
                    break;
            }
        }

        /// <summary>
        /// Mark a fault as identified by the player.
        /// </summary>
        public bool IdentifyFault(string targetNodeId, FaultType guessedType)
        {
            foreach (var fault in activeFaults)
            {
                if (fault.targetNodeId == targetNodeId && fault.faultType == guessedType && fault.isActive)
                {
                    fault.isIdentified = true;
                    Debug.Log($"[FaultInjector] Fault correctly identified: {guessedType} at '{targetNodeId}'");
                    return true;
                }
            }

            Debug.Log($"[FaultInjector] Incorrect fault identification: {guessedType} at '{targetNodeId}'");
            return false;
        }

        /// <summary>
        /// Mark a fault as repaired by the player.
        /// </summary>
        public bool RepairFault(string targetNodeId, CircuitSimulator simulator)
        {
            foreach (var fault in activeFaults)
            {
                if (fault.targetNodeId == targetNodeId && fault.isIdentified && !fault.isRepaired)
                {
                    fault.isRepaired = true;
                    fault.isActive = false;

                    if (simulator != null)
                    {
                        simulator.SetNodeState(fault.targetNodeId, CircuitNodeState.Energized);
                    }

                    Debug.Log($"[FaultInjector] Fault repaired at '{targetNodeId}'");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if all faults have been identified.
        /// </summary>
        public bool AllFaultsIdentified()
        {
            foreach (var fault in activeFaults)
            {
                if (!fault.isIdentified) return false;
            }
            return activeFaults.Count > 0;
        }

        /// <summary>
        /// Check if all faults have been repaired.
        /// </summary>
        public bool AllFaultsRepaired()
        {
            foreach (var fault in activeFaults)
            {
                if (!fault.isRepaired) return false;
            }
            return activeFaults.Count > 0;
        }

        public void ClearAll()
        {
            activeFaults.Clear();
        }
    }

    /// <summary>
    /// Represents a single injected fault in the circuit.
    /// </summary>
    public class InjectedFault
    {
        public FaultType faultType;
        public string targetNodeId;
        public bool isActive;
        public bool isIdentified;
        public bool isRepaired;
    }

    /// <summary>
    /// Tracks player's diagnostic progress for fault identification.
    /// </summary>
    public class FaultDiagnostic
    {
        private List<DiagnosticStep> steps = new List<DiagnosticStep>();
        private bool faultCorrectlyIdentified;
        private string identifiedFaultNode;
        private FaultType identifiedFaultType;

        public bool FaultCorrectlyIdentified => faultCorrectlyIdentified;
        public string IdentifiedFaultNode => identifiedFaultNode;
        public FaultType IdentifiedFaultType => identifiedFaultType;
        public int StepCount => steps.Count;

        /// <summary>
        /// Record a diagnostic step the player performed.
        /// </summary>
        public void RecordStep(string action, string targetNode, string reading)
        {
            steps.Add(new DiagnosticStep
            {
                action = action,
                targetNode = targetNode,
                reading = reading,
                timestamp = Time.time
            });

            Debug.Log($"[FaultDiagnostic] Step recorded: {action} at '{targetNode}' = {reading}");
        }

        /// <summary>
        /// Player attempts to identify the fault.
        /// </summary>
        public bool AttemptIdentification(string nodeId, FaultType type, FaultInjector injector)
        {
            bool correct = injector.IdentifyFault(nodeId, type);
            if (correct)
            {
                faultCorrectlyIdentified = true;
                identifiedFaultNode = nodeId;
                identifiedFaultType = type;
            }
            return correct;
        }

        /// <summary>
        /// Get a summary of the diagnostic process.
        /// </summary>
        public string GetDiagnosticSummary()
        {
            string summary = $"Diagnostic Steps: {steps.Count}\n";
            summary += $"Fault Identified: {(faultCorrectlyIdentified ? "Yes" : "No")}\n";
            if (faultCorrectlyIdentified)
            {
                summary += $"Fault: {identifiedFaultType} at {identifiedFaultNode}\n";
            }
            return summary;
        }
    }

    /// <summary>
    /// A single diagnostic step recorded during troubleshooting.
    /// </summary>
    public class DiagnosticStep
    {
        public string action;     // "measure-voltage", "check-continuity", "visual-inspect"
        public string targetNode;
        public string reading;
        public float timestamp;
    }

    /// <summary>
    /// Node states in the circuit simulator.
    /// </summary>
    public enum CircuitNodeState
    {
        Energized,
        DeEnergized,
        Open,           // No continuity (broken wire, tripped breaker)
        HighResistance, // Bad splice, corroded connection
        Intermittent,   // Loose connection
        Overloaded      // Drawing too much current
    }

    /// <summary>
    /// Simple circuit simulator for troubleshooting tasks.
    /// Tracks node states and allows voltage/continuity queries.
    /// </summary>
    public class CircuitSimulator
    {
        private Dictionary<string, CircuitNodeState> nodeStates = new Dictionary<string, CircuitNodeState>();
        private Dictionary<string, float> nodeVoltages = new Dictionary<string, float>();
        private Dictionary<string, List<string>> connections = new Dictionary<string, List<string>>();
        private float sourceVoltage = 120f;

        public float SourceVoltage => sourceVoltage;

        /// <summary>
        /// Add a circuit node.
        /// </summary>
        public void AddNode(string nodeId, CircuitNodeState initialState = CircuitNodeState.Energized)
        {
            nodeStates[nodeId] = initialState;
            RecalculateVoltages();
        }

        /// <summary>
        /// Connect two nodes together.
        /// </summary>
        public void ConnectNodes(string nodeA, string nodeB)
        {
            if (!connections.ContainsKey(nodeA))
                connections[nodeA] = new List<string>();
            if (!connections.ContainsKey(nodeB))
                connections[nodeB] = new List<string>();

            if (!connections[nodeA].Contains(nodeB))
                connections[nodeA].Add(nodeB);
            if (!connections[nodeB].Contains(nodeA))
                connections[nodeB].Add(nodeA);

            RecalculateVoltages();
        }

        /// <summary>
        /// Set the state of a circuit node.
        /// </summary>
        public void SetNodeState(string nodeId, CircuitNodeState state)
        {
            nodeStates[nodeId] = state;
            RecalculateVoltages();
        }

        /// <summary>
        /// Get the state of a circuit node.
        /// </summary>
        public CircuitNodeState GetNodeState(string nodeId)
        {
            if (nodeStates.TryGetValue(nodeId, out CircuitNodeState state))
                return state;
            return CircuitNodeState.DeEnergized;
        }

        /// <summary>
        /// Measure voltage at a node (what a voltage tester would read).
        /// </summary>
        public float MeasureVoltage(string nodeId)
        {
            if (nodeVoltages.TryGetValue(nodeId, out float voltage))
                return voltage;
            return 0f;
        }

        /// <summary>
        /// Check continuity between two nodes (what a multimeter would read).
        /// </summary>
        public bool CheckContinuity(string nodeA, string nodeB)
        {
            if (!nodeStates.ContainsKey(nodeA) || !nodeStates.ContainsKey(nodeB))
                return false;

            // Check if there is a path between nodeA and nodeB without open nodes
            HashSet<string> visited = new HashSet<string>();
            return TraversePath(nodeA, nodeB, visited);
        }

        private bool TraversePath(string current, string target, HashSet<string> visited)
        {
            if (current == target) return true;
            if (visited.Contains(current)) return false;

            visited.Add(current);

            // An open node blocks continuity
            CircuitNodeState state = GetNodeState(current);
            if (state == CircuitNodeState.Open && current != target)
                return false;

            if (connections.TryGetValue(current, out List<string> neighbors))
            {
                foreach (string neighbor in neighbors)
                {
                    if (TraversePath(neighbor, target, visited))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Recalculate voltages based on node states and connections.
        /// </summary>
        private void RecalculateVoltages()
        {
            nodeVoltages.Clear();

            foreach (var kvp in nodeStates)
            {
                string nodeId = kvp.Key;
                CircuitNodeState state = kvp.Value;

                switch (state)
                {
                    case CircuitNodeState.Energized:
                        nodeVoltages[nodeId] = sourceVoltage;
                        break;
                    case CircuitNodeState.DeEnergized:
                    case CircuitNodeState.Open:
                        nodeVoltages[nodeId] = 0f;
                        break;
                    case CircuitNodeState.HighResistance:
                        nodeVoltages[nodeId] = sourceVoltage * 0.7f; // Voltage drop
                        break;
                    case CircuitNodeState.Intermittent:
                        // Fluctuating voltage
                        nodeVoltages[nodeId] = sourceVoltage * Random.Range(0.3f, 1.0f);
                        break;
                    case CircuitNodeState.Overloaded:
                        nodeVoltages[nodeId] = sourceVoltage * 0.85f; // Slight sag
                        break;
                }
            }
        }

        /// <summary>
        /// Check if a node is receiving power.
        /// </summary>
        public bool IsNodeEnergized(string nodeId)
        {
            CircuitNodeState state = GetNodeState(nodeId);
            return state == CircuitNodeState.Energized ||
                   state == CircuitNodeState.HighResistance ||
                   state == CircuitNodeState.Overloaded;
        }
    }
}
