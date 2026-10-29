using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 《雾钟》简易对话管理器。
/// 通过 WuzhongEvents 发布对话开始/结束通知，不要求 UI、音效等系统直接引用本类。
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("对话 UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueHintText;

    [Header("显示设置")]
    [SerializeField, Min(0f)] private float characterDelay = 0.02f;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;

    private string[] currentLines;
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping;

    public bool IsDialogueOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中存在多个 DialogueManager，重复对象将被销毁。", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsDialogueOpen || !ContinueKeyPressed())
            return;

        if (isTyping)
        {
            // 打字尚未结束时，按继续键立即显示完整句子。
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            typingCoroutine = null;
            return;
        }

        ShowNextLine();
    }

    /// <summary>
    /// 同时兼容 Unity 新输入系统和旧输入管理器。
    /// 项目设置为 Input System Package (New) 时，Input.GetKeyDown 不会正常读取按键。
    /// </summary>
    private bool ContinueKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (continueKey == KeyCode.Space && Keyboard.current.spaceKey.wasPressedThisFrame)
                return true;
            if (continueKey == KeyCode.Return && Keyboard.current.enterKey.wasPressedThisFrame)
                return true;
            if (continueKey == KeyCode.KeypadEnter && Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                return true;
            if (continueKey == KeyCode.Escape && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(continueKey))
            return true;
#endif
        return false;
    }
        if (isTyping)
        {
            // 打字尚未结束时，按继续键立即显示完整句子。
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            typingCoroutine = null;
            return;
        }

        ShowNextLine();
    }

    /// <summary>开始一段对话。speaker 可为空，lines 至少应包含一行。</summary>
    public void StartDialogue(string speaker, string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("对话内容为空。", this);
            return;
        }

        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogWarning("DialogueManager 尚未绑定 Dialogue Panel 或 Dialogue Text，请在 Inspector 中设置。", this);
            Debug.Log(speaker + "：" + string.Join("\n", lines), this);
            return;
        }

        currentLines = (string[])lines.Clone();
        currentLineIndex = 0;
        IsDialogueOpen = true;
        dialoguePanel.SetActive(true);

        if (speakerText != null)
            speakerText.text = speaker;

        if (continueHintText != null)
            continueHintText.text = continueKey == KeyCode.Space ? "按 空格 继续" : $"按 {continueKey} 继续";

        DisplayCurrentLine();
        WuzhongEvents.RaiseDialogueStarted(speaker, (string[])currentLines.Clone());
    }

    public void CloseDialogue()
    {
        bool wasOpen = IsDialogueOpen;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = null;
        isTyping = false;
        IsDialogueOpen = false;
        currentLines = null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 只有真正打开过的对话才发布结束事件，避免重复通知。
        if (wasOpen)
            WuzhongEvents.RaiseDialogueEnded();
    }

    private void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLines == null || currentLineIndex >= currentLines.Length)
        {
            CloseDialogue();
            return;
        }

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (characterDelay <= 0f)
        {
            dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            typingCoroutine = null;
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char character in line)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(characterDelay);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void OnDestroy()
    {
        // 若物件在对话进行中被销毁，也要通知订阅者收尾。
        CloseDialogue();
        if (Instance == this)
            Instance = null;
    }
}
