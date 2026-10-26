using UnityEngine;

/// <summary>
/// 建筑交互组件。添加到建筑根对象，按 E 时会在 Console 输出测试信息。
/// </summary>
public class BuildingInteractable : MonoBehaviour, IInteractable
{
    [Header("建筑交互")]
    [SerializeField] private string buildingName = "老宅";
    [SerializeField] private string actionDescription = "调查";

    public string InteractionPrompt => $"{actionDescription}{buildingName}";

    public void Interact()
    {
        Debug.Log($"你{actionDescription}了：{buildingName}", this);
    }
}
