using UnityEngine;
using System.Collections.Generic;
using TradeProof.Data;

namespace TradeProof.Training
{
    /// <summary>
    /// Dialogue tree that manages branching customer conversations
    /// for troubleshooting tasks. Uses DialogueDefinition nodes from TaskDefinition.
    /// </summary>
    public class DialogueTree
    {
        private Dictionary<string, DialogueDefinition> nodes = new Dictionary<string, DialogueDefinition>();
        private string rootNodeId;

        public int NodeCount => nodes.Count;

        /// <summary>
        /// Build the tree from an array of DialogueDefinition nodes.
        /// The first node is treated as the root.
        /// </summary>
        public void BuildFromDefinitions(DialogueDefinition[] definitions)
        {
            nodes.Clear();
            if (definitions == null || definitions.Length == 0) return;

            foreach (var def in definitions)
            {
                if (def != null && !string.IsNullOrEmpty(def.id))
                {
                    nodes[def.id] = def;
                }
            }

            rootNodeId = definitions[0].id;
            Debug.Log($"[DialogueTree] Built tree with {nodes.Count} nodes, root: '{rootNodeId}'");
        }

        /// <summary>
        /// Get a dialogue node by ID.
        /// </summary>
        public DialogueDefinition GetNode(string nodeId)
        {
            if (nodes.TryGetValue(nodeId, out DialogueDefinition node))
                return node;
            return null;
        }

        /// <summary>
        /// Get the root node ID.
        /// </summary>
        public string GetRootNodeId()
        {
            return rootNodeId;
        }

        /// <summary>
        /// Check if a node exists.
        /// </summary>
        public bool HasNode(string nodeId)
        {
            return nodes.ContainsKey(nodeId);
        }
    }

    /// <summary>
    /// Runs a dialogue conversation, tracking current node, player choices,
    /// and accumulated diagnostic points.
    /// </summary>
    public class DialogueRunner
    {
        private DialogueTree tree;
        private string currentNodeId;
        private int diagnosticScore;
        private int maxDiagnosticScore;
        private List<string> visitedNodes = new List<string>();
        private List<DialogueChoice> selectedChoices = new List<DialogueChoice>();
        private bool isComplete;

        public bool IsComplete => isComplete;
        public int DiagnosticScore => diagnosticScore;
        public int MaxDiagnosticScore => maxDiagnosticScore;
        public string CurrentNodeId => currentNodeId;

        /// <summary>
        /// Initialize the runner with a dialogue tree.
        /// </summary>
        public void Initialize(DialogueTree dialogueTree)
        {
            tree = dialogueTree;
            diagnosticScore = 0;
            maxDiagnosticScore = 0;
            visitedNodes.Clear();
            selectedChoices.Clear();
            isComplete = false;

            // Calculate max diagnostic score
            CalculateMaxDiagnosticScore();

            // Start at root
            currentNodeId = tree.GetRootNodeId();
            if (!string.IsNullOrEmpty(currentNodeId))
            {
                visitedNodes.Add(currentNodeId);
            }

            Debug.Log($"[DialogueRunner] Initialized with max diagnostic score: {maxDiagnosticScore}");
        }

        private void CalculateMaxDiagnosticScore()
        {
            // Find the maximum possible diagnostic points by summing the best choice per node
            // This is a simplification; a full tree traversal would be more accurate
            maxDiagnosticScore = 0;

            // Just sum all positive diagnostic points from all choices across all nodes
            // The actual max depends on the path, but this gives a reasonable upper bound
            HashSet<string> counted = new HashSet<string>();
            CalculateMaxRecursive(tree.GetRootNodeId(), counted);
        }

        private void CalculateMaxRecursive(string nodeId, HashSet<string> counted)
        {
            if (string.IsNullOrEmpty(nodeId) || counted.Contains(nodeId)) return;
            counted.Add(nodeId);

            DialogueDefinition node = tree.GetNode(nodeId);
            if (node == null) return;

            // Find the best choice in this node
            if (node.choices != null && node.choices.Length > 0)
            {
                int bestPoints = 0;
                string bestNextId = null;

                foreach (var choice in node.choices)
                {
                    if (choice.diagnosticPoints > bestPoints)
                    {
                        bestPoints = choice.diagnosticPoints;
                    }
                    // Follow all paths to find total max
                    if (!string.IsNullOrEmpty(choice.nextDialogueId))
                    {
                        CalculateMaxRecursive(choice.nextDialogueId, counted);
                    }
                }
                maxDiagnosticScore += bestPoints;
            }

            // Follow linear next node
            if (!string.IsNullOrEmpty(node.nextDialogueId))
            {
                CalculateMaxRecursive(node.nextDialogueId, counted);
            }
        }

        /// <summary>
        /// Get the current dialogue node.
        /// </summary>
        public DialogueDefinition GetCurrentNode()
        {
            if (isComplete || string.IsNullOrEmpty(currentNodeId))
                return null;
            return tree.GetNode(currentNodeId);
        }

        /// <summary>
        /// Select a choice from the current node.
        /// Returns the NPC's response text, or null if invalid.
        /// </summary>
        public string SelectChoice(int choiceIndex)
        {
            DialogueDefinition currentNode = GetCurrentNode();
            if (currentNode == null) return null;

            if (currentNode.choices == null || choiceIndex < 0 || choiceIndex >= currentNode.choices.Length)
            {
                Debug.LogWarning($"[DialogueRunner] Invalid choice index: {choiceIndex}");
                return null;
            }

            DialogueChoice choice = currentNode.choices[choiceIndex];
            selectedChoices.Add(choice);

            // Add diagnostic points
            diagnosticScore += choice.diagnosticPoints;

            Debug.Log($"[DialogueRunner] Selected choice: '{choice.choiceText}' " +
                      $"(+{choice.diagnosticPoints} diagnostic points, total: {diagnosticScore})");

            // Navigate to next node
            string nextId = choice.nextDialogueId;
            if (string.IsNullOrEmpty(nextId))
            {
                // If no explicit next, check node's default next
                nextId = currentNode.nextDialogueId;
            }

            if (!string.IsNullOrEmpty(nextId) && tree.HasNode(nextId))
            {
                currentNodeId = nextId;
                visitedNodes.Add(currentNodeId);
            }
            else
            {
                // No more dialogue
                isComplete = true;
                Debug.Log($"[DialogueRunner] Dialogue complete. Diagnostic score: {diagnosticScore}/{maxDiagnosticScore}");
            }

            return choice.responseText;
        }

        /// <summary>
        /// Advance to the next node in a linear dialogue (no choices).
        /// </summary>
        public bool AdvanceToNext()
        {
            DialogueDefinition currentNode = GetCurrentNode();
            if (currentNode == null) return false;

            if (!string.IsNullOrEmpty(currentNode.nextDialogueId) && tree.HasNode(currentNode.nextDialogueId))
            {
                currentNodeId = currentNode.nextDialogueId;
                visitedNodes.Add(currentNodeId);
                return true;
            }

            isComplete = true;
            return false;
        }

        /// <summary>
        /// Get the diagnostic score as a ratio (0-1).
        /// </summary>
        public float GetDiagnosticRatio()
        {
            if (maxDiagnosticScore <= 0) return 0f;
            return Mathf.Clamp01((float)diagnosticScore / maxDiagnosticScore);
        }

        /// <summary>
        /// Get the diagnostic score.
        /// </summary>
        public int GetDiagnosticScore()
        {
            return diagnosticScore;
        }

        /// <summary>
        /// Get how many nodes the player has visited.
        /// </summary>
        public int GetVisitedNodeCount()
        {
            return visitedNodes.Count;
        }
    }
}
