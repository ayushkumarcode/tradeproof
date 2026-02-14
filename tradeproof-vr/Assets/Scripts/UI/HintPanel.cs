using UnityEngine;
using TMPro;
using System.Collections;

namespace TradeProof.UI
{
    /// <summary>
    /// Floating panel that appears when a hint is requested.
    /// Shows hint text and auto-hides after 5 seconds (configurable).
    /// Positioned to the side of the user's view for readability.
    /// </summary>
    public class HintPanel : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas canvas;

        [Header("Elements")]
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private TextMeshProUGUI headerText;

        [Header("Settings")]
        [SerializeField] private float defaultDisplayDuration = 5f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Positioning")]
        [SerializeField] private float distanceFromPlayer = 0.7f;
        [SerializeField] private float horizontalOffset = -0.3f; // Left of center
        [SerializeField] private float verticalOffset = 0.0f;
        [SerializeField] private float followSpeed = 4f;

        [Header("Visual")]
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.1f, 0.2f, 0.92f);
        [SerializeField] private Color headerColor = new Color(0.9f, 0.7f, 0.1f, 1f);
        [SerializeField] private Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private CanvasGroup canvasGroup;
        private Coroutine displayCoroutine;
        private Camera playerCamera;

        private void Awake()
        {
            SetupCanvas();
            SetupElements();

            canvasGroup = canvas.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
        }

        private void Start()
        {
            playerCamera = Camera.main;
        }

        private void SetupCanvas()
        {
            if (canvas == null)
            {
                canvas = gameObject.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                }
            }

            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400f, 250f);
            rt.localScale = Vector3.one * 0.0008f;

            UnityEngine.UI.Image bg = canvas.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (bg == null)
            {
                bg = canvas.gameObject.AddComponent<UnityEngine.UI.Image>();
            }
            bg.color = backgroundColor;
        }

        private void SetupElements()
        {
            // Header
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(canvas.transform, false);

            headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "HINT";
            headerText.fontSize = 24;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = headerColor;

            RectTransform headerRT = headerObj.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.05f, 0.82f);
            headerRT.anchorMax = new Vector2(0.95f, 0.98f);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            // Hint text
            GameObject textObj = new GameObject("HintText");
            textObj.transform.SetParent(canvas.transform, false);

            hintText = textObj.AddComponent<TextMeshProUGUI>();
            hintText.text = "";
            hintText.fontSize = 18;
            hintText.alignment = TextAlignmentOptions.TopLeft;
            hintText.color = textColor;
            hintText.enableWordWrapping = true;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.08f, 0.05f);
            textRT.anchorMax = new Vector2(0.92f, 0.80f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            forward.y = 0f;
            forward.Normalize();

            Vector3 targetPos = playerCamera.transform.position +
                                forward * distanceFromPlayer +
                                right * horizontalOffset +
                                Vector3.up * verticalOffset;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.LookRotation(
                (transform.position - playerCamera.transform.position).normalized,
                Vector3.up
            );
        }

        // --- Public API ---

        public void ShowHint(string text, float duration = -1f)
        {
            if (duration < 0f) duration = defaultDisplayDuration;

            if (hintText != null) hintText.text = text;

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            gameObject.SetActive(true);
            displayCoroutine = StartCoroutine(DisplayCoroutine(duration));
        }

        public void HideHint()
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            StartCoroutine(FadeOut());
        }

        private IEnumerator DisplayCoroutine(float duration)
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Wait for duration
            yield return new WaitForSeconds(duration);

            // Fade out
            yield return FadeOut();
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void SetHeader(string text)
        {
            if (headerText != null) headerText.text = text;
        }
    }
}
