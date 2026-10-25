using UnityEngine;

/// <summary>
/// 《雾钟》主角移动控制器。
/// 支持四方向贴图切换，并通过 Rigidbody2D 插值减少移动时的视觉抖动。
/// 注意：四张方向贴图应使用一致的 Sprite Pivot（建议 Bottom Center），
/// 否则切换贴图时角色外观可能上下跳动。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.0f;
    [Tooltip("输入方向平滑时间。设为 0 可立即响应；数值越大起步/停步越柔和。")]
    [SerializeField, Min(0f)] private float inputSmoothTime = 0.04f;

    [Header("四方向角色贴图")]
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;
    private Vector2 lastFacing = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 让 Rigidbody2D 在渲染帧之间插值，减少高速移动时的视觉抖动。
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        ApplyFacingSprite();
    }

    private void Update()
    {
        Vector2 rawInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // 归一化，避免斜向移动速度更快。
        if (rawInput.sqrMagnitude > 1f)
            rawInput.Normalize();

        if (inputSmoothTime > 0f)
            smoothedInput = Vector2.SmoothDamp(
                smoothedInput, rawInput, ref inputVelocity, inputSmoothTime);
        else
            smoothedInput = rawInput;

        moveInput = smoothedInput;

        // 使用原始输入判断朝向，避免平滑过程中方向延迟或反复切换。
        if (rawInput.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
                lastFacing = rawInput.x < 0f ? Vector2.left : Vector2.right;
            else
                lastFacing = rawInput.y > 0f ? Vector2.up : Vector2.down;

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

        if (nextSprite != null)
            spriteRenderer.sprite = nextSprite;
    }
}
