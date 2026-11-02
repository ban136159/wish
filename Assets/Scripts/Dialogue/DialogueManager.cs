using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 《雾钟》对话管理器。
/// 支持逐字对话，以及对话结束后以方向键选择事件选项。
/// 选项文字显示在现有 Dialogue Text 中，不需要额外建立按钮或 UI 物件。
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
    [SerializeField] private KeyCode continueKey = KeyCode.E;

    [Header("选项操作")]
    [SerializeField] private KeyCode previousChoiceKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode nextChoiceKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode confirmChoiceKey = KeyCode.Return;
    [SerializeField] private KeyCode cancelChoiceKey = KeyCode.Escape;

    private string[] currentLines;
    private DialogueChoice[] currentChoices;
    private int currentLineIndex;
    private int selectedChoiceIndex;
    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool isChoosing;

    public bool IsDialogueOpen { get; private set; }
    public bool IsChoosing => isChoosing;

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
        if (!IsDialogueOpen)
            return;

        if (isChoosing)
        {
            HandleChoiceInput();
            return;
        }

        if (!ContinueKeyPressed())
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

    /// <summary>读取对话继续键。项目目前使用旧 Input Manager。</summary>
    private bool ContinueKeyPressed()
    {
        return Input.GetKeyDown(continueKey);
    }

    /// <summary>兼容旧调用：只开始对话，不显示结束选项。</summary>
    public void StartDialogue(string speaker, string[] lines)
    {
        StartDialogue(speaker, lines, null);
    }

    /// <summary>开始对话，并在全部台词播放完后显示可选项。</summary>
    public void StartDialogue(string speaker, string[] lines, DialogueChoice[] choices)
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

        if (IsDialogueOpen)
            CloseDialogue();

        currentLines = (string[])lines.Clone();
        currentChoices = HasChoices(choices) ? (DialogueChoice[])choices.Clone() : null;
        currentLineIndex = 0;
        selectedChoiceIndex = 0;
        isChoosing = false;
        IsDialogueOpen = true;
        dialoguePanel.SetActive(true);

        if (speakerText != null)
            speakerText.text = speaker;

        if (continueHintText != null)
            continueHintText.text = $"按 {continueKey} 继续";

        DisplayCurrentLine();
        WuzhongEvents.RaiseDialogueStarted(speaker, (string[])currentLines.Clone());
    }

    private static bool HasChoices(DialogueChoice[] choices)
    {
        if (choices == null || choices.Length == 0)
            return false;

        foreach (DialogueChoice choice in choices)
            if (choice != null)
                return true;

        return false;
    }

    public void CloseDialogue()
    {
        bool wasOpen = IsDialogueOpen;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = null;
        isTyping = false;
        isChoosing = false;
        IsDialogueOpen = false;
        currentLines = null;
        currentChoices = null;

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
            if (HasChoices(currentChoices))
                BeginChoiceSelection();
            else
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

    private void BeginChoiceSelection()
    {
        isChoosing = true;
        selectedChoiceIndex = 0;
        while (selectedChoiceIndex < currentChoices.Length &&
               currentChoices[selectedChoiceIndex] == null)
            selectedChoiceIndex++;

        RenderChoices();

        if (continueHintText != null)
            continueHintText.text = "↑↓ 选择　Enter 确认　Esc 关闭";
    }

    private void HandleChoiceInput()
    {
        if (Input.GetKeyDown(cancelChoiceKey))
        {
            CloseDialogue();
            return;
        }

        if (Input.GetKeyDown(previousChoiceKey) || Input.GetKeyDown(KeyCode.W))
        {
            MoveChoiceSelection(-1);
            return;
        }

        if (Input.GetKeyDown(nextChoiceKey) || Input.GetKeyDown(KeyCode.S))
        {
            MoveChoiceSelection(1);
            return;
        }

        if (Input.GetKeyDown(confirmChoiceKey) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelectedChoice();
        }
    }

    private void MoveChoiceSelection(int direction)
    {
        if (currentChoices == null || currentChoices.Length == 0)
            return;

        int validCount = 0;
        foreach (DialogueChoice choice in currentChoices)
            if (choice != null) validCount++;

        if (validCount == 0)
            return;

        do
        {
            selectedChoiceIndex += direction;
            if (selectedChoiceIndex < 0)
                selectedChoiceIndex = currentChoices.Length - 1;
            else if (selectedChoiceIndex >= currentChoices.Length)
                selectedChoiceIndex = 0;
        }
        while (currentChoices[selectedChoiceIndex] == null);

        RenderChoices();
    }

    private void RenderChoices()
    {
        if (dialogueText == null || currentChoices == null)
            return;

        StringBuilder output = new StringBuilder();
        if (currentLines != null && currentLines.Length > 0)
            output.Append(currentLines[currentLines.Length - 1]).AppendLine().AppendLine();

        output.AppendLine("你要怎么做？");
        for (int i = 0; i < currentChoices.Length; i++)
        {
            DialogueChoice choice = currentChoices[i];
            if (choice == null)
                continue;

            output.Append(i == selectedChoiceIndex ? "▶ " : "　 ");
            output.Append(i + 1).Append(". ").AppendLine(choice.Label);
        }

        dialogueText.text = output.ToString();
    }

    private void ConfirmSelectedChoice()
    {
        if (currentChoices == null ||
            selectedChoiceIndex < 0 ||
            selectedChoiceIndex >= currentChoices.Length)
            return;

        DialogueChoice selected = currentChoices[selectedChoiceIndex];
        if (selected == null)
            return;

        if (!selected.CanExecute(out string reason))
        {
            if (!string.IsNullOrWhiteSpace(reason))
                dialogueText.text = reason + "\n\n按方向键选择其他选项。";
            return;
        }

        // 先关闭对话并发布 DialogueEnded，再执行场景切换或道具奖励。
        // 这样旧的 DialogueOutcome 订阅者也能正常完成收尾。
        CloseDialogue();
        selected.Execute();
    }

    private void OnDestroy()
    {
        CloseDialogue();
        if (Instance == this)
            Instance = null;
    }
}
