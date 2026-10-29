# 《雾钟》观察者模式接入说明

分支：`feat/wuzhong-observer-pattern`

## 设计约定

- **发布者**只负责在状态变化时调用 `WuzhongEvents.Raise...`，不直接寻找 UI、音效或任务对象。
- **事件中心** `WuzhongEvents` 提供统一的事件入口。
- **观察者**通过 C# event 的 `+=` 订阅，通过 `-=` 取消订阅。
- 观察者组件建议继承 `WuzhongEventObserver`，分别在 `SubscribeEvents()` 和 `UnsubscribeEvents()` 配对订阅，避免重复订阅。
- 不要在 `Update()` 中订阅事件；不要在普通场景切换时调用 `WuzhongEvents.ResetAll()`。

## 当前事件

| 事件 | 用途 |
|---|---|
| `InteractionTargetChanged(string id)` | 主角当前可交互目标变化；没有目标时 id 为 null |
| `InteractionStarted(string id)` | 开始与指定对象交互 |
| `InteractionEnded()` | 交互流程结束时的通知 |
| `DialogueStarted(string speaker, string[] lines)` | 对话开始 |
| `DialogueEnded()` | 对话结束 |
| `ClueDiscovered(string clueId, string title, string description)` | 发现线索 |
| `LoopDayChanged(int day)` | 时间循环天数变化 |

## 新增一个观察者

创建一个继承 `WuzhongEventObserver` 的 MonoBehaviour：

```csharp
using UnityEngine;

public class ExampleUIObserver : WuzhongEventObserver
{
    protected override void SubscribeEvents()
    {
        WuzhongEvents.DialogueStarted += HandleDialogueStarted;
        WuzhongEvents.DialogueEnded += HandleDialogueEnded;
    }

    protected override void UnsubscribeEvents()
    {
        WuzhongEvents.DialogueStarted -= HandleDialogueStarted;
        WuzhongEvents.DialogueEnded -= HandleDialogueEnded;
    }

    private void HandleDialogueStarted(string speaker, string[] lines)
    {
        Debug.Log($"显示对话 UI：{speaker}");
    }

    private void HandleDialogueEnded()
    {
        Debug.Log("隐藏对话 UI");
    }
}
```

## Unity 验证步骤

1. 拉取本分支并等待 Unity 编译。
2. 在场景中创建空物体，例如 `EventDebugObserver`。
3. 添加 `WuzhongEventDebugObserver` 组件。
4. Play，靠近可交互物体并按 E；打开/关闭对话。
5. 在 Console 查看对应事件日志。

## 注意

当前分支的事件中心和部分交互/对话代码已接入事件通知。线索系统和时间循环系统需要在对应的实际业务脚本中调用 `RaiseClueDiscovered` 与 `RaiseLoopDayChanged`；事件声明本身不会自动产生线索或时间循环行为。此分支的代码尚未在本地 Unity 编辑器中编译验证。
