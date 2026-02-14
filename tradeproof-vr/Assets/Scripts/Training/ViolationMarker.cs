using UnityEngine;
using TradeProof.Data;
using TradeProof.UI;

namespace TradeProof.Training
{
    /// <summary>
    /// Marks a violation location on an electrical panel.
    /// MUST be a child of the panel GameObject so position is relative to the panel transform.
    /// All positions use local coordinates to ensure alignment regardless of panel placement.
    /// </summary>
    public class ViolationMarker : MonoBehaviour
    {
        [Header("Violation Data")]
        [SerializeField] private string violationId;
        [SerializeField] private string violationType;
        [SerializeField] private string necCode;
        [SerializeField] private string description;
        [SerializeField] private string hintText;
        [SerializeField] private string severity;

        [Header("Visual")]
        [SerializeField] private MeshRenderer highlightRenderer;
        [SerializeField] private float highlightRadius = 0.03f;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f); // Invisible by default
        [SerializeField] private Color learnHighlightColor = new Color(1f, 0.8f, 0f, 0.5f); // Gold
        [SerializeField] private Color hintGlowColor = new Color(0f, 0.8f, 1f, 0.4f); // Cyan
        [SerializeField] private Color identifiedColor = new Color(0f, 1f, 0f, 0.5f); // Green
        [SerializeField] private Color missedColor = new Color(1f, 0f, 0f, 0.5f); // Red

        [Header("Label")]
        [SerializeField] private FloatingLabel floatingLabel;
        [SerializeField] private Vector3 labelOffset = new Vector3(0f, 0.05f, -0.02f);

        private bool isIdentified;
        private bool isHighlighted;
        private bool isHintGlowing;
        private float glowTimer;
        private float glowDuration = 5f;
        private Material highlightMaterial;

        public string ViolationId => violationId;
        public string NecCode => necCode;
        public string Description => description;
        public string HintText => hintText;
        public bool IsIdentified => isIdentified;

        private void Awake()
        {
            // Create highlight sphere if not assigned
            if (highlightRenderer == null)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(transform, false);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = Vector3.one * highlightRadius * 2f;

                // Remove collider from visual sphere (we use our own)
                Collider sphereCol = sphere.GetComponent<Collider>();
                if (sphereCol != null) Destroy(sphereCol);

                highlightRenderer = sphere.GetComponent<MeshRenderer>();
            }

            // Create transparent material
            highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.SetFloat("_Mode", 3); // Transparent mode
            highlightMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            highlightMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            highlightMaterial.SetInt("_ZWrite", 0);
            highlightMaterial.DisableKeyword("_ALPHATEST_ON");
            highlightMaterial.EnableKeyword("_ALPHABLEND_ON");
            highlightMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            highlightMaterial.renderQueue = 3000;
            highlightMaterial.color = normalColor;
            highlightRenderer.material = highlightMaterial;

            // Add collider for interaction (trigger)
            SphereCollider col = gameObject.GetComponent<SphereCollider>();
            if (col == null)
            {
                col = gameObject.AddComponent<SphereCollider>();
            }
            col.radius = highlightRadius * 1.5f; // Slightly larger than visual
            col.isTrigger = true;

            // Create floating label
            if (floatingLabel == null)
            {
                GameObject labelObj = new GameObject("ViolationLabel");
                labelObj.transform.SetParent(transform, false);
                labelObj.transform.localPosition = labelOffset;
                floatingLabel = labelObj.AddComponent<FloatingLabel>();
            }
            floatingLabel.gameObject.SetActive(false);
        }

        /// <summary>
        /// Initialize the marker from a violation definition.
        /// Position is set as LOCAL position relative to parent (the panel).
        /// </summary>
        public void Initialize(ViolationDefinition violationDef)
        {
            violationId = violationDef.id;
            violationType = violationDef.type;
            necCode = violationDef.necCode;
            description = violationDef.description;
            hintText = violationDef.hintText;
            severity = violationDef.severity;

            // CRITICAL: Set as LOCAL position relative to parent panel
            transform.localPosition = violationDef.localPosition.ToVector3();

            // Set severity-based highlight color
            if (severity == "critical")
            {
                learnHighlightColor = new Color(1f, 0.3f, 0f, 0.5f); // Orange-red
            }

            isIdentified = false;
            isHighlighted = false;

            Debug.Log($"[ViolationMarker] Initialized: {violationId} at local pos {transform.localPosition}");
        }

        private void Update()
        {
            // Hint glow timer
            if (isHintGlowing)
            {
                glowTimer -= Time.deltaTime;
                float pulse = Mathf.Sin(Time.time * 4f) * 0.3f + 0.7f;
                Color glowColor = hintGlowColor;
                glowColor.a *= pulse;
                highlightMaterial.color = glowColor;

                if (glowTimer <= 0f)
                {
                    StopHintGlow();
                }
            }
        }

        // --- State Methods ---

        public void ShowLearnHighlight()
        {
            isHighlighted = true;
            highlightMaterial.color = learnHighlightColor;

            if (floatingLabel != null)
            {
                NECCodeEntry codeEntry = NECDatabase.GetCode(necCode);
                string labelText = codeEntry != null
                    ? $"NEC {necCode}\n{codeEntry.title}\n{description}"
                    : $"NEC {necCode}\n{description}";
                floatingLabel.SetText(labelText);
                floatingLabel.gameObject.SetActive(true);
            }
        }

        public void HideHighlight()
        {
            isHighlighted = false;
            isHintGlowing = false;
            highlightMaterial.color = normalColor;

            if (floatingLabel != null)
            {
                floatingLabel.gameObject.SetActive(false);
            }
        }

        public void StartHintGlow()
        {
            isHintGlowing = true;
            glowTimer = glowDuration;
            highlightMaterial.color = hintGlowColor;
        }

        public void StopHintGlow()
        {
            isHintGlowing = false;
            if (!isIdentified && !isHighlighted)
            {
                highlightMaterial.color = normalColor;
            }
        }

        public void MarkAsIdentified()
        {
            isIdentified = true;
            isHintGlowing = false;
            highlightMaterial.color = identifiedColor;

            if (floatingLabel != null)
            {
                NECCodeEntry codeEntry = NECDatabase.GetCode(necCode);
                string labelText = codeEntry != null
                    ? $"FOUND: NEC {necCode}\n{codeEntry.title}"
                    : $"FOUND: NEC {necCode}";
                floatingLabel.SetText(labelText);
                floatingLabel.SetColor(Color.green);
                floatingLabel.gameObject.SetActive(true);
            }
        }

        public void MarkAsMissed()
        {
            if (!isIdentified)
            {
                highlightMaterial.color = missedColor;

                if (floatingLabel != null)
                {
                    NECCodeEntry codeEntry = NECDatabase.GetCode(necCode);
                    string labelText = codeEntry != null
                        ? $"MISSED: NEC {necCode}\n{codeEntry.title}\n{description}"
                        : $"MISSED: NEC {necCode}\n{description}";
                    floatingLabel.SetText(labelText);
                    floatingLabel.SetColor(Color.red);
                    floatingLabel.gameObject.SetActive(true);
                }
            }
        }

        // --- Interaction ---

        /// <summary>
        /// Called when the user points at / selects this violation marker.
        /// </summary>
        public bool TryIdentify()
        {
            if (isIdentified) return false;

            bool success = Core.TaskManager.Instance.IdentifyViolation(violationId);
            if (success)
            {
                MarkAsIdentified();
            }
            return success;
        }
    }
}
