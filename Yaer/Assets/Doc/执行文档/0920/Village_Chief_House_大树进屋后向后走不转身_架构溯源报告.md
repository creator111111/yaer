# Village_Chief_House 大树进屋后向后走不转身 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab / Animator）  
> Unity：2020.3.48f1  
> 复现路径：巨树 2 楼 `StairsDoor_BackToChief` → `EnterFrom_Tree2f` → 村长家「向后」  
> 对照：村门 `House_Chief` → `EnterFrom_Village` → 同样向后

---

## ① 结论一句话

**「向后」若是左右掉头，偶发不转身是 Home 从待机进走路时，第一下 A/D 没赶上订阅，Turn 没被叫到；大树回程只是更容易一落地就立刻反向按。** 若按的是 W/S，本来就不会翻面。最小改：在 `HomeWalkState.Enter` 里按当前横键补一次 `MoveLeft`/`MoveRight`（对齐 Combat 进跑），不要拆转身护栏，也不要让纵深翻面。

---

## ② 原因

### 「向后」是什么

| 按键 | 算不算转身 | 现网行为 |
|------|------------|----------|
| **A / D（左右）** | **是** | `MoveLeft`/`MoveRight` → `TurnLeft`/`TurnRight`：根节点 Y 转 0↔180 |
| **W / S（纵深）** | **否** | 只改权威 Y；**故意不翻面**（2.5D） |

产品要稳的是左右反向；纵深不要改成翻面。

### 转身「动画」由谁触发

村长家 `isFightingScene=0`，走 **Home** 状态机，**没有**独立 Turn 状态/Clip。

```
HomeIdle / HomeBink →（有移动意图）→ HomeWalkState
  Enter：订阅 onLeftInput/onRightInput，SetWalkSpeed()
  按住 A/D → MoveLeft/MoveRight → TurnLeft/TurnRight
    → onTurnAction：
         PlayerEffectComponent.CreateTurnDust（扬尘）
         PlayerLogic.OnTurnAction（只拧按键提示，不播片）
         TownPlayerLocomotion 转身护栏（只清 vx，不挡翻面）
```

`TurnLeft`/`TurnRight` 若**已经是该朝向**会早退，**不**调 `onTurnAction`（无扬尘、无可观察「转身」）。

新玩家进场 `MoveComponent.OnInit`：**默认朝右**。传送不改朝向。大树回程与大门进屋落点后都是朝右起步。

### 和大树回程的关系

| | 大树回程 | 村门进屋 |
|--|----------|----------|
| 键 | `Village_KenMuNi1_Tree2f` | `Village_KenMuNi1` |
| 落点 | `EnterFrom_Tree2f` **(-2.8, 4)** 楼梯顶 | `EnterFrom_Village` **(17.1, -6.61)** |
| 楼梯门 | 左侧约 **(-4.51, 4.8)** | — |
| 朝向 | 默认右（面向屋内） | 同左 |

楼梯在落点**左侧**。人一落地就想「往后／往回楼梯」→ 按 **A**。此时常还在 Idle，切 Walk 的那一帧和 Combat 进跑一样：**订阅还没挂上，KeyDown 已解析完**，`MoveLeft` 没被叫到 → 人用朝右的脸 + `SetWalkSpeed` 往右蹭，或站着，**没有 Turn、没有扬尘**。再按住 A 几帧后订阅已在，才会翻面——体感「有时候不转」。

大门落在右侧深处，人常先同向走几步（已在 Walk），再反向时订阅已在，Turn 稳定，对照「正常」。

`VillageWalkObstacles` 根节点现网 **Inactive**，不是本案主因。转身护栏只在 **已经 Turn 成功** 后清水平速，不会取消翻面。

### 调用链与偶发断点

```
进场 Teleport → direction=Right（OnInit）
  → 多半停在 HomeIdle
玩家立刻按 A（往楼梯「向后」）
  → Idle.Update：HasMoveInput → ChangeState(HomeWalk)
  → Walk.Enter：才订阅 onLeftInput + SetWalkSpeed
  ★ 断点：本帧 Left 回调已空跑 → 无 TurnLeft / 无 onTurnAction
下一帧仍按住 A → MoveLeft → TurnLeft → 扬尘（此时才「转」）
```

若先进 W/S 进 Walk，再按 A：订阅已在，反向应立刻 Turn（矩阵第 4 行）。

---

## ③ 用户怎么复现

| # | 操作 | 预期（修前） |
|---|------|----------------|
| 1 | 2 楼门进村长家，**落地立刻点一下 A**（往楼梯） | 偶发：脸仍朝右、无扬尘 |
| 2 | 落地先按住 D 走几步，再按 A | 应翻面 + 扬尘 |
| 3 | 贴楼梯再反向 D | 应翻面（护栏最多停步，不挡脸） |
| 4 | 只按 W/S | **不翻面**（正常） |
| 5 | 村门进屋，同样立刻点 A | 偶发同构，但较少被当成「大树专属」 |

可选：临时看 `direction` / 根 `eulerAngles.y`；有扬尘 = `onTurnAction` 已进。

---

## ④ 给施工员

### 推荐方案（只此一种）

在 `HomeWalkState.Enter` 里，订阅完 `onLeftInput`/`onRightInput` 且 `SetWalkSpeed()` 之后：

若 `LocomotionMode == Village2_5D`，按当前横键/轴补一次 `MoveLeft(true)` 或 `MoveRight(true)`。  
逻辑对齐 `CombatRunState.ApplyVillageCombatRunEnterHorizontalFromInput`（队列 → GetKey A/D → Raw Horizontal）。可抽小共用方法，避免两处漂移。

效果：Idle→Walk 首帧反向也会进 `Turn*` → `onTurnAction` → 扬尘，左右反向稳定。

### 不要改

| 不要改 | 原因 |
|--------|------|
| W/S 触发翻面 | 产品禁止纵深当掉头 |
| 关掉 `VillageWalkObstacleTurnImmediateBlock` | 护栏防穿模；不挡翻面 |
| 只给 `EnterFrom_Tree2f` 写死朝向 | 治标不治 Idle→Walk 丢键；大门同构仍在 |
| Animator / 新 Turn 状态 | Home 现网就靠旋转 + 扬尘 |
| 回程门 / EnterPos 坐标 | 落点对，问题在进 Walk |

### 否决

- 把「向后」理解成纵深并加翻面：破坏 2.5D。  
- 拆转身护栏「好走一点」：穿模回潮，且不修丢 Turn。
