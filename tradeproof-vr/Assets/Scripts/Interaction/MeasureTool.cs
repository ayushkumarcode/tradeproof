using UnityEngine;
using TradeProof.UI;
using TradeProof.Core;

namespace TradeProof.Interaction
{
    /// <summary>
    /// Tape measure / ruler tool for measuring distances in the training environment.
    /// Extends a visual tape (LineRenderer) from the tool body to the measurement point.
    /// Displays the measured distance on a FloatingLabel above the tool.
    ///
    /// Visual: small rectangular body 0.05m x 0.03m x 0.02m with extending tape.
    /// toolType = "tape-measure"
    /// </summary>
    public class MeasureTool : MonoBehaviour
    {
        [Header("Tool Properties")]
        [SerializeField] private string toolType = "tape-measure";
        [SerializeField] private float maxMeasureDistance = 5f; // meters
        [SerializeField] private float minMeasureDistance = 0.01f;

        [Header("Visual — Body")]
        [SerializeField] private float bodyWidth = 0.05f;
        [SerializeField] private float bodyHeight = 0.03f;
        [SerializeField] private float bodyDepth = 0.02f;
        [SerializeField] private Color bodyColor = new Color(0.9f, 0.7f, 0.1f, 1f); // Yellow tape measure
        [SerializeField] private MeshRenderer bodyRenderer;

        [Header("Visual — Tape")]
        [SerializeField] private LineRenderer tapeLineRenderer;
        [SerializeField] private Color tapeColor = new Color(0.95f, 0.9f, 0.3f, 1f); // Yellow tape
        [SerializeField] private float tapeWidth = 0.003f;
        [SerializeField] private int tapeSegments = 10;

        [Header("Measurement")]
        [SerializeField] private Transform tapeStartPoint; // On the body
        [SerializeField] private Transform tapeEndPoint;   // Extended end (movable)
        [SerializeField] private FloatingLabel measurementLabel;
        [SerializeField] private bool isMeasuring;
        [SerializeField] private float lastMeasuredDistance;

        [Header("Interaction")]
        [SerializeField] private GrabInteractable grabInteractable;
        [SerializeField] private bool isExtended;

        public string ToolType => toolType;
        public float LastMeasuredDistance => lastMeasuredDistance;
        public bool IsMeasuring => isMeasuring;

        private void Awake()
        {
            BuildVisual();
            SetupTapeLine();
            SetupLabel();
            SetupGrabInteractable();
        }

        private void BuildVisual()
        {
            // Create tool body
            if (bodyRenderer == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "TapeMeasureBody";
                body.transform.SetParent(transform, false);
                body.transform.localPosition = Vector3.zero;
                body.transform.localScale = new Vector3(bodyWidth, bodyHeight, bodyDepth);

                bodyRenderer = body.GetComponent<MeshRenderer>();
                Material bodyMat = new Material(Shader.Find("Standard"));
                bodyMat.color = bodyColor;
                bodyRenderer.material = bodyMat;

                // Remove collider from visual; parent will have the collider
                Collider bodyCol = body.GetComponent<Collider>();
                if (bodyCol != null) Destroy(bodyCol);
            }

            // Create tape start point
            if (tapeStartPoint == null)
            {
                GameObject startObj = new GameObject("TapeStart");
                startObj.transform.SetParent(transform, false);
                startObj.transform.localPosition = new Vector3(bodyWidth / 2f, 0f, 0f);
                tapeStartPoint = startObj.transform;
            }

            // Create tape end point (initially retracted)
            if (tapeEndPoint == null)
            {
                GameObject endObj = new GameObject("TapeEnd");
                endObj.transform.SetParent(transform, false);
                endObj.transform.localPosition = new Vector3(bodyWidth / 2f + 0.01f, 0f, 0f);
                tapeEndPoint = endObj.transform;

                // Add small visual at the end of the tape
                GameObject endVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                endVisual.name = "TapeEndTab";
                endVisual.transform.SetParent(endObj.transform, false);
                endVisual.transform.localPosition = Vector3.zero;
                endVisual.transform.localScale = new Vector3(0.008f, 0.015f, 0.003f);

                Material endMat = new Material(Shader.Find("Standard"));
                endMat.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Metal tab
                endMat.SetFloat("_Metallic", 0.8f);
                endVisual.GetComponent<MeshRenderer>().material = endMat;

                Collider endCol = endVisual.GetComponent<Collider>();
                if (endCol != null) Destroy(endCol);
            }
        }

        private void SetupTapeLine()
        {
            if (tapeLineRenderer == null)
            {
                tapeLineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            tapeLineRenderer.positionCount = 2;
            tapeLineRenderer.startWidth = tapeWidth;
            tapeLineRenderer.endWidth = tapeWidth;
            tapeLineRenderer.useWorldSpace = true;
            tapeLineRenderer.numCapVertices = 2;

            Material tapeMat = new Material(Shader.Find("Standard"));
            tapeMat.color = tapeColor;
            tapeLineRenderer.material = tapeMat;

            // Initially hidden
            tapeLineRenderer.enabled = false;
        }

        private void SetupLabel()
        {
            if (measurementLabel == null)
            {
                GameObject labelObj = new GameObject("MeasurementLabel");
                labelObj.transform.SetParent(transform, false);
                labelObj.transform.localPosition = new Vector3(0f, bodyHeight + 0.02f, 0f);

                measurementLabel = labelObj.AddComponent<FloatingLabel>();
                measurementLabel.SetText("");
                measurementLabel.SetFontSize(1.8f);
                measurementLabel.SetColor(Color.white);
                labelObj.SetActive(false);
            }
        }

        private void SetupGrabInteractable()
        {
            if (grabInteractable == null)
            {
                grabInteractable = GetComponent<GrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = gameObject.AddComponent<GrabInteractable>();
                }
            }

            grabInteractable.SetToolType(toolType);
            grabInteractable.SetGripOffset(new Vector3(0f, -0.01f, 0.01f));
        }

        private void Update()
        {
            if (isMeasuring && tapeStartPoint != null && tapeEndPoint != null)
            {
                UpdateTapeVisual();
            }
        }

        private void UpdateTapeVisual()
        {
            if (tapeLineRenderer == null) return;

            tapeLineRenderer.enabled = true;
            tapeLineRenderer.SetPosition(0, tapeStartPoint.position);
            tapeLineRenderer.SetPosition(1, tapeEndPoint.position);

            float distance = Vector3.Distance(tapeStartPoint.position, tapeEndPoint.position);
            lastMeasuredDistance = distance;

            // Update label
            UpdateMeasurementDisplay(distance);
        }

        private void UpdateMeasurementDisplay(float distanceMeters)
        {
            if (measurementLabel == null) return;

            measurementLabel.gameObject.SetActive(true);

            // Show in both metric and imperial
            float inches = distanceMeters * 39.3701f;
            int feet = Mathf.FloorToInt(inches / 12f);
            float remainingInches = inches % 12f;

            string display;
            if (feet > 0)
            {
                display = $"{feet}' {remainingInches:F1}\"\n({distanceMeters:F3}m)";
            }
            else
            {
                display = $"{inches:F1}\"\n({distanceMeters * 100f:F1}cm)";
            }

            measurementLabel.SetText(display);
        }

        // --- Public API ---

        /// <summary>
        /// Measure the distance between two world-space points.
        /// Returns the distance in meters and displays it on the label.
        /// </summary>
        public float MeasureDistance(Vector3 from, Vector3 to)
        {
            float distance = Vector3.Distance(from, to);

            // Clamp to reasonable range
            distance = Mathf.Clamp(distance, minMeasureDistance, maxMeasureDistance);

            // Position the tape endpoints
            if (tapeStartPoint != null)
                tapeStartPoint.position = from;
            if (tapeEndPoint != null)
                tapeEndPoint.position = to;

            isMeasuring = true;
            lastMeasuredDistance = distance;

            UpdateMeasurementDisplay(distance);
            UpdateTapeVisual();

            AudioManager.Instance.PlaySnapSound();

            Debug.Log($"[MeasureTool] Measured distance: {distance:F3}m ({distance * 39.3701f:F1} inches)");

            return distance;
        }

        /// <summary>
        /// Start measuring from the tape start point.
        /// Call UpdateMeasureTarget to update the endpoint as the player moves.
        /// </summary>
        public void StartMeasuring()
        {
            isMeasuring = true;
            tapeLineRenderer.enabled = true;
            measurementLabel.gameObject.SetActive(true);
            isExtended = true;

            Debug.Log("[MeasureTool] Started measuring");
        }

        /// <summary>
        /// Update the measurement target point (the extended end of the tape).
        /// </summary>
        public void UpdateMeasureTarget(Vector3 worldPosition)
        {
            if (!isMeasuring) return;

            if (tapeEndPoint != null)
            {
                tapeEndPoint.position = worldPosition;
            }
        }

        /// <summary>
        /// Stop measuring and retract the tape.
        /// </summary>
        public void StopMeasuring()
        {
            isMeasuring = false;
            isExtended = false;
            tapeLineRenderer.enabled = false;

            if (measurementLabel != null)
            {
                measurementLabel.gameObject.SetActive(false);
            }

            // Retract tape end back to start
            if (tapeEndPoint != null && tapeStartPoint != null)
            {
                tapeEndPoint.localPosition = new Vector3(bodyWidth / 2f + 0.01f, 0f, 0f);
            }

            Debug.Log("[MeasureTool] Stopped measuring");
        }

        /// <summary>
        /// Get the last measured distance in the specified unit.
        /// </summary>
        public float GetDistanceInUnit(string unit)
        {
            switch (unit)
            {
                case "inches":
                    return lastMeasuredDistance * 39.3701f;
                case "feet":
                    return lastMeasuredDistance * 3.28084f;
                case "cm":
                    return lastMeasuredDistance * 100f;
                case "mm":
                    return lastMeasuredDistance * 1000f;
                case "meters":
                default:
                    return lastMeasuredDistance;
            }
        }

        /// <summary>
        /// Check if the last measurement is within tolerance of an expected value.
        /// </summary>
        public bool IsWithinTolerance(float expectedValue, float tolerance, string unit = "inches")
        {
            float measured = GetDistanceInUnit(unit);
            float difference = Mathf.Abs(measured - expectedValue);
            bool withinTolerance = difference <= tolerance;

            Debug.Log($"[MeasureTool] Tolerance check: measured={measured:F2}{unit}, " +
                      $"expected={expectedValue:F2}{unit}, diff={difference:F2}, " +
                      $"tolerance={tolerance:F2}, result={withinTolerance}");

            return withinTolerance;
        }
    }
}
