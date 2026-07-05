# 村庄先 W/S 后 A/D 无横向位移 — 执行说明（Combat 轨 · 入态与转向早退）

| 项 | 内容 |
|----|------|
| **文档性质** | 架构侦探溯源 + 施工/验收指引（以当前仓库代码为准） |
| **现象** | 村庄 **`Village2_5D`** 下，玩家**先**用 W/S 做纵深移动，**再**按 A/D 期望横向移动时，**常常完全没有横向位移**（体感为「AD 没反应」）；换装 **Home** 控制器时往往**不出现**或明显减轻 |
| **关联文档** | `Assets/Doc/执行文档/0513/村庄遇纵深障碍后横向移动迟滞_架构溯源与施工执行说明.md`（横移被 `StopMoveInX` 误杀等问题）、`Assets/Doc/02_SYSTEM_SPEC.md` §4（Combat Run 与纵深协同） |
| **Unity** | 2020.3.x |

---

## 1. 结论（根因已能在静态代码上闭环）

**主因（高置信）**：在 **Combat 地面跑态 `CombatRunState`** 中，进村且**仅有纵深意图**进入 `Enter` 时，走 **`villageDepthOnly`** 分支会**跳过 `SetRunSpeed()`**，`MoveComponent` 上 **`moveSpeedX` 保持为 0**（通常因上一状态 `CombatIdleState.Enter` 已 `StopMove()`）。  
此后玩家按下与**当前面朝一致**的方向键（例如默认朝右时再按 D）时，**`CombatRunState.MoveRight` / `MoveLeft` 仅在「从反向翻向本向」时才会 `SetRunSpeed()`**；接着调用 **`MoveComponent.MoveRight` / `MoveLeft` → `TurnRight` / `TurnLeft`**，若**已经面朝该侧**，`Turn*` 在 **`isCheckDir==true` 默认路径下会直接 `return`**，**不会给 `moveSpeedX` 赋值** → 横向目标速度**一直为 0**，`OnFixedUpdate` 里 `MoveVelocity` 写入刚体的水平分量也为 0 → **表现为 AD「无响应」**。

**与 0513 文档问题的关系**：0513 侧重 **`StopMoveInX` 与「纵深 / 横移意图」门控错配**；本条是 **`Enter` 未建立初速 + 同向转向早退** 导致的「**首帧横移速度建不起来**」，二者可叠加，但**因果链独立**，验收时需分开对照。

**为何常被描述成「先 WS 再 AD」**：W/S 往往先把角色从 Idle **切进 `CombatRunState` 且不带横向键** → 命中 **`villageDepthOnly`**；随后再按 A/D 才第一次尝试建立横向速度，正好撞上上述 **`SetRunSpeed` 条件过窄 + `Turn*` 早退** 组合。

---

## 2. 逻辑溯源（代码级）

### 2.1 `CombatIdleState` 入态：横向速度被清零

- **`Enter`** 调用 **`moveComponent.StopMove()`**，`Velocity` 与 **`rg.velocity`** 归零。  
- **代码位置**：`Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Combat/State/Ground/CombatIdleState.cs`。

### 2.2 `CombatRunState.Enter`：纯纵深时故意不给水平 Run 速

- 条件：`LocomotionMode == Village2_5D && !inputComponent.HasVillageExploreHorizontalMoveIntent()` → **`villageDepthOnly == true`**。  
- 为 **`true`** 时**不执行** **`moveComponent.SetRunSpeed()`**（注释写明：避免纵深与横向速度叠加、不吃 Run 体力等设计意图）。  
- **直接后果**：从 Idle 经「仅纵深意图」进入 Run 时，**`moveSpeedX` 仍为 0**。  
- **代码位置**：`Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Combat/State/Ground/CombatRunState.cs`（`Enter` 内约 40–49 行）。

### 2.3 `CombatRunState.MoveRight` / `MoveLeft`：`SetRunSpeed` 仅在「翻面」时调用

- **`MoveRight`**：仅当 **`moveComponent.Direction == EDirectionType.Left`** 时调用 **`SetRunSpeed()`**，再 **`moveComponent.MoveRight`**。  
- **`MoveLeft`**：对称地，仅当当前为 **Right** 时 **`SetRunSpeed()`**。  
- **含义**：玩家**已经面朝右**时再按 D（向右），**不会**走 `SetRunSpeed()` 分支。  
- **代码位置**：同上文件 `MoveRight` / `MoveLeft`（约 124–150 行）。

### 2.4 `MoveComponent.TurnRight` / `TurnLeft`：已朝目标侧则整函数早退

- **`TurnRight(bool isCheckDir = true)`**：若 **`isCheckDir && direction != EDirectionType.Left`**，**直接 `return`**，后面的 **`direction = Right`、`moveSpeedX *= -1`** 等**不会执行**。  
- 当 **`moveSpeedX` 已为 0** 且**已朝右**时：早退后 **速度仍为 0**（没有「乘 -1 赋初值」路径）。  
- **`MoveRight`** 仅调用 **`TurnRight`**，**自身不写 `moveSpeedX`**。  
- **代码位置**：`Assets/Scripts/Game/GameRuntime/Entities/Component/Move/MoveComponent.cs`（`TurnRight` / `TurnLeft`、`MoveRight` / `MoveLeft`）。

### 2.5 与 Home 轨的对照（为何 Home 往往「正常」）

- **`HomeWalkState.Enter`** 在订阅 A/D 前**无条件** **`moveComponent.SetWalkSpeed()`**，纯 W/S 进 Walk 时也会先写入**非零**的 **`moveSpeedX`**（沿当前朝向）。  
- 之后即使 **`TurnRight` 因同向早退**不写速度，**刚体横移仍由 Enter 已写入的速度驱动**。  
- **代码位置**：`Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Home/HomeWalkState.cs`（`Enter`）。

---

## 3. 典型复现（验收用）

| 步骤 | 操作 | 期望（当前缺陷下） |
|------|------|--------------------|
| 1 | 村庄场景、`RuntimeAnimatorController` 为 **Combat**、`LocomotionMode` 为 **`Village2_5D`** | — |
| 2 | 仅按 **W 或 S**（不碰 A/D），使角色从 Idle 进入 **Run**（Animator 上 Run 体态可仍为真） | `CombatRunState.Enter` 走 **`villageDepthOnly`**，`moveSpeedX` 为 **0** |
| 3 | 保持默认**面朝右**，再只按 **D**（与朝向一致） | **`MoveRight` 不调 `SetRunSpeed`，`TurnRight` 早退** → **无横向位移** |
| 4 | 松开 D，改按 **A**（反向） | **`MoveLeft` 中因 `Direction==Right` 会 `SetRunSpeed()`** 并翻面 → **横向位移恢复**，易被误判为「只有换向才有效」 |

> **说明**：若先按 **A**（与默认右相反），可能**第一步就有速度**（翻面分支会 `SetRunSpeed`），与「先 D 无反应、先 A 能动」形成**不对称**，便于组内快速确认本缺陷。

---

## 4. 施工方向（最小改动；合入前评审）

> 下列为**实现方向**，具体数值与 API 以仓库为准；改动应限制在 **Combat 村庄路径** 或 **CombatRunState 包装方法**，避免影响战斗场景常规跑图。

| 优先级 | 方向 | 说明 |
|--------|------|------|
| **P0** | 在 **`CombatRunState.MoveRight` / `MoveLeft`** 中补「**零速补票**」 | 在调用 **`moveComponent.MoveRight` / `MoveLeft`** 之前（或之后视你们对 `Turn` 的约定），若 **`Mathf.Abs(moveComponent.moveSpeedX)` 低于小死区**（与 `TownPlayerLocomotion.walkAnimatorDeadZone` 同量级即可讨论），则 **`SetRunSpeed()`**，保证**同向首按**也能建立横向速度。**原因**：与 Home 的 **`SetWalkSpeed()`** 入态语义对齐，且不改 `MoveComponent` 全局 `Turn*` 语义，战斗其它状态机风险相对可控。 |
| **P1** | 调整 **`villageDepthOnly` 与 `SetRunSpeed` 的取舍** | 例如仅在 **`HasVillageExploreHorizontalMoveIntent()` 为真**的帧再 `SetRunSpeed`，或 Enter 仍跳过、但在 **`Update` 首次检测到横向意图**时补一次初速；需与「纵深不叠横向速度」的原始注释一起评审，避免回归 0513 类叠移手感。 |
| **P2**（不推荐首选） | 改 **`MoveComponent.TurnRight`/`TurnLeft` 早退语义** | 全局影响面大，易波及战斗翻面与数值符号，**不符合**「最小改动」原则，仅作文档级替代方案记录。 |

### 4.1 替代方案（文档级）

| 方案 | 说明 |
|------|------|
| **村庄 Combat 专用子类 Move** | 覆写 `MoveRight` 在零速时强制写速；隔离清晰，但引入 Prefab/组件替换成本。 |
| **村庄强制 Home 控制器** | 产品向决策，非本条程序单点修复。 |

---

## 5. 验收清单（PR / 试玩）

| ID | 条件 | 通过说明 |
|----|------|----------|
| C-01 | Combat、村庄，**仅 W/S** 进 Run 后，**先按与朝向同向的 A/D** | 角色**稳定获得**与朝向一致的横向位移，无需先反向再回正向 |
| C-02 | 同上，**先反向再同向** | 行为与改动前兼容，无速度翻倍或异常冲刺 |
| C-03 | Home 控制器同操作回归 | **不因** Combat 改动引入 Home 侧 Animator / 速度异常 |
| C-04 | 非村庄战斗场景常规 Run | **无**新增「零速起步失败」或翻面逻辑回归 |

---

## 6. 关键代码索引（PR 描述用）

| 文件 | 说明 |
|------|------|
| `CombatRunState.cs` | `Enter` 内 `villageDepthOnly` 与 `SetRunSpeed` 跳过；`MoveRight`/`MoveLeft` 条件 `SetRunSpeed` |
| `MoveComponent.cs` | `TurnRight`/`TurnLeft` 同向 `return`；`MoveRight`/`MoveLeft` 不写绝对初速 |
| `CombatIdleState.cs` | `StopMove` 清零初态 |
| `HomeWalkState.cs` | 对照：`Enter` 无条件 `SetWalkSpeed` |
| `PlayerInputComponent.cs` | `HasVillageExploreHorizontalMoveIntent`（与 `villageDepthOnly` 判定相关，**非**本 bug 主因但需一并回归） |

---

*文档版本：2026-05-14；静态代码结论，实施前建议用 Animator 参数 + `moveSpeedX` / `rb.velocity.x` 短时日志做一次同复现表对照。*
