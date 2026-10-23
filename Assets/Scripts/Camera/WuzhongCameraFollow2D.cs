using UnityEngine;

/// <summary>
/// 《雾钟》2D 摄像机跟随。
/// 适用于 XY 平面的 SpriteRenderer / Rigidbody2D 主角。
/// </summary>
[DisallowMultipleComponent]
public class WuzhongCameraFollow2D : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("跟随设置")]
    [Tooltip("先设为 (0, 0, -10)，确认主角可见后再微调。")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("勾选后摄像机直接跟随，排查位置问题时建议开启。")]
    [SerializeField] private bool snapToTarget = true;

    [Min(0.01f)]
    [SerializeField] private float smoothTime = 0.12f;

    [Header("可选：移动方向预留视野")]
    [SerializeField] private bool enableLookAhead;
    [SerializeField] private float lookAheadDistance = 0.4f;
    [SerializeField] private float lookAheadSpeedThreshold = 0.1f;

    [Header("可选：地图边界")]
    [SerializeField] private bool useBounds;
    [SerializeField] private Vector2 minBounds = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 maxBounds = new Vector2(20f, 20f);

    private Vector3 smoothVelocity;
    private Rigidbody2D targetBody;
    private Vector2 lastMoveDirection = Vector2.zero;

    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            CacheTargetBody();
            SnapNow();
        }
    }

    private void Awake()
    {
        CacheTargetBody();
        SnapNow();
    }

    private void OnEnable()
    {
        CacheTargetBody();
        SnapNow();
    }

    private void CacheTargetBody()
    {
        targetBody = target != null ? target.GetComponent<Rigidbody2D>() : null;
    }

    private void SnapNow()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        desired.z = offset.z;
        transform.position = desired;
        smoothVelocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;

        if (enableLookAhead)
        {
            Vector2 velocity = targetBody != null ? targetBody.velocity : Vector2.zero;
            if (velocity.sqrMagnitude > lookAheadSpeedThreshold * lookAheadSpeedThreshold)
                lastMoveDirection = velocity.normalized;

            if (velocity.magnitude >= lookAheadSpeedThreshold)
                desired += (Vector3)(lastMoveDirection * lookAheadDistance);
        }

        // 明确保持摄像机位于主角后方，避免 Z 值意外导致主角被裁切。
        desired.z = target.position.z + offset.z;

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }

        if (snapToTarget)
        {
            transform.position = desired;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref smoothVelocity, smoothTime);
        }
    }

    private void OnValidate()
    {
        smoothTime = Mathf.Max(0.01f, smoothTime);
        lookAheadDistance = Mathf.Max(0f, lookAheadDistance);
        lookAheadSpeedThreshold = Mathf.Max(0f, lookAheadSpeedThreshold);
    }
}
