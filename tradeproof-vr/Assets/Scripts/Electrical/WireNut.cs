using UnityEngine;
using System.Collections.Generic;
using TradeProof.Interaction;
using TradeProof.Training;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Wire connector (wire nut) that joins 2-4 wires together.
    /// Has a GrabInteractable component for VR interaction.
    /// Visual: cone shape (scaled sphere top + cylinder body), colored by size.
    ///   Yellow: 2 wires (14 or 12 AWG)
    ///   Red: 3 wires
    ///   Tan: 4 wires
    /// Dimensions: approximately 0.025m tall, 0.015m diameter.
    /// Provides visual glow feedback when wires are properly connected.
    /// </summary>
    public class WireNut : MonoBehaviour
    {
        [Header("Wire Nut Properties")]
        [SerializeField] private WireNutSize nutSize = WireNutSize.Yellow;
        [SerializeField] private int maxWires = 2;

        [Header("Dimensions (meters)")]
        [SerializeField] private float nutHeight = 0.025f;
        [SerializeField] private float nutDiameter = 0.015f;

        [Header("Connected Wires")]
        [SerializeField] private List<WireSegment> connectedWires = new List<WireSegment>();

        [Header("Visual")]
        [SerializeField] private MeshRenderer capRenderer;
        [SerializeField] private MeshRenderer bodyRenderer;
        private Material capMaterial;
        private Material bodyMaterial;
        private GrabInteractable grabInteractable;

        [Header("Glow Feedback")]
        [SerializeField] private bool showConnectionGlow;
        private float glowPulseTimer;

        public enum WireNutSize
        {
            Yellow, // 2 wires (14 AWG or 12 AWG)
            Red,    // 3 wires
            Tan     // 4 wires
        }

        public WireNutSize Size => nutSize;
        public int MaxWires => maxWires;
        public bool IsConnected => connectedWires.Count >= 2;
        public List<WireSegment> ConnectedWires => new List<WireSegment>(connectedWires);
        public int ConnectedWireCount => connectedWires.Count;

        private void Awake()
        {
            ConfigureSizeProperties();
            BuildNutVisual();
            SetupGrabInteractable();
        }

        private void Update()
        {
            UpdateGlowFeedback();
        }

        // ---------------------------------------------------------------
        // Configuration
        // ---------------------------------------------------------------

        private void ConfigureSizeProperties()
        {
            switch (nutSize)
            {
                case WireNutSize.Yellow:
                    maxWires = 2;
                    nutHeight = 0.022f;
                    nutDiameter = 0.013f;
                    break;
                case WireNutSize.Red:
                    maxWires = 3;
                    nutHeight = 0.025f;
                    nutDiameter = 0.015f;
                    break;
                case WireNutSize.Tan:
                    maxWires = 4;
                    nutHeight = 0.028f;
                    nutDiameter = 0.017f;
                    break;
            }
        }

        // ---------------------------------------------------------------
        // Visual Construction
        // ---------------------------------------------------------------

        private void BuildNutVisual()
        {
            Color nutColor = GetNutColor();

            // --- Cap (top cone approximated by a scaled sphere) ---
            if (capRenderer == null)
            {
                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cap.name = "WireNutCap";
                cap.transform.SetParent(transform, false);
                cap.transform.localPosition = new Vector3(0f, nutHeight * 0.35f, 0f);
                cap.transform.localScale = new Vector3(nutDiameter, nutHeight * 0.5f, nutDiameter);

                capRenderer = cap.GetComponent<MeshRenderer>();
                capMaterial = new Material(Shader.Find("Standard"));
                capMaterial.color = nutColor;
                capMaterial.EnableKeyword("_EMISSION");
                capMaterial.SetColor("_EmissionColor", Color.black);
                capRenderer.material = capMaterial;

                // Remove collider from visual; parent handles collision
                Destroy(cap.GetComponent<Collider>());
            }

            // --- Body (cylinder base) ---
            if (bodyRenderer == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                body.name = "WireNutBody";
                body.transform.SetParent(transform, false);
                body.transform.localPosition = new Vector3(0f, -nutHeight * 0.15f, 0f);
                body.transform.localScale = new Vector3(nutDiameter * 0.85f, nutHeight * 0.35f, nutDiameter * 0.85f);

                bodyRenderer = body.GetComponent<MeshRenderer>();
                bodyMaterial = new Material(Shader.Find("Standard"));
                bodyMaterial.color = nutColor;
                bodyMaterial.EnableKeyword("_EMISSION");
                bodyMaterial.SetColor("_EmissionColor", Color.black);
                bodyRenderer.material = bodyMaterial;

                // Remove collider from visual
                Destroy(body.GetComponent<Collider>());
            }

            // --- Ribbed texture lines (decorative rings on the body) ---
            int ribCount = 4;
            float ribSpacing = nutHeight * 0.6f / (ribCount + 1);
            for (int i = 0; i < ribCount; i++)
            {
                float yPos = nutHeight * 0.1f - (i + 1) * ribSpacing;
                GameObject rib = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rib.name = $"Rib_{i}";
                rib.transform.SetParent(transform, false);
                rib.transform.localPosition = new Vector3(0f, yPos, 0f);
                rib.transform.localScale = new Vector3(nutDiameter * 0.90f, 0.0005f, nutDiameter * 0.90f);

                Material ribMat = new Material(Shader.Find("Standard"));
                ribMat.color = nutColor * 0.85f; // Slightly darker for contrast
                rib.GetComponent<MeshRenderer>().material = ribMat;
                Destroy(rib.GetComponent<Collider>());
            }

            // --- Wire opening at bottom (small dark cylinder) ---
            GameObject opening = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            opening.name = "WireOpening";
            opening.transform.SetParent(transform, false);
            opening.transform.localPosition = new Vector3(0f, -nutHeight * 0.45f, 0f);
            opening.transform.localScale = new Vector3(nutDiameter * 0.5f, 0.002f, nutDiameter * 0.5f);

            Material openingMat = new Material(Shader.Find("Standard"));
            openingMat.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            opening.GetComponent<MeshRenderer>().material = openingMat;
            Destroy(opening.GetComponent<Collider>());
        }

        private Color GetNutColor()
        {
            switch (nutSize)
            {
                case WireNutSize.Yellow:
                    return new Color(0.95f, 0.85f, 0.15f, 1f); // Yellow
                case WireNutSize.Red:
                    return new Color(0.85f, 0.15f, 0.12f, 1f); // Red
                case WireNutSize.Tan:
                    return new Color(0.82f, 0.72f, 0.55f, 1f); // Tan
                default:
                    return Color.gray;
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

            grabInteractable.SetToolType("wirenut");
            grabInteractable.SetGripOffset(new Vector3(0f, -0.01f, 0f));

            // Ensure collider exists for grab detection
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
                capsule.radius = nutDiameter / 2f;
                capsule.height = nutHeight;
                capsule.center = Vector3.zero;
            }
        }

        // ---------------------------------------------------------------
        // Wire Attachment
        // ---------------------------------------------------------------

        /// <summary>
        /// Attaches an array of wires to this wire nut.
        /// Validates gauge compatibility and maximum wire count.
        /// Returns true if all wires were successfully attached.
        /// </summary>
        public bool AttachWires(WireSegment[] wires)
        {
            if (wires == null || wires.Length == 0)
            {
                Debug.LogWarning("[WireNut] No wires provided");
                return false;
            }

            if (wires.Length > maxWires)
            {
                Debug.LogWarning($"[WireNut] Too many wires ({wires.Length}) for {nutSize} wire nut (max {maxWires})");
                return false;
            }

            if (connectedWires.Count + wires.Length > maxWires)
            {
                Debug.LogWarning($"[WireNut] Adding {wires.Length} wires would exceed capacity " +
                                 $"({connectedWires.Count + wires.Length}/{maxWires})");
                return false;
            }

            // Validate gauge compatibility — all wires should be same gauge or compatible
            if (!ValidateGaugeCompatibility(wires))
            {
                Debug.LogWarning("[WireNut] Wire gauge mismatch — all wires should be compatible gauges");
                return false;
            }

            foreach (var wire in wires)
            {
                if (wire != null && !connectedWires.Contains(wire))
                {
                    connectedWires.Add(wire);
                }
            }

            showConnectionGlow = IsConnected;

            Debug.Log($"[WireNut] {connectedWires.Count} wires attached ({nutSize})");
            return true;
        }

        /// <summary>
        /// Detaches all wires from this wire nut.
        /// </summary>
        public void DetachWires()
        {
            connectedWires.Clear();
            showConnectionGlow = false;

            // Reset glow
            if (capMaterial != null)
            {
                capMaterial.SetColor("_EmissionColor", Color.black);
            }
            if (bodyMaterial != null)
            {
                bodyMaterial.SetColor("_EmissionColor", Color.black);
            }

            Debug.Log("[WireNut] All wires detached");
        }

        /// <summary>
        /// Detaches a specific wire from this wire nut.
        /// </summary>
        public void DetachWire(WireSegment wire)
        {
            if (wire != null && connectedWires.Contains(wire))
            {
                connectedWires.Remove(wire);
                showConnectionGlow = IsConnected;

                Debug.Log($"[WireNut] Wire detached — {connectedWires.Count} remaining");
            }
        }

        // ---------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------

        /// <summary>
        /// Validates that all provided wires have compatible gauges.
        /// Wire nuts can join 14 AWG and 12 AWG together, but not wildly different gauges.
        /// </summary>
        private bool ValidateGaugeCompatibility(WireSegment[] wires)
        {
            if (wires.Length <= 1) return true;

            int minGauge = int.MaxValue;
            int maxGauge = int.MinValue;

            foreach (var wire in wires)
            {
                if (wire == null) continue;

                int gauge = wire.WireGaugeAWG;
                if (gauge < minGauge) minGauge = gauge;
                if (gauge > maxGauge) maxGauge = gauge;
            }

            // Also check against already connected wires
            foreach (var wire in connectedWires)
            {
                if (wire == null) continue;

                int gauge = wire.WireGaugeAWG;
                if (gauge < minGauge) minGauge = gauge;
                if (gauge > maxGauge) maxGauge = gauge;
            }

            // Allow up to 2 gauge sizes difference (e.g., 14 and 12 OK, but not 14 and 8)
            int gaugeDifference = maxGauge - minGauge;
            return gaugeDifference <= 2;
        }

        /// <summary>
        /// Validates the wire nut connection is complete and properly sized.
        /// </summary>
        public bool ValidateConnection()
        {
            if (connectedWires.Count < 2)
            {
                Debug.LogWarning("[WireNut] Fewer than 2 wires — connection incomplete");
                return false;
            }

            if (connectedWires.Count > maxWires)
            {
                Debug.LogWarning($"[WireNut] Too many wires ({connectedWires.Count}) for {nutSize} wire nut");
                return false;
            }

            return true;
        }

        // ---------------------------------------------------------------
        // Visual Feedback
        // ---------------------------------------------------------------

        private void UpdateGlowFeedback()
        {
            if (!showConnectionGlow) return;

            glowPulseTimer += Time.deltaTime;
            float pulse = Mathf.Sin(glowPulseTimer * 2f) * 0.15f + 0.25f;
            Color glowColor = GetNutColor() * pulse;

            if (capMaterial != null)
            {
                capMaterial.SetColor("_EmissionColor", glowColor);
            }
            if (bodyMaterial != null)
            {
                bodyMaterial.SetColor("_EmissionColor", glowColor);
            }
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>
        /// Sets the wire nut size and reconfigures visuals.
        /// </summary>
        public void SetSize(WireNutSize size)
        {
            nutSize = size;
            ConfigureSizeProperties();

            // Update visual colors
            Color nutColor = GetNutColor();
            if (capMaterial != null) capMaterial.color = nutColor;
            if (bodyMaterial != null) bodyMaterial.color = nutColor;
        }

        public string GetDescription()
        {
            return $"{nutSize} wire nut — {connectedWires.Count}/{maxWires} wires connected";
        }

        public string GetNECReference()
        {
            return "NEC 110.14(B): Splices shall be made with listed devices (wire connectors)";
        }
    }
}
