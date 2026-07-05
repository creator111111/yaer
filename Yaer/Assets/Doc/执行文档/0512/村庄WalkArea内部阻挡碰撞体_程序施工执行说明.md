# 村庄 WalkArea 内部阻挡碰撞体 — 程序施工执行说明

| 项 | 内容 |
|----|------|
| **文档性质** | 在既有 **VillageWalkArea（`PolygonCollider2D` + 脚本几何内收）** 之外，补充**可走区内部的实体阻挡**；供**施工员**与策划摆场景、与程序定 Layer / 执行顺序 |
| **前置文档** | `村庄移动系统第三阶段_施工执行说明.md`、`村庄DNF式2.5D移动_迁移方案.md`、`02_SYSTEM_SPEC.md`、`00_MASTER_PROMPT.md` |
| **问题背景** | 第三阶段已约定：**WalkArea 多边形为 Trigger**，用 **`OverlapPoint` + 几何修正**保证「不出多边形」；**未**覆盖「多边形内不能穿过的柱子、花坛、台阶投影」等**局部阻挡** |

---

## 1. 本需求要做什么（产品口径）

| 范围 | 说明 |
|------|------|
| **新增能力** | 在 **`VillageWalkArea` 多边形内部**，增加一类**仅村庄探索玩法**下生效的 **2D 碰撞体**，使玩家 **A/D 横移** 与（按技术选型）**W/S 纵深** 无法**穿模穿过**指定区域 |
| **与 WalkArea 关系** | **外边界**仍由 WalkArea 脚本约束；**阻挡体**只解决「合法区内再挖洞 / 立墙」 |
| **策划工作流** | 在 Editor 中摆放 **BoxCollider2D / CapsuleCollider2D / CompositeCollider2D** 等，**不调 C# 数字**即可完成大部分摆关；必要时挂统一 **标识组件** 便于调试与规则扩展 |
| **架构兼容** | **不**替换 `Rigidbody2D` 村庄管线；**不**改输入与 NodeCanvas；阻挡逻辑以 **Layer + Collider 形态 +（可选）薄代码** 为主 |

---

## 2. 硬性约束（合入前自检）

| 禁止 | 执行口径 |
|------|----------|
| **禁止重构整个移动系统** | 不在 `MoveComponent` 内堆村庄专属分支作为主路径；不把 WalkArea 改为「全靠物理挤出边界」 |
| **禁止修改输入系统** | 不改 `InputActions`、键位映射、指令队列 |
| **禁止修改剧情系统** | 不碰 `StoryComponentGSM`、NodeCanvas 驱动链 |
| **禁止用村庄脚本写 `Run`** | 与 `02_SYSTEM_SPEC` §4.2 及 `TownPlayerLocomotion` 注释一致 |
| **禁止把纵深主位移写到 Z** | 规范 §1：村庄纵深仍为 **世界 Y** |

---

## 3. 架构快照与关键风险（施工前必读）

### 3.1 当前移动写回方式（与「挡碰撞」的关系）

| 子系统 | 行为摘要 |
|--------|----------|
| **`MoveComponent.OnFixedUpdate`** | 以 **`Rigidbody2D.velocity`**（含 `Velocity.x` 等）参与 **Unity 2D 物理步**；横移与障碍物的 **刚体碰撞分离**在此阶段自然发生（前提是 **Layer 碰撞矩阵** 允许与阻挡层碰撞）。 |
| **`TownPlayerLocomotion.OnFixedUpdate`** | 对 **`_villageWorldY`** 积分后 **`WriteRootTransformWithAuthoritativeDepthY()`**：将 **`Rigidbody2D.position`** 设为 **`(rbPos.x, _villageWorldY)`**，并把 **`velocity.y` 置 0**；随后在 **`PostPhysicsResyncDepthCoroutine`**（`WaitForFixedUpdate` 之后）**再次**写回权威 Y 并 **`ApplyVillageWalkPolygonPostCorrection()`**。 |

### 3.2 核心风险：**纵深 Y 由脚本权威驱动，与「纯静态体挡纵向」易打架**

- **横移（X）**：在默认执行顺序下，阻挡体为 **非 Trigger** 的 **Static / Kinematic Rigidbody2D + Collider2D** 时，通常能形成玩家**贴墙滑移**或**顶停**，与现有 **`velocity.x`** 方案较易共存（仍须实机调 **Rigidbody2D Collision Detection**、**刚体插值**，避免薄墙穿透）。  
- **纵深（W/S，世界 Y）**：`TownPlayerLocomotion` **每物理帧按积分结果直接写 `position.y`**，**不会**自动读取「与障碍重叠后物理引擎推回的 Y」。因此：  
  - **仅**在纵深方向立一块「挡板 Collider」而**不**改 `TownPlayerLocomotion`，玩家仍可能**按速度积分穿入/穿出**挡板（表现为穿模或与 Collider 重叠抖动）。  
  - **结论**：若策划需要 **W/S 方向不可越过**的障碍，必须在文档中二选一：**(A)** 在 `TownPlayerLocomotion` 增加 **Cast / 夹紧**（对 `_villageWorldY` 或积分前位移做几何限制）；**(B)** 策划只使用 **横向（屏幕左右）阻挡**，纵深障碍用 **WalkArea 多边形裁切道路**表达，而不用「纵向薄墙」。

以上原因须在 PR 与策划说明中写清，避免验收争议。

### 3.3 与 WalkArea（Trigger）的共存

- WalkArea 建议保持 **Trigger + 脚本修正**（第三阶段已定），**不要**把 WalkArea 改成依赖物理挤压来「挡人」，否则与 **不规则多边形 + 刚体** 的组合易产生抖动、与对话/检测 Trigger 混淆。  
- **阻挡体**应为 **独立物体**、**独立 Collider**，与 **`VillageWalkArea` 物体非子父或同 Collider** 均可，但须在场景中 **几何上位于 WalkArea 多边形内**（策划规范）；可选：Editor 脚本用 **`PolygonCollider2D.OverlapPoint(阻挡体中心)`** 做摆放校验（**可选工具，非首版必做**）。

---

## 4. 推荐技术方案（分档：优先低侵入）

### 4.1 方案 A（首推）：**专用 Physics Layer + 非 Trigger 静态碰撞体**

| 项 | 说明 |
|----|------|
| **新增 Layer** | 例如 **`VillageWalkBlocker`**（名称以团队为准，在 **Tags & Layers** 登记）。 |
| **碰撞矩阵** | 仅勾选与 **Player**（或玩家 `Rigidbody2D` 所在 Layer）在 **2D 矩阵**中碰撞；与 **敌人普攻、无关 Trigger、WalkArea 所用层** 等 **显式关闭**，避免村庄内误伤、误挡检测。 |
| **刚体与 Collider** | 阻挡物使用 **`Rigidbody2D` Body Type = Static**（或 Kinematic 且不脚本改位），**`Collider2D.isTrigger = false`**；形状以 **Box / Capsule / Polygon** 贴合美术。 |
| **适用** | **以阻挡横移为主**的墙、栏杆、大体积障碍；纵深「不能过线」优先用 **WalkArea 顶点**表达。 |

**施工注意**：合入后须全量检查 **Physics 2D Layer Collision Matrix**，并在 `OPEN_QUESTIONS` 记录「是否与某战斗技能共用 Layer」等例外。

### 4.2 方案 B：**纵深方向也要实体挡板**

| 项 | 说明 |
|----|------|
| **做法** | 在 **`TownPlayerLocomotion`** 的纵深积分路径上，对 **`Rigidbody2D` + `CapsuleCollider2D`（或项目现有身体形状）** 做 **`Cast` / `BoxCast`**，命中 **`VillageWalkBlocker`（或带 `IVillageDepthObstacle` 标记的 Collider）** 时，**限制 `_villageWorldY` 或 `depthVelocity`**（夹紧到接触前一刻或沿法线清零法向分量）。 |
| **代价** | **触及核心村庄组件**，回归面大于方案 A；须补充 **验收用 Debug 开关**（如 `[VillageBlocker]` 日志，用完即关）。 |

### 4.3 方案 C（不推荐作首版）：**纯 Trigger + 脚本把玩家推回**

| 缺点 | 与 WalkArea 几何修正重复、易与多 Trigger 叠加顺序敏感；手感不如刚体分离直观。 |

---

## 5. 场景与资源规范（策划向）

### 5.1 命名与层级建议

| 类型 | 建议 |
|------|------|
| **父节点** | 如 **`VillageWalkObstacles`**，与 **`VillageWalkArea_Root`** 平级，便于隐藏/复制场景。 |
| **单物体命名** | `VillageBlock_花坛01`、`VillageBlock_台阶左` 等，便于日志与 QA 填表。 |
| **Layer** | 统一使用 **`VillageWalkBlocker`**（或团队最终命名），**禁止**随手挂在 **Default** 导致全局误碰。 |

### 5.2 与对话、拾取、剧情 Trigger 的隔离

- **阻挡**用 **非 Trigger + Blocker Layer**。  
- **对话 / 拾取**继续用 **Trigger + 独立 Layer**；在矩阵中保证 **Blocker 不与剧情检测层产生错误交互**（例如阻挡层不挡射线式对话若项目用 Raycast LayerMask，须单独测）。  

---

## 6. 推荐施工顺序

1. **定 Layer 与矩阵**：新增 **`VillageWalkBlocker`**，只与玩家碰撞；文档化截图矩阵。  
2. **场景中批量摆 Collider**：先只验证 **A/D 贴墙**（方案 A）。  
3. **实机调参**：玩家 **`Rigidbody2D`** 的 **Collision Detection**、**Sleep Mode**；薄墙加厚或改用 **CompositeCollider2D**。  
4. **若需纵轴挡板**：再开 **方案 B** 任务单，在 `TownPlayerLocomotion` 内 **最小增量** 增加 Cast + 注释说明与 WalkArea 修正的先后顺序。  
5. **回归**：进村/出村、WalkArea 凹角、贴障碍旋转、与 **`VillageMovementInputDebug`** 联合看日志；Forest 等战斗场景 **零矩阵副作用**。

---

## 7. 验收清单

| ID | 条件 | 通过说明 |
|----|------|----------|
| B-01 | 外边界仍有效 | 玩家无法稳定停留在 **WalkArea 多边形外**（与第三阶段一致） |
| B-02 | 区内阻挡 | 在策划放置的 **至少 2 类**障碍（薄墙、厚块）下，**横移**无法穿入模型内部（允许贴边滑移） |
| B-03 | 村庄外无回归 | 非 `Village2_5D` 场景下 **Layer 矩阵** 不改变战斗/森林既有碰撞行为（或改变项已书面列出并测完） |
| B-04 | 无输入/剧情破坏 | 与 §2 禁止项一致；对话与拾取 **不误挡**（或已按 Layer 修通） |
| B-05 | 纵轴需求已声明 | 若策划坚持 **W/S 穿不过某线**：已实现 **§4.2** 或在 `OPEN_QUESTIONS` 写明「暂用 WalkArea 裁切替代」 |

---

## 8. 可修改边界（摘要）

| **鼓励** | 新增 **Layer**、场景中 **Collider 与 Static 刚体**、可选 **`VillageWalkObstacle` 标识 MonoBehaviour**（仅序列化/调试/Gizmos）、方案 B 下 **`TownPlayerLocomotion` 局部 Cast 逻辑**（须注释齐全）。 |
| **谨慎** | 修改 **`MoveComponent`** 全局碰撞或速度上限；修改 **玩家 Prefab** 的 **Rigidbody2D** 全局参数（影响全游戏）。 |
| **禁止** | §2 表内各项；为挡人而 **把 WalkArea 改为非 Trigger 挤压主方案**；在 **`Update`** 堆砌大量业务阻挡逻辑。 |

---

## 9. 关键代码索引（便于 PR 描述）

| 路径 | 说明 |
|------|------|
| `TownPlayerLocomotion.cs` | 权威纵深 Y 写回、`ApplyVillageWalkPolygonPostCorrection`、`PostPhysicsResyncDepthCoroutine` |
| `MoveComponent.cs` | `OnFixedUpdate` → `MoveVelocity` → `rg.velocity` |
| `村庄移动系统第三阶段_施工执行说明.md` | WalkArea Trigger 与策划规范 |

---

## 10. 替代方案说明（文档级）

| 方案 | 适用 | 缺点 |
|------|------|------|
| **Tilemap CompositeCollider2D** | 大量规则格挡 | 工作流与现有多边形 WalkArea 并行，美术流水线需对齐 |
| **NavMesh** | 大地图寻路 | 与当前「刚体 + WalkArea」架构不一致，重构面大 |
| **纯 WalkArea 挖洞** | 障碍边界与可走区边界一致 | 凹多边形维护成本高，难表达「圆形花坛」等 |

---

*文档版本：2026-05-11；与第三阶段 WalkArea **互补**：外轮廓靠 Polygon，**区内阻挡**靠专用 Layer 刚体 +（可选）纵深 Cast。落地后请更新 Layer 表截图与 `OPEN_QUESTIONS` 中纵轴需求结论。*
