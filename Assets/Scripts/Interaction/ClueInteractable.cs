using UnityEngine;

/// <summary>
/// 可调查的线索物体。交互时将线索写入场景中的 ClueJournal。
/// </summary>
public class ClueInteractable : MonoBehaviour, IInteractable
{
    // 线索的唯一标识；不同线索不要使用相同 ID。
    [SerializeField] private string clueId = "clue_old_ticket";
    [SerializeField] private string clueTitle = "旧船票";
    [TextArea(2, 5)] [SerializeField] private string clueDescription = "船票边缘已经被水浸得发软。日期似乎与记忆中的洪灾有关。";

    // 玩家靠近时可供提示 UI 使用的交互文字。
    public string InteractionPrompt => $"调查：{clueTitle}";

    public void Interact()
    {
        // 在当前场景中查找线索记录管理器。
        ClueJournal journal = FindFirstObjectByType<ClueJournal>();

        // 没有记录管理器时给出警告，避免静默失败。
        if (journal == null) { Debug.LogWarning("场景中没有 ClueJournal，无法登记线索。"); return; }

        // AddClue 会阻止重复 ID，因此重复调查不会重复登记。
        bool added = journal.AddClue(clueId, clueTitle, clueDescription);
        Debug.Log(added ? $"发现线索：{clueTitle}" : $"已记录过线索：{clueTitle}");
    }
}
