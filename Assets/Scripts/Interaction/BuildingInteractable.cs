using UnityEngine;

/// <summary>
/// Adds a simple configurable interaction to a building.
/// Attach this component to the building root or to a parent of its collider.
/// </summary>
public class BuildingInteractable : MonoBehaviour, IInteractable
{
    [Header("Building Interaction")]
    [SerializeField] private string buildingName = "老宅";
    [SerializeField] private string actionDescription = "调查";

    /// <summary>
    /// Text shown in the player's interaction prompt.
    /// Example: [E] 调查老宅
    /// </summary>
    public string InteractionPrompt => $"{actionDescription}{buildingName}";

    /// <summary>
    /// Called by PlayerInteractor when the player presses E nearby.
    /// Replace or extend this with dialogue, scene loading, or investigation logic.
    /// </summary>
    public void Interact()
    {
        Debug.Log($"玩家触发建筑交互：{actionDescription}{buildingName}", this);
    }
}
