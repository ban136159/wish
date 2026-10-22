using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private DialogueLine[] openingLines;
    private int lineIndex;
    public bool IsPlaying { get; private set; }
    public event Action<DialogueLine> LineChanged;
    public event Action DialogueEnded;
    private void Start() { if (dialoguePanel != null) dialoguePanel.SetActive(false); }
    private void Update() { if (IsPlaying && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))) Advance(); }
    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0) return;
        openingLines = lines; lineIndex = 0; IsPlaying = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        LineChanged?.Invoke(openingLines[lineIndex]);
    }
    public void StartOpeningDialogue() => StartDialogue(openingLines);
    public void Advance()
    {
        if (!IsPlaying) return;
        lineIndex++;
        if (openingLines == null || lineIndex >= openingLines.Length) { IsPlaying = false; if (dialoguePanel != null) dialoguePanel.SetActive(false); DialogueEnded?.Invoke(); return; }
        LineChanged?.Invoke(openingLines[lineIndex]);
    }
}
