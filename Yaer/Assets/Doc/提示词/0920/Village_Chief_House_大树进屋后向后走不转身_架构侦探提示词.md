# Cursor Agent Prompt · 大树进村长家后：往后走有时不转身

> **角色**：【架构侦探】只读查清「向后移动有时没转身 / 没播转身动画」；报告通过后再施工  
> **日期**：2026-09-20  
> **路径**：巨树 2 楼 → 进 `Village_Chief_House`（回程门 `StairsDoor_BackToChief` / 落点 `EnterFrom_Tree2f`）之后，在村长家里移动  
> **现象（用户）**：玩家**向后移动**时，**有时候**不会转身，也**没触发转身动画**  
> **产品期望（钉死）**：和村里/其它室内一样，改变左右朝向时要转身并播转身动画；不要时灵时不灵  
> **不是**：改大树 2 楼走路；改进门对白；改 DayLight；改成战斗横版跳跃  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_Chief_House_大树进屋后向后走不转身_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「向后」是反方向横移还是纵深 W/S、以及转身动画由谁触发之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 从大树上的门进到村长家以后，人往回走的时候，有时脸不转、转身动画也不播。  
> 要查是进屋落点把朝向弄乱了，还是移动/障碍护栏把转身掐掉了，还是动画状态机根本没收到转身。

### 「向后」必须先钉死（报告①前先回答）

村长家是室内 2.5D（Y=纵深），「向后」可能是两种完全不同的事：

| 含义 | 按键 | 应不应该「转身」 |
|------|------|------------------|
| **反方向左右走** | 原本朝右走，改按左（或反过来） | **要**转身 + 转身动画 |
| **纵深往后** | W/S 在房间里走远近 | **通常不该**左右翻面；若用户期望翻面，写进 OPEN，不要擅自当 bug |

侦探须用复现步骤写死用户说的是哪一种。若两种都会，分开写。

### 进门路径（预扫）

```
KenMuNi1 巨树 2 楼 StairsDoor_BackToChief
  → Village_Chief_House
  → EnterPosKey = Village_KenMuNi1_Tree2f
  → 落点 EnterFrom_Tree2f（约 -2.8, 4，楼梯顶一带）
  → 室内 Village2_5D + VillageWalkArea
```

「有时候」→ 高度可疑和：**进屋瞬间朝向**、**贴障碍/楼梯时 Turn 护栏**、**纯纵深不进横移 Turn** 有关。

### 嫌疑

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A** | 进屋 Teleport/落点后朝向仍是树上的面，第一次「往后」其实是继续同向，没有触发 Turn | 记进屋前后 `localScale.x` / Move 面向；再按反方向键看 `onTurnAction` 有没有 |
| **B** | `MoveComponent` 转身条件苛刻（速度阈值、同帧又被清零），偶发不进 Turn | 读 Turn 触发条件；对比门口正常进屋路径 |
| **C** | `VillageWalkObstacleTurnImmediateBlock`：转身同帧清水平速度 / 脚底分离，动画像没转身 | 贴楼梯、障碍时复现；关护栏对比（只分析，本阶段不改） |
| **D** | Home 状态机在 Idle/Walk/Bink，转身动画挂在另一条链，纵深 Walk 不播翻面 | `HomeWalkState` / Animator 参数；村里街对比 |
| **E** | 只有「从大树回程进屋」坏，从村门正常进屋不坏 → 落点/朝向/楼梯顶障碍特有 | 两条进门路径对照表 |

### 禁止

- 本阶段不改代码、不改场景、不改 Animator。  
- 不要把「纵深 W/S 不翻面」未经确认就改成翻面。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/施工说明/0920/Village_KenMuNi1_树洞上面没法回村长家_施工说明.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageWalkObstacleTurnImmediateBlock.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Home/HomeWalkState.cs
@Assets/GameRes/Scenes/Village_Chief_House.unity
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab、Animator。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_Chief_House_大树进屋后向后走不转身_架构溯源报告.md

---

## 背景（策划白话）

从大树进去村长家之后，玩家向后移动的时候，有时候会没转身，也没触发转身动画。

要查清：
1. 「向后」是左右掉头，还是房间里纵深往后
2. 转身动画现网由谁触发（Move 翻面？Animator Trigger？状态切换？）
3. 为什么只是「有时候」，和大树回程进屋有什么关系
4. 最小改哪一处，才能稳定转身，且不破坏障碍护栏、村里走路

对照路径：
- 复现：巨树 2 楼门 → 村长家楼梯顶落点 → 向后走
- 对照：村里大门正常进村长家 → 同样向后走（若对照正常，缺口在回程落点/朝向/楼梯区）

---

## 必读 / 优先扫描

### A. 先钉「向后」与「转身动画」是什么

1. 读 Move / Turn / `onTurnAction` / `PlayerLogic.OnTurnAction`：什么条件算转身，播的是哪段动画或哪次 Scale/参数  
2. Home 控制器里有没有独立 Turn 状态，还是只靠朝向翻转 + Walk  
3. 报告里用一句话定义用户说的转身失败长什么样（脸朝错、动画没播、还是两者都有）

### B. 大树回程进屋链

读回程门与落点（施工说明 0920 树洞回村长家）：

- 进场后有没有 SetFollow / Teleport / 锁输入 / 改 Facing  
- `EnterFrom_Tree2f` 附近有没有 `VillageWalkObstacle`、窄 WalkArea，导致第一次反方向被护栏吃掉  
- 进屋后 LocomotionMode 是否仍是 Village2_5D

### C. 「有时候」复现矩阵

| 操作 | 期望 | 现网 |
|------|------|------|
| 进屋后立刻按反方向横移 | 转身+动画 | |
| 先同向走几步再反向 | 转身+动画 | |
| 贴楼梯/墙再反向 | 仍应转身（可被挡位移，但朝向应转） | |
| 只按 W/S 纵深 | 按产品：是否要翻面（须写清） | |
| 村门正常进屋后同样操作 | 对照是否正常 | |

### D. 推荐一种最小改法

只推一种，并满足：

1. 大树回程进屋后，左右反向移动稳定转身并有动画  
2. 不拆掉 `VillageWalkObstacleTurnImmediateBlock` 的穿模防护（若它是主因，只修误伤转身动画的那一环）  
3. 纵深 W/S 规则不擅自改成翻面，除非产品确认  
4. 村里街道与其它民居对照不被误伤

另外两种各用一句话否决。

---

## 报告结构（固定）

① 结论一句话（主因 + 「向后」是哪种）  
② 原因（大白话 + 从按键到转身动画的调用链，标偶发断点）  
③ 用户需要做什么（大树进门后怎么复现；对照村门进屋）  
④ 给施工员的补充：改哪些文件、不要改哪些、推荐方案、否决方案

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_Chief_House_大树进屋后向后走不转身_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageWalkObstacleTurnImmediateBlock.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告推荐方案做最小改。
不要改大树 WalkArea2，不要改进门对白，不要擅自让纵深 W/S 翻面（除非报告写明产品已确认）。

目标：
- 从大树进村长家后，左右反向移动要稳定转身并播转身动画
- 障碍护栏仍要防穿模；若报告说护栏误伤转身，只修误伤那一环
- 村里其它场景走路不被误伤

限制：
- 禁止在 Update 里堆新的转身状态机
- 复杂分支写清替代方案
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_Chief_House_大树进屋后向后走不转身_施工说明.md`

完成后用大白话给验收清单：大树回程进屋后立刻反向走、走几步再反向、贴楼梯反向；再对照村门进屋同样操作。
```
