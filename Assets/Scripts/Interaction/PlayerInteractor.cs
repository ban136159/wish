using UnityEngine;

/// <summary>
/// 检测玩家附近可交互的对象，并在按下 E 时与最近的对象交互。
/// 需要在 Inspector 中配置可交互 Layer，避免检测到无关碰撞体。
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Transform interactPoint; // 检测中心；未设置时使用玩家位置
    [SerializeField, Min(0.1f)] private float interactRadius = 1.2f; // 检测半径
    [SerializeField] private LayerMask interactableLayers; // 可交互对象所在层

    private IInteractable currentTarget; // 当前检测到的最近目标

    private void Update()
    {
        // 每帧刷新目标，确保玩家移动后交互对象也随之更新。
        FindTarget();

        // 仅在按下 E 的这一帧执行交互。
        if (currentTarget != null && Input.GetKeyDown(KeyCode.E)) currentTarget.Interact();
    }

    private void FindTarget()
    {
        currentTarget = null;

        // 优先使用单独设置的检测点，否则以玩家自身位置作为中心。
        Vector2 center = interactPoint != null ? interactPoint.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, interactRadius, interactableLayers);
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            // 接口组件可能位于碰撞体的父物体上，因此向父级查找。
            IInteractable candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null) continue;

            float distance = Vector2.Distance(center, hit.transform.position);
            // 只保留距离检测中心最近的有效对象。
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                currentTarget = candidate;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 选中玩家时在 Scene 视图画出检测范围，便于调试。
        Gizmos.color = Color.yellow;
        Vector3 center = interactPoint != null ? interactPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}
