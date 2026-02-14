using UnityEngine;
using TradeProof.Interaction;
using TradeProof.Training;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents an individual conductor wire in the electrical system.
    /// Wraps WireSegment with additional electrical properties and NEC validation.
    /// Visual states: normal, selected (glow), correct (green pulse), wrong (red pulse).
    /// </summary>
    public class Wire : MonoBehaviour
    {
        [Header("Electrical Properties")]
        [SerializeField] private int gaugeAWG = 12;
        [SerializeField] private WireType wireType = WireType.Hot;
        [SerializeField] private float lengthMeters = 1.5f;
        [SerializeField] private string insulationType = "THHN/THWN";

        [Header("Wire Segment")]
        [SerializeField] private WireSegment segment;

        [Header("Connections")]
        [SerializeField] private SnapPoint sourceConnection; // Panel side
        [SerializeField] private SnapPoint destinationConnection; // Device side

        [Header("State")]
        [SerializeField] private bool isInstalled;
        [SerializeField] private bool isEnergized;
        [SerializeField] private bool hasProperConnection;

        public enum WireType
        {
            Hot,        // Black or Red
            Neutral,    // White
            Ground,     // Green or Bare
            Traveler    // Red (3-way switch)
        }

        public int GaugeAWG => gaugeAWG;
        public WireType Type => wireType;
        public bool IsInstalled => isInstalled;
        public bool IsEnergized => isEnergized;
        public WireSegment Segment => segment;

        private void Awake()
        {
            if (segment == null)
            {
                segment = GetComponent<WireSegment>();
                if (segment == null)
                {
                    segment = gameObject.AddComponent<WireSegment>();
                }
            }
        }

        /// <summary>
        /// Get the standard insulation color for this wire type.
        /// </summary>
        public Color GetInsulationColor()
        {
            switch (wireType)
            {
                case WireType.Hot:
                    return new Color(0.1f, 0.1f, 0.1f); // Black
                case WireType.Neutral:
                    return new Color(0.9f, 0.9f, 0.9f); // White
                case WireType.Ground:
                    return new Color(0.1f, 0.6f, 0.1f); // Green
                case WireType.Traveler:
                    return new Color(0.8f, 0.1f, 0.1f); // Red
                default:
                    return Color.gray;
            }
        }

        /// <summary>
        /// Get maximum ampacity for this wire based on NEC 310.16.
        /// Assumes copper conductor with 75C insulation (THWN).
        /// </summary>
        public int GetAmpacity()
        {
            return WireSegment.GetMaxAmperageForGauge(gaugeAWG);
        }

        /// <summary>
        /// Validate that this wire is correctly sized for the given circuit amperage.
        /// </summary>
        public bool ValidateForCircuit(int circuitAmperage)
        {
            return GetAmpacity() >= circuitAmperage;
        }

        /// <summary>
        /// Mark this wire as installed between two connection points.
        /// </summary>
        public void Install(SnapPoint source, SnapPoint destination)
        {
            sourceConnection = source;
            destinationConnection = destination;
            isInstalled = true;

            if (segment != null)
            {
                segment.TrySnapStart(source);
                segment.TrySnapEnd(destination);
            }

            Debug.Log($"[Wire] {wireType} wire ({gaugeAWG} AWG) installed");
        }

        /// <summary>
        /// Uninstall this wire from its connections.
        /// </summary>
        public void Uninstall()
        {
            if (segment != null)
            {
                segment.DetachAll();
            }

            sourceConnection = null;
            destinationConnection = null;
            isInstalled = false;
            isEnergized = false;

            Debug.Log($"[Wire] {wireType} wire removed");
        }

        /// <summary>
        /// Check if this wire has proper connections at both ends.
        /// </summary>
        public bool CheckConnections()
        {
            if (segment == null) return false;

            hasProperConnection = segment.IsFullyConnected;

            if (hasProperConnection)
            {
                // Validate snap point types
                if (segment.StartSnapPoint != null)
                {
                    bool sourceValid = ValidateSnapPointType(segment.StartSnapPoint);
                    if (!sourceValid) hasProperConnection = false;
                }

                if (segment.EndSnapPoint != null)
                {
                    bool destValid = ValidateSnapPointType(segment.EndSnapPoint);
                    if (!destValid) hasProperConnection = false;
                }
            }

            return hasProperConnection;
        }

        private bool ValidateSnapPointType(SnapPoint sp)
        {
            string accepted = sp.AcceptedWireType;
            if (accepted == "any") return true;

            switch (wireType)
            {
                case WireType.Hot:
                case WireType.Traveler:
                    return accepted == "hot";
                case WireType.Neutral:
                    return accepted == "neutral";
                case WireType.Ground:
                    return accepted == "ground";
                default:
                    return false;
            }
        }

        /// <summary>
        /// Set visual state based on validation results.
        /// </summary>
        public void ShowValidationState(bool isCorrect)
        {
            if (segment == null) return;

            if (isCorrect)
            {
                segment.ShowCorrect();
            }
            else
            {
                segment.ShowWrong();
            }
        }

        /// <summary>
        /// Get a human-readable description of this wire.
        /// </summary>
        public string GetDescription()
        {
            return $"{wireType} ({GetInsulationColorName()}) — {gaugeAWG} AWG, {GetAmpacity()}A max, {lengthMeters:F1}m";
        }

        private string GetInsulationColorName()
        {
            switch (wireType)
            {
                case WireType.Hot: return "Black";
                case WireType.Neutral: return "White";
                case WireType.Ground: return "Green";
                case WireType.Traveler: return "Red";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Get NEC code violations related to this wire.
        /// </summary>
        public string[] GetViolations(int circuitAmperage)
        {
            System.Collections.Generic.List<string> violations = new System.Collections.Generic.List<string>();

            if (!ValidateForCircuit(circuitAmperage))
            {
                violations.Add($"NEC 310.16: {gaugeAWG} AWG wire rated for {GetAmpacity()}A connected to {circuitAmperage}A circuit. Minimum {GetRequiredGaugeForAmperage(circuitAmperage)} AWG required.");
            }

            if (isInstalled && !hasProperConnection)
            {
                violations.Add("NEC 110.14: Wire termination is loose or improperly connected.");
            }

            return violations.ToArray();
        }

        private int GetRequiredGaugeForAmperage(int amps)
        {
            if (amps <= 15) return 14;
            if (amps <= 20) return 12;
            if (amps <= 30) return 10;
            if (amps <= 40) return 8;
            return 6;
        }
    }
}
