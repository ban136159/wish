using UnityEngine;

/// <summary>
/// 《雾钟》传统 2D 摄像机跟随。
/// 将此脚本挂到 Main Camera，并把 Player 拖入 Target。
/// 适用于角色在 XY 平面移动的 2D 场景。
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;
    [Tooltip("启用后只跟随 X/Y，不改变摄像机 Z 坐标。")]
    [SerializeField] private bool followXYOnly = true;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        if (followXYOnly)
            desiredPosition.z = offset.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        velocity = Vector3.zero;
    }
}
