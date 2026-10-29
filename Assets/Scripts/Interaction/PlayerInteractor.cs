using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 《雾钟》交互系统：检测主角附近的可交互物体，并发布目标变化/交互事件。
/// </summary>
[DisallowMultipleComponent]
public class PlayerInteractor : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private Transform interactPoint;
    [SerializeField, Min(0.1f)] private float interactRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("提示界面")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Image backgroundImage;

    [Header("字体设置")]
    [Tooltip("将中文像素字体生成的 TMP Font Asset 拖到这里。")]
    [SerializeField] private TMP_FontAsset pixelFont;
    [SerializeField, Min(8f)] private float fontSize = 24f;
    [SerializeField] private Color fontColor = new Color(1f, 0.93f, 0.80f, 1f);
    [SerializeField] private string keyLabel = "E";
    [SerializeField] private Color keyColor = new Color(0.85f, 0.70f, 0.41f, 1f);

    [Header("背景设置")]
    [Tooltip("Alpha 控制背景不透明度；0 为透明，1 为完全不透明。")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.78f);

    [Header("头顶位置")]
    [SerializeField] private Vector3 promptLocalOffset = new Vector3(0f, 1.5f, 0f);

    private IInteractable currentTarget;

    public void ConfigurePrompt(Canvas canvas, TMP_Text text, Image background)
    {
        promptCanvas = canvas;
        promptText = text;
        backgroundImage = background;
        ApplyStyle();
        SetPromptVisible(false);
    }

    private void Awake()
    {
        ApplyStyle();
        SetPromptVisible(false);
    }

    private void LateUpdate()
    {
        if (promptCanvas != null && promptCanvas.transform.parent == transform)
            promptCanvas.transform.localPosition = promptLocalOffset;
    }

    private void Update()
    {
        // 对话期间由 DialogueManager 接管继续键，不再检测交互。
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueOpen)
        {
            SetPromptVisible(false);
            return;
        }

        FindTarget();
        UpdatePrompt();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            WuzhongEvents.RaiseInteractionStarted(GetTargetId(currentTarget));
            currentTarget.Interact();
        }
    }

    private void FindTarget()
    {
        IInteractable previousTarget = currentTarget;
        currentTarget = null;

        Vector2 center = interactPoint != null
            ? (Vector2)interactPoint.position
            : (Vector2)transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, interactRadius, interactableLayers);
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null)
                continue;

            float distance = Vector2.Distance(center, hit.ClosestPoint(center));
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                currentTarget = candidate;
            }
        }

        if (!ReferenceEquals(previousTarget, currentTarget))
        {
            WuzhongEvents.RaiseInteractionTargetChanged(
                currentTarget == null ? null : GetTargetId(currentTarget));
        }
    }

    private static string GetTargetId(IInteractable target)
    {
        if (target is Component component)
            return component.gameObject.name;

        return target == null ? string.Empty : target.GetType().Name;
    }

    private void UpdatePrompt()
    {
        if (promptCanvas == null || promptText == null)
            return;

        bool canInteract = currentTarget != null;
        SetPromptVisible(canInteract);

        if (canInteract)
        {
            promptText.text =
                $"<color=#{ColorUtility.ToHtmlStringRGB(keyColor)}>[{keyLabel}]</color> " +
                $"{currentTarget.InteractionPrompt}";
        }
    }

    private void ApplyStyle()
    {
        if (promptText != null)
        {
            if (pixelFont != null)
                promptText.font = pixelFont;

            promptText.fontSize = fontSize;
            promptText.color = fontColor;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.enableWordWrapping = false;
            promptText.raycastTarget = false;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            backgroundImage.raycastTarget = false;
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptCanvas != null && promptCanvas.gameObject.activeSelf != visible)
            promptCanvas.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        SetPromptVisible(false);
        if (currentTarget != null)
        {
            currentTarget = null;
            WuzhongEvents.RaiseInteractionTargetChanged(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = interactPoint != null ? interactPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}
