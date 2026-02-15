using UnityEngine;
using System.Collections.Generic;
using TradeProof.Training;

namespace TradeProof.Interaction
{
    /// <summary>
    /// Handles both hand tracking (Meta Hand Tracking SDK) and controller input.
    /// Supports ray casting for pointing (panel inspection) and grab gestures for picking up objects.
    ///
    /// Grip offsets per tool type:
    ///   Wire: held between thumb and index, offset (0, -0.02, 0.05) from hand center
    ///   Screwdriver: held in fist, offset (0, 0, 0.08) from hand center
    ///   Wire strippers: held in fist, offset (0, 0, 0.06) from hand center
    /// </summary>
    public class HandInteraction : MonoBehaviour
    {
        [Header("Hand Tracking")]
        [SerializeField] private OVRHand ovrHand;
        [SerializeField] private OVRSkeleton ovrSkeleton;
        [SerializeField] private OVRHand.Hand handType = OVRHand.Hand.HandRight;

        [Header("Controller")]
        [SerializeField] private OVRInput.Controller controllerType = OVRInput.Controller.RTouch;

        [Header("Ray Settings")]
        [SerializeField] private float rayLength = 5f;
        [SerializeField] private float rayWidth = 0.005f;
        [SerializeField] private LayerMask interactableLayers = ~0;
        [SerializeField] private Color rayColorIdle = new Color(0.5f, 0.5f, 1f, 0.5f);
        [SerializeField] private Color rayColorHover = new Color(0f, 1f, 0f, 0.8f);
        [SerializeField] private Color rayColorGrab = new Color(1f, 0.5f, 0f, 0.8f);

        [Header("Grab Settings")]
        [SerializeField] private float grabRadius = 0.05f;
        [SerializeField] private float pinchThreshold = 0.7f;
        [SerializeField] private float gripThreshold = 0.7f;

        [Header("Tracking Space")]
        [SerializeField] private Transform trackingSpace;

        [Header("Visual")]
        [SerializeField] private LineRenderer rayLineRenderer;
        [SerializeField] private GameObject rayHitIndicator;

        // State
        private bool isHandTracking;
        private bool isPinching;
        private bool isGripping;
        private bool wasPinching;
        private bool wasGripping;
        private GrabInteractable currentGrabbedObject;
        private GrabInteractable hoveredObject;
        private ViolationMarker hoveredViolation;

        // Hand bone tracking
        private Transform handCenter;
        private Transform indexTip;
        private Transform thumbTip;

        // Grip offsets per tool type
        private static readonly Dictionary<string, Vector3> GripOffsets = new Dictionary<string, Vector3>
        {
            { "wire", new Vector3(0f, -0.02f, 0.05f) },
            { "screwdriver", new Vector3(0f, 0f, 0.08f) },
            { "wire_strippers", new Vector3(0f, 0f, 0.06f) },
            { "default", new Vector3(0f, 0f, 0.04f) }
        };

        private void Awake()
        {
            SetupRayVisual();
        }

        private void Start()
        {
            // Find tracking space
            if (trackingSpace == null)
            {
                OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
                if (cameraRig != null)
                {
                    trackingSpace = cameraRig.trackingSpace;
                }
            }

            // Find hand components
            if (ovrHand == null)
            {
                OVRHand[] hands = FindObjectsOfType<OVRHand>();
                foreach (var hand in hands)
                {
                    // Identify the correct hand via public GetHand() API
                    if ((OVRHand.Hand)hand.GetHand() == handType)
                    {
                        ovrHand = hand;
                        break;
                    }
                }
            }

            if (ovrSkeleton == null && ovrHand != null)
            {
                ovrSkeleton = ovrHand.GetComponent<OVRSkeleton>();
            }
        }

        private void SetupRayVisual()
        {
            if (rayLineRenderer == null)
            {
                rayLineRenderer = gameObject.AddComponent<LineRenderer>();
                rayLineRenderer.positionCount = 2;
                rayLineRenderer.startWidth = rayWidth;
                rayLineRenderer.endWidth = rayWidth * 0.5f;

                Material rayMat = new Material(Shader.Find("Standard"));
                rayMat.SetFloat("_Mode", 3); // Transparent
                rayMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                rayMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                rayMat.SetInt("_ZWrite", 0);
                rayMat.EnableKeyword("_ALPHABLEND_ON");
                rayMat.renderQueue = 3000;
                rayMat.color = rayColorIdle;
                rayLineRenderer.material = rayMat;
            }

            if (rayHitIndicator == null)
            {
                rayHitIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rayHitIndicator.transform.localScale = Vector3.one * 0.01f;
                Destroy(rayHitIndicator.GetComponent<Collider>());

                Material hitMat = new Material(Shader.Find("Standard"));
                hitMat.color = Color.white;
                hitMat.SetColor("_EmissionColor", Color.white * 0.5f);
                hitMat.EnableKeyword("_EMISSION");
                rayHitIndicator.GetComponent<MeshRenderer>().material = hitMat;
                rayHitIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateInputState();
            UpdateRay();
            UpdateGrab();
        }

        // --- Input Detection ---

        private void UpdateInputState()
        {
            wasPinching = isPinching;
            wasGripping = isGripping;

            isHandTracking = ovrHand != null && ovrHand.IsTracked;

            if (isHandTracking)
            {
                // Hand tracking pinch and grip detection
                isPinching = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) >= pinchThreshold;
                isGripping = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) >= gripThreshold &&
                             ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) >= gripThreshold;

                // Update bone transforms
                UpdateHandBones();
            }
            else
            {
                // Controller input
                isPinching = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerType) >= 0.5f;
                isGripping = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controllerType) >= 0.5f;
            }
        }

        private void UpdateHandBones()
        {
            if (ovrSkeleton == null || ovrSkeleton.Bones == null || ovrSkeleton.Bones.Count == 0) return;

            foreach (var bone in ovrSkeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_Index3) // Index fingertip
                {
                    indexTip = bone.Transform;
                }
                else if (bone.Id == OVRSkeleton.BoneId.Hand_Thumb3) // Thumb tip
                {
                    thumbTip = bone.Transform;
                }
                else if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
                {
                    handCenter = bone.Transform;
                }
            }
        }

        // --- Ray Casting ---

        private void UpdateRay()
        {
            Vector3 rayOrigin;
            Vector3 rayDirection;

            if (isHandTracking && indexTip != null)
            {
                // Ray from index fingertip
                rayOrigin = indexTip.position;
                rayDirection = indexTip.forward;
            }
            else
            {
                // Ray from controller
                Vector3 localPos = OVRInput.GetLocalControllerPosition(controllerType);
                Quaternion localRot = OVRInput.GetLocalControllerRotation(controllerType);

                if (trackingSpace != null)
                {
                    rayOrigin = trackingSpace.TransformPoint(localPos);
                    rayDirection = trackingSpace.TransformDirection(localRot * Vector3.forward);
                }
                else
                {
                    rayOrigin = localPos;
                    rayDirection = localRot * Vector3.forward;
                }
            }

            // Perform raycast
            RaycastHit hit;
            bool didHit = Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength, interactableLayers);

            // Update hover state
            hoveredObject = null;
            hoveredViolation = null;

            if (didHit)
            {
                // Check for GrabInteractable
                hoveredObject = hit.collider.GetComponent<GrabInteractable>();
                if (hoveredObject == null)
                    hoveredObject = hit.collider.GetComponentInParent<GrabInteractable>();

                // Check for ViolationMarker
                hoveredViolation = hit.collider.GetComponent<ViolationMarker>();
                if (hoveredViolation == null)
                    hoveredViolation = hit.collider.GetComponentInParent<ViolationMarker>();
            }

            // Update ray visual
            UpdateRayVisual(rayOrigin, rayDirection, didHit, hit);

            // Handle pointing at violations
            if (didHit && hoveredViolation != null)
            {
                // Pinch to select violation
                if (isPinching && !wasPinching)
                {
                    hoveredViolation.TryIdentify();
                }
            }
        }

        private void UpdateRayVisual(Vector3 origin, Vector3 direction, bool didHit, RaycastHit hit)
        {
            if (rayLineRenderer == null) return;

            // Only show ray when not grabbing
            bool showRay = currentGrabbedObject == null;
            rayLineRenderer.enabled = showRay;

            if (!showRay) return;

            Vector3 endPoint = didHit ? hit.point : origin + direction * rayLength;

            rayLineRenderer.SetPosition(0, origin);
            rayLineRenderer.SetPosition(1, endPoint);

            // Update color
            Color rayColor = rayColorIdle;
            if (hoveredObject != null || hoveredViolation != null)
            {
                rayColor = rayColorHover;
            }
            if (isPinching)
            {
                rayColor = rayColorGrab;
            }
            rayLineRenderer.material.color = rayColor;

            // Update hit indicator
            if (rayHitIndicator != null)
            {
                rayHitIndicator.SetActive(didHit);
                if (didHit)
                {
                    rayHitIndicator.transform.position = hit.point;
                    rayHitIndicator.transform.rotation = Quaternion.LookRotation(hit.normal);
                }
            }
        }

        // --- Grabbing ---

        private void UpdateGrab()
        {
            // Grab detection
            bool grabTriggered = false;
            bool releaseTriggered = false;

            if (isHandTracking)
            {
                // Grab on pinch start or grip start
                grabTriggered = (isPinching && !wasPinching) || (isGripping && !wasGripping);
                releaseTriggered = (!isPinching && wasPinching) || (!isGripping && wasGripping);
            }
            else
            {
                // Controller: grab trigger
                grabTriggered = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controllerType);
                releaseTriggered = OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, controllerType);
            }

            // Handle grab
            if (grabTriggered && currentGrabbedObject == null)
            {
                TryGrab();
            }

            // Handle release
            if (releaseTriggered && currentGrabbedObject != null)
            {
                ReleaseGrab();
            }

            // Update grabbed object position
            if (currentGrabbedObject != null)
            {
                UpdateGrabbedObjectPosition();
            }
        }

        private void TryGrab()
        {
            Vector3 grabCenter = GetGrabPosition();

            // Find closest grabbable within radius
            Collider[] nearby = Physics.OverlapSphere(grabCenter, grabRadius, interactableLayers);
            float closestDist = float.MaxValue;
            GrabInteractable closest = null;

            foreach (var col in nearby)
            {
                GrabInteractable interactable = col.GetComponent<GrabInteractable>();
                if (interactable == null)
                    interactable = col.GetComponentInParent<GrabInteractable>();

                if (interactable != null && interactable.IsGrabbable)
                {
                    float dist = Vector3.Distance(grabCenter, interactable.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }

            // Also check if hovering via ray
            if (closest == null && hoveredObject != null && hoveredObject.IsGrabbable)
            {
                closest = hoveredObject;
            }

            if (closest != null)
            {
                currentGrabbedObject = closest;
                currentGrabbedObject.OnGrabbed(this);
                Core.AudioManager.Instance.PlayGrabSound();
                Debug.Log($"[HandInteraction] Grabbed: {currentGrabbedObject.name}");
            }
        }

        private void ReleaseGrab()
        {
            if (currentGrabbedObject != null)
            {
                currentGrabbedObject.OnReleased(this);
                Core.AudioManager.Instance.PlayReleaseSound();
                Debug.Log($"[HandInteraction] Released: {currentGrabbedObject.name}");
                currentGrabbedObject = null;
            }
        }

        private void UpdateGrabbedObjectPosition()
        {
            if (currentGrabbedObject == null) return;

            Vector3 handPos = GetGrabPosition();
            Quaternion handRot = GetGrabRotation();

            // Apply tool-specific grip offset
            Vector3 gripOffset = GetGripOffsetForObject(currentGrabbedObject);
            Vector3 offsetPosition = handPos + handRot * gripOffset;

            currentGrabbedObject.UpdateGrabbedPosition(offsetPosition, handRot);
        }

        // --- Position Helpers ---

        private Vector3 GetGrabPosition()
        {
            if (isHandTracking)
            {
                // Use midpoint between thumb and index for pinch grab
                if (isPinching && thumbTip != null && indexTip != null)
                {
                    return Vector3.Lerp(thumbTip.position, indexTip.position, 0.5f);
                }
                // Use hand center for grip grab
                if (handCenter != null)
                {
                    return handCenter.position;
                }
            }

            // Controller position
            Vector3 localPos = OVRInput.GetLocalControllerPosition(controllerType);
            if (trackingSpace != null)
            {
                return trackingSpace.TransformPoint(localPos);
            }
            return localPos;
        }

        private Quaternion GetGrabRotation()
        {
            if (isHandTracking && handCenter != null)
            {
                return handCenter.rotation;
            }

            Quaternion localRot = OVRInput.GetLocalControllerRotation(controllerType);
            if (trackingSpace != null)
            {
                return trackingSpace.rotation * localRot;
            }
            return localRot;
        }

        private Vector3 GetGripOffsetForObject(GrabInteractable obj)
        {
            // Use object's defined grip offset if available
            Vector3 customOffset = obj.GetGripOffset();
            if (customOffset != Vector3.zero)
            {
                return customOffset;
            }

            // Fall back to type-based offsets
            string toolType = obj.ToolType;
            if (!string.IsNullOrEmpty(toolType) && GripOffsets.TryGetValue(toolType, out Vector3 offset))
            {
                return offset;
            }

            return GripOffsets["default"];
        }

        // --- Public API ---

        public bool IsGrabbing => currentGrabbedObject != null;
        public GrabInteractable GrabbedObject => currentGrabbedObject;
        public bool IsPinching => isPinching;
        public bool IsGripping => isGripping;
        public bool IsHandTracking => isHandTracking;

        public void ForceRelease()
        {
            if (currentGrabbedObject != null)
            {
                ReleaseGrab();
            }
        }

        public Vector3 GetPointerPosition()
        {
            return GetGrabPosition();
        }

        public Vector3 GetPointerDirection()
        {
            if (isHandTracking && indexTip != null)
            {
                return indexTip.forward;
            }

            Quaternion localRot = OVRInput.GetLocalControllerRotation(controllerType);
            if (trackingSpace != null)
            {
                return trackingSpace.TransformDirection(localRot * Vector3.forward);
            }
            return localRot * Vector3.forward;
        }
    }
}
