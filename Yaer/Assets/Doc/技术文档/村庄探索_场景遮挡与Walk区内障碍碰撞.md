# 村庄探索：场景遮挡与 Walk 区内障碍碰撞 — 技术说明

| 项 | 内容 |
|----|------|
| **适用范围** | 村庄 `PlayerLocomotionMode.Village2_5D`、WalkArea 多边形内可走区、场景物体 Sprite 排序、区内实体障碍（横移 + 纵深 W/S） |
| **关联执行说明** | `Assets/Doc/执行文档/村庄场景物体图层遮挡_程序施工执行说明.md`、`Assets/Doc/执行文档/村庄WalkArea内部阻挡碰撞体_程序施工执行说明.md` |
| **Unity** | 2020.3.x（与工程主版本一致） |

---

## 1. 问题背景与分工

### 1.1 场景物体「前后遮挡」（Sorting）

村庄纵深使用**世界坐标 Y**；玩家侧已有按 Y 刷新 `sortingOrder` 的逻辑，但**未**在 `Default` / `SceneObject` 等 **Sorting Layer** 之间切换。任务需要在物体上按「玩家 Y vs 锚点 Y」切换 **SpriteRenderer** 的 Sorting Layer（及可选 Order），使 DNF 式前后关系正确。

### 1.2 Walk 区内障碍（Physics Layer + 纵深夹紧）

- **横移（A/D）**：`MoveComponent` 通过 `Rigidbody2D.velocity.x` 参与物理步，**非 Trigger** 障碍与 **`PlayerFoot`** 层碰撞矩阵配对后，可自然贴墙/顶停。
- **纵深（W/S）**：`TownPlayerLocomotion` 对 **`_villageWorldY`** 积分后**直接写回** `Rigidbody2D.position.y`，**不依赖** Y 向速度物理解算。因此**仅靠 Layer 碰撞无法挡住 W/S**；必须在写回前对权威 Y 做 **Cast / Overlap + 夹紧**。

---

## 2. 代码与资源索引

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs` | 场景物体：村庄模式下比较玩家与锚点**世界 Y**，切换 `Default` ↔ `SceneObject`，可选 Order；出村恢复初始 Sorting。 |
| `Assets/Scripts/Game/Static/Name/Settings/SortingLayerName.cs` | Sprite Sorting Layer 常量：`Default`、`SceneObject`（须与 `TagManager` → Sorting Layers 一致）。 |
| `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageWalkObstacleCollisionBootstrap.cs` | 运行时 `BeforeSceneLoad`：`VillageWalkObstacle` 在 Physics 2D 中**仅与 `PlayerFoot` 碰撞**，其余层 `IgnoreLayerCollision`。 |
| `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/Editor/VillageWalkObstacleCollisionMatrixMenu.cs` | 菜单 **Yaer → Physics2D → 应用村庄障碍层（仅碰 PlayerFoot）**，将同一策略写入工程设置（便于提交 `ProjectSettings`）。 |
| `Assets/Scripts/Game/Static/Name/Settings/LayerName.cs` | 物理 Layer 常量：`PlayerFoot`、`VillageWalkObstacle` 等。 |
| `Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs` | 村庄纵深移动；内含 **`enableVillageDepthObstacleClamp`** 及对 `VillageWalkObstacle` 的 **PlayerFoot Cast/夹紧**（W/S 阻挡）。 |
| `ProjectSettings/TagManager.asset` | 用户 Layer 中需存在 **`PlayerFoot`**、**`VillageWalkObstacle`**（名称与 `LayerName` 常量一致）。 |

---

## 3. 场景遮挡（VillageSceneObjectDepthSort）

### 3.1 挂载与配置

1. 在需要参与遮挡的物体上添加 **`VillageSceneObjectDepthSort`**。
2. **Target Sprite Renderers**：不填则 `Awake` 使用本物体单个 `SpriteRenderer`；多部件（树干/树冠）请列表逐项指定。
3. **Anchor Override**：可选；未指定时回退为 **`DepthComponent.FootCld` 中心 Y**（若存在）否则 **`transform.position.y`**。
4. **Player Logic Override**：可选；未指定则在 **OnEnable** 时 `FindGameObjectWithTag("Player")` **一次**并缓存（禁止每帧 Find）。
5. **Invert Player Versus Anchor Comparison**：若前后与美术直觉相反可勾选。

### 3.2 行为约定

- 仅在 **`PlayerInputComponent.LocomotionMode == Village2_5D`** 时生效；离开村庄模式时**仅在边沿**恢复进入场景时记录的 Layer/Order，避免非村庄场景每帧抢写。
- 玩家比较 Y：默认在 `TownPlayerLocomotion` **启用**时用 **`DebugAuthoritativeWorldY`**，否则 **`Rigidbody2D.position.y`**（与权威纵深一致；未使用 `spriteForDepthSort` 的 Y，详见脚本注释）。
- **与 `DepthComponent`**：二者都会改 `sortingOrder`，**不建议同开**；同开时控制台会打 `[VillageOcclusion]` 警告。

### 3.3 验收要点

- 世界 Y 语义一致（不把 Z 当纵深主比较量）。
- `sortingLayerName` 在 `Default` / `SceneObject` 间切换符合策划预期。
- 进村/出村、纯 A/D、纯 W/S、斜向无新增卡死；**不在**村庄脚本中写 **`Run`**（与 `02_SYSTEM_SPEC` 一致）。

---

## 4. Walk 区内障碍（Layer 矩阵 + 纵深夹紧）

### 4.1 策划摆关

| 对象 | Layer | Collider | Rigidbody2D |
|------|--------|-----------|-------------|
| 区内障碍 | **`VillageWalkObstacle`** | **非 Trigger**（Box/Capsule/Polygon 等） | **Static**（推荐） |
| 玩家用于被挡的碰撞体 | **`PlayerFoot`** | 与脚底/身体下缘一致 | 随玩家 Prefab |

障碍应放在 **WalkArea 多边形内部**；外边界仍由 WalkArea 脚本修正（第三阶段文档）。

### 4.2 碰撞矩阵

- **`VillageWalkObstacleCollisionBootstrap`**：游戏启动前对 `VillageWalkObstacle` 层逐层设置，**只与 `PlayerFoot` 不忽略**。
- **Editor 菜单**：执行一次后保存工程，便于在 **Project Settings → Physics 2D** 中目视矩阵与版本管理。

若未来需要障碍与**额外层**碰撞，须改 `ApplyPolicy()` 或改为纯手调矩阵并评估是否移除运行时引导。

### 4.3 纵深阻挡（TownPlayerLocomotion）

| Inspector 字段 | 说明 |
|----------------|------|
| **enableVillageDepthObstacleClamp** | 总开关（默认开启）。 |
| **villageDepthFootProbeOverride** | 用于 Cast 的 `Collider2D`；不填则在玩家子层级找**首个 Layer = PlayerFoot** 的 Collider。 |
| **villageObstacleContactSkin** | 命中后预留皮肤厚度，减轻贴边抖动。 |
| **villageObstacleDepthDebugLog** | 纵深被挡时打印 `[VillageBlockerDepth]`。 |

**算法概要（FixedUpdate 内）**：在 `_villageWorldY` 积分与 Y 边界 Clamp 之后、`WriteRootTransformWithAuthoritativeDepthY` 之前：

1. 用 **PlayerFoot** 探针对 **`VillageWalkObstacle`** 层（非 Trigger）做 **`Collider2D.Cast`**，沿纵深方向限制本帧位移；命中则截断位移并将 **`depthVelocity` 置 0**。
2. 若 Cast 未缩短但仍重叠（例如起点已在障碍内），使用 **`OverlapCollider` + 二分** 或 **小步挤出**。

### 4.4 常见问题

| 现象 | 可能原因 |
|------|-----------|
| A/D 能挡、W/S 能穿 | 未开纵深夹紧，或找不到 **PlayerFoot** 探针（看 `[VillageBlockerDepth]` 警告）。 |
| 完全无碰撞 | 障碍为 **Trigger**、障碍未在 **`VillageWalkObstacle`**、脚未在 **`PlayerFoot`**，或矩阵未生效。 |
| 贴边抖 | 略增 **`villageObstacleContactSkin`** 或加厚障碍 Collider。 |
| 与 `DepthComponent` 排序打架 | 遮挡物体禁用其一或只保留一种排序策略。 |

---

## 5. 替代方案（文档级）

| 方向 | 说明 |
|------|------|
| 纵深障碍仅用 WalkArea 裁切 | 不用区内薄墙，多边形维护成本高但无 Cast 成本。 |
| 纯物理挤出纵深 | 需把纵深改为 velocity 驱动，与现有 `TownPlayerLocomotion` 权威 Y 及 WalkArea 策略冲突面大。 |
| 遮挡仅用 `sortingOrder` 不切 Layer | 不满足「Default / SceneObject」任务口径时可不采用。 |

---

## 6. 变更与验证清单（PR 用）

**建议自测**

1. 村庄内前后走动：遮挡 Layer 与视觉一致；出村后物体 Sorting 恢复。
2. 区内障碍：薄墙、厚块各一类；**A/D** 与 **W/S** 均不可穿入障碍内部。
3. 非村庄场景：无 Missing Script；Physics 2D 矩阵对战斗/森林无未记录副作用。

**相关日志标签**

- `[VillageOcclusion]`：遮挡排序状态变化（需勾选组件内 Debug）。
- `[VillageBlockerDepth]`：纵深障碍夹紧（`TownPlayerLocomotion.villageObstacleDepthDebugLog`）。
- `[VillageWalkObstacle]`：Layer 名缺失时矩阵跳过（`VillageWalkObstacleCollisionBootstrap`）。

---

*文档版本：2026-05-11；汇总村庄遮挡组件、障碍 Layer 引导与 `TownPlayerLocomotion` 纵深夹紧的实现与使用说明。*
