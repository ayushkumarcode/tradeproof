using UnityEngine;
using System.Collections.Generic;
using TradeProof.Interaction;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Represents a bus bar inside the electrical panel.
    /// Types: Hot (main bus), Neutral, Ground.
    /// Each bus bar has multiple connection snap points positioned as LOCAL offsets.
    /// </summary>
    public class BusBar : MonoBehaviour
    {
        [Header("Bus Bar Properties")]
        [SerializeField] private BusBarType barType = BusBarType.Hot;
        [SerializeField] private int maxConnections = 12;

        [Header("Dimensions (meters)")]
        [SerializeField] private float barLength = 0.5f;
        [SerializeField] private float barWidth = 0.025f;
        [SerializeField] private float barThickness = 0.003f;

        [Header("Snap Points")]
        [SerializeField] private List<SnapPoint> connectionPoints = new List<SnapPoint>();

        [Header("Visual")]
        [SerializeField] private MeshRenderer barRenderer;

        public enum BusBarType
        {
            Hot,
            Neutral,
            Ground
        }

        public BusBarType Type => barType;
        public int MaxConnections => maxConnections;
        public int OccupiedConnections
        {
            get
            {
                int count = 0;
                foreach (var sp in connectionPoints)
                {
                    if (sp.IsOccupied) count++;
                }
                return count;
            }
        }

        public void Initialize(BusBarType type, int connections)
        {
            barType = type;
            maxConnections = connections;
            BuildVisual();
            CreateConnectionPoints();
        }

        private void BuildVisual()
        {
            if (barRenderer == null)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = $"BusBar_{barType}";
                bar.transform.SetParent(transform, false);
                bar.transform.localPosition = Vector3.zero;
                bar.transform.localScale = new Vector3(barWidth, barLength, barThickness);

                barRenderer = bar.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Metallic", 0.8f);
                mat.SetFloat("_Glossiness", 0.6f);

                switch (barType)
                {
                    case BusBarType.Hot:
                        mat.color = new Color(0.7f, 0.5f, 0.2f, 1f); // Copper/brass
                        break;
                    case BusBarType.Neutral:
                        mat.color = new Color(0.8f, 0.8f, 0.85f, 1f); // Silver/aluminum
                        break;
                    case BusBarType.Ground:
                        mat.color = new Color(0.6f, 0.8f, 0.6f, 1f); // Green-tinted
                        break;
                }

                barRenderer.material = mat;

                // Remove box collider from visual, keep parent trigger
                Collider col = bar.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
        }

        private void CreateConnectionPoints()
        {
            connectionPoints.Clear();

            float spacing = barLength / (maxConnections + 1);
            string wireType = GetAcceptedWireType();

            for (int i = 0; i < maxConnections; i++)
            {
                GameObject spObj = new GameObject($"Connection_{i}");
                spObj.transform.SetParent(transform, false);

                // Position along the bar length — LOCAL coordinates
                float yPos = (barLength / 2f) - ((i + 1) * spacing);
                spObj.transform.localPosition = new Vector3(0f, yPos, -barThickness);

                SnapPoint sp = spObj.AddComponent<SnapPoint>();
                sp.SetAcceptedWireType(wireType);
                sp.SetAmpRating(0); // Bus bar accepts any amperage wire
                sp.SetSnapPointId($"{barType.ToString().ToLower()}-bar-{i}");

                // Add screw visual
                CreateScrewVisual(spObj.transform);

                connectionPoints.Add(sp);
            }
        }

        private void CreateScrewVisual(Transform parent)
        {
            GameObject screw = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            screw.name = "Screw";
            screw.transform.SetParent(parent, false);
            screw.transform.localPosition = Vector3.zero;
            screw.transform.localScale = new Vector3(0.006f, 0.002f, 0.006f);
            screw.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Material screwMat = new Material(Shader.Find("Standard"));
            screwMat.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            screwMat.SetFloat("_Metallic", 0.9f);
            screw.GetComponent<MeshRenderer>().material = screwMat;

            Collider col = screw.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private string GetAcceptedWireType()
        {
            switch (barType)
            {
                case BusBarType.Hot:
                    return "hot";
                case BusBarType.Neutral:
                    return "neutral";
                case BusBarType.Ground:
                    return "ground";
                default:
                    return "any";
            }
        }

        // --- Public API ---

        public SnapPoint GetNextAvailableSnapPoint()
        {
            foreach (var sp in connectionPoints)
            {
                if (!sp.IsOccupied)
                {
                    return sp;
                }
            }
            Debug.LogWarning($"[BusBar] {barType} bar is full — no available connections");
            return null;
        }

        public SnapPoint GetSnapPoint(int index)
        {
            if (index >= 0 && index < connectionPoints.Count)
            {
                return connectionPoints[index];
            }
            return null;
        }

        public List<SnapPoint> GetAllSnapPoints()
        {
            return new List<SnapPoint>(connectionPoints);
        }

        public bool HasAvailableConnection()
        {
            return OccupiedConnections < maxConnections;
        }

        public void DisconnectAll()
        {
            foreach (var sp in connectionPoints)
            {
                if (sp.IsOccupied)
                {
                    sp.DetachWire();
                }
            }
        }

        public bool ValidateConnections()
        {
            foreach (var sp in connectionPoints)
            {
                if (sp.IsOccupied && !sp.ValidateConnection())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
