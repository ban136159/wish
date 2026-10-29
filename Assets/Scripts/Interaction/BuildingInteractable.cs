using UnityEngine;

/// <summary>
/// 《雾钟》建筑交互组件。
/// 添加到建筑根对象，并确保建筑或其子对象带有 Collider2D。
/// </summary>
public class BuildingInteractable : MonoBehaviour, IInteractable
{
    public enum InteractionType
    {
        Investigation,
        Dialogue,
        OpenDoor
    }

    [Header("基本设置")]
    [SerializeField] private string buildingName = "老宅";
    [SerializeField] private InteractionType interactionType = InteractionType.Investigation;
    [SerializeField] private string promptAction = "调查";

    [Header("对话设置")]
    [SerializeField] private string speakerName = "旁白";
    [TextArea(2, 6)]
    [SerializeField] private DialogueOutcome dialogueOutcome;
    [SerializeField] private string[] dialogueLines =
    {
        "门缝里透出一股潮湿的冷气。",
        "雾中似乎有人在低声呼唤。"
    };

    [Header("开门设置")]
    [Tooltip("可选：需要开门时隐藏或显示的门对象。")]
    [SerializeField] private GameObject doorVisual;
    [Tooltip("可选：开门后需要关闭的门碰撞体。")]
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private bool startsOpen;
    [SerializeField] private bool oneWayDoor = true;

    private bool isOpen;

    public string InteractionPrompt
    {
        get
        {
            switch (interactionType)
            {
                case InteractionType.Dialogue:
                    return $"交谈{buildingName}";
                case InteractionType.OpenDoor:
                    return isOpen ? $"{buildingName}已打开" : $"打开{buildingName}";
                default:
                    return $"{promptAction}{buildingName}";
            }
        }
    }

    private void Awake()
    {
        isOpen = startsOpen;
        ApplyDoorState();
    }

    public void Interact()
    {
        switch (interactionType)
        {
            case InteractionType.Dialogue:
                if (DialogueManager.Instance != null)
                {
                    if (dialogueOutcome != null)
                        dialogueOutcome.PrepareForNextDialogue();
                    DialogueManager.Instance.StartDialogue(speakerName, dialogueLines);
                }
                else
                {
                    Debug.LogWarning("场景中没有 DialogueManager，先创建并配置对话管理器。", this);
                    Debug.Log(speakerName + "：\n" + string.Join("\n", dialogueLines), this);
                }
                break;

            case InteractionType.OpenDoor:
                if (oneWayDoor && isOpen)
                    return;

                isOpen = !isOpen;
                ApplyDoorState();
                Debug.Log(isOpen ? $"打开了：{buildingName}" : $"关闭了：{buildingName}", this);
                break;

            default:
                Debug.Log($"玩家{promptAction}：{buildingName}", this);
                break;
        }
    }

    private void ApplyDoorState()
    {
        if (doorVisual != null)
            doorVisual.SetActive(!isOpen);

        if (doorCollider != null)
            doorCollider.enabled = !isOpen;
    }
}
