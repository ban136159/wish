using UnityEngine;

public class ClueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string clueId = "clue_old_ticket";
    [SerializeField] private string clueTitle = "旧船票";
    [TextArea(2, 5)] [SerializeField] private string clueDescription = "船票边缘已经被水浸得发软。日期似乎与记忆中的洪灾有关。";
    public string InteractionPrompt => $"调查：{clueTitle}";
    public void Interact()
    {
        ClueJournal journal = FindFirstObjectByType<ClueJournal>();
        if (journal == null) { Debug.LogWarning("场景中没有 ClueJournal，无法登记线索。"); return; }
        bool added = journal.AddClue(clueId, clueTitle, clueDescription);
        Debug.Log(added ? $"发现线索：{clueTitle}" : $"已记录过线索：{clueTitle}");
    }
}
