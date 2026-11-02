using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 建筑对话结束后显示的一个可选项。
/// 可在 BuildingInteractable 的 Inspector 中配置，不需要为每个选项编写脚本。
/// </summary>
[System.Serializable]
public sealed class DialogueChoice
{
    public enum ActionType
    {
        Stay,
        LoadScene,
        GiveItem,
        DiscoverClue
    }

    [Header("选项文字")]
    [SerializeField] private string label = "离开";

    [Header("执行动作")]
    [SerializeField] private ActionType action = ActionType.Stay;

    [Tooltip("动作选择 LoadScene 时填写，必须加入 Build Settings 的 Scenes In Build。")]
    [SerializeField] private string targetScene;

    [Tooltip("动作选择 GiveItem 时指定要加入背包的道具。")]
    [SerializeField] private ItemDefinition item;
    [Min(1)]
    [SerializeField] private int itemAmount = 1;

    [Tooltip("动作选择 DiscoverClue 时填写线索资料。")]
    [SerializeField] private string clueId;
    [SerializeField] private string clueTitle;
    [TextArea(2, 5)]
    [SerializeField] private string clueDescription;

    [Header("可选条件")]
    [Tooltip("不需要物品条件时留空；指定后，玩家必须持有足够数量才能选择此项。")]
    [SerializeField] private ItemDefinition requiredItem;
    [Min(1)]
    [SerializeField] private int requiredAmount = 1;
    [SerializeField] private string unavailableMessage = "你似乎还缺少必要的东西。";

    public string Label => string.IsNullOrWhiteSpace(label) ? "未命名选项" : label;

    public bool CanExecute(out string reason)
    {
        reason = string.Empty;

        if (requiredItem == null)
            return true;

        if (InventorySystem.Instance == null ||
            InventorySystem.Instance.Count(requiredItem) < Mathf.Max(1, requiredAmount))
        {
            reason = string.IsNullOrWhiteSpace(unavailableMessage)
                ? "你似乎还缺少必要的东西。"
                : unavailableMessage;
            return false;
        }

        return true;
    }

    public void Execute()
    {
        switch (action)
        {
            case ActionType.Stay:
                Debug.Log("选择：" + Label + "（留在当前场景）");
                break;

            case ActionType.LoadScene:
                if (string.IsNullOrWhiteSpace(targetScene))
                {
                    Debug.LogWarning("对话选项「" + Label + "」没有设置目标场景名称。");
                    return;
                }

                if (!Application.CanStreamedLevelBeLoaded(targetScene))
                {
                    Debug.LogError("无法载入场景「" + targetScene + "」。请检查场景名称，并将场景加入 Build Settings 的 Scenes In Build。");
                    return;
                }

                SceneManager.LoadScene(targetScene);
                break;

            case ActionType.GiveItem:
                if (item == null)
                {
                    Debug.LogWarning("对话选项「" + Label + "」没有指定奖励道具。");
                    return;
                }

                if (InventorySystem.Instance == null)
                {
                    Debug.LogError("场景中找不到 InventorySystem，无法发放道具。");
                    return;
                }

                InventorySystem.Instance.AddItem(item, Mathf.Max(1, itemAmount));
                Debug.Log("获得道具：" + item.DisplayName + " × " + Mathf.Max(1, itemAmount));
                break;

            case ActionType.DiscoverClue:
                if (string.IsNullOrWhiteSpace(clueId))
                {
                    Debug.LogWarning("对话选项「" + Label + "」没有设置线索 ID。");
                    return;
                }

                WuzhongEvents.RaiseClueDiscovered(clueId, clueTitle, clueDescription);
                Debug.Log("发现线索：" + clueTitle);
                break;
        }
    }
}
