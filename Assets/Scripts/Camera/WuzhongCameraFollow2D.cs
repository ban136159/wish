using UnityEngine;

/// <summary>
/// 《雾钟》2D 主角摄像机跟随。
/// 适用于 XY 平面上的 SpriteRenderer / Rigidbody2D 主角。
/// 将此脚本挂到 Main Camera，把 Player 拖到 target。
/// </summary>
[DisallowMultipleComponent]
public class WuzhongCameraFollow2D : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("构图")]
    [Tooltip("Z 通常为 -10；X/Y 可微调主角在画面中的位置。")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.8f, -10f);

    [Tooltip("数值越小，跟随越紧；建议 0.12 - 0.25。")]
    [Min(0.01f)]
    [SerializeField] private float smoothTime = 0.18f;

    [Tooltip("主角前方预留视野。设置为 0 可关闭。")]
    [SerializeField] private float lookAheadDistance = 0.6f;

    [Tooltip("速度多大时启用前方视野。")]
    [SerializeField] private float lookAheadSpeedThreshold = 0.1f;

    [Header("边界（可选）")]
    [SerializeField] private bool useBounds;
    [SerializeField] private Vector2 minBounds = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 maxBounds = new Vector2(20f, 20f);

    private Vector3 smoothVelocity;
    private Rigidbody2D targetBody;
    private Vector2 lastMoveDirection = Vector2.down;

    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            CacheTargetBody();
        }
    }

    private void Awake()
    {
        CacheTargetBody();
    }

    private void OnEnable()
    {
        CacheTargetBody();
    }

    private void CacheTargetBody()
    {
        targetBody = target != null ? target.GetComponent<Rigidbody2D>() : null;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 使用 velocity 以兼容较旧版本的 Unity。
        Vector2 velocity = targetBody != null ? targetBody.velocity : Vector2.zero;
        if (velocity.sqrMagnitude > lookAheadSpeedThreshold * lookAheadSpeedThreshold)
            lastMoveDirection = velocity.normalized;

        Vector3 lookAhead = velocity.magnitude >= lookAheadSpeedThreshold
            ? (Vector3)(lastMoveDirection * lookAheadDistance)
            : Vector3.zero;

        Vector3 desired = target.position + offset + lookAhead;
        desired.z = offset.z;

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref smoothVelocity, smoothTime);
    }

    private void OnValidate()
    {
        smoothTime = Mathf.Max(0.01f, smoothTime);
        lookAheadDistance = Mathf.Max(0f, lookAheadDistance);
        lookAheadSpeedThreshold = Mathf.Max(0f, lookAheadSpeedThreshold);
    }
}
