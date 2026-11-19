using UnityEngine;

/// <summary>
/// Scene pickup object. Uses the existing PlayerInteractor through IInteractable.
/// Add this component to a world item with a Collider2D set to Is Trigger.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int quantity = 1;

    [Header("Interaction")]
    [SerializeField] private string promptPrefix = "拾取";

    private bool collected;

    public string InteractionPrompt
    {
        get
        {
            string itemName = item != null ? item.DisplayName : "未知物品";
            return string.IsNullOrWhiteSpace(promptPrefix)
                ? itemName
                : $"{promptPrefix}{itemName}";
        }
    }

    private void Reset()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider != null)
            pickupCollider.isTrigger = true;
    }

    private void OnValidate()
    {
        quantity = Mathf.Max(1, quantity);
    }

    public void Interact()
    {
        if (collected)
            return;

        if (item == null)
        {
            Debug.LogWarning($"拾取物品未配置 ItemDefinition：{name}", this);
            return;
        }

        InventorySystem inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("拾取失败：场景中没有已初始化的 InventorySystem。", this);
            return;
        }

        if (!inventory.AddItem(item, quantity))
        {
            Debug.LogWarning($"拾取失败：{item.DisplayName}", this);
            return;
        }

        collected = true;
        Debug.Log($"获得物品：{item.DisplayName} × {quantity}", this);
        Destroy(gameObject);
    }
}
