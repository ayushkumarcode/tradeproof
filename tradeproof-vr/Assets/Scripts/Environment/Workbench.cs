using UnityEngine;
using System.Collections.Generic;
using TradeProof.Interaction;
using TradeProof.UI;

namespace TradeProof.Environment
{
    /// <summary>
    /// Procedural workbench with tool pegboard.
    /// Table surface at 0.9m with legs, pegboard behind on wall with tool outline slots.
    /// Tools can be returned to their pegboard positions.
    /// </summary>
    public class Workbench : MonoBehaviour
    {
        [Header("Dimensions")]
        [SerializeField] private float tableWidth = 2f;
        [SerializeField] private float tableDepth = 0.8f;
        [SerializeField] private float tableHeight = 0.9f;
        [SerializeField] private float tableThickness = 0.05f;
        [SerializeField] private float pegboardWidth = 2f;
        [SerializeField] private float pegboardHeight = 1.5f;

        [Header("Colors")]
        [SerializeField] private Color tableColor = new Color(0.5f, 0.35f, 0.2f);   // Brown wood
        [SerializeField] private Color legColor = new Color(0.35f, 0.25f, 0.15f);     // Darker wood legs
        [SerializeField] private Color pegboardColor = new Color(0.75f, 0.6f, 0.4f);  // Light pegboard
        [SerializeField] private Color outlineColor = new Color(0.45f, 0.35f, 0.2f);  // Darker outline

        // Tool slot management
        private Dictionary<string, Vector3> toolSlots = new Dictionary<string, Vector3>();
        private Dictionary<string, GrabInteractable> assignedTools = new Dictionary<string, GrabInteractable>();
        private Dictionary<string, GameObject> toolOutlines = new Dictionary<string, GameObject>();

        // Generated objects
        private GameObject tableSurface;
        private GameObject pegboard;
        private List<GameObject> legs = new List<GameObject>();

        // Default tool types and relative positions on pegboard
        private static readonly (string toolType, Vector3 relativePos)[] DefaultSlots = {
            ("wire-strippers",   new Vector3(-0.7f, 0.5f, -0.02f)),
            ("screwdriver",      new Vector3(-0.35f, 0.5f, -0.02f)),
            ("multimeter",       new Vector3(0f, 0.5f, -0.02f)),
            ("voltage-tester",   new Vector3(0.35f, 0.5f, -0.02f)),
            ("conduit-bender",   new Vector3(0.7f, 0.3f, -0.02f)),
            ("reaming-tool",     new Vector3(-0.5f, 0.15f, -0.02f)),
            ("tape-measure",     new Vector3(0.5f, 0.15f, -0.02f)),
        };

        private void Awake()
        {
            BuildWorkbench();
            BuildPegboard();
            SetupDefaultSlots();
        }

        // --- Construction ---

        private void BuildWorkbench()
        {
            // Table surface: cube at 0.9m height
            tableSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tableSurface.name = "TableSurface";
            tableSurface.transform.SetParent(transform, false);
            tableSurface.transform.localPosition = new Vector3(0f, tableHeight, 0f);
            tableSurface.transform.localScale = new Vector3(tableWidth, tableThickness, tableDepth);
            ApplyColor(tableSurface, tableColor);

            // Four legs at corners
            float legWidth = 0.06f;
            float halfW = tableWidth / 2f - legWidth / 2f - 0.02f;
            float halfD = tableDepth / 2f - legWidth / 2f - 0.02f;
            float legHeight = tableHeight - tableThickness / 2f;

            CreateLeg("LegFL", new Vector3(-halfW, legHeight / 2f, -halfD), legWidth, legHeight);
            CreateLeg("LegFR", new Vector3(halfW, legHeight / 2f, -halfD), legWidth, legHeight);
            CreateLeg("LegBL", new Vector3(-halfW, legHeight / 2f, halfD), legWidth, legHeight);
            CreateLeg("LegBR", new Vector3(halfW, legHeight / 2f, halfD), legWidth, legHeight);

            // Cross support between front legs
            GameObject crossbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossbar.name = "Crossbar";
            crossbar.transform.SetParent(transform, false);
            crossbar.transform.localPosition = new Vector3(0f, 0.3f, -halfD);
            crossbar.transform.localScale = new Vector3(tableWidth - 0.1f, 0.04f, 0.04f);
            ApplyColor(crossbar, legColor);
            Collider crossCol = crossbar.GetComponent<Collider>();
            if (crossCol != null) Destroy(crossCol);
        }

        private void CreateLeg(string name, Vector3 localPos, float width, float height)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = name;
            leg.transform.SetParent(transform, false);
            leg.transform.localPosition = localPos;
            leg.transform.localScale = new Vector3(width, height, width);
            ApplyColor(leg, legColor);

            Collider col = leg.GetComponent<Collider>();
            if (col != null) Destroy(col);

            legs.Add(leg);
        }

        private void BuildPegboard()
        {
            // Vertical pegboard behind the table, on the wall
            pegboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pegboard.name = "Pegboard";
            pegboard.transform.SetParent(transform, false);
            pegboard.transform.localPosition = new Vector3(
                0f,
                tableHeight + pegboardHeight / 2f + 0.1f,
                tableDepth / 2f - 0.01f
            );
            pegboard.transform.localScale = new Vector3(pegboardWidth, pegboardHeight, 0.02f);
            ApplyColor(pegboard, pegboardColor);

            // Add pegboard holes pattern (decorative dots grid)
            CreatePegboardPattern();
        }

        private void CreatePegboardPattern()
        {
            // Create a subtle pattern of "holes" on the pegboard face using small dark quads
            float spacing = 0.08f;
            int cols = Mathf.FloorToInt(pegboardWidth / spacing) - 1;
            int rows = Mathf.FloorToInt(pegboardHeight / spacing) - 1;

            GameObject patternParent = new GameObject("PegHoles");
            patternParent.transform.SetParent(pegboard.transform, false);

            // Only create a subset to avoid too many draw calls
            int step = 3; // Every 3rd hole for performance
            for (int r = 0; r < rows; r += step)
            {
                for (int c = 0; c < cols; c += step)
                {
                    float x = (-cols / 2f + c) * spacing / pegboardWidth;
                    float y = (-rows / 2f + r) * spacing / pegboardHeight;

                    GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    hole.name = "Hole";
                    hole.transform.SetParent(patternParent.transform, false);
                    hole.transform.localPosition = new Vector3(x, y, -0.51f);
                    hole.transform.localScale = Vector3.one * 0.01f;
                    Renderer holeRend = hole.GetComponent<Renderer>();
                    Material holeMat = new Material(Shader.Find("Standard"));
                    holeMat.color = new Color(0.55f, 0.4f, 0.25f); // Slightly darker than pegboard
                    holeRend.material = holeMat;

                    Collider holeCol = hole.GetComponent<Collider>();
                    if (holeCol != null) Destroy(holeCol);
                }
            }
        }

        private void SetupDefaultSlots()
        {
            foreach (var (toolType, relPos) in DefaultSlots)
            {
                // Convert relative pegboard position to world position of the pegboard
                Vector3 worldPos = pegboard.transform.TransformPoint(
                    new Vector3(
                        relPos.x / pegboardWidth,
                        relPos.y / pegboardHeight,
                        relPos.z
                    )
                );

                Vector3 slotLocalPos = transform.InverseTransformPoint(worldPos);
                toolSlots[toolType] = slotLocalPos;

                // Create visual outline on pegboard
                CreateToolOutline(toolType, relPos);
            }

            Debug.Log($"[Workbench] Set up {toolSlots.Count} default tool slots.");
        }

        // --- Tool Outline ---

        /// <summary>
        /// Creates a visual outline shape on the pegboard at the specified position.
        /// </summary>
        public void CreateToolOutline(string toolType, Vector3 position)
        {
            if (toolOutlines.ContainsKey(toolType))
            {
                Destroy(toolOutlines[toolType]);
                toolOutlines.Remove(toolType);
            }

            GameObject outline = new GameObject($"ToolOutline_{toolType}");
            outline.transform.SetParent(pegboard.transform, false);

            // Determine outline shape and size based on tool type
            Vector3 outlineScale;
            PrimitiveType outlineShape;

            switch (toolType)
            {
                case "wire-strippers":
                    outlineShape = PrimitiveType.Cube;
                    outlineScale = new Vector3(0.06f, 0.18f, 0.005f);
                    break;
                case "screwdriver":
                    outlineShape = PrimitiveType.Cube;
                    outlineScale = new Vector3(0.025f, 0.22f, 0.005f);
                    break;
                case "multimeter":
                    outlineShape = PrimitiveType.Cube;
                    outlineScale = new Vector3(0.08f, 0.14f, 0.005f);
                    break;
                case "voltage-tester":
                    outlineShape = PrimitiveType.Cube;
                    outlineScale = new Vector3(0.03f, 0.16f, 0.005f);
                    break;
                case "conduit-bender":
                    outlineShape = PrimitiveType.Cube;
                    outlineScale = new Vector3(0.05f, 0.25f, 0.005f);
                    break;
                case "reaming-tool":
                    outlineShape = PrimitiveType.Cylinder;
                    outlineScale = new Vector3(0.03f, 0.09f, 0.03f);
                    break;
                case "tape-measure":
                    outlineShape = PrimitiveType.Cube;
                    outlineScale = new Vector3(0.08f, 0.08f, 0.005f);
                    break;
                default:
                    outlineShape = PrimitiveType.Cube;
                    outlineScale = new Vector3(0.06f, 0.12f, 0.005f);
                    break;
            }

            GameObject outlinePrimitive = GameObject.CreatePrimitive(outlineShape);
            outlinePrimitive.name = "OutlineShape";
            outlinePrimitive.transform.SetParent(outline.transform, false);
            outlinePrimitive.transform.localPosition = Vector3.zero;
            outlinePrimitive.transform.localScale = outlineScale;

            Renderer outlineRend = outlinePrimitive.GetComponent<Renderer>();
            Material outlineMat = new Material(Shader.Find("Standard"));
            outlineMat.color = outlineColor;
            outlineMat.SetFloat("_Mode", 3); // Transparent
            outlineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMat.SetInt("_ZWrite", 0);
            outlineMat.EnableKeyword("_ALPHABLEND_ON");
            outlineMat.renderQueue = 3000;
            outlineMat.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, 0.4f);
            outlineRend.material = outlineMat;

            Collider outlineCol = outlinePrimitive.GetComponent<Collider>();
            if (outlineCol != null) Destroy(outlineCol);

            // Position on pegboard
            outline.transform.localPosition = new Vector3(
                position.x / pegboardWidth,
                position.y / pegboardHeight,
                position.z - 0.01f
            );

            // Label under the outline
            GameObject labelObj = new GameObject("ToolLabel");
            labelObj.transform.SetParent(outline.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, -outlineScale.y / 2f - 0.02f, 0f);
            labelObj.transform.localScale = Vector3.one * 0.5f;
            FloatingLabel label = labelObj.AddComponent<FloatingLabel>();
            label.SetText(FormatToolName(toolType));
            label.SetFontSize(1.5f);
            label.SetColor(new Color(0.8f, 0.8f, 0.8f));
            label.SetBillboard(false);

            toolOutlines[toolType] = outline;
        }

        private string FormatToolName(string toolType)
        {
            // Convert "wire-strippers" -> "Wire Strippers"
            string[] parts = toolType.Split('-');
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (string part in parts)
            {
                if (sb.Length > 0) sb.Append(' ');
                if (part.Length > 0)
                {
                    sb.Append(char.ToUpper(part[0]));
                    if (part.Length > 1)
                        sb.Append(part.Substring(1));
                }
            }
            return sb.ToString();
        }

        // --- Tool Slot Management ---

        /// <summary>
        /// Returns a tool to its designated pegboard slot position.
        /// </summary>
        public void ReturnToolToSlot(GrabInteractable tool)
        {
            if (tool == null) return;

            string toolType = tool.ToolType;
            if (string.IsNullOrEmpty(toolType))
            {
                Debug.LogWarning("[Workbench] Cannot return tool -- no tool type assigned.");
                return;
            }

            if (!toolSlots.ContainsKey(toolType))
            {
                Debug.LogWarning($"[Workbench] No slot defined for tool type: {toolType}");
                return;
            }

            Vector3 slotLocalPos = toolSlots[toolType];
            Vector3 worldPos = transform.TransformPoint(slotLocalPos);

            tool.transform.position = worldPos;
            tool.transform.rotation = Quaternion.identity;
            tool.transform.SetParent(transform);

            // Update assignment
            assignedTools[toolType] = tool;

            // Highlight outline to show tool is home
            if (toolOutlines.ContainsKey(toolType))
            {
                Renderer outlineRend = toolOutlines[toolType].GetComponentInChildren<Renderer>();
                if (outlineRend != null)
                {
                    Color c = outlineRend.material.color;
                    outlineRend.material.color = new Color(0.2f, 0.6f, 0.2f, 0.3f); // Green tint when occupied
                }
            }

            Debug.Log($"[Workbench] Tool '{toolType}' returned to slot.");
        }

        /// <summary>
        /// Registers a tool to a specific slot on the pegboard.
        /// </summary>
        public void AssignToolToSlot(string toolType, GrabInteractable tool)
        {
            if (string.IsNullOrEmpty(toolType) || tool == null) return;

            assignedTools[toolType] = tool;
            tool.SetToolType(toolType);

            // Position tool at slot
            if (toolSlots.ContainsKey(toolType))
            {
                Vector3 worldPos = transform.TransformPoint(toolSlots[toolType]);
                tool.transform.position = worldPos;
                tool.transform.rotation = Quaternion.identity;
                tool.transform.SetParent(transform);
            }

            Debug.Log($"[Workbench] Tool '{toolType}' assigned to slot.");
        }

        /// <summary>
        /// Gets the world position for a tool slot.
        /// </summary>
        public Vector3 GetSlotWorldPosition(string toolType)
        {
            if (toolSlots.TryGetValue(toolType, out Vector3 localPos))
            {
                return transform.TransformPoint(localPos);
            }
            return transform.position;
        }

        /// <summary>
        /// Checks if a tool type has been assigned to a slot.
        /// </summary>
        public bool HasToolInSlot(string toolType)
        {
            return assignedTools.ContainsKey(toolType) && assignedTools[toolType] != null;
        }

        /// <summary>
        /// Gets the tool assigned to a slot, or null if empty.
        /// </summary>
        public GrabInteractable GetToolFromSlot(string toolType)
        {
            if (assignedTools.TryGetValue(toolType, out GrabInteractable tool))
            {
                return tool;
            }
            return null;
        }

        /// <summary>
        /// Returns all registered tool slot types.
        /// </summary>
        public List<string> GetAllSlotTypes()
        {
            return new List<string>(toolSlots.Keys);
        }

        /// <summary>
        /// Adds a custom tool slot at a given local position.
        /// </summary>
        public void AddCustomSlot(string toolType, Vector3 localPosition)
        {
            toolSlots[toolType] = localPosition;
            CreateToolOutline(toolType, new Vector3(
                localPosition.x * pegboardWidth,
                (localPosition.y - tableHeight - 0.1f),
                -0.02f
            ));
        }

        // --- Utility ---

        private void ApplyColor(GameObject obj, Color color)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                rend.material = mat;
            }
        }
    }
}
