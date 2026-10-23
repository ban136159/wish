using UnityEngine;

/// <summary>
/// 《雾钟》HD-2D 基础移动控制器。
/// 角色在 XZ 地面移动；Y 轴用于高度。请挂到 PlayerRoot，
/// 并确保同一对象上没有同时启用旧的 PlayerController (Rigidbody2D)。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    [Header("移动")]
    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [Tooltip("启用后让移动方向相对摄像机的水平朝向，而不是世界坐标。")]
    [SerializeField] private bool cameraRelativeMovement = false;
    [SerializeField] private Transform cameraTransform;

    public Vector2 MoveInput { get; private set; }
    public Vector3 WorldMoveDirection { get; private set; }

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        MoveInput = new Vector2(x, z).normalized;

        Vector3 direction = new Vector3(MoveInput.x, 0f, MoveInput.y);

        if (cameraRelativeMovement && cameraTransform != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            direction = right * MoveInput.x + forward * MoveInput.y;
        }

        WorldMoveDirection = direction.sqrMagnitude > 1f
            ? direction.normalized
            : direction;

        controller.Move(WorldMoveDirection * moveSpeed * Time.deltaTime);
    }
}
