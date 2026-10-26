using System;
using UnityEngine;

/// <summary>《雾钟》对话管理器，保留原有公开方法和事件。</summary>
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private DialogueLine[] openingLines;
    private int lineIndex;
    private int startedFrame = -1;

    public bool IsPlaying { get; private set; }
    public event Action<DialogueLine> LineChanged;
    public event Action DialogueEnded;

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // 防止触发对话的同一帧 E 被当成“下一句”。
        if (IsPlaying && Time.frameCount > startedFrame &&
            (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
            Advance();
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager：没有配置对话内容。", this);
            return;
        }

        openingLines = lines;
        lineIndex = 0;
        IsPlaying = true;
        startedFrame = Time.frameCount;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        LineChanged?.Invoke(openingLines[lineIndex]);
    }

    public void StartOpeningDialogue() => StartDialogue(openingLines);

    public void Advance()
    {
        if (!IsPlaying) return;
        lineIndex++;
        if (openingLines == null || lineIndex >= openingLines.Length)
        {
            IsPlaying = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            DialogueEnded?.Invoke();
            return;
        }
        LineChanged?.Invoke(openingLines[lineIndex]);
    }

    public static DialogueManager FindActiveManager()
    {
        DialogueManager[] managers = FindObjectsOfType<DialogueManager>();
        foreach (DialogueManager manager in managers)
            if (manager != null && manager.IsPlaying) return manager;
        return managers.Length > 0 ? managers[0] : null;
    }
}
