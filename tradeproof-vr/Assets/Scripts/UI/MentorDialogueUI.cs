using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace TradeProof.UI
{
    /// <summary>
    /// Small world-space speech bubble that follows the AI mentor icon.
    /// Features fade-in/out animations and a message queue system.
    /// </summary>
    public class MentorDialogueUI : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject bubblePanel;
        private TextMeshProUGUI messageText;
        private GameObject pointerTriangle;
        private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.4f;

        // State
        private bool isShowing;
        private float currentDisplayDuration;
        private float displayTimer;
        private Coroutine fadeCoroutine;
        private Coroutine displayCoroutine;

        // Message queue
        private Queue<(string message, float duration)> messageQueue = new Queue<(string, float)>();

        private Camera playerCamera;

        private void Awake()
        {
            CreateBubbleUI();
            HideImmediate();
        }

        private void CreateBubbleUI()
        {
            // World-space canvas: 350x150
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(350f, 150f);
            canvasRT.localScale = Vector3.one * 0.0008f; // Small bubble: 350*0.0008 = 0.28m wide

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            gameObject.AddComponent<CanvasScaler>();

            // Bubble background -- rounded rectangle via solid image
            bubblePanel = new GameObject("BubbleBackground");
            bubblePanel.transform.SetParent(canvas.transform, false);
            RectTransform panelRT = bubblePanel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0f, 0.15f); // Leave room for pointer at bottom
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image bubbleBg = bubblePanel.AddComponent<Image>();
            bubbleBg.color = new Color(0.08f, 0.08f, 0.15f, 0.88f); // Dark semi-transparent

            // Message text
            GameObject textObj = new GameObject("MessageText");
            textObj.transform.SetParent(bubblePanel.transform, false);
            messageText = textObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "";
            messageText.fontSize = 18;
            messageText.color = Color.white;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.enableWordWrapping = true;
            messageText.overflowMode = TextOverflowModes.Ellipsis;
            messageText.margin = new Vector4(8f, 6f, 8f, 6f);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.05f, 0.05f);
            textRT.anchorMax = new Vector2(0.95f, 0.95f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            // Pointer triangle at bottom (pointing toward mentor below)
            CreatePointerTriangle();
        }

        private void CreatePointerTriangle()
        {
            pointerTriangle = new GameObject("Pointer");
            pointerTriangle.transform.SetParent(canvas.transform, false);
            RectTransform ptrRT = pointerTriangle.AddComponent<RectTransform>();
            ptrRT.anchorMin = new Vector2(0.42f, 0f);
            ptrRT.anchorMax = new Vector2(0.58f, 0.18f);
            ptrRT.offsetMin = Vector2.zero;
            ptrRT.offsetMax = Vector2.zero;

            // Simple triangle via image (a small dark quad that mimics a pointer)
            Image ptrImg = pointerTriangle.AddComponent<Image>();
            ptrImg.color = new Color(0.08f, 0.08f, 0.15f, 0.88f); // Match bubble

            // Rotate to form a downward-pointing triangle shape
            ptrRT.localRotation = Quaternion.Euler(0f, 0f, 45f);
            ptrRT.localScale = new Vector3(0.7f, 0.7f, 1f);
        }

        private void LateUpdate()
        {
            BillboardToCamera();
        }

        private void BillboardToCamera()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            Vector3 dir = playerCamera.transform.position - transform.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(-dir.normalized, Vector3.up);
            }
        }

        // --- Public API ---

        /// <summary>
        /// Displays a message in the speech bubble for the given duration with fade-in.
        /// If a message is already showing, the new message is queued.
        /// </summary>
        public void ShowMessage(string text, float duration = 5f)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (isShowing)
            {
                // Queue the message
                messageQueue.Enqueue((text, duration));
                return;
            }

            StartDisplaying(text, duration);
        }

        /// <summary>
        /// Hides the current message with fade-out. Processes queue after hiding.
        /// </summary>
        public void HideMessage()
        {
            if (!isShowing) return;

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            fadeCoroutine = StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Clears all queued messages and hides immediately.
        /// </summary>
        public void ClearAll()
        {
            messageQueue.Clear();

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            HideImmediate();
        }

        // --- Internal ---

        private void StartDisplaying(string text, float duration)
        {
            messageText.text = text;
            currentDisplayDuration = duration;
            isShowing = true;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }

            fadeCoroutine = StartCoroutine(FadeIn());
            displayCoroutine = StartCoroutine(AutoHideAfterDuration(duration));
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            fadeCoroutine = null;
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            isShowing = false;
            fadeCoroutine = null;

            // Process next message in queue
            ProcessQueue();
        }

        private IEnumerator AutoHideAfterDuration(float duration)
        {
            yield return new WaitForSeconds(duration);

            displayCoroutine = null;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            fadeCoroutine = StartCoroutine(FadeOut());
        }

        private void ProcessQueue()
        {
            if (messageQueue.Count > 0)
            {
                var (message, duration) = messageQueue.Dequeue();
                StartDisplaying(message, duration);
            }
        }

        private void HideImmediate()
        {
            isShowing = false;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
    }
}
