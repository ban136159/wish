using UnityEngine;

/// <summary>
/// 玩家交互检测器：检测附近最近的可交互物体，显示提示并处理 E 键。
/// 对话期间暂停物体交互，避免同一次按键触发多个系统。
/// </summary>
public class PlayerInteractionDetector : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField, Min(0.1f)] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    private InteractableObject currentTarget;

    private void Update()
    {
        // 对话期间不检测交互目标，也不允许再次触发调查。
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueOpen)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.HideInteractionPrompt();

            return;
        }

        FindNearestInteractable();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
            currentTarget.Interact();
    }

    private void FindNearestInteractable()
    {
        InteractableObject nearest = null;
        float nearestDistance = float.MaxValue;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRadius,
            interactableLayers
        );

        foreach (Collider2D hit in hits)
        {
            InteractableObject candidate = hit.GetComponentInParent<InteractableObject>();

            if (candidate == null || !candidate.isActiveAndEnabled)
                continue;

            float distance = Vector2.Distance(transform.position, candidate.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        if (currentTarget == nearest)
            return;

        currentTarget = nearest;

        if (UIManager.Instance == null)
            return;

        if (currentTarget != null)
            UIManager.Instance.ShowInteractionPrompt(currentTarget.PromptMessage);
        else
            UIManager.Instance.HideInteractionPrompt();
    }

    private void OnDisable()
    {
        currentTarget = null;

        if (UIManager.Instance != null)
            UIManager.Instance.HideInteractionPrompt();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
