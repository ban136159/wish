using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Transform interactPoint;
    [SerializeField, Min(0.1f)] private float interactRadius = 1.2f;
    [SerializeField] private LayerMask interactableLayers;
    private IInteractable currentTarget;
    private void Update() { FindTarget(); if (currentTarget != null && Input.GetKeyDown(KeyCode.E)) currentTarget.Interact(); }
    private void FindTarget()
    {
        currentTarget = null;
        Vector2 center = interactPoint != null ? interactPoint.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, interactRadius, interactableLayers);
        float nearestDistance = float.MaxValue;
        foreach (Collider2D hit in hits)
        {
            IInteractable candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null) continue;
            float distance = Vector2.Distance(center, hit.transform.position);
            if (distance < nearestDistance) { nearestDistance = distance; currentTarget = candidate; }
        }
    }
    private void OnDrawGizmosSelected() { Gizmos.color = Color.yellow; Vector3 center = interactPoint != null ? interactPoint.position : transform.position; Gizmos.DrawWireSphere(center, interactRadius); }
}
