using UnityEngine;

/// <summary>
/// 《雾钟》俯视角玩家控制器。
/// 图集切片为 4 列 × 4 行，每帧 48×64 像素；行顺序：下、左、右、上。
/// 每行第 1 帧站立，其余 3 帧循环播放行走动画。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;

    [Header("动画")]
    [Tooltip("按图集从上到下、每行从左到右的顺序，设置 16 张切片 Sprite。")]
    [SerializeField] private Sprite[] characterFrames = new Sprite[16];
    [SerializeField, Min(1f)] private float animationFps = 8f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private int facingRow = 0; // 0=下，1=左，2=右，3=上

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (characterFrames == null || characterFrames.Length != 16 ||
            System.Array.Exists(characterFrames, sprite => sprite == null))
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>("Characters/Player/wuzhong_player_atlas");
            if (loaded.Length == 16)
            {
                System.Array.Sort(loaded, (a, b) =>
                {
                    int row = b.rect.y.CompareTo(a.rect.y);
                    return row != 0 ? row : a.rect.x.CompareTo(b.rect.x);
                });
                characterFrames = loaded;
            }
        }

        if (characterFrames == null || characterFrames.Length != 16 ||
            System.Array.Exists(characterFrames, sprite => sprite == null))
        {
            Debug.LogWarning("PlayerController：请在 Inspector 中按图集顺序拖入 16 张 Sprite。", this);
            enabled = false;
            return;
        }

        ApplyFrame(0);
    }

    private void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        UpdateFacing();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveInput * moveSpeed;
#else
        rb.velocity = moveInput * moveSpeed;
#endif
    }

    private void UpdateFacing()
    {
        if (moveInput.sqrMagnitude < 0.01f) return;

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            facingRow = moveInput.x < 0f ? 1 : 2;
        else
            facingRow = moveInput.y < 0f ? 0 : 3;
    }

    private void UpdateAnimation()
    {
        int frame = moveInput.sqrMagnitude < 0.01f
            ? 0
            : 1 + (Mathf.FloorToInt(Time.time * animationFps) % 3);

        ApplyFrame(facingRow * 4 + frame);
    }

    private void ApplyFrame(int index)
    {
        if (index >= 0 && index < characterFrames.Length && characterFrames[index] != null)
            spriteRenderer.sprite = characterFrames[index];
    }
}
