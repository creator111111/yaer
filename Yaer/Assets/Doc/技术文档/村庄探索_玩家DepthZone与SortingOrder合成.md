# 村庄探索：玩家 DepthZone 与 Sorting Order 合成 — 技术说明

| 项 | 内容 |
|----|------|
| **适用范围** | 村庄 `PlayerLocomotionMode.Village2_5D`；策划用 **Trigger 体积**切换玩家 Sprite 的 **Sorting Layer**；可选在区内 **锁定 Order in Layer**（覆盖按世界 Y 的每帧排序） |
| **关联执行说明** | `Assets/Doc/执行文档/树屋楼梯深度区域系统_架构逻辑分析_执行说明.md` |
| **关联技术说明** | `Assets/Doc/技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md`（场景物体 `VillageSceneObjectDepthSort`、玩家 Y 排序语义） |
| **Unity** | 2020.3.x（与工程主版本一致） |

---

## 1. 问题背景与职责划分

### 1.1 两套逻辑为何「看起来在抢 Order」

- **`TownPlayerLocomotion`**（玩家移动组件，**本任务禁止改源码**）：在村庄模式下 **`OnUpdate`** 中调用 `ApplyDepthSortingFromWorldPosition()`，根据 **`spriteForDepthSort.transform.position` 的世界 Y** 写入  
  `sortingOrder = Round(-(y * depthSortingFactorY))`  
  与 `DepthComponent` 约定一致：**Y 越大，Order 越倾向变小（可为负）**。上楼梯时 Y 持续变化，Inspector 里会看到 Order 跟随变化（例如 **-30**）。
- **`VillagePlayerDepthZone` + `VillagePlayerDepthZoneListener`**：通过 **PlayerFoot** 层脚点进入 **Trigger**，切换玩家身体相关 **`SpriteRenderer.sortingLayerName`**（如 `Player` → `SceneObject`）；**不负责纵深 Y 位移**。

结论：**Layer 由 Zone 事件驱动；Order 默认仍由 Locomotion 按 Y 每帧驱动**。若策划要求「**进入某体积后，Order 固定为策划表上的值，不再随纵轴变化**」，必须在 **不改 `TownPlayerLocomotion`** 的前提下增加 **Order 合成策略**（见 §3）。

### 1.2 与场景遮挡文档的关系

| 维度 | `VillageSceneObjectDepthSort`（场景物体） | `VillagePlayerDepthZone`（玩家） |
|------|------------------------------------------|----------------------------------|
| 改谁 | 场景物体 `SpriteRenderer` | **玩家**子层级 `SpriteRenderer` |
| Sorting Layer | `Default` ↔ `SceneObject` 等 | 按 Zone 配置写入；无区时回 **`Player`** |
| Order | 可按锚点与玩家 Y 比较配置 | 默认不写；可选 **区内锁定**（§3） |

两者可并存；全局前后关系仍以 **`TagManager` 中 Sorting Layers 顺序** + 同层 **Order** 为准，摆关与数值表需策划统一验收。

---

## 2. 代码与资源索引

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillagePlayerDepthZone.cs` | 挂在 **Trigger `Collider2D`** 上；脚点进入/离开时向 Listener **注册/注销**；序列化 **目标 Sorting Layer**、**区优先级**、**是否锁定 Order** 及 **锁定时的 Order 数值**。 |
| `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillagePlayerDepthZoneListener.cs` | 挂在 **`PlayerLogic`** 同物体（可手动添加；否则 Zone 首次触发时 **AddComponent**）；缓存玩家子层级 **`SpriteRenderer[]`**；维护多区 **优先级 + 稳定次序**；**`LateUpdate`** 内应用 Layer，并在需要时 **后写** `sortingOrder`。 |
| `Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs` | **`spriteForDepthSort`**、`depthSortingFactorY`；每帧 Y → Order（**勿为 DepthZone 在此打补丁**，遵守任务卡）。 |
| `Assets/Scripts/Game/Static/Name/Settings/SortingLayerName.cs` | `Player` / `SceneObject` / `Default` 等常量（须与 **TagManager** 一致）。 |
| `ProjectSettings/TagManager.asset` | **Sorting Layers** 顺序与命名；物理 **Layer** 中含 **`PlayerFoot`**（脚点 Trigger 判定）。 |

---

## 3. Sorting Order 合成策略（区内锁定）

### 3.1 设计原则

- **执行顺序**：`TownPlayerLocomotion` 在 **Update** 写 Order → `VillagePlayerDepthZoneListener` 在 **LateUpdate** 末尾再次写入 **固定 Order**（当且仅当当前胜出 Zone 勾选了锁定）。  
  这样在**不修改移动脚本**的前提下，保证本帧渲染前 Order 为策划指定值。
- **多区规则**：与 Layer 一致，取 **最高 `zonePriority`**；同优先级时取 **更大 `GetInstanceID()`** 的 Zone，避免嵌套区单帧抖动。
- **非村庄模式**：Listener 将 Layer 拉回 **`Player`**，且 **不应用** Order 锁定（与 `VillageSceneObjectDepthSort` 的村庄门控思路一致）。

### 3.2 锁定写在哪几个 Renderer 上

| 优先级 | 行为 |
|--------|------|
| 1 | Listener 上 **`sortOrderOverridePrimaryRenderer`** 已赋值：只改该 **`SpriteRenderer.sortingOrder`**（**推荐**：与 **`TownPlayerLocomotion.spriteForDepthSort`** 指向同一引用，避免头发/武器等多部位被压成同一 Order）。 |
| 2 | 未手动指定：初始化时 **反射**读取 `TownPlayerLocomotion` 私有字段 **`spriteForDepthSort`**（字段名变更须同步改 Listener 内常量，否则退回下一档）。 |
| 3 | 反射失败：对当前缓存的 **全部** `SpriteRenderer` 写入同一 Order（验收时若发现子部件前后错位，请改用第 1 档手动绑定）。 |

### 3.3 Zone 侧配置项

| Inspector 字段 | 说明 |
|----------------|------|
| **targetSortingLayer** | 进入本区且本区在规则下胜出时，写入玩家身体的 Sorting Layer 名（与 TagManager 一致）。 |
| **zonePriority** | 多区重叠时越大越优先。 |
| **lockSortingOrderInZone** | 为 **true** 且本区胜出时，由 Listener 每帧（LateUpdate）覆写 **Order** 为下方数值。 |
| **sortingOrderInZone** | 与 `lockSortingOrderInZone` 配套；须与 **同层场景物体** 的 Order 表一起验收，避免栏杆/墙体穿插错误。 |

默认 **不锁定 Order**（`lockSortingOrderInZone = false`），与早期「仅切 Layer、Order 仍跟 Y」行为兼容。

---

## 4. 触发与脚点（常见「到某高度就失效」）

- **进入/离开**以 **`PlayerFoot` 物理层** 上的 **`Collider2D`** 与 Zone 的 **Trigger** 重叠为准（与 Walk 区脚点语义一致）。
- 若红字描述 **「脚底高于某高度就失效」**：优先检查 **脚点 Collider 是否仍完全在 Trigger 体积内**。楼梯上抬时脚点上移，容易从 **薄/矮的 Box** 上沿 **Exit**，导致 Zone 注销、Layer 回到 `Player`、Order 锁定解除——属 **摆体积与碰撞体尺寸** 问题，不是 Order 公式单点 Bug。
- **缓解**：加高 Trigger、略向下扩底、或微调脚点 Collider 中心/高度（在 Prefab 与关卡规范允许范围内）。

---

## 5. 验收建议

| ID | 项 | 通过条件 |
|----|-----|----------|
| PDZ-01 | 默认层 | 无生效 Zone 时，玩家主体 **`sortingLayerName == Player`**（与 TagManager 一致）。 |
| PDZ-02 | 进入切层 | **PlayerFoot** 进入 Zone 后 Layer 变为配置值；离开且无其它区时回到 **Player**。 |
| PDZ-03 | Order 锁定 | 勾选锁定并配置 `sortingOrderInZone` 后，在区内上下楼梯，**主深度 Sprite** 的 Order **保持不变**；出区后恢复随 **Y** 变化（由 Locomotion 接管）。 |
| PDZ-04 | 多区 | 重叠区 **优先级** 与文档一致；无单帧 Layer/Order 来回跳变。 |
| PDZ-05 | 模式隔离 | **非 Village2_5D** 下不应用 Zone Layer/Order 锁定表现，无战斗场景误切。 |
| PDZ-06 | 规范回归 | 进村 A/D、W/S、斜向无新增卡死；村庄链路不抢写 **Run**（见 `02_SYSTEM_SPEC.md` §4）。 |

调试日志：Listener 上 **`debugLogOnLayerChange`** 仅在 **Layer 实际变化** 时打印 **`[VillageDepthZone]`**，避免刷屏；Order 锁定默认不打日志，需要时可临时在 `ApplySortingOrderOverrideAfterLocomotion` 中加验收日志（用完删除或加开关）。

---

## 6. 替代方案说明（文档级）

| 方案 | 优点 | 缺点 |
|------|------|------|
| **A. LateUpdate 后写 Order（当前实现）** | 不改 `TownPlayerLocomotion`；与任务卡「禁止改移动脚本」一致。 | 每帧多一次写入；依赖 Unity **Update → LateUpdate** 顺序；反射兜底对字段重命名敏感。 |
| **B. 在 `TownPlayerLocomotion` 内检测 Zone 后跳过 Y 排序** | 无每帧重复写；语义最干净。 | **违反**当前任务卡/执行说明中对移动脚本的修改禁令，需策划与程序单独开卡评审后再做。 |

---

## 7. 文档变更记录

| 日期 | 说明 |
|------|------|
| 2026-05-13 | 初版：归纳玩家 DepthZone、与 `TownPlayerLocomotion` 的 Order 合成、配置与验收口径；与执行说明 §4「双写 Order」对齐。 |
