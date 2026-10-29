using UnityEngine;

/// <summary>
/// 《雾钟》主角控制器：八方向移动、八方向独立图片、待机呼吸、移动微动和 Y 轴显示排序。
/// 根物体负责 Rigidbody2D 移动；SpriteRenderer 可放在根物体或 Visual 子物体上。
/// 不会自动创建或修改碰撞箱。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    [Header("八方向角色图片（可选）")]
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite downLeftSprite;
    [SerializeField] private Sprite downRightSprite;
    [SerializeField] private Sprite upLeftSprite;
    [SerializeField] private Sprite upRightSprite;

    [Header("待机呼吸")]
    [Tooltip("待机时轻微缩放幅度。设为 0 可关闭。")]
    [SerializeField, Range(0f, 0.08f)] private float idleBreathAmount = 0.025f;
    [SerializeField, Min(0f)] private float idleBreathSpeed = 2.2f;

    [Header("移动上下微动")]
    [Tooltip("每次上下摆动的高度（Unity 单位）。设为 0 可关闭。")]
    [SerializeField, Range(0f, 0.2f)] private float moveBobAmount = 0.08f;
    [Tooltip("上下摆动速度。")]
    [SerializeField, Min(0f)] private float moveBobSpeed = 10f;

    private Rigidbody2D rb;
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
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.transform == transform)
        {
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

    private void Update()
    {
        // 支持旧 Input Manager 的方向键与 WASD。
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (moveInput.sqrMagnitude > 0.001f)
        {
            // 保留斜向信息，供八方向图片选择使用。
            lastFacing = moveInput;
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

        // 按角色根物体世界 Y 坐标排序，适用于俯视场景。
        spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);

        animationTime += Time.deltaTime;
        bool isMoving = moveInput.sqrMagnitude > 0.001f;

        float breath = Mathf.Sin(animationTime * idleBreathSpeed) * idleBreathAmount;
        float scaleX = 1f + breath * (isMoving ? 0.35f : 1f);
        float scaleY = 1f - breath * 0.25f;
        visualRoot.localScale = new Vector3(
            visualStartScale.x * scaleX,
            visualStartScale.y * scaleY,
            visualStartScale.z
        );

        float bob = isMoving
            ? Mathf.Abs(Mathf.Sin(animationTime * moveBobSpeed)) * moveBobAmount
            : 0f;
        visualRoot.localPosition = visualStartPosition + Vector3.up * bob;
    }

    private void UpdateFacingSprite()
    {
        if (spriteRenderer == null)
            return;

        Sprite targetSprite;

        // 当 X、Y 都有输入时，使用对应的独立斜向图片。
        if (Mathf.Abs(lastFacing.x) > 0.1f && Mathf.Abs(lastFacing.y) > 0.1f)
        {
            if (lastFacing.x < 0f)
                targetSprite = lastFacing.y > 0f ? upLeftSprite : downLeftSprite;
            else
                targetSprite = lastFacing.y > 0f ? upRightSprite : downRightSprite;
        }
        else if (Mathf.Abs(lastFacing.y) > 0.1f)
        {
            targetSprite = lastFacing.y > 0f ? upSprite : downSprite;
        }
        else
        {
            targetSprite = lastFacing.x < 0f ? leftSprite : rightSprite;
        }

        // 对应方向未配置图片时，保留上一张有效图片，避免角色变空白。
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
