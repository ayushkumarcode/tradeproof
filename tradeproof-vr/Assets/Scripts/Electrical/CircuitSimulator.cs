using UnityEngine;
using System.Collections.Generic;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents a single node in the circuit graph.
    /// Maps to a physical electrical component in the Unity scene.
    /// </summary>
    public class CircuitNode
    {
        public string nodeId;
        public string nodeType; // "panel", "breaker", "outlet", "switch", "junction", "fixture", "gfci"
        public bool isEnergized;
        public GameObject associatedObject;

        public CircuitNode(string id, string type, GameObject obj)
        {
            nodeId = id;
            nodeType = type;
            isEnergized = false;
            associatedObject = obj;
        }
    }

    /// <summary>
    /// Represents a connection (edge) between two circuit nodes.
    /// Each edge has a wire type (hot, neutral, ground) and a connected state.
    /// </summary>
    public class CircuitEdge
    {
        public CircuitNode from;
        public CircuitNode to;
        public string wireType; // "hot", "neutral", "ground"
        public bool isConnected;

        public CircuitEdge(CircuitNode from, CircuitNode to, string wireType)
        {
            this.from = from;
            this.to = to;
            this.wireType = wireType;
            this.isConnected = true;
        }
    }

    /// <summary>
    /// Graph-based circuit simulation engine. NOT a MonoBehaviour.
    /// Uses a graph of CircuitNodes and CircuitEdges to track energy flow.
    /// Propagates energy via BFS from the panel node through connected edges.
    /// Supports breaker state, switch state, GFCI tripped state, and junction pass-through.
    /// Provides voltage queries, energization checks, and continuity testing for the Multimeter.
    /// </summary>
    public class CircuitSimulator
    {
        private List<CircuitNode> nodes;
        private List<CircuitEdge> edges;
        private Dictionary<string, CircuitNode> nodeMap;

        public List<CircuitNode> Nodes => new List<CircuitNode>(nodes);
        public List<CircuitEdge> Edges => new List<CircuitEdge>(edges);
        public int NodeCount => nodes.Count;
        public int EdgeCount => edges.Count;

        public CircuitSimulator()
        {
            nodes = new List<CircuitNode>();
            edges = new List<CircuitEdge>();
            nodeMap = new Dictionary<string, CircuitNode>();
        }

        // --- Node Management ---

        /// <summary>
        /// Creates and adds a new node to the circuit graph.
        /// </summary>
        /// <param name="id">Unique identifier for the node.</param>
        /// <param name="type">Node type: "panel", "breaker", "outlet", "switch", "junction", "fixture", "gfci".</param>
        /// <param name="obj">The Unity GameObject associated with this node.</param>
        /// <returns>The created CircuitNode, or null if a node with that ID already exists.</returns>
        public CircuitNode AddNode(string id, string type, GameObject obj)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[CircuitSimulator] Cannot add node with null or empty ID.");
                return null;
            }

            if (nodeMap.ContainsKey(id))
            {
                Debug.LogWarning($"[CircuitSimulator] Node '{id}' already exists.");
                return nodeMap[id];
            }

            CircuitNode node = new CircuitNode(id, type, obj);
            nodes.Add(node);
            nodeMap[id] = node;

            Debug.Log($"[CircuitSimulator] Added node: {id} (type: {type})");
            return node;
        }

        /// <summary>
        /// Removes a node and all its connected edges from the graph.
        /// </summary>
        public bool RemoveNode(string id)
        {
            if (!nodeMap.TryGetValue(id, out CircuitNode node))
            {
                Debug.LogWarning($"[CircuitSimulator] Cannot remove node '{id}' — not found.");
                return false;
            }

            // Remove all edges connected to this node
            edges.RemoveAll(e => e.from == node || e.to == node);

            nodes.Remove(node);
            nodeMap.Remove(id);

            Debug.Log($"[CircuitSimulator] Removed node: {id}");
            return true;
        }

        /// <summary>
        /// Gets a node by its ID.
        /// </summary>
        public CircuitNode GetNode(string id)
        {
            if (nodeMap.TryGetValue(id, out CircuitNode node))
            {
                return node;
            }
            return null;
        }

        // --- Edge Management ---

        /// <summary>
        /// Creates and adds an edge between two nodes.
        /// </summary>
        /// <param name="fromId">Source node ID.</param>
        /// <param name="toId">Destination node ID.</param>
        /// <param name="wireType">Wire type: "hot", "neutral", "ground".</param>
        /// <returns>The created CircuitEdge, or null if either node was not found.</returns>
        public CircuitEdge AddEdge(string fromId, string toId, string wireType)
        {
            if (!nodeMap.TryGetValue(fromId, out CircuitNode fromNode))
            {
                Debug.LogWarning($"[CircuitSimulator] Cannot add edge — source node '{fromId}' not found.");
                return null;
            }

            if (!nodeMap.TryGetValue(toId, out CircuitNode toNode))
            {
                Debug.LogWarning($"[CircuitSimulator] Cannot add edge — destination node '{toId}' not found.");
                return null;
            }

            // Check for duplicate edge
            foreach (CircuitEdge existing in edges)
            {
                if (existing.from == fromNode && existing.to == toNode && existing.wireType == wireType)
                {
                    Debug.LogWarning($"[CircuitSimulator] Edge from '{fromId}' to '{toId}' ({wireType}) already exists.");
                    return existing;
                }
            }

            CircuitEdge edge = new CircuitEdge(fromNode, toNode, wireType);
            edges.Add(edge);

            Debug.Log($"[CircuitSimulator] Added edge: {fromId} -> {toId} ({wireType})");
            return edge;
        }

        /// <summary>
        /// Removes an edge between two nodes.
        /// </summary>
        public bool RemoveEdge(string fromId, string toId)
        {
            if (!nodeMap.TryGetValue(fromId, out CircuitNode fromNode) ||
                !nodeMap.TryGetValue(toId, out CircuitNode toNode))
            {
                Debug.LogWarning($"[CircuitSimulator] Cannot remove edge — node(s) not found.");
                return false;
            }

            int removed = edges.RemoveAll(e => e.from == fromNode && e.to == toNode);

            if (removed > 0)
            {
                Debug.Log($"[CircuitSimulator] Removed {removed} edge(s): {fromId} -> {toId}");
                return true;
            }

            Debug.LogWarning($"[CircuitSimulator] No edge found from '{fromId}' to '{toId}'.");
            return false;
        }

        // --- Energy Propagation ---

        /// <summary>
        /// Propagates energy through the circuit graph using BFS starting from panel nodes.
        /// Rules:
        /// - Panel: always energized (source).
        /// - Breaker: passes energy only if state == On.
        /// - Switch: passes energy only if isOn == true (checks LightSwitch component).
        /// - GFCI: passes energy only if NOT tripped.
        /// - Junction: always passes (splice point).
        /// - Outlet/Fixture: terminal nodes, just marked as energized.
        /// </summary>
        public void PropagateEnergy()
        {
            // Reset all nodes to de-energized
            foreach (CircuitNode node in nodes)
            {
                node.isEnergized = false;
            }

            // Find all panel nodes (energy sources)
            Queue<CircuitNode> bfsQueue = new Queue<CircuitNode>();
            HashSet<string> visited = new HashSet<string>();

            foreach (CircuitNode node in nodes)
            {
                if (node.nodeType == "panel")
                {
                    node.isEnergized = true;
                    bfsQueue.Enqueue(node);
                    visited.Add(node.nodeId);
                }
            }

            // BFS traversal
            while (bfsQueue.Count > 0)
            {
                CircuitNode current = bfsQueue.Dequeue();

                // Find all edges from the current node
                foreach (CircuitEdge edge in edges)
                {
                    CircuitNode neighbor = null;

                    // Check both directions (edges are directional but we treat as bidirectional for energy flow)
                    if (edge.from == current && !visited.Contains(edge.to.nodeId))
                    {
                        neighbor = edge.to;
                    }
                    else if (edge.to == current && !visited.Contains(edge.from.nodeId))
                    {
                        neighbor = edge.from;
                    }

                    if (neighbor == null) continue;
                    if (!edge.isConnected) continue;

                    // Check if energy can pass through the neighbor
                    bool canPass = CanEnergyPass(neighbor);

                    if (canPass)
                    {
                        neighbor.isEnergized = true;
                        visited.Add(neighbor.nodeId);
                        bfsQueue.Enqueue(neighbor);
                    }
                }
            }

            Debug.Log($"[CircuitSimulator] Energy propagation complete. {GetEnergizedCount()}/{nodes.Count} nodes energized.");
        }

        /// <summary>
        /// Determines whether energy can pass through a given node based on its type and state.
        /// </summary>
        private bool CanEnergyPass(CircuitNode node)
        {
            switch (node.nodeType)
            {
                case "panel":
                    // Panel is always a source
                    return true;

                case "breaker":
                    // Breaker passes only if state == On
                    if (node.associatedObject != null)
                    {
                        CircuitBreaker breaker = node.associatedObject.GetComponent<CircuitBreaker>();
                        if (breaker != null)
                        {
                            return breaker.State == CircuitBreaker.BreakerState.On;
                        }
                    }
                    // If no component found, assume passes (allows for abstract simulation)
                    return true;

                case "switch":
                    // Switch passes only if isOn
                    if (node.associatedObject != null)
                    {
                        // Check for a component with an IsOn property
                        // Using GetComponent with interface or checking for MonoBehaviour with reflection
                        // For simplicity, check for a "LightSwitch" component by name
                        MonoBehaviour[] behaviours = node.associatedObject.GetComponents<MonoBehaviour>();
                        foreach (MonoBehaviour mb in behaviours)
                        {
                            System.Type type = mb.GetType();
                            System.Reflection.PropertyInfo isOnProp = type.GetProperty("IsOn");
                            if (isOnProp != null && isOnProp.PropertyType == typeof(bool))
                            {
                                return (bool)isOnProp.GetValue(mb);
                            }

                            // Also check for a field named isOn
                            System.Reflection.FieldInfo isOnField = type.GetField("isOn",
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Instance);
                            if (isOnField != null && isOnField.FieldType == typeof(bool))
                            {
                                return (bool)isOnField.GetValue(mb);
                            }
                        }
                    }
                    // Default: assume switch is on if no component found
                    return true;

                case "gfci":
                    // GFCI passes only if NOT tripped
                    if (node.associatedObject != null)
                    {
                        GFCIOutlet gfci = node.associatedObject.GetComponent<GFCIOutlet>();
                        if (gfci != null)
                        {
                            return !gfci.IsTripped;
                        }
                    }
                    return true;

                case "junction":
                    // Junction always passes (splice point)
                    return true;

                case "outlet":
                case "fixture":
                    // Terminal nodes — always accept energy
                    return true;

                default:
                    // Unknown type — allow passage
                    return true;
            }
        }

        // --- Queries ---

        /// <summary>
        /// Returns the voltage at a node. 120V if energized, 0V if not.
        /// </summary>
        public float GetVoltage(string nodeId)
        {
            if (nodeMap.TryGetValue(nodeId, out CircuitNode node))
            {
                return node.isEnergized ? 120f : 0f;
            }

            Debug.LogWarning($"[CircuitSimulator] Node '{nodeId}' not found for voltage query.");
            return 0f;
        }

        /// <summary>
        /// Returns whether a node is currently energized.
        /// </summary>
        public bool IsEnergized(string nodeId)
        {
            if (nodeMap.TryGetValue(nodeId, out CircuitNode node))
            {
                return node.isEnergized;
            }

            Debug.LogWarning($"[CircuitSimulator] Node '{nodeId}' not found for energization query.");
            return false;
        }

        /// <summary>
        /// Checks if a continuous path exists between two nodes (regardless of energy state).
        /// Used by the Multimeter in continuity mode. Performs BFS ignoring breaker/switch states
        /// but respecting edge connection state.
        /// </summary>
        public bool HasContinuity(string fromId, string toId)
        {
            if (!nodeMap.TryGetValue(fromId, out CircuitNode fromNode))
            {
                Debug.LogWarning($"[CircuitSimulator] Node '{fromId}' not found for continuity check.");
                return false;
            }

            if (!nodeMap.TryGetValue(toId, out CircuitNode toNode))
            {
                Debug.LogWarning($"[CircuitSimulator] Node '{toId}' not found for continuity check.");
                return false;
            }

            if (fromNode == toNode) return true;

            // BFS to find any path between the two nodes through connected edges
            Queue<CircuitNode> bfsQueue = new Queue<CircuitNode>();
            HashSet<string> visited = new HashSet<string>();

            bfsQueue.Enqueue(fromNode);
            visited.Add(fromNode.nodeId);

            while (bfsQueue.Count > 0)
            {
                CircuitNode current = bfsQueue.Dequeue();

                foreach (CircuitEdge edge in edges)
                {
                    if (!edge.isConnected) continue;

                    CircuitNode neighbor = null;

                    if (edge.from == current && !visited.Contains(edge.to.nodeId))
                    {
                        neighbor = edge.to;
                    }
                    else if (edge.to == current && !visited.Contains(edge.from.nodeId))
                    {
                        neighbor = edge.from;
                    }

                    if (neighbor == null) continue;

                    if (neighbor == toNode)
                    {
                        return true; // Path found
                    }

                    visited.Add(neighbor.nodeId);
                    bfsQueue.Enqueue(neighbor);
                }
            }

            return false; // No path found
        }

        // --- Utility ---

        /// <summary>
        /// Removes all nodes and edges from the circuit graph.
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
            nodeMap.Clear();

            Debug.Log("[CircuitSimulator] Circuit graph cleared.");
        }

        /// <summary>
        /// Returns the number of currently energized nodes.
        /// </summary>
        public int GetEnergizedCount()
        {
            int count = 0;
            foreach (CircuitNode node in nodes)
            {
                if (node.isEnergized) count++;
            }
            return count;
        }

        /// <summary>
        /// Gets all edges connected to a given node (both incoming and outgoing).
        /// </summary>
        public List<CircuitEdge> GetEdgesForNode(string nodeId)
        {
            List<CircuitEdge> result = new List<CircuitEdge>();

            if (!nodeMap.TryGetValue(nodeId, out CircuitNode node))
            {
                return result;
            }

            foreach (CircuitEdge edge in edges)
            {
                if (edge.from == node || edge.to == node)
                {
                    result.Add(edge);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all nodes of a specific type.
        /// </summary>
        public List<CircuitNode> GetNodesByType(string type)
        {
            List<CircuitNode> result = new List<CircuitNode>();

            foreach (CircuitNode node in nodes)
            {
                if (node.nodeType == type)
                {
                    result.Add(node);
                }
            }

            return result;
        }

        /// <summary>
        /// Disconnects an edge (sets isConnected to false) without removing it from the graph.
        /// Useful for simulating wire disconnection.
        /// </summary>
        public void DisconnectEdge(string fromId, string toId)
        {
            if (!nodeMap.TryGetValue(fromId, out CircuitNode fromNode) ||
                !nodeMap.TryGetValue(toId, out CircuitNode toNode))
            {
                return;
            }

            foreach (CircuitEdge edge in edges)
            {
                if ((edge.from == fromNode && edge.to == toNode) ||
                    (edge.to == fromNode && edge.from == toNode))
                {
                    edge.isConnected = false;
                    Debug.Log($"[CircuitSimulator] Edge disconnected: {fromId} <-> {toId}");
                }
            }
        }

        /// <summary>
        /// Reconnects a previously disconnected edge.
        /// </summary>
        public void ReconnectEdge(string fromId, string toId)
        {
            if (!nodeMap.TryGetValue(fromId, out CircuitNode fromNode) ||
                !nodeMap.TryGetValue(toId, out CircuitNode toNode))
            {
                return;
            }

            foreach (CircuitEdge edge in edges)
            {
                if ((edge.from == fromNode && edge.to == toNode) ||
                    (edge.to == fromNode && edge.from == toNode))
                {
                    edge.isConnected = true;
                    Debug.Log($"[CircuitSimulator] Edge reconnected: {fromId} <-> {toId}");
                }
            }
        }

        /// <summary>
        /// Returns a debug summary of the circuit graph state.
        /// </summary>
        public string GetDebugSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Circuit Simulator: {nodes.Count} nodes, {edges.Count} edges ===");

            sb.AppendLine("\nNodes:");
            foreach (CircuitNode node in nodes)
            {
                string objName = node.associatedObject != null ? node.associatedObject.name : "null";
                sb.AppendLine($"  [{node.nodeId}] type={node.nodeType}, energized={node.isEnergized}, obj={objName}");
            }

            sb.AppendLine("\nEdges:");
            foreach (CircuitEdge edge in edges)
            {
                sb.AppendLine($"  {edge.from.nodeId} -> {edge.to.nodeId} ({edge.wireType}) connected={edge.isConnected}");
            }

            return sb.ToString();
        }
    }
}
