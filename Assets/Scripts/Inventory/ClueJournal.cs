using System;
using System.Collections.Generic;
using UnityEngine;

public class ClueJournal : MonoBehaviour
{
    [Serializable] public class ClueEntry { public string id; public string title; [TextArea(2, 5)] public string description; }
    [SerializeField] private List<ClueEntry> clues = new List<ClueEntry>();
    public IReadOnlyList<ClueEntry> Clues => clues;
    public event Action<ClueEntry> ClueAdded;
    public bool AddClue(string id, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(id) || clues.Exists(c => c.id == id)) return false;
        ClueEntry entry = new ClueEntry { id = id, title = title, description = description };
        clues.Add(entry); ClueAdded?.Invoke(entry); return true;
    }
    public bool HasClue(string id) => clues.Exists(c => c.id == id);
}
