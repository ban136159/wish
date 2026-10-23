using System;
using UnityEngine;

/// <summary>
/// 管理一段对话的开始、逐句推进和结束。
/// UI 可监听 LineChanged 更新文本，监听 DialogueEnded 执行结束后的逻辑。
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel; // 对话 UI 面板
    [SerializeField] private DialogueLine[] openingLines; // Inspector 中配置的默认对话

    private int lineIndex; // 当前显示的对话下标

    public bool IsPlaying { get; private set; } // 当前是否正在播放对话

    // 每次切换到新句子时触发，参数为当前对话数据。
    public event Action<DialogueLine> LineChanged;

    // 所有对话播放完毕时触发。
    public event Action DialogueEnded;

    private void Start()
    {
        // 游戏开始时先隐藏对话面板。
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // 对话播放期间，按 E 或空格显示下一句。
        if (IsPlaying && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))) Advance();
    }

    /// <summary>使用指定的对话数组开始播放。</summary>
    public void StartDialogue(DialogueLine[] lines)
    {
        // 没有可播放的内容时不启动对话。
        if (lines == null || lines.Length == 0) return;

        openingLines = lines;
        lineIndex = 0;
        IsPlaying = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // 通知 UI 显示第一句。
        LineChanged?.Invoke(openingLines[lineIndex]);
    }

    /// <summary>播放 Inspector 中配置的默认开场对话。</summary>
    public void StartOpeningDialogue() => StartDialogue(openingLines);

    /// <summary>推进到下一句；如果已到末尾，则关闭对话并发出结束事件。</summary>
    public void Advance()
    {
        if (!IsPlaying) return;

        lineIndex++;

        // 超出数组范围代表对话结束。
        if (openingLines == null || lineIndex >= openingLines.Length)
        {
            IsPlaying = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            DialogueEnded?.Invoke();
            return;
        }

        // 通知 UI 更新为下一句内容。
        LineChanged?.Invoke(openingLines[lineIndex]);
    }
}
