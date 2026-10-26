using TMPro;
using UnityEngine;

/// <summary>
/// Detects nearby interactable objects, displays the interaction prompt,
/// and triggers the selected interaction when the player presses E.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform interactPoint;
    [SerializeField, Min(0.1f)] private float interactRadius = 1.2f;
    [SerializeField] private LayerMask interactableLayers;

    [Header("Interaction Prompt")]
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

    /// <summary>
    /// Finds the nearest interactable collider inside the configured radius.
    /// Interactable colliders must be on a layer included in Interactable Layers.
    /// </summary>
    private void FindTarget()
    {
        currentTarget = null;

        Vector2 center = interactPoint != null
            ? (Vector2)interactPoint.position
            : (Vector2)transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            center,
            interactRadius,
            interactableLayers
        );

        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null)
            {
                continue;
            }

            float distance = Vector2.Distance(center, hit.ClosestPoint(center));
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                currentTarget = candidate;
            }
        }
    }

    /// <summary>
    /// Shows a prompt for the current target, or hides it when no target is nearby.
    /// </summary>
    private void UpdatePrompt()
    {
        if (promptText == null)
        {
            return;
        }

        bool canInteract = currentTarget != null;
        SetPromptVisible(canInteract);

        if (canInteract)
        {
            promptText.text = $"[{keyLabel}] {currentTarget.InteractionPrompt}";
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(visible);
        }
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
