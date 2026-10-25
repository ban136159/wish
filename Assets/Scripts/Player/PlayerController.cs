using UnityEngine;

/// <summary>
/// 《雾钟》主角控制器。
/// 支持 WASD / 方向键四方向移动、四方向静态角色图片，以及待机呼吸和移动微动。
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

    [Header("角色微动动画")]
    [Tooltip("建议把角色图片放在 Player 的子物体上，再将该子物体拖到这里。留空时会自动寻找子物体 SpriteRenderer。")]
    [SerializeField] private Transform visualRoot;
    [Tooltip("待机呼吸幅度。0.025 代表约 2.5% 的缩放变化。")]
    [SerializeField, Range(0f, 0.08f)] private float idleBreathAmount = 0.025f;
    [Tooltip("待机呼吸速度。")]
    [SerializeField, Min(0f)] private float idleBreathSpeed = 2.2f;
    [Tooltip("移动时角色图片上下微动的高度（Unity 单位）。")]
    [SerializeField, Range(0f, 0.12f)] private float moveBobAmount = 0.035f;
    [Tooltip("移动时上下微动的速度。")]
    [SerializeField, Min(0f)] private float moveBobSpeed = 10f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.down;
    private Vector3 visualStartPosition;
    private Vector3 visualStartScale;
    private float animationTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 优先使用手动指定的视觉子物体；否则尝试寻找子物体上的 SpriteRenderer。
        if (visualRoot == null)
        {
            if (spriteRenderer != null && spriteRenderer.transform != transform)
                visualRoot = spriteRenderer.transform;
            else
            {
                SpriteRenderer childRenderer = GetComponentInChildren<SpriteRenderer>();
                if (childRenderer != null && childRenderer.transform != transform)
                {
                    spriteRenderer = childRenderer;
                    visualRoot = childRenderer.transform;
                }
            }
        }

        // 如果 SpriteRenderer 在子物体上，使用它作为角色外观。
        if (visualRoot != null)
            spriteRenderer = visualRoot.GetComponent<SpriteRenderer>();

        if (visualRoot != null)
        {
            visualStartPosition = visualRoot.localPosition;
            visualStartScale = visualRoot.localScale;
        }
        else if (spriteRenderer != null)
        {
            // 没有视觉子物体时仍可做缩放呼吸，但不移动根物体，避免和 Rigidbody2D 冲突。
            visualStartScale = spriteRenderer.transform.localScale;
        }

        // 俯视角移动不受重力影响，也不允许角色旋转。
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        UpdateFacingSprite();
    }

    private void Update()
    {
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

    private void LateUpdate()
    {
        if (spriteRenderer == null)
            return;

        animationTime += Time.deltaTime;
        bool isMoving = moveInput.sqrMagnitude > 0.001f;

        // 待机时轻微呼吸；移动时降低呼吸幅度，避免角色显得过度弹跳。
        float breath = Mathf.Sin(animationTime * idleBreathSpeed) * idleBreathAmount;
        float scaleFactor = 1f + (isMoving ? breath * 0.35f : breath);

        Transform scaleTarget = visualRoot != null ? visualRoot : spriteRenderer.transform;
        scaleTarget.localScale = new Vector3(
            visualStartScale.x * scaleFactor,
            visualStartScale.y * (1f - breath * (isMoving ? 0.2f : 0.25f)),
            visualStartScale.z
        );

        // 上下微动只作用于视觉子物体，不改变 Rigidbody2D 根物体的位置。
        if (visualRoot != null)
        {
            float bob = isMoving
                ? Mathf.Abs(Mathf.Sin(animationTime * moveBobSpeed)) * moveBobAmount
                : 0f;
            visualRoot.localPosition = visualStartPosition + Vector3.up * bob;
        }
    }

    private void UpdateFacingSprite()
    {
        if (spriteRenderer == null)
            return;

        Sprite targetSprite = null;
        if (lastFacing == Vector2.down) targetSprite = downSprite;
        else if (lastFacing == Vector2.up) targetSprite = upSprite;
        else if (lastFacing == Vector2.left) targetSprite = leftSprite;
        else if (lastFacing == Vector2.right) targetSprite = rightSprite;

        // 未绑定对应方向图片时保留当前图片，单张图片也能先完成测试。
        if (targetSprite != null)
            spriteRenderer.sprite = targetSprite;
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        idleBreathSpeed = Mathf.Max(0f, idleBreathSpeed);
        moveBobSpeed = Mathf.Max(0f, moveBobSpeed);
    }
}
