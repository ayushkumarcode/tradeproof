using UnityEngine;
using TradeProof.Interaction;

namespace TradeProof.Training
{
    /// <summary>
    /// Represents a physical wire segment that can be grabbed, routed, and snapped to connection points.
    /// Uses LineRenderer for visual representation.
    /// All connection points are relative to parent component transforms.
    /// </summary>
    public class WireSegment : MonoBehaviour
    {
        [Header("Wire Properties")]
        [SerializeField] private int wireGaugeAWG = 12; // 10, 12, or 14 AWG
        [SerializeField] private WireColor wireColor = WireColor.Black;
        [SerializeField] private float wireLength = 1.5f; // meters

        [Header("Visual")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float wireThickness = 0.004f; // meters
        [SerializeField] private int lineSegments = 20;
        [SerializeField] private Material wireMaterial;

        [Header("Endpoints")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private SnapPoint snappedStartPoint;
        [SerializeField] private SnapPoint snappedEndPoint;

        [Header("Interaction")]
        [SerializeField] private GrabInteractable grabInteractable;
        [SerializeField] private bool isStartGrabbed;
        [SerializeField] private bool isEndGrabbed;

        [Header("State")]
        [SerializeField] private WireState currentState = WireState.Idle;
        private float pulseTimer;
        private Color baseColor;
        private Color stateColor;

        public int WireGaugeAWG => wireGaugeAWG;
        public WireColor WireColorType => wireColor;
        public bool IsStartSnapped => snappedStartPoint != null;
        public bool IsEndSnapped => snappedEndPoint != null;
        public bool IsFullyConnected => IsStartSnapped && IsEndSnapped;
        public SnapPoint StartSnapPoint => snappedStartPoint;
        public SnapPoint EndSnapPoint => snappedEndPoint;

        public enum WireState
        {
            Idle,
            Selected,
            Correct,
            Wrong,
            Stripped
        }

        public enum WireColor
        {
            Black,   // Hot
            White,   // Neutral
            Green,   // Ground
            Red,     // Hot (second)
            Bare     // Ground (bare copper)
        }

        private void Awake()
        {
            SetupLineRenderer();
            SetupEndpoints();
            SetupGrabInteractable();
            SetWireVisualsByGauge();
        }

        private void SetupLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = lineSegments;
            lineRenderer.startWidth = wireThickness;
            lineRenderer.endWidth = wireThickness;
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCapVertices = 4;
            lineRenderer.numCornerVertices = 4;

            // Set material and color
            if (wireMaterial == null)
            {
                wireMaterial = new Material(Shader.Find("Standard"));
            }
            lineRenderer.material = wireMaterial;

            baseColor = GetColorForWireType(wireColor);
            wireMaterial.color = baseColor;
            stateColor = baseColor;
        }

        private void SetupEndpoints()
        {
            if (startPoint == null)
            {
                GameObject startObj = new GameObject("WireStart");
                startObj.transform.SetParent(transform, false);
                startObj.transform.localPosition = Vector3.zero;
                startPoint = startObj.transform;

                // Add collider for grab detection
                SphereCollider col = startObj.AddComponent<SphereCollider>();
                col.radius = 0.015f;
                col.isTrigger = true;
            }

            if (endPoint == null)
            {
                GameObject endObj = new GameObject("WireEnd");
                endObj.transform.SetParent(transform, false);
                endObj.transform.localPosition = new Vector3(0.3f, 0f, 0f);
                endPoint = endObj.transform;

                SphereCollider col = endObj.AddComponent<SphereCollider>();
                col.radius = 0.015f;
                col.isTrigger = true;
            }
        }

        private void SetupGrabInteractable()
        {
            if (grabInteractable == null)
            {
                grabInteractable = gameObject.GetComponent<GrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = gameObject.AddComponent<GrabInteractable>();
                }
            }

            // Set grip offset for wire — held between thumb and index finger
            grabInteractable.SetGripOffset(new Vector3(0f, -0.02f, 0.05f));
        }

        private void SetWireVisualsByGauge()
        {
            // Wire thickness based on AWG gauge (approximate real-world diameter in meters)
            switch (wireGaugeAWG)
            {
                case 14:
                    wireThickness = 0.003f; // ~1.6mm diameter
                    break;
                case 12:
                    wireThickness = 0.004f; // ~2.0mm diameter
                    break;
                case 10:
                    wireThickness = 0.005f; // ~2.6mm diameter
                    break;
                default:
                    wireThickness = 0.004f;
                    break;
            }

            if (lineRenderer != null)
            {
                lineRenderer.startWidth = wireThickness;
                lineRenderer.endWidth = wireThickness;
            }
        }

        private Color GetColorForWireType(WireColor color)
        {
            switch (color)
            {
                case WireColor.Black:
                    return new Color(0.1f, 0.1f, 0.1f); // Black
                case WireColor.White:
                    return new Color(0.9f, 0.9f, 0.9f); // White
                case WireColor.Green:
                    return new Color(0.1f, 0.6f, 0.1f); // Green
                case WireColor.Red:
                    return new Color(0.8f, 0.1f, 0.1f); // Red
                case WireColor.Bare:
                    return new Color(0.72f, 0.45f, 0.2f); // Copper
                default:
                    return Color.gray;
            }
        }

        private void Update()
        {
            UpdateLinePositions();
            UpdateVisualState();
        }

        private void UpdateLinePositions()
        {
            if (lineRenderer == null || startPoint == null || endPoint == null) return;

            Vector3 start = startPoint.position;
            Vector3 end = endPoint.position;

            // Create natural wire droop using catenary-like curve
            for (int i = 0; i < lineSegments; i++)
            {
                float t = (float)i / (lineSegments - 1);
                Vector3 point = Vector3.Lerp(start, end, t);

                // Add sag based on distance between endpoints
                float distance = Vector3.Distance(start, end);
                float sagAmount = distance * 0.1f; // 10% sag
                float sag = Mathf.Sin(t * Mathf.PI) * sagAmount;
                point.y -= sag;

                // Add slight random variation for realism
                if (i > 0 && i < lineSegments - 1)
                {
                    float noise = Mathf.PerlinNoise(t * 5f + Time.time * 0.1f, 0f) * 0.002f;
                    point.x += noise;
                    point.z += noise;
                }

                lineRenderer.SetPosition(i, point);
            }
        }

        private void UpdateVisualState()
        {
            switch (currentState)
            {
                case WireState.Selected:
                    // Subtle glow effect
                    float glow = Mathf.Sin(Time.time * 3f) * 0.2f + 0.8f;
                    wireMaterial.color = baseColor * glow + Color.white * (1f - glow) * 0.3f;
                    wireMaterial.SetColor("_EmissionColor", baseColor * 0.3f);
                    wireMaterial.EnableKeyword("_EMISSION");
                    break;

                case WireState.Correct:
                    pulseTimer += Time.deltaTime;
                    float correctPulse = Mathf.Sin(pulseTimer * 4f) * 0.3f + 0.7f;
                    wireMaterial.color = Color.Lerp(baseColor, Color.green, correctPulse * 0.5f);
                    wireMaterial.SetColor("_EmissionColor", Color.green * correctPulse * 0.2f);
                    wireMaterial.EnableKeyword("_EMISSION");
                    break;

                case WireState.Wrong:
                    pulseTimer += Time.deltaTime;
                    float wrongPulse = Mathf.Sin(pulseTimer * 6f) * 0.4f + 0.6f;
                    wireMaterial.color = Color.Lerp(baseColor, Color.red, wrongPulse * 0.6f);
                    wireMaterial.SetColor("_EmissionColor", Color.red * wrongPulse * 0.3f);
                    wireMaterial.EnableKeyword("_EMISSION");
                    break;

                case WireState.Idle:
                default:
                    wireMaterial.color = baseColor;
                    wireMaterial.DisableKeyword("_EMISSION");
                    break;
            }
        }

        // --- Snap Point Integration ---

        public bool TrySnapStart(SnapPoint snapPoint)
        {
            if (snappedStartPoint != null) return false;
            if (snapPoint == null || snapPoint.IsOccupied) return false;

            float distance = Vector3.Distance(startPoint.position, snapPoint.transform.position);
            if (distance <= snapPoint.SnapRadius)
            {
                snappedStartPoint = snapPoint;
                startPoint.position = snapPoint.transform.position;
                snapPoint.AttachWire(this);

                Core.AudioManager.Instance.PlaySnapSound();
                Debug.Log($"[WireSegment] Start snapped to {snapPoint.name}");
                return true;
            }
            return false;
        }

        public bool TrySnapEnd(SnapPoint snapPoint)
        {
            if (snappedEndPoint != null) return false;
            if (snapPoint == null || snapPoint.IsOccupied) return false;

            float distance = Vector3.Distance(endPoint.position, snapPoint.transform.position);
            if (distance <= snapPoint.SnapRadius)
            {
                snappedEndPoint = snapPoint;
                endPoint.position = snapPoint.transform.position;
                snapPoint.AttachWire(this);

                Core.AudioManager.Instance.PlaySnapSound();
                Debug.Log($"[WireSegment] End snapped to {snapPoint.name}");
                return true;
            }
            return false;
        }

        public void DetachStart()
        {
            if (snappedStartPoint != null)
            {
                snappedStartPoint.DetachWire();
                snappedStartPoint = null;
            }
        }

        public void DetachEnd()
        {
            if (snappedEndPoint != null)
            {
                snappedEndPoint.DetachWire();
                snappedEndPoint = null;
            }
        }

        public void DetachAll()
        {
            DetachStart();
            DetachEnd();
        }

        // --- State ---

        public void SetState(WireState state)
        {
            currentState = state;
            pulseTimer = 0f;
        }

        public void SetSelected(bool selected)
        {
            if (selected)
                SetState(WireState.Selected);
            else if (currentState == WireState.Selected)
                SetState(WireState.Idle);
        }

        public void ShowCorrect()
        {
            SetState(WireState.Correct);
        }

        public void ShowWrong()
        {
            SetState(WireState.Wrong);
        }

        // --- Validation ---

        public bool ValidateGaugeForAmperage(int breakerAmperage)
        {
            // NEC 310.16 simplified: 14 AWG = 15A, 12 AWG = 20A, 10 AWG = 30A
            int maxAmperage = GetMaxAmperageForGauge(wireGaugeAWG);
            return maxAmperage >= breakerAmperage;
        }

        public static int GetMaxAmperageForGauge(int gaugeAWG)
        {
            switch (gaugeAWG)
            {
                case 14: return 15;
                case 12: return 20;
                case 10: return 30;
                case 8: return 40;
                case 6: return 55;
                default: return 0;
            }
        }

        public static string GetGaugeDescription(int gaugeAWG)
        {
            return $"{gaugeAWG} AWG ({GetMaxAmperageForGauge(gaugeAWG)}A max)";
        }

        // --- Endpoint Access ---

        public void SetStartPosition(Vector3 worldPos)
        {
            if (startPoint != null)
                startPoint.position = worldPos;
        }

        public void SetEndPosition(Vector3 worldPos)
        {
            if (endPoint != null)
                endPoint.position = worldPos;
        }

        public Vector3 GetStartPosition()
        {
            return startPoint != null ? startPoint.position : transform.position;
        }

        public Vector3 GetEndPosition()
        {
            return endPoint != null ? endPoint.position : transform.position;
        }

        public Transform GetStartTransform()
        {
            return startPoint;
        }

        public Transform GetEndTransform()
        {
            return endPoint;
        }

        // --- Strip Wire ---

        public void StripWireEnd(bool startEnd)
        {
            SetState(WireState.Stripped);
            Debug.Log($"[WireSegment] {(startEnd ? "Start" : "End")} stripped — ready for connection");
        }
    }
}
