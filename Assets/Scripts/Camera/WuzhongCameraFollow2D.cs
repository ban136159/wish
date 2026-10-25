using UnityEngine;

/// <summary>
/// 《雾钟》2D 摄像机平滑跟随。
/// 将此脚本挂到 Main Camera，并在 Inspector 中把 Player 拖到 Target。
/// 适用于 Unity 2022.3 LTS。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class WuzhongCameraFollow2D : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("把 Hierarchy 中的 Player 拖到这里。")]
    [SerializeField] private Transform target;

    [Header("跟随设置")]
    [Tooltip("摄像机相对目标的位置偏移。2D 摄像机通常 Z 为 -10。")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("平滑时间越小，摄像机跟随越灵敏。建议 0.12 到 0.25。")]
    [SerializeField, Min(0f)] private float smoothTime = 0.18f;

    [Tooltip("0 表示不限制；大于 0 时限制摄像机最大跟随速度。")]
    [SerializeField, Min(0f)] private float maxSpeed = 0f;

    [Header("可选：地图边界")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-40f, -30f);
    [SerializeField] private Vector2 maxBounds = new Vector2(40f, 30f);

    private Vector3 currentVelocity;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target == null)
            Debug.LogWarning("WuzhongCameraFollow2D：请在 Inspector 的 Target 中指定 Player。", this);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds && cam != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float minX = minBounds.x + halfWidth;
            float maxX = maxBounds.x - halfWidth;
            float minY = minBounds.y + halfHeight;
            float maxY = maxBounds.y - halfHeight;

            // 如果地图比摄像机视野还小，则固定在该轴的地图中心。
            desiredPosition.x = minX > maxX
                ? (minBounds.x + maxBounds.x) * 0.5f
                : Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = minY > maxY
                ? (minBounds.y + maxBounds.y) * 0.5f
                : Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        desiredPosition.z = offset.z;
        transform.position = smoothTime <= 0f
            ? desiredPosition
            : Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime,
                maxSpeed <= 0f ? Mathf.Infinity : maxSpeed);
    }

    private void OnValidate()
    {
        smoothTime = Mathf.Max(0f, smoothTime);
        maxSpeed = Mathf.Max(0f, maxSpeed);
    }
}