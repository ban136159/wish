using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 将背包数据显示到现有 TMP 文本上。把此脚本挂到背包面板，并绑定内容文本与面板对象。
/// </summary>
public sealed class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private void Awake()
    {
        if (panel == null) panel = gameObject;
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.Changed += Refresh;
        WuzhongEvents.ItemAdded += OnItemAdded;
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.Changed -= Refresh;
        WuzhongEvents.ItemAdded -= OnItemAdded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle()
    {
        if (panel == null) return;
        bool show = !panel.activeSelf;
        panel.SetActive(show);
        if (show) Refresh();
    }

    public void Refresh()
    {
        if (inventoryText == null) return;
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            inventoryText.text = "背包系统尚未初始化";
            return;
        }

        StringBuilder text = new StringBuilder("背包\n\n");
        if (inventory.Items.Count == 0) text.AppendLine("暂无道具");
        foreach (InventorySystem.InventoryEntry entry in inventory.Items)
        {
            if (entry.item == null) continue;
            text.Append(entry.item.DisplayName).Append(" × ").Append(entry.quantity);
            if (entry.item.IsEquipment) text.Append(" [装备]");
            text.AppendLine();
            if (!string.IsNullOrWhiteSpace(entry.item.Description))
                text.Append("  ").AppendLine(entry.item.Description);
        }

        text.AppendLine("\n已装备");
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (slot == EquipmentSlot.None) continue;
            text.Append(slot).Append("：");
            text.AppendLine(inventory.Equipped.TryGetValue(slot, out ItemDefinition item)
                ? item.DisplayName : "空");
        }
        inventoryText.text = text.ToString();
    }

    private void OnItemAdded(string id, string displayName, int amount)
    {
        if (panel != null && panel.activeSelf) Refresh();
    }
}
