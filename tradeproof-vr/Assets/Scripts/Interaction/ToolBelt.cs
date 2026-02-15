using UnityEngine;
using System.Collections.Generic;

namespace TradeProof.Interaction
{
    /// <summary>
    /// A tool belt that follows the player's waist position.
    /// Provides quick access to tools during training tasks.
    /// Tools snap back to their belt positions when released near the belt.
    /// </summary>
    public class ToolBelt : MonoBehaviour
    {
        [Header("Belt Settings")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float beltHeightOffset = -0.5f; // Below camera
        [SerializeField] private float beltForwardOffset = 0.15f; // Slightly in front
        [SerializeField] private float beltFollowSpeed = 5f;

        [Header("Tool Slots")]
        [SerializeField] private List<ToolSlot> toolSlots = new List<ToolSlot>();
        [SerializeField] private float slotSpacing = 0.1f;
        [SerializeField] private float returnSnapRadius = 0.15f;

        [Header("Visual")]
        [SerializeField] private bool showBeltVisual = true;
        [SerializeField] private float beltWidth = 0.4f;
        private LineRenderer beltLineRenderer;

        [System.Serializable]
        public class ToolSlot
        {
            public string slotName;
            public GrabInteractable tool;
            public Vector3 localOffset;
            public bool isOccupied;
            public Vector3 originalToolPosition;
            public Quaternion originalToolRotation;
        }

        private void Start()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main?.transform;
            }

            SetupBeltVisual();
            InitializeToolSlots();
        }

        private void SetupBeltVisual()
        {
            if (!showBeltVisual) return;

            beltLineRenderer = gameObject.GetComponent<LineRenderer>();
            if (beltLineRenderer == null)
            {
                beltLineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            beltLineRenderer.positionCount = 2;
            beltLineRenderer.startWidth = 0.01f;
            beltLineRenderer.endWidth = 0.01f;
            beltLineRenderer.useWorldSpace = true;

            Material beltMat = new Material(Shader.Find("Standard"));
            beltMat.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            beltMat.SetFloat("_Mode", 3);
            beltMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            beltMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            beltMat.SetInt("_ZWrite", 0);
            beltMat.EnableKeyword("_ALPHABLEND_ON");
            beltMat.renderQueue = 3000;
            beltLineRenderer.material = beltMat;
        }

        private void InitializeToolSlots()
        {
            // Create default tool slot positions if none defined
            if (toolSlots.Count == 0)
            {
                CreateDefaultSlots();
            }

            // Record original tool positions
            foreach (var slot in toolSlots)
            {
                if (slot.tool != null)
                {
                    slot.originalToolPosition = slot.tool.transform.position;
                    slot.originalToolRotation = slot.tool.transform.rotation;
                    slot.isOccupied = true;
                }
            }
        }

        private void CreateDefaultSlots()
        {
            // Wire strippers slot
            ToolSlot strippersSlot = new ToolSlot();
            strippersSlot.slotName = "Wire Strippers";
            strippersSlot.localOffset = new Vector3(-0.3f, 0f, 0f);
            toolSlots.Add(strippersSlot);

            // Screwdriver slot
            ToolSlot screwdriverSlot = new ToolSlot();
            screwdriverSlot.slotName = "Screwdriver";
            screwdriverSlot.localOffset = new Vector3(-0.15f, 0f, 0f);
            toolSlots.Add(screwdriverSlot);

            // Voltage tester slot
            ToolSlot testerSlot = new ToolSlot();
            testerSlot.slotName = "Voltage Tester";
            testerSlot.localOffset = new Vector3(0f, 0f, 0f);
            toolSlots.Add(testerSlot);

            // Multimeter slot
            ToolSlot multimeterSlot = new ToolSlot();
            multimeterSlot.slotName = "Multimeter";
            multimeterSlot.localOffset = new Vector3(0.15f, 0f, 0f);
            toolSlots.Add(multimeterSlot);

            // Conduit bender slot
            ToolSlot benderSlot = new ToolSlot();
            benderSlot.slotName = "Conduit Bender";
            benderSlot.localOffset = new Vector3(0.3f, 0f, 0f);
            toolSlots.Add(benderSlot);
        }

        public void ConfigureForTask(string[] requiredTools)
        {
            // Clear existing slots
            toolSlots.Clear();

            if (requiredTools == null || requiredTools.Length == 0)
            {
                CreateDefaultSlots();
                return;
            }

            float spacing = 0.15f;
            float startX = -(requiredTools.Length - 1) * spacing / 2f;

            for (int i = 0; i < requiredTools.Length; i++)
            {
                ToolSlot slot = new ToolSlot();
                slot.slotName = requiredTools[i];
                slot.localOffset = new Vector3(startX + i * spacing, 0f, 0f);
                toolSlots.Add(slot);
            }

            Debug.Log($"[ToolBelt] Configured {requiredTools.Length} tool slots for task");
        }

        public ToolSlot GetSlotByName(string toolName)
        {
            foreach (var slot in toolSlots)
            {
                if (slot.slotName == toolName) return slot;
            }
            return null;
        }

        public void ClearAllSlots()
        {
            foreach (var slot in toolSlots)
            {
                if (slot.tool != null)
                {
                    Destroy(slot.tool.gameObject);
                    slot.tool = null;
                }
                slot.isOccupied = false;
            }
        }

        private void Update()
        {
            UpdateBeltPosition();
            UpdateBeltVisual();
            CheckToolReturns();
        }

        private void UpdateBeltPosition()
        {
            if (playerCamera == null) return;

            // Calculate belt position relative to player
            Vector3 cameraPos = playerCamera.position;
            Vector3 cameraForward = playerCamera.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 targetPosition = cameraPos +
                                     Vector3.up * beltHeightOffset +
                                     cameraForward * beltForwardOffset;

            // Smoothly follow
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * beltFollowSpeed);

            // Face the camera (but only rotate around Y)
            Vector3 lookDir = cameraForward;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }

            // Update tool slot positions
            for (int i = 0; i < toolSlots.Count; i++)
            {
                ToolSlot slot = toolSlots[i];
                if (slot.tool != null && slot.isOccupied && !slot.tool.IsGrabbed)
                {
                    Vector3 slotWorldPos = transform.TransformPoint(slot.localOffset);
                    slot.tool.transform.position = Vector3.Lerp(
                        slot.tool.transform.position, slotWorldPos, Time.deltaTime * beltFollowSpeed);
                    slot.tool.transform.rotation = transform.rotation;
                }
            }
        }

        private void UpdateBeltVisual()
        {
            if (beltLineRenderer == null) return;

            Vector3 left = transform.TransformPoint(new Vector3(-beltWidth / 2f, 0f, 0f));
            Vector3 right = transform.TransformPoint(new Vector3(beltWidth / 2f, 0f, 0f));

            beltLineRenderer.SetPosition(0, left);
            beltLineRenderer.SetPosition(1, right);
        }

        private void CheckToolReturns()
        {
            foreach (var slot in toolSlots)
            {
                if (slot.tool == null) continue;

                if (slot.tool.IsGrabbed)
                {
                    slot.isOccupied = false;
                }
                else if (!slot.isOccupied)
                {
                    // Check if tool is close enough to snap back
                    Vector3 slotWorldPos = transform.TransformPoint(slot.localOffset);
                    float distance = Vector3.Distance(slot.tool.transform.position, slotWorldPos);

                    if (distance <= returnSnapRadius)
                    {
                        slot.isOccupied = true;
                    }
                }
            }
        }

        // --- Public API ---

        public void AssignTool(int slotIndex, GrabInteractable tool)
        {
            if (slotIndex < 0 || slotIndex >= toolSlots.Count) return;

            toolSlots[slotIndex].tool = tool;
            toolSlots[slotIndex].isOccupied = true;
            toolSlots[slotIndex].originalToolPosition = tool.transform.position;
            toolSlots[slotIndex].originalToolRotation = tool.transform.rotation;
        }

        public void AddToolSlot(string name, GrabInteractable tool, Vector3 offset)
        {
            ToolSlot slot = new ToolSlot();
            slot.slotName = name;
            slot.tool = tool;
            slot.localOffset = offset;
            slot.isOccupied = true;
            slot.originalToolPosition = tool.transform.position;
            slot.originalToolRotation = tool.transform.rotation;
            toolSlots.Add(slot);
        }

        public void ReturnAllTools()
        {
            foreach (var slot in toolSlots)
            {
                if (slot.tool != null)
                {
                    slot.tool.ResetToOriginal();
                    slot.isOccupied = true;
                }
            }
        }
    }
}
