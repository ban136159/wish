using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>建筑通用交互：可配置为对话、调查或进入场景。</summary>
public class BuildingInteractable : MonoBehaviour, IInteractable
{
    public enum InteractionType { Dialogue, Investigation, OpenDoor }

    [Header("建筑信息")]
    [SerializeField] private string buildingName = "老宅";
    [SerializeField] private InteractionType interactionType = InteractionType.Investigation;
    [SerializeField] private string promptOverride;

    [Header("对话")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("调查")]
    [TextArea(2, 5)]
    [SerializeField] private string investigationText = "门前的泥地上，有一串通往祠堂的湿脚印。";
    [SerializeField] private GameObject investigationPanel;
    [SerializeField] private TMP_Text investigationLabel;

    [Header("开门 / 进入场景")]
    [SerializeField] private string sceneName;
    [SerializeField] private bool logDoorInsteadOfLoading;

    public string InteractionPrompt
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(promptOverride)) return promptOverride;
            switch (interactionType)
            {
                case InteractionType.Dialogue: return $"与{buildingName}交谈";
                case InteractionType.OpenDoor: return $"进入{buildingName}";
                default: return $"调查{buildingName}";
            }
        }
    }

    private void Awake()
    {
        if (dialogueManager == null) dialogueManager = FindObjectOfType<DialogueManager>();
        if (investigationPanel != null) investigationPanel.SetActive(false);
    }

    public void Interact()
    {
        switch (interactionType)
        {
            case InteractionType.Dialogue: StartBuildingDialogue(); break;
            case InteractionType.Investigation: ShowInvestigation(); break;
            case InteractionType.OpenDoor: OpenDoor(); break;
        }
    }

    private void StartBuildingDialogue()
    {
        if (dialogueManager == null) dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError($"建筑“{buildingName}”配置为对话交互，但场景中找不到 DialogueManager。", this);
            return;
        }
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning($"建筑“{buildingName}”没有配置对话台词，请在 Inspector 的 Dialogue Lines 中添加内容。", this);
            return;
        }
        dialogueManager.StartDialogue(dialogueLines);
    }

    private void ShowInvestigation()
    {
        Debug.Log($"【调查：{buildingName}】{investigationText}", this);
        if (investigationLabel != null) investigationLabel.text = investigationText;
        if (investigationPanel != null) investigationPanel.SetActive(true);
    }

    private void OpenDoor()
    {
        if (logDoorInsteadOfLoading)
        {
            Debug.Log($"玩家打开了“{buildingName}”的门（测试模式）。", this);
            return;
        }
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"建筑“{buildingName}”没有填写 Scene Name。请填写 Build Settings / Build Profiles 中的场景名称。", this);
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"无法加载场景“{sceneName}”。请确认场景已加入 Build Settings / Build Profiles 的场景列表，且名称拼写正确。", this);
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>可绑定到调查面板的关闭按钮。</summary>
    public void HideInvestigation()
    {
        if (investigationPanel != null) investigationPanel.SetActive(false);
    }
}
