using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TradeProof.UI
{
    public class WorkOrderBoardUI : MonoBehaviour
    {
        private Canvas canvas;

        private void Awake()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            canvas.transform.localScale = Vector3.one * 0.001f;

            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.08f, 0.05f, 0.95f);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(bg.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "JOB BOARD";
            title.fontSize = 40;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(0.9f, 0.7f, 0.1f);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Camera cam = Core.GameManager.Instance.MainCamera ?? Camera.main;
            if (cam != null)
            {
                Vector3 forward = cam.transform.forward;
                forward.y = 0;
                forward.Normalize();
                transform.position = cam.transform.position + forward * 1.3f + Vector3.up * 0.2f;
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
