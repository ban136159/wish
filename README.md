# 雾钟（Wu Zhong）Unity 项目基础目录

《雾钟》是一款 2D 俯视角中式民俗悬疑探索解谜游戏。玩家扮演沈砚，在每天清晨钟声响起后居民都会遗忘昨日的雾溪镇中，收集线索并调查洪灾与黑钟的真相。

## 环境
- Unity 6
- 2D / Universal 2D 模板
- C# 脚本放在 `Assets/Scripts/`
- 场景、素材与音频按目录分类

## 当前基础脚本
- `Player/PlayerController.cs`：俯视角移动
- `Interaction/IInteractable.cs`：可交互对象接口
- `Interaction/PlayerInteractor.cs`：检测附近可交互对象并按 E 交互
- `Interaction/ClueInteractable.cs`：调查并收集线索
- `Dialogue/DialogueLine.cs`：对话数据结构
- `Dialogue/DialogueManager.cs`：基础对话显示与推进
- `Inventory/ClueJournal.cs`：线索记录
- `TimeLoop/TimeLoopManager.cs`：七日循环的基础计时与天数推进
- `Core/GameManager.cs`：简单的全局管理入口

## 导入步骤
1. 将 `Assets/` 文件夹复制到 Unity 项目根目录（与 `Packages/` 同级）。
2. 在 Unity 中等待脚本编译。
3. 按下方的场景配置完成必要的 GameObject 组件绑定。

## 场景配置（最小可运行）
### 玩家
1. 创建 `Player` GameObject。
2. 添加 `SpriteRenderer`、`Rigidbody2D`（Body Type 设为 Dynamic、Gravity Scale 设为 0、Freeze Rotation Z）和 `Collider2D`。
3. 添加 `PlayerController` 与 `PlayerInteractor`。
4. 在 `PlayerInteractor` 上设置 `Interact Point`（可创建一个子物体 `InteractPoint` 并拖入），并设置 `Interact Radius`。
5. 可交互物体需在 `Interactable` Layer；将该 Layer 设为 `PlayerInteractor` 的 `Interactable Layers`。

### 调查物品
1. 创建物体并添加 `Collider2D`。
2. 添加 `ClueInteractable`，填写 `Clue Id`、`Clue Title`、`Clue Description`。
3. 把它放在 `Interactable` Layer。
4. 按 E 调查；线索会登记到 `ClueJournal`。

### 对话
1. 创建空物体 `DialogueManager` 并添加 `DialogueManager` 组件。
2. 在 Canvas 中创建 Panel，将对话框对象拖到 `Dialogue Panel`。
3. 对话文本 UI 可订阅 `LineChanged` 事件进行更新。

### 时间循环
1. 创建空物体 `TimeLoopManager` 并添加 `TimeLoopManager`。
2. 在 Inspector 设置 `Days Per Loop`、`Day Length Seconds`。
3. 当前版本提供天数/循环事件框架；存档、每日世界状态重置与多结局条件需要后续实现。

## 推荐目录
场景、素材、音频、预制体和 ScriptableObject 目录已预留；目前尚未包含最终美术资源、完整任务系统、存档系统、谜题逻辑或多结局实现。使用前请在 Unity 中测试并按需调整。