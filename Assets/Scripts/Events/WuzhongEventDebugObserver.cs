using UnityEngine;

/// <summary>
/// 《雾钟》事件监听示例。挂到场景中的任意 GameObject，Play 后在 Console 查看事件。
/// 这是调试观察者；正式 UI/音效系统可用同样方式订阅事件。
/// </summary>
public class WuzhongEventDebugObserver : MonoBehaviour
{
    private void OnEnable()
    {
        WuzhongEvents.InteractionTargetChanged += OnInteractionTargetChanged;
        WuzhongEvents.InteractionStarted += OnInteractionStarted;
        WuzhongEvents.InteractionEnded += OnInteractionEnded;
        WuzhongEvents.DialogueStarted += OnDialogueStarted;
        WuzhongEvents.DialogueEnded += OnDialogueEnded;
        WuzhongEvents.ClueDiscovered += OnClueDiscovered;
        WuzhongEvents.LoopDayChanged += OnLoopDayChanged;
    }

    private void OnDisable()
    {
        WuzhongEvents.InteractionTargetChanged -= OnInteractionTargetChanged;
        WuzhongEvents.InteractionStarted -= OnInteractionStarted;
        WuzhongEvents.InteractionEnded -= OnInteractionEnded;
        WuzhongEvents.DialogueStarted -= OnDialogueStarted;
        WuzhongEvents.DialogueEnded -= OnDialogueEnded;
        WuzhongEvents.ClueDiscovered -= OnClueDiscovered;
        WuzhongEvents.LoopDayChanged -= OnLoopDayChanged;
    }

    private void OnInteractionTargetChanged(string id)
        => Debug.Log(string.IsNullOrEmpty(id) ? "[雾钟事件] 已离开可交互目标" : $"[雾钟事件] 当前目标：{id}", this);

    private void OnInteractionStarted(string id)
        => Debug.Log($"[雾钟事件] 开始交互：{id}", this);

    private void OnInteractionEnded()
        => Debug.Log("[雾钟事件] 交互结束", this);

    private void OnDialogueStarted(string speaker, string[] lines)
        => Debug.Log($"[雾钟事件] 对话开始：{speaker}（{(lines == null ? 0 : lines.Length)} 行）", this);

    private void OnDialogueEnded()
        => Debug.Log("[雾钟事件] 对话结束", this);

    private void OnClueDiscovered(string clueId, string title, string description)
        => Debug.Log($"[雾钟事件] 发现线索：{title}（{clueId}）", this);

    private void OnLoopDayChanged(int day)
        => Debug.Log($"[雾钟事件] 时间循环进入第 {day} 天", this);
}
