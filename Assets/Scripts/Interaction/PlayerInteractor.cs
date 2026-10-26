using UnityEngine;

/// <summary>《雾钟》交互入口：只负责检测交互目标和输入，不直接创建或控制 UI。</summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform interactPoint;
    [SerializeField, Min(0.1f)] private float interactRadius = 1.2f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private string keyLabel = "E";

    private IInteractable currentTarget;
    private DialogueManager dialogueManager;
    private WuzhongUIManager uiManager;

    private void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        EnsureUIManager();
        uiManager.SetInteractionPrompt(false);
    }

    private void Update()
    {
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
        EnsureUIManager();

        if (dialogueManager != null && dialogueManager.IsPlaying)
        {
            currentTarget = null;
            uiManager.SetInteractionPrompt(false);
            return;
        }

        FindTarget();
        UpdatePrompt();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
            currentTarget.Interact();
    }

    private void EnsureUIManager()
    {
        if (uiManager != null) return;
        uiManager = WuzhongUIManager.Instance != null
            ? WuzhongUIManager.Instance
            : FindObjectOfType<WuzhongUIManager>();

        if (uiManager == null)
        {
            GameObject uiObject = new GameObject("WuzhongUIManager");
            uiManager = uiObject.AddComponent<WuzhongUIManager>();
        }
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
        if (uiManager == null) return;
        bool canInteract = currentTarget != null;
        string message = canInteract ? $"[{keyLabel}] {currentTarget.InteractionPrompt}" : string.Empty;
        uiManager.SetInteractionPrompt(canInteract, message);
    }

    private void OnDisable()
    {
        if (uiManager != null) uiManager.SetInteractionPrompt(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = interactPoint != null ? interactPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}