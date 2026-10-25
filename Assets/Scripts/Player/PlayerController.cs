using UnityEngine;

/// <summary>
/// 《雾钟》主角控制器：四方向移动、方向图片切换、待机呼吸、移动上下微动和脚底碰撞箱。
/// 角色图片直接放在 Player 根物体上也可以；脚本会自动创建 Visual 子物体来承载图片动画。
/// 适用于 Unity 2022.3 及更新版本。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    [Header("脚底碰撞箱")]
    [Tooltip("碰撞箱宽度占角色图片宽度的比例。")]
    [SerializeField, Range(0.1f, 1f)] private float colliderWidthRatio = 0.45f;
    [Tooltip("碰撞箱高度占角色图片高度的比例。")]
    [SerializeField, Range(0.05f, 0.5f)] private float colliderHeightRatio = 0.18f;
    [Tooltip("脚底碰撞箱向上或向下微调。正数向上，负数向下。")]
    [SerializeField] private float colliderOffsetY = 0f;

    [Header("四方向角色图片（可选）")]
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("待机呼吸")]
    [Tooltip("待机时轻微缩放幅度。设为 0 可关闭。")]
    [SerializeField, Range(0f, 0.08f)] private float idleBreathAmount = 0.025f;
    [SerializeField, Min(0f)] private float idleBreathSpeed = 2.2f;

    [Header("移动上下微动")]
    [Tooltip("每次上下摆动的高度（Unity 单位）。建议 0.04 到 0.10。设为 0 可关闭。")]
    [SerializeField, Range(0f, 0.2f)] private float moveBobAmount = 0.08f;
    [Tooltip("上下摆动速度。数值越大摆动越快。")]
    [SerializeField, Min(0f)] private float moveBobSpeed = 10f;

    private Rigidbody2D rb;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    private Transform visualRoot;
    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.down;
    private Vector3 visualStartPosition;
    private Vector3 visualStartScale;
    private float animationTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 先查找现有的视觉子物体。
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.transform == transform)
        {
            // 图片原本就在 Player 根物体上：自动建立 Visual 子物体，
            // 将 SpriteRenderer 的显示属性复制过去，根物体仍只负责物理移动。
            SpriteRenderer source = spriteRenderer;
            GameObject visualObject = new GameObject("Visual");
            visualRoot = visualObject.transform;
            visualRoot.SetParent(transform, false);
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;

            SpriteRenderer target = visualObject.AddComponent<SpriteRenderer>();
            target.sprite = source.sprite;
            target.color = source.color;
            target.sharedMaterial = source.sharedMaterial;
            target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder;
            target.flipX = source.flipX;
            target.flipY = source.flipY;
            target.drawMode = source.drawMode;
            target.maskInteraction = source.maskInteraction;
            target.enabled = source.enabled;
            target.spriteSortPoint = source.spriteSortPoint;
            source.enabled = false;
            spriteRenderer = target;
        }
        else if (spriteRenderer != null)
        {
            visualRoot = spriteRenderer.transform;
        }

        ConfigureFootCollider();

        if (visualRoot != null)
        {
            visualStartPosition = visualRoot.localPosition;
            visualStartScale = visualRoot.localScale;
        }
        else
        {
            Debug.LogWarning("PlayerController：找不到 SpriteRenderer，请给 Player 添加角色图片。", this);
        }

        UpdateFacingSprite();
    }

    /// <summary>
    /// 将碰撞箱缩小到角色脚底附近。碰撞箱放在 Player 根物体上，
    /// 因此不会跟随 Visual 的呼吸缩放和上下微动一起抖动。
    /// </summary>
    private void ConfigureFootCollider()
    {
        bodyCollider.isTrigger = false;

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            float width = Mathf.Max(0.05f, spriteBounds.size.x * colliderWidthRatio);
            float height = Mathf.Max(0.05f, spriteBounds.size.y * colliderHeightRatio);

            bodyCollider.size = new Vector2(width, height);
            // Sprite bounds 的底边位置，再加上视觉子物体的本地偏移。
            bodyCollider.offset = new Vector2(
                0f,
                spriteBounds.min.y + height * 0.5f +
                (visualRoot != null ? visualRoot.localPosition.y : 0f) +
                colliderOffsetY
            );
        }
        else
        {
            // 找不到图片时使用一个小型脚底碰撞箱作为后备值。
            bodyCollider.size = new Vector2(0.35f, 0.18f);
            bodyCollider.offset = new Vector2(0f, -0.4f + colliderOffsetY);
        }
    }

    private void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

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
        if (visualRoot == null || spriteRenderer == null)
            return;

        // 使用脚底碰撞箱的最低点进行 Y 轴排序：脚底越靠下，角色越靠前。
        // 房屋根物体上的 WuzhongYSort 也必须使用相同的 Sorting Layer。
        if (bodyCollider != null)
            spriteRenderer.sortingOrder = -Mathf.RoundToInt(bodyCollider.bounds.min.y * 100f);

        animationTime += Time.deltaTime;
        bool isMoving = moveInput.sqrMagnitude > 0.001f;

        // 待机呼吸：轻微缩放。移动时仍保留少量呼吸效果。
        float breath = Mathf.Sin(animationTime * idleBreathSpeed) * idleBreathAmount;
        float scaleX = 1f + breath * (isMoving ? 0.35f : 1f);
        float scaleY = 1f - breath * 0.25f;
        visualRoot.localScale = new Vector3(
            visualStartScale.x * scaleX,
            visualStartScale.y * scaleY,
            visualStartScale.z
        );

        // 移动时上下弹动；停止时准确回到 Visual 的初始位置。
        float bob = isMoving
            ? Mathf.Abs(Mathf.Sin(animationTime * moveBobSpeed)) * moveBobAmount
            : 0f;
        visualRoot.localPosition = visualStartPosition + Vector3.up * bob;
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

        if (targetSprite != null)
            spriteRenderer.sprite = targetSprite;
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        idleBreathSpeed = Mathf.Max(0f, idleBreathSpeed);
        moveBobSpeed = Mathf.Max(0f, moveBobSpeed);
        colliderWidthRatio = Mathf.Clamp(colliderWidthRatio, 0.1f, 1f);
        colliderHeightRatio = Mathf.Clamp(colliderHeightRatio, 0.05f, 0.5f);
    }
}
