using TMPro;
using UnityEngine;

/// <summary>
/// 检测主角附近的可交互物体，显示提示，并在按下 E 时触发交互。
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private Transform interactPoint;
    [SerializeField, Min(0.1f)] private float interactRadius = 1.2f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("交互提示")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string keyLabel = "E";

    private IInteractable currentTarget;

    private void Awake()
    {
        SetPromptVisible(false);
    }

    private void Update()
    {
        FindTarget();
        UpdatePrompt();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            currentTarget.Interact();
        }
    }

    private void FindTarget()
    {
        currentTarget = null;

        Vector2 center = interactPoint != null
            ? (Vector2)interactPoint.position
            : (Vector2)transform.position;

        Collider2D[] hits = interactableLayers.value == 0
            ? Physics2D.OverlapCircleAll(center, interactRadius)
            : Physics2D.OverlapCircleAll(center, interactRadius, interactableLayers);

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
    }

    private void UpdatePrompt()
    {
        if (promptText == null)
            return;

        bool canInteract = currentTarget != null;
        SetPromptVisible(canInteract);

        if (canInteract)
            promptText.text = $"[{keyLabel}] {currentTarget.InteractionPrompt}";
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        SetPromptVisible(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = interactPoint != null
            ? interactPoint.position
            : transform.position;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}
