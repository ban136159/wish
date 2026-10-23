using System;
using UnityEngine;

/// <summary>
/// 管理游戏内的日循环：累计一天的时间、推进日期，并在循环结束后重置到第一天。
/// 当前只提供计时与事件，不负责重置世界状态或存档。
/// </summary>
public class TimeLoopManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int daysPerLoop = 7; // 每轮包含的天数
    [SerializeField, Min(10f)] private float dayLengthSeconds = 300f; // 游戏内一天对应的现实秒数

    public int CurrentDay { get; private set; } = 1; // 当前轮次中的第几天
    public float DayProgress { get; private set; } // 当前一天的进度，通常在 0 到 1 之间
    public int LoopCount { get; private set; } = 1; // 已进入的循环轮数

    // 新的一天开始时触发，参数依次为天数和循环轮数。
    public event Action<int, int> NewDayStarted;

    // 新循环开始时触发，参数为新的循环轮数。
    public event Action<int> LoopRestarted;

    private void Update()
    {
        // 用现实时间增量推进游戏内一天的进度。
        DayProgress += Time.deltaTime / dayLengthSeconds;

        if (DayProgress >= 1f)
        {
            DayProgress = 0f;
            AdvanceDay();
        }
    }

    /// <summary>推进一天；到达本轮末日后进入下一轮的第一天。</summary>
    public void AdvanceDay()
    {
        if (CurrentDay < daysPerLoop)
        {
            CurrentDay++;
            NewDayStarted?.Invoke(CurrentDay, LoopCount);
            return;
        }

        // 本轮结束：循环数加一，日期回到第一天，并依次发送事件。
        LoopCount++;
        CurrentDay = 1;
        LoopRestarted?.Invoke(LoopCount);
        NewDayStarted?.Invoke(CurrentDay, LoopCount);
    }

    /// <summary>设置一天的长度，最短限制为 10 秒以避免过快推进。</summary>
    public void SetDayLength(float seconds) => dayLengthSeconds = Mathf.Max(10f, seconds);
}
