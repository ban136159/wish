using UnityEngine;

/// <summary>
/// 《雾钟》场景物体的 Y 轴遮挡排序。
/// 将此组件添加到完整房屋 PNG 的根物体上；排序基准应是房屋落地处，而不是图片中心。
/// 角色和房屋必须使用相同的 Sorting Layer，才能根据前后位置自动互相遮挡。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class WuzhongYSort : MonoBehaviour
{
    [Tooltip("排序基准相对物体 Transform 的 Y 偏移。完整房屋 PNG 通常填写图片底边相对中心的负值。")]
    [SerializeField] private float footOffsetY = -1f;

    [Tooltip("每个 Unity 单位转换成的排序刻度。通常保持 100。")]
    [SerializeField, Min(1)] private int orderScale = 100;

    [Tooltip("可选：手动指定 SpriteRenderer。不指定时自动查找本物体上的 Renderer。")]
    [SerializeField] private SpriteRenderer targetRenderer;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (targetRenderer == null)
            return;

        float sortY = transform.position.y + footOffsetY;
        targetRenderer.sortingOrder = -Mathf.RoundToInt(sortY * orderScale);
    }

    private void OnValidate()
    {
        orderScale = Mathf.Max(1, orderScale);
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }
}
