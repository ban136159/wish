using UnityEngine;

/// <summary>
/// 《雾钟》单张 PNG 玩家控制器。
/// 不使用 Sprite Sheet 或逐帧动画；移动时通过上下起伏和轻微倾斜模拟脚步。
/// 请将单张角色 PNG 导入为 Sprite (2D and UI)，并设置到 SpriteRenderer。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    [Header("单图晃动效果")]
    [Tooltip("移动时上下起伏的幅度（Unity 世界单位）。")]
    [SerializeField, Min(0f)] private float walkBobAmount = 0.045f;
    [Tooltip("移动时上下起伏的速度。")]
    [SerializeField, Min(0f)] private float walkBobSpeed = 12f;
    [Tooltip("静止时模拟呼吸的幅度。")]
    [SerializeField, Min(0f)] private float idleBobAmount = 0.008f;
    [SerializeField, Min(0f)] private float idleBobSpeed = 2f;
    [Tooltip("移动时角色视觉的最大倾斜角度。")]
    [SerializeField, Min(0f)] private float tiltAmount = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform visualTransform;
    private Vector2 moveInput;
    private Vector3 visualStartLocalPosition;
    private Quaternion visualStartLocalRotation;
    private float walkTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        SetupVisual();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning(
                "PlayerController：请给角色的 SpriteRenderer 指定一张 PNG Sprite。",
                this);
            enabled = false;
            return;
        }

        visualStartLocalPosition = visualTransform.localPosition;
        visualStartLocalRotation = visualTransform.localRotation;
    }

    private void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

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

    private void SetupVisual()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 如果 SpriteRenderer 原本挂在带 Rigidbody2D 的根对象上，
        // 自动将显示层移动到子对象，避免视觉晃动影响物理移动。
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
        float bob;
        float tilt = 0f;

        if (isMoving)
        {
            walkTime += Time.deltaTime * walkBobSpeed;
            bob = Mathf.Abs(Mathf.Sin(walkTime)) * walkBobAmount;
            tilt = -moveInput.x * tiltAmount;
        }
        else
        {
            bob = Mathf.Sin(Time.time * idleBobSpeed) * idleBobAmount;
        }

        visualTransform.localPosition = visualStartLocalPosition
            + Vector3.up * bob;

        visualTransform.localRotation = visualStartLocalRotation
            * Quaternion.Euler(0f, 0f, tilt);
    }
}
