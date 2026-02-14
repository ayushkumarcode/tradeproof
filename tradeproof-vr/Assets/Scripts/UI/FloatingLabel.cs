using UnityEngine;
using TMPro;

namespace TradeProof.UI
{
    /// <summary>
    /// Billboard text that always faces the user camera.
    /// Used for NEC code annotations on violations.
    /// Maintains readable distance from user (0.5-1.5m).
    /// Scales based on distance to remain legible.
    /// </summary>
    public class FloatingLabel : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private string labelText = "";
        [SerializeField] private float fontSize = 2.5f;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private TextAlignmentOptions alignment = TextAlignmentOptions.Center;

        [Header("Billboard")]
        [SerializeField] private bool billboardEnabled = true;
        [SerializeField] private bool lockYAxis = false; // Only rotate around Y

        [Header("Background")]
        [SerializeField] private bool showBackground = true;
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.15f, 0.85f);
        [SerializeField] private float backgroundPadding = 0.01f;
        [SerializeField] private MeshRenderer backgroundRenderer;

        [Header("Distance Scaling")]
        [SerializeField] private bool scaleWithDistance = true;
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 2.0f;
        [SerializeField] private float referenceDistance = 1.0f;
        [SerializeField] private float scaleMultiplier = 1.0f;

        [Header("Animation")]
        [SerializeField] private bool fadeInOnEnable = true;
        [SerializeField] private float fadeInDuration = 0.3f;
        private float fadeTimer;
        private float currentAlpha;

        private Camera mainCamera;
        private Vector3 baseScale;

        private void Awake()
        {
            SetupTextMesh();
            SetupBackground();
            baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (fadeInOnEnable)
            {
                fadeTimer = 0f;
                currentAlpha = 0f;
                UpdateAlpha(0f);
            }
        }

        private void SetupTextMesh()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshPro>();
                if (textMesh == null)
                {
                    textMesh = gameObject.AddComponent<TextMeshPro>();
                }
            }

            textMesh.text = labelText;
            textMesh.fontSize = fontSize;
            textMesh.color = textColor;
            textMesh.alignment = alignment;
            textMesh.enableWordWrapping = true;
            textMesh.overflowMode = TextOverflowModes.Overflow;
            textMesh.rectTransform.sizeDelta = new Vector2(0.3f, 0.15f);
            textMesh.sortingOrder = 100; // Render on top
        }

        private void SetupBackground()
        {
            if (!showBackground) return;

            if (backgroundRenderer == null)
            {
                GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bgObj.name = "LabelBackground";
                bgObj.transform.SetParent(transform, false);
                bgObj.transform.localPosition = new Vector3(0f, 0f, 0.001f); // Slightly behind text

                // Remove collider
                Collider col = bgObj.GetComponent<Collider>();
                if (col != null) Destroy(col);

                backgroundRenderer = bgObj.GetComponent<MeshRenderer>();

                Material bgMat = new Material(Shader.Find("Standard"));
                bgMat.SetFloat("_Mode", 3); // Transparent
                bgMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                bgMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                bgMat.SetInt("_ZWrite", 0);
                bgMat.EnableKeyword("_ALPHABLEND_ON");
                bgMat.renderQueue = 3000;
                bgMat.color = backgroundColor;

                backgroundRenderer.material = bgMat;
            }
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // Billboard rotation — always face the camera
            if (billboardEnabled)
            {
                Vector3 dirToCamera = mainCamera.transform.position - transform.position;

                if (lockYAxis)
                {
                    dirToCamera.y = 0f;
                }

                if (dirToCamera.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(-dirToCamera.normalized, Vector3.up);
                }
            }

            // Distance-based scaling
            if (scaleWithDistance)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
                float scaleFactor = (distance / referenceDistance) * scaleMultiplier;
                scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale);
                transform.localScale = baseScale * scaleFactor;
            }

            // Fade in animation
            if (fadeInOnEnable && currentAlpha < 1f)
            {
                fadeTimer += Time.deltaTime;
                currentAlpha = Mathf.Clamp01(fadeTimer / fadeInDuration);
                UpdateAlpha(currentAlpha);
            }

            // Update background size to match text bounds
            UpdateBackgroundSize();
        }

        private void UpdateAlpha(float alpha)
        {
            if (textMesh != null)
            {
                Color c = textMesh.color;
                c.a = alpha;
                textMesh.color = c;
            }

            if (backgroundRenderer != null)
            {
                Color bgc = backgroundRenderer.material.color;
                bgc.a = backgroundColor.a * alpha;
                backgroundRenderer.material.color = bgc;
            }
        }

        private void UpdateBackgroundSize()
        {
            if (backgroundRenderer == null || textMesh == null) return;

            // Get text bounds
            Vector2 textSize = textMesh.GetPreferredValues(textMesh.text);
            float width = textSize.x + backgroundPadding * 2f;
            float height = textSize.y + backgroundPadding * 2f;

            backgroundRenderer.transform.localScale = new Vector3(
                Mathf.Max(width, 0.05f),
                Mathf.Max(height, 0.03f),
                1f
            );
        }

        // --- Public API ---

        public void SetText(string text)
        {
            labelText = text;
            if (textMesh != null)
            {
                textMesh.text = text;
            }
        }

        public void SetColor(Color color)
        {
            textColor = color;
            if (textMesh != null)
            {
                textMesh.color = new Color(color.r, color.g, color.b, currentAlpha);
            }
        }

        public void SetFontSize(float size)
        {
            fontSize = size;
            if (textMesh != null)
            {
                textMesh.fontSize = size;
            }
        }

        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (backgroundRenderer != null)
            {
                backgroundRenderer.material.color = color;
            }
        }

        public void ShowBackground(bool show)
        {
            showBackground = show;
            if (backgroundRenderer != null)
            {
                backgroundRenderer.gameObject.SetActive(show);
            }
        }

        public void SetBillboard(bool enabled)
        {
            billboardEnabled = enabled;
        }
    }
}
