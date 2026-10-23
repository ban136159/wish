/// <summary>
/// 可交互对象的统一接口。
/// 玩家交互检测器通过该接口识别对象，而不必依赖某一种具体物品类型。
/// </summary>
public interface IInteractable
{
    // 提供给交互提示 UI 使用的简短文字，例如“调查：旧船票”。
    string InteractionPrompt { get; }

    // 执行对象自己的交互行为。
    void Interact();
}
