# 史莱姆 Y 轴同轴对齐（掉树 + 场景移动）— 架构溯源报告

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**主场景**：`ForestEastScene`（树上史莱姆）；同 Prefab 行为影响 `VerdantCorridor` / `WestRappRoad` 高台实例；`ForestScene` **无**史莱姆实例  
**产品期望**：落地后与移动中与玩家 **同一条水平战斗轴线**（无可见上下错位）  
**不是**：村庄 `TownPlayerLocomotion` 纵深；0723「站上卡住」物理案（可对照碰撞，本案是 **Y 对齐**）  
**提示词**：`Assets/Doc/提示词/0912/史莱姆Y轴同轴对齐_掉树与移动_架构侦探提示词.md`  
**对照**：`Assets/Doc/执行文档/7月/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**掉树偏与移动偏同源**：树上实例初始摆在 **Y≈7.41**，玩家战斗轴在 **Y≈−6.61**（Δ≈**14**）；落地只认 `IsGrounded`、**不 Snap 到玩家/轴线 Y**，地面 Move **只改 X** → 一旦落点或姿态残差偏了，跑多久都一直偏。现网还叠加：`bornDownY` 已废弃、`SetVelocity` 用 `MovePosition` 硬挪 + `MoveComponent` 大重力并行，落地 Y 不可控。

### ② 原因（通俗）

树上史莱姆一开始就站在比玩家高一截的「另一条跑道」上。掉下来时程序只问「脚底下碰到地面层没有」，碰到就停，**不会把脚对齐到雅儿站的那条线**；落地后走路也只左右挪，不再改高度。所以掉歪了就一直歪。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 期望（现网） |
|---|------|--------------|
| 1 | Hierarchy：`ForestEast` 选 `Slime (1)/(2)/(6)` | 初始 World Y ≈ **7.41** |
| 2 | 对照地面怪 `Slime` / `Slime (3)/(4)` 与 EnterPos | Y ≈ **−6.61**（与玩家进场同轴） |
| 3 | 触发树上史莱姆掉落后看 Transform.y | 应接近 −6.6 一带；若仍明显高于玩家 → 落点/残差问题 |
| 4 | 掉落后追击 ≥10s 再比双方 `position.y` | **差值应基本不变**（Move 不写 Y） |
| 5 | 拍板后施工：落地 Snap / 锁地面 Y（见 §5） | 目视同轴；跳攻仍可命中 |

### ④ 程序补充

见下文 §1～§6。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **掉树偏 / 移动偏是否同源** | **是** — 移动链**不主动改 Y**；可见持续上下差 = **落地（或摆放）留下的 Y 差被原样保持** |
| **主因** | ① 场景双轨摆放（树 Y≈7.41 vs 轴 Y≈−6.61）；② Born 落地无轴线 Snap；③ `bornDownY` 旧阈值已注释，现只靠短距 `IsGrounded` |
| **加重** | BornFall 每 Fixed `MovePosition(0, FallSpeed)` 与 `MoveComponent`（Gravity≈−100）并行；Idle **强行** `IsGrounded=true` |
| **非主因** | 追击 `targetDir.y` 未写位移；`IsInSameDepth` 追击已注释；村庄纵深无关 |
| **ForestScene** | 无 Slime Prefab 实例；影响面在 ForestEast + VerdantCorridor + WestRappRoad 高台怪 |

---

## 2. 调用链

### 2.1 掉树（写入 Y 处已标 ★）

```
树上实例 Sleep（初始 Y≈7.41）
  SlimeSleepState.Enter
    → moveCpn.canGravity = false；StopMove
  Update：FindTarget("BornTriggerArea")
    → EnterSubStateMachine<SlimeBornSubSM> → SlimeBornFallState

SlimeBornFallState.Enter
  → canGravity = true
  → downY = pos.y + BornDownY（Prefab bornDownY=-10）  // 仅算值，现网不用
  ★ FixedUpdate：slime.SetVelocity((0, FallSpeed))
       SetVelocity = BodyRg.MovePosition(transform.position + v)  // FallSpeed=-1 → 每帧硬降 1
  ★ 并行 MoveComponent.OnFixedUpdate：
       !IsGrounded && canGravity → Velocity += Gravity（Prefab Gravity.y=-100）
       → rg.velocity = Velocity
  Update：if (IsGrounded) → SlimeBornDownState
       // 旧：pos.y < downY 已注释

SlimeBornDownState.Enter
  → BodyRg.velocity = 0
  // ★ 无 Snap 到玩家 Y / 地面 hit 点
  → 动画结束 → SlimeIdleState
       Idle.Enter：★ 强制 moveCpn.IsGrounded = true；FreezePositionX
```

**落地判定**：`CapsuleGroundChecker` 仅 **向下 Raycast**，Mask=`GroundCenter`(bits `16384`)，默认探测距 ≈ `max(CapsuleRadius*2, 0.25)` ≈ **0.62**（Prefab 未覆写 `groundProbeDownDistance`）。

### 2.2 地面移动（写入 Y 处已标 ★）

```
SlimeMoveState / Idle 索敌
  targetDir = (playerPos - slimePos).normalized
  → 只用 targetDir.x → TurnToRight → MoveLeft/MoveRight
  ★ 只写 moveSpeedX；不写 position.y / moveSpeedY

MoveComponent.OnFixedUpdate
  GroundCheck → 落地时若 moveSpeedY<0 则清 0
  !IsGrounded && canGravity → ★ Velocity.y += Gravity
  rg.velocity = Velocity
  // 已落地且 IsGrounded：通常不再累加重力 → Y 保持落地值

JumpAtk（例外会改 Y）
  Up：抛物线 MovePosition → 目标玩家 x，最高点 endPos.y+3  ★
  Fall：抛物线移向 endPos（玩家当时 position）★
  → 仍以 IsGrounded 切落地；不保证最终 y == 玩家 y
```

### 2.3 谁**不**写 Y（已排除为主因）

| 路径 | 说明 |
|------|------|
| Move 追击 | 仅 `targetDir.x` |
| 旧 `IsInSameDepth` + 寻路 | `SlimeMoveState` 内大段已注释 |
| `DepthComponent` | Slime `OnInit` 里 `depthCpn.Init` **注释**；纵深排序不驱动位移 |
| 村庄 `TownPlayerLocomotion` | 战斗森林史莱姆不用 |

---

## 3. 证据表

| # | 证据 | 结论 | 状态 |
|---|------|------|------|
| E1 | ForestEast EnterPos（自 ForestScene / WestRapp / Verdant）均为 **y=−6.61** | **战斗轴线 Y ≈ −6.61** | ✅ 已证实 |
| E2 | 地面史莱姆：`Slime`/`(3)/(4)`/`EatSheep_*` 等 **y≈−6.61**（`(5)`≈−6.59） | 地面摆放已与轴同高 | ✅ 已证实 |
| E3 | **树上**：`Slime (1)` (168.48,**7.41**)、`(2)` (183.63,**7.41**)、`(6)` (361.9,**7.41**) | 掉树起点高 Δ≈**14.02** | ✅ 已证实 |
| E4 | `GroundLeft_1` Layer14、约 (128.8,−7.6)、盒高 2 → 顶面约 **−6.6**，覆盖树区 X | 理论落点应接近玩家轴 | ✅ 已证实 |
| E5 | Prefab `bornDownY: -10`；Fall 内 `y < downY` **已注释**；现只 `IsGrounded` | 旧设计落点 `7.41−10=−2.59` 本就不等于 −6.61；现网更不 Snap | ✅ 已证实 |
| E6 | `SetVelocity` = `MovePosition(pos+v)`，`FallSpeed=-1` | 每 Fixed 硬降 1，非物理速度 | ✅ 已证实 |
| E7 | Move Gravity `{0,-100}`，`canGravity` Fall 中打开 | 与 MovePosition 并行，落地 Y 抖动/过冲风险 | ✅ 已证实 |
| E8 | Move 主路径只 `MoveLeft/Right` | **移动不纠正 Y** → 与掉树同源表现 | ✅ 已证实 |
| E9 | `SlimeIdleState` 强制 `IsGrounded=true` | 可能掩盖「未真正贴地」 | ✅ 已证实 |
| E10 | JumpAtk `endPos=玩家 position`，落地仍看 `IsGrounded` | 跳攻可能部分纠偏，但非地面巡逻保证 | ✅ 已证实 |
| E11 | VerdantCorridor / WestRapp 亦有 y≈7.41 / 6.93 高台史莱姆 | 同 Prefab 同风险 | ✅ 已证实 |
| E12 | ForestScene 无 Slime PrefabInstance | 无直接实例影响 | ✅ 已证实 |
| E13 | Play 实测精确 Δ（落地后 / 移动 10s） | 需验收补数；静态摆放差已钉死 | ⚠️ 待 Play |
| E14 | 精灵 pivot 导致「Transform 同 Y 仍显偏」 | 可能加重观感；非双轨摆放主因 | 可疑 |

### 嫌疑复核

| 嫌疑 | 裁定 |
|------|------|
| **A 场景摆放 / 落点 ≠ 玩家轴** | **主因之一**（树实例双轨）；地面实例已对齐 |
| **B bornDownY / 落地判定** | **主因之一**（无 Snap；旧阈值废弃且本就不准） |
| **C 脚底/Pivot 视觉** | **可疑加重**；0723 GroundCld 已改 Trigger，不挡落点逻辑 |
| **D 移动时改写 Y** | **排除为主因**（Move 不写 Y；JumpAtk 例外） |
| **E 残留纵深追击** | **排除**（代码已注释；Depth Init 注释） |
| **F targetDir 含 Y 写位移** | **排除**（只用 `.x`） |

---

## 4. 「同轴」权威定义（施工必须遵守）

> **ForestEast（及同类横版战斗场）战斗轴线 Y = 该场景玩家站立地面 Y。**  
> 施工权威取：**当前玩家 `transform.position.y`**（进场后以 EnterPos / 实际站立为准；ForestEast 静态标尺 **≈ −6.61**）。  
> 史莱姆在 **地面态**（Idle / Move / Escape / BornDown 结束 / JumpAtk 落地后）的 `transform.position.y` 须与该权威 Y **目视同轴**（允许 ≤0.1～0.2 落地误差）。  
> **禁止**用村庄「Y=纵深」语义；`IsInSameDepth` 仅作攻击重叠参考，**不得**再设计成故意分轨站不同 Y。

---

## 5. 修复方案对比

| 方案 | 做法 | 优点 | 风险 | 推荐 |
|------|------|------|------|------|
| **A 地面态 Snap Y（主推）** | `BornDown` / JumpAtk Down / 进入 Idle·Move 时：`y = player.y`（或场景 `CombatAxisY` 常量）；可选落地后 `FreezePositionY`（跳起前解开） | 最小；直接满足产品；移动自然保持同轴 | 须在 JumpAtk 升空前解冻 Y；对齐后 `IsInSameDepth` 应变好而非更差 | ✅ **推荐** |
| **B 只改场景 + 落地探测** | 下调树实例初始 Y 或加落点辅助碰撞，使自然落到 −6.61；恢复/校准 `bornDownY` 作安全网 | 少改 AI | 多场景（East/Verdant/WestRapp）易漏；不解决过冲残差；BornFall 双驱动仍在 | 可与 A 叠加改摆放 |
| **C 重写 BornFall 单通道** | 去掉每帧 `MovePosition` 硬降，只走 `MoveComponent` 重力；落地 Raycast hit 点 Snap | 物理干净 | 改动面大于 A；需重调 FallSpeed/Gravity | 中期可选 |
| **D 仅 FreezePositionY** | 落地后冻 Y，不主动 Snap | 防漂移 | **不纠正**已偏的树落点 | ❌ 不足 |

**JumpAtk / Depth / 0723 风险**

| 点 | 评估 |
|----|------|
| JumpAtk | Snap 仅地面态；升空前清 FreezePositionY；落回再 Snap → 应仍可打中 |
| `IsInSameDepth` | 同轴后脚盒更易重叠 → **利好**命中；勿为对齐关 Depth 检测 |
| 0723 卡住 | 不恢复 `GroundCld` 实心；不改 PlayerFoot 矩阵；Snap 只改自身 `position.y` |

**验收标准**

1. 树上史莱姆掉落后与玩家 **目视同一水平线**；Debug 可选打双方 `position.y`。  
2. 追击/巡逻 ≥10s，相对玩家 **无持续上下漂**（Δ 不单调变大）。  
3. 普攻 / 跳跃攻击仍可命中。  
4. 不回归 0723「站上卡住」。  
5. 抽测 VerdantCorridor / WestRapp 高台史莱姆至少各 1 只。

---

## 6. 设计不清 → OPEN_QUESTIONS

已记入 `Assets/Doc/OPEN_QUESTIONS.md`：

- 权威 Y 用「实时玩家 y」还是「场景常量 −6.61」？  
- 地面已摆在 −6.61 的怪是否也强制每帧/进态 Snap？  
- JumpAtk 过程是否允许短暂不同轴（仅落地后对齐）？

---

## 附录：ForestEast 史莱姆 Y 一览

| 实例名 | 约 (x, y) | 角色 |
|--------|-----------|------|
| Slime / (3)/(4)、EatSheep_1/2 等 | y≈**−6.61** | 已在战斗轴 |
| **Slime (1)(2)(6)** | y≈**7.41** | **树上掉落源** |
| EnterPos（三入口） | y=**−6.61** | 玩家轴线标尺 |
| GroundLeft_1 顶 | ≈**−6.6** | GroundCenter 主走道 |

**关键路径**

| 用途 | 路径 |
|------|------|
| Born Fall/Down | `…/Slime/Anima/State/BornSubState/SlimeBornFallState.cs` 等 |
| Sleep 触发 | `…/SlimeSleepState.cs` |
| Move | `…/SlimeMoveState.cs` |
| 位移/重力 | `…/Move/MoveComponent.cs` |
| 落地检测 | `…/GroundCheck/CapsuleGroundChecker.cs` |
| Prefab | `Assets/GameRes/Prefabs/Entity/Monster/Slime.prefab` |
| 场景 | `Assets/GameRes/Scenes/ForestEastScene.unity` |
