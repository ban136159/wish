using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 《雾钟》主角移动控制器。
/// 支持四方向贴图切换和移动时轻微上下跳动。
/// 跳动只移动视觉子物体，不影响 Rigidbody2D 的物理根物体。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.0f;
    [Tooltip("输入方向平滑时间。设为 0 可立即响应。")]
    [SerializeField, Min(0f)] private float inputSmoothTime = 0.04f;

    [Header("移动跳动效果")]
    [SerializeField] private bool enableWalkBob = true;
    [Tooltip("跳动幅度（世界单位）。建议 0.015 到 0.04。")]
    [SerializeField, Range(0f, 0.1f)] private float bobAmplitude = 0.025f;
    [Tooltip("每秒跳动频率。建议 5 到 8。")]
    [SerializeField, Range(1f, 15f)] private float bobFrequency = 6f;
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

        // 如果 SpriteRenderer 挂在物理根物体上，运行时建立独立视觉子物体，
        // 避免为了跳动而移动带有 Rigidbody2D/Collider2D 的根物体。
        if (spriteRenderer != null && spriteRenderer.transform == transform)
            MoveRendererToVisualChild();

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

    private void MoveRendererToVisualChild()
    {
        SpriteRenderer oldRenderer = spriteRenderer;
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        SpriteRenderer newRenderer = visual.AddComponent<SpriteRenderer>();
        newRenderer.sprite = oldRenderer.sprite;
        newRenderer.color = oldRenderer.color;
        newRenderer.flipX = oldRenderer.flipX;
        newRenderer.flipY = oldRenderer.flipY;
        newRenderer.sharedMaterial = oldRenderer.sharedMaterial;
        newRenderer.sortingLayerID = oldRenderer.sortingLayerID;
        newRenderer.sortingOrder = oldRenderer.sortingOrder;
        newRenderer.maskInteraction = oldRenderer.maskInteraction;
        newRenderer.drawMode = oldRenderer.drawMode;
        newRenderer.size = oldRenderer.size;
        newRenderer.tileMode = oldRenderer.tileMode;
        newRenderer.spriteSortPoint = oldRenderer.spriteSortPoint;
        newRenderer.enabled = oldRenderer.enabled;

        // 禁用根物体上的渲染器，避免画面重叠；原组件保留，避免破坏场景配置。
        oldRenderer.enabled = false;
        spriteRenderer = newRenderer;
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
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
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
        if (lastFacing == Vector2.up) nextSprite = backSprite;
        else if (lastFacing == Vector2.down) nextSprite = frontSprite;
        else if (lastFacing == Vector2.left) nextSprite = leftSprite;
        else if (lastFacing == Vector2.right) nextSprite = rightSprite;

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
