# 村庄 Walk 障碍 — 方案 1：保留 Collider 几何、关闭脚↔障碍「物理碰撞」、阻挡仅走 Physics2D 查询

| 项 | 内容 |
|----|------|
| **文档性质** | 【施工员】执行说明：与策划/程序对齐「**方案 1**」的落地步骤、风险与验收；实施前建议通读 `00_MASTER_PROMPT.md` 模式分工 |
| **关联执行文档** | `优化VillageWalkObstacle判定问题_架构溯源与执行说明.md`（§0 已定「**不以纯物理独自挡人**」）、`0512/村庄WalkArea内部阻挡碰撞体_程序施工执行说明.md`（Walk 区内障碍与纵深脚本关系） |
| **主规范** | `Assets/Doc/02_SYSTEM_SPEC.md` §1（村庄 2.5D：世界 `Y` 为纵深、`Rigidbody2D`、重力为 0） |
| **Unity** | **2020.3.48f1**（与工程主规范一致） |

---

## 0. 方案 1 是什么（一句话）

**障碍物体上继续摆 `Collider2D`（策划仍用 Scene 线框定「挡哪里」），且障碍 **`isTrigger = true`**，从语义上标明「只参与逻辑/查询，不参与刚体硬碰」；同时在 Physics 2D 层矩阵里让 `PlayerFoot` 与 `VillageWalkObstacle` **不发生物理解算**（与 Trigger 策略一致，双保险）；**所有「挡不挡、停在哪」由脚本通过 `Physics2D` 的 Cast / Overlap / Distance 等查询，对着这些 Collider 几何来算。**

这样做的目的：**把「范围数据」和「阻挡语义」拆开**——Collider 仍是权威几何来源；**手感与精度**交给 C#；障碍用 **Trigger** 可避免与 **`Rigidbody2D.velocity`** 接触求解打架，并减少误用「非 Trigger + 矩阵」时与 Unity 查询默认行为的认知偏差（见 §3.2）。

---

## 1. 与现状代码的对应关系（施工前必读）

### 1.1 当前「脚 ↔ 障碍」为何会参与物理解算

- **`VillageWalkObstacleCollisionBootstrap`**（`RuntimeInitializeOnLoad`）在运行时把 **`VillageWalkObstacle` 层** 与 **`PlayerFoot` 层** 设为 **唯一互相不 Ignore** 的成对碰撞（其余层对障碍一律 Ignore）。  
- **结果**：玩家根上 **`Rigidbody2D` + `MoveComponent` 写入的 `velocity.x`** 会在物理步里与障碍 **非 Trigger** 体发生 **接触求解**（当前多数关卡资源如此），表现为横移被顶死、卡边等，与 **`TownPlayerLocomotion`** 里脚本写 **纵深 `Y`** 的路径**并行存在**，易产生「能上下不能左右」等复合现象。**方案 1 定稿后障碍改为 Trigger + 矩阵不碰**，从根上取消这条物理解算链。

### 1.2 方案 1 改完后，谁负责什么

| 层级 | 职责 | 说明 |
|------|------|------|
| **数据层** | 场景里 **`VillageWalkObstacle` 层 + `Collider2D`，且 `isTrigger = true`（正式口径，见 §3.2）** | **只定义阻挡域几何**；不参与刚体「挤开」；策划摆 Box / Capsule / Polygon，与现有工作流一致。 |
| **矩阵层** | **`PlayerFoot` 与 `VillageWalkObstacle` 在 2D 矩阵中改为不碰撞**（或运行时等价 `IgnoreLayerCollision`） | **不再**依赖引擎「挤开 / 顶住」玩家；**避免**与脚本写回 `position` 抢控制权。 |
| **行为层** | **`TownPlayerLocomotion`（纵深已有 Cast/Overlap）** + **待扩展的横向夹紧**（或独立 `VillageWalkObstacleResolver` 组件） | **所有阻挡语义**：沿移动方向 `Cast`、嵌入时 `OverlapCollider` + 分离、与 `WalkArea` 多边形修正顺序保持 §5 策略一致。 |

### 1.3 与《优化 VillageWalkObstacle 判定》文档的关系

- 同目录下 **`优化VillageWalkObstacle判定问题_架构溯源与执行说明.md`** 已把产品结论写为：**以脚本阻挡为主、Collider 定域**。  
- **本文档**把其中一条**可执行落地路径**固定为 **「方案 1」**：**明确关掉脚障物理碰撞**，并列出 **矩阵 / Bootstrap / 横向补逻辑** 的改法与验收。

---

## 2. 方案 1 的优点与代价（选型确认）

### 2.1 优点

- **策划友好**：仍用 Editor 摆 Collider，**不**要求先把关卡烘焙成自定义线段格式。  
- **与现有纵深逻辑一致**：`TownPlayerLocomotion` 已在用 **`ContactFilter2D` + `Cast` / `OverlapCollider` / `Physics2D.Distance`**；障碍改为 **Trigger** 后，施工时须把 **`BuildVillageObstacleContactFilter`（或等价写口）里 `useTriggers` 改为 `true`**，否则会出现「线框在却查不到」的假穿障（见 §3.2）。  
- **可解释性强**：阻挡结果来自 **显式 C# 分支**，便于打 `[VillageBlocker]` 类日志验收。

### 2.2 代价与必须补的坑

- **横移此前部分依赖「物理挡住」**：关掉碰撞后，若**不**在 **`MoveComponent` 写 `velocity` 之后**（或 **`Rigidbody2D` 位移前**）增加 **沿 `±X` 的 Cast 夹紧**，玩家可能 **穿障**；这是方案 1 的**必做配套**，不能只改矩阵。  
- **`ApplyVillageWalkObstacleFootPenetrationSeparation`** 当前依赖 **Overlap + Distance**；障碍为 **Trigger** 且 **`ContactFilter2D.useTriggers = true`** 后，须 **实机验证** Overlap / Distance 仍命中 **`VillageWalkObstacle` 层**（Unity 2020.3 各 API 对 Trigger 的默认排除规则以官方文档为准，**禁止**只改 Prefab 不改 Filter 导致纵深夹紧整段失效）。  
- **Editor 菜单与 Bootstrap 必须同源**：`VillageWalkObstacleCollisionMatrixMenu` 若仍写入「仅碰 PlayerFoot」，会与方案 1 冲突；需**同时**更新菜单文案与 `ApplyPolicy()` 策略（见 §3）。

---

## 3. 施工步骤（建议顺序）

### 3.1 产品确认（阻塞项）

- [ ] 策划确认：**村庄探索**下，障碍阻挡 **100%** 接受由脚本实现（含横移），**不再**依赖玩家脚与障碍的「硬碰硬」手感。  
- [ ] 程序确认：**战斗场景**若复用 `PlayerFoot` 层，**禁止**因改矩阵导致战斗穿模；若存在共用风险，须在 Layer 或子状态（仅村庄启用策略）上**拆分**（见 §4.2）。

### 3.2 障碍 Collider：Trigger 为正式口径（写入 PR / 策划表）

| 项 | 执行口径 |
|----|----------|
| **障碍本体** | 所有 **`VillageWalkObstacle` 层** 上的阻挡用 **`Collider2D.isTrigger = true`**。语义：**体积只供脚本查询，不当作刚体挡墙**。 |
| **Layer 矩阵** | **`PlayerFoot` 与 `VillageWalkObstacle` 不碰撞**（与 Trigger 叠加，避免将来有人改回非 Trigger 时又出现物理解算顶死）。Bootstrap / Editor 菜单与工程 `Physics2DSettings` **三处同源**。 |
| **`ContactFilter2D`** | **`useTriggers = true`**，且 **`SetLayerMask` 仍只含 `VillageWalkObstacle`**，避免把无关 Trigger 全扫进来。涉及文件（当前仓库）：`TownPlayerLocomotion.BuildVillageObstacleContactFilter` 等凡对障碍层做 **Cast / Overlap / Raycast** 的写口，**须统一改**，禁止只改一处导致纵深能挡、横移不挡。 |
| **Raycast 特例** | `Physics2D.Raycast` 系列若使用 **重载层掩码**而非 `ContactFilter2D`，须核对 Unity 2020.3 文档中 **对 Trigger 的默认行为**；不一致时**改为带 `ContactFilter2D` 的重载**或显式 **`QueryTriggerInteraction`**（若该 API 在所用重载中可用），避免脚底射线「穿透 Trigger 障碍」。 |
| **替代方案（不推荐作默认）** | 障碍 **`isTrigger = false`** + 仅矩阵 Ignore：与旧 `TownPlayerLocomotion` 默认 **`useTriggers = false`** 更对齐，但**易与「方案 1 语义」混淆**；若特殊关卡坚持非 Trigger，须在 PR 中单点说明并仍走 **脚本 Cast 夹紧**，不得依赖物理解算挡人。 |

### 3.3 修改 Physics 2D Layer 碰撞策略（核心）

- [ ] **运行时**：调整 **`VillageWalkObstacleCollisionBootstrap.ApplyPolicy()`**，使 **`VillageWalkObstacle` 与 `PlayerFoot` 也设为 Ignore**（即障碍层与**所有**层均不碰撞，或至少与 PlayerFoot 不碰撞 —— **以「脚不再与障碍发生物理解算」为验收**）。  
- [ ] **Editor**：更新 **`VillageWalkObstacleCollisionMatrixMenu`**，与 Bootstrap **同一策略**，便于提交 `ProjectSettings/Physics2DSettings.asset`。  
- [ ] **注释**：在 `LayerName.cs` 或 Bootstrap 文件头注释中写明：**方案 1 下矩阵不再表达「挡人」，只表达「减少误碰」**；**挡人**由 `TownPlayerLocomotion`（及扩展）负责。

### 3.4 横向阻挡：必做配套（否则方案 1 不完整）

在 **`PlayerLocomotionMode.Village2_5D`** 且 **`TownPlayerLocomotion.enabled`**（或与村庄横移等价的门控）时，增加**与纵深对称**的横向约束，建议优先级：

1. **以 `PlayerFoot` 探针 `Collider2D` 为形状**，对 **`VillageWalkObstacle` 层** `Cast(Vector2.right/left, …)`，命中则把本帧 **`Rigidbody2D.position.x`** 或 **`velocity.x`** 夹紧到接触前允许位移（** skin 常量**与纵深 `villageObstacleContactSkin` 同量级，避免抖动）。  
2. **与 `WriteRootTransformWithAuthoritativeDepthY` / `PostPhysicsResyncDepthCoroutine` 的执行顺序**对齐现有 `0512` §3.1、`TownPlayerLocomotion` 内 §5 策略注释：**先横后纵或先纵后横须单点文档化**，避免一帧内 X、Y 各写一次导致穿障。

**替代方案（文档级）**：横向夹紧放在 **`PlayerMoveComponent` 子类**仅在村庄模式调用 —— 侵入输入链，**不推荐**作首版；优先集中在 **`TownPlayerLocomotion`** 或 **`IVillageWalkObstacleClamp`** 单一写口。

### 3.5 纵深与穿透分离脚本的回归

- [ ] **`ApplyVillageWalkObstacleDepthClamp`**：障碍改 **Trigger** 且 Filter **`useTriggers = true`** 后，Cast / Overlap / 脚底射线须**仍能**命中障碍；打开 `villageObstacleDepthDebugLog` 做短测后关闭。  
- [ ] **`ApplyVillageWalkObstacleFootPenetrationSeparation`**：Trigger 下 **`Physics2D.Distance` / Overlap** 是否仍满足「防嵌入」须**实机确认**；若引擎对 Trigger–Trigger 距离语义不足，则改为 **纯 Cast 夹紧** 或 **沿法线 `MovePosition` 上限**（与 §2.2 一致，以实测为准）。  
- [ ] **Prefab / 场景批量检查**：现有 **`VillageWalkObstacle` 物体**若仍为 **非 Trigger**，须按 §3.2 批量勾选 **`Is Trigger`**，避免半套资源导致「有的能挡有的不能挡」。

### 3.6 文档与 OPEN_QUESTIONS

- [ ] 在 `Docs/OPEN_QUESTIONS.md`（若工程路径为 `Assets/Doc/...` 则按团队约定）记录：**战斗 / 非村庄场景**是否仍依赖旧矩阵。  
- [ ] 更新 `技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md` 中「矩阵挡人」表述，改为与方案 1 一致。

---

## 4. 风险与对策

### 4.1 穿障

- **原因**：仅关矩阵、未补横向 Cast。  
- **对策**：§3.4 必做；QA 用 **最大速度贴障边缘反复摩擦** 用例。

### 4.2 战斗与其它玩法共用 Layer

- **原因**：`PlayerFoot` 若在森林/战斗仍需要碰某层，全局 Ignore 可能误伤。  
- **对策**：方案 1 策略仅在 **`SetVillageExplorationMode(true)`** 时应用 **运行时 Ignore**，离村恢复；或 **村庄专用 Foot 子物体 Layer**（改动 Prefab，成本高）。**须在 PR 写清选型。**

### 4.3 性能

- **原因**：每帧对障碍层 Cast 次数上升。  
- **对策**：`ContactFilter2D` 单层掩码、复用 List、障碍数量可控；若瓶颈再议 **空间哈希**（非首版）。

### 4.4 Trigger 与剧情 / 拾取 / 其它 Trigger 叠在一起

- **原因**：村庄内常见 **对话圈、交互区** 亦为 Trigger；若与 **`VillageWalkObstacle` 同层或同物体多 Collider**，可能混淆 **OnTriggerEnter** 回调归属，或策划误把剧情 Trigger 挂在障碍层。  
- **对策**：**阻挡**与 **剧情 Trigger** **分物体、分 Layer**；障碍层 **仅**摆 Walk 阻挡；程序侧查询 **始终** `ContactFilter2D` **LayerMask = 仅 VillageWalkObstacle**，不把「全图 Trigger」扫进同一列表。

---

## 5. 验收清单（QA / 验收员）

| 编号 | 步骤 | 期望 |
|------|------|------|
| Q1 | 进村，沿障碍 **正面横移** | **不穿透**障碍 Collider 线框 |
| Q2 | 同一位置 **W/S 纵深** | 与改前策划预期一致，**无**异常穿出 WalkArea |
| Q3 | 障碍 **斜边 / 窄口**（Trigger + 脚本夹紧） | 无旧版「卡死只能贴边」类回归（允许手感变化但须可解释） |
| Q4 | **离村 / 切战斗场景** | 无穿模、无矩阵错误（依 §4.2 选型） |
| Q5 | 关闭调试日志后 **Console 无刷屏** | 仅保留必要 Warning |

---

## 6. 与其它方案的边界（避免重复立项）

| 方案 | 与方案 1 区别 |
|------|----------------|
| **方案 2（Kinematic 全程 MovePosition）** | 方案 1 **可仍用** `velocity.x` + Cast 夹紧；方案 2 则更多把横移也改为 **每帧解算后 MovePosition**。 |
| **方案 3（纯多边形/线段数据）** | 不再依赖 Unity Collider 摆关；工具链成本更高。 |

---

## 7. 主要代码入口索引（施工时快速跳转）

| 主题 | 路径 |
|------|------|
| 村庄纵深与障碍 Cast/Overlap/分离 | `Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs`（施工重点：`BuildVillageObstacleContactFilter` 须 **`useTriggers = true`**，与障碍 **Trigger** 定稿一致） |
| 运行时 2D 矩阵策略 | `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageWalkObstacleCollisionBootstrap.cs` |
| Editor 写矩阵菜单 | `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/Editor/VillageWalkObstacleCollisionMatrixMenu.cs` |
| Layer 常量 | `Assets/Scripts/Game/Static/Name/Settings/LayerName.cs` |
| 横移写入 | `Assets/Scripts/Game/GameRuntime/Entities/Component/Move/MoveComponent.cs`（`MoveVelocity` / `OnFixedUpdate`） |

---

**文档版本**：0514 方案 1 执行说明（Trigger 定稿修订）  
**维护建议**：代码合入后在本节追加 **PR 链接 / 合入日期 / `ContactFilter2D` 是否已全路径 `useTriggers=true` 确认人 / §4.2 矩阵恢复策略**。
