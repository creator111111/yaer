# VerdantCorridor 木虫 · 击飞/踩上无法落地 — 架构溯源报告

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**场景**：`VerdantCorridor` → `Monster/wormBattleEvent` → 多只 `WoodWorm (*)`（用户箭头 `WoodWorm (8)`）  
**现象**：虫子把玩家击飞后无法落地（悬在虫上方/半空横躺）；跳到虫身同样粘住落不下来  
**产品期望**：木虫 **不当踏板**；击飞后落回真地面；跳到虫身应落下或穿过，不粘死  
**用户疑问**：原来有这个碰撞吗？不正常。  
**不是**：改木虫攻击数值/AI；改 Physics2D 全局矩阵；玩家落地 Mask 加入 OnlyMapObj（会把虫当地板）；恢复挤出订阅（除非另批）  
**对照**：  
- `执行文档/7月/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`  
- `执行文档/7月/0723/ForestEast_藤蔓站上卡住_架构溯源报告.md`（`TenWanLogic` 已跟史莱姆改 Trigger）  
**提示词**：`Assets/Doc/提示词/0913/VerdantCorridor_木虫击飞踩上无法落地_架构侦探提示词.md`  
**施工说明**：`Assets/Doc/施工说明/0913/VerdantCorridor_木虫GroundCld不挡落地_施工说明.md`（2026-09-13 已按 A + 虫巢落地）

---

## 沟通摘要

### ① 结论一句话

**是 0723 同族残留，不是「木虫该当地板」**：`GroundCld` 本来只给怪和地图碰，Prefab 却做成实心大盒；人脚踩上去会被托住，落地检测又不认这层，下落态就一直等落地。史莱姆/战斗藤蔓已经运行时改成 Trigger，**木虫没跟**。碰撞组件原来就有，挡人是副作用。

### ② 原因（通俗）

虫子脚下有一块「只该顶地板、不该顶人」的厚钢板。人被打飞或跳上去，脚踩在钢板上，游戏却不认为这是地面，于是既落不下去、也切不回站立。原来那套「把人从怪身上拨下去」的挤开也关了。史莱姆、战斗藤蔓修过同一件事；走廊这群木虫还是旧 Prefab。

左右 `eventLeft/RightCollider` 是战斗区墙，默认还关着，**不是**粘在虫顶的原因。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网预期（未修） |
|---|------|------------------|
| 1 | 走廊 `wormBattleEvent`：被木虫打飞 ≥3 | 易落在虫顶/半空；动画停 JumpFall / DamageFlyFall 或横躺，`IsGrounded=false` |
| 2 | 主动跳上虫背 ≥3 | 粘住，不落到草地 |
| 3 | Pause：该虫 `Cld/GroundCld` | Layer=OnlyMapObj(7)，**Is Trigger=false**，盒约 5.3×1.4 |
| 4 | Physics2D：PlayerFoot ↔ 该虫 GroundCld | 修前应有实心接触（矩阵不对称，以 Debugger 为准） |
| 5 | 对照同图史莱姆（若有）踩上 | **应不卡**（`Slime.OnInit` 已 Trigger） |
| 6 | 打死虫后再落到原位 | `BaseMonster.OnDead` 已 `groundCld.enabled=false`，死后一般不挡 |
| 7 | 拍板方案 **A** | `WoodWormLogic.OnInit` 对齐史莱姆：`groundCld.isTrigger=true` |

### ④ 程序补充

见下文 §1～§6。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **是否 0723 同族** | **是**：存活实心 `GroundCld` + Layer OnlyMapObj + 玩家 Mask 不认 + `JumpFall`/`DamageFlyFall` 死等 `IsGrounded` |
| **原来有碰撞吗** | **有**。设计给「怪碰地图」；**当玩家踏板是副作用**，不是产品要的地板 |
| **木虫是否已修** | **否**。`WoodWormLogic.OnInit` 只把 GroundCld 设为 layer 7，**无** `isTrigger=true` |
| **史莱姆 / 战斗藤蔓** | `Slime.cs` / `TenWanLogic.cs` 已在 OnInit 改 Trigger；木虫是漏网 |
| **WoodWorm vs `_1`** | 两份 Prefab GroundCld 同构（实心、OnlyMapObj、≈5.29×1.3）；**都挂 `WoodWormLogic`**，改 OnInit 一次覆盖场景虫 + 巢生 |
| **场景覆盖** | 走廊实例 guid=`WoodWorm.prefab`，未见改 GroundCld Trigger → **不是「部分虫卡」** |
| **WoodWormRoot** | 同样实心 GroundCld（≈9.2×3.1，OnlyMapObj）；**不是** TenWanSceneObj 那种砍断挡板。踩巢会同样卡。建议同期 Trigger（OPEN Q1） |
| **event 左右墙** | 默认 **Inactive**；战斗界墙，勿与虫盒混淆 |
| **攻击盒** | Body1 / Foot / CollArea 均为 Trigger → **F 排除** |
| **推荐** | **方案 A**：`WoodWormLogic.OnInit` `groundCld.isTrigger=true`（注释对齐史莱姆）；保留死后关盒 |

---

## 2. 调用链（击飞/跳跃 → 接触 → IsGrounded → 状态机）

```
木虫攻击 CollArea_NorAtk（atkType=Normal，breakHight=1，Trigger）
  地面受伤：Damage1/2 + KnockBack（bounceHeight=breakHight）
  空中受伤（IsJumping）：无条件 DamageFly（SetDamageFlyHeight）
  BreakType：DamageFlySM → DamageFlyFall 等 IsGrounded

玩家跳跃下落
  CombatJumpSM → JumpFallState：if (IsGrounded) 才切 FallDownIdle/Run

落地检测 CapsuleGroundChecker
  只向下 Raycast；useTriggers=false
  GroundLayerMask = GroundCenter|GroundCommon（Player Prefab bits=1064960，无 bit7）
  ★ 不认 OnlyMapObj → 踩在虫 GroundCld 上 IsGrounded 仍 false

物理接触
  Body1 / Foot = Trigger（不挡刚体）
  GroundCld = 实心、OnlyMapObj、盒 5.29×1.36、Offset.y≈0.49（盖住虫身高度）
  玩家 Mass 远小于虫 Mass=9999；挤开订阅已关
  → 人被托在虫顶，下落态切不出去

死后
  BaseMonster.OnDead：groundCld.enabled=false
  → 尸体一般不再挡；本案是存活态
```

「无法落地」= 物理上被托在非地面层 + 逻辑上 `IsGrounded==false` 死等。不是 AI、不是伤害数字。

---

## 3. 证据表

| # | 证据 | 结论 | 状态 |
|---|------|------|------|
| E1 | `WoodWorm.prefab` GroundCld：Layer=7，`m_IsTrigger: 0`，Size≈(5.29, 1.36)，Offset.y≈0.49 | 实心大盒盖虫身 | ✅ |
| E2 | `WoodWorm_1.prefab` GroundCld：Layer=7，Trigger=0，Size≈(5.29, 1.31) | 巢生变体同构 | ✅ |
| E3 | Body1 / Foot `m_IsTrigger: 1`；攻击盒 Trigger | 挡人的只有 GroundCld | ✅ |
| E4 | `WoodWormLogic.OnInit` 设 `groundCld.layer=onlyMapObjLayer`，**无** isTrigger=true | 漏修 | ✅ |
| E5 | `Slime.OnInit` / `TenWanLogic.OnInit` 已 Trigger + 同族注释 | 决议在先，木虫未抄 | ✅ |
| E6 | Player `GroundLayerMask` bits=1064960（层 14+20）；checker `useTriggers=false` | 不认虫盒为地面 | ✅ |
| E7 | `JumpFallState` / `DamageFlyFallState` 仅 `if (IsGrounded)` 切态 | 托住则横躺/下落死等 | ✅ |
| E8 | 虫 `GravityScale=0`、Mass=9999；落地靠 GroundChecker | GroundCld→Trigger **不应**让虫掉出地图（与史莱姆/藤蔓同） | ✅ |
| E9 | 走廊实例全部 `guid: 72441d0b…` = `WoodWorm.prefab`；未见改 GroundCld Trigger | 全员同病 | ✅ |
| E10 | Root 生虫：`WoodWorm_1.prefab` + `WoodWormLogic` | OnInit 修复覆盖巢生 | ✅ |
| E11 | `WoodWormRoot.prefab` GroundCld Trigger=0，Size≈(9.20, 3.09) | 同族；踩巢同样风险 | ✅ |
| E12 | `eventLeft/RightCollider` 默认 Active=0，Layer=8 | 排除为虫顶粘住主因 | ✅ |
| E13 | `BaseMonster.OnDead` 关 groundCld | 死后不挡，保留 | ✅ |
| E14 | 0723：不改矩阵、不加 Mask、不恢复挤出 | 本案沿用 | ✅ |
| E15 | Play 标 Contact / 卡死状态名 | 侦探未实机；链路足够定性 | ⚠️ 待验收 |

### 复现矩阵

| 操作 | 期望（产品） | 现网（静态） |
|------|--------------|--------------|
| 被木虫击飞后自然下落 | 落到草地，可站立移动 | 易被 GroundCld 托住，`IsGrounded=false`，下落态不结束 |
| 主动跳到虫背/虫顶 | 不粘；落下或穿过 | 同左，粘在虫顶 |
| Physics2D Foot↔GroundCld | 修后无实心阻挡 | 修前应有接触（以 Debugger 为准） |
| 对照同场景史莱姆踩上 | 不卡 | 已 Trigger，应不卡 |
| 打死虫后再落其位置 | 不挡 | OnDead 关盒，一般不挡 |

### 嫌疑复核

| 嫌疑 | 裁定 |
|------|------|
| **A 存活 GroundCld 实心托脚** | **主因 · 已证实** |
| **B 下落态死等 IsGrounded** | **机制叠加 · 已证实**（不要靠改 Mask 去「认虫」） |
| **C 挤出已关** | **加重**；禁止本期恢复 |
| **D 仅部分实例覆盖** | **排除**（同 Prefab、无 Trigger 覆盖） |
| **E 战斗区左右墙** | **排除为主因**（默认关；位置是界墙） |
| **F 攻击盒误挡** | **排除**（Trigger） |

---

## 4. 影响面

| 范围 | 说明 |
|------|------|
| **走廊 wormBattleEvent** | `WoodWorm (3)…(12)` 等 + 若干 `WoodWormRoot`；用户截图虫群 |
| **其它场景** | `ForestEastScene` / `Village_OutSide` / `WestRappRoad` 同样用 `WoodWormLogic` → **改 OnInit 全局受益**，属同族该修 |
| **WoodWorm_1** | 无独立 Logic 类；巢 `CreateWoodWorm` 走同一 OnInit |
| **WoodWormRoot** | 独立 `WoodWormRootLogic`，**不会**吃到 WoodWorm 的 Trigger。踩巢是否卡 = OPEN Q1 |
| **史莱姆 / 战斗藤蔓** | 已修，勿回滚 |
| **TenWanSceneObj / 天琬挡板** | **勿动**；不改矩阵正是为了保护它们 |
| **0723 虫卵** | 死后关盒已在基类；勿把卵存活挡路设计拆掉 |

未抽到 `BaseMonster`：0723 起意就是「按怪 Logic 抄 Trigger」，避免误伤场景挡板。本案继续只动木虫（+可选 Root），**不要**基类一刀切。

---

## 5. 修复方案对比

### 方案 A（推荐 · 对齐史莱姆）

`WoodWormLogic.OnInit`（`base.OnInit` 之后、已拿到 `groundCld`）:

```csharp
// GroundCld：Prefab 实心 + OnlyMapObj + 横向大盒，踩上/击飞落到虫顶会托住 PlayerFoot；
// 又不进 GroundLayerMask → JumpFall/DamageFlyFall 死等 IsGrounded（0723 史莱姆/藤蔓同族残留）。
// 怪落地靠 GroundChecker + GravityScale=0，GroundCld 本意「只和地图碰」；改 Trigger 后不再当玩家踏板。
// 替代：改 Physics2D 矩阵 / 把 OnlyMapObj 加进玩家 Mask（0723 否决）；恢复挤出（本期不恢复）。
if (groundCld != null)
{
    groundCld.isTrigger = true;
}
```

`WoodWorm` / `WoodWorm_1` / 运行时巢生全部走这里。可选再把 Prefab `m_IsTrigger` 改成 1（编辑器所见即所得），**以 OnInit 为准**，避免场景旧实例漏改。

保留 `BaseMonster.OnDead` 关盒。

| 利 | 弊 |
|----|-----|
| 与史莱姆/藤蔓同一决议；改动面最小 | 须抽测虫仍能在草地走 |
| 覆盖两份 Prefab + 巢生 + 全场景 | Root 不会自动修（见 Q1） |

### 方案 B（压扁 GroundCld 为贴地薄条）

改 Prefab 尺寸/Offset，让盒不再盖住虫背。

| 利 | 弊 |
|----|-----|
| 理论上仍实心碰地 | 两份 Prefab + 场景实例易漏；薄条仍可能托脚；与已落地的史莱姆策略分叉 |

**不优先**。

### 方案 C（禁止当首选）

`IgnoreLayerCollision(PlayerFoot, OnlyMapObj)` 或玩家 Mask 加入 OnlyMapObj。

0723 已否决：挡板/天琬/未砍断藤蔓风险；Mask 加入会把虫**当成真正地面**（另一种卡死）。

### 禁止

- 恢复 `PlayerBodyCollider` 挤出订阅  
- 改木虫伤害 / AI / `breakHight`  
- 改 `TownPlayerLocomotion`  
- 把 Root 当成「必须实心挡路」却不先 Play 确认（与 TenWanSceneObj 不同）

### 推荐与回归

| 项 | 内容 |
|----|------|
| **推荐** | **A**；Root 默认 **一并 Trigger**（Q1） |
| **不要动** | 矩阵、玩家 Mask、挤出、史莱姆/藤蔓已修代码、战斗区左右墙、TenWanSceneObj |
| **回归** | 木虫仍在地面行动；死后不挡；史莱姆/藤蔓不回滚；虫巢若 Trigger 后仍可打、可刷虫 |

### 验收标准

1. 击飞 ≥5：均能落回地面站立。  
2. 主动跳上虫背 ≥5：不长期悬空/横躺。  
3. Physics：存活态 Foot↔GroundCld **无实心阻挡**（Trigger）。  
4. 木虫自身仍能在地面左右移动。  
5. 抽测史莱姆/战斗藤蔓不回归。  
6. 打死虫后落其位置不挡。  
7. 若做了 Root：踩巢不粘；巢仍可被打、仍能生虫。

---

## 6. OPEN_QUESTIONS

已记入 `Assets/Doc/OPEN_QUESTIONS.md`：

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | `WoodWormRoot` GroundCld 是否本期 Trigger？ | **建议是**（同族战斗巢，非砍断挡板；盒更大，踩上同样卡） | 待确认 |
| Q2 | 是否同步改 Prefab `m_IsTrigger=1`？ | **建议是**（与 OnInit 双写；OnInit 仍是权威） | 待确认 |
| Q3 | 是否抽到 BaseMonster？ | **本期否**（避免误伤场景挡板） | 待确认 |
| Q4 | 矩阵 PlayerFoot↔OnlyMapObj 不对称是否另案？ | **本期否**（0723 决议） | 待确认 |

---

## 7. 给施工员的一句话

**按史莱姆/战斗藤蔓同一句：`WoodWormLogic.OnInit` 把 `GroundCld` 改成 Trigger；虫子不当地板。虫巢建议一起改，别动全局碰撞矩阵。**
