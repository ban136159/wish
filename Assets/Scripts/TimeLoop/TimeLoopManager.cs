using System;
using UnityEngine;

public class TimeLoopManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int daysPerLoop = 7;
    [SerializeField, Min(10f)] private float dayLengthSeconds = 300f;
    public int CurrentDay { get; private set; } = 1;
    public float DayProgress { get; private set; }
    public int LoopCount { get; private set; } = 1;
    public event Action<int, int> NewDayStarted;
    public event Action<int> LoopRestarted;
    private void Update() { DayProgress += Time.deltaTime / dayLengthSeconds; if (DayProgress >= 1f) { DayProgress = 0f; AdvanceDay(); } }
    public void AdvanceDay()
    {
        if (CurrentDay < daysPerLoop) { CurrentDay++; NewDayStarted?.Invoke(CurrentDay, LoopCount); return; }
        LoopCount++; CurrentDay = 1; LoopRestarted?.Invoke(LoopCount); NewDayStarted?.Invoke(CurrentDay, LoopCount);
    }
    public void SetDayLength(float seconds) => dayLengthSeconds = Mathf.Max(10f, seconds);
}
