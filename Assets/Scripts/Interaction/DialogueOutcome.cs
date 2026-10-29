using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 对话结束后执行的结果。将它挂到对话触发物上，并配置结果。
/// 由交互物在启动对话前调用 PrepareForNextDialogue；对话结束事件触发后执行。
/// </summary>
public sealed class DialogueOutcome : MonoBehaviour
{
    public enum OutcomeType { Stay, LoadScene, GiveItem }

    [SerializeField] private OutcomeType outcome = OutcomeType.Stay;
    [SerializeField] private string sceneName;
    [SerializeField] private ItemDefinition rewardItem;
    [Min(1)] [SerializeField] private int rewardAmount = 1;
    [SerializeField] private bool showResultMessage = true;

    private bool waitingForDialogueEnd;

    private void OnEnable() => WuzhongEvents.DialogueEnded += OnDialogueEnded;
    private void OnDisable() => WuzhongEvents.DialogueEnded -= OnDialogueEnded;

    /// <summary>在调用 DialogueManager.StartDialogue 之前调用。</summary>
    public void PrepareForNextDialogue()
    {
        waitingForDialogueEnd = true;
    }

    private void OnDialogueEnded()
    {
        if (!waitingForDialogueEnd) return;
        waitingForDialogueEnd = false;

        switch (outcome)
        {
            case OutcomeType.Stay:
                if (showResultMessage) Debug.Log("对话结束：留在当前场景。", this);
                break;
            case OutcomeType.LoadScene:
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    Debug.LogWarning("DialogueOutcome 未设置目标场景名称。", this);
                    return;
                }
                if (showResultMessage) Debug.Log("正在切换场景：" + sceneName, this);
                SceneManager.LoadScene(sceneName);
                break;
            case OutcomeType.GiveItem:
                if (rewardItem == null)
                {
                    Debug.LogWarning("DialogueOutcome 未设置奖励道具。", this);
                    return;
                }
                if (InventorySystem.Instance == null)
                {
                    Debug.LogWarning("场景中找不到 InventorySystem，无法发放道具。", this);
                    return;
                }
                InventorySystem.Instance.AddItem(rewardItem, rewardAmount);
                if (showResultMessage) Debug.Log("获得道具：" + rewardItem.DisplayName, this);
                break;
        }
    }
}
