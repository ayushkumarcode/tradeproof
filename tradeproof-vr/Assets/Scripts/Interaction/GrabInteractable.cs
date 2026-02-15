using UnityEngine;

namespace TradeProof.Interaction
{
    /// <summary>
    /// Makes an object grabbable by HandInteraction.
    /// Supports both hand tracking grab and controller trigger grab.
    /// Maintains physics while grabbed (kinematic toggle).
    /// Returns to original position if released outside a valid snap point.
    /// </summary>
    public class GrabInteractable : MonoBehaviour
    {
        [Header("Grab Settings")]
        [SerializeField] private bool isGrabbable = true;
        [SerializeField] private bool returnOnInvalidRelease = true;
        [SerializeField] private float returnSpeed = 5f;

        [Header("Tool Type")]
        [SerializeField] private string toolType = "default";

        [Header("Grip Offset")]
        [SerializeField] private Vector3 gripOffset = Vector3.zero;

        [Header("Physics")]
        [SerializeField] private bool usePhysics = true;
        private Rigidbody rb;
        private bool wasKinematic;
        private bool wasGravity;

        [Header("State")]
        private bool isGrabbed;
        private bool isReturning;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Transform originalParent;
        private HandInteraction currentGrabber;

        [Header("Snap")]
        private SnapPoint nearestSnapPoint;
        private float snapSearchRadius = 0.05f;

        [Header("Visual Feedback")]
        [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 1f, 1f);
        [SerializeField] private Color grabColor = new Color(1f, 0.9f, 0.5f, 1f);
        private MeshRenderer meshRenderer;
        private Color originalColor;
        private bool hasOriginalColor;

        public bool IsGrabbable => isGrabbable;
        public bool IsGrabbed => isGrabbed;
        public string ToolType => toolType;

        private void Awake()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalParent = transform.parent;

            rb = GetComponent<Rigidbody>();
            if (rb == null && usePhysics)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
            if (meshRenderer != null)
            {
                originalColor = meshRenderer.material.color;
                hasOriginalColor = true;
            }

            // Ensure collider exists
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.size = Vector3.one * 0.05f;
            }
        }

        private void Update()
        {
            if (isReturning)
            {
                transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * returnSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, Time.deltaTime * returnSpeed);

                if (Vector3.Distance(transform.position, originalPosition) < 0.005f)
                {
                    transform.position = originalPosition;
                    transform.rotation = originalRotation;
                    isReturning = false;
                }
            }
        }

        // --- Grab/Release ---

        public void OnGrabbed(HandInteraction grabber)
        {
            if (!isGrabbable || isGrabbed) return;

            isGrabbed = true;
            isReturning = false;
            currentGrabber = grabber;

            // Save physics state and make kinematic
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                wasGravity = rb.useGravity;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Detach from parent (so it follows hand freely)
            transform.SetParent(null);

            // Visual feedback
            if (meshRenderer != null)
            {
                meshRenderer.material.color = grabColor;
            }

            Debug.Log($"[GrabInteractable] {name} grabbed");
        }

        public void OnReleased(HandInteraction grabber)
        {
            if (!isGrabbed || currentGrabber != grabber) return;

            isGrabbed = false;
            currentGrabber = null;

            // Check for nearby snap points
            nearestSnapPoint = FindNearestSnapPoint();

            if (nearestSnapPoint != null && !nearestSnapPoint.IsOccupied)
            {
                // Snap to point
                SnapToPoint(nearestSnapPoint);
            }
            else if (returnOnInvalidRelease)
            {
                // Return to original position
                isReturning = true;

                // Restore parent
                transform.SetParent(originalParent);
            }

            // Restore physics
            if (rb != null)
            {
                rb.isKinematic = wasKinematic;
                rb.useGravity = wasGravity;
            }

            // Visual feedback
            if (meshRenderer != null && hasOriginalColor)
            {
                meshRenderer.material.color = originalColor;
            }

            Debug.Log($"[GrabInteractable] {name} released" +
                      (nearestSnapPoint != null ? $" — snapped to {nearestSnapPoint.name}" : ""));
        }

        public void UpdateGrabbedPosition(Vector3 worldPosition, Quaternion worldRotation)
        {
            if (!isGrabbed) return;

            transform.position = worldPosition;
            transform.rotation = worldRotation;

            // Check proximity to snap points for visual feedback
            nearestSnapPoint = FindNearestSnapPoint();
            if (nearestSnapPoint != null)
            {
                nearestSnapPoint.ShowProximityFeedback(true);
            }
        }

        // --- Snap Points ---

        private SnapPoint FindNearestSnapPoint()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, snapSearchRadius);
            SnapPoint closest = null;
            float closestDist = float.MaxValue;

            foreach (var col in nearby)
            {
                SnapPoint sp = col.GetComponent<SnapPoint>();
                if (sp != null && !sp.IsOccupied)
                {
                    float dist = Vector3.Distance(transform.position, sp.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = sp;
                    }
                }
            }

            return closest;
        }

        private void SnapToPoint(SnapPoint point)
        {
            transform.position = point.transform.position;
            transform.rotation = point.transform.rotation;
            transform.SetParent(point.transform);

            // Notify the snap point
            Training.WireSegment wire = GetComponent<Training.WireSegment>();
            if (wire != null)
            {
                point.AttachWire(wire);
            }
        }

        // --- Public API ---

        public void SetGripOffset(Vector3 offset)
        {
            gripOffset = offset;
        }

        public Vector3 GetGripOffset()
        {
            return gripOffset;
        }

        public void SetGrabbable(bool grabbable)
        {
            isGrabbable = grabbable;
        }

        public void SetToolType(string type)
        {
            toolType = type;
        }

        public void ResetToOriginal()
        {
            isGrabbed = false;
            isReturning = false;
            currentGrabber = null;
            transform.SetParent(originalParent);
            transform.position = originalPosition;
            transform.rotation = originalRotation;

            if (rb != null)
            {
                rb.isKinematic = wasKinematic;
                rb.useGravity = wasGravity;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (meshRenderer != null && hasOriginalColor)
            {
                meshRenderer.material.color = originalColor;
            }
        }

        // --- Hover Feedback ---

        public void OnHoverEnter()
        {
            if (!isGrabbed && meshRenderer != null)
            {
                meshRenderer.material.color = hoverColor;
            }
        }

        public void OnHoverExit()
        {
            if (!isGrabbed && meshRenderer != null && hasOriginalColor)
            {
                meshRenderer.material.color = originalColor;
            }
        }
    }
}
