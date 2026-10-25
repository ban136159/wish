using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 《雾钟》主角移动控制器。
/// 支持四方向贴图切换，并可在独立的视觉子物体上播放轻微上下跳动。
/// Rigidbody2D 所在的根物体只负责移动和碰撞，避免视觉动画干扰物理。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.0f;
    [Tooltip("输入方向平滑时间。设为 0 可立即响应。")]
    [SerializeField, Min(0f)] private float inputSmoothTime = 0.04f;

    [Header("移动跳动效果")]
    [Tooltip("移动时启用轻微上下跳动。需要将 SpriteRenderer 放在主角的子物体上。")]
    [SerializeField] private bool enableWalkBob = true;
    [Tooltip("跳动幅度（世界单位）。建议 0.015 到 0.04。")]
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
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 只对独立的视觉子物体做跳动，绝不改动 Rigidbody2D 根物体的位置。
        if (spriteRenderer != null && spriteRenderer.transform != transform)
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
        Vector2 rawInput = ReadMoveInput();
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

    private Vector2 ReadMoveInput()
    {
        Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                input.x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                input.y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                input.y += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        // 新旧输入系统同时启用时，优先采用有实际输入的那一套。
        if (input.sqrMagnitude < 0.01f)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
        }
#endif

        return input;
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
        if (spriteTransform != null)
            spriteTransform.localPosition = spriteBaseLocalPosition;
        bobWeight = 0f;
    }
}
