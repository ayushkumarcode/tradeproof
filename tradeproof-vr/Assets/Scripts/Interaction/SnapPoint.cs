using UnityEngine;
using TradeProof.Training;

namespace TradeProof.Interaction
{
    /// <summary>
    /// Defines a connection point on electrical components.
    /// When a wire endpoint enters the snap radius, it visually snaps and locks.
    /// Provides visual feedback: green glow when in range, click when snapped.
    /// Validates correct connection type.
    ///
    /// Snap radius: 0.02m — precise but not frustrating.
    /// All snap points are child transforms of their parent component.
    /// </summary>
    public class SnapPoint : MonoBehaviour
    {
        [Header("Snap Settings")]
        [SerializeField] private float snapRadius = 0.02f;
        [SerializeField] private bool isOccupied;

        [Header("Validation")]
        [SerializeField] private string acceptedWireType = "any"; // "hot", "neutral", "ground", "any"
        [SerializeField] private int ampRating; // 0 = any
        [SerializeField] private string snapPointId;

        [Header("Connected Wire")]
        private WireSegment connectedWire;

        [Header("Visual")]
        [SerializeField] private MeshRenderer visualRenderer;
        [SerializeField] private Color idleColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color proximityColor = new Color(0f, 1f, 0f, 0.6f); // Green
        [SerializeField] private Color occupiedColor = new Color(0f, 0.7f, 0f, 0.4f);
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.6f); // Red
        private Material visualMaterial;
        private bool showingProximity;

        [Header("Label")]
        [SerializeField] private string labelText;

        public float SnapRadius => snapRadius;
        public bool IsOccupied => isOccupied;
        public WireSegment ConnectedWire => connectedWire;
        public string AcceptedWireType => acceptedWireType;
        public string SnapPointId => snapPointId;

        private void Awake()
        {
            // Create visual indicator
            if (visualRenderer == null)
            {
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.transform.SetParent(transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = Vector3.one * snapRadius * 2f;

                // Remove collider from visual (we use our own trigger)
                Collider visualCol = visual.GetComponent<Collider>();
                if (visualCol != null) Destroy(visualCol);

                visualRenderer = visual.GetComponent<MeshRenderer>();
            }

            // Create transparent material
            visualMaterial = new Material(Shader.Find("Standard"));
            visualMaterial.SetFloat("_Mode", 3); // Transparent
            visualMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            visualMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            visualMaterial.SetInt("_ZWrite", 0);
            visualMaterial.EnableKeyword("_ALPHABLEND_ON");
            visualMaterial.renderQueue = 3000;
            visualMaterial.color = idleColor;
            visualRenderer.material = visualMaterial;

            // Add trigger collider for snap detection
            SphereCollider trigger = gameObject.GetComponent<SphereCollider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<SphereCollider>();
            }
            trigger.radius = snapRadius;
            trigger.isTrigger = true;
        }

        private void Update()
        {
            // Pulse effect when showing proximity
            if (showingProximity && !isOccupied)
            {
                float pulse = Mathf.Sin(Time.time * 5f) * 0.2f + 0.8f;
                Color pulsed = proximityColor;
                pulsed.a *= pulse;
                visualMaterial.color = pulsed;
            }
        }

        // --- Trigger Detection ---

        private void OnTriggerEnter(Collider other)
        {
            if (isOccupied) return;

            WireSegment wire = other.GetComponent<WireSegment>();
            if (wire == null)
                wire = other.GetComponentInParent<WireSegment>();

            if (wire != null)
            {
                ShowProximityFeedback(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isOccupied) return;

            WireSegment wire = other.GetComponent<WireSegment>();
            if (wire == null)
                wire = other.GetComponentInParent<WireSegment>();

            if (wire != null)
            {
                // Auto-snap if wire endpoint is close enough and not grabbed
                GrabInteractable grab = wire.GetComponent<GrabInteractable>();
                if (grab != null && !grab.IsGrabbed)
                {
                    // Check which endpoint is closer
                    float distStart = Vector3.Distance(wire.GetStartPosition(), transform.position);
                    float distEnd = Vector3.Distance(wire.GetEndPosition(), transform.position);

                    if (distStart <= snapRadius)
                    {
                        TryAttachWire(wire, true);
                    }
                    else if (distEnd <= snapRadius)
                    {
                        TryAttachWire(wire, false);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            WireSegment wire = other.GetComponent<WireSegment>();
            if (wire == null)
                wire = other.GetComponentInParent<WireSegment>();

            if (wire != null && !isOccupied)
            {
                ShowProximityFeedback(false);
            }
        }

        // --- Attachment ---

        private bool TryAttachWire(WireSegment wire, bool startEndpoint)
        {
            if (isOccupied) return false;

            // Validate wire type
            if (!ValidateWireType(wire))
            {
                visualMaterial.color = invalidColor;
                Core.AudioManager.Instance.PlayIncorrectSound();
                Debug.Log($"[SnapPoint] Invalid wire type: expected {acceptedWireType}");
                return false;
            }

            // Validate wire gauge for amp rating
            if (ampRating > 0 && !wire.ValidateGaugeForAmperage(ampRating))
            {
                visualMaterial.color = invalidColor;
                Core.AudioManager.Instance.PlayIncorrectSound();
                Debug.Log($"[SnapPoint] Wire gauge {wire.WireGaugeAWG} AWG insufficient for {ampRating}A");
                return false;
            }

            // Snap the wire
            if (startEndpoint)
            {
                wire.TrySnapStart(this);
            }
            else
            {
                wire.TrySnapEnd(this);
            }

            return true;
        }

        public void AttachWire(WireSegment wire)
        {
            if (isOccupied)
            {
                Debug.LogWarning($"[SnapPoint] {name} already occupied");
                return;
            }

            connectedWire = wire;
            isOccupied = true;
            showingProximity = false;

            visualMaterial.color = occupiedColor;
            // Note: Snap sound is played by WireSegment.TrySnapStart/End to avoid double playback

            Debug.Log($"[SnapPoint] Wire attached to {name}");
        }

        public void DetachWire()
        {
            connectedWire = null;
            isOccupied = false;
            visualMaterial.color = idleColor;

            Debug.Log($"[SnapPoint] Wire detached from {name}");
        }

        // --- Validation ---

        private bool ValidateWireType(WireSegment wire)
        {
            if (acceptedWireType == "any") return true;

            WireSegment.WireColor wireColor = wire.WireColorType;

            switch (acceptedWireType)
            {
                case "hot":
                    return wireColor == WireSegment.WireColor.Black || wireColor == WireSegment.WireColor.Red;
                case "neutral":
                    return wireColor == WireSegment.WireColor.White;
                case "ground":
                    return wireColor == WireSegment.WireColor.Green || wireColor == WireSegment.WireColor.Bare;
                default:
                    return true;
            }
        }

        public bool ValidateConnection()
        {
            if (!isOccupied || connectedWire == null) return false;

            bool typeValid = ValidateWireType(connectedWire);
            bool gaugeValid = ampRating <= 0 || connectedWire.ValidateGaugeForAmperage(ampRating);

            return typeValid && gaugeValid;
        }

        // --- Visual Feedback ---

        public void ShowProximityFeedback(bool show)
        {
            showingProximity = show;
            if (!show && !isOccupied)
            {
                visualMaterial.color = idleColor;
            }
        }

        // --- Configuration ---

        public void SetAcceptedWireType(string type)
        {
            acceptedWireType = type;
        }

        public void SetAmpRating(int amps)
        {
            ampRating = amps;
        }

        public void SetSnapPointId(string id)
        {
            snapPointId = id;
        }

        public void SetLabel(string text)
        {
            labelText = text;
        }
    }
}
