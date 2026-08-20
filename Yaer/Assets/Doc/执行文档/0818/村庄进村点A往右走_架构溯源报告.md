# 进村后点一下 A 人往右走 — 架构溯源报告

**文档版本**：v1.0（2026-08-18）  
**文档性质**：【架构侦探】只读溯源；**本文件不施工**  
**Unity**：2020.3.48f1 / C#  
**已确认现象**：第一章进 `Village_KenMuNi1`，开场对话结束后，**点一下 A（左），人往右走**  
**范围**：村庄 2.5D 横向第一下方向。不扩龙宫、不改 Forest 战斗关手感、不推翻 0513 清 X / 0514 同向早退补票

关联：

- 提示词：`Assets/Doc/提示词/0818/村庄进村点A往右走_架构侦探提示词.md`
- 规范：`Assets/Doc/02_SYSTEM_SPEC.md` §4 Home/Combat 双轨
- 0514：`Assets/Doc/执行文档/5月/0514/村庄先WS后AD无横向位移_CombatRun入态与转向早退_执行说明.md`
- 0513：`Assets/Doc/执行文档/5月/0513/村庄遇纵深障碍后横向移动迟滞_架构溯源与施工执行说明.md`

---

## ① 结论一句话

**根因是两件事叠在一起：Idle 时左右键还没人听；进跑时却按「默认朝右」灌了跑步速度。点一下 A 时，Left 还在队列里（被当成「有横向意图」），于是灌了向右的速；紧接着松键把 Left 扔掉，`MoveLeft` 再也没机会执行，人就只剩往右那一下。** 不是锚点反了、也不是脸和速度长期对反。

---

## ② 原因（生活类比）

门房默认「客人从右边来」。你喊「往左走」时，传令兵（`onLeftInput`）还在跑态门口办入职，这声令 **Idle 那一帧没人接**。等传令兵上岗，门房已经按默认往右推了一把。你只点了一下，令牌刚进篮子又被松键收走，传令兵上岗后篮子空了，**没有第二声「往左」**，人就只被往右推了那一下。

长按 A 时，传令兵上岗后下一声还能听见，观感上可能「先错一截再纠正」或很快被扳回来——验收要以点按为准。

---

## ③ 用户需要做什么

认 **方案 B′（推荐）**：只在村庄 Combat 进跑时，**先按当前 A/D（或队列里的 Left/Right）同步转一次向，再给速度**；不要用默认朝右灌速。纯 W/S 仍然不给横向速度（保住 0513/0514）。

施工后按此验收：

| # | 操作 | 期望 |
|---|------|------|
| 1 | 地图点肯姆尼 → 开场对话结束 → **不要先 W/S**，点一下 A | **往左**，不能往右滑一下 |
| 2 | 同样只点一下 D | **往右** |
| 3 | 按住 A | 从第一下物理位移起就是往左，无「先右后左」 |
| 4 | 先 W/S 再按与朝向同向的 D | 仍能横移（0514 不回归） |
| 5 | 顶纵墙再 A/D | 不粘死（0513 不回归） |
| 6 | Forest 战斗关左右跑 | 与现网一致 |
| 7 | 龙宫左右走 | 与现网一致（Home，本期不改） |

村民家点 A 是否同样反，**本期不修**，只记 OPEN。

---

## ④ 给程序看的补充

### 4.1 场景身份（核实）

| 项 | 现网 |
|----|------|
| `Village_KenMuNi1` Config | 复用 `ForestScene.asset`，**`isFightingScene=1`** |
| 动画轨 | **Combat**（`Run`），不是 Home `Walk` |
| `LocomotionMode` | `PlayerLogic.RefreshVillageExplorationFromActiveScene`：场景名是 KenMuNi1 则 **`Village2_5D`** |
| 对话结束 | `StoryEndHandle` → `ResumeGameHandle` → `SetAllowMove(true)`；对话中 `GetKeyDown` 被 `cantMove` 挡掉，对话后第一下 A 是正常 KeyDown |

禁止为躲这个 Bug 把村街改成 Home，也禁止改 `ForestScene` 的 `isFightingScene`（会连累森林战斗关）。

### 4.2 朝向契约

```csharp
// MoveComponent.OnInit：写死朝右，会盖掉 Prefab 序列化
direction = EDirectionType.Right;
```

`Player.prefab` 上 `direction: 0` 实际是枚举 **Left**，但进游戏第一帧就被改成 **Right**。  
`SetRunSpeed()` = `runSpeed * DirectionSign`，默认 **+X 向右**。

**不要把默认朝向改成 Left 当修复**（点 D 又会反）。脸朝右灌右速、点 A 却没翻面，看起来就是「往右走」，不是倒滑步。

`TurnLeft`：已朝左则 `isCheckDir` 早退。本案是 **没来得及 TurnLeft**，不是早退把脸和速度撕开。禁止改 `Turn*` 全局（0514 已否 P2）。

### 4.3 Update 顺序（预扫有一处必须纠正）

预扫以为：`priority` Input=0、Animator=1，SortedList 从小到大 → Input 先。

**核实：**

- `ComponentSystem` 确为 `SortedList<int, List>`，键升序更新。
- `SyncComponentsToSystem` 调用的是 `AddComponent(component)`，**没有传入 `Priority`**，运行时 **全部进 0 号桶**。
- Prefab `componentsList` 顺序：**`PlayerCsAnimator` 第一**（yaml 上标 priority 1 但不参与这趟 Add），**`PlayerInputComponent` 第五**（priority 0）。
- 因此同一 `PlayerLogic.OnUpdate` → `componentSystem.OnUpdate()`：**先 Animator（Idle.Update / 可能的 Run.Enter），后 Input（入队 + ParseMoveCmd）**。

`priority` 字段目前只给编辑器 `SortComponent` 用，**不是**本 Bug 的「Input 先于切态」证据。真正的丢令发生在：

1. **Idle 时 `onLeftInput` 仍为 null**（只在 `CombatRunState.Enter` 才 `+= MoveLeft`）；  
2. **`ChangeState` 只 `SetAnimatorEnter`，要等 `IsName("Run")` 才 `Enter()`**（过渡 `duration=0`，仍通常晚于 KeyDown 当帧）；  
3. KeyDown 当帧 ParseMoveCmd 打在空订阅上。

Idle 切跑还用了 `HasVillageExploreHorizontalMoveIntent()`，里面 **直接 `GetKey(A)` / Raw Horizontal**，不依赖队列。所以 **Animator 先跑时，Idle 已经能在 Input 入队之前申请切 Run**。

### 4.4 同一帧时序（点一下 A）

`Left`/`Right` 在 `longPressCmd` 里：KeyDown 入队且留着，**KeyUp 才 Remove**。Parse 只对非长按指令当帧出队。

```
【第 0 帧 KeyDown A】
  Animator  Idle.Update
            GetKey(A)==true → ChangeState<CombatRunState>()
            本帧 Animator 多半仍 IsName("Idle") → Run.Enter 还没走
            订阅仍空
  Input     GetKeyDown(A) → 队列插入 Left
            ParseMoveCmd(Left) → onLeftInput?.Invoke  ← 空，令丢了
            Left 留在队列（长按）

【之后某一帧：Animator 到达 Run，且你已经松开 A = KeyUp 当帧】
  Animator  IsName("Run") → CombatRunState.Enter
            += MoveLeft / MoveRight
            villageDepthOnly = Village2_5D && !HasVillageExploreHorizontalMoveIntent()
            此时队列里 **往往还在**（Input 还没跑到 KeyUp）→ 意图=true
            → SetRunSpeed()  ← Direction 仍是 Right → velocity.x = +runSpeed
  Input     GetKeyUp(A) → 从队列 Remove Left
            ParseMoveCmd：队空 → **不会 MoveLeft**
  FixedUpdate  把 +X 写进刚体  → 人往右走一下，然后 Run.Update 发现没横向意图回 Idle、StopMove
```

这就是「点一下」只剩往右一下。  
按住 A：Enter 之后同帧 Input 仍会 `ParseMoveCmd(Left)` → `MoveLeft` → `TurnLeft` 把速度乘 -1，长按多半能扳回来；若验收仍看到「先右后左」，就是这一下 `SetRunSpeed(+X)` 已经进物理或动画朝右闪了一下。施工目标是 **Enter 不要按默认朝右灌速**。

`CombatRunState.MoveLeft`：仅当当前朝右才 `SetRunSpeed`，再 `EnsureVillageCombatRunHorizontalSpeedIfStale`，再 `MoveLeft`。点按路径 **根本没走进 MoveLeft**。

### 4.5 0514 补票对本案的影响

`EnsureVillageCombatRunHorizontalSpeedIfStale`：仅村庄、且 `|moveSpeedX| ≤ 0.12` 才再 `SetRunSpeed()`。

| 问 | 答 |
|----|----|
| 会不会单独造成「点 A 往右」？ | **不会**。点按没调用 `MoveLeft`，补票函数不会跑。 |
| 会不会加重？ | **不会把右速再加大**。若错误右速已经 > 死区，它直接 return。 |
| 要不要为了本案删掉？ | **不要删**。那是 0514「先 WS 再同向 D 无位移」的补票；删了会回归。 |
| 会不会修好本案？ | **不会**。它补的是「没速度」，不看 A/D，更不会把 +X 改成 -X。 |

`villageDepthOnly` 跳过 `SetRunSpeed` **必须保留给纯 W/S**，否则纵深叠横向，0513 类问题回来。

### 4.6 Home 对照（本期不改）

`HomeWalkState.Enter`：先订阅，再 **无条件** `SetWalkSpeed()`（同样用 `DirectionSign`）。村街是 Combat，家里是这套。若村民家点 A 也反，机制同类。**默认村街先修，家里写入 OPEN。**

### 4.7 方案对比

| 方案 | 摘要 | 点 A 第一下 | 0513/0514 | 战斗关 | 结论 |
|------|------|-------------|-----------|--------|------|
| **B′（推荐）** | 村庄且有横向意图时：**禁止**用默认 `DirectionSign` 灌速；订阅后按队列/A/D **同步** `MoveLeft`/`MoveRight`（内部已有翻面 SetRunSpeed + 0514 零速补票）。纯纵深仍 skip | 高：Enter 当帧方向已对 | 保留 villageDepthOnly；不删补票 | `Village2_5D` 门控，Forest 仍走现网 `SetRunSpeed()` | **推荐** |
| A | Enter 后 `GetKey(A)` 则 `MoveLeft` 再 `SetRunSpeed`；D 对称 | 高 | 须避开纯纵深 | 门控村庄 | 与 B′ 同类；注意用队列+GetKey，**不要只靠 GetKeyDown**（当帧可能已过） |
| C | Idle 就订阅左右 | 好 | 订阅生命周期易重复/泄漏 | 面偏大 | 否决作主方案 |
| D | 进村默认 TurnLeft | **不解决丢令**；点 D 可能反 | 无关 | 否决 | **不作主方案** |

B′ 相对 A：不先灌右再纠正，而是 **先转向再写速**（或只走已有 `MoveLeft`/`MoveRight`）。`MoveLeft` 在朝右时会 `SetRunSpeed` 再 `TurnLeft`（`moveSpeedX *= -1`）；若速度仍为 0，0514 补票会在 `MoveLeft` 里补上。

伪代码（仅示意，施工时加注释）：

```csharp
// CombatRunState.Enter，订阅之后：
bool village = inputComponent.LocomotionMode == PlayerLocomotionMode.Village2_5D;
bool horiz = inputComponent.HasVillageExploreHorizontalMoveIntent();
bool villageDepthOnly = village && !horiz;
if (villageDepthOnly) { /* 不 SetRunSpeed，与现网一致 */ }
else if (village) {
    // 队列队首或整队 Left / GetKey A → MoveLeft(true)
    // Right / D → MoveRight(true)
    // 禁止在未转向前 moveComponent.SetRunSpeed();
} else {
    moveComponent.SetRunSpeed(); // 非村庄 Combat 保持现网
}
```

左右同时按下：跟队列队首（现网 Parse 也只看队首），不要自己发明优先级。

### 4.8 最小改动文件

| 文件 | 动作 |
|------|------|
| `CombatRunState.cs` | 仅 `Enter` 的村庄横向分支；保留 `villageDepthOnly`、`EnsureVillageCombatRunHorizontalSpeedIfStale`、`Update` 里 Raw Vertical 清 X |

**不改**：`MoveComponent.TurnLeft/TurnRight`、`PlayerInputComponent` 订阅上移、`ForestScene.asset`、龙宫、HomeWalkState、Prefab 默认朝向、Animator。

### 4.9 严禁（核实后维持）

1. 改 `Turn*` 全局早退。  
2. 村街改 Home 控制器躲 Bug。  
3. 改 KenMuNi / Forest 的 `isFightingScene`。  
4. 默认朝向改 Left。  
5. 用等待/延迟硬配第一帧。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-18 | v1.0 侦探：丢订阅 + 默认朝右灌速；点按 KeyUp 帧队列仍在导致灌速后无 MoveLeft；推荐 B′；0514 补票保留 |
