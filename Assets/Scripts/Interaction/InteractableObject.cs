using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 可交互物体组件，可挂在门、NPC、线索等对象上。
/// 提示文字与交互事件可在 Inspector 中配置。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InteractableObject : MonoBehaviour
{
    [Header("交互设置")]
    [SerializeField] private string objectName = "可调查物品";
    [SerializeField] private string promptMessage = "按 E 调查";

    [Header("交互事件")]
    [SerializeField] private UnityEvent onInteract;

    public string ObjectName => objectName;
    public string PromptMessage => promptMessage;

    public void Interact()
    {
        Debug.Log($"触发交互：{objectName}", this);
        onInteract?.Invoke();
    }
}
