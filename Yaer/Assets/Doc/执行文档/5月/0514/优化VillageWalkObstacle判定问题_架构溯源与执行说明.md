# 优化 VillageWalkObstacle 判定（严格对照 Collider 仍觉不准）— 架构溯源与执行说明

| 项 | 内容 |
|----|------|
| **文档性质** | 【架构侦探】逻辑溯源 + 【施工员 / 验收员】排查与验证指引（以当前仓库代码为准；侦探阶段**不**改代码） |
| **任务卡** | `Assets/Doc/任务卡/0514/优化VillageWalkObstacle判定问题.md` |
| **主规范** | `Assets/Doc/00_MASTER_PROMPT.md`（模式分工）、`Assets/Doc/02_SYSTEM_SPEC.md` §1（村庄 2.5D：`Y` 为纵深、`Rigidbody2D`、重力为 0） |
| **现象（验收口径）** | 在 **Scene 视图严格对照 Collider 线框**（**不依赖**脚底 `PlayerFoot` 的 Sprite——该探针通常**无 Sprite**）的前提下，仍感到 **`VillageWalkObstacle` 上 `PolygonCollider2D` 的阻挡与多边形边界不一致**（例如在线框外已被挡住、或停驻位置与线框直觉不符） |
| **Unity** | **2020.3.48f1**（与工程主规范一致） |
| **需求方向（已定）** | **碰撞体只负责界定阻挡范围**；**具体阻挡行为以代码脚本实现并持续优化**；**不采用「纯物理碰撞独自承担阻挡」为主方案**（已观察到会 **挤出 / 穿透** 等不稳定表现） |

---

## 0. 需求与目标架构（产品结论 · 写入执行口径）

以下作为后续 **【施工员】** 改代码时的 **验收目标**，与任务卡「优化判定」合并理解。

### 0.1 背景结论

- 在现有村庄纵深（权威 **`Y`** 多由脚本积分并写回、另有 WalkArea 几何修正等）与 **2D 碰撞矩阵** 组合下，**仅依赖引擎默认的纯物理接触** 来「挡住」玩家时，仍会出现 **被挤出障碍、穿出边界一侧** 等 **不可靠** 现象，**阻挡功能当前做得不好**。  
- 因此团队 **明确转向**：**以脚本实现的阻挡为主路径**，而不是把正确性押在「纯物理 + 矩阵」上。

### 0.2 正式需求（两层分工）

| 层级 | 职责 | 说明 |
|------|------|------|
| **数据层：碰撞体** | **`VillageWalkObstacle` 上配置的 `Collider2D`（含 `PolygonCollider2D`）** | **只作为「阻挡域 / 范围」的几何来源**：顶点、Offset、Edge Radius 等均在 Scene 里可调；**运行时以 Collider 几何为权威**，不在脚本里硬编码另一套「看不见的多边形」替代关卡摆放。 |
| **行为层：代码脚本** | **`TownPlayerLocomotion` 纵深路径上的 Cast / Overlap / 夹紧 / 挤出等**（可扩展为独立组件，但须遵守现有架构） | **具体「挡不挡、停在哪、如何避免挤出、与 WalkArea 修正的先后顺序」等**，**由 C# 显式实现并优化**；目标是在 **严格对照 Collider 线框** 的前提下，**手感稳定、边界可解释**。 |
| **辅助：Layer 矩阵** | **`VillageWalkObstacleCollisionBootstrap` + ProjectSettings** | **继续承担「与其它 Layer 隔离、避免误碰」** 的工程职责；**不作为**「唯一阻挡实现」——即矩阵解决 **谁和谁可能发生物理事件**，**不保证**纵深玩法下的 **语义正确阻挡**。 |

### 0.3 施工边界（避免与主规范冲突）

- 仍使用 **Unity 2020.3.48f1**、**村庄纵深沿世界 `Y`**（见 `02_SYSTEM_SPEC.md` §1）；**不在 `Update` 里无节制堆业务**，优先在 **`FixedUpdate` / 既有纵深写回点** 上收敛逻辑。  
- **替代方案（文档级）**：若未来要引入 **更精确的距离场 / 最近点投影** 等，仍应以 **障碍 `Collider2D` 几何** 为输入，**不**与「Collider 定域」原则矛盾。

---

## 1. 结论摘要（按当前反馈校正）

### 1.1 观察前提（与任务描述对齐）

- **`PlayerFoot`** 在工程中主要承担 **脚底探针 / 可走检测** 等职责，通常只有 **`Collider2D`**，**没有**用于对比边界的 Sprite；因此「准不准」应建立在 **Collider 几何 vs Collider 几何** 上，而不是「贴图轮廓」。

### 1.2 工程里实际存在「两条路径」（排查时必须拆开）

| 路径 | 作用 | 与「纯 Unity 碰撞关系」的关系 |
|------|------|-------------------------------|
| **A. Physics 2D Layer 碰撞矩阵** | 决定 **哪些 Layer 之间** 在引擎里**允许**发生 2D 碰撞/接触 | **工程隔离层**：本仓库由 **`VillageWalkObstacleCollisionBootstrap`** 在 **`BeforeSceneLoad`** 对 **`VillageWalkObstacle`** 与各层调用 **`Physics2D.IgnoreLayerCollision`**，使障碍层 **理论上仅与 `PlayerFoot` 发生物理事件**、与其余层忽略；**不负责**「纵深玩法下稳定挡人、不挤出」（见 §0）。 |
| **B. `TownPlayerLocomotion` 纵深夹紧** | 在写回 **`Rigidbody2D.position.y`** 之前，用 **`PlayerFoot` 上的 `Collider2D`** 对障碍层做 **`Cast` / `OverlapCollider` + 二分 / 挤出**，主动改写 **`_villageWorldY`** | **产品主路径（须持续优化）**：以 **障碍 `Collider2D` 几何** 为范围输入，**用脚本定义「停在哪、如何防挤出」**；实现细节（世界 ±Y Cast、`villageObstacleContactSkin`、临时 `SyncTransforms` 等）可能导致与线框直觉偏差，属 **本任务施工重点**。 |

**因此**：若你**严格按线框**仍觉得不准，排查阶段仍建议 **拆开 A（矩阵）与 B（脚本）** 做对照；**产品路径上** 已确定 **以 B 为主做优化**（见 **§0**），**A** 负责 **隔离误碰** 与 **核对 Layer 是否错位**。

### 1.3 为何「运行时 2D 碰撞矩阵」值得优先深挖

- **设计意图上**：「障碍只挡脚底」本身 **完全可以用 Project Settings → Physics 2D 的 Layer Collision Matrix 固化**；`VillageWalkObstacleCollisionMatrixMenu` 也是把同一策略 **写进工程设置** 的 Editor 入口。  
- **`VillageWalkObstacleCollisionBootstrap` 的潜在风险点（文档级假设，需用 Play 模式 + Physics 调试验证）**：  
  - **与其它 `RuntimeInitializeOnLoadMethod` 的执行顺序**：若工程里另有代码在 **`BeforeSceneLoad` / `AfterSceneLoad`** 等阶段 **再次改写 Layer 碰撞**，最终矩阵可能与「你在 Editor 里看到并保存的那份」不一致。  
  - **「只改 Obstacle 这一行」**：Bootstrap 只遍历 **「Obstacle 对每个 i」** 的 Ignore 位；**不会**反向约束 **`PlayerFoot` 对其它层** 的勾选。一般这没问题，但若关卡里 **误把障碍挂在非 `VillageWalkObstacle` 层**、或 **Foot 探针实际 Layer 与预期不符**，你会看到 **与 Polygon 线框「对不上」的假象**（本质是 **参与碰撞的并不是你以为的那一对 Layer**）。  
  - **运行时覆写 vs 仅使用磁盘矩阵**：若团队期望 **「完全不要运行时改矩阵」**，则应评估 **删除 Bootstrap、只保留 ProjectSettings 手调 + 版本提交** 是否与现有流程冲突（见 §6）。

- **与 §0 的分工**：**施工主战场** 在 **脚本阻挡（路径 B）**；本节（矩阵）主要用于 **验收与排障**，避免 **错层 / 矩阵被覆写** 造成「脚本已优化仍对不齐线框」的假问题。

### 1.4 与「纯物理阻挡」的关系（与 §0 对齐）

- **仅矩阵 + 引擎默认物理解析**，在当前 **脚本主导纵深写回** 的流程下，**仍不足以** 构成稳定、可验收的阻挡：除「线框与体感不一致」外，还已观察到 **纯物理路径下有时会把角色挤出障碍外** 等失败模式。  
- **当前结论**：**不把「纯物理独自挡人」作为目标**；**以 Collider 为范围数据源 + 脚本实现阻挡语义**（§0.2）。若仍有人尝试「纵深全改 `velocity.y` 只靠碰」，应单独立项评审 **与 WalkArea / `MoveComponent` 的耦合**，**不在本任务卡默认范围内**。

---

## 2. 逻辑溯源（代码链路）

### 2.1 Layer 定义

- **`LayerName.PlayerFoot`**、**`LayerName.VillageWalkObstacle`** 常量定义见 `Assets/Scripts/Game/Static/Name/Settings/LayerName.cs`。

### 2.2 运行时 2D 碰撞矩阵（**重点文件**）

- **`VillageWalkObstacleCollisionBootstrap.ApplyPolicy()`**：对每个 `i in [0,31]`，调用 **`Physics2D.IgnoreLayerCollision(obstacle, i, !shouldCollide)`**，其中 **`shouldCollide == (i == footLayerIndex)`**。  
- **含义**：**`VillageWalkObstacle` 层** 在运行时结束时，应表现为 **只与 `PlayerFoot` 层碰撞**、与其余层 **全部忽略**。  
- **Editor 对齐入口**：`VillageWalkObstacleCollisionMatrixMenu` — 与 Bootstrap **同一套策略**，用于把矩阵 **写入工程** 以便版本管理。  
- **代码路径**：`Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageWalkObstacleCollisionBootstrap.cs`、`.../Editor/VillageWalkObstacleCollisionMatrixMenu.cs`。

### 2.3 纵深障碍夹紧（脚本路径 B · **与 §0 主方案一致**）

- **`TownPlayerLocomotion.ApplyVillageWalkObstacleDepthClamp`**：在 **`enableVillageDepthObstacleClamp`** 为真时，对 **`PlayerFoot`** 解析到的 **`Collider2D`** 使用 **`ContactFilter2D`**（**非 Trigger**、**LayerMask 仅障碍层**）做 **Cast / Overlap**，并可能改写 **`_villageWorldY`** 与 **`depthVelocity`**。  
- **与矩阵的关系**：Cast/Overlap **只查询** 障碍层几何；**Layer 矩阵** 保证查询对象与环境 **Layer 组合正确**。**「挡稳、不挤出」** 的语义由 **本段脚本** 负责，不在矩阵层自动完成。  
- **代码路径**：`Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs`（纵深障碍相关字段与 `ApplyVillageWalkObstacleDepthClamp` 一带）。

### 2.4 WalkArea 多边形修正（与障碍无关的另一条几何）

- **`ApplyVillageWalkPolygonPostCorrection`**：用 **WalkArea** 的 **`PolygonCollider2D`** 把 **`Rigidbody2D.position`** 收进可走区；与 **障碍层** 是 **不同 Collider / 不同语义**。若感到「边界诡异」，需在 Scene 里 **同时显示 WalkArea 与 Obstacle** 两套线框，避免把 WalkArea 的收边误判成障碍 Polygon 的「膨胀」。

---

## 3. 分项排查清单（严格 Collider 口径）

> 建议顺序：**Play 下确认矩阵与 Layer（排除假缺陷）** → **在 §0 框架下优化脚本路径 B**（必要时用 B4 做诊断）→ 再查 Polygon 资源真值（B6）。

| 序号 | 检查项 | 说明 |
|------|--------|------|
| B1 | **障碍物体、Foot 物体在 Play 时的实际 Layer** | 与 `LayerName` 常量字符串是否一致；**错层**会直接导致「你以为在看 Polygon A，实际碰的是别层逻辑」。 |
| B2 | **运行时矩阵 vs ProjectSettings 静态矩阵** | 用 Editor 菜单写入后保存工程；进 Play 后在 **Physics Debugger / Layer Collision Matrix** 观察 **`VillageWalkObstacle` 行** 是否仍 **仅勾选 `PlayerFoot`**；若 Play 前后不一致，重点查 **Bootstrap 与其它初始化** 的顺序与覆写。 |
| B3 | **是否存在其它脚本调用 `Physics2D.IgnoreLayerCollision`** | 全局搜索；若有，记录 **调用时机** 是否与 Bootstrap 冲突。 |
| B4 | **对照实验：关闭 `enableVillageDepthObstacleClamp`** | 用于 **定位当前 B 实现的问题点**（皮肤、Cast、二分、挤出步长等）；**不**再解读为「应关掉脚本改纯物理」——**产品方向已定为脚本主阻挡**（§0）。关闭后若穿透加剧，反证 **脚本层必须补强**。 |
| B5 | **`villageObstacleContactSkin` 与 Foot 形状** | **非 Sprite 问题**：Foot **Collider 的真实大小/偏移** 决定 Cast/Overlap；**skin 过大** 会在数值上 **提前** 停住，**体感像「比 Polygon 更早被挡」**。 |
| B6 | **障碍 `PolygonCollider2D` 自身** | **Offset、Scale、Edge Radius、同一物体多个 Collider** 仍会导致 **线框本身** 比你记忆中的「边界」更大——这一条是 **Collider 资源真值**，与是否有 Sprite 无关。 |
| B7 | **WalkArea 与 Obstacle 线框同时显示** | 排除 **WalkArea 收边** 对「障碍边界」判断的干扰。 |

---

## 4. 验收操作（【验收员】）

1. **矩阵快照**：进入 Play 前/后各截一张 **Physics 2D Layer Collision** 中 **`VillageWalkObstacle`** 行的配置（或使用 Unity 内置 Physics 调试视图），核对 **是否仅与 `PlayerFoot` 碰撞**。  
2. **路径 B 开关对照**：在同一复现点，切换 **`enableVillageDepthObstacleClamp`**，对比 **阻挡发生瞬间** 的 **Foot 线框与障碍 Polygon 线框** 的几何关系是否发生变化。  
3. **日志**：需要脚本侧证据时，临时开启 **`villageObstacleDepthDebugLog`**，关注 **`[VillageBlockerDepth]`** 分支（用完请关）。  
4. **记录**：**物体名、Prefab、Layer 索引、是否有多处改矩阵、`contactSkin`、B 开关状态**，便于施工闭环。

---

## 5. 施工方向（【施工员】；合入前评审）

> **与 §0 对齐**：优先 **优化脚本阻挡（路径 B）**；矩阵（路径 A）以 **正确隔离 + 可复现** 为底线，不作为「挡人」的唯一实现。

| 优先级 | 方向 | 说明 |
|--------|------|------|
| **P0** | **以 Collider 几何为输入，修正脚本阻挡语义** | 在 **`TownPlayerLocomotion.ApplyVillageWalkObstacleDepthClamp`**（或抽出的专用组件）内，针对 **挤出、提前停、与线框不一致** 做 **最小增量修复**：例如 Cast 命中筛选、`contactSkin` 与二分精度、`TryDepenetrate` 步长与方向偏好、与 **`ApplyVillageWalkPolygonPostCorrection`** 的帧内顺序等；**禁止**在关卡侧复制一套「隐形多边形」替代障碍 Collider。 |
| **P1** | **矩阵与 Layer 可观测性** | 保持 **Bootstrap / ProjectSettings** 与 §4 验收一致，排除 **错层、Play 时矩阵被覆写** 导致的「脚本算对、输入几何却错了」的假问题。 |
| **P2** | **可选：Foot 探针专用化** | 若 **`PlayerFoot`** 同时服务多种检测且形状过大，在架构允许下考虑 **纵深专用子 Collider**（仍挂在 `PlayerFoot` 层或统一由 `ContactFilter` 约束），**不改变**「范围仍来自障碍 Collider」的原则。 |

**重要**：§0 已锁定 **「Collider 定域 + 脚本挡」**；若对 **挤出容忍度、是否允许沿障碍滑移** 等仍不澄清，再记入 **`Docs/OPEN_QUESTIONS.md`**，**勿擅自改核心方向**。

---

## 6. 替代方案说明（文档级）

| 方案 | 说明 |
|------|------|
| **主方案（与 §0 一致）** | **障碍 `Collider2D` 提供范围 + 脚本实现阻挡**；矩阵负责 **Layer 隔离**。 |
| **仅 Project Settings 矩阵、不跑 Bootstrap** | 减少运行时覆写；需团队自律 **提交正确矩阵**。 |
| **Bootstrap + Editor 菜单定期落盘** | 与现仓库一致；注意 **与其它 `IgnoreLayerCollision` 调用的顺序**。 |
| **纵深改为纯 `velocity.y` 物理解算并弱化脚本 B** | **不推荐作为本任务默认**：与 WalkArea / 现有写回链耦合大，且 **已观察到纯物理挤出**；若单独立项需单独评审与验收集。 |
| **在脚本内用数学形状替代关卡 Collider** | **不推荐**：违背「范围以场景碰撞体为准」，维护成本高。 |

---

## 7. 参考索引（速查）

| 主题 | 路径 |
|------|------|
| 纵深障碍夹紧（路径 B） | `TownPlayerLocomotion.cs` |
| 运行时 Layer 矩阵（路径 A） | `VillageWalkObstacleCollisionBootstrap.cs` |
| Editor 写矩阵 | `VillageWalkObstacleCollisionMatrixMenu.cs` |
| Layer 常量 | `LayerName.cs` |

---

**文档版本**：2026-05-14；**修订**：  
1）PlayerFoot 无 Sprite、严格 Collider 观察、排查时拆开矩阵与脚本；  
2）**已定需求**：纯物理不可靠（含挤出）→ **碰撞体界定范围 + 脚本实现阻挡为主**；施工与替代方案与 §0 对齐。
