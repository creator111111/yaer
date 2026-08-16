# ForestEast · 史莱姆站上卡住 & 树洞卵卡住 — 架构溯源报告

**文档版本**：v1.0（2026-07-23）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码**）  
**触发现象**：
- **A**：跳到 / 击飞落到史莱姆身上 → 卡住动不了，不会挤开掉落  
- **B**：`ForestEastScene` 树洞内被虫卵挡住无法前进（截图见 `Assets/Doc/提示词/0723/树洞_史莱姆与卵卡住_截图.png`）  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构侦探提示词.md`
- Prefab / Physics2D 矩阵 / 玩家·史莱姆·虫卵·树洞脚本静态阅读
- 对照：`Assets/Doc/执行文档/0722/ForestScene_*`（落地检测修复）

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**A：不是把史莱姆当地面；是「怪脚下的实心 `GroundCld`（OnlyMapObj）+ 挤开逻辑被关掉 + 下落态死等 `IsGrounded`」叠在一起——人像粘在史莱姆壳上，既落不下来也挤不开。**  
**B：树洞设计是爬行 + 蹲击打碎虫卵开路；现状是爬行碰到卵会 `StopMove`，卵的 `GroundCld` 死后还不关，头顶 E 只是裂缝旁白不是开路键——所以会永久/半永久卡在卵前。**

生活类比：  
- A：脚踩在一块「只该顶地板、不该顶人」的厚钢板（`GroundCld`）上，钢板又不算地板（落地灯不亮），原来的「把人从怪身上拨下去」的手也被绑住了。  
- B：洞里规定必须爬着走、用蹲击砸开卵才能过；可爬的时候一蹭到卵就被刹车，砸破了挡板还在，旁边的 E 只是路牌「洞裂了快走」，不是「拆路障」。

---

## ② 原因（两条链）

### 问题 A — 落到史莱姆身上卡住

#### 链路表

| 阶段 | 谁 | 做什么 | 结果 |
|------|-----|--------|------|
| 1 起跳/击飞 | `PlayerMoveComponent.SetJumpSpeed` / `SetDamageFlySpeed` | 写抛物线 `Velocity` | 空中 |
| 2 落地检测 | `CapsuleGroundChecker`：**只向下** Raycast；`GroundLayerMask=GroundCenter\|GroundCommon`（`1064960`）；`useTriggers=false` | **不会**把史莱姆 Body/Foot/GroundCld 判成地面 | |
| 3 史莱姆碰撞 | `Cld/Body`、`Cld/Foot` → **Trigger**；`GroundCld` → **非 Trigger**、Layer **OnlyMapObj(7)**、盒约 **7.3×2.9** | Body/Foot 设计上不挡刚体；**GroundCld 是唯一实心大盒** | |
| 4 质量/冻结 | 史莱姆 `Rigidbody2D.Mass=9999`；`SlimeIdleState` **FreezePositionX** | 人几乎挤不动史莱姆 | |
| 5 状态机 | `JumpFallState` / `DamageFlyFallState`：`if (IsGrounded)` 才切落地态 | 若人被非地面物体托住而 Mask 打不中 → **长期 `IsGrounded=false`，卡在下落态** | |
| 6 挤开 | `PlayerBodyCollider` 里 `OnCollisionShowEventInSpcAction`（实体碰撞挤出）**订阅已被注释** | 「跳到怪身上被挤开掉落」的旧设计 **当前未生效** | |

```
跳跃/击飞下落
  → CapsuleGroundChecker 不认史莱姆为地面
  → 唯一实心盒 GroundCld（OnlyMapObj）可能把 PlayerFoot 顶住（见下方矩阵注）
  → JumpFall / DamageFlyFall 等不到 IsGrounded
  → 挤开脚本已禁用 + 史莱姆 Mass/冻 X → 无法挤开掉落
```

#### 根因归类（对应提示词选项）

| 选项 | 是否像 | 说明 |
|------|--------|------|
| 地面检测把史莱姆当地面 → `IsGrounded=true` | **不太像（排除为主因）** | Mask 仅 GroundCenter+GroundCommon；Trigger 且不在 Mask |
| 非 Trigger 实体碰撞 + 速度/状态机卡死 | **最像（P0）** | `GroundCld` 实心 + 下落态死等 `IsGrounded` + Mass/冻 X |
| 击飞未清 IgnoreCollision | **不太像** | 战斗路径无玩家↔史莱姆 `IgnoreCollision` |
| 其它：挤开设计缺失 | **P1 加重** | 挤出订阅被注释；Trigger 路径 `OnCollisionMonster` 只 `StopMove` 不推开 |

#### Physics2D 矩阵注（施工前必验）

| 层 | 索引 | 与对方 |
|----|------|--------|
| PlayerFoot | 3 | `matrix[3]` **含** OnlyMapObj 位 |
| OnlyMapObj | 7 | `matrix[7]` **不含** PlayerFoot 位 |
| Player | 11 | 与 OnlyMapObj **双边都不碰** |

矩阵 **不对称**。Unity 正常应用 `IgnoreLayerCollision` 会双边同改；MelvMay 说明冲突 mask 可能导致无接触。  
因此：**静态上 `GroundCld` 是唯一合理「托住玩家」的实心盒，但运行时是否真有 PlayerFoot↔GroundCld 接触，必须用 Physics Debugger / Contact 一眼钉死**（见 §③）。  
无论接触是否成立：`GroundCld` 过大、挤开关闭、下落态只认地面——三条都成立，修复方向不变。

#### 「挤开掉落」有没有设计？

| 机制 | 状态 |
|------|------|
| Body/Foot 用 Trigger 穿怪 | Prefab 如此；状态机里改 `isTrigger` 的代码多已注释 |
| `OnCollisionShowEventInSpcAction` 脚本挤出 | **已禁用（注释）** → 设计存在但坏了/关掉了 |
| `Physics2D.IgnoreCollision` 玩家↔怪 | 战斗路径 **无** |
| 把 OnlyMapObj 当落地层 | **没有，也不该加**（会另类卡死） |

---

### 问题 B — 树洞被卵卡住

#### 设计意图 vs 现状

| 项目 | 设计意图 | 当前实际 |
|------|----------|----------|
| 通关一句话 | 进树洞强制爬行 → **蹲击打碎挡路虫卵** → 走到 `PassTreeBridge` | 卵前爬不动；E 不消碰撞；打碎后 `GroundCld` 可能仍挡 |
| 进洞 | `ForestEastTreeEnterTrigger` → `isInTreeBridge`、强制蹲爬、禁起立 | 符合 |
| 卵 | `WormEggLogic` 可受伤；死后关碰撞并孵虫；`spcWormEgg` 死后开观察剧情 | **可打**；`SetActiveAll` **关不到 `GroundCld`**；`MonsterRealRemove` 空实现，卵不销毁 |
| 绕开 | 通道低矮 + 卵贴地 → **设计不能绕** | 只能打碎 |
| 头顶 E | 打碎后可看破卵；另有裂缝观察 | 卡卵时常命中 **`ViewBridgeFracture`（Click）**：旁白「裂了/快走」，**不开路** |

#### 场景锚点（`ForestEastScene` / TreeBridge）

| 实例 | 约 x | 角色 |
|------|------|------|
| `WormEggType3` = `spcWormEgg` | ~276 | 特殊卵；死后开 `ViewBrokenEgg` |
| `WormEggType2` | ~292 | 中段挡路 |
| `WormEggType1` | ~317 | 靠出口挡路 |
| `ViewBridgeFractureStoryTrigger` | ~304 | **Click + E**，裂缝旁白 |
| `PassTreeBridgeStoryTrigger` | ~342 | Enter 过桥（卵之后） |
| `CanNotSomeActionArea` | ~301 | 禁 `SquatUp` |

#### 卡死机制（可叠加）

1. **爬行刹车（脚本）**  
   `PlayerBodyCollider.OnCollisionMonster`：当 `IsClimbMove`（或跑步/普攻）为真且碰到带 `ColliderResponder` 的活怪 Trigger 时 → `StopMove()` + `canInStateSetPos=false`。  
   树洞全程爬行 → 贴卵 Body 就会被持续刹住。

2. **卵 `GroundCld` 残留（物理）**  
   Prefab `WormEggType1/2/3`：`GroundCld` Layer=7、`isTrigger=0`、盒很大（Type3 本地约 6.6×5.0）。  
   `CldControllerComponent.nodes` 只有 Body/Body2/Body3/Foot（Trigger）——**不含 GroundCld**。  
   `OnDead` → `SetActiveAll(false)` **关不掉挡路实心盒**。

3. **E 误导（交互）**  
   E → Click 交互 → 裂缝剧情，不是打碎。真正开路是 **蹲停后普通攻击**（`SquatStay` → `SquatAtk`）；`ClimbMove` **未挂攻击**。

---

### 公共：与 0722 / Town

| 问题 | 判断 |
|------|------|
| 与 0722 `CapsuleGroundChecker` / 去掉 Default **同源回归？** | **否。** 0722 是半空 **误判 grounded=true**（旁侧墙）；本案是 **怪实心盒 / 爬行 StopMove / 死后碰撞未关**，且常伴随 **grounded=false** 卡下落态。 |
| `TownPlayerLocomotion`？ | **无关，勿改。** 树洞战斗爬行不走村纵深。 |

---

## ③ 用户需要做什么（检查清单）

进 **`ForestEastScene` 树洞**，Pause 选中 Player + 史莱姆/卵，按下面验：

### 复现 A

1. 跳到史莱姆顶上（或被击飞落到史莱姆上）。  
2. 看 Player：`Is Grounded`（多半 **false**）、`Velocity`、当前动画态是否停在 JumpFall / DamageFlyFall。  
3. Physics 2D Debugger：是否存在 **PlayerFoot ↔ 史莱姆 `GroundCld`** 接触。  
4. 史莱姆：`GroundCld` 是否 Active、非 Trigger、Layer=OnlyMapObj；Rigidbody Constraints 是否冻 X。

### 复现 B

1. 爬行贴红色卵，看是否无法前进；Console/状态是否 `IsClimbMove=true`。  
2. 按 E：是否只播裂缝对白、卵仍在。  
3. 停住蹲下用普攻打碎卵：`CldController` 关掉的节点 vs **`GroundCld` 是否还亮着**。  
4. 打碎后能否走到 `PassTreeBridge`（约更右侧）。

### 期望（产品口径，供施工对照）

- A：落到史莱姆上应滑落/挤开到真地面，可继续操作；不卡下落态。  
- B：按设计蹲击打碎卵后通道畅通；E 不暗示「按了就能开路」。

---

## ④ 给程序看的补充

### 可疑根因优先级

| 优先级 | 问题 | 锚点 | 证据 |
|--------|------|------|------|
| **P0** | A：`GroundCld` 实心托人 + 下落态死等 `IsGrounded` | `Slime.prefab` `GroundCld`；`JumpFallState` / `DamageFlyFallState`；`BaseMonster.OnInit` 强制 OnlyMapObj | 唯一非 Trigger 大盒；落地 Mask 不含 OnlyMapObj |
| **P0** | B：爬行 `OnCollisionMonster` → `StopMove` | `PlayerBodyCollider.cs` + `ClimbMoveState` `IsClimbMove` | 树洞必爬；卵 Body 为 Trigger+Responder |
| **P0** | B：死后关不掉 `GroundCld` | `WormEggLogic.OnDead` + `CldController` nodes 无 GroundCld | Prefab 三型一致 |
| **P1** | A：挤开逻辑禁用 | `PlayerBodyCollider.Start` 注释掉 Collision 订阅 | 「挤开掉落」缺失 |
| **P1** | A：Mass 9999 + Idle 冻 X | `Slime.prefab` / `SlimeIdleState` | 无法被水平顶开 |
| **P1** | B：E=裂缝 Click 误导 | `ViewBridgeFractureStoryTrigger` | 不开路 |
| **P2** | 矩阵 PlayerFoot↔OnlyMapObj 不对称 | `Physics2DSettings.asset` | 运行时确认接触后再改矩阵 |

### 建议修复方向（只建议，本阶段不施工）

**问题 A（按稳妥顺序）**

1. 运行时确认 Foot↔`GroundCld` 接触后：将 **PlayerFoot ↔ OnlyMapObj** 矩阵改为 **Ignore（双边）**，让 `GroundCld` 只服务「怪与地图/场景物」，不当玩家踏板。  
2. 或：缩小/压扁史莱姆 `GroundCld` 为贴地薄条，或改为 Trigger（需回归怪是否还站得住——怪落地主要靠 GroundChecker，RB GravityScale=0）。  
3. 勿把 OnlyMapObj 加进 `GroundLayerMask`（防回归另一类「站在怪上算落地」）。  
4. 可选：谨慎恢复挤出逻辑，或下落态对「脚下碰到怪 GroundCld」做侧向推开/强制切落地（避免死等）。  
5. **回归**：Forest 普通跳不飞出场景（0722）；史莱姆仍能在地面移动/受击。

**问题 B**

1. **必做**：`GroundCld` 纳入 `CldController`，或 `OnDead` 里显式 `groundCld.enabled=false` / `SetActive(false)`；Type1/2/3 一致。  
2. 爬行贴卵：`OnCollisionMonster` 对 WormEgg 放行，或允许 `ClimbMove` 中蹲击；HUD 提示「停住攻击打碎卵」。  
3. UX：卵前不要用 Click「E」暗示开路；裂缝 Trigger 与卵碰撞错开。  
4. **回归**：左进树洞 → 依次打 Type3/2/1 → 可选 `ViewBrokenEgg` → `PassTreeBridge` → 出洞。

### 相关文件清单

| 路径 | 角色 |
|------|------|
| `Assets/GameRes/Prefabs/Entity/Monster/Slime.prefab` | GroundCld / Mass / Body·Foot Trigger |
| `Assets/GameRes/Prefabs/Entity/Monster/WormEggType1/2/3.prefab` | 卵 GroundCld / CldController |
| `Assets/GameRes/Prefabs/Entity/Player/Player.prefab` | PlayerFoot、GroundLayerMask |
| `Assets/GameRes/Scenes/ForestEastScene.unity` | 树洞卵实例、剧情 Trigger |
| `Assets/Scripts/.../CapsuleGroundChecker.cs` | 只向下落地检测 |
| `Assets/Scripts/.../MoveComponent.cs` | IsGrounded / Velocity |
| `Assets/Scripts/.../PlayerMoveComponent.cs` | 跳/击飞初速 |
| `Assets/Scripts/.../BaseMonster.cs` | groundCld→OnlyMapObj |
| `Assets/Scripts/.../Slime.cs` / `SlimeIdleState.cs` | 死亡 Trigger、冻 X |
| `Assets/Scripts/.../JumpFallState.cs` / `DamageFlyFallState.cs` | 等 IsGrounded |
| `Assets/Scripts/.../PlayerBodyCollider.cs` | 挤开禁用；爬行 StopMove |
| `Assets/Scripts/.../WormEggLogic.cs` | 死亡关 Cld |
| `Assets/Scripts/.../CldControllerComponent.cs` | SetActiveAll |
| `Assets/Scripts/.../TreeBridgeLogic.cs` / `ForestEastTreeBridgeStoryMgr.cs` / `ForestEastTreeEnterTrigger.cs` | 树洞状态 |
| `Assets/Scripts/.../ClimbMoveState.cs` / `SquatStayState.cs` | 爬行 / 蹲击 |
| `ProjectSettings/Physics2DSettings.asset` / `TagManager.asset` | 层与矩阵 |
| `Assets/Doc/执行文档/0722/*` | 落地修复对照（不同源） |

### 开放问题

已记入 `Assets/Doc/OPEN_QUESTIONS.md`（本节对应条目）。施工前建议产品/程序拍板：

- 优先改 **矩阵 Ignore** 还是改 **GroundCld 形状/Trigger/死亡关闭**？  
- 爬行中是否允许直接攻击卵？  
- 卵前 E 是保留旁白还是挪开/改文案？

---

**文档路径**：`Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`
