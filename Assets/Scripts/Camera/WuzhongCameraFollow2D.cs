using UnityEngine;

/// <summary>
/// 《雾钟》2D 摄像机跟随脚本。
/// 将此脚本挂到 Main Camera，并把 Player 的 Transform 拖到 target。
/// 适用于正交 2D 摄像机；不依赖 Rigidbody2D.linearVelocity。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class WuzhongCameraFollow2D : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("把主角 Player 拖到这里。")]
    [SerializeField] private Transform target;

    [Header("跟随设置")]
    [Tooltip("摄像机相对目标的偏移。2D 常用 Z=-10。")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("启用平滑跟随；关闭后摄像机立即跟随。")]
    [SerializeField] private bool smoothFollow = true;

    [Tooltip("平滑时间越小，跟随越紧。建议 0.08 到 0.2。")]
    [Min(0f)]
    [SerializeField] private float smoothTime = 0.12f;

    [Tooltip("是否只跟随 X 轴。")]
    [SerializeField] private bool followX = true;

    [Tooltip("是否跟随 Y 轴。")]
    [SerializeField] private bool followY = true;

    [Header("可选边界")]
    [Tooltip("启用后限制摄像机中心的位置。边界值为世界坐标。")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

    private Vector3 currentVelocity;

    /// <summary>允许其他脚本在运行时更换跟随目标。</summary>
    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            currentVelocity = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 currentPosition = transform.position;

        if (!followX)
            desiredPosition.x = currentPosition.x;

        if (!followY)
            desiredPosition.y = currentPosition.y;

        desiredPosition.z = offset.z;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        if (smoothFollow && smoothTime > 0f)
        {
            transform.position = Vector3.SmoothDamp(
                currentPosition,
                desiredPosition,
                ref currentVelocity,
                smoothTime
            );
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

    private void OnValidate()
    {
        smoothTime = Mathf.Max(0f, smoothTime);
        if (maxBounds.x < minBounds.x)
            maxBounds.x = minBounds.x;
        if (maxBounds.y < minBounds.y)
            maxBounds.y = minBounds.y;
    }
}
