using UnityEngine;

/// <summary>
/// 2D 俯视角玩家移动控制器。
/// 通过 Rigidbody2D 移动角色，避免直接修改 Transform 导致的物理检测问题。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // Inspector 中可调整的移动速度，限制为非负数。
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    private Rigidbody2D rb;       // 玩家刚体组件
    private Vector2 moveInput;    // 每帧读取的移动方向

    private void Awake()
    {
        // 缓存刚体引用，并关闭重力与旋转，适用于俯视角移动。
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Update 中读取输入；normalized 可避免斜向移动速度更快。
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void FixedUpdate()
    {
        // Unity 6 使用 linearVelocity，较早版本使用 velocity。
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveInput * moveSpeed;
#else
        rb.velocity = moveInput * moveSpeed;
#endif
    }
}
