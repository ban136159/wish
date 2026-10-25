# 雾钟 Unity 自动地图生成器

## 安装
1. 将 Assets/Scripts/Map/WuzhongMapGenerator.cs 复制到 Unity 项目的同一路径。
2. 将 Assets/Editor/WuzhongMapGeneratorEditor.cs 复制到项目的 Assets/Editor/ 文件夹。
3. 等待 Unity 编译完成。

## 场景准备
1. Hierarchy 右键 -> 2D Object -> Tilemap -> Rectangular，创建 Grid 和 Tilemap。
2. 将新 Tilemap 命名为 Ground_Auto，只用于自动生成的地面。
3. 在 Hierarchy 创建空物体 WuzhongMapGenerator，添加 WuzhongMapGenerator 组件。
4. 把 Ground_Auto 上的 Tilemap 组件拖到组件的 Ground Tilemap 字段。
5. 建议将 Grid 的 Cell Size 设为 (1, 1, 0)，并确认 Tilemap 的 Transform 缩放为 (1, 1, 1)。

## 添加资源
- Ground Sprites：拖入切片后的草地、泥地、地面 Sprite。
- Road Sprites：拖入石板路或泥路 Sprite。
- Border Sprites：可选，拖入边缘地面 Sprite。
- Buildings：添加房屋、祠堂、戏台等 Sprite；设置占格宽高和 Weight。
- Props：添加树木、灯笼、石碑、古井等 Sprite；设置数量、权重和占格大小。

## 生成
在组件 Inspector 下方点击“生成地图”。生成后可以在 Scene 中继续手工调整。
随机种子相同且 Randomize Seed Each Generation 关闭时，随机结果可复现。

## 注意
- 生成按钮会清空所分配 Ground Tilemap 上的所有 Tile。务必使用专用 Tilemap。
- 生成的建筑与装饰位于 GeneratedObjects 下；清理按钮只删除此父物体下的对象。
- 此版本是快速原型生成器：道路为简单的横向主街和概率性小路；建筑和装饰按权重随机放置，并以占格范围避免彼此重叠。生成后建议人工调整关键建筑的位置。
- 大型 Sprite 的 Pivot 会影响摆放效果。房屋、树木通常可在 Sprite Editor 中将 Pivot 设为 Bottom Center，再重新生成。
- 碰撞体选项会给每个生成物体添加 BoxCollider2D；复杂建筑建议关闭该选项，之后单独配置碰撞区域。
