# 雾钟（Wu Zhong）Unity 项目基础代码

《雾钟》是一款 2D 俯视角中式民俗悬疑探索解谜游戏。玩家扮演沈砚，在每天清晨钟声响起后居民都会遗忘昨日的雾溪镇中，收集线索并调查洪灾与黑钟的真相。

## 开发环境

- Unity 2022.3.62f3 LTS
- Universal 2D 模板
- C# 脚本位于 `Assets/Scripts/`
- 地图改为在 Unity 编辑器中使用 Grid、Tilemap 和 Tile Palette 手工搭建；本分支不包含自动地图生成器

## 保留的核心脚本

- `Player/PlayerController.cs`：主角四方向移动、方向贴图切换与可选的行走起伏效果
- `Camera/WuzhongCameraFollow2D.cs`：2D 摄像机平滑跟随、轴向跟随开关与可选边界
- `Interaction/IInteractable.cs`：可交互对象接口
- `Interaction/PlayerInteractor.cs`：检测附近可交互对象并按 E 交互
- `Interaction/ClueInteractable.cs`：调查并收集线索
- `Dialogue/DialogueLine.cs`：对话数据结构
- `Dialogue/DialogueManager.cs`：基础对话显示与推进
- `Inventory/ClueJournal.cs`：线索记录
- `TimeLoop/TimeLoopManager.cs`：七日循环的基础计时与天数推进
- `Core/GameManager.cs`：简单的全局管理入口

## 导入与验证

1. 将需要的 `Assets/Scripts/` 内容迁移到 Unity 项目的 `Assets/Scripts/` 下；不要覆盖整个 `ProjectSettings/` 或 `Packages/`。
2. 在 Unity 中等待脚本编译完成，并检查 Console 是否有红色错误。
3. 确保项目已安装并启用适用的输入系统设置。移动脚本兼容旧输入管理器；若使用新输入系统，请确认 Input System 包已安装并启用。
4. 在测试场景中创建 Player，添加 `Rigidbody2D`、`Collider2D`、`SpriteRenderer` 和 `PlayerController`。
5. 将 `WuzhongCameraFollow2D` 添加到 Main Camera，并将 Player 的 Transform 拖到 `Target` 字段。

## 玩家配置

- `Rigidbody2D` 建议设为 Dynamic，Gravity Scale 为 0，并冻结 Z 轴旋转。
- 在 `PlayerController` 中按需分配 Front、Back、Left、Right 四方向 Sprite。
- 脚本会在必要时将根物体上的 SpriteRenderer 移到视觉子物体，以避免行走起伏影响物理根物体。
- 使用 2D 正交摄像机；常见 Z 偏移为 -10。若不需要平滑跟随，可在摄像机脚本中关闭 Smooth Follow。

## 手工创建地图

本分支移除了自动地图生成器及其自定义 Inspector 工具。请在 Unity 中通过 `GameObject > 2D Object > Tilemap` 创建 Grid/Tilemap，再用 Tile Palette 绘制地面和道路。大型建筑、树木和灯笼可以作为独立 SpriteRenderer 对象放置，并根据需要添加 Collider2D。

## 当前范围

对话、交互、线索记录和时间循环脚本仍保留为基础框架，使用前请在 Unity 中验证并按项目需求调整。完整任务系统、存档、每日世界状态重置和多结局逻辑仍需后续开发。
