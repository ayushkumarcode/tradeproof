using UnityEngine;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Outlet or switch face plate (cover plate).
    /// Types: BlankPlate, SingleGangOutlet, SingleGangSwitch, DoubleGang.
    /// Standard single gang dimensions: 0.114m x 0.070m (4.5" x 2.75").
    /// Has a GrabInteractable component for VR interaction.
    /// Provides Install() / Remove() methods for mounting to outlets and switches.
    /// </summary>
    public class CoverPlate : MonoBehaviour
    {
        [Header("Plate Type")]
        [SerializeField] private PlateType plateType = PlateType.SingleGangOutlet;

        [Header("Dimensions (meters)")]
        [SerializeField] private float plateWidth = 0.070f;    // 2.75 inches
        [SerializeField] private float plateHeight = 0.114f;   // 4.5 inches
        [SerializeField] private float plateThickness = 0.003f;

        [Header("State")]
        [SerializeField] private bool isInstalled;

        [Header("Mount Point")]
        [SerializeField] private SnapPoint mountSnapPoint;

        [Header("Visual")]
        [SerializeField] private MeshRenderer plateRenderer;
        [SerializeField] private Color plateColor = new Color(0.95f, 0.93f, 0.88f, 1f); // Ivory/white

        private GrabInteractable grabInteractable;
        private Material plateMaterial;

        public enum PlateType
        {
            BlankPlate,
            SingleGangOutlet,
            SingleGangSwitch,
            DoubleGang
        }

        public PlateType Type => plateType;
        public bool IsInstalled => isInstalled;
        public SnapPoint MountSnapPoint => mountSnapPoint;

        private void Awake()
        {
            ConfigureDimensions();
            BuildPlateVisual();
            CreateMountSnapPoint();
            SetupGrabInteractable();
        }

        // ---------------------------------------------------------------
        // Configuration
        // ---------------------------------------------------------------

        private void ConfigureDimensions()
        {
            switch (plateType)
            {
                case PlateType.BlankPlate:
                case PlateType.SingleGangOutlet:
                case PlateType.SingleGangSwitch:
                    plateWidth = 0.070f;
                    plateHeight = 0.114f;
                    break;
                case PlateType.DoubleGang:
                    plateWidth = 0.121f;  // ~4.75 inches
                    plateHeight = 0.114f;
                    break;
            }
        }

        // ---------------------------------------------------------------
        // Visual Construction
        // ---------------------------------------------------------------

        private void BuildPlateVisual()
        {
            // --- Main plate body ---
            if (plateRenderer == null)
            {
                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = "CoverPlateBody";
                plate.transform.SetParent(transform, false);
                plate.transform.localPosition = Vector3.zero;
                plate.transform.localScale = new Vector3(plateWidth, plateHeight, plateThickness);

                plateRenderer = plate.GetComponent<MeshRenderer>();
                plateMaterial = new Material(Shader.Find("Standard"));
                plateMaterial.color = plateColor;
                plateMaterial.SetFloat("_Glossiness", 0.4f);
                plateRenderer.material = plateMaterial;

                // Remove auto-generated collider; we add our own on the parent
                Destroy(plate.GetComponent<Collider>());
            }

            // --- Cutout shape based on plate type ---
            switch (plateType)
            {
                case PlateType.SingleGangOutlet:
                    CreateOutletCutout(Vector3.zero);
                    break;

                case PlateType.SingleGangSwitch:
                    CreateSwitchCutout(Vector3.zero);
                    break;

                case PlateType.DoubleGang:
                    CreateOutletCutout(new Vector3(-0.025f, 0f, 0f));
                    CreateOutletCutout(new Vector3(0.025f, 0f, 0f));
                    break;

                case PlateType.BlankPlate:
                    // No cutout — solid plate
                    break;
            }

            // --- Mounting screw holes (top and bottom) ---
            CreateMountingScrewVisual(new Vector3(0f, plateHeight * 0.38f, -plateThickness / 2f - 0.001f));
            CreateMountingScrewVisual(new Vector3(0f, -plateHeight * 0.38f, -plateThickness / 2f - 0.001f));

            // --- Beveled edge (thin border around plate) ---
            CreateBeveledEdge();
        }

        private void CreateOutletCutout(Vector3 offset)
        {
            // Rectangular opening for outlet (duplex receptacle shape)
            float cutoutWidth = 0.028f;
            float cutoutHeight = 0.040f;

            GameObject cutout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cutout.name = "OutletCutout";
            cutout.transform.SetParent(transform, false);
            cutout.transform.localPosition = offset + new Vector3(0f, 0f, -plateThickness / 2f - 0.001f);
            cutout.transform.localScale = new Vector3(cutoutWidth, cutoutHeight, 0.002f);

            Material cutoutMat = new Material(Shader.Find("Standard"));
            cutoutMat.color = new Color(0.12f, 0.12f, 0.12f, 1f); // Dark interior
            cutout.GetComponent<MeshRenderer>().material = cutoutMat;
            Destroy(cutout.GetComponent<Collider>());

            // Rounded corners approximated by small spheres at cutout corners
            float hx = cutoutWidth / 2f - 0.002f;
            float hy = cutoutHeight / 2f - 0.002f;
            Vector3[] corners = new Vector3[]
            {
                offset + new Vector3(-hx, hy, -plateThickness / 2f - 0.001f),
                offset + new Vector3(hx, hy, -plateThickness / 2f - 0.001f),
                offset + new Vector3(-hx, -hy, -plateThickness / 2f - 0.001f),
                offset + new Vector3(hx, -hy, -plateThickness / 2f - 0.001f)
            };

            foreach (var corner in corners)
            {
                GameObject cornerDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cornerDot.name = "CutoutCorner";
                cornerDot.transform.SetParent(transform, false);
                cornerDot.transform.localPosition = corner;
                cornerDot.transform.localScale = Vector3.one * 0.003f;

                Material cornerMat = new Material(Shader.Find("Standard"));
                cornerMat.color = new Color(0.12f, 0.12f, 0.12f, 1f);
                cornerDot.GetComponent<MeshRenderer>().material = cornerMat;
                Destroy(cornerDot.GetComponent<Collider>());
            }
        }

        private void CreateSwitchCutout(Vector3 offset)
        {
            // Toggle opening (narrower rectangle, taller)
            float cutoutWidth = 0.012f;
            float cutoutHeight = 0.030f;

            GameObject cutout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cutout.name = "SwitchCutout";
            cutout.transform.SetParent(transform, false);
            cutout.transform.localPosition = offset + new Vector3(0f, 0f, -plateThickness / 2f - 0.001f);
            cutout.transform.localScale = new Vector3(cutoutWidth, cutoutHeight, 0.002f);

            Material cutoutMat = new Material(Shader.Find("Standard"));
            cutoutMat.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            cutout.GetComponent<MeshRenderer>().material = cutoutMat;
            Destroy(cutout.GetComponent<Collider>());
        }

        private void CreateMountingScrewVisual(Vector3 localPos)
        {
            // Screw slot (small elongated hole)
            GameObject screwSlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screwSlot.name = "ScrewSlot";
            screwSlot.transform.SetParent(transform, false);
            screwSlot.transform.localPosition = localPos;
            screwSlot.transform.localScale = new Vector3(0.008f, 0.003f, 0.001f);

            Material slotMat = new Material(Shader.Find("Standard"));
            slotMat.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            screwSlot.GetComponent<MeshRenderer>().material = slotMat;
            Destroy(screwSlot.GetComponent<Collider>());

            // Screw head (small cylinder)
            GameObject screwHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            screwHead.name = "ScrewHead";
            screwHead.transform.SetParent(transform, false);
            screwHead.transform.localPosition = localPos + new Vector3(0f, 0f, -0.001f);
            screwHead.transform.localScale = new Vector3(0.006f, 0.001f, 0.006f);
            screwHead.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Material screwMat = new Material(Shader.Find("Standard"));
            screwMat.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            screwMat.SetFloat("_Metallic", 0.8f);
            screwMat.SetFloat("_Glossiness", 0.5f);
            screwHead.GetComponent<MeshRenderer>().material = screwMat;
            Destroy(screwHead.GetComponent<Collider>());
        }

        private void CreateBeveledEdge()
        {
            Color edgeColor = plateColor * 0.92f; // Slightly darker edge

            // Top edge
            CreateEdgeSegment("EdgeTop",
                new Vector3(0f, plateHeight / 2f, 0f),
                new Vector3(plateWidth + 0.002f, 0.002f, plateThickness + 0.001f),
                edgeColor);

            // Bottom edge
            CreateEdgeSegment("EdgeBottom",
                new Vector3(0f, -plateHeight / 2f, 0f),
                new Vector3(plateWidth + 0.002f, 0.002f, plateThickness + 0.001f),
                edgeColor);

            // Left edge
            CreateEdgeSegment("EdgeLeft",
                new Vector3(-plateWidth / 2f, 0f, 0f),
                new Vector3(0.002f, plateHeight + 0.002f, plateThickness + 0.001f),
                edgeColor);

            // Right edge
            CreateEdgeSegment("EdgeRight",
                new Vector3(plateWidth / 2f, 0f, 0f),
                new Vector3(0.002f, plateHeight + 0.002f, plateThickness + 0.001f),
                edgeColor);
        }

        private void CreateEdgeSegment(string name, Vector3 localPos, Vector3 scale, Color color)
        {
            GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = name;
            edge.transform.SetParent(transform, false);
            edge.transform.localPosition = localPos;
            edge.transform.localScale = scale;

            Material edgeMat = new Material(Shader.Find("Standard"));
            edgeMat.color = color;
            edgeMat.SetFloat("_Glossiness", 0.3f);
            edge.GetComponent<MeshRenderer>().material = edgeMat;
            Destroy(edge.GetComponent<Collider>());
        }

        // ---------------------------------------------------------------
        // Mount Snap Point
        // ---------------------------------------------------------------

        private void CreateMountSnapPoint()
        {
            if (mountSnapPoint == null)
            {
                GameObject spObj = new GameObject("MountPoint");
                spObj.transform.SetParent(transform, false);
                spObj.transform.localPosition = new Vector3(0f, 0f, plateThickness / 2f + 0.002f);

                mountSnapPoint = spObj.AddComponent<SnapPoint>();
                mountSnapPoint.SetAcceptedWireType("any");
                mountSnapPoint.SetAmpRating(0);
                mountSnapPoint.SetSnapPointId("coverplate-mount");
                mountSnapPoint.SetLabel("Cover Plate Mount");
            }
        }

        // ---------------------------------------------------------------
        // Grab Interactable Setup
        // ---------------------------------------------------------------

        private void SetupGrabInteractable()
        {
            grabInteractable = GetComponent<GrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = gameObject.AddComponent<GrabInteractable>();
            }

            grabInteractable.SetToolType("coverplate");
            grabInteractable.SetGripOffset(new Vector3(0f, 0f, -0.01f));

            // Add collider for grab detection
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(plateWidth, plateHeight, plateThickness + 0.005f);
                box.center = Vector3.zero;
            }
        }

        // ---------------------------------------------------------------
        // Install / Remove
        // ---------------------------------------------------------------

        /// <summary>
        /// Installs the cover plate onto an outlet or switch.
        /// Disables grab interaction once installed.
        /// </summary>
        public void Install()
        {
            if (isInstalled)
            {
                Debug.LogWarning("[CoverPlate] Already installed");
                return;
            }

            isInstalled = true;

            // Disable further grabbing while installed
            if (grabInteractable != null)
            {
                grabInteractable.SetGrabbable(false);
            }

            Debug.Log($"[CoverPlate] {plateType} installed");
        }

        /// <summary>
        /// Removes the cover plate from the outlet or switch.
        /// Re-enables grab interaction.
        /// </summary>
        public void Remove()
        {
            if (!isInstalled)
            {
                Debug.LogWarning("[CoverPlate] Not currently installed");
                return;
            }

            isInstalled = false;

            // Re-enable grabbing
            if (grabInteractable != null)
            {
                grabInteractable.SetGrabbable(true);
            }

            Debug.Log($"[CoverPlate] {plateType} removed");
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>
        /// Sets the plate color (e.g., ivory, white, brown, etc.).
        /// </summary>
        public void SetColor(Color color)
        {
            plateColor = color;
            if (plateMaterial != null)
            {
                plateMaterial.color = plateColor;
            }
        }

        /// <summary>
        /// Returns true if the plate type matches the device it covers.
        /// </summary>
        public bool ValidateForDevice(string deviceType)
        {
            switch (deviceType)
            {
                case "outlet":
                    return plateType == PlateType.SingleGangOutlet || plateType == PlateType.DoubleGang;
                case "switch":
                    return plateType == PlateType.SingleGangSwitch;
                case "blank":
                    return plateType == PlateType.BlankPlate;
                default:
                    return true;
            }
        }

        public string GetDescription()
        {
            return $"{plateType} cover plate — {(isInstalled ? "installed" : "not installed")}";
        }

        public string GetNECReference()
        {
            return "NEC 406.6: Cover plates shall be installed on all outlets, and NEC 110.12: Neat and workmanlike";
        }
    }
}
