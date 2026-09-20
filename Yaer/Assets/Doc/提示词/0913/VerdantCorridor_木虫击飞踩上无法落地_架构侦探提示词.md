# Cursor Agent Prompt · 苍翠走廊木虫：击飞/踩上后玩家无法落地

> **角色**：先【架构侦探】只读核实；拍板后【施工员】按 0723 同族策略最小修复  
> **日期**：2026-09-13  
> **场景**：`VerdantCorridor` → `Monster/wormBattleEvent` → 多只 `WoodWorm (*)`（用户箭头指 `WoodWorm (8)`）  
> **现象（用户实测 + 截图）**：  
> 1. 虫子把玩家 **击飞** 后，玩家 **无法落地**（悬在虫上方/半空横躺）  
> 2. 玩家 **跳到虫子身上** 同样 **无法落地**  
> **产品期望（钉死）**：木虫 **不当踏板**；击飞后能正常落回地面；跳到虫身/虫顶应落下或穿过，**不**粘在虫上死等落地  
> **用户疑问**：原来有这个碰撞吗？不正常。  
> **不是**：改木虫攻击数值/AI；改 Physics2D 全局矩阵（0723 已否决当首选）；改玩家 GroundLayerMask 去认怪当地面；恢复挤出订阅（除非报告+OPEN 明确要求）  
> **对照前案（必读）**：  
> - `执行文档/7月/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`  
> - `执行文档/7月/0723/ForestEast_藤蔓站上卡住_架构溯源报告.md`  
> - 史莱姆修复：运行时 `groundCld.isTrigger = true`（`Slime.cs`）  
> **报告落盘**：`Assets/Doc/执行文档/0913/VerdantCorridor_木虫击飞踩上无法落地_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 走廊打木虫时，被虫打飞或跳到虫身上，人会卡住落不下来，像粘在虫壳上。  
> 这不正常。虫子不该当地板。查清是不是和以前史莱姆/藤蔓「站上卡住」同一类，再按同一套办法修。

### 「原来有这个碰撞吗？」— 助手预答（须核实）

| 设计意图 | 现状问题 |
|----------|----------|
| `GroundCld` 图层 **OnlyMapObj(7)**：本意让怪 **和地图/场景物** 碰，方便站地 | Prefab 里 **`m_IsTrigger: 0`（实心）**，会和 **PlayerFoot** 顶住 |
| Body/攻击盒多为 Trigger：受伤/打人用，不挡刚体 | 人脚踩到实心 `GroundCld` → 被托住 |
| 玩家落地只认 `GroundCenter\|GroundCommon` | **不认** OnlyMapObj → `IsGrounded=false` → `JumpFall`/`DamageFlyFall` **死等落地** |

→ **碰撞组件原来就有**（设计给「怪碰地」），**当玩家踏板是副作用，属 0723 同族残留**；史莱姆已运行时改 Trigger，**木虫未跟**。

### 术语钉死

| 词 | 本案含义 |
|----|----------|
| **无法落地** | 下落/击飞态下长时间 `IsGrounded==false`，或物理上被托在虫顶无法落到 Ground 层 |
| **击飞** | 木虫攻击导致的击退/击飞（`DamageFly*` 等） |
| **踩上** | 跳跃落下与虫身/`GroundCld` 发生实心接触 |
| **同族** | 与史莱姆/战斗藤蔓：`OnlyMapObj` 实心 `GroundCld` + 落地 Mask 不认 + 下落态死等 `IsGrounded` |

### 现网预扫（须核实）

```
WoodWorm.prefab / WoodWorm_1.prefab
  Cld/GroundCld：Layer=7 OnlyMapObj，m_IsTrigger=0（实心）
  盒约 Size (5.29 × 1.36)、Offset.y≈0.49（横向大、盖住虫身高度）
  Body1 / 其它：多为 Trigger

WoodWormLogic.OnInit：
  groundCld.layer = onlyMapObjLayer(7)
  ★ 无 groundCld.isTrigger = true（对比 Slime.cs 已有）

BaseMonster.OnDead：
  groundCld.enabled = false   // 仅死后关；存活态仍挡人

玩家：
  CapsuleGroundChecker → Mask 不含 OnlyMapObj
  JumpFall / DamageFlyFall → 等 IsGrounded
```

场景：`VerdantCorridor/Monster/wormBattleEvent` 下 `WoodWorm (3)…(8)` 等（截图虫群）；含 `eventLeft/RightCollider` 为战斗区界，**勿与虫 GroundCld 混淆**。

### 嫌疑优先级

| # | 嫌疑 | 体感 |
|---|------|------|
| **A（主 · 同族）** | 存活 `GroundCld` 实心托住 PlayerFoot | 踩上/击飞落在虫顶卡住 |
| **B** | 下落态死等 `IsGrounded`，Mask 不认虫 | 粘住不切落地 |
| **C** | 挤出/拨开逻辑已关（0723） | 无侧向推离 |
| **D** | 仅场景实例覆盖 / 巢生 `WoodWorm_1` | 部分虫卡、部分不卡 |
| **E** | `wormBattleEvent` 左右墙 Collider | 卡在边界而非虫顶——须排除 |
| **F** | 攻击盒误挡 | Body 已是 Trigger 则次要 |

### 复现矩阵（侦探须填）

| 操作 | 期望（产品） | 现网 |
|------|--------------|------|
| 被木虫击飞后自然下落 | 落到草地，可站立移动 | |
| 主动跳到虫背/虫顶 | 不粘住；落下或穿过 | |
| Physics2D：PlayerFoot↔该虫 `GroundCld` 是否有接触 | 修前应有 | |
| 对照：同场景史莱姆（若有）踩上 | 应不卡（已 Trigger） | |
| 打死虫后再落其位置 | 死后 GroundCld 应关，不挡 | |

### 侦探须回答

1. 是否 **0723 同族**（实心 GroundCld + OnlyMapObj + 死等 IsGrounded）？证据？  
2. 「原来有碰撞吗」：设计意图 vs 挡人副作用，写进结论。  
3. `WoodWorm` / `WoodWorm_1` / 场景覆盖是否一致？`WoodWormRoot` 是否也有实心 GroundCld？  
4. **方案 ≥2**（对齐史莱姆决议优先）：  
   - **A（推荐）**：`WoodWormLogic`（及 `_1` 若共用/分逻辑）OnInit：`groundCld.isTrigger = true`；或 Prefab 直接改 Trigger；回归怪是否还能站地  
   - **B**：Prefab 压扁 GroundCld 为贴地薄条（改动大、易漏实例）  
   - **C**：改 Physics2D 矩阵 Ignore PlayerFoot↔OnlyMapObj（**0723 否决首选**，挡板风险）  
   禁止恢复挤出除非 OPEN 另批。  

### 必读

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`  
3. 0723 史莱姆 + 藤蔓溯源报告  
4. `Slime.cs`（`groundCld.isTrigger = true` 注释与实现）  
5. `WoodWormLogic.cs` / `WoodWorm.prefab` / `WoodWorm_1.prefab`  
6. `BaseMonster.OnDead`（死后关 groundCld）  
7. 玩家 `CapsuleGroundChecker` / `JumpFall` / `DamageFlyFall`  
8. 本提示词 + 用户 Hierarchy/Game 截图  

### 禁止（侦探阶段）

- 禁止改代码 / Prefab / 场景 / Git  
- 禁止首推改全局碰撞矩阵  
- 禁止把木虫 Ground 层加进玩家落地 Mask（会把虫当地板）  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0913/VerdantCorridor_木虫击飞踩上无法落地_架构溯源报告.md`

结构：

1. **结论一句话**（是否同族 GroundCld；原来有无「该挡人」的设计）  
2. **调用链**（击飞/跳跃下落 → 接触 → IsGrounded → 状态机）  
3. **证据表**（Prefab Trigger/Layer/Size；与史莱姆对照）  
4. **影响面**（走廊 wormBattleEvent、其它场景木虫、Root/巢生）  
5. **方案 ≥2** + 推荐（默认对齐史莱姆 Trigger）+ 验收 + 回归（怪站地、挡板、0723）  
6. 不清处记 `OPEN_QUESTIONS.md`

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告确认与 0723 同族；推荐 GroundCld→Trigger（或报告等价最小方案）。  
> **目标**：VerdantCorridor 木虫击飞后可落地；跳到虫上不粘死；死后仍不挡路。  
> **优先**：对齐 `Slime.cs`——在 `WoodWormLogic`（及需要的 `_1`）Init 里 `groundCld.isTrigger = true` + 详细注释；或改 Prefab `m_IsTrigger=1`（报告择一，实例/变体要扫全）。保留 `BaseMonster.OnDead` 关盒。  
> **禁止**：改全局 Physics2D 矩阵；玩家落地 Mask 加入 OnlyMapObj；恢复挤出订阅；改木虫伤害/AI；把 Root 挡板误改成可穿（若 Root 是障碍须单独裁定）。  
> **文档**：`Assets/Doc/施工说明/0913/VerdantCorridor_木虫GroundCld不挡落地_施工说明.md`  
> **验收**：  
> 1）击飞 ≥5 次：均能落回地面站立；  
> 2）主动跳上虫背 ≥5 次：不长期悬空/横躺卡死；  
> 3）Physics：存活态 Foot↔GroundCld 不再实心托住（Trigger 后无阻挡）；  
> 4）木虫自身仍能在地面行动；  
> 5）抽测史莱姆/藤蔓不回归；虫巢墙/Root 若存在仍按设计挡或通。  

---

## 【验收员】Prompt（可选）

> Debug `[WoodWormGroundCld]`：Init 后打 `groundCld.isTrigger` / layer / enabled；卡死时打玩家 `IsGrounded`、是否与该虫 GroundCld 有 Contact。  
> 输出：通过项 + 剩余风险（场景覆盖、WoodWorm_1、Root）。
