using System;

/// <summary>
/// 《雾钟》轻量事件中心。
/// 用于解耦交互、对话、线索与时间循环系统。
/// 订阅者应在 OnEnable 订阅、OnDisable 取消订阅，避免场景切换后残留回调。
/// </summary>
public static class WuzhongEvents
{
    /// <summary>玩家开始与可交互对象交互。</summary>
    public static event Action<string> InteractionStarted;

    /// <summary>玩家结束当前交互。</summary>
    public static event Action InteractionEnded;

    /// <summary>一段对话开始；参数依次为说话者和对话行。</summary>
    public static event Action<string, string[]> DialogueStarted;

    /// <summary>当前对话结束。</summary>
    public static event Action DialogueEnded;

    /// <summary>玩家发现线索；参数依次为线索 ID、标题、描述。</summary>
    public static event Action<string, string, string> ClueDiscovered;

    /// <summary>循环天数变化；参数为当前天数。</summary>
    public static event Action<int> LoopDayChanged;

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

    /// <summary>
    /// 在测试或重置游戏状态时清空所有静态订阅，防止播放模式重载设置导致的残留。
    /// 正常业务代码不应随意调用此方法。
    /// </summary>
    public static void ResetAll()
    {
        InteractionStarted = null;
        InteractionEnded = null;
        DialogueStarted = null;
        DialogueEnded = null;
        ClueDiscovered = null;
        LoopDayChanged = null;
    }
}
