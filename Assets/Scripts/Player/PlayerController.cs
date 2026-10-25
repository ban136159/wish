using UnityEngine;

/// <summary>
/// 《雾钟》主角控制器。
/// 支持 WASD / 方向键四方向移动，并可选绑定四方向静态角色图片。
/// 适用于 Unity 2022.3 的 Rigidbody2D。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    [Header("四方向角色图片（可选）")]
    [Tooltip("角色朝下、面向镜头时使用的图片。")]
    [SerializeField] private Sprite downSprite;

    [Tooltip("角色朝上、背对镜头时使用的图片。")]
    [SerializeField] private Sprite upSprite;

    [Tooltip("角色朝左时使用的图片。")]
    [SerializeField] private Sprite leftSprite;

    [Tooltip("角色朝右时使用的图片。")]
    [SerializeField] private Sprite rightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 俯视角移动不受重力影响，也不允许角色旋转。
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 启动时显示默认朝向图片（如果已经绑定）。
        UpdateFacingSprite();
    }

    private void Update()
    {
        // 兼容 Unity 2022.3 常见的 Input Manager 输入设置。
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(horizontal, vertical).normalized;

        // 仅在有输入时更新朝向；松开按键后保持最后朝向。
        if (moveInput.sqrMagnitude > 0.001f)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                lastFacing = moveInput.x < 0f ? Vector2.left : Vector2.right;
            else
                lastFacing = moveInput.y < 0f ? Vector2.down : Vector2.up;

            UpdateFacingSprite();
        }
    }

    private void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveInput * moveSpeed;
#else
        rb.velocity = moveInput * moveSpeed;
#endif
    }

    private void UpdateFacingSprite()
    {
        if (spriteRenderer == null)
            return;

        Sprite targetSprite = null;

        if (lastFacing == Vector2.down)
            targetSprite = downSprite;
        else if (lastFacing == Vector2.up)
            targetSprite = upSprite;
        else if (lastFacing == Vector2.left)
            targetSprite = leftSprite;
        else if (lastFacing == Vector2.right)
            targetSprite = rightSprite;

        // 未绑定对应方向图片时保留 SpriteRenderer 当前图片，
        // 因此只有一张角色图时也能先完成移动测试。
        if (targetSprite != null)
            spriteRenderer.sprite = targetSprite;
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
    }
}
