# ForestEast · 跳跃落到藤蔓（TenWan）身上卡住 — 架构溯源报告

**文档版本**：v1.0（2026-07-23）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码**）  
**触发现象**：跳跃（或击飞）落点压在藤蔓怪顶端/细茎上 → 卡住、下不来、动不了  
**截图**：`Assets/Doc/提示词/0723/ForestEast_藤蔓站上卡住_截图.png`（人踩在细高绿藤尖上，左侧紫色史莱姆，右侧木牌）  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0723/ForestEast_藤蔓站上卡住_架构侦探提示词.md`
- 对照：`Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`
- `Assets/Doc/OPEN_QUESTIONS.md`（史莱姆 Q2/Q5 已决议）
- Prefab / 场景实例 / `TenWanLogic` / `TenWanSceneObjLogic` / `BaseMonster` / 玩家落地态 静态阅读

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**截图里是战斗怪 `TenWan`（不是场景障碍 `Tenwan`）；卡死与今日史莱姆同族——存活态实心 `GroundCld`（OnlyMapObj、整株细高盒）托住脚，落地 Mask 又不认它，`JumpFall`/`DamageFlyFall` 死等 `IsGrounded`，挤开又已关。史莱姆已修，藤蔓战斗怪还没跟。**

生活类比：细长钢柱顶着你的脚（`GroundCld`），落地灯不亮（不算地面），旁边「把人拨下去」的手被绑住（挤出订阅关了）——人就粘在尖上。

---

## ② 原因

### 2.1 实体分清（截图锚点）

| 类型 | 场景路径 / 实例 | 脚本 | 角色 | 是否截图对象 |
|------|-----------------|------|------|--------------|
| **战斗藤蔓** | `ForestEastScene` Prefab 实例：`TenWan`（~x=196.97）、`TenWan (1)`（~x=219.01）、`TenWan (2)`（~x=365.15） | `TenWanLogic` ← `BaseMonster` | 可觉醒/攻击的中立怪；Prefab=`Assets/GameRes/Prefabs/Entity/Monster/TenWan.prefab` | **是（高度确信）**：细茎+顶端球、旁有史莱姆+木牌，符合战斗场构图 |
| **场景藤蔓障碍** | 场景内嵌物体 `Tenwan`（~x=33.44,y=7.82），`tenWanName=forestEastTenwan` | `TenWanSceneObjLogic`（注释写明「场景中的藤蔓，不是怪物」） | **挡路障碍**：砍断前 `groundCld` 实心挡人，砍断后才 `isTrigger=true` | **否**；勿当本案修目标 |

> 运行时复现时请在 Hierarchy 点选脚下藤蔓，确认脚本是 `TenWanLogic` 还是 `TenWanSceneObjLogic`。

### 2.2 链路表（跳跃 / 击飞同源）

| 阶段 | 谁 | 做什么 | 结果 |
|------|-----|--------|------|
| 1 起跳/击飞 | `PlayerMoveComponent.SetJumpSpeed` / `SetDamageFlySpeed` | 写抛物线 `Velocity` | 空中 |
| 2 落地检测 | `CapsuleGroundChecker`：只向下；Mask=`GroundCenter\|GroundCommon`；`useTriggers=false` | **不认**藤蔓 Body/Foot/GroundCld 为地面 | |
| 3 藤蔓碰撞 | `Cld/Body1`、`Cld/Foot` → **Trigger**；`GroundCld` → **非 Trigger**、Layer **OnlyMapObj(7)**、盒约 **1.76×11.48**、offset.y≈5.47（**整株细高柱，尖顶也在盒内**） | Body/Foot 不挡刚体；**GroundCld 是唯一实心「踏板」** | |
| 4 质量/冻结 | `Rigidbody2D.Mass=9999`；`m_Constraints=7`（**FreezeAll**）；`GravityScale=0`；`baseMoveSpeed=0` | 人挤不动藤蔓 | |
| 5 状态机 | `JumpFallState` / `DamageFlyFallState`：`if (IsGrounded)` 才切落地态 | 被非地面实心盒托住 → **长期 `IsGrounded=false`，卡下落态** | |
| 6 挤开 | `PlayerBodyCollider` 里 `OnCollisionShowEventInSpcAction` **订阅仍注释**（OPEN_QUESTIONS Q5） | 「跳到怪身上挤开掉落」**未生效** | |

```
跳跃/击飞下落
  → CapsuleGroundChecker 不认藤蔓为地面
  → 唯一实心盒 GroundCld（OnlyMapObj、整株高柱）可能托住 PlayerFoot
  → JumpFall / DamageFlyFall 等不到 IsGrounded
  → 挤开已关 + Mass/FreezeAll → 无法挤开滑落
```

击飞路径与跳跃下落态共用「死等 `IsGrounded`」机制，**同源**；不必另开一条根因。

### 2.3 对照史莱姆报告：同族残留

| 项 | 史莱姆（修复前 / 已施工） | 战斗 `TenWan`（现状） |
|----|---------------------------|----------------------|
| `GroundCld` Prefab | 实心、OnlyMapObj、大盒 | **同样**：实心、`m_IsTrigger:0`、Layer 7；且为**细高整株盒**（尖顶易「站」住人） |
| 存活态 Trigger | **已修**：`Slime.OnInit` → `groundCld.isTrigger=true` | **未修**：`TenWanLogic.OnInit` **无**同款处理 |
| 死后关盒 | `BaseMonster.OnDead` → `groundCld.enabled=false`（基类已覆盖） | `TenWanLogic.OnDead` 调 `base.OnDead()` → **已覆盖**；卡死主因是**存活态** |
| Body/Foot | Trigger | Trigger（`Body1`/`Foot`） |
| 挤开 | 本期不恢复 | 同 |
| 矩阵 / GroundLayerMask | 不改 | **勿改**（延续 Q1/Q2） |

**归类：同族残留（P0）。** 差异仅在于藤蔓盒更细更高（视觉上像「站在尖上」），物理机制与史莱姆一致。

### 2.4 各态碰撞快照（战斗 TenWan）

| 态 | Body1 / Foot | GroundCld | 说明 |
|----|--------------|-----------|------|
| Sleep / Idle / Awake / Attack | Prefab 恒 Trigger | Prefab 恒 **实心**；`BaseMonster.OnInit` 强制 Layer=OnlyMapObj | 存活全程可当踏板 |
| Dead | `base.OnDead` 再设 Trigger；`CldController.SetActiveAll(false)`（nodes 仅 Body1） | **`groundCld.enabled=false`**（基类） | 死后不应再托人 |

场景障碍 `TenWanSceneObj`：**故意**用实心 `groundCld`/`collArea` 挡路，砍断帧事件才 `isTrigger=true`——与战斗怪需求相反，**禁止套用「存活即 Trigger」**。

### 2.5 与 0722 / Town

| 问题 | 判断 |
|------|------|
| 与 0722 落地 Mask 修复同源回归？ | **否。** 0722 是半空误判 `grounded=true`；本案是怪实心盒 + `grounded=false` 卡下落态。 |
| `TownPlayerLocomotion`？ | **无关，勿改。** |

---

## ③ 用户需要做什么（检查清单）

进 **`ForestEastScene`**，找战斗藤蔓（约 x≈197 / 219 / 365，或截图同构图：旁有史莱姆+木牌）：

1. **复现**：跳到藤蔓尖上（可选再测：被击飞落到藤蔓上）。  
2. Hierarchy 选中脚下物体：脚本应是 **`TenWanLogic`**（不是 `TenWanSceneObjLogic`）。  
3. Pause 看 Player：`Is Grounded`（多半 **false**）、`Velocity`、动画是否停在 **JumpFall / DamageFlyFall**。  
4. Physics 2D Debugger：是否存在 **PlayerFoot ↔ TenWan `GroundCld`** 接触。  
5. Inspector：`GroundCld` → Active、**Is Trigger=false**、Layer=**OnlyMapObj**；根节点 RB Mass≈9999、Constraints=FreezeAll。  
6. （对照）场景障碍 `Tenwan`（靠前段）：砍断前挡路是设计；**不要**为修战斗怪把它改成存活 Trigger。

**期望（产品口径）**：跳到 / 击飞落到**战斗**藤蔓上应滑落或被挤到真地面，可继续移动；不应把藤蔓当踏板僵住。

---

## ④ 给程序看的补充

### 可疑根因优先级

| 优先级 | 项 | 锚点 | 证据 |
|--------|-----|------|------|
| **P0** | 存活 `GroundCld` 实心托人 + 下落态死等 `IsGrounded` | `TenWan.prefab` `GroundCld`；`JumpFallState` / `DamageFlyFallState`；`BaseMonster.OnInit`→OnlyMapObj | 与史莱姆修复前同构；`TenWanLogic` 未做 Trigger |
| **P1** | 挤开逻辑禁用 | `PlayerBodyCollider.Start` 注释掉 Collision 订阅 | Q5 本期不恢复 |
| **P1** | Mass 9999 + FreezeAll | `TenWan.prefab` RB | 无法被水平顶开 |
| **P2** | 矩阵 PlayerFoot↔OnlyMapObj 不对称 | `Physics2DSettings.asset` | Q1/Q2 已决议不改矩阵 |
| — | 场景 `TenWanSceneObj` 实心挡板 | `TenWanSceneObjLogic` 砍断前挡、后 Trigger | **设计如此；勿当 Bug 乱改** |

### 建议修复方向（只建议，本阶段不施工）

1. **优先（对齐史莱姆决议）**：在 `TenWanLogic.OnInit`（`base.OnInit` 之后）对战斗怪执行 `groundCld.isTrigger = true`，注释说明同史莱姆（怪落地靠 GroundChecker + GravityScale=0；不改矩阵、不改 GroundLayerMask、不恢复挤出）。  
2. **死后**：已有 `BaseMonster.OnDead` 关 `groundCld`，一般无需再动；回归确认死后不残留托人。  
3. **禁止**：改 Physics2D 矩阵；把 OnlyMapObj 加进 `GroundLayerMask`；改 `TownPlayerLocomotion`；对 **`TenWanSceneObjLogic`** 在存活态套同款 Trigger（会拆掉「砍藤蔓开路」挡板）。  
4. **可选后续**：若多怪仍漏修，再考虑基类策略或按怪类型配置——本案最小化只动 `TenWanLogic` 即可。  
5. **回归**：  
   - 跳/击飞落到战斗藤蔓 → 滑落可动；  
   - Forest 普通跳不飞出场景（0722）；  
   - 史莱姆 / 虫卵已修路径不回滚；  
   - 场景 `Tenwan` 砍断前仍挡路、砍断后可过。

### 与「史莱姆 GroundCld→Trigger」施工的关系

| 关系 | 说明 |
|------|------|
| 同决议 | 怪侧 Trigger + 死后关盒；不改矩阵；不恢复挤出（Q2/Q5） |
| 未覆盖原因 | 史莱姆只在 `Slime.OnInit` 写了 Trigger；**未抽到 `BaseMonster`**，故 `TenWan` 等仍实心 |
| 施工面 | 复制史莱姆写法到 `TenWanLogic`（或等价 Prefab 默认 Trigger + 运行时保证）；**不要**误改场景障碍 |

### 相关文件清单

| 路径 | 角色 |
|------|------|
| `Assets/GameRes/Prefabs/Entity/Monster/TenWan.prefab` | 战斗藤蔓 GroundCld / Body1·Foot Trigger / Mass·FreezeAll |
| `Assets/GameRes/Scenes/ForestEastScene.unity` | 实例 `TenWan` / `TenWan (1)` / `TenWan (2)`；场景障碍 `Tenwan` |
| `Assets/Scripts/.../TenWan/TenWanLogic.cs` | 战斗怪逻辑；**缺** GroundCld→Trigger |
| `Assets/Scripts/.../TenWanSceneObj/TenWanSceneObjLogic.cs` | 场景障碍；存活挡、死后 Trigger——**勿误改** |
| `Assets/Scripts/.../BaseMonster.cs` | `groundCld`→OnlyMapObj；`OnDead` 关盒 |
| `Assets/Scripts/.../Slime/Slime.cs` | 同款修复参考（`groundCld.isTrigger=true`） |
| `Assets/Scripts/.../CapsuleGroundChecker.cs` / `MoveComponent.cs` | 落地检测 |
| `Assets/Scripts/.../JumpFallState.cs` / `DamageFlyFallState.cs` | 死等 IsGrounded |
| `Assets/Scripts/.../PlayerBodyCollider.cs` | 挤出订阅已关 |
| `Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md` | 同族先例 |
| `Assets/Doc/执行文档/0722/*` | 落地修复对照（不同源） |
| `Assets/Doc/OPEN_QUESTIONS.md` | 史莱姆决议 + 本案补充条目 |

### 开放问题

已记入 `Assets/Doc/OPEN_QUESTIONS.md`（本节「藤蔓站上卡住」）。施工默认建议：

- 战斗 `TenWan`：**直接套用**史莱姆 Q2（GroundCld→Trigger），无需新设计拍板。  
- 场景 `TenWanSceneObj`：**保持**砍断前实心挡板；若产品希望「未砍断也不能站上去」，另开需求，勿与本案混修。

---

**文档路径**：`Assets/Doc/执行文档/0723/ForestEast_藤蔓站上卡住_架构溯源报告.md`
