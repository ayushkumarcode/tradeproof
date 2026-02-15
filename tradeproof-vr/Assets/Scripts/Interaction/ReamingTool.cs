using UnityEngine;
using System.Collections.Generic;
using TradeProof.Core;

namespace TradeProof.Interaction
{
    /// <summary>
    /// Deburring/reaming tool for conduit ends.
    /// T-shaped handle with a small cone blade tip.
    /// When applied to a conduit, sets its IsReamed property to true.
    ///
    /// Visual: T-shape handle 0.08m wide, blade tip (small cone).
    /// toolType = "reaming-tool"
    /// </summary>
    public class ReamingTool : MonoBehaviour
    {
        [Header("Tool Properties")]
        [SerializeField] private string toolType = "reaming-tool";

        [Header("Visual — Handle")]
        [SerializeField] private float handleWidth = 0.08f;
        [SerializeField] private float handleHeight = 0.02f;
        [SerializeField] private float handleDepth = 0.02f;
        [SerializeField] private float shaftLength = 0.06f;
        [SerializeField] private float shaftDiameter = 0.012f;
        [SerializeField] private Color handleColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Black rubber grip
        [SerializeField] private Color metalColor = new Color(0.75f, 0.75f, 0.78f, 1f); // Steel

        [Header("Visual — Blade")]
        [SerializeField] private float bladeLength = 0.025f;
        [SerializeField] private float bladeBaseDiameter = 0.015f;
        [SerializeField] private Transform bladeTip;

        [Header("Renderers")]
        [SerializeField] private MeshRenderer handleRenderer;
        [SerializeField] private MeshRenderer shaftRenderer;
        [SerializeField] private MeshRenderer bladeRenderer;

        [Header("Interaction")]
        [SerializeField] private GrabInteractable grabInteractable;
        [SerializeField] private float reamingDistance = 0.03f; // How close tip must be to conduit end

        [Header("State")]
        [SerializeField] private bool isReaming;
        [SerializeField] private float reamingProgress;
        [SerializeField] private float reamingDuration = 1.5f; // seconds to complete reaming

        public string ToolType => toolType;
        public bool IsReaming => isReaming;
        public float ReamingProgress => reamingProgress;

        private void Awake()
        {
            BuildVisual();
            SetupGrabInteractable();
        }

        private void BuildVisual()
        {
            // Create T-handle (horizontal bar)
            if (handleRenderer == null)
            {
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                handle.name = "Handle";
                handle.transform.SetParent(transform, false);
                handle.transform.localPosition = new Vector3(0f, 0f, 0f);
                handle.transform.localScale = new Vector3(handleWidth, handleHeight, handleDepth);

                handleRenderer = handle.GetComponent<MeshRenderer>();
                Material handleMat = new Material(Shader.Find("Standard"));
                handleMat.color = handleColor;
                handleRenderer.material = handleMat;

                Collider handleCol = handle.GetComponent<Collider>();
                if (handleCol != null) Destroy(handleCol);
            }

            // Create shaft (vertical bar going down from center of handle)
            if (shaftRenderer == null)
            {
                GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shaft.name = "Shaft";
                shaft.transform.SetParent(transform, false);
                shaft.transform.localPosition = new Vector3(0f, -(handleHeight / 2f + shaftLength / 2f), 0f);
                shaft.transform.localScale = new Vector3(shaftDiameter, shaftLength / 2f, shaftDiameter);

                shaftRenderer = shaft.GetComponent<MeshRenderer>();
                Material shaftMat = new Material(Shader.Find("Standard"));
                shaftMat.color = metalColor;
                shaftMat.SetFloat("_Metallic", 0.8f);
                shaftMat.SetFloat("_Glossiness", 0.6f);
                shaftRenderer.material = shaftMat;

                Collider shaftCol = shaft.GetComponent<Collider>();
                if (shaftCol != null) Destroy(shaftCol);
            }

            // Create cone blade tip
            if (bladeRenderer == null)
            {
                // Use a cylinder scaled to approximate a cone
                // (Unity doesn't have a built-in cone primitive)
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                blade.name = "BladeTip";
                float bladeY = -(handleHeight / 2f + shaftLength + bladeLength / 2f);
                blade.transform.SetParent(transform, false);
                blade.transform.localPosition = new Vector3(0f, bladeY, 0f);
                blade.transform.localScale = new Vector3(bladeBaseDiameter, bladeLength / 2f, bladeBaseDiameter);

                bladeRenderer = blade.GetComponent<MeshRenderer>();
                Material bladeMat = new Material(Shader.Find("Standard"));
                bladeMat.color = new Color(0.65f, 0.65f, 0.68f, 1f); // Hardened steel, slightly darker
                bladeMat.SetFloat("_Metallic", 0.9f);
                bladeMat.SetFloat("_Glossiness", 0.7f);
                bladeRenderer.material = bladeMat;

                Collider bladeCol = blade.GetComponent<Collider>();
                if (bladeCol != null) Destroy(bladeCol);

                // Create blade tip transform for distance checking
                GameObject tipObj = new GameObject("BladeTipPoint");
                tipObj.transform.SetParent(blade.transform, false);
                tipObj.transform.localPosition = new Vector3(0f, -0.5f, 0f); // Bottom of the cone
                bladeTip = tipObj.transform;
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
            // Grip at the handle center
            grabInteractable.SetGripOffset(new Vector3(0f, 0f, 0f));
        }

        private void Update()
        {
            if (isReaming)
            {
                reamingProgress += Time.deltaTime / reamingDuration;
                if (reamingProgress >= 1f)
                {
                    reamingProgress = 1f;
                    isReaming = false;
                }
            }
        }

        // --- Public API ---

        /// <summary>
        /// Apply the reaming tool to a conduit end.
        /// Sets the conduit's IsReamed property to true and plays a sound.
        /// </summary>
        public bool ApplyToConduit(Conduit conduit)
        {
            if (conduit == null)
            {
                Debug.LogWarning("[ReamingTool] Cannot apply to null conduit");
                return false;
            }

            if (conduit.IsReamed)
            {
                Debug.Log("[ReamingTool] Conduit is already reamed");
                return false;
            }

            // Check distance from blade tip to conduit end
            if (bladeTip != null)
            {
                float distance = Vector3.Distance(bladeTip.position, conduit.GetCutEnd());
                if (distance > reamingDistance)
                {
                    Debug.Log($"[ReamingTool] Too far from conduit end (distance: {distance:F3}m, max: {reamingDistance}m)");
                    return false;
                }
            }

            // Start reaming
            isReaming = true;
            reamingProgress = 0f;

            conduit.IsReamed = true;

            // Play reaming sound
            AudioManager.Instance.PlayCorrectSound();

            Debug.Log($"[ReamingTool] Applied to conduit '{conduit.name}' - reaming complete (NEC 358.28)");
            return true;
        }

        /// <summary>
        /// Get the position of the blade tip for proximity checks.
        /// </summary>
        public Vector3 GetBladeTipPosition()
        {
            return bladeTip != null ? bladeTip.position : transform.position;
        }

        /// <summary>
        /// Check if the tool tip is close enough to a conduit end.
        /// </summary>
        public bool IsNearConduitEnd(Conduit conduit)
        {
            if (conduit == null || bladeTip == null) return false;
            float distance = Vector3.Distance(bladeTip.position, conduit.GetCutEnd());
            return distance <= reamingDistance;
        }
    }

    /// <summary>
    /// Represents a piece of EMT conduit for bending exercises.
    /// Tracks bend angles, measurements, and reaming state.
    /// </summary>
    public class Conduit : MonoBehaviour
    {
        [Header("Conduit Properties")]
        [SerializeField] private float length = 1.0f;        // meters (approx 3 feet)
        [SerializeField] private float diameter = 0.019f;     // 3/4" EMT
        [SerializeField] private string conduitType = "EMT";

        [Header("State")]
        [SerializeField] private bool isReamed;
        [SerializeField] private bool isCut;
        [SerializeField] private float cutLength;
        [SerializeField] private List<ConduitBend> bends = new List<ConduitBend>();

        [Header("Visual")]
        [SerializeField] private LineRenderer conduitRenderer;
        [SerializeField] private Color conduitColor = new Color(0.75f, 0.75f, 0.78f, 1f); // Steel
        [SerializeField] private int visualSegments = 30;

        [Header("Endpoints")]
        [SerializeField] private Transform startEnd;
        [SerializeField] private Transform cutEnd;

        public float Length => length;
        public float Diameter => diameter;
        public string ConduitType => conduitType;
        public bool IsReamed { get => isReamed; set => isReamed = value; }
        public bool IsCut => isCut;
        public float CutLength => cutLength;
        public int BendCount => bends.Count;

        [System.Serializable]
        public class ConduitBend
        {
            public float angle;          // degrees
            public float distanceFromEnd; // where the bend is measured from the end
            public string bendType;      // "90-degree", "offset", "saddle", "stub-up"

            public ConduitBend(float angle, float distanceFromEnd, string bendType)
            {
                this.angle = angle;
                this.distanceFromEnd = distanceFromEnd;
                this.bendType = bendType;
            }
        }

        private void Awake()
        {
            BuildVisual();
        }

        private void BuildVisual()
        {
            // Create conduit line
            if (conduitRenderer == null)
            {
                conduitRenderer = gameObject.AddComponent<LineRenderer>();
            }

            conduitRenderer.positionCount = visualSegments;
            conduitRenderer.startWidth = diameter;
            conduitRenderer.endWidth = diameter;
            conduitRenderer.useWorldSpace = true;
            conduitRenderer.numCapVertices = 4;

            Material conduitMat = new Material(Shader.Find("Standard"));
            conduitMat.color = conduitColor;
            conduitMat.SetFloat("_Metallic", 0.7f);
            conduitMat.SetFloat("_Glossiness", 0.5f);
            conduitRenderer.material = conduitMat;

            // Create endpoints
            if (startEnd == null)
            {
                GameObject startObj = new GameObject("ConduitStart");
                startObj.transform.SetParent(transform, false);
                startObj.transform.localPosition = Vector3.zero;
                startEnd = startObj.transform;
            }

            if (cutEnd == null)
            {
                GameObject endObj = new GameObject("ConduitEnd");
                endObj.transform.SetParent(transform, false);
                endObj.transform.localPosition = new Vector3(length, 0f, 0f);
                cutEnd = endObj.transform;
            }

            UpdateVisual();
        }

        private void Update()
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (conduitRenderer == null || startEnd == null || cutEnd == null) return;

            Vector3 start = startEnd.position;
            Vector3 end = cutEnd.position;

            for (int i = 0; i < visualSegments; i++)
            {
                float t = (float)i / (visualSegments - 1);
                Vector3 point = Vector3.Lerp(start, end, t);

                // Apply bends
                foreach (var bend in bends)
                {
                    float bendT = bend.distanceFromEnd / length;
                    if (Mathf.Abs(t - bendT) < 0.1f)
                    {
                        float bendInfluence = 1f - Mathf.Abs(t - bendT) / 0.1f;
                        float bendRadians = bend.angle * Mathf.Deg2Rad;
                        point.y += Mathf.Sin(bendRadians * bendInfluence) * 0.05f;
                    }
                }

                conduitRenderer.SetPosition(i, point);
            }
        }

        // --- Public API ---

        /// <summary>
        /// Get the position of the cut end (for reaming tool proximity checks).
        /// </summary>
        public Vector3 GetCutEnd()
        {
            return cutEnd != null ? cutEnd.position : transform.position + Vector3.right * length;
        }

        /// <summary>
        /// Get the position of the start end.
        /// </summary>
        public Vector3 GetStartEnd()
        {
            return startEnd != null ? startEnd.position : transform.position;
        }

        /// <summary>
        /// Apply a bend at a specific distance from the end.
        /// </summary>
        public ConduitBend ApplyBend(float angle, float distanceFromEnd, string bendType)
        {
            // NEC 358.24 — no more than 360 degrees total between pull points
            float totalBendAngle = 0f;
            foreach (var existing in bends)
            {
                totalBendAngle += Mathf.Abs(existing.angle);
            }

            if (totalBendAngle + Mathf.Abs(angle) > 360f)
            {
                Debug.LogWarning($"[Conduit] NEC 358.24 violation — total bends would exceed 360 degrees " +
                                 $"(current: {totalBendAngle}, adding: {Mathf.Abs(angle)})");
            }

            ConduitBend bend = new ConduitBend(angle, distanceFromEnd, bendType);
            bends.Add(bend);

            AudioManager.Instance.PlaySnapSound();
            Debug.Log($"[Conduit] Bend applied: {angle} degrees at {distanceFromEnd:F3}m ({bendType})");

            return bend;
        }

        /// <summary>
        /// Get the total bend angle of all bends.
        /// </summary>
        public float GetTotalBendAngle()
        {
            float total = 0f;
            foreach (var bend in bends)
            {
                total += Mathf.Abs(bend.angle);
            }
            return total;
        }

        /// <summary>
        /// Check if a bend angle is within tolerance of the expected value.
        /// </summary>
        public bool CheckBendAccuracy(int bendIndex, float expectedAngle, float tolerance)
        {
            if (bendIndex < 0 || bendIndex >= bends.Count) return false;
            float difference = Mathf.Abs(bends[bendIndex].angle - expectedAngle);
            return difference <= tolerance;
        }

        /// <summary>
        /// Cut the conduit to a specific length.
        /// </summary>
        public void CutToLength(float newLength)
        {
            if (newLength <= 0f || newLength > length)
            {
                Debug.LogWarning($"[Conduit] Invalid cut length: {newLength} (original: {length})");
                return;
            }

            cutLength = newLength;
            isCut = true;
            isReamed = false; // Cut ends need reaming per NEC 358.28

            if (cutEnd != null)
            {
                cutEnd.localPosition = new Vector3(cutLength, 0f, 0f);
            }

            Debug.Log($"[Conduit] Cut to {cutLength:F3}m — needs reaming (NEC 358.28)");
        }

        /// <summary>
        /// Get all bends on this conduit.
        /// </summary>
        public List<ConduitBend> GetBends()
        {
            return new List<ConduitBend>(bends);
        }

        /// <summary>
        /// Get a specific bend by index.
        /// </summary>
        public ConduitBend GetBend(int index)
        {
            if (index >= 0 && index < bends.Count)
                return bends[index];
            return null;
        }

        /// <summary>
        /// Reset the conduit to its original state.
        /// </summary>
        public void ResetConduit()
        {
            bends.Clear();
            isReamed = false;
            isCut = false;
            cutLength = 0f;

            if (cutEnd != null)
            {
                cutEnd.localPosition = new Vector3(length, 0f, 0f);
            }
        }
    }
}
