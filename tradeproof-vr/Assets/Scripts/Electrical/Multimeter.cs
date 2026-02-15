using UnityEngine;
using TMPro;
using TradeProof.Interaction;
using TradeProof.Core;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Functional multimeter tool for measuring voltage, resistance, and continuity.
    /// Grabbable via GrabInteractable component.
    /// Visual: rectangular body with display face (TextMeshPro), mode dial, and two probe jacks.
    /// Modes: Off, AC Voltage, DC Voltage, Resistance, Continuity.
    /// Takes readings from target components by checking SnapPoint energization state.
    /// Body color: dark yellow (0.9, 0.7, 0.1), display background black.
    /// </summary>
    public class Multimeter : MonoBehaviour
    {
        public enum MultimeterMode
        {
            Off,
            ACVoltage,
            DCVoltage,
            Resistance,
            Continuity
        }

        [Header("Tool Properties")]
        [SerializeField] private string toolType = "multimeter";

        [Header("State")]
        [SerializeField] private MultimeterMode currentMode = MultimeterMode.Off;
        [SerializeField] private float currentReading;

        [Header("Dimensions (meters)")]
        [SerializeField] private float bodyWidth = 0.08f;
        [SerializeField] private float bodyHeight = 0.17f;
        [SerializeField] private float bodyDepth = 0.04f;

        [Header("Visual Colors")]
        [SerializeField] private Color bodyColor = new Color(0.9f, 0.7f, 0.1f, 1f);     // Dark yellow
        [SerializeField] private Color displayBgColor = new Color(0.05f, 0.08f, 0.05f, 1f); // Dark green-black LCD
        [SerializeField] private Color displayTextColor = new Color(0.1f, 0.1f, 0.1f, 1f);  // Dark LCD text
        [SerializeField] private Color redProbeColor = new Color(0.8f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color blackProbeColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Components
        private GrabInteractable grabInteractable;
        private TextMeshPro displayText;

        // Visual references
        private MeshRenderer bodyRenderer;
        private MeshRenderer displayRenderer;
        private Transform dialTransform;
        private Transform redProbeJack;
        private Transform blackProbeJack;

        [Header("Probe Positions")]
        [SerializeField] private Transform redProbeTip;
        [SerializeField] private Transform blackProbeTip;

        public MultimeterMode CurrentMode => currentMode;
        public float CurrentReading => currentReading;
        public string ToolType => toolType;
        public Transform RedProbeTip => redProbeTip;
        public Transform BlackProbeTip => blackProbeTip;

        private void Awake()
        {
            BuildVisual();
            SetupGrabInteractable();
            SetMode(MultimeterMode.Off);
        }

        private void BuildVisual()
        {
            // --- Main body ---
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "MultimeterBody";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(bodyWidth, bodyHeight, bodyDepth);

            bodyRenderer = body.GetComponent<MeshRenderer>();
            Material bodyMat = new Material(Shader.Find("Standard"));
            bodyMat.color = bodyColor;
            bodyMat.SetFloat("_Glossiness", 0.3f);
            bodyRenderer.material = bodyMat;

            // Remove body collider (main collider on parent)
            Collider bodyCol = body.GetComponent<Collider>();
            if (bodyCol != null) Destroy(bodyCol);

            // --- Display face (quad with text) ---
            GameObject display = GameObject.CreatePrimitive(PrimitiveType.Quad);
            display.name = "DisplayFace";
            display.transform.SetParent(transform, false);
            display.transform.localPosition = new Vector3(0f, bodyHeight * 0.2f, -bodyDepth / 2f - 0.001f);
            display.transform.localScale = new Vector3(bodyWidth * 0.75f, bodyHeight * 0.2f, 1f);
            display.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            displayRenderer = display.GetComponent<MeshRenderer>();
            Material displayMat = new Material(Shader.Find("Standard"));
            displayMat.color = displayBgColor;
            displayRenderer.material = displayMat;

            Collider displayCol = display.GetComponent<Collider>();
            if (displayCol != null) Destroy(displayCol);

            // --- Display text (TextMeshPro) ---
            GameObject textObj = new GameObject("DisplayText");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, bodyHeight * 0.2f, -bodyDepth / 2f - 0.003f);
            textObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            displayText = textObj.AddComponent<TextMeshPro>();
            displayText.text = "OFF";
            displayText.fontSize = 3f;
            displayText.alignment = TextAlignmentOptions.Center;
            displayText.color = displayTextColor;
            displayText.rectTransform.sizeDelta = new Vector2(bodyWidth * 0.7f, bodyHeight * 0.15f);

            // --- Mode dial (small cylinder on front face) ---
            GameObject dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dial.name = "ModeDial";
            dial.transform.SetParent(transform, false);
            dial.transform.localPosition = new Vector3(0f, -bodyHeight * 0.05f, -bodyDepth / 2f - 0.003f);
            dial.transform.localScale = new Vector3(0.025f, 0.004f, 0.025f);
            dial.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Material dialMat = new Material(Shader.Find("Standard"));
            dialMat.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            dialMat.SetFloat("_Metallic", 0.5f);
            dial.GetComponent<MeshRenderer>().material = dialMat;

            // Dial pointer (small cube showing current selection)
            GameObject dialPointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dialPointer.name = "DialPointer";
            dialPointer.transform.SetParent(dial.transform, false);
            dialPointer.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            dialPointer.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);

            Material pointerMat = new Material(Shader.Find("Standard"));
            pointerMat.color = Color.white;
            dialPointer.GetComponent<MeshRenderer>().material = pointerMat;

            Collider dialPointerCol = dialPointer.GetComponent<Collider>();
            if (dialPointerCol != null) Destroy(dialPointerCol);

            dialTransform = dial.transform;

            BoxCollider dialCol = dial.GetComponent<BoxCollider>();
            if (dialCol == null)
            {
                dialCol = dial.AddComponent<BoxCollider>();
            }
            dialCol.isTrigger = true;

            // Remove cylinder collider, replace with box trigger
            CapsuleCollider cylCol = dial.GetComponent<CapsuleCollider>();
            if (cylCol != null) Destroy(cylCol);

            // --- Mode labels around dial ---
            CreateDialLabel("OFF", new Vector3(0f, 0.018f, 0f), 0f);
            CreateDialLabel("V AC", new Vector3(0.018f, 0f, 0f), 0f);
            CreateDialLabel("V DC", new Vector3(0f, -0.018f, 0f), 0f);
            CreateDialLabel("OHM", new Vector3(-0.014f, -0.012f, 0f), 0f);
            CreateDialLabel("CONT", new Vector3(-0.014f, 0.012f, 0f), 0f);

            // --- Red probe jack (bottom right) ---
            GameObject redJack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            redJack.name = "RedProbeJack";
            redJack.transform.SetParent(transform, false);
            redJack.transform.localPosition = new Vector3(bodyWidth * 0.2f, -bodyHeight / 2f + 0.01f, -bodyDepth / 2f - 0.002f);
            redJack.transform.localScale = new Vector3(0.008f, 0.005f, 0.008f);
            redJack.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Material redJackMat = new Material(Shader.Find("Standard"));
            redJackMat.color = redProbeColor;
            redJack.GetComponent<MeshRenderer>().material = redJackMat;

            Collider redJackCol = redJack.GetComponent<Collider>();
            if (redJackCol != null) Destroy(redJackCol);

            redProbeJack = redJack.transform;

            // --- Black probe jack (bottom left) ---
            GameObject blackJack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            blackJack.name = "BlackProbeJack";
            blackJack.transform.SetParent(transform, false);
            blackJack.transform.localPosition = new Vector3(-bodyWidth * 0.2f, -bodyHeight / 2f + 0.01f, -bodyDepth / 2f - 0.002f);
            blackJack.transform.localScale = new Vector3(0.008f, 0.005f, 0.008f);
            blackJack.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Material blackJackMat = new Material(Shader.Find("Standard"));
            blackJackMat.color = blackProbeColor;
            blackJack.GetComponent<MeshRenderer>().material = blackJackMat;

            Collider blackJackCol = blackJack.GetComponent<Collider>();
            if (blackJackCol != null) Destroy(blackJackCol);

            blackProbeJack = blackJack.transform;

            // --- Probe tips (child transforms for measurement positions) ---
            GameObject redTip = new GameObject("RedProbeTip");
            redTip.transform.SetParent(transform, false);
            redTip.transform.localPosition = new Vector3(bodyWidth * 0.2f, -bodyHeight / 2f - 0.02f, 0f);
            redProbeTip = redTip.transform;

            GameObject blackTip = new GameObject("BlackProbeTip");
            blackTip.transform.SetParent(transform, false);
            blackTip.transform.localPosition = new Vector3(-bodyWidth * 0.2f, -bodyHeight / 2f - 0.02f, 0f);
            blackProbeTip = blackTip.transform;

            // --- Main collider ---
            BoxCollider mainCol = gameObject.GetComponent<BoxCollider>();
            if (mainCol == null)
            {
                mainCol = gameObject.AddComponent<BoxCollider>();
            }
            mainCol.center = Vector3.zero;
            mainCol.size = new Vector3(bodyWidth + 0.01f, bodyHeight + 0.01f, bodyDepth + 0.01f);
        }

        private void CreateDialLabel(string text, Vector3 localOffset, float rotation)
        {
            GameObject labelObj = new GameObject($"DialLabel_{text}");
            labelObj.transform.SetParent(dialTransform.parent, false);
            labelObj.transform.localPosition = dialTransform.localPosition + localOffset + new Vector3(0f, 0f, -0.002f);
            labelObj.transform.localRotation = Quaternion.Euler(0f, 180f, rotation);

            TextMeshPro label = labelObj.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = 1.2f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.rectTransform.sizeDelta = new Vector2(0.03f, 0.01f);
        }

        private void SetupGrabInteractable()
        {
            grabInteractable = GetComponent<GrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = gameObject.AddComponent<GrabInteractable>();
            }
            grabInteractable.SetToolType(toolType);
            grabInteractable.SetGripOffset(new Vector3(0f, -bodyHeight * 0.15f, 0f));
        }

        /// <summary>
        /// Sets the multimeter mode. Rotates the dial visual and updates the display prefix.
        /// </summary>
        public void SetMode(MultimeterMode mode)
        {
            currentMode = mode;
            currentReading = 0f;

            // Rotate dial to match mode
            if (dialTransform != null)
            {
                float dialAngle = 0f;
                switch (mode)
                {
                    case MultimeterMode.Off:        dialAngle = 0f;    break;
                    case MultimeterMode.ACVoltage:   dialAngle = 72f;   break;
                    case MultimeterMode.DCVoltage:   dialAngle = 144f;  break;
                    case MultimeterMode.Resistance:  dialAngle = 216f;  break;
                    case MultimeterMode.Continuity:  dialAngle = 288f;  break;
                }

                // Rotate around the Z-axis (facing the user)
                dialTransform.localRotation = Quaternion.Euler(90f, 0f, dialAngle);
            }

            // Update display
            UpdateDisplay(0f);

            Debug.Log($"[Multimeter] Mode set to: {mode}");
        }

        /// <summary>
        /// Cycles to the next mode.
        /// </summary>
        public void CycleMode()
        {
            int nextMode = ((int)currentMode + 1) % 5;
            SetMode((MultimeterMode)nextMode);
        }

        /// <summary>
        /// Takes a reading from the target GameObject.
        /// Checks for SnapPoint energization, CircuitBreaker state, Outlet connections, etc.
        /// In Continuity mode, plays a beep sound via AudioManager when circuit is complete.
        /// </summary>
        public float TakeReading(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("[Multimeter] No target specified for reading.");
                return 0f;
            }

            if (currentMode == MultimeterMode.Off)
            {
                Debug.Log("[Multimeter] Multimeter is OFF.");
                return 0f;
            }

            float reading = 0f;

            // Check for SnapPoint (terminal connection point)
            SnapPoint snapPoint = target.GetComponent<SnapPoint>();
            if (snapPoint == null)
                snapPoint = target.GetComponentInChildren<SnapPoint>();

            // Check for CircuitBreaker
            CircuitBreaker breaker = target.GetComponent<CircuitBreaker>();
            if (breaker == null)
                breaker = target.GetComponentInParent<CircuitBreaker>();

            // Check for Outlet
            Outlet outlet = target.GetComponent<Outlet>();
            if (outlet == null)
                outlet = target.GetComponentInParent<Outlet>();

            switch (currentMode)
            {
                case MultimeterMode.ACVoltage:
                    if (snapPoint != null && snapPoint.IsOccupied)
                    {
                        reading = 120.0f; // Standard residential AC
                    }
                    else if (breaker != null && breaker.State == CircuitBreaker.BreakerState.On)
                    {
                        reading = 120.0f;
                    }
                    else if (outlet != null && outlet.AreAllTerminalsConnected())
                    {
                        reading = 120.0f;
                    }
                    else
                    {
                        reading = 0.0f; // Dead circuit
                    }
                    break;

                case MultimeterMode.DCVoltage:
                    // DC voltage typically 0 in residential; included for tool completeness
                    reading = 0.0f;
                    break;

                case MultimeterMode.Resistance:
                    if (snapPoint != null && snapPoint.IsOccupied && snapPoint.ConnectedWire != null)
                    {
                        // Low resistance indicates good connection
                        reading = 0.2f; // Ohms — good wire connection
                    }
                    else
                    {
                        reading = 99999f; // Open circuit (OL on display)
                    }
                    break;

                case MultimeterMode.Continuity:
                    bool hasContinuity = false;

                    if (snapPoint != null && snapPoint.IsOccupied && snapPoint.ConnectedWire != null)
                    {
                        hasContinuity = true;
                    }
                    else if (breaker != null && breaker.State == CircuitBreaker.BreakerState.On)
                    {
                        hasContinuity = true;
                    }

                    if (hasContinuity)
                    {
                        reading = 0.1f; // Low resistance = continuity
                        // Play continuity beep
                        PlayContinuityBeep();
                    }
                    else
                    {
                        reading = 99999f; // Open — no beep
                    }
                    break;
            }

            currentReading = reading;
            UpdateDisplay(reading);

            Debug.Log($"[Multimeter] {currentMode} reading on {target.name}: {reading}");
            return reading;
        }

        /// <summary>
        /// Updates the display text with the reading value and appropriate prefix/suffix.
        /// </summary>
        public void UpdateDisplay(float value)
        {
            if (displayText == null) return;

            switch (currentMode)
            {
                case MultimeterMode.Off:
                    displayText.text = "OFF";
                    displayText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                    break;

                case MultimeterMode.ACVoltage:
                    displayText.text = $"{value:F1}\nV AC";
                    displayText.color = displayTextColor;
                    break;

                case MultimeterMode.DCVoltage:
                    displayText.text = $"{value:F1}\nV DC";
                    displayText.color = displayTextColor;
                    break;

                case MultimeterMode.Resistance:
                    if (value >= 99999f)
                    {
                        displayText.text = "OL\nOhms";
                    }
                    else
                    {
                        displayText.text = $"{value:F1}\nOhms";
                    }
                    displayText.color = displayTextColor;
                    break;

                case MultimeterMode.Continuity:
                    if (value >= 99999f)
                    {
                        displayText.text = "OL\nCont";
                    }
                    else
                    {
                        displayText.text = $"{value:F1}\nCont";
                    }
                    displayText.color = displayTextColor;
                    break;
            }
        }

        /// <summary>
        /// Plays the continuity beep tone via AudioManager.
        /// Uses a 2000Hz tone for 0.15 seconds, consistent with real multimeter behavior.
        /// </summary>
        private void PlayContinuityBeep()
        {
            if (AudioManager.Instance != null)
            {
                // Generate a 2000Hz beep tone procedurally
                int sampleRate = 44100;
                float duration = 0.15f;
                int sampleCount = Mathf.CeilToInt(sampleRate * duration);
                float[] samples = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = 1f - (t / duration);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * 2000f * t) * envelope * 0.3f;
                }

                AudioClip beepClip = AudioClip.Create("ContinuityBeep", sampleCount, 1, sampleRate, false);
                beepClip.SetData(samples, 0);

                AudioSource source = GetComponent<AudioSource>();
                if (source == null)
                {
                    source = gameObject.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    source.spatialBlend = 1f; // 3D sound from meter
                }
                source.PlayOneShot(beepClip);
            }
        }

        /// <summary>
        /// Returns the display prefix string for the current mode.
        /// </summary>
        public string GetModePrefix()
        {
            switch (currentMode)
            {
                case MultimeterMode.Off:         return "OFF";
                case MultimeterMode.ACVoltage:   return "V AC";
                case MultimeterMode.DCVoltage:   return "V DC";
                case MultimeterMode.Resistance:  return "Ohms";
                case MultimeterMode.Continuity:  return "Cont";
                default:                         return "";
            }
        }

        /// <summary>
        /// Returns probe tip transforms for positioning probes in the scene.
        /// Index 0 = red probe, Index 1 = black probe.
        /// </summary>
        public Transform[] GetProbePositions()
        {
            return new Transform[] { redProbeTip, blackProbeTip };
        }
    }
}
