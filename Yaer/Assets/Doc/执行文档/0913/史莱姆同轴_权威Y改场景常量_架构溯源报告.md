# 史莱姆同轴 · 权威 Y 改场景常量 — 架构溯源报告

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**现象（用户）**：史莱姆一直想跟玩家实时同轴 → **玩家一跳，史莱姆也飞起来**  
**产品期望**：同轴 = 场景战斗**地面轴常量**（平常走路/站立那条线）；掉落落地 / Idle / Move / 跳攻落地贴回该轴；**禁止**抄玩家当前 `position.y`（含空中）  
**不是**：取消 JumpAtk；改村庄纵深；恢复 0723 GroundCld；推翻 0912「要同轴」（只改 **权威 Y 来源**）  
**前案**：`Assets/Doc/施工说明/0912/史莱姆Y轴同轴对齐_施工说明.md`（OPEN Q1 曾默认「实时玩家 y」——**本案推翻**）  
**提示词**：`Assets/Doc/提示词/0913/史莱姆同轴_权威Y改场景常量_架构侦探提示词.md`  
**施工说明**：`Assets/Doc/施工说明/0913/史莱姆同轴_权威Y改场景常量_施工说明.md`（2026-09-13 已按 A+A′ 落地）

---

## 沟通摘要

### ① 结论一句话

**飞起来是因为 0912 把权威 Y 设成了玩家实时高度**：玩家跳的时候史莱姆一进 Idle/Move/落地就会 `Snap` 到空中 Y，Idle 再把 Y 冻住，看起来就像跟着飞。权威应改成场景常量 **−6.61**（现网兜底已是这个数），不要再跟玩家跳。

### ② 原因（通俗）

0912 为了「掉树后跟雅儿一条线」，让史莱姆每次站定/走路/落地都去抄雅儿现在的高度。雅儿站着没问题；雅儿一跳，抄到的就是半空，史莱姆就被拽上去。产品要的同轴是「地面跑道」，不是「人在哪史莱姆在哪」。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网预期（未修） |
|---|------|------------------|
| 1 | ForestEast：史莱姆 Idle/追击时玩家连跳 | 史莱姆 Y 会被拉高（尤其刚切 Idle 时冻在空中） |
| 2 | 对照：玩家站稳再看双方 Y | 接近同高（这是 0912 还「对」的部分） |
| 3 | 树上掉落落地（玩家站着） | 仍应贴地面轴；改常量后这条应更好/不变差 |
| 4 | 拍板：权威 = **−6.61f**（三战斗场共用） | 本期不必加 Config 字段 |
| 5 | 拍板：JumpAtk `endPos.y` 是否本期一并改常量 | 建议 **是**（否则跳着打仍会飞特别高） |

### ④ 程序补充

见下文 §1～§6。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **「玩家跳史莱姆飞」** | **是** — `TryResolveCombatAxisY` **优先** `player.transform.position.y`；地面态 Enter 即 Snap |
| **加重** | `Idle.Enter` Snap 后 `FreezePositionX\|Y` → 抄到空中高度就钉在空中；强制 `IsGrounded=true` 还会挡住重力拉回 |
| **次要** | JumpAtk `endPos = atkTarget.position`（含空中 Y），顶点 `endPos.y+3` → 玩家跳时跳攻更高 |
| **权威应改为** | 场景战斗轴常量 **`−6.61f`**（把现 `CombatAxisYFallback` 升为唯一权威） |
| **多场景** | ForestEast / VerdantCorridor / WestRappRoad 地面 EnterPos / 地面怪标尺均为 **−6.61**；未发现第二套战斗轴 |
| **Config** | `GameSceneManagerConfig` **无** 轴高字段；本期不必扩 SO（OPEN Q2） |
| **atkTarget.y 兜底** | **应拿掉**（与玩家实时 Y 同类：空中目标会抬轴） |
| **Dead** | **不调** Snap（0913 死亡案已禁）；本案勿加 |
| **推荐** | **方案 A**：`TryResolveCombatAxisY` 只返回常量；**建议同期 A′**：JumpAtk `endPos.y` 用常量、`endPos.x` 仍跟玩家 |

---

## 2. 调用链（谁 Snap、权威怎么解析）

```
地面态 Enter
  SlimeBornDownState.Enter     → SnapToCombatAxisY()   // 掉树落地
  SlimeJumpAtkDownState.Enter  → SnapToCombatAxisY()   // 跳攻落地
  SlimeIdleState.Enter         → SnapToCombatAxisY()
                               → FreezePositionX|Y     // ★ 抄到哪钉到哪
                               → IsGrounded = true     // ★ 空中也不再吃重力
  SlimeMoveState.Enter         → SnapToCombatAxisY()
                               → FreezeRotation only

SnapToCombatAxisY
  → TryResolveCombatAxisY(out axisY)
       ★ 现网：
         1) player.transform.position.y     // 含跳跃
         2) else atkTarget.position.y       // 含空中目标
         3) else CombatAxisYFallback −6.61
       ★ 产品要：只返回场景常量（−6.61）
  → |slime.y − axisY| > 0.05 则写 transform + Rigidbody2D.position
  → 清竖直速度

JumpAtk（不走 Snap，但写 Y）
  SlimeJumpAtkUpBefore.Enter
    endPos = atkTarget.position          // ★ 含玩家空中 y
  SlimeJumpAtkUpState.FixedUpdate
    maxHeightPos = (endPos.x, endPos.y + 3)
  SlimeJumpAtkFallState
    抛物线落到 endPos（y 仍是起跳时的玩家 y）
  → 落地再 JumpAtkDown Snap（现网再抄一次玩家 y）

Dead：不调用 Snap（已核实）
```

**复现钉死（代码预判）**

| 情境 | 现网 |
|------|------|
| 史莱姆已在 Idle、玩家只跳、怪不切态 | Y 已冻，**不一定**跟飞（用户体感「一直跟」多半来自切态） |
| 玩家跳时怪进 Idle / Move / 跳攻落地 | **Snap 到空中 Y** → Idle 则冻在空中 |
| 攻击结束回 Idle、索敌进 Move | 战斗中高频 Enter → 体感「一直跟」 |
| 玩家跳时怪发动 JumpAtk | 顶点/落点 y 被抬高 |

不是 Update 每帧追 Y；是 **每次进地面态抄一次实时玩家 Y**，效果上仍会跟跳。

---

## 3. 证据表

| # | 证据 | 结论 | 状态 |
|---|------|------|------|
| E1 | `TryResolveCombatAxisY` L105：`axisY = player.transform.position.y` | 权威 = 实时玩家 Y | ✅ |
| E2 | 注释写明「优先当前玩家 position.y」 | 与 0912 OPEN Q1 一致 | ✅ |
| E3 | Snap 调用点仅 BornDown / JumpAtkDown / Idle / Move | 地面态才会飞；升空中不 Snap | ✅ |
| E4 | Idle Snap 后 `FreezePositionY` + `IsGrounded=true` | 空中 Snap 后钉住、重力被骗过 | ✅ |
| E5 | `CombatAxisYFallback = -6.61f` 已存在，仅无玩家时用 | 常量现成，升权威即可 | ✅ |
| E6 | ForestEast EnterPos / 地面标尺 y=−6.61（0912 E1–E2；场景仍见 −6.61） | 战斗轴 = −6.61 | ✅ |
| E7 | WestRappRoad 多处 y=−6.61（含 Enter 类坐标） | 同轴高 | ✅ |
| E8 | 树上实例 Y≈7.41 是**出生点**不是战斗轴 | 落地应 Snap 到 −6.61，不是跟树、也不是跟跳 | ✅ |
| E9 | `GameSceneManagerConfig` 无轴高字段 | 本期用代码常量即可 | ✅ |
| E10 | JumpAtk `endPos` 整份 `atkTarget.position`；顶点 `+3` | 次要飞高同源 | ✅ |
| E11 | Dead 不调 Snap | 排除死亡案误伤 | ✅ |
| E12 | 村庄 −6.61 是 DNF **纵深**标尺，语义不同 | **禁止**把本案接到 Town 纵深 | ✅ 边界 |
| E13 | Play：跳 10 次史莱姆 y 是否跟升 | 侦探未实机；链路已足够定性 | ⚠️ 待验收 |

### 嫌疑复核

| 项 | 裁定 |
|----|------|
| **A Snap→玩家实时 Y** | **主因 · 已证实** |
| **B JumpAtk endPos.y** | **次要同源**；建议本期纳入 |
| **C 多场景轴高不统一** | **本期未发现**；三场均为 −6.61 |
| **D Dead 误 Snap** | **排除** |

---

## 4. 与 0912 OPEN Q1 对照（推翻原因）

| | 0912 施工默认 | 本案（用户钉死） |
|--|----------------|------------------|
| Q1 权威 Y | **实时玩家 y**；无玩家用 −6.61 | **场景常量 −6.61**；玩家跳 **不参与** |
| Q2 地面怪也 Snap | 是 | **保持**（仍 Snap，只是对齐常量） |
| Q3 跳攻空中不同轴、落地再齐 | 是 | **保持**；落地齐的是常量轴，不是玩家空中 Y |

0912 报告里其实已写过「或场景 `CombatAxisY` 常量」，施工选了实时玩家 y，才会在跳跃时暴露。  
**同轴目标不推翻**：掉树/走路仍要贴地面跑道；只改「跑道高度从哪来」。

---

## 5. 修复方案对比

### 方案 A（产品指定 · 推荐）

`TryResolveCombatAxisY` **只返回** `CombatAxisYFallback`（可改名为 `CombatAxisY`，值 **−6.61f**）。删除玩家 / atkTarget 分支。更新注释：权威 = 横版战斗地面轴，**不是**玩家瞬时 Y，**不是**村庄纵深。

JumpAtk **建议同期 A′**（最小、同一文件族）：

```
endPos.x = atkTarget.x（仍追人左右）
endPos.y = CombatAxisY
maxHeightPos.y = CombatAxisY + 3   // 既有跳高，不再叠玩家空中 y
```

落地 `JumpAtkDown` 仍 Snap 到同一常量。

| 利 | 弊 |
|----|-----|
| 改动面集中 `BaseSlimeState`（+可选 JumpAtk 两处 y） | 若将来有第二套轴高须再加配置 |
| 玩家跳不再抬史莱姆；掉树仍回 −6.61 | 玩家若站在真·高台（非跳跃），史莱姆不跟高台 Y（现网战斗场地面轴统一，可接受） |

### 方案 B（对照 · 不推荐）

仅当玩家 `IsGrounded` 才用玩家 y，否则用常量。

| 利 | 弊 |
|----|-----|
| 站着时仍跟脚底微差 | 用户已倾向纯常量；落地判定抖动仍可能偶发抬轴 |
| | 高台/斜坡若出现会跟脚，和「场景常量」口径不一致 |

### 方案 C（禁止）

继续优先实时玩家 y。

### 方案 D（过大 · 本期否）

给 `GameSceneManagerConfig` 加 `combatAxisY`，每场景填。现网三场同值，无必要。

### 推荐与禁止

| 项 | 内容 |
|----|------|
| **推荐** | **A + A′**：常量权威 Snap；JumpAtk 落点/顶点 y 用常量 + 既有 +3 |
| **不要动** | Snap **调用点**（仍只地面态 Enter）；Dead；0723 GroundCld；村庄 Town；关掉 JumpAtk；Update 每帧追 Y；BornFall 重写 |

### 回归与验收

| 风险 | 验收 |
|------|------|
| 掉树不同轴 | 树上掉落后与**站立**玩家目视同轴（Δy 小，约 ≤0.2） |
| Move/Idle 抬飞 | 切态时 Y 停在 −6.61 一带 |
| 玩家跳 | 旁观/追击史莱姆 **Y 不跟飞** |
| JumpAtk | 仍能跳、落地回轴；若做了 A′，顶点不因玩家空中离谱 |
| 死亡定格 0913 | 抽测尸体不掉屏、不误 Snap |
| 0723 | 人踩壳不卡死 |

---

## 6. OPEN_QUESTIONS

已记入 `Assets/Doc/OPEN_QUESTIONS.md`；并应把 0912 Q1 标为 **本案推翻**。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 权威 Y 取值？ | **−6.61f**（三战斗场共用代码常量） | 待确认 |
| Q2 | 是否给 Config 加 per-scene 轴高？ | **本期否** | 待确认 |
| Q3 | JumpAtk `endPos.y` / 顶点是否本期改常量轴？ | **建议是（A′）** | 待确认 |
| Q4 | 0912 施工说明 Q1 是否改写为「已推翻」？ | **是**（0913 施工说明写明即可） | 待确认 |

---

## 7. 给施工员的一句话

**`TryResolveCombatAxisY` 只返回 −6.61；建议 JumpAtk 只跟玩家 X、Y 用同一常量；不要再抄玩家跳跃高度。**
