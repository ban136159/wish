using UnityEngine;

/// <summary>
/// 《雾钟》主角移动控制器。
/// 支持上下左右四方向 Sprite 切换，并保留 Rigidbody2D 移动方式。
/// 在 Inspector 中分别指定 Front/Back/Left/Right Sprite。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    [Header("四方向角色贴图")]
    [Tooltip("主角面向下方时使用的贴图。")]
    [SerializeField] private Sprite frontSprite;
    [Tooltip("主角面向上方时使用的贴图。")]
    [SerializeField] private Sprite backSprite;
    [Tooltip("主角面向左方时使用的贴图。")]
    [SerializeField] private Sprite leftSprite;
    [Tooltip("主角面向右方时使用的贴图。")]
    [SerializeField] private Sprite rightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 如果 SpriteRenderer 在子物体上，也尝试查找。
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        ApplyFacingSprite();
    }

    private void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 仅在有输入时更新朝向；停止移动时保留最后的朝向。
        if (moveInput.sqrMagnitude > 0.001f)
        {
            // 对角线输入时选择幅度较大的轴，避免朝向频繁跳变。
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                lastFacing = moveInput.x < 0f ? Vector2.left : Vector2.right;
            else
                lastFacing = moveInput.y > 0f ? Vector2.up : Vector2.down;

            ApplyFacingSprite();
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

    private void ApplyFacingSprite()
    {
        if (spriteRenderer == null)
            return;

        Sprite nextSprite = null;

        if (lastFacing == Vector2.up)
            nextSprite = backSprite;
        else if (lastFacing == Vector2.down)
            nextSprite = frontSprite;
        else if (lastFacing == Vector2.left)
            nextSprite = leftSprite;
        else if (lastFacing == Vector2.right)
            nextSprite = rightSprite;

        // 未设置某个方向的 Sprite 时保留当前贴图，不把角色变成空白。
        if (nextSprite != null)
            spriteRenderer.sprite = nextSprite;
    }
}
