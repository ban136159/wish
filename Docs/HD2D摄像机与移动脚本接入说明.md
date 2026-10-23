# 《雾钟》HD-2D 摄像机与移动脚本接入说明

本次新增脚本用于逐步搭建 HD-2D 测试场景，不会替换现有 2D 场景。

## 新增脚本

- `Assets/Scripts/Camera/CameraFollow3D.cs`：用于 3D 场景，角色在 XZ 地面移动。
- `Assets/Scripts/Camera/CameraFollow2D.cs`：用于现有 XY 平面 2D 场景。
- `Assets/Scripts/Player/PlayerController3D.cs`：HD-2D 测试场景的基础移动控制器，使用 CharacterController。

## A. 在 HD-2D 测试场景中设置跟随摄像机

1. 新建或打开一个测试场景，例如 `HD2D_Village_Test`。
2. 创建地面：先使用一个 Cube 或 Plane，确保地面位于 Y=0 附近。
3. 创建空物体 `PlayerRoot`，将它放到地面上。
4. 给 `PlayerRoot` 添加 `CharacterController` 和 `PlayerController3D`。
5. 将角色的 SpriteRenderer 放到 `PlayerRoot` 的子物体 `PlayerVisual` 上。角色贴图应保持透明背景。
6. 选择 `Main Camera`，添加 `CameraFollow3D`，将 `PlayerRoot` 拖到 Target。
7. 初始 Offset 可用 `(0, 8, -8)`；摄像机会跟随目标并朝向角色。按画面构图调整 Offset。
8. 点击 Play，用 WASD 或方向键测试。

### 注意

- 这是 3D 场景：X/Z 是地面方向，Y 是高度。不要在同一个 PlayerRoot 上同时启用旧的 `PlayerController`（它使用 Rigidbody2D）和新的 `PlayerController3D`。
- 若想让按键方向相对摄像机，勾选 `Camera Relative Movement`。固定斜俯视角且希望按键始终对应世界方向时，可以不勾选。
- `CharacterController` 不会自动施加重力；本脚本适用于平地原型。后续有坡道、台阶或下落需求时，需要添加重力与地面检测。
- Sprite 的朝向切换尚由现有 2D 控制器管理；迁移到 3D 时，应把四方向切换逻辑改为读取 `PlayerController3D.MoveInput`，不要保留两个移动控制器。
- 本次代码尚未在 Unity Editor 中编译或运行验证，请在测试场景中确认组件、输入设置与摄像机效果。

## B. 在现有 2D 场景中设置跟随摄像机

1. 选择现有 `Main Camera`。
2. 添加 `CameraFollow2D`。
3. 将当前 `Player` 对象拖入 Target。
4. 默认 Offset 为 `(0, 0, -10)`，保持摄像机在角色后方。
5. 调整 Smooth Time：值越小跟随越紧，值越大越平滑。

2D 摄像机脚本只跟随 XY 平面，不适用于 HD-2D 的 XZ 地面场景。

## 推荐迁移顺序

先保留原 2D 场景作为备份，再在独立 HD-2D 测试场景中验证移动、镜头、碰撞、四方向 Sprite 与遮挡。确认运行正常后再迁移交互与剧情系统。
