using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>《雾钟》统一 UI 层：集中管理交互提示、对话和调查界面。</summary>
[DefaultExecutionOrder(-100)]
public class WuzhongUIManager : MonoBehaviour
{
    public static WuzhongUIManager Instance { get; private set; }

    [Header("UI Layout")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    [SerializeField] private float promptBottomOffset = 90f;

    private GameObject promptPanel;
    private TMP_Text promptText;
    private GameObject dialoguePanel;
    private TMP_Text speakerText;
    private TMP_Text dialogueText;
    private GameObject investigationPanel;
    private TMP_Text investigationText;
    private DialogueManager dialogueManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUI();
    }

    private void Start() => BindDialogueManager();

    private void OnEnable() => BindDialogueManager();

    private void OnDisable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.LineChanged -= OnDialogueLineChanged;
            dialogueManager.DialogueEnded -= OnDialogueEnded;
        }
    }

    private void Update()
    {
        if (dialogueManager == null)
            BindDialogueManager();

        if (investigationPanel != null && investigationPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            HideInvestigation();
    }

    private void BindDialogueManager()
    {
        DialogueManager found = FindObjectOfType<DialogueManager>();
        if (found == dialogueManager) return;

        if (dialogueManager != null)
        {
            dialogueManager.LineChanged -= OnDialogueLineChanged;
            dialogueManager.DialogueEnded -= OnDialogueEnded;
        }

        dialogueManager = found;
        if (dialogueManager != null)
        {
            dialogueManager.LineChanged += OnDialogueLineChanged;
            dialogueManager.DialogueEnded += OnDialogueEnded;
        }
    }

    private void BuildUI()
    {
        GameObject canvasObject = new GameObject("WuzhongUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        promptPanel = CreatePanel("InteractionPromptPanel", canvasObject.transform, new Color(0.04f, 0.07f, 0.08f, 0.88f));
        SetAnchors(promptPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, promptBottomOffset), new Vector2(760f, 84f));
        promptText = CreateText("PromptText", promptPanel.transform, 30, TextAlignmentOptions.Center);
        Stretch(promptText.rectTransform, 24f, 8f, 24f, 8f);
        promptPanel.SetActive(false);

        dialoguePanel = CreatePanel("DialoguePanel", canvasObject.transform, new Color(0.025f, 0.035f, 0.045f, 0.96f));
        SetAnchors(dialoguePanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(1500f, 260f));
        speakerText = CreateText("SpeakerText", dialoguePanel.transform, 30, TextAlignmentOptions.Left);
        SetAnchors(speakerText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(-72f, 52f));
        dialogueText = CreateText("DialogueText", dialoguePanel.transform, 27, TextAlignmentOptions.TopLeft);
        SetAnchors(dialogueText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-72f, -88f));
        dialogueText.enableWordWrapping = true;
        dialoguePanel.SetActive(false);

        investigationPanel = CreatePanel("InvestigationPanel", canvasObject.transform, new Color(0.025f, 0.035f, 0.045f, 0.94f));
        SetAnchors(investigationPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100f, 340f));
        investigationText = CreateText("InvestigationText", investigationPanel.transform, 30, TextAlignmentOptions.TopLeft);
        Stretch(investigationText.rectTransform, 48f, 48f, 48f, 48f);
        investigationText.enableWordWrapping = true;
        investigationPanel.SetActive(false);
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return panel;
    }

    private TMP_Text CreateText(string name, Transform parent, float size, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.color = new Color(1f, 0.92f, 0.78f, 1f);
        text.alignment = alignment;
        text.raycastTarget = false;
        text.text = string.Empty;
        return text;
    }

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    public void SetInteractionPrompt(bool visible, string message = "")
    {
        if (promptPanel == null) return;
        bool anotherPanelOpen = (dialoguePanel != null && dialoguePanel.activeSelf) ||
                                (investigationPanel != null && investigationPanel.activeSelf);
        bool shouldShow = visible && !anotherPanelOpen;
        promptPanel.SetActive(shouldShow);
        if (shouldShow && promptText != null) promptText.text = message;
    }

    public void ShowInvestigation(string message)
    {
        if (investigationPanel == null) return;
        investigationText.text = message;
        investigationPanel.SetActive(true);
        SetInteractionPrompt(false);
    }

    public void HideInvestigation()
    {
        if (investigationPanel != null) investigationPanel.SetActive(false);
    }

    private void OnDialogueLineChanged(DialogueLine line)
    {
        if (line == null) return;
        speakerText.text = line.speakerName;
        dialogueText.text = line.text;
        dialoguePanel.SetActive(true);
        SetInteractionPrompt(false);
        HideInvestigation();
    }

    private void OnDialogueEnded()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}