using UnityEngine;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents a standard electrical outlet (receptacle) in the training environment.
    /// Includes hot, neutral, and ground terminals as child snap points.
    /// Supports 15A and 20A configurations per NEC 210.21.
    /// </summary>
    public class Outlet : MonoBehaviour
    {
        [Header("Outlet Properties")]
        [SerializeField] private int amperageRating = 20;
        [SerializeField] private OutletType outletType = OutletType.Duplex;
        [SerializeField] private bool isGFCI;

        [Header("Dimensions (meters)")]
        [SerializeField] private float outletWidth = 0.070f;  // ~2.75 inches
        [SerializeField] private float outletHeight = 0.114f; // ~4.5 inches
        [SerializeField] private float outletDepth = 0.025f;  // ~1 inch

        [Header("Box Dimensions")]
        [SerializeField] private float boxWidth = 0.051f;   // ~2 inches
        [SerializeField] private float boxHeight = 0.076f;  // ~3 inches
        [SerializeField] private float boxDepth = 0.070f;   // ~2.75 inches

        [Header("Terminals — All positioned as LOCAL offsets")]
        [SerializeField] private SnapPoint hotTerminal;
        [SerializeField] private SnapPoint neutralTerminal;
        [SerializeField] private SnapPoint groundTerminal;

        [Header("Visual")]
        [SerializeField] private MeshRenderer outletFaceRenderer;
        [SerializeField] private MeshRenderer outletBoxRenderer;
        [SerializeField] private Color faceColor = new Color(0.9f, 0.88f, 0.8f, 1f); // Ivory
        [SerializeField] private Color boxColor = new Color(0.2f, 0.4f, 0.6f, 1f); // Blue metal

        public enum OutletType
        {
            Duplex,       // Standard two-outlet receptacle
            Single,       // Single outlet
            GFCI,         // GFCI protected
            USB,          // With USB ports
            TwistLock     // Industrial twist-lock
        }

        public int AmperageRating => amperageRating;
        public SnapPoint HotTerminal => hotTerminal;
        public SnapPoint NeutralTerminal => neutralTerminal;
        public SnapPoint GroundTerminal => groundTerminal;
        public bool IsGFCI => isGFCI;

        private void Awake()
        {
            BuildOutletVisual();
            CreateTerminals();
        }

        private void BuildOutletVisual()
        {
            // Create outlet box
            if (outletBoxRenderer == null)
            {
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = "OutletBox";
                box.transform.SetParent(transform, false);
                box.transform.localPosition = new Vector3(0f, 0f, boxDepth / 2f);
                box.transform.localScale = new Vector3(boxWidth, boxHeight, boxDepth);

                outletBoxRenderer = box.GetComponent<MeshRenderer>();
                Material boxMat = new Material(Shader.Find("Standard"));
                boxMat.color = boxColor;
                outletBoxRenderer.material = boxMat;
            }

            // Create outlet face plate
            if (outletFaceRenderer == null)
            {
                GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cube);
                face.name = "OutletFace";
                face.transform.SetParent(transform, false);
                face.transform.localPosition = new Vector3(0f, 0f, -0.002f);
                face.transform.localScale = new Vector3(outletWidth, outletHeight, 0.003f);

                outletFaceRenderer = face.GetComponent<MeshRenderer>();
                Material faceMat = new Material(Shader.Find("Standard"));
                faceMat.color = faceColor;
                outletFaceRenderer.material = faceMat;
            }

            // Create outlet slots (visual only)
            CreateOutletSlots();
        }

        private void CreateOutletSlots()
        {
            // Top outlet
            CreateSlotVisual("TopOutlet", new Vector3(0f, 0.025f, -0.005f));

            // Bottom outlet (for duplex)
            if (outletType == OutletType.Duplex)
            {
                CreateSlotVisual("BottomOutlet", new Vector3(0f, -0.025f, -0.005f));
            }

            // Ground hole
            CreateGroundSlotVisual("GroundSlot_Top", new Vector3(0f, 0.015f, -0.005f));
            if (outletType == OutletType.Duplex)
            {
                CreateGroundSlotVisual("GroundSlot_Bottom", new Vector3(0f, -0.035f, -0.005f));
            }
        }

        private void CreateSlotVisual(string name, Vector3 localPos)
        {
            // Hot slot (shorter, narrower)
            GameObject hotSlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hotSlot.name = $"{name}_Hot";
            hotSlot.transform.SetParent(transform, false);
            hotSlot.transform.localPosition = localPos + new Vector3(-0.008f, 0f, 0f);
            hotSlot.transform.localScale = new Vector3(0.003f, 0.010f, 0.002f);

            Material slotMat = new Material(Shader.Find("Standard"));
            slotMat.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            hotSlot.GetComponent<MeshRenderer>().material = slotMat;
            Destroy(hotSlot.GetComponent<Collider>());

            // Neutral slot (taller, wider — T-shaped for 20A)
            GameObject neutralSlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neutralSlot.name = $"{name}_Neutral";
            neutralSlot.transform.SetParent(transform, false);
            neutralSlot.transform.localPosition = localPos + new Vector3(0.008f, 0f, 0f);

            if (amperageRating >= 20)
            {
                // T-shaped neutral slot for 20A
                neutralSlot.transform.localScale = new Vector3(0.004f, 0.012f, 0.002f);
            }
            else
            {
                neutralSlot.transform.localScale = new Vector3(0.003f, 0.012f, 0.002f);
            }

            neutralSlot.GetComponent<MeshRenderer>().material = new Material(slotMat);
            Destroy(neutralSlot.GetComponent<Collider>());
        }

        private void CreateGroundSlotVisual(string name, Vector3 localPos)
        {
            GameObject groundSlot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            groundSlot.name = name;
            groundSlot.transform.SetParent(transform, false);
            groundSlot.transform.localPosition = localPos;
            groundSlot.transform.localScale = new Vector3(0.005f, 0.001f, 0.005f);
            groundSlot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Material slotMat = new Material(Shader.Find("Standard"));
            slotMat.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            groundSlot.GetComponent<MeshRenderer>().material = slotMat;
            Destroy(groundSlot.GetComponent<Collider>());
        }

        private void CreateTerminals()
        {
            // Hot terminal — brass screw side, LOCAL position relative to outlet
            if (hotTerminal == null)
            {
                GameObject hotObj = new GameObject("HotTerminal");
                hotObj.transform.SetParent(transform, false);
                hotObj.transform.localPosition = new Vector3(-boxWidth / 2f - 0.005f, 0.01f, boxDepth / 2f);

                hotTerminal = hotObj.AddComponent<SnapPoint>();
                hotTerminal.SetAcceptedWireType("hot");
                hotTerminal.SetAmpRating(amperageRating);
                hotTerminal.SetSnapPointId("outlet-hot");
                hotTerminal.SetLabel("Hot (Brass)");
            }

            // Neutral terminal — silver screw side
            if (neutralTerminal == null)
            {
                GameObject neutralObj = new GameObject("NeutralTerminal");
                neutralObj.transform.SetParent(transform, false);
                neutralObj.transform.localPosition = new Vector3(boxWidth / 2f + 0.005f, 0.01f, boxDepth / 2f);

                neutralTerminal = neutralObj.AddComponent<SnapPoint>();
                neutralTerminal.SetAcceptedWireType("neutral");
                neutralTerminal.SetAmpRating(0);
                neutralTerminal.SetSnapPointId("outlet-neutral");
                neutralTerminal.SetLabel("Neutral (Silver)");
            }

            // Ground terminal — green screw
            if (groundTerminal == null)
            {
                GameObject groundObj = new GameObject("GroundTerminal");
                groundObj.transform.SetParent(transform, false);
                groundObj.transform.localPosition = new Vector3(0f, -boxHeight / 2f - 0.005f, boxDepth / 2f);

                groundTerminal = groundObj.AddComponent<SnapPoint>();
                groundTerminal.SetAcceptedWireType("ground");
                groundTerminal.SetAmpRating(0);
                groundTerminal.SetSnapPointId("outlet-ground");
                groundTerminal.SetLabel("Ground (Green)");
            }
        }

        // --- Validation ---

        /// <summary>
        /// Validates outlet installation per NEC 210.21.
        /// On a 20A circuit, either 15A or 20A outlets are allowed.
        /// On a 15A circuit, only 15A outlets are allowed.
        /// </summary>
        public bool ValidateForCircuit(int circuitAmperage)
        {
            if (circuitAmperage == 20)
            {
                return amperageRating == 15 || amperageRating == 20;
            }
            if (circuitAmperage == 15)
            {
                return amperageRating == 15;
            }
            return amperageRating <= circuitAmperage;
        }

        public bool AreAllTerminalsConnected()
        {
            return hotTerminal != null && hotTerminal.IsOccupied &&
                   neutralTerminal != null && neutralTerminal.IsOccupied &&
                   groundTerminal != null && groundTerminal.IsOccupied;
        }

        public bool ValidateConnections()
        {
            bool valid = true;

            if (hotTerminal != null && hotTerminal.IsOccupied)
            {
                valid &= hotTerminal.ValidateConnection();
            }
            else
            {
                valid = false;
            }

            if (neutralTerminal != null && neutralTerminal.IsOccupied)
            {
                valid &= neutralTerminal.ValidateConnection();
            }
            else
            {
                valid = false;
            }

            if (groundTerminal != null && groundTerminal.IsOccupied)
            {
                valid &= groundTerminal.ValidateConnection();
            }
            else
            {
                valid = false;
            }

            return valid;
        }

        public string GetNECReference()
        {
            return $"NEC 210.21(B)(3): {amperageRating}A receptacle on {amperageRating}A circuit";
        }
    }
}
