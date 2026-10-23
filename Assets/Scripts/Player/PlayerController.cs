using UnityEngine;

/// <summary>
/// 《雾钟》四方向玩家控制器。
/// 在 Inspector 中分别指定面向前/后/左/右的 Sprite；移动方向改变时自动切换图片。
/// 视觉晃动已大幅减弱，且只作用于 PlayerVisual 子对象，不影响物理移动。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    [Header("四方向图片")]
    [Tooltip("角色朝屏幕下方/面向玩家时使用的图片。")]
    [SerializeField] private Sprite frontSprite;
    [Tooltip("角色朝屏幕上方/背对玩家时使用的图片。")]
    [SerializeField] private Sprite backSprite;
    [Tooltip("角色朝左时使用的图片。")]
    [SerializeField] private Sprite leftSprite;
    [Tooltip("角色朝右时使用的图片。")]
    [SerializeField] private Sprite rightSprite;

    [Header("轻微视觉效果")]
    [Tooltip("移动时上下起伏幅度。建议 0.005 - 0.02。设为 0 可关闭。")]
    [SerializeField, Min(0f)] private float walkBobAmount = 0.012f;
    [Tooltip("移动时上下起伏速度。")]
    [SerializeField, Min(0f)] private float walkBobSpeed = 9f;
    [Tooltip("静止时呼吸幅度。设为 0 可完全关闭。")]
    [SerializeField, Min(0f)] private float idleBobAmount = 0.001f;
    [SerializeField, Min(0f)] private float idleBobSpeed = 1.5f;
    [Tooltip("移动时角色视觉的最大倾斜角度。")]
    [SerializeField, Min(0f)] private float tiltAmount = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform visualTransform;
    private Vector2 moveInput;
    private Vector3 visualStartLocalPosition;
    private Quaternion visualStartLocalRotation;
    private float walkTime;
    private Sprite lastAppliedSprite;

    private enum FacingDirection { Front, Back, Left, Right }
    private FacingDirection facing = FacingDirection.Front;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        SetupVisual();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("PlayerController：找不到 SpriteRenderer，请给主角添加 SpriteRenderer。", this);
            enabled = false;
            return;
        }

        visualStartLocalPosition = visualTransform.localPosition;
        visualStartLocalRotation = visualTransform.localRotation;

        // 如果四张方向图还没全部设置，先保留 Prefab 原本的图片作为备用。
        if (frontSprite == null) frontSprite = spriteRenderer.sprite;
        ApplyFacingSprite();
    }

    private void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        UpdateFacingDirection();
        UpdateVisualMotion();
    }

    private void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveInput * moveSpeed;
#else
        rb.velocity = moveInput * moveSpeed;
#endif
    }

    private void UpdateFacingDirection()
    {
        // 对角线移动时优先采用输入幅度较大的轴，避免方向图片快速来回切换。
        if (moveInput.sqrMagnitude < 0.01f)
            return;

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            facing = moveInput.x < 0 ? FacingDirection.Left : FacingDirection.Right;
        else
            facing = moveInput.y < 0 ? FacingDirection.Front : FacingDirection.Back;

        ApplyFacingSprite();
    }

    private void ApplyFacingSprite()
    {
        if (spriteRenderer == null) return;

        Sprite desired = facing switch
        {
            FacingDirection.Front => frontSprite,
            FacingDirection.Back => backSprite,
            FacingDirection.Left => leftSprite,
            FacingDirection.Right => rightSprite,
            _ => frontSprite
        };

        // 未设置某个方向的图时，保留当前可用图片，不会把角色变成空白。
        if (desired != null && desired != lastAppliedSprite)
        {
            spriteRenderer.sprite = desired;
            lastAppliedSprite = desired;
        }
    }

    private void SetupVisual()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // SpriteRenderer 若原本位于带 Rigidbody2D 的根对象上，
        // 将显示层移动到子对象，确保晃动只影响画面，不影响物理移动。
        if (spriteRenderer != null && spriteRenderer.transform == transform)
        {
            SpriteRenderer oldRenderer = spriteRenderer;
            GameObject visual = new GameObject("PlayerVisual");
            visual.transform.SetParent(transform, false);

            spriteRenderer = visual.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = oldRenderer.sprite;
            spriteRenderer.color = oldRenderer.color;
            spriteRenderer.flipX = oldRenderer.flipX;
            spriteRenderer.flipY = oldRenderer.flipY;
            spriteRenderer.sharedMaterial = oldRenderer.sharedMaterial;
            spriteRenderer.sortingLayerID = oldRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = oldRenderer.sortingOrder;
            spriteRenderer.drawMode = oldRenderer.drawMode;
            spriteRenderer.size = oldRenderer.size;
            spriteRenderer.maskInteraction = oldRenderer.maskInteraction;
            spriteRenderer.enabled = oldRenderer.enabled;

            Destroy(oldRenderer);
        }
        else if (spriteRenderer == null)
        {
            GameObject visual = new GameObject("PlayerVisual");
            visual.transform.SetParent(transform, false);
            spriteRenderer = visual.AddComponent<SpriteRenderer>();
        }

        visualTransform = spriteRenderer.transform;
    }

    private void UpdateVisualMotion()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        float bob = 0f;
        float tilt = 0f;

        if (isMoving)
        {
            walkTime += Time.deltaTime * walkBobSpeed;
            bob = Mathf.Abs(Mathf.Sin(walkTime)) * walkBobAmount;
            tilt = -moveInput.x * tiltAmount;
        }
        else if (idleBobAmount > 0f)
        {
            bob = Mathf.Sin(Time.time * idleBobSpeed) * idleBobAmount;
        }

        visualTransform.localPosition = visualStartLocalPosition + Vector3.up * bob;
        visualTransform.localRotation = visualStartLocalRotation * Quaternion.Euler(0f, 0f, tilt);
    }
}
