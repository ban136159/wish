using UnityEngine;

/// <summary>
/// 可在 Project 面板中创建的物品配置。用 Create > Wu Zhong > Item Definition 创建。
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Wu Zhong/Item Definition")]
public sealed class ItemDefinition : ScriptableObject
{
    [Header("基础信息")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [TextArea(2, 5)] [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [Min(1)] [SerializeField] private int maxStack = 99;

    [Header("装备")]
    [SerializeField] private bool isEquipment;
    [SerializeField] private EquipmentSlot equipmentSlot;

    public string ItemId => itemId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => Mathf.Max(1, maxStack);
    public bool IsEquipment => isEquipment;
    public EquipmentSlot Slot => equipmentSlot;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemId))
            itemId = name;
        maxStack = Mathf.Max(1, maxStack);
    }
}

public enum EquipmentSlot
{
    None,
    Head,
    Body,
    Accessory,
    MainHand,
    OffHand
}
