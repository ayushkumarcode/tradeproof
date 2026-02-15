using UnityEngine;
using System.Collections.Generic;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// GFCI (Ground Fault Circuit Interrupter) outlet extending the existing Outlet class.
    /// Adds TEST button (red), RESET button (black), and indicator LED.
    /// Tracks tripped state and downstream outlets protected by this GFCI.
    /// Test() trips the GFCI, cutting power to self and all downstream outlets.
    /// Reset() restores power if line-side has power.
    /// Validates GFCI functionality per NEC 210.8.
    /// </summary>
    public class GFCIOutlet : Outlet
    {
        [Header("GFCI Properties")]
        [SerializeField] private bool isTripped;
        [SerializeField] private bool isFaulty;
        [SerializeField] private List<Outlet> downstreamOutlets = new List<Outlet>();

        [Header("GFCI Visual")]
        [SerializeField] private GameObject testButtonObj;
        [SerializeField] private GameObject resetButtonObj;
        [SerializeField] private GameObject indicatorLedObj;
        [SerializeField] private MeshRenderer testButtonRenderer;
        [SerializeField] private MeshRenderer resetButtonRenderer;
        [SerializeField] private MeshRenderer indicatorLedRenderer;

        [Header("GFCI Colors")]
        [SerializeField] private Color testButtonColor = new Color(0.8f, 0.1f, 0.1f, 1f);    // Red
        [SerializeField] private Color resetButtonColor = new Color(0.15f, 0.15f, 0.15f, 1f); // Black
        [SerializeField] private Color ledOnColor = new Color(0.1f, 1f, 0.1f, 1f);            // Green (powered)
        [SerializeField] private Color ledOffColor = new Color(0.3f, 0.3f, 0.3f, 1f);         // Gray (tripped/no power)
        [SerializeField] private Color ledFaultColor = new Color(1f, 0.1f, 0.1f, 1f);         // Red (fault)

        [Header("GFCI Dimensions")]
        [SerializeField] private float buttonWidth = 0.012f;
        [SerializeField] private float buttonHeight = 0.008f;
        [SerializeField] private float buttonDepth = 0.005f;
        [SerializeField] private float ledDiameter = 0.004f;

        // State
        private float testButtonDefaultZ;
        private float resetButtonDefaultZ;

        public bool IsTripped => isTripped;
        public bool IsFaulty
        {
            get { return isFaulty; }
            set { isFaulty = value; UpdateIndicatorLed(); }
        }
        public List<Outlet> DownstreamOutlets => downstreamOutlets;

        // NOTE: No Awake() defined here. Unity will call the base Outlet.Awake()
        // which builds the outlet visual and terminals. GFCI-specific elements are built in Start()
        // to ensure the base outlet is fully constructed first.

        private void Start()
        {
            BuildGFCIVisual();
            UpdateIndicatorLed();
        }

        private void BuildGFCIVisual()
        {
            float faceZ = -0.005f; // Slightly in front of the outlet face

            // --- TEST button (red, small cube) ---
            if (testButtonObj == null)
            {
                testButtonObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testButtonObj.name = "GFCI_TestButton";
                testButtonObj.transform.SetParent(transform, false);
                testButtonObj.transform.localPosition = new Vector3(-0.012f, 0f, faceZ);
                testButtonObj.transform.localScale = new Vector3(buttonWidth, buttonHeight, buttonDepth);

                testButtonRenderer = testButtonObj.GetComponent<MeshRenderer>();
                Material testMat = new Material(Shader.Find("Standard"));
                testMat.color = testButtonColor;
                testButtonRenderer.material = testMat;

                // Button collider for interaction
                BoxCollider testCol = testButtonObj.GetComponent<BoxCollider>();
                if (testCol != null)
                {
                    testCol.isTrigger = true;
                }

                testButtonDefaultZ = faceZ;
            }

            // --- RESET button (black, small cube) ---
            if (resetButtonObj == null)
            {
                resetButtonObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                resetButtonObj.name = "GFCI_ResetButton";
                resetButtonObj.transform.SetParent(transform, false);
                resetButtonObj.transform.localPosition = new Vector3(0.012f, 0f, faceZ);
                resetButtonObj.transform.localScale = new Vector3(buttonWidth, buttonHeight, buttonDepth);

                resetButtonRenderer = resetButtonObj.GetComponent<MeshRenderer>();
                Material resetMat = new Material(Shader.Find("Standard"));
                resetMat.color = resetButtonColor;
                resetButtonRenderer.material = resetMat;

                // Button collider for interaction
                BoxCollider resetCol = resetButtonObj.GetComponent<BoxCollider>();
                if (resetCol != null)
                {
                    resetCol.isTrigger = true;
                }

                resetButtonDefaultZ = faceZ;
            }

            // --- Indicator LED (small sphere between buttons) ---
            if (indicatorLedObj == null)
            {
                indicatorLedObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indicatorLedObj.name = "GFCI_IndicatorLED";
                indicatorLedObj.transform.SetParent(transform, false);
                indicatorLedObj.transform.localPosition = new Vector3(0f, 0f, faceZ - 0.001f);
                indicatorLedObj.transform.localScale = Vector3.one * ledDiameter;

                indicatorLedRenderer = indicatorLedObj.GetComponent<MeshRenderer>();
                Material ledMat = new Material(Shader.Find("Standard"));
                ledMat.color = ledOnColor;
                ledMat.EnableKeyword("_EMISSION");
                ledMat.SetColor("_EmissionColor", ledOnColor * 1.5f);
                indicatorLedRenderer.material = ledMat;

                // Remove LED collider (visual only)
                Collider ledCol = indicatorLedObj.GetComponent<Collider>();
                if (ledCol != null) Destroy(ledCol);
            }

            // --- Button labels ---
            CreateButtonLabel("TEST", testButtonObj.transform, new Vector3(0f, buttonHeight / 2f + 0.003f, 0f));
            CreateButtonLabel("RESET", resetButtonObj.transform, new Vector3(0f, buttonHeight / 2f + 0.003f, 0f));
        }

        private void CreateButtonLabel(string text, Transform parent, Vector3 localOffset)
        {
            GameObject labelObj = new GameObject($"Label_{text}");
            labelObj.transform.SetParent(parent, false);
            labelObj.transform.localPosition = localOffset;
            labelObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TMPro.TextMeshPro label = labelObj.AddComponent<TMPro.TextMeshPro>();
            label.text = text;
            label.fontSize = 0.8f;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.color = Color.white;
            label.rectTransform.sizeDelta = new Vector2(0.02f, 0.005f);
        }

        /// <summary>
        /// Tests the GFCI by tripping it. Cuts power to self and all downstream outlets.
        /// This simulates pressing the TEST button on a real GFCI outlet.
        /// </summary>
        public void Test()
        {
            SetTripped(true);
            Debug.Log("[GFCIOutlet] TEST pressed — GFCI tripped.");
        }

        /// <summary>
        /// Resets the GFCI, restoring power if line-side has power.
        /// This simulates pressing the RESET button on a real GFCI outlet.
        /// </summary>
        public void Reset()
        {
            // Can only reset if line-side power is available
            bool hasLinePower = HotTerminal != null && HotTerminal.IsOccupied;

            if (hasLinePower)
            {
                SetTripped(false);
                Debug.Log("[GFCIOutlet] RESET pressed — GFCI restored.");
            }
            else
            {
                Debug.LogWarning("[GFCIOutlet] RESET failed — no line-side power available.");
            }
        }

        /// <summary>
        /// Sets the tripped state of the GFCI outlet.
        /// When tripped: TEST button pops out visually, LED turns off, downstream outlets lose power.
        /// When reset: buttons return to normal, LED shows green.
        /// </summary>
        public void SetTripped(bool tripped)
        {
            isTripped = tripped;

            // Update TEST button visual (pops out when tripped)
            if (testButtonObj != null)
            {
                float zOffset = tripped ? -buttonDepth * 0.6f : 0f;
                Vector3 pos = testButtonObj.transform.localPosition;
                pos.z = testButtonDefaultZ + zOffset;
                testButtonObj.transform.localPosition = pos;
            }

            // Update RESET button visual
            if (resetButtonObj != null)
            {
                float zOffset = tripped ? buttonDepth * 0.3f : 0f;
                Vector3 pos = resetButtonObj.transform.localPosition;
                pos.z = resetButtonDefaultZ + zOffset;
                resetButtonObj.transform.localPosition = pos;
            }

            // Update indicator LED
            UpdateIndicatorLed();

            // Propagate to downstream outlets
            foreach (Outlet downstream in downstreamOutlets)
            {
                if (downstream == null) continue;

                GFCIOutlet downstreamGFCI = downstream as GFCIOutlet;
                if (downstreamGFCI != null)
                {
                    // If downstream is also a GFCI, trip it as well
                    downstreamGFCI.SetTripped(tripped);
                }

                // Log downstream status
                Debug.Log($"[GFCIOutlet] Downstream outlet {downstream.name} power {(tripped ? "CUT" : "RESTORED")}.");
            }
        }

        /// <summary>
        /// Updates the indicator LED based on current state.
        /// </summary>
        private void UpdateIndicatorLed()
        {
            if (indicatorLedRenderer == null) return;

            Material ledMat = indicatorLedRenderer.material;
            Color targetColor;

            if (isFaulty)
            {
                targetColor = ledFaultColor; // Red — faulty GFCI
            }
            else if (isTripped)
            {
                targetColor = ledOffColor; // Gray — tripped/no power
            }
            else
            {
                targetColor = ledOnColor; // Green — powered and functional
            }

            ledMat.color = targetColor;
            ledMat.SetColor("_EmissionColor", targetColor * 1.5f);
        }

        /// <summary>
        /// Adds a downstream outlet to be protected by this GFCI.
        /// Per NEC 210.8, all outlets downstream of a GFCI are protected.
        /// </summary>
        public void AddDownstreamOutlet(Outlet outlet)
        {
            if (outlet == null || outlet == this) return;

            if (!downstreamOutlets.Contains(outlet))
            {
                downstreamOutlets.Add(outlet);
                Debug.Log($"[GFCIOutlet] Added downstream outlet: {outlet.name}. Total protected: {downstreamOutlets.Count}.");
            }
        }

        /// <summary>
        /// Removes a downstream outlet from GFCI protection.
        /// </summary>
        public void RemoveDownstreamOutlet(Outlet outlet)
        {
            if (downstreamOutlets.Remove(outlet))
            {
                Debug.Log($"[GFCIOutlet] Removed downstream outlet: {outlet.name}. Total protected: {downstreamOutlets.Count}.");
            }
        }

        /// <summary>
        /// Returns true if this GFCI outlet is providing power (not tripped and line-side connected).
        /// </summary>
        public bool IsProvidingPower()
        {
            if (isTripped) return false;
            if (isFaulty) return false;
            return HotTerminal != null && HotTerminal.IsOccupied;
        }

        /// <summary>
        /// Validates outlet installation per NEC 210.21 AND GFCI functionality per NEC 210.8.
        /// NEC 210.8 requires GFCI protection in kitchens, bathrooms, garages, outdoors,
        /// laundry areas, and within 6 feet of sinks.
        /// </summary>
        public new bool ValidateForCircuit(int circuitAmperage)
        {
            // First validate base outlet requirements (NEC 210.21)
            bool baseValid = base.ValidateForCircuit(circuitAmperage);

            if (!baseValid)
            {
                Debug.LogWarning("[GFCIOutlet] NEC 210.21 — Base outlet validation failed.");
                return false;
            }

            // Validate GFCI-specific requirements
            if (isFaulty)
            {
                Debug.LogWarning("[GFCIOutlet] NEC 210.8 — GFCI is faulty and does not provide ground fault protection.");
                return false;
            }

            // Test that the GFCI can actually trip
            if (!CanTrip())
            {
                Debug.LogWarning("[GFCIOutlet] NEC 210.8 — GFCI does not trip when tested. Replace immediately.");
                return false;
            }

            // Validate all terminals connected
            if (!AreAllTerminalsConnected())
            {
                Debug.LogWarning("[GFCIOutlet] NEC 210.8 — GFCI terminals not fully connected.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Simulates the GFCI self-test. A functioning GFCI must be able to trip.
        /// Returns false if the GFCI is faulty and cannot trip.
        /// </summary>
        private bool CanTrip()
        {
            // A faulty GFCI cannot trip
            return !isFaulty;
        }

        /// <summary>
        /// Gets the NEC reference for GFCI requirements.
        /// </summary>
        public new string GetNECReference()
        {
            return $"NEC 210.8: GFCI protection required. {AmperageRating}A GFCI receptacle. " +
                   $"Protecting {downstreamOutlets.Count} downstream outlet(s). " +
                   $"Status: {(isTripped ? "TRIPPED" : "ACTIVE")}, Fault: {(isFaulty ? "YES" : "NO")}";
        }
    }
}
