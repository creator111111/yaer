# Cursor Agent Prompt · 史莱姆同轴：权威 Y 改为场景常量（禁止跟玩家跳跃）

> **角色**：先【架构侦探】只读核实现网 Snap 行为；拍板后【施工员】最小改权威 Y 解析  
> **日期**：2026-09-13  
> **现象（用户实测）**：史莱姆 **一直想跟玩家实时同轴** → 玩家一跳，史莱姆也飞起来，很不正常  
> **产品期望（钉死 · 用户口径）**：  
> - 「同轴」= 玩家 **平常走路/站立** 的那条 **场景战斗地面轴**（场景常量）  
> - 史莱姆在这条轴上掉落落地、Idle、Move、跳攻落地后贴回  
> - **不是**每帧/每次进状态去抄玩家当前 `position.y`（含空中）  
> **不是**：取消史莱姆 JumpAtk 技能本身；改村庄 2.5D 纵深；恢复 0723 GroundCld 实心；推翻 0912「要同轴」目标（只改 **权威 Y 来源**）  
> **前案**：`Assets/Doc/施工说明/0912/史莱姆Y轴同轴对齐_施工说明.md`（OPEN Q1 曾默认「实时玩家 y」——**本案要推翻该默认**）  
> **报告落盘**：`Assets/Doc/执行文档/0913/史莱姆同轴_权威Y改场景常量_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 同轴不是「跟着雅儿飞」。  
> 先认死一条地面跑道高度（场景常量），史莱姆掉下来、走路都贴这条线。  
> 雅儿跳、史莱姆自己跳攻，可以暂时离开；落地再回到这条线。  
> **不要一直去找玩家现在的高度。**

### 术语钉死

| 词 | 本案含义 |
|----|----------|
| **战斗轴 / 平常移动的轴** | 横版场景里玩家站立走路用的固定地面 Y（场景常量），**不含**跳跃瞬时 Y |
| **场景常量** | 代码/配置里的固定 float（现网已有兜底 `CombatAxisYFallback = -6.61f`）；侦探须核实 ForestEast / VerdantCorridor / WestRapp 地面怪与 EnterPos 是否同值，多场景是否共用一个常量或按场景配置 |
| **一直找玩家** | `TryResolveCombatAxisY` 优先 `player.transform.position.y`（0912 现网） |
| **正确 Snap** | `axisY = 场景战斗轴常量`（可保留「无配置时」兜底）；**禁止**用玩家空中 Y |

### 现网断点（助手预扫）

```
BaseSlimeState.TryResolveCombatAxisY()
  现网：玩家实体.position.y → 否则 atkTarget.y → 否则 CombatAxisYFallback(-6.61)
  产品要：场景常量（-6.61 或场景配置）为主；不要跟玩家实时 Y

调用 Snap 的 Enter：
  SlimeBornDownState / SlimeJumpAtkDownState / SlimeIdleState / SlimeMoveState

叠加：
  SlimeJumpAtkUpBefore：endPos = atkTarget.position（含空中）
  SlimeJumpAtkUpState：maxHeight = endPos.y + 3
  → 玩家在空中时跳攻也会「飞特别高」（次要，建议与常量轴一并修）
```

| 前案 OPEN Q1 | 本案裁定（用户钉死） |
|--------------|----------------------|
| 实时玩家 y；无玩家用 −6.61 | **场景常量**；玩家跳不参与权威 Y |

### 嫌疑 / 任务焦点

| # | 项 | 说明 |
|---|----|------|
| **A（主）** | `TryResolveCombatAxisY` 优先玩家实时 Y | 玩家跳 → Idle/Move/落地 Snap → 史莱姆飞 |
| **B** | JumpAtk `endPos.y` 用玩家瞬时位 | 跳攻顶点被抬高 |
| **C** | 多场景轴高是否都是 −6.61 | 侦探用地面怪 / EnterPos / 玩家落地 Y 核对；若有第二套轴高须写清配置方案 |
| **D** | Dead / 其它态误 Snap | 预扫 Dead 不调 Snap；顺带确认 |

### 侦探须回答

1. 现网「玩家跳史莱姆飞」是否确由 Snap→玩家实时 Y 引起？复现：玩家跳时史莱姆进 Idle/Move 或 JumpAtkDown。  
2. ForestEast（及走廊/西拉）**场景常量**应取多少？是否统一 `-6.61f`？证据（EnterPos、地面 `Slime` 静态 Y、玩家落地 Y）。  
3. `atkTarget.y` 兜底是否也要拿掉（避免空中目标）？  
4. JumpAtk 的 `endPos.y` / `maxHeightPos.y` 是否改为「常量轴 + 固定跳高」？推荐是否纳入本期。  
5. **方案 ≥2**：  
   - **A（产品指定 · 推荐）**：`TryResolveCombatAxisY` **只返回场景常量**（可把现 `CombatAxisYFallback` 升为唯一权威；或从 `GameSceneManagerConfig`/场景字段读，缺省 −6.61）  
   - **B**：仅当玩家 `IsGrounded` 才用玩家 y，否则用常量（用户已倾向常量，B 作对照）  
   - **C**：禁止 — 继续实时玩家 y  
   列利弊；写清「不要动」：0723、村庄纵深、Dead 定格案、JumpAtk 技能开关。

### 必读

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`（战斗横版同轴）  
3. `Assets/Doc/施工说明/0912/史莱姆Y轴同轴对齐_施工说明.md`（须改写 OPEN Q1）  
4. `BaseSlimeState.cs`（`SnapToCombatAxisY` / `TryResolveCombatAxisY` / `CombatAxisYFallback`）  
5. `SlimeIdleState` / `SlimeMoveState` / `SlimeBornDownState` / `SlimeJumpAtkDownState`  
6. `SlimeJumpAtkUpBefore.cs` / `SlimeJumpAtkUpState.cs`（跳攻高度）  
7. 本提示词  

### 禁止（侦探阶段）

- 禁止改代码 / Prefab / 场景 / Git  
- 禁止把「同轴」重新解释成跟玩家跳跃  
- 禁止为修本案关掉 JumpAtk  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0913/史莱姆同轴_权威Y改场景常量_架构溯源报告.md`

结构：

1. **结论一句话**（飞起来是否因实时玩家 Y；权威应改为场景常量）  
2. **调用链**（谁 Snap、权威怎么解析）  
3. **证据表**（常量候选值、多场景；已证实/排除）  
4. **与 0912 OPEN Q1 对照**（推翻原因）  
5. **方案 ≥2** + 推荐（默认 **场景常量**）+ 是否含 JumpAtk endPos + 验收 + 回归（掉树同轴、死亡定格、0723）  
6. 不清处记 `Assets/Doc/OPEN_QUESTIONS.md`

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告确认权威 Y 改为场景常量；常量取值已钉死（预期多为 `-6.61f`，以报告为准）。  
> **目标**：史莱姆地面态 Snap **只对齐场景战斗轴常量**；玩家跳跃时史莱姆 **不再飞起**；掉树/Move 后仍与玩家**站立时**同轴；JumpAtk 仍可发动，落地回常量轴。  
> **优先**：改 `TryResolveCombatAxisY`（及注释）；必要时 JumpAtk 的 `endPos.y` / 顶点改用常量轴 + 既有跳高；同步改 0912 施工说明 OPEN Q1 或在 0913 施工说明写明推翻。  
> **禁止**：继续优先 `player.transform.position.y`；Dead 调 Snap；改 0723 GroundCld；改村庄 Town 纵深；关掉 JumpAtk；在 Update 每帧追玩家 Y。  
> **文档**：`Assets/Doc/施工说明/0913/史莱姆同轴_权威Y改场景常量_施工说明.md`  
> **验收**：  
> 1）玩家反复跳：旁观/追击中的史莱姆 **Y 不跟飞**（目视贴地面轴）；  
> 2）树上掉落落地后与玩家站立同轴（Δy 小）；  
> 3）Move/Idle 切换不抬飞；  
> 4）JumpAtk：可跳起、落地回轴；若修了 endPos，顶点不再因玩家空中而离谱；  
> 5）抽测死亡定格、0723 不站上卡住不回归。  

---

## 【验收员】Prompt（可选）

> Debug `[SlimeCombatAxis]`：`SnapToCombatAxisY` 时打 `axisY`、玩家 y、玩家是否落地、史莱姆 y。  
> 玩家跳 10 次：史莱姆 y 应稳定在常量附近，不应出现与玩家空中 y 同步抬升。
