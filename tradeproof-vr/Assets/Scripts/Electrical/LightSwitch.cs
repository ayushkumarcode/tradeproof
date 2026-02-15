using UnityEngine;
using System.Collections.Generic;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents a residential light switch with procedural visuals.
    /// Supports SinglePole, ThreeWay, FourWay, and Dimmer configurations.
    /// Face plate dimensions: 0.07m x 0.114m (standard single gang).
    /// Terminals are child SnapPoints accepting appropriate wire types.
    /// Toggle() flips the paddle visual and isOn state.
    /// </summary>
    public class LightSwitch : MonoBehaviour
    {
        [Header("Switch Type")]
        [SerializeField] private SwitchType switchType = SwitchType.SinglePole;

        [Header("Dimensions (meters)")]
        [SerializeField] private float faceWidth = 0.070f;    // ~2.75 inches
        [SerializeField] private float faceHeight = 0.114f;   // ~4.5 inches
        [SerializeField] private float bodyDepth = 0.025f;    // ~1 inch

        [Header("State")]
        [SerializeField] private bool isOn;
        [SerializeField] private bool isEnergized;

        [Header("Terminals — SinglePole")]
        [SerializeField] private SnapPoint lineTerminal;
        [SerializeField] private SnapPoint loadTerminal;
        [SerializeField] private SnapPoint groundTerminal;

        [Header("Terminals — ThreeWay / FourWay")]
        [SerializeField] private SnapPoint commonTerminal;
        [SerializeField] private SnapPoint traveler1Terminal;
        [SerializeField] private SnapPoint traveler2Terminal;

        [Header("Visual")]
        [SerializeField] private MeshRenderer bodyRenderer;
        [SerializeField] private Transform paddleTransform;
        [SerializeField] private Color bodyColor = new Color(0.95f, 0.93f, 0.88f, 1f); // Ivory

        private List<Transform> screwVisuals = new List<Transform>();

        public enum SwitchType
        {
            SinglePole,
            ThreeWay,
            FourWay,
            Dimmer
        }

        public SwitchType Type => switchType;
        public bool IsOn => isOn;
        public bool IsEnergized => isEnergized;
        public SnapPoint LineTerminal => lineTerminal;
        public SnapPoint LoadTerminal => loadTerminal;
        public SnapPoint GroundTerminal => groundTerminal;
        public SnapPoint CommonTerminal => commonTerminal;
        public SnapPoint Traveler1Terminal => traveler1Terminal;
        public SnapPoint Traveler2Terminal => traveler2Terminal;

        private void Awake()
        {
            BuildSwitchVisual();
            CreateTerminals();
        }

        // ---------------------------------------------------------------
        // Visual Construction
        // ---------------------------------------------------------------

        private void BuildSwitchVisual()
        {
            // --- Switch body (ivory rectangle) ---
            if (bodyRenderer == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "SwitchBody";
                body.transform.SetParent(transform, false);
                body.transform.localPosition = new Vector3(0f, 0f, bodyDepth / 2f);
                body.transform.localScale = new Vector3(faceWidth, faceHeight, bodyDepth);

                bodyRenderer = body.GetComponent<MeshRenderer>();
                Material bodyMat = new Material(Shader.Find("Standard"));
                bodyMat.color = bodyColor;
                bodyRenderer.material = bodyMat;
            }

            // --- Face plate (thin ivory plate on front) ---
            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            face.name = "FacePlate";
            face.transform.SetParent(transform, false);
            face.transform.localPosition = new Vector3(0f, 0f, -0.002f);
            face.transform.localScale = new Vector3(faceWidth, faceHeight, 0.003f);

            Material faceMat = new Material(Shader.Find("Standard"));
            faceMat.color = bodyColor;
            face.GetComponent<MeshRenderer>().material = faceMat;
            Destroy(face.GetComponent<Collider>());

            // --- Toggle opening (dark slot on face) ---
            GameObject toggleSlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toggleSlot.name = "ToggleSlot";
            toggleSlot.transform.SetParent(transform, false);
            toggleSlot.transform.localPosition = new Vector3(0f, 0f, -0.004f);
            toggleSlot.transform.localScale = new Vector3(0.012f, 0.030f, 0.002f);

            Material slotMat = new Material(Shader.Find("Standard"));
            slotMat.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            toggleSlot.GetComponent<MeshRenderer>().material = slotMat;
            Destroy(toggleSlot.GetComponent<Collider>());

            // --- Paddle toggle (small cube that rotates) ---
            if (paddleTransform == null)
            {
                // Pivot point at center of slot
                GameObject paddlePivot = new GameObject("PaddlePivot");
                paddlePivot.transform.SetParent(transform, false);
                paddlePivot.transform.localPosition = new Vector3(0f, 0f, -0.004f);

                GameObject paddle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                paddle.name = "Paddle";
                paddle.transform.SetParent(paddlePivot.transform, false);
                paddle.transform.localPosition = new Vector3(0f, 0.008f, 0f);
                paddle.transform.localScale = new Vector3(0.010f, 0.018f, 0.004f);

                Material paddleMat = new Material(Shader.Find("Standard"));
                paddleMat.color = bodyColor;
                paddle.GetComponent<MeshRenderer>().material = paddleMat;

                // Make paddle trigger for interaction
                BoxCollider paddleCol = paddle.GetComponent<BoxCollider>();
                if (paddleCol != null)
                {
                    paddleCol.isTrigger = true;
                }

                paddleTransform = paddlePivot.transform;

                // Set initial rotation based on state
                UpdatePaddleVisual();
            }

            // --- Mounting screw holes (top and bottom of face) ---
            CreateMountingScrewVisual(new Vector3(0f, faceHeight * 0.40f, -0.004f));
            CreateMountingScrewVisual(new Vector3(0f, -faceHeight * 0.40f, -0.004f));
        }

        private void CreateMountingScrewVisual(Vector3 localPos)
        {
            GameObject screw = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            screw.name = "MountingScrew";
            screw.transform.SetParent(transform, false);
            screw.transform.localPosition = localPos;
            screw.transform.localScale = new Vector3(0.006f, 0.001f, 0.006f);
            screw.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Material screwMat = new Material(Shader.Find("Standard"));
            screwMat.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            screwMat.SetFloat("_Metallic", 0.8f);
            screwMat.SetFloat("_Glossiness", 0.5f);
            screw.GetComponent<MeshRenderer>().material = screwMat;
            Destroy(screw.GetComponent<Collider>());
        }

        private Transform CreateTerminalScrewVisual(Vector3 localPos, Color screwColor)
        {
            GameObject screw = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            screw.name = "TerminalScrew";
            screw.transform.SetParent(transform, false);
            screw.transform.localPosition = localPos;
            screw.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);

            Material screwMat = new Material(Shader.Find("Standard"));
            screwMat.color = screwColor;
            screwMat.SetFloat("_Metallic", 0.9f);
            screwMat.SetFloat("_Glossiness", 0.7f);
            screw.GetComponent<MeshRenderer>().material = screwMat;
            Destroy(screw.GetComponent<Collider>());

            screwVisuals.Add(screw.transform);
            return screw.transform;
        }

        // ---------------------------------------------------------------
        // Terminal Creation
        // ---------------------------------------------------------------

        private void CreateTerminals()
        {
            // Colors for screw visuals
            Color brassColor = new Color(0.72f, 0.58f, 0.20f, 1f);   // Brass (hot / common)
            Color silverColor = new Color(0.78f, 0.78f, 0.80f, 1f);  // Silver (neutral / traveler)
            Color greenColor = new Color(0.18f, 0.55f, 0.18f, 1f);   // Green (ground)

            switch (switchType)
            {
                case SwitchType.SinglePole:
                case SwitchType.Dimmer:
                    CreateSinglePoleTerminals(brassColor, silverColor, greenColor);
                    break;

                case SwitchType.ThreeWay:
                    CreateThreeWayTerminals(brassColor, silverColor, greenColor);
                    break;

                case SwitchType.FourWay:
                    CreateFourWayTerminals(brassColor, silverColor, greenColor);
                    break;
            }
        }

        private void CreateSinglePoleTerminals(Color brass, Color silver, Color green)
        {
            // Line terminal (hot in) — brass screw, right side top
            if (lineTerminal == null)
            {
                Vector3 linePos = new Vector3(faceWidth / 2f + 0.005f, 0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(linePos, brass);

                GameObject lineObj = new GameObject("LineTerminal");
                lineObj.transform.SetParent(transform, false);
                lineObj.transform.localPosition = linePos;

                lineTerminal = lineObj.AddComponent<SnapPoint>();
                lineTerminal.SetAcceptedWireType("hot");
                lineTerminal.SetAmpRating(0);
                lineTerminal.SetSnapPointId("switch-line");
                lineTerminal.SetLabel("Line (Brass)");
            }

            // Load terminal (hot out) — brass screw, right side bottom
            if (loadTerminal == null)
            {
                Vector3 loadPos = new Vector3(faceWidth / 2f + 0.005f, -0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(loadPos, brass);

                GameObject loadObj = new GameObject("LoadTerminal");
                loadObj.transform.SetParent(transform, false);
                loadObj.transform.localPosition = loadPos;

                loadTerminal = loadObj.AddComponent<SnapPoint>();
                loadTerminal.SetAcceptedWireType("hot");
                loadTerminal.SetAmpRating(0);
                loadTerminal.SetSnapPointId("switch-load");
                loadTerminal.SetLabel("Load (Brass)");
            }

            // Ground terminal — green screw, bottom
            if (groundTerminal == null)
            {
                Vector3 groundPos = new Vector3(0f, -faceHeight / 2f + 0.008f, bodyDepth / 2f);
                CreateTerminalScrewVisual(groundPos, green);

                GameObject groundObj = new GameObject("GroundTerminal");
                groundObj.transform.SetParent(transform, false);
                groundObj.transform.localPosition = groundPos;

                groundTerminal = groundObj.AddComponent<SnapPoint>();
                groundTerminal.SetAcceptedWireType("ground");
                groundTerminal.SetAmpRating(0);
                groundTerminal.SetSnapPointId("switch-ground");
                groundTerminal.SetLabel("Ground (Green)");
            }
        }

        private void CreateThreeWayTerminals(Color brass, Color silver, Color green)
        {
            // Common terminal — dark brass screw (distinguishing), right side center
            if (commonTerminal == null)
            {
                Color darkBrass = new Color(0.45f, 0.35f, 0.12f, 1f); // Darker brass for common
                Vector3 commonPos = new Vector3(faceWidth / 2f + 0.005f, 0f, bodyDepth / 2f);
                CreateTerminalScrewVisual(commonPos, darkBrass);

                GameObject commonObj = new GameObject("CommonTerminal");
                commonObj.transform.SetParent(transform, false);
                commonObj.transform.localPosition = commonPos;

                commonTerminal = commonObj.AddComponent<SnapPoint>();
                commonTerminal.SetAcceptedWireType("hot");
                commonTerminal.SetAmpRating(0);
                commonTerminal.SetSnapPointId("switch-common");
                commonTerminal.SetLabel("Common (Dark Brass)");
            }

            // Traveler 1 — silver screw, left side top
            if (traveler1Terminal == null)
            {
                Vector3 t1Pos = new Vector3(-faceWidth / 2f - 0.005f, 0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(t1Pos, silver);

                GameObject t1Obj = new GameObject("Traveler1Terminal");
                t1Obj.transform.SetParent(transform, false);
                t1Obj.transform.localPosition = t1Pos;

                traveler1Terminal = t1Obj.AddComponent<SnapPoint>();
                traveler1Terminal.SetAcceptedWireType("hot");
                traveler1Terminal.SetAmpRating(0);
                traveler1Terminal.SetSnapPointId("switch-traveler1");
                traveler1Terminal.SetLabel("Traveler 1 (Silver)");
            }

            // Traveler 2 — silver screw, left side bottom
            if (traveler2Terminal == null)
            {
                Vector3 t2Pos = new Vector3(-faceWidth / 2f - 0.005f, -0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(t2Pos, silver);

                GameObject t2Obj = new GameObject("Traveler2Terminal");
                t2Obj.transform.SetParent(transform, false);
                t2Obj.transform.localPosition = t2Pos;

                traveler2Terminal = t2Obj.AddComponent<SnapPoint>();
                traveler2Terminal.SetAcceptedWireType("hot");
                traveler2Terminal.SetAmpRating(0);
                traveler2Terminal.SetSnapPointId("switch-traveler2");
                traveler2Terminal.SetLabel("Traveler 2 (Silver)");
            }

            // Ground terminal — green screw, bottom
            if (groundTerminal == null)
            {
                Vector3 groundPos = new Vector3(0f, -faceHeight / 2f + 0.008f, bodyDepth / 2f);
                CreateTerminalScrewVisual(groundPos, new Color(0.18f, 0.55f, 0.18f, 1f));

                GameObject groundObj = new GameObject("GroundTerminal");
                groundObj.transform.SetParent(transform, false);
                groundObj.transform.localPosition = groundPos;

                groundTerminal = groundObj.AddComponent<SnapPoint>();
                groundTerminal.SetAcceptedWireType("ground");
                groundTerminal.SetAmpRating(0);
                groundTerminal.SetSnapPointId("switch-ground");
                groundTerminal.SetLabel("Ground (Green)");
            }
        }

        private void CreateFourWayTerminals(Color brass, Color silver, Color green)
        {
            // Four-way has 4 traveler terminals (2 in, 2 out) and a ground
            // Traveler In 1 — brass screw, right side top
            if (traveler1Terminal == null)
            {
                Vector3 t1Pos = new Vector3(faceWidth / 2f + 0.005f, 0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(t1Pos, brass);

                GameObject t1Obj = new GameObject("Traveler1Terminal");
                t1Obj.transform.SetParent(transform, false);
                t1Obj.transform.localPosition = t1Pos;

                traveler1Terminal = t1Obj.AddComponent<SnapPoint>();
                traveler1Terminal.SetAcceptedWireType("hot");
                traveler1Terminal.SetAmpRating(0);
                traveler1Terminal.SetSnapPointId("switch-traveler1");
                traveler1Terminal.SetLabel("Traveler In 1 (Brass)");
            }

            // Traveler In 2 — brass screw, right side bottom
            if (traveler2Terminal == null)
            {
                Vector3 t2Pos = new Vector3(faceWidth / 2f + 0.005f, -0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(t2Pos, brass);

                GameObject t2Obj = new GameObject("Traveler2Terminal");
                t2Obj.transform.SetParent(transform, false);
                t2Obj.transform.localPosition = t2Pos;

                traveler2Terminal = t2Obj.AddComponent<SnapPoint>();
                traveler2Terminal.SetAcceptedWireType("hot");
                traveler2Terminal.SetAmpRating(0);
                traveler2Terminal.SetSnapPointId("switch-traveler2");
                traveler2Terminal.SetLabel("Traveler In 2 (Brass)");
            }

            // Common In — silver screw, left side top (output traveler)
            if (commonTerminal == null)
            {
                Vector3 commonPos = new Vector3(-faceWidth / 2f - 0.005f, 0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(commonPos, silver);

                GameObject commonObj = new GameObject("CommonTerminal");
                commonObj.transform.SetParent(transform, false);
                commonObj.transform.localPosition = commonPos;

                commonTerminal = commonObj.AddComponent<SnapPoint>();
                commonTerminal.SetAcceptedWireType("hot");
                commonTerminal.SetAmpRating(0);
                commonTerminal.SetSnapPointId("switch-common");
                commonTerminal.SetLabel("Traveler Out 1 (Silver)");
            }

            // Line terminal reused as second output traveler — silver screw, left side bottom
            if (lineTerminal == null)
            {
                Vector3 linePos = new Vector3(-faceWidth / 2f - 0.005f, -0.02f, bodyDepth / 2f);
                CreateTerminalScrewVisual(linePos, silver);

                GameObject lineObj = new GameObject("LineTerminal");
                lineObj.transform.SetParent(transform, false);
                lineObj.transform.localPosition = linePos;

                lineTerminal = lineObj.AddComponent<SnapPoint>();
                lineTerminal.SetAcceptedWireType("hot");
                lineTerminal.SetAmpRating(0);
                lineTerminal.SetSnapPointId("switch-traveler-out2");
                lineTerminal.SetLabel("Traveler Out 2 (Silver)");
            }

            // Ground terminal — green screw, bottom
            if (groundTerminal == null)
            {
                Vector3 groundPos = new Vector3(0f, -faceHeight / 2f + 0.008f, bodyDepth / 2f);
                CreateTerminalScrewVisual(groundPos, new Color(0.18f, 0.55f, 0.18f, 1f));

                GameObject groundObj = new GameObject("GroundTerminal");
                groundObj.transform.SetParent(transform, false);
                groundObj.transform.localPosition = groundPos;

                groundTerminal = groundObj.AddComponent<SnapPoint>();
                groundTerminal.SetAcceptedWireType("ground");
                groundTerminal.SetAmpRating(0);
                groundTerminal.SetSnapPointId("switch-ground");
                groundTerminal.SetLabel("Ground (Green)");
            }
        }

        // ---------------------------------------------------------------
        // Operations
        // ---------------------------------------------------------------

        /// <summary>
        /// Flips the switch state and rotates the paddle visual by 30 degrees.
        /// </summary>
        public void Toggle()
        {
            isOn = !isOn;
            UpdatePaddleVisual();

            Debug.Log($"[LightSwitch] {switchType} toggled {(isOn ? "ON" : "OFF")}");

            // Propagate power state through load terminal when energized
            if (isEnergized)
            {
                PropagePowerState();
            }
        }

        /// <summary>
        /// Set whether the switch is receiving power from the supply.
        /// </summary>
        public void SetEnergized(bool energized)
        {
            isEnergized = energized;

            if (isEnergized && isOn)
            {
                PropagePowerState();
            }
        }

        private void UpdatePaddleVisual()
        {
            if (paddleTransform == null) return;

            // Rotate paddle 30 degrees up (on) or down (off)
            float angle = isOn ? -30f : 30f;
            paddleTransform.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        private void PropagePowerState()
        {
            // Simple power propagation: when energized and on, load side is hot
            bool loadHot = isEnergized && isOn;
            Debug.Log($"[LightSwitch] Load side {(loadHot ? "ENERGIZED" : "DE-ENERGIZED")}");
        }

        // ---------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------

        /// <summary>
        /// Checks that all required terminals for this switch type are occupied.
        /// </summary>
        public bool ValidateConnections()
        {
            bool valid = true;

            switch (switchType)
            {
                case SwitchType.SinglePole:
                case SwitchType.Dimmer:
                    if (lineTerminal == null || !lineTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Line terminal not connected");
                        valid = false;
                    }
                    if (loadTerminal == null || !loadTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Load terminal not connected");
                        valid = false;
                    }
                    if (groundTerminal == null || !groundTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Ground terminal not connected — NEC 404.9(B)");
                        valid = false;
                    }
                    break;

                case SwitchType.ThreeWay:
                    if (commonTerminal == null || !commonTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Common terminal not connected");
                        valid = false;
                    }
                    if (traveler1Terminal == null || !traveler1Terminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Traveler 1 terminal not connected");
                        valid = false;
                    }
                    if (traveler2Terminal == null || !traveler2Terminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Traveler 2 terminal not connected");
                        valid = false;
                    }
                    if (groundTerminal == null || !groundTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Ground terminal not connected — NEC 404.9(B)");
                        valid = false;
                    }
                    break;

                case SwitchType.FourWay:
                    if (traveler1Terminal == null || !traveler1Terminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Traveler In 1 not connected");
                        valid = false;
                    }
                    if (traveler2Terminal == null || !traveler2Terminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Traveler In 2 not connected");
                        valid = false;
                    }
                    if (commonTerminal == null || !commonTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Traveler Out 1 not connected");
                        valid = false;
                    }
                    if (lineTerminal == null || !lineTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Traveler Out 2 not connected");
                        valid = false;
                    }
                    if (groundTerminal == null || !groundTerminal.IsOccupied)
                    {
                        Debug.LogWarning("[LightSwitch] Ground terminal not connected — NEC 404.9(B)");
                        valid = false;
                    }
                    break;
            }

            return valid;
        }

        /// <summary>
        /// Returns whether the switch is providing power on the load side.
        /// For three-way, this depends on internal contact position and energized state.
        /// </summary>
        public bool IsOutputEnergized()
        {
            return isEnergized && isOn;
        }

        public string GetNECReference()
        {
            switch (switchType)
            {
                case SwitchType.SinglePole:
                    return "NEC 404.2(A): Single-pole switch on ungrounded conductor";
                case SwitchType.ThreeWay:
                    return "NEC 404.2(A): Three-way switch — common + 2 travelers";
                case SwitchType.FourWay:
                    return "NEC 404.2(A): Four-way switch — 4 travelers between three-way pair";
                case SwitchType.Dimmer:
                    return "NEC 404.14(E): Dimmer switch — rated for load type";
                default:
                    return "NEC 404";
            }
        }
    }
}
