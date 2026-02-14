using UnityEngine;
using System.Collections.Generic;
using TradeProof.Data;
using TradeProof.Interaction;
using TradeProof.Training;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents a full residential electrical panel assembly.
    /// Dimensions based on real residential panel: ~14.5" wide x 28" tall x 4" deep
    /// (0.368m x 0.711m x 0.102m)
    /// All sub-components are positioned relative to the panel transform.
    /// </summary>
    public class ElectricalPanel : MonoBehaviour
    {
        [Header("Panel Dimensions (meters)")]
        [SerializeField] private float panelWidth = 0.368f;   // 14.5 inches
        [SerializeField] private float panelHeight = 0.711f;  // 28 inches
        [SerializeField] private float panelDepth = 0.102f;   // 4 inches

        [Header("Components")]
        [SerializeField] private List<CircuitBreaker> breakers = new List<CircuitBreaker>();
        [SerializeField] private BusBar busBar;
        [SerializeField] private BusBar neutralBar;
        [SerializeField] private BusBar groundBar;
        [SerializeField] private Transform directory;
        [SerializeField] private List<Transform> knockouts = new List<Transform>();

        [Header("Snap Points")]
        [SerializeField] private List<SnapPoint> breakerSnapPoints = new List<SnapPoint>();
        [SerializeField] private List<SnapPoint> neutralBarSnapPoints = new List<SnapPoint>();
        [SerializeField] private List<SnapPoint> groundBarSnapPoints = new List<SnapPoint>();

        [Header("Violations")]
        private List<ViolationMarker> activeViolations = new List<ViolationMarker>();

        [Header("Panel State")]
        [SerializeField] private int maxBreakerSpaces = 24;
        [SerializeField] private bool hasDirectory = true;
        [SerializeField] private string panelLabel = "200A Main Service Panel";

        [Header("Visual")]
        [SerializeField] private MeshRenderer panelBodyRenderer;
        [SerializeField] private MeshRenderer panelDoorRenderer;
        [SerializeField] private Color panelColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private bool isDoorOpen = true;

        public float PanelWidth => panelWidth;
        public float PanelHeight => panelHeight;
        public float PanelDepth => panelDepth;
        public int MaxBreakerSpaces => maxBreakerSpaces;
        public int CurrentBreakerCount => breakers.Count;
        public bool HasDirectory => hasDirectory;
        public List<CircuitBreaker> Breakers => breakers;

        private void Awake()
        {
            BuildPanelStructure();
        }

        private void BuildPanelStructure()
        {
            // Create panel body if no renderer assigned
            if (panelBodyRenderer == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "PanelBody";
                body.transform.SetParent(transform, false);
                body.transform.localPosition = Vector3.zero;
                body.transform.localScale = new Vector3(panelWidth, panelHeight, panelDepth);

                panelBodyRenderer = body.GetComponent<MeshRenderer>();
                Material panelMat = new Material(Shader.Find("Standard"));
                panelMat.color = panelColor;
                panelBodyRenderer.material = panelMat;
            }

            // Create bus bars as children
            if (busBar == null)
            {
                busBar = CreateBusBar("BusBar_Hot", new Vector3(0f, 0f, panelDepth * -0.3f), BusBar.BusBarType.Hot);
            }
            if (neutralBar == null)
            {
                neutralBar = CreateBusBar("BusBar_Neutral",
                    new Vector3(panelWidth * 0.35f, 0f, panelDepth * -0.3f), BusBar.BusBarType.Neutral);
            }
            if (groundBar == null)
            {
                groundBar = CreateBusBar("BusBar_Ground",
                    new Vector3(panelWidth * -0.35f, 0f, panelDepth * -0.3f), BusBar.BusBarType.Ground);
            }

            // Create directory area
            if (directory == null)
            {
                GameObject dirObj = new GameObject("Directory");
                dirObj.transform.SetParent(transform, false);
                dirObj.transform.localPosition = new Vector3(0f, panelHeight * 0.45f, panelDepth * -0.5f);
                directory = dirObj.transform;
            }

            // Create knockout holes
            CreateKnockouts();

            // Create breaker snap points along the bus bar
            CreateBreakerSnapPoints();
        }

        private BusBar CreateBusBar(string name, Vector3 localPos, BusBar.BusBarType type)
        {
            GameObject barObj = new GameObject(name);
            barObj.transform.SetParent(transform, false);
            barObj.transform.localPosition = localPos;

            BusBar bar = barObj.AddComponent<BusBar>();
            bar.Initialize(type, 12); // 12 connection points per bar
            return bar;
        }

        private void CreateKnockouts()
        {
            knockouts.Clear();

            // Knockouts on the bottom of the panel
            float knockoutSpacing = 0.04f;
            int knockoutCount = 6;
            float startX = -knockoutSpacing * (knockoutCount - 1) / 2f;

            for (int i = 0; i < knockoutCount; i++)
            {
                GameObject ko = new GameObject($"Knockout_{i}");
                ko.transform.SetParent(transform, false);
                ko.transform.localPosition = new Vector3(
                    startX + i * knockoutSpacing,
                    -panelHeight * 0.48f,
                    0f
                );

                // Visual knockout circle
                GameObject koVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                koVisual.transform.SetParent(ko.transform, false);
                koVisual.transform.localPosition = Vector3.zero;
                koVisual.transform.localScale = new Vector3(0.025f, 0.005f, 0.025f);
                koVisual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                Material koMat = new Material(Shader.Find("Standard"));
                koMat.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                koVisual.GetComponent<MeshRenderer>().material = koMat;

                knockouts.Add(ko.transform);
            }
        }

        private void CreateBreakerSnapPoints()
        {
            breakerSnapPoints.Clear();

            // Left column and right column of breaker spaces
            float spacingY = panelHeight / (maxBreakerSpaces / 2 + 1);

            for (int i = 0; i < maxBreakerSpaces; i++)
            {
                bool leftSide = (i % 2 == 0);
                int row = i / 2;

                float xPos = leftSide ? -panelWidth * 0.15f : panelWidth * 0.15f;
                float yPos = panelHeight * 0.4f - (row * spacingY);

                GameObject spObj = new GameObject($"BreakerSlot_{i}");
                spObj.transform.SetParent(transform, false);
                spObj.transform.localPosition = new Vector3(xPos, yPos, panelDepth * -0.4f);

                SnapPoint sp = spObj.AddComponent<SnapPoint>();
                sp.SetAcceptedWireType("hot");
                sp.SetSnapPointId($"breaker-slot-{i}");

                breakerSnapPoints.Add(sp);
            }
        }

        // --- Violation Management ---

        public ViolationMarker AddViolation(ViolationDefinition violationDef)
        {
            GameObject markerObj = new GameObject($"Violation_{violationDef.id}");
            markerObj.transform.SetParent(transform, false); // Child of panel — local coordinates

            ViolationMarker marker = markerObj.AddComponent<ViolationMarker>();
            marker.Initialize(violationDef);

            activeViolations.Add(marker);
            return marker;
        }

        public List<ViolationMarker> GetViolations()
        {
            return new List<ViolationMarker>(activeViolations);
        }

        public bool IsViolationIdentified(string violationId)
        {
            foreach (var marker in activeViolations)
            {
                if (marker.ViolationId == violationId)
                    return marker.IsIdentified;
            }
            return false;
        }

        public int GetIdentifiedViolationCount()
        {
            int count = 0;
            foreach (var marker in activeViolations)
            {
                if (marker.IsIdentified) count++;
            }
            return count;
        }

        public void ClearViolations()
        {
            foreach (var marker in activeViolations)
            {
                if (marker != null)
                    Destroy(marker.gameObject);
            }
            activeViolations.Clear();
        }

        // --- Panel Operations ---

        public void SetDoorOpen(bool open)
        {
            isDoorOpen = open;
            if (panelDoorRenderer != null)
            {
                // Animate door rotation
                float targetAngle = open ? -120f : 0f;
                panelDoorRenderer.transform.localRotation = Quaternion.Euler(0f, targetAngle, 0f);
            }
        }

        public void SetDirectoryPresent(bool present)
        {
            hasDirectory = present;
            if (directory != null)
            {
                directory.gameObject.SetActive(present);
            }
        }

        public CircuitBreaker AddBreaker(int amperage, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxBreakerSpaces)
            {
                Debug.LogError($"[ElectricalPanel] Invalid slot index: {slotIndex}");
                return null;
            }

            if (breakers.Count >= maxBreakerSpaces)
            {
                Debug.LogWarning("[ElectricalPanel] Panel is full — NEC 408.54 violation");
            }

            GameObject breakerObj = new GameObject($"Breaker_{amperage}A_Slot{slotIndex}");
            breakerObj.transform.SetParent(transform, false);

            // Position at the breaker slot
            if (slotIndex < breakerSnapPoints.Count)
            {
                breakerObj.transform.localPosition = breakerSnapPoints[slotIndex].transform.localPosition;
            }

            CircuitBreaker breaker = breakerObj.AddComponent<CircuitBreaker>();
            breaker.Initialize(amperage, slotIndex);

            breakers.Add(breaker);
            return breaker;
        }

        public void RemoveBreaker(int slotIndex)
        {
            for (int i = breakers.Count - 1; i >= 0; i--)
            {
                if (breakers[i].SlotIndex == slotIndex)
                {
                    Destroy(breakers[i].gameObject);
                    breakers.RemoveAt(i);
                    break;
                }
            }
        }

        // --- Bus Bar Access ---

        public SnapPoint GetNextNeutralSnapPoint()
        {
            if (neutralBar != null)
                return neutralBar.GetNextAvailableSnapPoint();
            return null;
        }

        public SnapPoint GetNextGroundSnapPoint()
        {
            if (groundBar != null)
                return groundBar.GetNextAvailableSnapPoint();
            return null;
        }

        // --- Validation ---

        public bool IsOverfilled()
        {
            return breakers.Count > maxBreakerSpaces;
        }

        public bool ValidatePanel()
        {
            bool valid = true;

            if (!hasDirectory)
            {
                Debug.LogWarning("[ElectricalPanel] NEC 408.4 — Missing panel directory");
                valid = false;
            }

            if (IsOverfilled())
            {
                Debug.LogWarning("[ElectricalPanel] NEC 408.54 — Panel overfilled");
                valid = false;
            }

            foreach (var breaker in breakers)
            {
                if (breaker.IsDoubleTapped)
                {
                    Debug.LogWarning($"[ElectricalPanel] NEC 408.41 — Breaker at slot {breaker.SlotIndex} is double-tapped");
                    valid = false;
                }
            }

            return valid;
        }
    }
}
