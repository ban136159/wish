using TMPro;
using UnityEngine;

/// <summary>《雾钟》通用交互入口：检测最近的可交互对象并响应 E。</summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform interactPoint;
    [SerializeField, Min(0.1f)] private float interactRadius = 1.2f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("Interaction Prompt")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string keyLabel = "E";

    private IInteractable currentTarget;
    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        // 对话期间 E 由 DialogueManager 推进，不再触发场景交互。
        if (dialogueManager != null && dialogueManager.IsPlaying)
        {
            currentTarget = null;
            SetPromptVisible(false);
            return;
        }

        FindTarget();
        UpdatePrompt();
        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
            currentTarget.Interact();
    }

    private void FindTarget()
    {
        currentTarget = null;
        Vector2 center = interactPoint != null ? (Vector2)interactPoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, interactRadius, interactableLayers);
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null) continue;
            float distance = Vector2.Distance(center, hit.ClosestPoint(center));
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                currentTarget = candidate;
            }
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null) return;
        bool canInteract = currentTarget != null;
        SetPromptVisible(canInteract);
        if (canInteract) promptText.text = $"[{keyLabel}] {currentTarget.InteractionPrompt}";
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText != null && promptText.gameObject.activeSelf != visible)
            promptText.gameObject.SetActive(visible);
    }

    private void OnDisable() => SetPromptVisible(false);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = interactPoint != null ? interactPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}
