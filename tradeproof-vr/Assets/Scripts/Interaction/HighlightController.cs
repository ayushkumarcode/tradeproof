using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TradeProof.Interaction
{
    /// <summary>
    /// Controls highlighting of objects in the scene.
    /// Used to draw attention to specific components during learn mode.
    /// Manages highlight outline effects and glow animations.
    /// </summary>
    public class HighlightController : MonoBehaviour
    {
        private static HighlightController _instance;
        public static HighlightController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<HighlightController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("HighlightController");
                        _instance = go.AddComponent<HighlightController>();
                    }
                }
                return _instance;
            }
        }

        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = new Color(0f, 0.8f, 1f, 1f); // Cyan
        [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f, 1f); // Orange
        [SerializeField] private Color correctColor = new Color(0f, 1f, 0f, 1f); // Green
        [SerializeField] private Color errorColor = new Color(1f, 0f, 0f, 1f); // Red
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseIntensity = 0.5f;

        [Header("Active Highlights")]
        private Dictionary<GameObject, HighlightInfo> activeHighlights = new Dictionary<GameObject, HighlightInfo>();

        private class HighlightInfo
        {
            public MeshRenderer renderer;
            public Material originalMaterial;
            public Material highlightMaterial;
            public Color originalColor;
            public Color targetColor;
            public float duration;
            public float elapsed;
            public bool permanent;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            List<GameObject> toRemove = new List<GameObject>();

            foreach (var kvp in activeHighlights)
            {
                HighlightInfo info = kvp.Value;
                if (info.renderer == null)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }

                // Update pulse effect
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + (1f - pulseIntensity);

                if (info.highlightMaterial != null)
                {
                    Color emissionColor = info.targetColor * pulse * 0.5f;
                    info.highlightMaterial.SetColor("_EmissionColor", emissionColor);
                }

                // Check duration
                if (!info.permanent)
                {
                    info.elapsed += Time.deltaTime;
                    if (info.elapsed >= info.duration)
                    {
                        // Restore original
                        RestoreHighlight(info);
                        toRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var key in toRemove)
            {
                activeHighlights.Remove(key);
            }
        }

        // --- Public API ---

        public void HighlightObject(GameObject obj, float duration = 3f)
        {
            ApplyHighlight(obj, highlightColor, duration, false);
        }

        public void HighlightObjectPermanent(GameObject obj)
        {
            ApplyHighlight(obj, highlightColor, 0f, true);
        }

        public void HighlightWarning(GameObject obj, float duration = 3f)
        {
            ApplyHighlight(obj, warningColor, duration, false);
        }

        public void HighlightCorrect(GameObject obj, float duration = 2f)
        {
            ApplyHighlight(obj, correctColor, duration, false);
        }

        public void HighlightError(GameObject obj, float duration = 2f)
        {
            ApplyHighlight(obj, errorColor, duration, false);
        }

        public void RemoveHighlight(GameObject obj)
        {
            if (activeHighlights.TryGetValue(obj, out HighlightInfo info))
            {
                RestoreHighlight(info);
                activeHighlights.Remove(obj);
            }
        }

        public void RemoveAllHighlights()
        {
            foreach (var kvp in activeHighlights)
            {
                if (kvp.Value.renderer != null)
                {
                    RestoreHighlight(kvp.Value);
                }
            }
            activeHighlights.Clear();
        }

        // --- Implementation ---

        private void ApplyHighlight(GameObject obj, Color color, float duration, bool permanent)
        {
            if (obj == null) return;

            // Remove existing highlight if any
            RemoveHighlight(obj);

            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = obj.GetComponentInChildren<MeshRenderer>();
            }
            if (renderer == null) return;

            HighlightInfo info = new HighlightInfo();
            info.renderer = renderer;
            info.originalMaterial = renderer.material;
            info.originalColor = renderer.material.color;
            info.targetColor = color;
            info.duration = duration;
            info.elapsed = 0f;
            info.permanent = permanent;

            // Create highlight material
            info.highlightMaterial = new Material(renderer.material);
            info.highlightMaterial.EnableKeyword("_EMISSION");
            info.highlightMaterial.SetColor("_EmissionColor", color * 0.3f);

            // Apply slight color tint
            Color tintedColor = Color.Lerp(info.originalColor, color, 0.3f);
            info.highlightMaterial.color = tintedColor;

            renderer.material = info.highlightMaterial;
            activeHighlights[obj] = info;
        }

        private void RestoreHighlight(HighlightInfo info)
        {
            if (info.renderer != null && info.originalMaterial != null)
            {
                info.renderer.material = info.originalMaterial;
            }

            if (info.highlightMaterial != null)
            {
                Destroy(info.highlightMaterial);
            }
        }

        // --- Utility ---

        /// <summary>
        /// Creates a highlight outline around an object using a scaled duplicate.
        /// More visible than emission-only highlighting.
        /// </summary>
        public GameObject CreateOutline(GameObject obj, Color color, float outlineWidth = 0.003f)
        {
            if (obj == null) return null;

            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return null;

            GameObject outline = new GameObject($"{obj.name}_Outline");
            outline.transform.SetParent(obj.transform, false);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localRotation = Quaternion.identity;
            outline.transform.localScale = Vector3.one * (1f + outlineWidth);

            MeshFilter outlineMF = outline.AddComponent<MeshFilter>();
            outlineMF.sharedMesh = meshFilter.sharedMesh;

            MeshRenderer outlineMR = outline.AddComponent<MeshRenderer>();
            Material outlineMat = new Material(Shader.Find("Standard"));
            outlineMat.color = color;
            outlineMat.SetFloat("_Mode", 3);
            outlineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMat.SetInt("_ZWrite", 0);
            outlineMat.EnableKeyword("_ALPHABLEND_ON");
            outlineMat.renderQueue = 3000;
            outlineMat.EnableKeyword("_EMISSION");
            outlineMat.SetColor("_EmissionColor", color * 0.5f);

            // Front face culling (show outline only)
            outlineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);

            outlineMR.material = outlineMat;

            return outline;
        }
    }
}
