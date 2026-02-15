using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using TradeProof.Core;
using TradeProof.Data;
using TradeProof.UI;

namespace TradeProof.AI
{
    /// <summary>
    /// Simple humanoid NPC representing a customer at a job site.
    /// Visual: capsule body + sphere head + thin cube arms.
    /// Shows dialogue panel when player approaches within 2m.
    /// </summary>
    public class CustomerNPC : MonoBehaviour
    {
        [Header("Customer Info")]
        [SerializeField] private string customerName = "Customer";
        public string CustomerName
        {
            get => customerName;
            set
            {
                customerName = value;
                if (nameLabel != null) nameLabel.SetText(value);
            }
        }

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private float dialoguePanelDistance = 1.2f;

        // Visual components
        private GameObject bodyRoot;
        private GameObject bodyObj;
        private GameObject headObj;
        private GameObject leftArm;
        private GameObject rightArm;
        private FloatingLabel nameLabel;

        // Dialogue system
        private Canvas dialogueCanvas;
        private GameObject dialoguePanel;
        private TextMeshProUGUI dialogueText;
        private List<Button> choiceButtons = new List<Button>();
        private List<GameObject> choiceButtonObjects = new List<GameObject>();
        private GameObject speechIndicator;

        // State
        private DialogueDefinition currentDialogue;
        private DialogueDefinition[] dialogueSequence;
        private int currentDialogueIndex;
        private bool isDialogueActive;
        private bool isPlayerInRange;
        private Camera playerCamera;

        // Idle animation
        private float idleBobPhase;

        // Callback
        public System.Action<int, int> OnDiagnosticPointsEarned; // (points, totalPossible)

        private void Awake()
        {
            CreateBody();
            CreateNameLabel();
            CreateDialoguePanel();
            HideDialogue();
        }

        private void CreateBody()
        {
            bodyRoot = new GameObject("CustomerBody");
            bodyRoot.transform.SetParent(transform, false);

            // Body: capsule 0.35m radius, 0.9m height, positioned at 0.9m above ground
            bodyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyObj.name = "Body";
            bodyObj.transform.SetParent(bodyRoot.transform, false);
            bodyObj.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            bodyObj.transform.localScale = new Vector3(0.35f, 0.45f, 0.25f); // capsule height is 2*scale.y
            Renderer bodyRend = bodyObj.GetComponent<Renderer>();
            Material shirtMat = new Material(Shader.Find("Standard"));
            shirtMat.color = new Color(0.2f, 0.4f, 0.7f); // Blue shirt
            bodyRend.material = shirtMat;

            // Remove body collider (we'll add our own trigger)
            Collider bodyCol = bodyObj.GetComponent<Collider>();
            if (bodyCol != null) Destroy(bodyCol);

            // Legs (lower body): capsule for pants
            GameObject legs = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            legs.name = "Legs";
            legs.transform.SetParent(bodyRoot.transform, false);
            legs.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            legs.transform.localScale = new Vector3(0.3f, 0.35f, 0.22f);
            Renderer legsRend = legs.GetComponent<Renderer>();
            Material pantsMat = new Material(Shader.Find("Standard"));
            pantsMat.color = new Color(0.15f, 0.15f, 0.2f); // Dark pants
            legsRend.material = pantsMat;
            Collider legsCol = legs.GetComponent<Collider>();
            if (legsCol != null) Destroy(legsCol);

            // Head: sphere 0.15m radius at 1.65m height
            headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headObj.name = "Head";
            headObj.transform.SetParent(bodyRoot.transform, false);
            headObj.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            headObj.transform.localScale = Vector3.one * 0.3f; // diameter 0.3m = radius 0.15m
            Renderer headRend = headObj.GetComponent<Renderer>();
            Material skinMat = new Material(Shader.Find("Standard"));
            skinMat.color = new Color(0.85f, 0.7f, 0.55f); // Skin tone
            headRend.material = skinMat;
            Collider headCol = headObj.GetComponent<Collider>();
            if (headCol != null) Destroy(headCol);

            // Left arm
            leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(bodyRoot.transform, false);
            leftArm.transform.localPosition = new Vector3(-0.28f, 0.9f, 0f);
            leftArm.transform.localScale = new Vector3(0.08f, 0.55f, 0.08f);
            Renderer leftArmRend = leftArm.GetComponent<Renderer>();
            leftArmRend.material = shirtMat;
            Collider leftArmCol = leftArm.GetComponent<Collider>();
            if (leftArmCol != null) Destroy(leftArmCol);

            // Right arm
            rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(bodyRoot.transform, false);
            rightArm.transform.localPosition = new Vector3(0.28f, 0.9f, 0f);
            rightArm.transform.localScale = new Vector3(0.08f, 0.55f, 0.08f);
            Renderer rightArmRend = rightArm.GetComponent<Renderer>();
            rightArmRend.material = shirtMat;
            Collider rightArmCol = rightArm.GetComponent<Collider>();
            if (rightArmCol != null) Destroy(rightArmCol);

            // Add a sphere trigger for proximity detection
            SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1f, 0f);
            trigger.radius = interactionRange;
        }

        private void CreateNameLabel()
        {
            GameObject labelObj = new GameObject("NameLabel");
            labelObj.transform.SetParent(transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 1.9f, 0f);
            nameLabel = labelObj.AddComponent<FloatingLabel>();
            nameLabel.SetText(customerName);
            nameLabel.SetFontSize(3f);
            nameLabel.SetColor(Color.white);
        }

        private void CreateDialoguePanel()
        {
            // World-space canvas: 0.5m x 0.3m
            GameObject canvasObj = new GameObject("DialogueCanvas");
            canvasObj.transform.SetParent(transform, false);
            canvasObj.transform.localPosition = new Vector3(0f, 2.1f, 0f);

            dialogueCanvas = canvasObj.AddComponent<Canvas>();
            dialogueCanvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRT = dialogueCanvas.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(500f, 300f);
            canvasRT.localScale = Vector3.one * 0.001f; // 500 * 0.001 = 0.5m

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Dialogue panel background
            dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(dialogueCanvas.transform, false);
            RectTransform panelRT = dialoguePanel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image panelBg = dialoguePanel.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.08f, 0.15f, 0.92f);

            // Dialogue text
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(dialoguePanel.transform, false);
            dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "";
            dialogueText.fontSize = 22;
            dialogueText.color = Color.white;
            dialogueText.enableWordWrapping = true;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.05f, 0.45f);
            textRT.anchorMax = new Vector2(0.95f, 0.92f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            // Speaker name area
            GameObject speakerObj = new GameObject("SpeakerName");
            speakerObj.transform.SetParent(dialoguePanel.transform, false);
            TextMeshProUGUI speakerText = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerText.text = customerName;
            speakerText.fontSize = 18;
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.color = new Color(0.5f, 0.8f, 1f);
            RectTransform speakerRT = speakerObj.GetComponent<RectTransform>();
            speakerRT.anchorMin = new Vector2(0.05f, 0.92f);
            speakerRT.anchorMax = new Vector2(0.95f, 1f);
            speakerRT.offsetMin = Vector2.zero;
            speakerRT.offsetMax = Vector2.zero;

            // Pre-create 4 choice buttons
            for (int i = 0; i < 4; i++)
            {
                GameObject btnObj = CreateChoiceButton(i);
                choiceButtonObjects.Add(btnObj);
                Button btn = btnObj.GetComponent<Button>();
                choiceButtons.Add(btn);
                btnObj.SetActive(false);
            }

            // Speech indicator (small animated dot)
            speechIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            speechIndicator.name = "SpeechIndicator";
            speechIndicator.transform.SetParent(transform, false);
            speechIndicator.transform.localPosition = new Vector3(0.2f, 1.7f, 0.15f);
            speechIndicator.transform.localScale = Vector3.one * 0.04f;
            Renderer indicatorRend = speechIndicator.GetComponent<Renderer>();
            Material indicatorMat = new Material(Shader.Find("Standard"));
            indicatorMat.color = new Color(0.3f, 0.8f, 1f);
            indicatorMat.EnableKeyword("_EMISSION");
            indicatorMat.SetColor("_EmissionColor", new Color(0.3f, 0.8f, 1f) * 0.5f);
            indicatorRend.material = indicatorMat;
            Collider indicatorCol = speechIndicator.GetComponent<Collider>();
            if (indicatorCol != null) Destroy(indicatorCol);
            speechIndicator.SetActive(false);
        }

        private GameObject CreateChoiceButton(int index)
        {
            float btnHeight = 0.1f;
            float btnSpacing = 0.005f;
            float startY = 0.02f;

            GameObject btnObj = new GameObject($"ChoiceButton_{index}");
            btnObj.transform.SetParent(dialoguePanel.transform, false);
            RectTransform btnRT = btnObj.AddComponent<RectTransform>();

            float yMin = startY + index * (btnHeight + btnSpacing);
            float yMax = yMin + btnHeight;
            btnRT.anchorMin = new Vector2(0.05f, yMin);
            btnRT.anchorMax = new Vector2(0.95f, yMax);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.15f, 0.25f, 0.4f, 0.9f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.25f, 0.4f, 0.6f);
            colors.pressedColor = new Color(0.1f, 0.2f, 0.35f);
            btn.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Choice";
            btnText.fontSize = 16;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.MidlineLeft;
            btnText.enableWordWrapping = true;
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.03f, 0f);
            textRT.anchorMax = new Vector2(0.97f, 1f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            int capturedIndex = index;
            btn.onClick.AddListener(() => OnChoiceSelected(capturedIndex));

            return btnObj;
        }

        private void Update()
        {
            AnimateIdle();
            CheckPlayerProximity();
            BillboardDialogue();
            AnimateSpeechIndicator();
        }

        /// <summary>
        /// Subtle idle bob animation for the NPC.
        /// </summary>
        public void AnimateIdle()
        {
            if (bodyRoot == null) return;

            idleBobPhase += Time.deltaTime;
            float bob = Mathf.Sin(idleBobPhase * 1.5f) * 0.005f;
            bodyRoot.transform.localPosition = new Vector3(0f, bob, 0f);

            // Slight arm sway
            if (leftArm != null)
            {
                float armSway = Mathf.Sin(idleBobPhase * 1.2f) * 3f;
                leftArm.transform.localRotation = Quaternion.Euler(0f, 0f, armSway);
            }
            if (rightArm != null)
            {
                float armSway = Mathf.Sin(idleBobPhase * 1.2f + Mathf.PI) * 3f;
                rightArm.transform.localRotation = Quaternion.Euler(0f, 0f, armSway);
            }
        }

        private void CheckPlayerProximity()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
            bool inRange = distance <= interactionRange;

            if (inRange && !isPlayerInRange)
            {
                isPlayerInRange = true;
                OnPlayerEnteredRange();
            }
            else if (!inRange && isPlayerInRange)
            {
                isPlayerInRange = false;
                OnPlayerExitedRange();
            }
        }

        private void BillboardDialogue()
        {
            if (dialogueCanvas == null || playerCamera == null) return;

            // Make dialogue panel face the camera
            Vector3 dirToCamera = playerCamera.transform.position - dialogueCanvas.transform.position;
            if (dirToCamera.sqrMagnitude > 0.001f)
            {
                dialogueCanvas.transform.rotation = Quaternion.LookRotation(-dirToCamera.normalized, Vector3.up);
            }
        }

        private void AnimateSpeechIndicator()
        {
            if (speechIndicator == null || !speechIndicator.activeSelf) return;

            float pulse = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
            float scale = Mathf.Lerp(0.03f, 0.05f, pulse);
            speechIndicator.transform.localScale = Vector3.one * scale;
        }

        private void OnPlayerEnteredRange()
        {
            if (currentDialogue != null && !isDialogueActive)
            {
                ShowDialoguePanel();
            }
        }

        private void OnPlayerExitedRange()
        {
            if (isDialogueActive)
            {
                HideDialogue();
            }
        }

        // --- Public API ---

        /// <summary>
        /// Assigns a full dialogue sequence and starts from the first entry.
        /// </summary>
        public void ShowDialogue(DialogueDefinition dialogue)
        {
            currentDialogue = dialogue;
            if (isPlayerInRange)
            {
                ShowDialoguePanel();
            }
        }

        /// <summary>
        /// Assigns an array of dialogue definitions for sequential display.
        /// </summary>
        public void SetDialogueSequence(DialogueDefinition[] dialogues)
        {
            dialogueSequence = dialogues;
            currentDialogueIndex = 0;
            if (dialogues != null && dialogues.Length > 0)
            {
                ShowDialogue(dialogues[0]);
            }
        }

        /// <summary>
        /// Called when the player selects a dialogue choice.
        /// Awards diagnostic points and advances dialogue.
        /// </summary>
        public void OnChoiceSelected(int index)
        {
            if (currentDialogue == null || currentDialogue.choices == null) return;
            if (index < 0 || index >= currentDialogue.choices.Length) return;

            DialogueChoice choice = currentDialogue.choices[index];

            // Award diagnostic points
            if (choice.diagnosticPoints > 0)
            {
                int maxPoints = 0;
                foreach (var c in currentDialogue.choices)
                {
                    if (c.diagnosticPoints > maxPoints) maxPoints = c.diagnosticPoints;
                }
                OnDiagnosticPointsEarned?.Invoke(choice.diagnosticPoints, maxPoints);
                Debug.Log($"[CustomerNPC] Diagnostic points earned: {choice.diagnosticPoints}");
            }

            // Show the NPC response if available
            if (!string.IsNullOrEmpty(choice.responseText))
            {
                dialogueText.text = $"<b>{customerName}:</b>\n{choice.responseText}";
                HideChoiceButtons();

                // Advance to next dialogue after a delay
                if (!string.IsNullOrEmpty(choice.nextDialogueId))
                {
                    StartCoroutine(AdvanceDialogueAfterDelay(choice.nextDialogueId, 3f));
                }
                else
                {
                    // Try advancing to next in sequence
                    StartCoroutine(AdvanceSequenceAfterDelay(3f));
                }
            }
            else
            {
                // Advance immediately
                if (!string.IsNullOrEmpty(choice.nextDialogueId))
                {
                    AdvanceToDialogue(choice.nextDialogueId);
                }
                else
                {
                    AdvanceSequence();
                }
            }

            AudioManager.Instance.PlayButtonClick();
        }

        public void HideDialogue()
        {
            isDialogueActive = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (speechIndicator != null) speechIndicator.SetActive(false);
        }

        // --- Internal ---

        private void ShowDialoguePanel()
        {
            if (currentDialogue == null) return;

            isDialogueActive = true;
            dialoguePanel.SetActive(true);
            speechIndicator.SetActive(true);

            // Set dialogue text
            string speaker = !string.IsNullOrEmpty(currentDialogue.speakerName) ? currentDialogue.speakerName : customerName;
            dialogueText.text = $"<b>{speaker}:</b>\n{currentDialogue.text}";

            // Set up choice buttons
            HideChoiceButtons();
            if (currentDialogue.choices != null)
            {
                for (int i = 0; i < currentDialogue.choices.Length && i < choiceButtonObjects.Count; i++)
                {
                    choiceButtonObjects[i].SetActive(true);
                    TextMeshProUGUI btnText = choiceButtonObjects[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        btnText.text = currentDialogue.choices[i].choiceText;
                    }
                }
            }
        }

        private void HideChoiceButtons()
        {
            foreach (var btnObj in choiceButtonObjects)
            {
                btnObj.SetActive(false);
            }
        }

        private void AdvanceToDialogue(string dialogueId)
        {
            if (dialogueSequence == null) return;

            foreach (var dialogue in dialogueSequence)
            {
                if (dialogue.id == dialogueId)
                {
                    currentDialogue = dialogue;
                    ShowDialoguePanel();
                    return;
                }
            }

            // Dialogue not found, hide
            Debug.LogWarning($"[CustomerNPC] Dialogue '{dialogueId}' not found in sequence.");
            HideDialogue();
        }

        private void AdvanceSequence()
        {
            if (dialogueSequence == null) return;

            currentDialogueIndex++;
            if (currentDialogueIndex < dialogueSequence.Length)
            {
                currentDialogue = dialogueSequence[currentDialogueIndex];
                ShowDialoguePanel();
            }
            else
            {
                HideDialogue();
            }
        }

        private System.Collections.IEnumerator AdvanceDialogueAfterDelay(string dialogueId, float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceToDialogue(dialogueId);
        }

        private System.Collections.IEnumerator AdvanceSequenceAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceSequence();
        }
    }
}
