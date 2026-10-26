using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 《雾钟》简易对话管理器。
/// 在场景中创建一个 DialogueManager 对象，并在 Inspector 中绑定对话面板、文本和继续提示。
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
    [SerializeField] private KeyCode continueKey = KeyCode.Escape;

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
        if (!IsDialogueOpen || !Input.GetKeyDown(KeyCode.E))
            return;

        if (isTyping)
        {
            // 打字尚未结束时，按 E 立即显示完整句子。
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

        currentLines = lines;
        currentLineIndex = 0;
        IsDialogueOpen = true;
        dialoguePanel.SetActive(true);

        if (speakerText != null)
            speakerText.text = speaker;

        if (continueHintText != null)
            continueHintText.text = "按 E 继续";

        DisplayCurrentLine();
    }

    public void CloseDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = null;
        isTyping = false;
        IsDialogueOpen = false;
        currentLines = null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
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
        if (Instance == this)
            Instance = null;
    }
}
