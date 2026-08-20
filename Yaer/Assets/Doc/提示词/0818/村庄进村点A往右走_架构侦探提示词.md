# Cursor Agent Prompt · 进村后点一下 A，人往右走

> **角色**：先【架构侦探】只溯源、不改代码；报告拍板后再开【施工员】  
> **日期**：2026-08-18  
> **已确认现象（开发者）**：第一章进 `Village_KenMuNi1`，开场对话结束后，**点一下 A（左），人往右走**。  
> **范围**：村庄 2.5D 横向第一下方向；不扩龙宫、不改 Combat 战斗关卡手感、不推翻 0513/0514 已修的清 X / 同向早退补票。  
> **本阶段**：只读扫描 + 写溯源报告。禁止改 C# / Prefab / 场景。

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品口径

- **Bug**：进村后点一下 A，人往右走。  
- **期望**：点 A 往左，点 D 往右；第一下点按不能走反。  
- **验收主路径**：地图点肯姆尼 → `Village_KenMuNiStart` 播完能走 → **不要先 W/S**，只点一下 A → 必须往左。对照：只点一下 D → 往右；按住 A 不能先错一帧再纠正（观感上也要第一帧就对）。

### 生活类比

门房默认「客人从右边来」。你喊「往左走」，传令兵这帧还没上岗，门房已经按默认往右推了一把。下一帧传令兵才听见，但你已经松手了，人就只剩往右那一下。

### 预扫调用链（侦探必须用代码核对 Update 顺序）

村街 **`Village_KenMuNi1` 挂 `ForestScene.asset`，`isFightingScene=1`** → 进村加载 **Combat** 控制器，不是 Home。

| 步骤 | 谁 | 预扫行为 |
|------|----|----------|
| 出生 | `MoveComponent.OnInit` | **写死** `direction = Right` |
| 待机 | `CombatIdleState.Update` | 有横移或纵深意图 → `ChangeState<CombatRunState>()` |
| 入跑 | `CombatRunState.Enter` | 先 `onRightInput += MoveRight` / `onLeftInput += MoveLeft`；若**不是**纯纵深则 **`SetRunSpeed()`**（`runSpeed * DirectionSign`，默认 **+X 向右**） |
| 转向 | `MoveComponent.MoveLeft` → `TurnLeft` | 已朝左才改逻辑朝向；`isCheckDir` 同向早退 |
| 输入 | `PlayerInputComponent.ParseMoveCmd` | 队首 Left 才 `onLeftInput`。Idle 时 **Run 尚未订阅**，这一帧 Left **会丢** |

`Player.prefab`：`PlayerInputComponent.priority=0`，`PlayerCsAnimator.priority=1`。侦探必须查 `ComponentSystem` **谁先 OnUpdate**：若 Input 先于切态，点 A 的订阅窗口为空，随后 Enter 仍按朝右灌速度 → 与现象吻合。

点一下（非长按）时：Left 不在 `longPressCmd` 会从队列移除；下一帧没有 Left，只剩 Enter 灌进去的向右速度。

### 与旧文档的边界（不要修错病）

| 文档 | 现象 | 与本案 |
|------|------|--------|
| `0514` 先 WS 再同向 D 无位移 | 纯纵深入 Run **不** `SetRunSpeed`，同向 `TurnRight` 早退 | 本案是 **点 A 却往右**，更像 Enter **灌了默认朝右速度** 且 **TurnLeft 没赶上**。`EnsureVillageCombatRunHorizontalSpeedIfStale` 是补「没速度」，**可能加重「有速度但方向错」**，侦探须写明要不要动它 |
| `0513` 纵深时每帧 `StopMoveInX` | 横移被清掉 | 不是「走反」 |
| `HomeWalkState.Enter` 无条件 `SetWalkSpeed()` | 家里 W/S 会横向滑 | 村街是 Combat；**回归**时进村民家点 A 也要左，勿把家里一起改坏 |

### 严禁的施工方向（预判，侦探可推翻但须写理由）

1. 改 `MoveComponent.TurnLeft/TurnRight` 全局早退（战斗全场景风险，0514 已否 P2）。  
2. 把村街改成 Home 控制器来「躲开 Combat」。  
3. 进村改龙宫 / 改 `ForestScene` 战斗关 `isFightingScene`。  
4. 只把默认朝向改成 Left（点 D 又会反）。  
5. 用等待/延迟硬匹配第一帧。

### 侦探须比较的方案（报告只推荐一个）

| 方案 | 摘要 | 点 A 第一帧 | 对 0513/0514 | 战斗关 |
|------|------|-------------|--------------|--------|
| A Enter 后立刻按键位 `MoveLeft/MoveRight` | 村庄且 `GetKey(A)` 则 `MoveLeft` 再 `SetRunSpeed`；D 对称。点按用 `GetKeyDown` 或队列 | 高 | 须避开纯纵深 | 用 `Village2_5D` 门控 |
| B 禁止 Enter 用默认 `DirectionSign` 灌速 | 有横移意图时不 `SetRunSpeed`，改由已订阅的 MoveLeft/Right 给速；点按若本帧订阅已过，须在 Enter **同步补一次** 对应 Turn | 须补同步调用 | 接近现网 villageDepthOnly | 门控村庄 |
| C Input 在 Idle 就订阅左右 | 订阅上移，切态不再丢第一帧 | 好 | 订阅生命周期要防重复 | 面可能偏大 |
| D 默认朝向跟落点 | 进村 TurnLeft | **不解决点 A 丢帧** | 无关 | 否决为主方案 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/5月/0514/村庄先WS后AD无横向位移_CombatRun入态与转向早退_执行说明.md
@Assets/Doc/执行文档/5月/0513/村庄遇纵深障碍后横向移动迟滞_架构溯源与施工执行说明.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Combat/State/Ground/CombatIdleState.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Combat/State/Ground/CombatRunState.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/PlayerInputComponent.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Move/MoveComponent.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Home/HomeWalkState.cs
@Assets/GameRes/Prefabs/Entity/Player/Player.prefab
@Assets/GameRes/Config/SceneManagerConfig/ForestScene.asset

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、Animator。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 进肯姆尼村后，点一下 A，人往右走。这是 Bug。
2. 期望：A 左、D 右；第一下点按方向必须对。
3. 不要当成「锚点/动画反了」除非你能证明脸和速度长期对反（开发者复现是点 A 往右，不是倒着滑步）。
4. 目标：钉死 Update 顺序 + 为何第一帧用默认朝右灌速度；给出村庄专用最小修法。

---

## 必读线索

### A. 第一帧谁丢了 Left
- `ComponentSystem` 按 priority / 列表顺序：Input vs CsAnimator 谁先 `OnUpdate`
- Idle 时 `onLeftInput` 有没有订阅（预扫：只在 `CombatRunState.Enter` 订）
- 点一下 A：是否 `longPressCmd`、是否当帧出队
- `CombatRunState.Enter` 在 `HasVillageExploreHorizontalMoveIntent()==true` 时 `SetRunSpeed()` 是否在 `MoveLeft` 之前执行

### B. 朝向契约
- `MoveComponent.OnInit` 默认 Right；Prefab 序列化 `direction: 0`（Left）是否被 OnInit 覆盖
- `TurnLeft` 先改 `root.rotation` 再 `isCheckDir` 早退，会不会脸和速度脱节（本案次要）
- `DirectionSign` 与 `SetRunSpeed` 符号

### C. 不要误修
- 0514 补票 `EnsureVillageCombatRunHorizontalSpeedIfStale`：对「点 A 往右」是缓解还是加重
- `villageDepthOnly` 跳过 SetRunSpeed：必须保留给纯 W/S
- 村民家 HomeWalkState 无条件 `SetWalkSpeed`：是否同类；本期默认 **村街 Combat 先修**，家里若同现象写入 OPEN 勿擅自改

### D. 场景身份
- 确认 KenMuNi1 仍用 Forest Config Combat 轨
- 进村后 `LocomotionMode == Village2_5D`

---

## 侦探任务清单

1. **结论一句话**：点 A 往右的根因（丢订阅 + 默认朝右灌速 / 其它）。
2. **时序图**（同一帧）：Input → Idle.Update → Run.Enter → SetRunSpeed / MoveLeft 有无。
3. **推荐方案**（A/B/C 或组合，D 不作主方案）+ 否决理由。
4. **最小改动文件列表**；门控必须 `Village2_5D`（或明确等价），禁止改 Turn* 全局。
5. **验收清单**：
   - 进村，不先 WS，点一下 A → 往左；点一下 D → 往右
   - 按住 A 从第一帧起往左，无「先右后左」
   - 先 WS 再 D 仍能动（0514 不回归）
   - 顶纵墙再 AD 不粘死（0513 不回归）
   - Forest 战斗关左右跑正常
   - 龙宫左右走正常
6. OPEN 新节「进村点A往右走 · 2026-08-18」：村民家 Home 是否同 Bug、是否本期。
7. **禁止**：改资产；把默认朝向改成 Left 当修复。

---

## 输出要求

写入：`Assets/Doc/执行文档/0818/村庄进村点A往右走_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：传令兵晚到岗，门房按往右默认推了一把）  
③ 用户需要做什么（认方案 + 验收）  
④ 给程序看：时序、方案对比、最小文件、回归 0513/0514  

口头汇报用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0818/村庄进村点A往右走_架构溯源报告.md
@Assets/Doc/执行文档/5月/0514/村庄先WS后AD无横向位移_CombatRun入态与转向早退_执行说明.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按溯源报告做最小化修改，使进村后点一下 A 往左、点一下 D 往右，第一帧方向正确。

必须遵守：
- 仅村庄 2.5D（或报告写明的门控）；不改 MoveComponent.Turn* 全局早退；
- 保留纯 W/S 不叠横向速度；0513/0514 不回归；
- 不改龙宫、不把村街改成 Home 控制器来躲 Bug；
- 不在 Update 堆业务。

每次提交说明：改了哪些文件、第一帧如何按 A/D 给速、如何验收点按与长按。
```
