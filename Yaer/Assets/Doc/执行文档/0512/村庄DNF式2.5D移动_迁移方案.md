# 村庄（Village_KenMuNi1）DNF 式 2.5D 移动 — 架构迁移方案（施工版）

| 项 | 内容 |
|----|------|
| 文档版本 | **v1.3 — 2026-05-08（施工员交付版）** |
| **纵深轴修正（实测）** | 本项目为 **2D（`Rigidbody2D`）环境**，DNF 式「前后纵深」应映射为 **世界 Y 轴位移**，**不得**再作为主方案驱动 **世界 Z**。`LocomotionMode.Village2_5D` 在本文档中一律按 **Y 纵深** 理解（若代码里枚举名未改，以本文语义为准）。 |
| 已定稿方案 | **方案 C**：新建 **`TownPlayerLocomotion`** + **入村启用 / 离村关闭**；**不**在 `MoveComponent` 内堆村庄分支作为主路径 |
| 文档用途 | 发给施工员：实现依据、验收口径、溯源背景、风险与回归范围 |
| 实现范围说明 | 本文档为**设计与约束**；具体脚本接口、序列化字段、PR 粒度由施工员拆分，但须满足下文章节中的**验收标准** |

---

## 0. 施工员必读：一页纸摘要

1. **做什么**：仅在 **`Village_KenMuNi1`** 内，**W/S（或等价 Vertical 轴）驱动世界 Y 上的「纵深带」位移**（惯性 + 摩擦，类 DNF 在 2D 下的映射）；同时 **禁用蹲下、普攻、重击、冲刺攻击**（交互建议保留；**跳跃默认建议关闭**，见第 3.5 节与重力冲突说明）。  
2. **怎么做**：新增 **`TownPlayerLocomotion`**，与 **`PlayerMoveComponent` / `MoveComponent`** 约定 **谁写 `Velocity`/谁写纵深 Y、FixedUpdate 顺序**；村庄模式在 **`PlayerInputComponent`** 侧 **过滤战斗指令**（`LocomotionMode`，见第 4.3 节）。  
3. **Z 轴约束**：村庄纵深模式下 **`transform.position.z`（或等价根节点 Z）应保持不变**；若需镜头/层级微调，**仅限非位移主路径的辅助手段**（例如相机子轨道），且须在 PR 说明中写明。  
4. **必须先排雷**：**Y 纵深与 `MoveComponent` 重力共用 `Velocity.y`** 的冲突（第 3.5 节）；**`DepthComponent` 已按 Y 排序**（第 3.6 节）；地面 **`GroundLayerMask`** 见 **第 3.3.1 节**。  
5. **不要做什么**：不要把战斗纵深写进 **Z** 主位移；不要无设计地把纵深 Y 与跳跃重力叠在同一 `Rigidbody2D.velocity.y` 上导致手感崩坏。

---

## 1. 验收标准（施工完成判定）

| ID | 验收项 | 通过条件 |
|----|--------|----------|
| AC-01 | 场景限定 | **仅**在 `Village_KenMuNi1` 时 `TownPlayerLocomotion` **启用**；离开村庄后 **完全不生效**，战斗关卡行为无回归 |
| AC-02 | **Y 向纵深** | 按住 W/S（或 Vertical），角色在 **纵深方向的世界 Y** 上 **连续变化**（惯性 + 摩擦），无单帧瞬移 |
| AC-03 | **X 与纵深 Y** | **A/D** 仍驱动现有 Home 横向逻辑；**纵深由 Y 实现**；**根 Z 不变**（或符合第 0 节「辅助」约定）；无与 `MovePosition`/速度写入的无序打架 |
| AC-04 | 输入裁剪 | 村庄内 **蹲下、普攻、重击、冲刺攻击** 不触发；**S 与 W/S** 若键位冲突须按第 5 节分流 |
| AC-05 | 边界 | **纵深 Y** 在 **YMin/YMax**（或等价 Collider / SO）内 Clamp |
| AC-06 | 渲染遮挡 | 纵深移动时 **与场景物体前后关系正确**；须说明是否依赖现有 **`DepthComponent`（基于 Y）** 或额外规则（第 3.6 节） |
| AC-07 | 地面 | **落地检测正常**；若纵深改 Y 导致 **`CapsuleGroundChecker` / `root` 偏移** 异常，须在 PR 中写明调整 |
| AC-08 | 回归 | Forest 等战斗场景 **零新增报错** |

---

## 2. 背景与产品约束

### 2.1 进村流程（已通）

- 地图：`MapFormLogic` → `ButtonJingLingVillage` → **`SceneName.Village_KenMuNi1`**。  
- 场景：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`。

### 2.2 技术约束

- Player 与战斗关卡 **共用实体架构**，村庄能力必须 **可开关、可拆除**。

### 2.3 坐标语义（定稿）

| 轴 | 村庄 `Village2_5D` 模式下 |
|----|---------------------------|
| **X** | 左右移动（既有 `Rigidbody2D` / Home 状态机） |
| **Y** | **DNF 式纵深（W/S）+ 现有重力/跳跃（若启用跳跃须见 3.5 隔离策略）** |
| **Z** | **保持静止**；不作纵深主位移；仅允许文档第 0 节所述的辅助用途 |

---

## 3. 现状溯源（施工员理解用）

### 3.1 移动相关核心类（与 `FightingFormLogic` 无关）

| 组件 / 类 | 路径参考 | 职责 |
|-----------|-----------|------|
| `PlayerInputComponent` | `.../Player/Components/` | 指令队列、`moveInputFuncDict` / `controlInputFuncDict` |
| `PlayerMoveComponent` → `MoveComponent` | `.../Component/Move/` | `Velocity.x/y`、**重力作用于 `Velocity.y`**、`Rigidbody2D.velocity` |
| `PlayerLogic` | `Entities/Player/` | `SetPos`、Animator 切换等 |
| `PlayerCsAnimator` + `HomeWalkState` 等 | `Components/CsAnimator/` | 左右输入 → `SetWalkSpeed` 等 |
| `CapsuleGroundChecker` | 与 `PlayerMoveComponent` 同物体 | **`GroundLayerMask`** + `CapsuleCast`（第 3.3.1 节） |

### 3.2 物理与「深度」语义（已修正）

- 位移主力：**`Rigidbody2D`，平面为 XY；其中 `Velocity.y` 已被重力与跳跃占用。**  
- **`DepthComponent`**（场景对象常用）：用 **`transform.position.y`** 推 **`SpriteRenderer.sortingOrder`**。  
- **结论**：在本项目 2D 管线中，**纵深应走 Y**，与现有深度排序的「读 Y」**天然一致**；此前文档中「改 Z 再改排序」的路线 **不适用为主方案**。

### 3.3 地面 Layer（施工前必查）

- `Player.prefab`：`GroundLayerMask.m_Bits = 1064960` → **`GroundCenter` + `GroundCommon`**（以当前 `TagManager` 为准）。  
- `Village_KenMuNi1` 大量物体在 **Layer 8（Map）**；若脚底下可走面只在 Map，**胶囊检测可能失败**。优先 **对齐场景 Layer** 或文档化扩展 Mask。

### 3.3.1 Player 预制体：`GroundLayerMask` 挂接

| 项 | 说明 |
|----|------|
| 执行检测 | **`CapsuleGroundChecker`** → `Physics2D.CapsuleCast(..., GroundLayerMask)` |
| 挂点 | 与 **`PlayerMoveComponent`** **同一子物体**（`Player` → `Components` → **`PlayerMoveComponent`**） |
| 绑定 | `MoveComponent.OnInit`：`GetComponent<BaseGroundChecker>()` + `Init(this.root)` |
| `Detect/GroundDetect` | **仅占位 Transform**；检测几何以 **`MoveComponent.root` + `GroundCheckOffset`** 为准。**纵深改 Y 后须复测**胶囊与脚底是否仍落在可走面上 |

### 3.4 输入默认键位

- 默认 **`C` = 蹲**；若键位把蹲绑到 **S**，与 **W/S 纵深** 冲突，须村庄分流。

### 3.5 **Y 纵深 vs 战斗重力系统（重新评估）**

`MoveComponent` 在 **`OnFixedUpdate`** 中：未落地时 **`Velocity += Gravity * dt`**（重力作用于 **y**），落地后将 **`moveSpeedY` 归零**；水平与竖直速度最终写入 **`rg.velocity`**。

| 风险 | 说明 |
|------|------|
| **同一 `Velocity.y`** | 若 `TownPlayerLocomotion` 与 `MoveComponent` **同时写 `rg.velocity.y`**（纵深「走」+ 重力/跳），会出现 **不可叠加的互相覆盖**，手感与落地判定易崩。 |
| **纵深沿 Y「平移」与「跳跃抛物线」语义冲突** | 在 DNF 式村庄里，若仍要 **Space 跳跃**，须二选一或组合： **(A)** 村庄 **关闭跳跃**，纵深仅允许在 **贴地条带**内改 Y；**(B)** **视觉纵深**写在 **子节点本地 Y**，**刚体世界 Y** 仍只服务跳跃/重力（`DepthComponent` 若挂在子物体或需改为读「脚点合成 Y」）；**(C)** 村庄用 **独立 Kinematic 方案** 仅改展示层（工作量大，见替代方案）。 |

**施工默认推荐**：**村庄关闭跳跃**（与文档 v1.2 默认一致），纵深 Y 由 **`TownPlayerLocomotion` 与 `MoveComponent` 协调后的单写口** 驱动（具体由实现 PR 说明：例如仅当 `IsGrounded` 且村庄模式时合并纵深速度到 `Velocity.y`，或子节点方案）。

### 3.6 **Y 纵深 vs Sorting / 遮挡（重新评估）**

- **`DepthComponent`**（`Entities/Component/Physics/DepthComponent.cs`）：`sortingOrder ≈ f(-transform.position.y)`。  
- **若纵深位移直接改变玩家（或 `DepthComponent` 所读 Transform）的世界 Y**：**排序会随 Y 自动变化**，与「走近物体则遮挡关系变化」**一致**，一般 **有利于** 2.5D 观感。  
- **注意**：若 **启用跳跃** 且跳跃也大幅改变同一 Transform 的 Y，**排序会在起跳/落地过程中连续变化**；需美术确认是否接受；若不接受，应把 **排序采样点** 与 **抛物线顶点** 解耦（例如 `DepthComponent` 改为读 **子节点「脚底/腰带」** 或 **仅地面条带的 Y」**）。  
- **Z 静止**：不改变 Z 时，**Sorting Layer 不因 Z 变化**；遮挡关系 **主要由 Y（及现有 Sorting Layer 配置）** 决定。

---

## 4. 方案定稿：方案 C（新建组件 + 薄侵入）

### 4.1 不推荐

| 方案 | 结论 |
|------|------|
| A. 在 `MoveComponent` 内硬编码村庄纵深 + 惯性 | **禁止作为主实现** |
| B. 仅扩 Animator | **不足** |
| D. 主纵深仍走 **世界 Z** | **与实测结论相悖**；2D 主流程下 **不作为村庄主方案** |

### 4.2 方案 C：要点

- **`TownPlayerLocomotion`**：负责 **Y 向纵深** 的速度目标、摩擦、边界 Clamp；**不写 Z**（或仅写辅助节点且 AC-03 可验）。  
- 与 **`MoveComponent`** 明确 **FixedUpdate 顺序** 与 **`Velocity.y` 单写者规则**（见第 3.5 节）。  
- **`LocomotionMode`**：`Default` / **`Village2_5D`**（语义 = **Y 纵深村庄**，非 Z）。

### 4.3 `PlayerInputComponent`

- 在 **`Village2_5D`** 下过滤 `Squat`、各类攻击等（PR 附 `ControlInputType` 表）。

### 4.4 `DisablePlayerMove` 边界

- 仍建议 **`SetVillageExplorationMode(bool)`**：只关战斗向，**保留左右 + 纵深 Y**。

---

## 5. 功能裁剪清单（村庄内）

| 能力 | 村庄默认 |
|------|----------|
| 蹲下 / 攻击 | **关** |
| 左右 | **开** |
| **W/S → 纵深 Y** | **开** |
| 跳跃 | **默认关**（规避 3.5 节）；若策划强开须选定 **子节点纵深** 或 **单写口协议** |
| 交互（E） | **建议开** |

---

## 6. Animator

- Home：`Walk`、`Idle`、`IdleSubState*`。  
- **建议**：`|Velocity.x| + |纵深 Y 速度| > 死区` → `Walk = true`（参数进 Inspector/SO）。

---

## 7. Sorting 与渲染（本节替代原「Z 排序三选一」）

- **主路径**：依赖现有 **`DepthComponent` 基于 Y 的排序** 即可与 **Y 纵深** 对齐；**无需**为纵深再单独走 Z。  
- **若采用第 3.5 节子节点纵深**：须同步评估 **`DepthComponent` 挂载在哪个 Transform** 上，避免「身体在子节点动、排序仍读根节点 Y」导致遮挡错误。  
- **禁止**：以 **主位移改 Z** 实现村庄纵深并与本文定稿冲突的合入（除非单独变更需求并经评审）。

---

## 8. **Y 边界（Boundary）**

- 使用 **`YMin` / `YMax`**（空物体、触发器、SO 引用等）；**不再使用 ZMin/ZMax 作为纵深主边界**。  
- 验收：**AC-05**。

---

## 9. 推荐施工顺序

1. 地面 Layer / `GroundLayerMask`（**AC-07**）。  
2. `LocomotionMode` + 输入过滤（**AC-04**）。  
3. `TownPlayerLocomotion`：**Y 纵深 + 摩擦 + 边界**，并落实 **第 3.5 节** 与 `MoveComponent` 的协议（**AC-02、AC-03、AC-05**）。  
4. Sorting / `DepthComponent` 读点确认（**AC-06**）。  
5. 入村启用 / 离村关闭（**AC-01、AC-08**）。

---

## 10. 预计改动面

| 类型 | 对象 |
|------|------|
| 新增 | `TownPlayerLocomotion.cs`（建议路径同前版） |
| 门控 | `PlayerInputComponent.cs`、`PlayerLogic` 或 `PlayerExplorationFlags` |
| 开关 | `Village_KenMuNi1` 对应 Procedure / GSM |
| 预制体 / 场景 | `Player.prefab`、`Village_KenMuNi1.unity`（**Y** 边界） |
| 可选 | `DepthComponent` 或排序读点调整（仅当采用子节点纵深时） |

---

## 11. 风险与回归测试（自测表）

| 风险 | 自测 |
|------|------|
| `Velocity.y` 打架 | W/S + 松手摩擦 + 若开跳跃交叉测 |
| `DepthComponent` 读错 Transform | 走近遮挡物前后关系录屏对比 |
| `SetPos(Vector2)` | 传送/剧情后 **Y 纵深条带** 与 **Z 不变** 是否仍满足 |
| `Walk` 抖动 | 仅横向、仅纵深、斜向各 30s |
| 误开村庄模式 | Forest 全战斗操作回归 |

---

## 12. 结论表

| 问题 | 结论 |
|------|------|
| 纵深轴 | **世界 Y**；**Z 静止**（或仅辅助） |
| 主方案 | **方案 C** + `LocomotionMode.Village2_5D` = **Y 纵深语义** |
| 重力 | **`Velocity.y` 单写者须设计**；默认 **关跳 + 贴地纵深** 成本最低 |
| Sorting | **现有 Y 驱动排序与 Y 纵深一致**；子节点纵深时须改 **读点** |
| 地面 | **`CapsuleGroundChecker` + `GroundLayerMask`**（第 3.3.1 节） |

---

## 13. 维护说明

- 若改 **`GroundLayerMask`** 或 `TagManager`，同步第 3.3、3.3.1 节。  
- **若代码枚举仍名 `Village2_5D`**：以本文 **「Y 纵深」** 为唯一语义来源，或在代码注释中写明 **非 Z**。  
- 版本：**v1.3**；重大轴约定再变时递增子版本。

---

**关联文档**：`Assets/Doc/场景切换与对话触发跳转_架构溯源报告.md`。
