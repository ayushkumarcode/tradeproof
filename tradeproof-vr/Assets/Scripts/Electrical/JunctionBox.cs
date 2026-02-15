using UnityEngine;
using System.Collections.Generic;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Standard 4x4x2.125 inch wire splicing junction box.
    /// Dimensions: 0.102m x 0.102m x 0.054m.
    /// 8 SnapPoints for wire connections (4 per side).
    /// 4 wire nut attachment positions inside the box.
    /// Validates NEC 314.16 box fill calculations.
    /// Box volume: 21 cubic inches for a 4x4 box.
    /// </summary>
    public class JunctionBox : MonoBehaviour
    {
        [Header("Dimensions (meters)")]
        [SerializeField] private float boxWidth = 0.102f;    // 4 inches
        [SerializeField] private float boxHeight = 0.102f;   // 4 inches
        [SerializeField] private float boxDepth = 0.054f;    // 2.125 inches

        [Header("Box Fill (NEC 314.16)")]
        [SerializeField] private float boxVolumeInches = 21f; // 21 cubic inches for 4x4 box

        [Header("Wire Connections")]
        [SerializeField] private List<SnapPoint> wireSnapPoints = new List<SnapPoint>();

        [Header("Wire Nut Positions")]
        [SerializeField] private List<Transform> wireNutPositions = new List<Transform>();

        [Header("Knockout Visuals")]
        [SerializeField] private List<Transform> knockoutVisuals = new List<Transform>();

        [Header("Visual")]
        [SerializeField] private MeshRenderer boxRenderer;
        [SerializeField] private Color boxColor = new Color(0.45f, 0.50f, 0.58f, 1f); // Blue-gray metal

        public float BoxWidth => boxWidth;
        public float BoxHeight => boxHeight;
        public float BoxDepth => boxDepth;
        public float BoxVolumeInches => boxVolumeInches;
        public List<SnapPoint> WireSnapPoints => wireSnapPoints;
        public List<Transform> WireNutPositions => wireNutPositions;

        private void Awake()
        {
            BuildBoxVisual();
            CreateWireSnapPoints();
            CreateKnockouts();
            CreateWireNutPositions();
        }

        // ---------------------------------------------------------------
        // Visual Construction
        // ---------------------------------------------------------------

        private void BuildBoxVisual()
        {
            if (boxRenderer == null)
            {
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = "JunctionBoxBody";
                box.transform.SetParent(transform, false);
                box.transform.localPosition = Vector3.zero;
                box.transform.localScale = new Vector3(boxWidth, boxHeight, boxDepth);

                boxRenderer = box.GetComponent<MeshRenderer>();
                Material boxMat = new Material(Shader.Find("Standard"));
                boxMat.color = boxColor;
                boxMat.SetFloat("_Metallic", 0.7f);
                boxMat.SetFloat("_Glossiness", 0.3f);
                boxRenderer.material = boxMat;
            }

            // --- Interior floor (slightly darker) ---
            GameObject interior = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interior.name = "BoxInterior";
            interior.transform.SetParent(transform, false);
            interior.transform.localPosition = new Vector3(0f, 0f, boxDepth * 0.45f);
            interior.transform.localScale = new Vector3(boxWidth * 0.92f, boxHeight * 0.92f, 0.002f);

            Material intMat = new Material(Shader.Find("Standard"));
            intMat.color = new Color(0.35f, 0.38f, 0.42f, 1f);
            intMat.SetFloat("_Metallic", 0.5f);
            interior.GetComponent<MeshRenderer>().material = intMat;
            Destroy(interior.GetComponent<Collider>());

            // --- Box rim (front opening edge) ---
            CreateBoxRim();
        }

        private void CreateBoxRim()
        {
            float rimThickness = 0.003f;
            Color rimColor = new Color(0.50f, 0.55f, 0.60f, 1f);

            // Top rim
            CreateRimSegment("RimTop",
                new Vector3(0f, boxHeight / 2f, -boxDepth / 2f),
                new Vector3(boxWidth, rimThickness, rimThickness),
                rimColor);

            // Bottom rim
            CreateRimSegment("RimBottom",
                new Vector3(0f, -boxHeight / 2f, -boxDepth / 2f),
                new Vector3(boxWidth, rimThickness, rimThickness),
                rimColor);

            // Left rim
            CreateRimSegment("RimLeft",
                new Vector3(-boxWidth / 2f, 0f, -boxDepth / 2f),
                new Vector3(rimThickness, boxHeight, rimThickness),
                rimColor);

            // Right rim
            CreateRimSegment("RimRight",
                new Vector3(boxWidth / 2f, 0f, -boxDepth / 2f),
                new Vector3(rimThickness, boxHeight, rimThickness),
                rimColor);
        }

        private void CreateRimSegment(string name, Vector3 localPos, Vector3 scale, Color color)
        {
            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rim.name = name;
            rim.transform.SetParent(transform, false);
            rim.transform.localPosition = localPos;
            rim.transform.localScale = scale;

            Material rimMat = new Material(Shader.Find("Standard"));
            rimMat.color = color;
            rimMat.SetFloat("_Metallic", 0.6f);
            rim.GetComponent<MeshRenderer>().material = rimMat;
            Destroy(rim.GetComponent<Collider>());
        }

        // ---------------------------------------------------------------
        // Wire Snap Points (4 per side)
        // ---------------------------------------------------------------

        private void CreateWireSnapPoints()
        {
            wireSnapPoints.Clear();

            float spacing = boxHeight / 5f;

            // Left side — 4 connection points
            for (int i = 0; i < 4; i++)
            {
                float yPos = (boxHeight / 2f) - ((i + 1) * spacing);
                Vector3 pos = new Vector3(-boxWidth / 2f - 0.005f, yPos, 0f);
                SnapPoint sp = CreateWireSnapPoint($"WirePoint_Left_{i}", pos, "any", $"jbox-left-{i}");
                wireSnapPoints.Add(sp);
            }

            // Right side — 4 connection points
            for (int i = 0; i < 4; i++)
            {
                float yPos = (boxHeight / 2f) - ((i + 1) * spacing);
                Vector3 pos = new Vector3(boxWidth / 2f + 0.005f, yPos, 0f);
                SnapPoint sp = CreateWireSnapPoint($"WirePoint_Right_{i}", pos, "any", $"jbox-right-{i}");
                wireSnapPoints.Add(sp);
            }
        }

        private SnapPoint CreateWireSnapPoint(string name, Vector3 localPos, string wireType, string id)
        {
            GameObject spObj = new GameObject(name);
            spObj.transform.SetParent(transform, false);
            spObj.transform.localPosition = localPos;

            SnapPoint sp = spObj.AddComponent<SnapPoint>();
            sp.SetAcceptedWireType(wireType);
            sp.SetAmpRating(0);
            sp.SetSnapPointId(id);

            return sp;
        }

        // ---------------------------------------------------------------
        // Knockouts
        // ---------------------------------------------------------------

        private void CreateKnockouts()
        {
            knockoutVisuals.Clear();

            float knockoutDiameter = 0.022f; // Standard 1/2 inch knockout
            Color knockoutColor = new Color(0.35f, 0.38f, 0.42f, 1f);

            float spacing = boxHeight / 5f;

            // Left side knockouts (aligned with wire snap points)
            for (int i = 0; i < 4; i++)
            {
                float yPos = (boxHeight / 2f) - ((i + 1) * spacing);
                Vector3 pos = new Vector3(-boxWidth / 2f, yPos, 0f);
                Transform ko = CreateKnockoutVisual($"Knockout_Left_{i}", pos,
                    Quaternion.Euler(0f, 0f, 90f), knockoutDiameter, knockoutColor);
                knockoutVisuals.Add(ko);
            }

            // Right side knockouts
            for (int i = 0; i < 4; i++)
            {
                float yPos = (boxHeight / 2f) - ((i + 1) * spacing);
                Vector3 pos = new Vector3(boxWidth / 2f, yPos, 0f);
                Transform ko = CreateKnockoutVisual($"Knockout_Right_{i}", pos,
                    Quaternion.Euler(0f, 0f, 90f), knockoutDiameter, knockoutColor);
                knockoutVisuals.Add(ko);
            }

            // Bottom knockouts (2 centered)
            for (int i = 0; i < 2; i++)
            {
                float xPos = (i == 0) ? -0.02f : 0.02f;
                Vector3 pos = new Vector3(xPos, -boxHeight / 2f, 0f);
                Transform ko = CreateKnockoutVisual($"Knockout_Bottom_{i}", pos,
                    Quaternion.Euler(0f, 0f, 0f), knockoutDiameter, knockoutColor);
                knockoutVisuals.Add(ko);
            }

            // Top knockouts (2 centered)
            for (int i = 0; i < 2; i++)
            {
                float xPos = (i == 0) ? -0.02f : 0.02f;
                Vector3 pos = new Vector3(xPos, boxHeight / 2f, 0f);
                Transform ko = CreateKnockoutVisual($"Knockout_Top_{i}", pos,
                    Quaternion.Euler(0f, 0f, 0f), knockoutDiameter, knockoutColor);
                knockoutVisuals.Add(ko);
            }
        }

        private Transform CreateKnockoutVisual(string name, Vector3 localPos, Quaternion rotation,
            float diameter, Color color)
        {
            GameObject ko = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ko.name = name;
            ko.transform.SetParent(transform, false);
            ko.transform.localPosition = localPos;
            ko.transform.localRotation = rotation;
            ko.transform.localScale = new Vector3(diameter, 0.003f, diameter);

            Material koMat = new Material(Shader.Find("Standard"));
            koMat.color = color;
            koMat.SetFloat("_Metallic", 0.5f);
            ko.GetComponent<MeshRenderer>().material = koMat;
            Destroy(ko.GetComponent<Collider>());

            // Knockout ring outline (slightly larger, darker)
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = $"{name}_Ring";
            ring.transform.SetParent(ko.transform, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(1.15f, 0.5f, 1.15f);

            Material ringMat = new Material(Shader.Find("Standard"));
            ringMat.color = color * 0.7f;
            ringMat.SetFloat("_Metallic", 0.6f);
            ring.GetComponent<MeshRenderer>().material = ringMat;
            Destroy(ring.GetComponent<Collider>());

            return ko.transform;
        }

        // ---------------------------------------------------------------
        // Wire Nut Positions
        // ---------------------------------------------------------------

        private void CreateWireNutPositions()
        {
            wireNutPositions.Clear();

            // 4 wire nut positions inside the box, evenly spaced in a 2x2 grid
            float offsetX = boxWidth * 0.20f;
            float offsetY = boxHeight * 0.20f;
            float zPos = boxDepth * 0.2f; // Inside the box, slightly above floor

            Vector3[] positions = new Vector3[]
            {
                new Vector3(-offsetX, offsetY, zPos),   // Top-left
                new Vector3(offsetX, offsetY, zPos),    // Top-right
                new Vector3(-offsetX, -offsetY, zPos),  // Bottom-left
                new Vector3(offsetX, -offsetY, zPos)    // Bottom-right
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject nutPos = new GameObject($"WireNutPosition_{i}");
                nutPos.transform.SetParent(transform, false);
                nutPos.transform.localPosition = positions[i];

                // Small visual indicator (subtle dot)
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indicator.name = "PositionIndicator";
                indicator.transform.SetParent(nutPos.transform, false);
                indicator.transform.localPosition = Vector3.zero;
                indicator.transform.localScale = Vector3.one * 0.005f;

                Material indMat = new Material(Shader.Find("Standard"));
                indMat.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                indMat.SetFloat("_Mode", 3); // Transparent
                indMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                indMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                indMat.SetInt("_ZWrite", 0);
                indMat.EnableKeyword("_ALPHABLEND_ON");
                indMat.renderQueue = 3000;
                indicator.GetComponent<MeshRenderer>().material = indMat;
                Destroy(indicator.GetComponent<Collider>());

                wireNutPositions.Add(nutPos.transform);
            }
        }

        // ---------------------------------------------------------------
        // Box Fill Validation — NEC 314.16
        // ---------------------------------------------------------------

        /// <summary>
        /// Validates box fill per NEC 314.16.
        /// Each conductor takes a specific volume allowance based on its AWG gauge.
        ///   14 AWG: 2.0 cubic inches per conductor
        ///   12 AWG: 2.25 cubic inches per conductor
        /// Box volume for a 4x4 box: 21 cubic inches.
        /// Returns true if the total conductor fill fits within box volume.
        /// </summary>
        public bool ValidateBoxFill(int conductorCount, int gaugeAWG)
        {
            float volumePerConductor = GetVolumePerConductor(gaugeAWG);
            float totalFill = conductorCount * volumePerConductor;

            if (totalFill > boxVolumeInches)
            {
                Debug.LogWarning($"[JunctionBox] NEC 314.16 VIOLATION — Box fill {totalFill:F2} cu.in. exceeds " +
                                 $"box volume {boxVolumeInches:F1} cu.in. ({conductorCount}x {gaugeAWG} AWG)");
                return false;
            }

            Debug.Log($"[JunctionBox] Box fill OK: {totalFill:F2}/{boxVolumeInches:F1} cu.in. " +
                       $"({conductorCount}x {gaugeAWG} AWG)");
            return true;
        }

        /// <summary>
        /// Returns the volume allowance per conductor in cubic inches, per NEC 314.16(B).
        /// </summary>
        public static float GetVolumePerConductor(int gaugeAWG)
        {
            switch (gaugeAWG)
            {
                case 18: return 1.50f;
                case 16: return 1.75f;
                case 14: return 2.00f;
                case 12: return 2.25f;
                case 10: return 2.50f;
                case 8:  return 3.00f;
                case 6:  return 5.00f;
                default:
                    Debug.LogWarning($"[JunctionBox] Unknown gauge {gaugeAWG} AWG — defaulting to 2.25 cu.in.");
                    return 2.25f;
            }
        }

        /// <summary>
        /// Calculates the maximum number of conductors of a given gauge that fit in this box.
        /// </summary>
        public int GetMaxConductors(int gaugeAWG)
        {
            float volumePerConductor = GetVolumePerConductor(gaugeAWG);
            return Mathf.FloorToInt(boxVolumeInches / volumePerConductor);
        }

        /// <summary>
        /// Returns the current fill percentage (0-100+).
        /// </summary>
        public float GetFillPercentage(int conductorCount, int gaugeAWG)
        {
            float volumePerConductor = GetVolumePerConductor(gaugeAWG);
            float totalFill = conductorCount * volumePerConductor;
            return (totalFill / boxVolumeInches) * 100f;
        }

        // ---------------------------------------------------------------
        // Connection Queries
        // ---------------------------------------------------------------

        /// <summary>
        /// Returns the number of currently occupied wire snap points.
        /// </summary>
        public int GetOccupiedConnectionCount()
        {
            int count = 0;
            foreach (var sp in wireSnapPoints)
            {
                if (sp != null && sp.IsOccupied)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns the next available (unoccupied) wire snap point, or null if full.
        /// </summary>
        public SnapPoint GetNextAvailableSnapPoint()
        {
            foreach (var sp in wireSnapPoints)
            {
                if (sp != null && !sp.IsOccupied)
                {
                    return sp;
                }
            }
            Debug.LogWarning("[JunctionBox] All wire connection points occupied");
            return null;
        }

        /// <summary>
        /// Disconnects all wires from the junction box.
        /// </summary>
        public void DisconnectAll()
        {
            foreach (var sp in wireSnapPoints)
            {
                if (sp != null && sp.IsOccupied)
                {
                    sp.DetachWire();
                }
            }
        }

        public string GetNECReference()
        {
            return $"NEC 314.16: Junction box fill — {boxVolumeInches} cu.in. capacity";
        }
    }
}
