using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家背包运行时数据。跨场景保留；物品以 ItemDefinition 作为稳定配置引用。
/// UI 通过 Changed 事件刷新，不需要让背包直接引用界面。
/// </summary>
[DisallowMultipleComponent]
public sealed class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Serializable]
    public sealed class InventoryEntry
    {
        public ItemDefinition item;
        [Min(1)] public int quantity = 1;
    }

    [SerializeField] private List<InventoryEntry> startingItems = new List<InventoryEntry>();
    private readonly List<InventoryEntry> items = new List<InventoryEntry>();
    private readonly Dictionary<EquipmentSlot, ItemDefinition> equipped =
        new Dictionary<EquipmentSlot, ItemDefinition>();

    public event Action Changed;
    public IReadOnlyList<InventoryEntry> Items => items;
    public IReadOnlyDictionary<EquipmentSlot, ItemDefinition> Equipped => equipped;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (InventoryEntry entry in startingItems)
            if (entry != null && entry.item != null)
                AddItem(entry.item, entry.quantity);
    }

    public bool AddItem(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int remaining = amount;
        while (remaining > 0)
        {
            InventoryEntry stack = items.Find(e => e.item == item && e.quantity < item.MaxStack);
            if (stack == null)
            {
                stack = new InventoryEntry { item = item, quantity = 0 };
                items.Add(stack);
            }

            int added = Mathf.Min(remaining, item.MaxStack - stack.quantity);
            stack.quantity += added;
            remaining -= added;
        }

        WuzhongEvents.RaiseItemAdded(item.ItemId, item.DisplayName, amount);
        Changed?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0 || Count(item) < amount)
            return false;

        int remaining = amount;
        for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
        {
            InventoryEntry entry = items[i];
            if (entry.item != item) continue;
            int removed = Mathf.Min(remaining, entry.quantity);
            entry.quantity -= removed;
            remaining -= removed;
            if (entry.quantity <= 0) items.RemoveAt(i);
        }

        Changed?.Invoke();
        return true;
    }

    public int Count(ItemDefinition item)
    {
        int total = 0;
        foreach (InventoryEntry entry in items)
            if (entry.item == item) total += entry.quantity;
        return total;
    }

    public bool Equip(ItemDefinition item)
    {
        if (item == null || !item.IsEquipment || Count(item) < 1 || item.Slot == EquipmentSlot.None)
            return false;

        equipped[item.Slot] = item;
        Changed?.Invoke();
        return true;
    }

    public bool Unequip(EquipmentSlot slot)
    {
        bool removed = equipped.Remove(slot);
        if (removed) Changed?.Invoke();
        return removed;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
