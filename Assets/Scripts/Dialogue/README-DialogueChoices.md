# 对话结束选项设置说明

本功能适用于 Unity 2022.3 LTS，沿用项目现有的 DialogueManager、BuildingInteractable、InventorySystem 和 WuzhongEvents。

## 1. 建筑设置

1. 在 Hierarchy 选中建筑或其交互区域。
2. 确认物件上有 Collider2D 和 BuildingInteractable。
3. 将 Interaction Type 设为 Dialogue。
4. 填写 Speaker Name 和 Dialogue Lines。
5. 展开 Dialogue Choices，设置 Size（选项数量）。
6. 逐项填写 Label 和 Action。

选项会在所有对话台词播放完后显示在现有 Dialogue Text 中，不需要新增按钮或 UI 物件。

## 2. 支持的动作

- Stay：关闭对话，留在当前场景。
- LoadScene：载入 Target Scene。场景必须先加入 File > Build Settings > Scenes In Build。
- GiveItem：将 ItemDefinition 指定的道具加入 InventorySystem 背包，可设置数量。
- DiscoverClue：通过 WuzhongEvents 发布线索发现事件，填写 Clue Id、Title 和 Description。

## 3. 道具配置

1. 在 Project 面板右键。
2. 选择 Create > Wu Zhong > Item Definition。
3. 设置 Item Id、Display Name、Description、Icon 和 Max Stack。
4. 回到建筑的 GiveItem 选项，将创建的 ItemDefinition 拖入 Item 栏位。

玩家可使用项目现有 InventoryUI 的 I 键查看背包（前提是场景中已有背包 UI 并完成引用设置）。

## 4. 条件选项

可选填写 Required Item 和 Required Amount。玩家背包中没有足够数量时，选项仍会显示，但确认时会显示 Unavailable Message，不会执行动作。

例如：只有拿到「旧钥匙」才能选择「进入地下室」。

## 5. 操作方式

- 对话期间：使用原有 Continue Key（默认 E）继续。
- 选项期间：↑ / W 上移，↓ / S 下移，Enter / 小键盘 Enter / Space 确认，Esc 关闭。
- 选项显示在现有 Dialogue Text 中，因此请确保该文本框高度足以容纳多行选项。

## 6. 兼容性

- BuildingInteractable 的 Dialogue Choices 留空时，继续使用旧版 DialogueOutcome。
- DialogueManager 原有 StartDialogue(speaker, lines) 调用仍然可用。
- InventorySystem 仍负责背包数据；道具通过 AddItem 发放。
- 场景切换使用 SceneManager.LoadScene，场景名称必须在 Build Settings 中注册。
