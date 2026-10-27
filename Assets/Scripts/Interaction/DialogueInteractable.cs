using UnityEngine;

/// <summary>
/// 《雾钟》基础对话交互物。
/// 将此脚本挂到老宅门、祠堂、调查点等对象上；
/// 该对象需要有 Collider2D，且 PlayerInteractor 的检测范围能覆盖到它。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [Header("交互提示")]
    [SerializeField] private string interactionPrompt = "调查";

    [Header("对话内容")]
    [SerializeField] private string speakerName = "旁白";
    [TextArea(2, 6)]
    [SerializeField] private string[] dialogueLines =
    {
        "雾气深处传来一阵沉闷的钟声。",
        "这座老宅似乎已经很久没有人居住了。"
    };

    public string InteractionPrompt => interactionPrompt;

    private void Reset()
    {
        Collider2D targetCollider = GetComponent<Collider2D>();
        targetCollider.isTrigger = true;
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("场景里没有 DialogueManager。请先创建并配置对话管理器。", this);
            return;
        }

        DialogueManager.Instance.StartDialogue(speakerName, dialogueLines);
    }
}
