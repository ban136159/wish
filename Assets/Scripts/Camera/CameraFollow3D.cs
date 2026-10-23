using UnityEngine;

/// <summary>
/// 《雾钟》HD-2D 摄像机跟随。
/// 将此脚本挂到 Main Camera，并把 PlayerRoot 拖入 Target。
/// 适用于角色在 XZ 地面移动、Y 轴代表高度的 3D 场景。
/// </summary>
public class CameraFollow3D : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("镜头位置")]
    [Tooltip("相对于 PlayerRoot 的世界坐标偏移。默认从斜上方、角色后方观察。")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
    [Tooltip("镜头平滑时间，数值越小跟随越紧。")]
    [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
    [Tooltip("开启后摄像机持续朝向目标。")]
    [SerializeField] private bool lookAtTarget = true;
    [Tooltip("摄像机注视目标时的额外高度，避免总是盯着角色脚底。")]
    [SerializeField] private float lookAtHeight = 0.8f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        if (lookAtTarget)
        {
            Vector3 lookPoint = target.position + Vector3.up * lookAtHeight;
            transform.rotation = Quaternion.LookRotation(
                lookPoint - transform.position,
                Vector3.up
            );
        }
    }

    /// <summary>供剧情或重生系统在运行时更换跟随对象。</summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        velocity = Vector3.zero;
    }
}
