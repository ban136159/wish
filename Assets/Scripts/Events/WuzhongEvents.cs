using System;

/// <summary>
/// 《雾钟》全局事件中心：让交互、对话、线索和时间循环系统通过事件通信。
/// 订阅者应在 OnEnable 订阅，并在 OnDisable 取消订阅。
/// </summary>
public static class WuzhongEvents
{
    public static event Action<string> InteractionTargetChanged;
    public static event Action<string> InteractionStarted;
    public static event Action InteractionEnded;
    public static event Action<string, string[]> DialogueStarted;
    public static event Action DialogueEnded;
    public static event Action<string, string, string> ClueDiscovered;
    public static event Action<int> LoopDayChanged;

    public static void RaiseInteractionTargetChanged(string interactionId)
        => InteractionTargetChanged?.Invoke(interactionId);

    public static void RaiseInteractionStarted(string interactionId)
        => InteractionStarted?.Invoke(interactionId);

    public static void RaiseInteractionEnded()
        => InteractionEnded?.Invoke();

    public static void RaiseDialogueStarted(string speaker, string[] lines)
        => DialogueStarted?.Invoke(speaker, lines);

    public static void RaiseDialogueEnded()
        => DialogueEnded?.Invoke();

    public static void RaiseClueDiscovered(string clueId, string title, string description)
        => ClueDiscovered?.Invoke(clueId, title, description);

    public static void RaiseLoopDayChanged(int day)
        => LoopDayChanged?.Invoke(day);

    /// <summary>仅在测试或明确的全局重置流程中调用；不要在普通场景切换时随意清空事件。</summary>
    public static void ResetAll()
    {
        InteractionTargetChanged = null;
        InteractionStarted = null;
        InteractionEnded = null;
        DialogueStarted = null;
        DialogueEnded = null;
        ClueDiscovered = null;
        LoopDayChanged = null;
    }
}
