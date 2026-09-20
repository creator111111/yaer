# 史莱姆 Y 轴同轴对齐 — 施工说明

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【施工员】按侦探报告方案 **A** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0912/史莱姆Y轴同轴对齐_掉树与移动_架构溯源报告.md`  
**根因**：树上实例 Y≈7.41 落地只认 `IsGrounded`、不 Snap；Move 只改 X → 落偏则一直偏

---

## 沟通摘要

### ① 结论一句话

**史莱姆地面态（掉树落地 / 跳攻落地 / Idle / Move）会把 Y 对齐到场景战斗轴 −6.61（0913 起）；掉落后应与站立雅儿同一条水平线，玩家跳时怪不跟飞。**

### ② 原因（通俗）

树上史莱姆本来就站在更高的「另一条跑道」。掉下来只问「碰到地没有」，不跟雅儿对齐；落地后再走路只左右挪。所以掉歪了就一直歪。现在落地和站着走路时会把高度拉齐。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | ForestEast 触发树上 `Slime (1)/(2)/(6)` 掉落 | 落地后与玩家 **目视同轴**；双方 `position.y` 差 ≤0.2 |
| 2 | 掉落后追击/巡逻 ≥10s | 相对玩家 **无持续上下漂** |
| 3 | 普攻 + 跳跃攻击 | 仍可命中；跳起过程可短暂不同轴，落地再齐 |
| 4 | 不踩 0723 | 人不会站在史莱姆壳上卡住 |
| 5 | 抽测 VerdantCorridor / WestRapp 高台史莱姆各 ≥1 | 同轴同样成立 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A 核心** | `BaseSlimeState.cs` | `SnapToCombatAxisY()`：权威 Y = 玩家 → atkTarget → 兜底 −6.61；同步 `transform`+`Rigidbody2D`，清竖直速度 |
| **BornDown** | `SlimeBornDownState.cs` | Enter 里 Snap（掉树主路径） |
| **JumpAtkDown** | `SlimeJumpAtkDownState.cs` | Enter 里 Snap（跳攻落地） |
| **Idle** | `SlimeIdleState.cs` | Enter Snap；`FreezePositionX\|Y`（防站立残速顶偏） |
| **Move** | `SlimeMoveState.cs` | Enter Snap |
| **JumpAtk 升空** | `SlimeJumpAtkUpBefore.cs` | 注释标明解冻 Y（原已 `FreezeRotation`） |

### 未改（本期禁止 / 不做）

- BornFall 双驱动重写（方案 C）
- 场景树实例初始 Y 下调（方案 B，可另案叠加）
- `GroundCld` 实心 / PlayerFoot / 村庄 Town 纵深
- 给 `IsInSameDepth` 关检测

---

## 权威 Y（OPEN Q1～Q3 施工默认）

| 项 | 取值 |
|----|------|
| Q1 | ~~实时玩家 y~~ → **0913 已推翻**：场景常量 **−6.61**（见 `施工说明/0913/史莱姆同轴_权威Y改场景常量_施工说明.md`） |
| Q2 | 地面怪进 Idle/Move **也 Snap**（保持） |
| Q3 | JumpAtk 空中允许不同轴，**仅落地 Snap**（落地齐常量轴；A′ 起跳落点 y 亦用常量） |

**替代**：若验收仍偶发过冲，再议方案 C（单通道重力 + hit 点 Snap），勿再只 Freeze 不 Snap。
