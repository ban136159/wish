using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 可交互物体组件，可挂在门、NPC、线索等对象上。
/// 可在 Inspector 中配置调查提示、对话内容和额外交互事件。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InteractableObject : MonoBehaviour
{
    [Header("交互设置")]
    [SerializeField] private string objectName = "可调查物品";
    [SerializeField] private string promptMessage = "按 E 调查";

    [Header("调查对话")]
    [SerializeField] private bool startDialogueOnInteract = true;
    [SerializeField] private string speakerName = "旁白";
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines =
    {
        "木门上布满了潮湿的痕迹。",
        "门缝深处，似乎传来一阵微弱的钟声……"
    };

    [Header("交互事件（可选）")]
    [SerializeField] private UnityEvent onInteract;

    public string ObjectName => objectName;
    public string PromptMessage => promptMessage;

    public void Interact()
    {
        Debug.Log($"触发交互：{objectName}", this);

        // 开始对话前隐藏交互提示，避免两个 UI 同时出现。
        if (UIManager.Instance != null)
            UIManager.Instance.HideInteractionPrompt();

        if (startDialogueOnInteract)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(speakerName, dialogueLines);
            }
            else
            {
                Debug.LogWarning(
                    $"「{objectName}」配置为启动对话，但场景中没有 DialogueManager。",
                    this
                );
            }
        }

        // 保留 Inspector 中配置的事件，方便之后添加开门、播放音效等行为。
        onInteract?.Invoke();
    }
}
