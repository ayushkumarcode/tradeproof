using UnityEngine;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Ceiling-mounted light fixture with a built-in junction box.
    /// Dome is a hemisphere approximated by a scaled sphere (0.3m diameter, 0.1m height).
    /// Junction box (0.1m cube) sits above the dome.
    /// Terminals in the junction box accept hot, neutral, and ground wires.
    /// SetPowered() toggles emission to simulate light on/off.
    /// </summary>
    public class LightFixture : MonoBehaviour
    {
        [Header("Fixture Dimensions (meters)")]
        [SerializeField] private float domeDiameter = 0.30f;
        [SerializeField] private float domeHeight = 0.10f;
        [SerializeField] private float junctionBoxSize = 0.10f;

        [Header("Terminals")]
        [SerializeField] private SnapPoint hotTerminal;
        [SerializeField] private SnapPoint neutralTerminal;
        [SerializeField] private SnapPoint groundTerminal;

        [Header("State")]
        [SerializeField] private bool isPowered;

        [Header("Visual")]
        [SerializeField] private MeshRenderer domeRenderer;
        [SerializeField] private MeshRenderer junctionBoxRenderer;
        [SerializeField] private Color domeColorOff = new Color(0.35f, 0.35f, 0.35f, 1f);    // Dark gray when off
        [SerializeField] private Color domeColorOn = new Color(1f, 0.98f, 0.92f, 1f);         // Warm white when on
        [SerializeField] private Color emissionColor = new Color(1f, 0.95f, 0.80f, 1f);       // Warm white emission
        [SerializeField] private float emissionIntensity = 2f;
        [SerializeField] private Color junctionBoxColor = new Color(0.2f, 0.2f, 0.2f, 1f);    // Dark metal

        private Material domeMaterial;

        public bool IsPowered => isPowered;
        public SnapPoint HotTerminal => hotTerminal;
        public SnapPoint NeutralTerminal => neutralTerminal;
        public SnapPoint GroundTerminal => groundTerminal;

        private void Awake()
        {
            BuildFixtureVisual();
            CreateTerminals();
            ApplyPowerState();
        }

        // ---------------------------------------------------------------
        // Visual Construction
        // ---------------------------------------------------------------

        private void BuildFixtureVisual()
        {
            // --- Junction box (sits at transform origin, above the dome) ---
            if (junctionBoxRenderer == null)
            {
                GameObject jBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jBox.name = "JunctionBox";
                jBox.transform.SetParent(transform, false);
                jBox.transform.localPosition = new Vector3(0f, 0f, 0f);
                jBox.transform.localScale = new Vector3(junctionBoxSize, junctionBoxSize, junctionBoxSize);

                junctionBoxRenderer = jBox.GetComponent<MeshRenderer>();
                Material jBoxMat = new Material(Shader.Find("Standard"));
                jBoxMat.color = junctionBoxColor;
                jBoxMat.SetFloat("_Metallic", 0.6f);
                jBoxMat.SetFloat("_Glossiness", 0.3f);
                junctionBoxRenderer.material = jBoxMat;
            }

            // --- Mounting plate (between junction box and dome) ---
            GameObject mountPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mountPlate.name = "MountingPlate";
            mountPlate.transform.SetParent(transform, false);
            mountPlate.transform.localPosition = new Vector3(0f, -junctionBoxSize / 2f - 0.005f, 0f);
            mountPlate.transform.localScale = new Vector3(0.12f, 0.005f, 0.12f);

            Material plateMat = new Material(Shader.Find("Standard"));
            plateMat.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            plateMat.SetFloat("_Metallic", 0.5f);
            mountPlate.GetComponent<MeshRenderer>().material = plateMat;
            Destroy(mountPlate.GetComponent<Collider>());

            // --- Light dome (hemisphere approximated by a scaled sphere, bottom half visible) ---
            if (domeRenderer == null)
            {
                GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dome.name = "LightDome";
                dome.transform.SetParent(transform, false);
                // Position dome below junction box; sphere center offset so top half is hidden
                float domeYOffset = -junctionBoxSize / 2f - 0.005f - domeHeight * 0.3f;
                dome.transform.localPosition = new Vector3(0f, domeYOffset, 0f);
                dome.transform.localScale = new Vector3(domeDiameter, domeHeight, domeDiameter);

                domeRenderer = dome.GetComponent<MeshRenderer>();
                domeMaterial = new Material(Shader.Find("Standard"));
                domeMaterial.color = domeColorOff;
                // Enable emission keyword so we can toggle it later
                domeMaterial.EnableKeyword("_EMISSION");
                domeMaterial.SetColor("_EmissionColor", Color.black);
                domeRenderer.material = domeMaterial;

                // Remove collider from dome; parent object handles collision
                Destroy(dome.GetComponent<Collider>());
            }

            // --- Bulb socket indicator (small cylinder inside dome) ---
            GameObject socket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            socket.name = "BulbSocket";
            socket.transform.SetParent(transform, false);
            float socketY = -junctionBoxSize / 2f - 0.01f;
            socket.transform.localPosition = new Vector3(0f, socketY, 0f);
            socket.transform.localScale = new Vector3(0.02f, 0.015f, 0.02f);

            Material socketMat = new Material(Shader.Find("Standard"));
            socketMat.color = new Color(0.72f, 0.58f, 0.20f, 1f); // Brass socket
            socketMat.SetFloat("_Metallic", 0.8f);
            socket.GetComponent<MeshRenderer>().material = socketMat;
            Destroy(socket.GetComponent<Collider>());
        }

        // ---------------------------------------------------------------
        // Terminal Creation
        // ---------------------------------------------------------------

        private void CreateTerminals()
        {
            float terminalZ = junctionBoxSize / 2f + 0.005f;

            // Hot terminal — accepts hot wire
            if (hotTerminal == null)
            {
                Vector3 hotPos = new Vector3(-0.025f, 0f, terminalZ);
                CreateTerminalScrewVisual(hotPos, new Color(0.72f, 0.58f, 0.20f, 1f)); // Brass

                GameObject hotObj = new GameObject("HotTerminal");
                hotObj.transform.SetParent(transform, false);
                hotObj.transform.localPosition = hotPos;

                hotTerminal = hotObj.AddComponent<SnapPoint>();
                hotTerminal.SetAcceptedWireType("hot");
                hotTerminal.SetAmpRating(0);
                hotTerminal.SetSnapPointId("fixture-hot");
                hotTerminal.SetLabel("Hot (Black)");
            }

            // Neutral terminal — accepts neutral wire
            if (neutralTerminal == null)
            {
                Vector3 neutralPos = new Vector3(0.025f, 0f, terminalZ);
                CreateTerminalScrewVisual(neutralPos, new Color(0.78f, 0.78f, 0.80f, 1f)); // Silver

                GameObject neutralObj = new GameObject("NeutralTerminal");
                neutralObj.transform.SetParent(transform, false);
                neutralObj.transform.localPosition = neutralPos;

                neutralTerminal = neutralObj.AddComponent<SnapPoint>();
                neutralTerminal.SetAcceptedWireType("neutral");
                neutralTerminal.SetAmpRating(0);
                neutralTerminal.SetSnapPointId("fixture-neutral");
                neutralTerminal.SetLabel("Neutral (White)");
            }

            // Ground terminal — accepts ground wire
            if (groundTerminal == null)
            {
                Vector3 groundPos = new Vector3(0f, -junctionBoxSize / 2f + 0.01f, terminalZ);
                CreateTerminalScrewVisual(groundPos, new Color(0.18f, 0.55f, 0.18f, 1f)); // Green

                GameObject groundObj = new GameObject("GroundTerminal");
                groundObj.transform.SetParent(transform, false);
                groundObj.transform.localPosition = groundPos;

                groundTerminal = groundObj.AddComponent<SnapPoint>();
                groundTerminal.SetAcceptedWireType("ground");
                groundTerminal.SetAmpRating(0);
                groundTerminal.SetSnapPointId("fixture-ground");
                groundTerminal.SetLabel("Ground (Green)");
            }
        }

        private void CreateTerminalScrewVisual(Vector3 localPos, Color screwColor)
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
        }

        // ---------------------------------------------------------------
        // Power Control
        // ---------------------------------------------------------------

        /// <summary>
        /// Toggles the light fixture on or off.
        /// When powered: dome material emits warm white (1, 0.95, 0.8) at intensity 2.
        /// When off: dark gray material with no emission.
        /// </summary>
        public void SetPowered(bool powered)
        {
            isPowered = powered;
            ApplyPowerState();

            Debug.Log($"[LightFixture] {(isPowered ? "ON — warm white emission" : "OFF")}");
        }

        private void ApplyPowerState()
        {
            if (domeMaterial == null) return;

            if (isPowered)
            {
                domeMaterial.color = domeColorOn;
                domeMaterial.EnableKeyword("_EMISSION");
                domeMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            }
            else
            {
                domeMaterial.color = domeColorOff;
                domeMaterial.SetColor("_EmissionColor", Color.black);
            }
        }

        // ---------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------

        /// <summary>
        /// Checks that all required terminals (hot, neutral, ground) are occupied.
        /// </summary>
        public bool ValidateConnections()
        {
            bool valid = true;

            if (hotTerminal == null || !hotTerminal.IsOccupied)
            {
                Debug.LogWarning("[LightFixture] Hot terminal not connected");
                valid = false;
            }

            if (neutralTerminal == null || !neutralTerminal.IsOccupied)
            {
                Debug.LogWarning("[LightFixture] Neutral terminal not connected");
                valid = false;
            }

            if (groundTerminal == null || !groundTerminal.IsOccupied)
            {
                Debug.LogWarning("[LightFixture] Ground terminal not connected — NEC 250.110");
                valid = false;
            }

            return valid;
        }

        public bool AreAllTerminalsConnected()
        {
            return hotTerminal != null && hotTerminal.IsOccupied &&
                   neutralTerminal != null && neutralTerminal.IsOccupied &&
                   groundTerminal != null && groundTerminal.IsOccupied;
        }

        public string GetNECReference()
        {
            return "NEC 410.10: Luminaires shall be installed per listing, grounded per 410.44";
        }
    }
}
