using UnityEngine;
using TradeProof.Interaction;
using TradeProof.Core;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Non-contact voltage tester (tick tester). Grabbable via GrabInteractable component.
    /// Visual: pen-shaped body (scaled cylinder), tip (small sphere), LED indicator, pocket clip.
    /// Detection: uses Physics.OverlapSphere at the tip to find energized components.
    /// When voltage is detected: LED turns red, plays 2000Hz beep.
    /// When no voltage: LED turns green, no sound.
    /// Body color: dark red. Detection radius: 0.03m.
    /// </summary>
    public class VoltageTester : MonoBehaviour
    {
        [Header("Tool Properties")]
        [SerializeField] private string toolType = "voltage-tester";

        [Header("Detection")]
        [SerializeField] private float detectionRadius = 0.03f;
        [SerializeField] private bool isDetectingVoltage;

        [Header("Dimensions")]
        [SerializeField] private float bodyDiameter = 0.015f;
        [SerializeField] private float bodyLength = 0.15f;
        [SerializeField] private float tipDiameter = 0.005f;

        [Header("Visual Colors")]
        [SerializeField] private Color bodyColor = new Color(0.5f, 0.1f, 0.1f, 1f);    // Dark red
        [SerializeField] private Color clipColor = new Color(0.3f, 0.3f, 0.3f, 1f);     // Dark gray
        [SerializeField] private Color ledOffColor = new Color(0.1f, 0.4f, 0.1f, 1f);   // Dim green
        [SerializeField] private Color ledOnColor = new Color(1f, 0.1f, 0.1f, 1f);      // Bright red
        [SerializeField] private Color ledSafeColor = new Color(0.1f, 1f, 0.1f, 1f);    // Bright green

        // Components
        private GrabInteractable grabInteractable;
        private AudioSource audioSource;

        // Visual references
        private MeshRenderer bodyRenderer;
        private MeshRenderer tipRenderer;
        private MeshRenderer ledRenderer;
        private MeshRenderer clipRenderer;
        private Transform tipTransform;

        // Audio
        private AudioClip beepClip;
        private bool wasDetecting;
        private float beepCooldown;
        private float beepInterval = 0.3f; // Beep every 0.3 seconds while detecting

        public bool IsDetectingVoltage => isDetectingVoltage;
        public string ToolType => toolType;
        public float DetectionRadius => detectionRadius;

        private void Awake()
        {
            BuildVisual();
            SetupGrabInteractable();
            SetupAudio();
        }

        private void Update()
        {
            CheckForVoltage();
            UpdateVisualFeedback();
            UpdateAudioFeedback();
        }

        private void BuildVisual()
        {
            // --- Body: scaled cylinder (pen shape) ---
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "TesterBody";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = Vector3.zero;
            // Cylinder height = bodyLength, radius = bodyDiameter/2
            body.transform.localScale = new Vector3(bodyDiameter, bodyLength / 2f, bodyDiameter);

            bodyRenderer = body.GetComponent<MeshRenderer>();
            Material bodyMat = new Material(Shader.Find("Standard"));
            bodyMat.color = bodyColor;
            bodyMat.SetFloat("_Glossiness", 0.4f);
            bodyRenderer.material = bodyMat;

            // Remove body collider (main collider on parent)
            Collider bodyCol = body.GetComponent<Collider>();
            if (bodyCol != null) Destroy(bodyCol);

            // --- Tip: small sphere at the end ---
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "TesterTip";
            tip.transform.SetParent(transform, false);
            tip.transform.localPosition = new Vector3(0f, -bodyLength / 2f - tipDiameter / 2f, 0f);
            tip.transform.localScale = Vector3.one * tipDiameter;

            tipRenderer = tip.GetComponent<MeshRenderer>();
            Material tipMat = new Material(Shader.Find("Standard"));
            tipMat.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark conductive tip
            tipMat.SetFloat("_Metallic", 0.8f);
            tipRenderer.material = tipMat;

            // Remove tip collider
            Collider tipCol = tip.GetComponent<Collider>();
            if (tipCol != null) Destroy(tipCol);

            tipTransform = tip.transform;

            // --- LED indicator: small sphere near the tip ---
            GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            led.name = "LEDIndicator";
            led.transform.SetParent(transform, false);
            led.transform.localPosition = new Vector3(0f, -bodyLength / 2f + bodyDiameter * 1.5f, -bodyDiameter / 2f - 0.002f);
            led.transform.localScale = Vector3.one * 0.006f;

            ledRenderer = led.GetComponent<MeshRenderer>();
            Material ledMat = new Material(Shader.Find("Standard"));
            ledMat.color = ledOffColor;
            ledMat.EnableKeyword("_EMISSION");
            ledMat.SetColor("_EmissionColor", ledOffColor * 0.5f);
            ledRenderer.material = ledMat;

            Collider ledCol = led.GetComponent<Collider>();
            if (ledCol != null) Destroy(ledCol);

            // --- Pocket clip: small cube on the side ---
            GameObject clip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clip.name = "PocketClip";
            clip.transform.SetParent(transform, false);
            clip.transform.localPosition = new Vector3(bodyDiameter / 2f + 0.001f, bodyLength * 0.15f, 0f);
            clip.transform.localScale = new Vector3(0.003f, 0.04f, 0.006f);

            clipRenderer = clip.GetComponent<MeshRenderer>();
            Material clipMat = new Material(Shader.Find("Standard"));
            clipMat.color = clipColor;
            clipMat.SetFloat("_Metallic", 0.7f);
            clipRenderer.material = clipMat;

            Collider clipCol = clip.GetComponent<Collider>();
            if (clipCol != null) Destroy(clipCol);

            // --- Main collider for the entire tester ---
            CapsuleCollider mainCol = gameObject.GetComponent<CapsuleCollider>();
            if (mainCol == null)
            {
                mainCol = gameObject.AddComponent<CapsuleCollider>();
            }
            mainCol.direction = 1; // Y-axis (along pen length)
            mainCol.radius = bodyDiameter / 2f + 0.005f;
            mainCol.height = bodyLength + tipDiameter + 0.01f;
            mainCol.center = new Vector3(0f, -tipDiameter / 2f, 0f);
        }

        private void SetupGrabInteractable()
        {
            grabInteractable = GetComponent<GrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = gameObject.AddComponent<GrabInteractable>();
            }
            grabInteractable.SetToolType(toolType);
            grabInteractable.SetGripOffset(new Vector3(0f, bodyLength * 0.1f, 0f));
        }

        private void SetupAudio()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound from tester
            audioSource.loop = false;

            // Generate the 2000Hz beep clip
            GenerateBeepClip();
        }

        private void GenerateBeepClip()
        {
            int sampleRate = 44100;
            float duration = 0.1f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // Linear fade out
                samples[i] = Mathf.Sin(2f * Mathf.PI * 2000f * t) * envelope * 0.25f;
            }

            beepClip = AudioClip.Create("VoltageBeep", sampleCount, 1, sampleRate, false);
            beepClip.SetData(samples, 0);
        }

        /// <summary>
        /// Continuously checks for voltage near the tip using Physics.OverlapSphere.
        /// Looks for energized SnapPoints, active CircuitBreakers, and connected Outlets.
        /// </summary>
        private void CheckForVoltage()
        {
            if (tipTransform == null) return;

            isDetectingVoltage = false;

            Collider[] nearby = Physics.OverlapSphere(tipTransform.position, detectionRadius);

            foreach (Collider col in nearby)
            {
                // Check SnapPoint
                SnapPoint snapPoint = col.GetComponent<SnapPoint>();
                if (snapPoint == null)
                    snapPoint = col.GetComponentInParent<SnapPoint>();

                if (snapPoint != null && snapPoint.IsOccupied)
                {
                    // Occupied snap point on a hot terminal suggests voltage
                    if (snapPoint.AcceptedWireType == "hot" || snapPoint.AcceptedWireType == "any")
                    {
                        isDetectingVoltage = true;
                        break;
                    }
                }

                // Check CircuitBreaker
                CircuitBreaker breaker = col.GetComponent<CircuitBreaker>();
                if (breaker == null)
                    breaker = col.GetComponentInParent<CircuitBreaker>();

                if (breaker != null && breaker.State == CircuitBreaker.BreakerState.On)
                {
                    isDetectingVoltage = true;
                    break;
                }

                // Check Outlet
                Outlet outlet = col.GetComponent<Outlet>();
                if (outlet == null)
                    outlet = col.GetComponentInParent<Outlet>();

                if (outlet != null && outlet.HotTerminal != null && outlet.HotTerminal.IsOccupied)
                {
                    isDetectingVoltage = true;
                    break;
                }

                // Check Wire
                Wire wire = col.GetComponent<Wire>();
                if (wire == null)
                    wire = col.GetComponentInParent<Wire>();

                if (wire != null && wire.IsEnergized && wire.Type == Wire.WireType.Hot)
                {
                    isDetectingVoltage = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Updates LED color and emission based on voltage detection state.
        /// Red + glow when voltage detected; green when safe.
        /// </summary>
        private void UpdateVisualFeedback()
        {
            if (ledRenderer == null) return;

            Material ledMat = ledRenderer.material;

            if (isDetectingVoltage)
            {
                // Pulsing red LED
                float pulse = Mathf.Sin(Time.time * 10f) * 0.3f + 0.7f;
                Color pulsedColor = ledOnColor * pulse;
                ledMat.color = pulsedColor;
                ledMat.SetColor("_EmissionColor", ledOnColor * 2f * pulse);
            }
            else
            {
                // Steady green LED
                ledMat.color = ledSafeColor;
                ledMat.SetColor("_EmissionColor", ledSafeColor * 0.5f);
            }
        }

        /// <summary>
        /// Plays beep sound at regular intervals while voltage is detected.
        /// Stops when no voltage is present.
        /// </summary>
        private void UpdateAudioFeedback()
        {
            if (isDetectingVoltage)
            {
                beepCooldown -= Time.deltaTime;
                if (beepCooldown <= 0f)
                {
                    PlayBeep();
                    beepCooldown = beepInterval;
                }
            }
            else
            {
                beepCooldown = 0f; // Reset so first detection beeps immediately
            }
        }

        private void PlayBeep()
        {
            if (audioSource != null && beepClip != null)
            {
                audioSource.PlayOneShot(beepClip);
            }
        }

        /// <summary>
        /// Returns the world position of the detection tip.
        /// </summary>
        public Vector3 GetTipPosition()
        {
            return tipTransform != null ? tipTransform.position : transform.position;
        }

        /// <summary>
        /// Manually triggers a voltage check (useful for external systems).
        /// Returns true if voltage is detected at the tip.
        /// </summary>
        public bool ManualCheck()
        {
            CheckForVoltage();
            return isDetectingVoltage;
        }
    }
}
