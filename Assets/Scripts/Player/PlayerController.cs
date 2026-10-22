using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 3.5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private void Awake() { rb = GetComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.freezeRotation = true; }
    private void Update() { moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized; }
    private void FixedUpdate() { rb.linearVelocity = moveInput * moveSpeed; }
}
