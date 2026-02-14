using UnityEngine;
using System.Collections.Generic;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents a circuit breaker in the electrical panel.
    /// Handles amperage rating, wire connections, and double-tap detection.
    /// Visual representation includes a toggle switch and wire terminal.
    /// </summary>
    public class CircuitBreaker : MonoBehaviour
    {
        [Header("Breaker Properties")]
        [SerializeField] private int amperageRating = 20;
        [SerializeField] private int slotIndex = -1;
        [SerializeField] private string circuitLabel = "";
        [SerializeField] private BreakerState state = BreakerState.On;
        [SerializeField] private BreakerType breakerType = BreakerType.SinglePole;

        [Header("Wire Terminal")]
        [SerializeField] private SnapPoint wireTerminal;
        [SerializeField] private bool allowsDoubleTap = false; // Most residential breakers do NOT
        private List<Training.WireSegment> connectedWires = new List<Training.WireSegment>();

        [Header("Visual")]
        [SerializeField] private MeshRenderer breakerBody;
        [SerializeField] private Transform toggleSwitch;
        [SerializeField] private Color breakerColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        [SerializeField] private float breakerWidth = 0.018f;   // ~0.7 inches
        [SerializeField] private float breakerHeight = 0.076f;  // ~3 inches
        [SerializeField] private float breakerDepth = 0.06f;    // ~2.5 inches

        [Header("Label")]
        [SerializeField] private TMPro.TextMeshPro amperageLabel;

        public int AmperageRating => amperageRating;
        public int SlotIndex => slotIndex;
        public string CircuitLabel => circuitLabel;
        public BreakerState State => state;
        public bool IsDoubleTapped => connectedWires.Count > 1 && !allowsDoubleTap;
        public bool AllowsDoubleTap => allowsDoubleTap;
        public int ConnectedWireCount => connectedWires.Count;
        public SnapPoint WireTerminal => wireTerminal;

        public enum BreakerState
        {
            On,
            Off,
            Tripped
        }

        public enum BreakerType
        {
            SinglePole,
            DoublePole,
            GFCI,
            AFCI,
            DualFunction // AFCI/GFCI
        }

        public void Initialize(int amperage, int slot)
        {
            amperageRating = amperage;
            slotIndex = slot;
            BuildVisual();
            CreateWireTerminal();
        }

        private void BuildVisual()
        {
            // Create breaker body
            if (breakerBody == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "BreakerBody";
                body.transform.SetParent(transform, false);
                body.transform.localPosition = Vector3.zero;
                body.transform.localScale = new Vector3(breakerWidth, breakerHeight, breakerDepth);

                breakerBody = body.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = breakerColor;
                breakerBody.material = mat;
            }

            // Create toggle switch
            if (toggleSwitch == null)
            {
                GameObject toggle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                toggle.name = "ToggleSwitch";
                toggle.transform.SetParent(transform, false);
                toggle.transform.localPosition = new Vector3(0f, 0f, breakerDepth * -0.5f - 0.005f);
                toggle.transform.localScale = new Vector3(breakerWidth * 0.6f, 0.02f, 0.01f);

                Material toggleMat = new Material(Shader.Find("Standard"));
                toggleMat.color = state == BreakerState.On ? Color.red : Color.black;
                toggle.GetComponent<MeshRenderer>().material = toggleMat;

                toggleSwitch = toggle.transform;

                // Add collider for interaction
                BoxCollider toggleCol = toggle.GetComponent<BoxCollider>();
                if (toggleCol != null)
                {
                    toggleCol.isTrigger = true;
                }
            }

            // Create amperage label
            CreateAmperageLabel();
        }

        private void CreateAmperageLabel()
        {
            if (amperageLabel != null) return;

            GameObject labelObj = new GameObject("AmperageLabel");
            labelObj.transform.SetParent(transform, false);
            labelObj.transform.localPosition = new Vector3(0f, breakerHeight * 0.3f, breakerDepth * -0.5f - 0.002f);
            labelObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            amperageLabel = labelObj.AddComponent<TMPro.TextMeshPro>();
            amperageLabel.text = $"{amperageRating}A";
            amperageLabel.fontSize = 1f;
            amperageLabel.alignment = TMPro.TextAlignmentOptions.Center;
            amperageLabel.color = Color.white;
            amperageLabel.rectTransform.sizeDelta = new Vector2(breakerWidth, 0.015f);
        }

        private void CreateWireTerminal()
        {
            if (wireTerminal != null) return;

            GameObject terminalObj = new GameObject("WireTerminal");
            terminalObj.transform.SetParent(transform, false);
            terminalObj.transform.localPosition = new Vector3(0f, breakerHeight * 0.4f, 0f);

            wireTerminal = terminalObj.AddComponent<SnapPoint>();
            wireTerminal.SetAcceptedWireType("hot");
            wireTerminal.SetAmpRating(amperageRating);
            wireTerminal.SetSnapPointId($"breaker-{slotIndex}-terminal");
        }

        // --- Operations ---

        public void Toggle()
        {
            if (state == BreakerState.On)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        public void TurnOn()
        {
            state = BreakerState.On;
            UpdateToggleVisual();
            Debug.Log($"[CircuitBreaker] Slot {slotIndex} ({amperageRating}A) turned ON");
        }

        public void TurnOff()
        {
            state = BreakerState.Off;
            UpdateToggleVisual();
            Debug.Log($"[CircuitBreaker] Slot {slotIndex} ({amperageRating}A) turned OFF");
        }

        public void Trip()
        {
            state = BreakerState.Tripped;
            UpdateToggleVisual();
            Debug.Log($"[CircuitBreaker] Slot {slotIndex} ({amperageRating}A) TRIPPED");
        }

        private void UpdateToggleVisual()
        {
            if (toggleSwitch == null) return;

            MeshRenderer toggleRenderer = toggleSwitch.GetComponent<MeshRenderer>();

            switch (state)
            {
                case BreakerState.On:
                    toggleSwitch.localRotation = Quaternion.Euler(-15f, 0f, 0f);
                    if (toggleRenderer != null) toggleRenderer.material.color = Color.red;
                    break;
                case BreakerState.Off:
                    toggleSwitch.localRotation = Quaternion.Euler(15f, 0f, 0f);
                    if (toggleRenderer != null) toggleRenderer.material.color = Color.black;
                    break;
                case BreakerState.Tripped:
                    toggleSwitch.localRotation = Quaternion.Euler(0f, 0f, 0f); // Center position
                    if (toggleRenderer != null) toggleRenderer.material.color = new Color(1f, 0.5f, 0f);
                    break;
            }
        }

        // --- Wire Connections ---

        public bool ConnectWire(Training.WireSegment wire)
        {
            if (connectedWires.Contains(wire)) return false;

            connectedWires.Add(wire);

            if (IsDoubleTapped)
            {
                Debug.LogWarning($"[CircuitBreaker] NEC 408.41 VIOLATION — Double-tapped breaker at slot {slotIndex}");
            }

            // Validate wire gauge
            if (!wire.ValidateGaugeForAmperage(amperageRating))
            {
                Debug.LogWarning($"[CircuitBreaker] NEC 310.16 VIOLATION — {wire.WireGaugeAWG} AWG wire on {amperageRating}A breaker");
            }

            return true;
        }

        public void DisconnectWire(Training.WireSegment wire)
        {
            connectedWires.Remove(wire);
        }

        public void DisconnectAll()
        {
            connectedWires.Clear();
        }

        // --- Label ---

        public void SetCircuitLabel(string label)
        {
            circuitLabel = label;
        }

        // --- Validation ---

        public bool ValidateWireGauge()
        {
            foreach (var wire in connectedWires)
            {
                if (!wire.ValidateGaugeForAmperage(amperageRating))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetRequiredMinGauge()
        {
            // NEC 310.16 simplified
            if (amperageRating <= 15) return 14;
            if (amperageRating <= 20) return 12;
            if (amperageRating <= 30) return 10;
            if (amperageRating <= 40) return 8;
            if (amperageRating <= 55) return 6;
            return 4;
        }
    }
}
