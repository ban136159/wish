using UnityEngine;

/// <summary>
/// Optional top-down Y sorting for a single SpriteRenderer.
/// Assign a sortPoint at the character's feet or the building's ground contact point.
/// This script does not generate or modify the map.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class WuzhongYSort : MonoBehaviour
{
    [Tooltip("Character feet or building ground-contact point. If empty, uses this object's position.")]
    public Transform sortPoint;

    [Tooltip("Use the same value for objects that should share a depth layer.")]
    public int orderOffset;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Vector3 point = sortPoint != null ? sortPoint.position : transform.position;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-point.y * 100f) + orderOffset;
    }
}
