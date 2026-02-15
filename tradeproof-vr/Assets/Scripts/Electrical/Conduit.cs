using UnityEngine;
using System.Collections.Generic;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents an EMT (Electrical Metallic Tubing) conduit segment.
    /// Standard trade sizes: 3/4" (0.019m ID) and 1" (0.025m ID).
    /// Default length: 3m (10ft stick).
    /// Visual: LineRenderer-based tube with metallic silver appearance.
    /// Tracks bend points for NEC 358.24 validation (max 360 degrees total bends).
    /// Validates reaming per NEC 358.28.
    /// </summary>
    public class Conduit : MonoBehaviour
    {
        [System.Serializable]
        public struct BendPoint
        {
            public float angle;
            public float distanceFromEnd;
            public Vector3 direction;

            public BendPoint(float angle, float distanceFromEnd, Vector3 direction)
            {
                this.angle = angle;
                this.distanceFromEnd = distanceFromEnd;
                this.direction = direction;
            }
        }

        public enum TradeSize
        {
            ThreeQuarterInch, // 3/4" — 0.019m inner diameter
            OneInch           // 1"   — 0.025m inner diameter
        }

        [Header("Conduit Properties")]
        [SerializeField] private TradeSize tradeSize = TradeSize.ThreeQuarterInch;
        [SerializeField] private float lengthMeters = 3.0f; // 10ft stick
        [SerializeField] private string conduitType = "EMT"; // Electrical Metallic Tubing

        [Header("Bend Tracking")]
        [SerializeField] private List<BendPoint> bendPoints = new List<BendPoint>();

        [Header("Reaming State")]
        [SerializeField] private bool isReamed;

        [Header("Visual")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Color conduitColor = new Color(0.75f, 0.75f, 0.78f, 1f); // Metallic silver
        [SerializeField] private int lineSegmentsPerBend = 8;

        private float innerDiameter;
        private float outerDiameter;

        public TradeSize Size => tradeSize;
        public float LengthMeters => lengthMeters;
        public bool IsReamed => isReamed;
        public List<BendPoint> BendPoints => new List<BendPoint>(bendPoints);
        public int BendCount => bendPoints.Count;

        private void Awake()
        {
            CalculateDimensions();
            BuildVisual();
        }

        private void CalculateDimensions()
        {
            switch (tradeSize)
            {
                case TradeSize.ThreeQuarterInch:
                    innerDiameter = 0.019f;
                    outerDiameter = 0.023f;
                    break;
                case TradeSize.OneInch:
                    innerDiameter = 0.025f;
                    outerDiameter = 0.030f;
                    break;
            }
        }

        private void BuildVisual()
        {
            // Create the LineRenderer for the conduit tube
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            // Configure LineRenderer for tube appearance
            lineRenderer.startWidth = outerDiameter;
            lineRenderer.endWidth = outerDiameter;
            lineRenderer.numCapVertices = 6;
            lineRenderer.numCornerVertices = 6;
            lineRenderer.useWorldSpace = false;

            // Material setup — metallic silver
            Material conduitMat = new Material(Shader.Find("Standard"));
            conduitMat.color = conduitColor;
            conduitMat.SetFloat("_Metallic", 0.85f);
            conduitMat.SetFloat("_Glossiness", 0.6f);
            lineRenderer.material = conduitMat;

            // Add a capsule collider along the length for interaction
            CapsuleCollider col = GetComponent<CapsuleCollider>();
            if (col == null)
            {
                col = gameObject.AddComponent<CapsuleCollider>();
            }
            col.direction = 2; // Z-axis (along conduit length)
            col.radius = outerDiameter / 2f;
            col.height = lengthMeters;
            col.center = new Vector3(0f, 0f, lengthMeters / 2f);

            // Initial straight run
            UpdateVisual();
        }

        /// <summary>
        /// Adds a bend point to the conduit. Validates that total bends
        /// do not exceed 360 degrees per NEC 358.24.
        /// </summary>
        public bool AddBend(float angle, float distanceFromEnd)
        {
            return AddBend(angle, distanceFromEnd, Vector3.up);
        }

        /// <summary>
        /// Adds a bend point with a specified direction.
        /// </summary>
        public bool AddBend(float angle, float distanceFromEnd, Vector3 direction)
        {
            // Validate angle
            if (angle <= 0f || angle > 180f)
            {
                Debug.LogWarning($"[Conduit] Invalid bend angle: {angle}. Must be between 0 and 180 degrees.");
                return false;
            }

            // Validate distance from end
            if (distanceFromEnd < 0f || distanceFromEnd > lengthMeters)
            {
                Debug.LogWarning($"[Conduit] Invalid bend position: {distanceFromEnd}m. Must be within conduit length ({lengthMeters}m).");
                return false;
            }

            // Check NEC 358.24: total bends must not exceed 360 degrees
            float totalAfterBend = GetTotalBendAngle() + angle;
            if (totalAfterBend > 360f)
            {
                Debug.LogWarning($"[Conduit] NEC 358.24 VIOLATION — Total bends ({totalAfterBend:F1} degrees) would exceed 360 degrees between pull points.");
                return false;
            }

            BendPoint newBend = new BendPoint(angle, distanceFromEnd, direction.normalized);
            bendPoints.Add(newBend);

            // Sort bends by distance from end for proper rendering
            bendPoints.Sort((a, b) => a.distanceFromEnd.CompareTo(b.distanceFromEnd));

            UpdateVisual();

            Debug.Log($"[Conduit] Bend added: {angle} degrees at {distanceFromEnd:F2}m from end. Total: {GetTotalBendAngle():F1} degrees.");
            return true;
        }

        /// <summary>
        /// Returns the sum of all bend angles in this conduit run.
        /// </summary>
        public float GetTotalBendAngle()
        {
            float total = 0f;
            foreach (BendPoint bend in bendPoints)
            {
                total += bend.angle;
            }
            return total;
        }

        /// <summary>
        /// Marks the conduit end as properly reamed per NEC 358.28.
        /// Cut ends of EMT must be reamed to remove burrs that could damage wire insulation.
        /// </summary>
        public void SetReamed(bool reamed)
        {
            isReamed = reamed;

            if (reamed)
            {
                Debug.Log("[Conduit] Conduit end reamed — NEC 358.28 compliant.");
            }
            else
            {
                Debug.Log("[Conduit] Conduit end NOT reamed — potential NEC 358.28 violation.");
            }
        }

        /// <summary>
        /// Validates the entire conduit run:
        /// - Total bends must be less than 360 degrees (NEC 358.24)
        /// - All cut ends must be reamed (NEC 358.28)
        /// Returns true if the run passes all checks.
        /// </summary>
        public bool ValidateRun()
        {
            bool valid = true;

            // NEC 358.24 — Maximum total bends
            float totalBends = GetTotalBendAngle();
            if (totalBends > 360f)
            {
                Debug.LogWarning($"[Conduit] NEC 358.24 VIOLATION — Total bends ({totalBends:F1} degrees) exceed 360 degrees between pull points.");
                valid = false;
            }

            // NEC 358.28 — Reaming
            if (!isReamed)
            {
                Debug.LogWarning("[Conduit] NEC 358.28 VIOLATION — Conduit end not reamed. Burrs may damage wire insulation.");
                valid = false;
            }

            if (valid)
            {
                Debug.Log($"[Conduit] Run validated: {totalBends:F1} degrees total bends, reamed: {isReamed}.");
            }

            return valid;
        }

        /// <summary>
        /// Redraws the LineRenderer with bend deformations applied.
        /// Straight segments connect between bends, with arcs at each bend point.
        /// </summary>
        public void UpdateVisual()
        {
            if (lineRenderer == null) return;

            List<Vector3> points = new List<Vector3>();

            if (bendPoints.Count == 0)
            {
                // Straight conduit run along local Z-axis
                points.Add(Vector3.zero);
                points.Add(new Vector3(0f, 0f, lengthMeters));
            }
            else
            {
                // Build path with bends
                Vector3 currentPos = Vector3.zero;
                Vector3 currentDir = Vector3.forward;
                float lastDistance = 0f;

                points.Add(currentPos);

                foreach (BendPoint bend in bendPoints)
                {
                    // Straight segment to bend point
                    float segmentLength = bend.distanceFromEnd - lastDistance;
                    if (segmentLength > 0f)
                    {
                        currentPos += currentDir * segmentLength;
                        points.Add(currentPos);
                    }

                    // Generate arc for the bend
                    Vector3 bendAxis = Vector3.Cross(currentDir, bend.direction);
                    if (bendAxis.sqrMagnitude < 0.001f)
                    {
                        bendAxis = Vector3.Cross(currentDir, Vector3.up);
                        if (bendAxis.sqrMagnitude < 0.001f)
                        {
                            bendAxis = Vector3.Cross(currentDir, Vector3.right);
                        }
                    }
                    bendAxis.Normalize();

                    // Minimum bend radius (approximately 6x trade size for EMT)
                    float bendRadius = outerDiameter * 6f;
                    float angleStep = bend.angle / lineSegmentsPerBend;

                    for (int i = 1; i <= lineSegmentsPerBend; i++)
                    {
                        float stepAngle = angleStep * i;
                        Quaternion rotation = Quaternion.AngleAxis(stepAngle, bendAxis);
                        Vector3 arcPoint = currentPos + rotation * (currentDir * bendRadius) - currentDir * bendRadius;

                        // Offset arc point using the bend radius
                        Vector3 radiusOffset = rotation * (-bendAxis * bendRadius) + bendAxis * bendRadius;
                        Vector3 bendPoint = currentPos + (rotation * currentDir) * bendRadius * Mathf.Sin(stepAngle * Mathf.Deg2Rad)
                                           + (1f - Mathf.Cos(stepAngle * Mathf.Deg2Rad)) * bendRadius * Vector3.Cross(bendAxis, currentDir).normalized;

                        points.Add(bendPoint);
                    }

                    // Update direction after bend
                    Quaternion fullRotation = Quaternion.AngleAxis(bend.angle, bendAxis);
                    currentDir = fullRotation * currentDir;

                    lastDistance = bend.distanceFromEnd;
                }

                // Final straight segment to end
                float remainingLength = lengthMeters - lastDistance;
                if (remainingLength > 0f)
                {
                    currentPos = points[points.Count - 1];
                    points.Add(currentPos + currentDir * remainingLength);
                }
            }

            // Apply points to LineRenderer
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());

            // Update collider center based on new geometry
            CapsuleCollider col = GetComponent<CapsuleCollider>();
            if (col != null && points.Count >= 2)
            {
                Vector3 midpoint = (points[0] + points[points.Count - 1]) / 2f;
                col.center = midpoint;
                col.height = Vector3.Distance(points[0], points[points.Count - 1]);
            }
        }

        /// <summary>
        /// Removes all bends and resets to a straight conduit.
        /// </summary>
        public void ClearBends()
        {
            bendPoints.Clear();
            UpdateVisual();
            Debug.Log("[Conduit] All bends cleared.");
        }

        /// <summary>
        /// Returns the inner diameter based on trade size.
        /// </summary>
        public float GetInnerDiameter()
        {
            return innerDiameter;
        }

        /// <summary>
        /// Returns the outer diameter based on trade size.
        /// </summary>
        public float GetOuterDiameter()
        {
            return outerDiameter;
        }

        /// <summary>
        /// Get NEC code violations related to this conduit run.
        /// </summary>
        public string[] GetViolations()
        {
            List<string> violations = new List<string>();

            float totalBends = GetTotalBendAngle();
            if (totalBends > 360f)
            {
                violations.Add($"NEC 358.24: Total bends ({totalBends:F1} degrees) exceed 360 degrees between pull points. A pull box or junction is required.");
            }

            if (!isReamed)
            {
                violations.Add("NEC 358.28: Cut conduit end is not reamed. Burrs may damage conductor insulation during pulling.");
            }

            return violations.ToArray();
        }

        /// <summary>
        /// Get a human-readable description of this conduit segment.
        /// </summary>
        public string GetDescription()
        {
            string sizeLabel = tradeSize == TradeSize.ThreeQuarterInch ? "3/4\"" : "1\"";
            return $"{conduitType} Conduit — {sizeLabel} trade size, {lengthMeters:F1}m length, {bendPoints.Count} bends ({GetTotalBendAngle():F0} degrees total), Reamed: {isReamed}";
        }
    }
}
