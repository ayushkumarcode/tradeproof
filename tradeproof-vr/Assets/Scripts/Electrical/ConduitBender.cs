using UnityEngine;
using TradeProof.Interaction;
using TradeProof.UI;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Tool for bending EMT conduit. Grabbable via GrabInteractable component.
    /// Visual: bender shoe (curved, built from multiple small cubes) + elongated handle.
    /// When held near a Conduit (within 0.1m), shows a bend angle guide via FloatingLabel.
    /// Applies bends to conduit at the contact position.
    /// Colors: orange handle (0.9, 0.4, 0.1), steel shoe (0.5, 0.5, 0.52).
    /// </summary>
    public class ConduitBender : MonoBehaviour
    {
        [Header("Tool Properties")]
        [SerializeField] private string toolType = "conduit-bender";

        [Header("Bend State")]
        [SerializeField] private float currentBendAngle;
        [SerializeField] private float maxBendAngle = 180f;
        [SerializeField] private float bendAngleIncrement = 5f;

        [Header("Detection")]
        [SerializeField] private float detectionRadius = 0.1f;
        [SerializeField] private Transform shoeTransform;

        [Header("Visual")]
        [SerializeField] private Color handleColor = new Color(0.9f, 0.4f, 0.1f, 1f); // Orange
        [SerializeField] private Color shoeColor = new Color(0.5f, 0.5f, 0.52f, 1f);  // Steel

        [Header("Dimensions")]
        [SerializeField] private float handleLength = 0.6f;
        [SerializeField] private float handleDiameter = 0.03f;
        [SerializeField] private float shoeWidth = 0.15f;
        [SerializeField] private float shoeHeight = 0.08f;
        [SerializeField] private int shoeCurveSegments = 6;

        // Components
        private GrabInteractable grabInteractable;
        private FloatingLabel angleLabel;
        private Conduit nearbyConduit;

        // Visual references
        private GameObject handleObj;
        private GameObject shoeObj;
        private GameObject angleIndicator;
        private MeshRenderer[] shoeSegmentRenderers;

        public float CurrentBendAngle => currentBendAngle;
        public string ToolType => toolType;

        private void Awake()
        {
            BuildVisual();
            SetupGrabInteractable();
            CreateAngleLabel();
        }

        private void Update()
        {
            CheckForNearbyConduit();
            UpdateAngleIndicator();
        }

        private void BuildVisual()
        {
            // --- Handle: elongated cylinder ---
            handleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handleObj.name = "BenderHandle";
            handleObj.transform.SetParent(transform, false);
            // Handle extends upward from the shoe
            handleObj.transform.localPosition = new Vector3(0f, handleLength / 2f + shoeHeight / 2f, 0f);
            handleObj.transform.localScale = new Vector3(handleDiameter, handleLength / 2f, handleDiameter);

            Material handleMat = new Material(Shader.Find("Standard"));
            handleMat.color = handleColor;
            handleMat.SetFloat("_Metallic", 0.3f);
            handleMat.SetFloat("_Glossiness", 0.4f);
            handleObj.GetComponent<MeshRenderer>().material = handleMat;

            // Remove handle collider (main collider is on parent)
            Collider handleCol = handleObj.GetComponent<Collider>();
            if (handleCol != null) Destroy(handleCol);

            // --- Shoe: curved shape built from multiple small cubes ---
            shoeObj = new GameObject("BenderShoe");
            shoeObj.transform.SetParent(transform, false);
            shoeObj.transform.localPosition = Vector3.zero;
            shoeTransform = shoeObj.transform;

            Material shoeMat = new Material(Shader.Find("Standard"));
            shoeMat.color = shoeColor;
            shoeMat.SetFloat("_Metallic", 0.7f);
            shoeMat.SetFloat("_Glossiness", 0.5f);

            shoeSegmentRenderers = new MeshRenderer[shoeCurveSegments];

            // Build curved shoe from small cubes arranged in an arc
            float arcAngle = 90f; // Quarter-circle shoe
            float segmentWidth = shoeWidth / shoeCurveSegments;
            float curveRadius = shoeHeight;

            for (int i = 0; i < shoeCurveSegments; i++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"ShoeSegment_{i}";
                segment.transform.SetParent(shoeObj.transform, false);

                // Position each segment along the curve
                float t = (float)i / (shoeCurveSegments - 1);
                float angle = t * arcAngle * Mathf.Deg2Rad;

                float xPos = -shoeWidth / 2f + i * segmentWidth + segmentWidth / 2f;
                float yPos = Mathf.Sin(angle) * curveRadius * 0.3f - shoeHeight / 2f;
                float zPos = 0f;

                segment.transform.localPosition = new Vector3(xPos, yPos, zPos);
                segment.transform.localScale = new Vector3(segmentWidth * 1.05f, 0.015f, 0.04f);

                // Slight rotation to follow the curve
                float rotAngle = Mathf.Lerp(-15f, 15f, t);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, rotAngle);

                segment.GetComponent<MeshRenderer>().material = new Material(shoeMat);
                shoeSegmentRenderers[i] = segment.GetComponent<MeshRenderer>();

                // Remove individual colliders
                Collider segCol = segment.GetComponent<Collider>();
                if (segCol != null) Destroy(segCol);
            }

            // --- Main collider for the entire tool ---
            BoxCollider mainCol = gameObject.GetComponent<BoxCollider>();
            if (mainCol == null)
            {
                mainCol = gameObject.AddComponent<BoxCollider>();
            }
            mainCol.center = new Vector3(0f, handleLength / 2f, 0f);
            mainCol.size = new Vector3(shoeWidth, handleLength + shoeHeight, 0.06f);

            // --- Angle indicator arc (visual guide) ---
            BuildAngleIndicator();
        }

        private void BuildAngleIndicator()
        {
            angleIndicator = new GameObject("AngleIndicator");
            angleIndicator.transform.SetParent(shoeTransform, false);
            angleIndicator.transform.localPosition = new Vector3(0f, -shoeHeight / 2f, 0.03f);

            // Create arc segments for the angle guide
            LineRenderer arcLine = angleIndicator.AddComponent<LineRenderer>();
            arcLine.startWidth = 0.003f;
            arcLine.endWidth = 0.003f;
            arcLine.useWorldSpace = false;
            arcLine.positionCount = 0;

            Material arcMat = new Material(Shader.Find("Standard"));
            arcMat.color = new Color(0f, 1f, 0.5f, 0.8f); // Bright green guide
            arcMat.SetFloat("_Mode", 3); // Transparent
            arcMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            arcMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            arcMat.SetInt("_ZWrite", 0);
            arcMat.EnableKeyword("_ALPHABLEND_ON");
            arcMat.renderQueue = 3000;
            arcLine.material = arcMat;

            angleIndicator.SetActive(false);
        }

        private void SetupGrabInteractable()
        {
            grabInteractable = GetComponent<GrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = gameObject.AddComponent<GrabInteractable>();
            }
            grabInteractable.SetToolType(toolType);
            grabInteractable.SetGripOffset(new Vector3(0f, handleLength * 0.6f, 0f));
        }

        private void CreateAngleLabel()
        {
            GameObject labelObj = new GameObject("BendAngleLabel");
            labelObj.transform.SetParent(transform, false);
            labelObj.transform.localPosition = new Vector3(0f, -shoeHeight - 0.03f, 0.03f);

            angleLabel = labelObj.AddComponent<FloatingLabel>();
            angleLabel.SetText("");
            angleLabel.SetFontSize(2f);
            angleLabel.SetColor(Color.white);
            labelObj.SetActive(false);
        }

        /// <summary>
        /// Checks for nearby Conduit components within detection radius of the shoe.
        /// When found, shows the bend angle guide label.
        /// </summary>
        private void CheckForNearbyConduit()
        {
            if (shoeTransform == null) return;

            Collider[] nearby = Physics.OverlapSphere(shoeTransform.position, detectionRadius);
            nearbyConduit = null;

            foreach (Collider col in nearby)
            {
                Conduit conduit = col.GetComponent<Conduit>();
                if (conduit == null)
                    conduit = col.GetComponentInParent<Conduit>();

                if (conduit != null)
                {
                    nearbyConduit = conduit;
                    break;
                }
            }

            // Show/hide angle label
            if (angleLabel != null)
            {
                bool showLabel = nearbyConduit != null && grabInteractable.IsGrabbed;
                angleLabel.gameObject.SetActive(showLabel);

                if (showLabel)
                {
                    float totalBends = nearbyConduit.GetTotalBendAngle();
                    float remaining = 360f - totalBends;
                    angleLabel.SetText($"Bend: {currentBendAngle:F0} deg\nTotal: {totalBends:F0}/360 deg\nRemaining: {remaining:F0} deg");

                    // Color warning when approaching limit
                    if (remaining < 90f)
                    {
                        angleLabel.SetColor(new Color(1f, 0.5f, 0f)); // Orange warning
                    }
                    else if (remaining < 45f)
                    {
                        angleLabel.SetColor(Color.red); // Red danger
                    }
                    else
                    {
                        angleLabel.SetColor(Color.white);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the visual angle indicator arc based on currentBendAngle.
        /// </summary>
        private void UpdateAngleIndicator()
        {
            if (angleIndicator == null) return;

            bool showIndicator = nearbyConduit != null && grabInteractable.IsGrabbed && currentBendAngle > 0f;
            angleIndicator.SetActive(showIndicator);

            if (!showIndicator) return;

            LineRenderer arcLine = angleIndicator.GetComponent<LineRenderer>();
            if (arcLine == null) return;

            // Draw arc representing the current bend angle
            int arcSegments = 20;
            arcLine.positionCount = arcSegments + 1;

            float arcRadius = 0.05f;
            for (int i = 0; i <= arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                float angle = t * currentBendAngle * Mathf.Deg2Rad;
                float x = Mathf.Sin(angle) * arcRadius;
                float y = -Mathf.Cos(angle) * arcRadius + arcRadius;

                arcLine.SetPosition(i, new Vector3(x, y, 0f));
            }
        }

        /// <summary>
        /// Applies a bend to the specified conduit at the current shoe position.
        /// Uses the shoe's world position to calculate distance from conduit end.
        /// </summary>
        public bool ApplyBend(Conduit conduit, float angle)
        {
            if (conduit == null)
            {
                Debug.LogWarning("[ConduitBender] No conduit specified.");
                return false;
            }

            if (angle <= 0f || angle > maxBendAngle)
            {
                Debug.LogWarning($"[ConduitBender] Invalid bend angle: {angle}. Must be between 0 and {maxBendAngle} degrees.");
                return false;
            }

            // Calculate distance from the conduit's start based on shoe position
            Vector3 shoeWorldPos = shoeTransform != null ? shoeTransform.position : transform.position;
            Vector3 localPos = conduit.transform.InverseTransformPoint(shoeWorldPos);
            float distanceFromEnd = Mathf.Clamp(localPos.z, 0f, conduit.LengthMeters);

            // Direction of the bend based on shoe orientation
            Vector3 bendDirection = shoeTransform != null ? shoeTransform.up : transform.up;

            bool success = conduit.AddBend(angle, distanceFromEnd, bendDirection);

            if (success)
            {
                Debug.Log($"[ConduitBender] Applied {angle} degree bend at {distanceFromEnd:F2}m from end.");
                currentBendAngle = 0f; // Reset after applying
            }

            return success;
        }

        /// <summary>
        /// Sets the target bend angle being formed. Used during the bending motion.
        /// Clamped between 0 and maxBendAngle.
        /// </summary>
        public void SetBendAngle(float angle)
        {
            currentBendAngle = Mathf.Clamp(angle, 0f, maxBendAngle);
        }

        /// <summary>
        /// Increments the current bend angle by the preset increment amount.
        /// </summary>
        public void IncrementBendAngle()
        {
            currentBendAngle = Mathf.Min(currentBendAngle + bendAngleIncrement, maxBendAngle);
        }

        /// <summary>
        /// Decrements the current bend angle by the preset increment amount.
        /// </summary>
        public void DecrementBendAngle()
        {
            currentBendAngle = Mathf.Max(currentBendAngle - bendAngleIncrement, 0f);
        }

        /// <summary>
        /// Attempts to apply the current bend angle to the nearest detected conduit.
        /// Returns true if the bend was successfully applied.
        /// </summary>
        public bool ApplyCurrentBend()
        {
            if (nearbyConduit == null)
            {
                Debug.LogWarning("[ConduitBender] No conduit nearby to bend.");
                return false;
            }

            if (currentBendAngle <= 0f)
            {
                Debug.LogWarning("[ConduitBender] No bend angle set.");
                return false;
            }

            return ApplyBend(nearbyConduit, currentBendAngle);
        }

        /// <summary>
        /// Returns the currently detected nearby conduit, or null if none in range.
        /// </summary>
        public Conduit GetNearbyConduit()
        {
            return nearbyConduit;
        }
    }
}
