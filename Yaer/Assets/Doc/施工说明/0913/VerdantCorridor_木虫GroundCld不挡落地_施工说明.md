# VerdantCorridor 木虫 GroundCld 不挡落地 — 施工说明

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【施工员】按侦探报告方案 **A**；用户明确 **虫巢一并改**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0913/VerdantCorridor_木虫击飞踩上无法落地_架构溯源报告.md`  
**根因**：木虫/虫巢存活态 `GroundCld` 实心 OnlyMapObj 大盒托住 PlayerFoot，又不进落地 Mask → JumpFall/DamageFlyFall 死等（0723 史莱姆/藤蔓同族残留）

---

## 沟通摘要

### ① 结论一句话

**木虫和虫巢的 GroundCld 都改成 Trigger，不再当玩家踏板；击飞/跳上应能落到真地面。**

### ② 原因（通俗）

虫子脚下有块「只该顶地图」的厚钢板，Prefab 却做成实心。人被打飞或跳上去踩在钢板上，游戏又不认这是地面，就粘住落不下来。史莱姆/战斗藤蔓早就改过，木虫和巢漏了。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 走廊 wormBattleEvent：被木虫击飞 ≥5 | 均能落回地面站立 |
| 2 | 主动跳上虫背 ≥5 | 不长期悬空/横躺 |
| 3 | 踩虫巢 ≥3 | 不粘；巢仍可被打、仍能生虫 |
| 4 | Pause：存活 `GroundCld` | `Is Trigger=true` |
| 5 | 木虫自身 | 仍能在地面左右移动 |
| 6 | 打死虫后落其位 | 不挡（OnDead 关盒保留） |
| 7 | 抽测史莱姆/战斗藤蔓 | 不回归 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A 木虫** | `WoodWormLogic.cs` `OnInit` | `groundCld.isTrigger = true`（注释对齐史莱姆） |
| **巢生** | 同 · `initComponentData` | 找 GroundCld 后同样写 Trigger（WoodWorm_1） |
| **A′ 虫巢** | `WoodWormRootLogic.cs` `OnInit` | 同款 Trigger（用户要求一并改） |
| **Prefab 双写** | `WoodWorm.prefab` / `WoodWorm_1.prefab` / `WoodWormRoot.prefab` | GroundCld `m_IsTrigger: 1`（OnInit 仍是权威） |

### 未改

- Physics2D 矩阵 / 玩家 GroundLayerMask  
- 挤出订阅 / 木虫伤害 AI / TenWanSceneObj  
- 抽到 BaseMonster（避免误伤场景挡板）  

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 | WoodWormRoot **本期 Trigger**（用户确认） |
| Q2 | Prefab **同步** `m_IsTrigger=1` |
| Q3 | **不**抽 BaseMonster |
| Q4 | 矩阵不对称 **本期否** |
