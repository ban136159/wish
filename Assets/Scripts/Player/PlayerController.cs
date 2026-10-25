using UnityEngine;

/// <summary>
/// 《雾钟》主角移动控制器。
/// 支持四方向贴图切换，并在移动时对 SpriteRenderer 做轻微上下跳动。
/// 跳动只作用于 SpriteRenderer，不移动 Rigidbody2D，避免影响碰撞与摄像机。
/// 四张方向贴图建议使用一致的 Pivot（通常是 Bottom Center）。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.0f;
    [Tooltip("输入方向平滑时间。设为 0 可立即响应。")]
    [SerializeField, Min(0f)] private float inputSmoothTime = 0.04f;

    [Header("移动跳动效果")]
    [Tooltip("移动时启用轻微上下跳动。")]
    [SerializeField] private bool enableWalkBob = true;
    [Tooltip("跳动幅度（世界单位）。建议 0.015 到 0.04，越小越轻微。")]
    [SerializeField, Range(0f, 0.1f)] private float bobAmplitude = 0.025f;
    [Tooltip("每秒跳动频率。建议 5 到 8。")]
    [SerializeField, Range(1f, 15f)] private float bobFrequency = 6f;
    [Tooltip("开始和停止移动时跳动淡入/淡出的速度。")]
    [SerializeField, Min(0.01f)] private float bobFadeSpeed = 10f;

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

    private Transform spriteTransform;
    private Vector3 spriteBaseLocalPosition;
    private float bobTime;
    private float bobWeight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteTransform = spriteRenderer.transform;
            spriteBaseLocalPosition = spriteTransform.localPosition;
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        ApplyFacingSprite();
    }

    private void Update()
    {
        Vector2 rawInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (rawInput.sqrMagnitude > 1f)
            rawInput.Normalize();

        if (inputSmoothTime > 0f)
            smoothedInput = Vector2.SmoothDamp(
                smoothedInput, rawInput, ref inputVelocity, inputSmoothTime);
        else
            smoothedInput = rawInput;

        moveInput = smoothedInput;

        if (rawInput.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
                lastFacing = rawInput.x < 0f ? Vector2.left : Vector2.right;
            else
                lastFacing = rawInput.y > 0f ? Vector2.up : Vector2.down;

            ApplyFacingSprite();
        }

        UpdateWalkBob();
    }

    private void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveInput * moveSpeed;
#else
        rb.velocity = moveInput * moveSpeed;
#endif
    }

    private void UpdateWalkBob()
    {
        if (spriteTransform == null)
            return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        float targetWeight = enableWalkBob && isMoving ? 1f : 0f;
        bobWeight = Mathf.MoveTowards(bobWeight, targetWeight, bobFadeSpeed * Time.deltaTime);

        if (isMoving && enableWalkBob)
            bobTime += Time.deltaTime * bobFrequency * Mathf.PI * 2f;

        // 使用正弦波的上半部分，让角色呈现轻微、规律的脚步起伏。
        float bobOffset = Mathf.Abs(Mathf.Sin(bobTime)) * bobAmplitude * bobWeight;
        Vector3 localPosition = spriteBaseLocalPosition;
        localPosition.y += bobOffset;
        spriteTransform.localPosition = localPosition;
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

    private void OnDisable()
    {
        // 禁用脚本时复位贴图位置，避免角色停在跳动的最高点。
        if (spriteTransform != null)
            spriteTransform.localPosition = spriteBaseLocalPosition;
        bobWeight = 0f;
    }
}
