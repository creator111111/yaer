# ForestEast 树洞 · 打碎第一虫卵后卡住无法前进 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 场景 / Prefab / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` 树洞内（截图：碎红卵前、头顶 **E**、地上绿盒、右前方幼虫）  
**现象**：打碎**第一颗**挡路卵后卡在原地，无法继续往洞内爬/蹲走  
**产品期望**：碎后应能越过碎壳前进；E 旁白可点可不点，**不点也不锁位移**；后续卵/木虫链可用  
**不是**：删 E 交差；改死羊/吸羊；改 `CameraTreeInArea`；再拖三块地板碰运气；重写树桥  
**对照**：0723 卵 `GroundCld` 死后须关；0914 洞口自动爬锁 / 洞内人 Y−1（**已施工**）  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_树洞第一虫卵打碎后卡住_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q5  
**本会话**：未能 Play 卡死帧打相交盒；以 YAML/C# 钉嫌疑，施工前须用 Debug 分叉

---

## 沟通摘要

### ① 结论一句话

第一颗卵是 **`WormEggType3` / `spcWormEgg`（约 276.26, -6.36）**。E 是死后打开的 **`ViewBrokenEggStoryTrigger`（Click）**，不是开路键，**单凭 E 不能定罪**。代码上 `BaseMonster.OnDead` **已** `groundCld.enabled=false`（0723 残留修复），但碎壳仍留场；**碎卵后立刻孵出的 `woodWormObj`（本地约 −3.19）会贴在人身边，`PlayerBodyCollider` 对木虫仍每帧 `StopMove`（只跳过卵）**——这是「打碎之后」才出现的强嫌疑。须与 **自动爬锁未清**、**人 Y−1 后顶板夹死** 用卡死帧三分：物理挡 / 脚本刹 / 输入锁。

### ② 原因（通俗）

树洞规矩是爬着走、蹲着砸开卵才能过。砸破第一颗后，碎壳图还在，但挡路板按设计应关掉；同时卵里会蹦出一条小虫，爬的时候蹭到虫会被脚本一直刹车。头顶 E 只是「看看碎卵」的旁白开关，不按不该钉死。洞口那套自动爬锁、以及刚做的洞内人降 1，可能叠在一起，让人更像「整个人坏了」而不只是「被墙挡住」。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网 / 待验 |
|---|------|-------------|
| 1 | 新档左口进洞，**不按 E**，蹲击 Type3，打完按方向 | 期望越过碎壳；现网用户报卡 |
| 2 | 卡死帧 Hierarchy | `WormEggType3/GroundCld` **enabled?**；`ViewBrokenEgg` Active；孵出木虫 Active/位置 |
| 3 | Console | `[CanNotSomeActionArea] Start` 有无成对 `Stop`；有无对白壳 |
| 4 | 卡死帧打 | `DisablePlayerMove` / `AutoMoveState` / `IsClimbMove` / `canInStateSetPos` / `HasRunningStory` / player.y |
| 5 | 绿盒是谁 | Trigger 观察盒 / 卵 GroundCld / 爬区 `CanNotSomeActionArea` 三分 |

### ④ 程序补充

见下文。

---

## 1. 第一卵身份与空间

| 物体 | 现网 | 说明 |
|------|------|------|
| **第一卵** | `WormEggType3` PrefabInstance，根 stripped `1296467376`；世界 **(276.26, -6.359)** | `TreeBridgeLogic.spcWormEgg` |
| `GroundCld` | Layer **7 OnlyMapObj**；`isTrigger=0`；Size≈**(6.61×4.99)**；Scale **0.6**；Offset≈(-0.22, 1.92) | 活卵挡路真源；`groundCld` 序列化指向该盒 |
| `CldController.nodes` | Body / Body2 / Body3 / Foot（均为 Trigger 路径组件） | **不含 GroundCld** → 仅靠 `SetActiveAll` **关不掉**挡路盒 |
| 孵虫 | `woodWormObj` 子 Prefab，本地约 **(-3.19, 0)** → 世界 x≈**273** | 在卵左侧，贴近从左口来的玩家 |
| 观察 E | `ViewBrokenEggStoryTrigger` **(276.1, 0)**；盒 Size **(3.45×4.5)** Offset.y=**-5.8**；默认 **Inactive** | `triggerType=Click(0)`；`ForestEastSceneViewBrokenEgg`；`SingleUseInArchive=0` |
| 右前方幼虫 | 须分：① 卵孵出的 `woodWormObj`；② `storyWoodWormLogicList` 两条场景木虫（对白 `start` 才相关，非碎卵瞬间） | 截图平台虫优先核孵出虫是否已 Active |
| 后续卵 | Type2≈292、Type1≈317 | **不是**本票「第一颗」 |
| 爬区 | `CanNotSomeActionArea` 约 x **259～339**，SquatUp | 洞内几乎全程在区内 |

设计：强制蹲爬 → **蹲击碎卵开路** → 可看一眼旁白 → 继续右行。`MonsterRealRemove` **故意不销毁**碎壳；开路靠关碰撞。

---

## 2. 打碎后调用链

```
蹲击致死
  WormEggLogic.OnDead
    base.OnDead
      body/foot → Trigger
      groundCld.enabled = false     ★ 0723 已写进基类（注释即树洞卵）
      OnDeadEventFunc …
    ChangeState<WormEggBreakState>
    CldController.SetActiveAll(false)  // 关不到 GroundCld
    成就计数

TreeBridgeLogic.Update（每帧）
  spcWormEgg.IsDead && !eggStoryTrigger.activeSelf
    → eggStoryTrigger.SetActive(true)   ★ 头顶 E 出现（Click，不自动播）

Break 动画结束 → RemoveMonsterOnDead 帧事件
  MonsterDeadEndEvent
    woodWormObj.SetActive(true)
    WoodWormLogic.OnBounceFromWormEgg → Bounce 态
    // 不调 base：不走普通尸体移除
```

`ViewBrokenEgg`：图有完整台词；Condition `TriggerCount==0` 只分流首句/次句，**不是空壳死 Condition**。Click 才会 `TriggerStory` → `hasInStoryEventState` / Pause。**不按 E 不应进对白锁。**

---

## 3. 嫌疑裁定

| # | 嫌疑 | 裁定 | 证据 |
|---|------|------|------|
| **A 碎壳 GroundCld 仍实心** | **须 Play 验；代码已关** | OnDead 显式 `enabled=false`；nodes 仍不含 GroundCld。若卡死帧仍 enabled→回归/引用丢；若 false 仍顶→其它盒（G） |
| **B 自动爬锁** | **次主 / 可叠加** | 洞内永不 Exit 宽盒；`StartAutoCrawl`→`DisablePlayerMove`+`AutoMove`。0914 已为**进洞门**加 `StopAutoCrawlForPlayer`+`blockAutoCrawl`，**碎卵路径未调**。若卡死帧 `isCrawling` 且 AutoMove 空、Disable 仍 true → 像「完全没响应」 |
| **C 观察对白锁** | **弱（不点 E）** | Click；E 出现是设计。仅当已点开且 `HasRunningStory` 才锁 |
| **D 孵虫 StopMove** | **主嫌疑（打碎之后特有）** | `OnCollisionMonster` **跳过卵、不跳过木虫**；ClimbMove 下贴虫 → `StopMove` + `canInStateSetPos=false`。虫出生在 ≈273，人砸完常停在卵前 → 极易贴上 |
| **E 人 Y−1 / 地板夹** | **加重项** | 0914 已施工 `InTreePlayerYOffset=1`；`GroundUp` 顶板 Y≈-7.35。可能与打卵位置叠出「贴地钉死」，但**不是碎卵脚本独有** |
| **F 蹲击态卡死** | **可能** | 洞内 `isEnableSquatUp=false`；攻完应回 SquatStay 再爬。若 `canInStateSetPos=false` 未清，位移发飘/假死 |
| **G 其它 OnlyMapObj 盒** | **备选** | GroundCld 已关仍挡时扫脚接触列表 |

### 「动不了」三分类（施工前必选一）

| 类型 | 表现 | 卡死帧看什么 |
|------|------|----------------|
| **位移锁** | 按方向完全无位移意图 | `cantMove` / `AutoMove` / `HasRunningStory` / Pause |
| **脚本刹** | 有爬姿态但每帧被停 | `IsClimbMove` + 身侧活木虫 + `canInStateSetPos==false` |
| **物理挡** | 像顶墙 | `GroundCld.enabled` 或脚接触实心盒 / 顶板 |

截图同时有 **E + 碎卵 + 幼虫** → 优先验 **D**，再验 **A**，再验 **B**。

---

## 4. 与近期改动关系

| 改动 | 关系 |
|------|------|
| 0723 `groundCld.enabled=false` | 已在基类；若仍挡须证「没执行到 / 又被打开 / 不是这只盒」 |
| 0914 洞口停爬 | 只服务 `ForestEastTreeEnterTrigger`；**碎卵不调用** |
| 0914 洞内人 Y−1 | **已落地**；可加重顶板夹死（OPEN Q4），勿当唯一根因回滚贴底 |
| 手动挪 Ground* | 无重力吸附，**不能**单独解释碎卵后卡死 |

---

## 5. 方案对比与推荐

### 方案 A（推荐 · 先证后修）：卡死帧三分 + 最小补丁

1. Debug（前缀 `[WormEggBreak]` / `[CanNotSomeActionArea]`）：死瞬间与卡死帧打 `GroundCld.enabled`、孵虫 Active/坐标、Start/Stop、Disable/AutoMove/Climb、`HasRunningStory`、player.y、脚 Overlap。  
2. 按结果最小修（可组合）：  
   - **A 成立**：死后及 `MonsterDeadEndEvent` 末再断言 `groundCld.enabled=false`（防动画/子物体改回）；可选把 GroundCld 挂进 `CldController.nodes`。  
   - **D 成立**：孵虫 Bounce 短时 `isEnableMovePassMonster` / 跳过对新孵木虫的 `StopMove`，或出生点改到卵**右侧**（远离玩家）；**勿**对活卵拆掉 GroundCld。  
   - **B 成立**：碎卵 `OnDead` 或蹲击结束调用 `StopAutoCrawlForPlayer`，并在仍处 SquatUp 区内 **重新 `isEnableSquatUp=false`**（Stop 会把站起打开，洞内不许站）。  
3. **保留** `ViewBrokenEgg`；最多保证 Click 结束解锁（若证 C）。

### 方案 B：只关 GroundCld / 只挪孵虫

单修易漏「打碎后」木虫刹。可作 A 的子集，不宜单独结案。

### 方案 C：删 E / 缩爬区到洞外 / 回滚 Y−1 或贴底

**否决或另案**：删 E 违反产品；缩爬区破坏洞内必须爬；无因果回滚贴底/Y 禁止作本票主修。

---

## 6. 验收（拍板施工后）

| # | 项 | 期望 |
|---|----|------|
| 1 | 新档左口 → 碎 Type3 → **不按 E** | 能越过碎壳继续爬 |
| 2 | 碎后按 E 听完 | 旁白结束可爬 |
| 3 | 第二颗卵 | 仍可打、仍挡路（活卵 GroundCld 在） |
| 4 | 出洞 | 正常；Y 偏移仍对称恢复 |
| 5 | Console | 死瞬间 GroundCld=false；爬区 Start/Stop 可解释 |

---

## 7. 替代方案说明

| 路径 | 说明 |
|------|------|
| **权威** | Play 三分后按 A/D/B 最小补丁；活卵继续挡路 |
| **只删碎壳** | 违反「碎壳不销毁」美术/成就路径，不推荐 |
| **Ignore 矩阵 PlayerFoot↔OnlyMapObj** | 0723 已否（挡板副作用）；本期勿开 |

---

## 8. 参考路径

| 用途 | 路径 |
|------|------|
| 卵逻辑 | `WormEggLogic.cs` |
| 死后关盒 | `BaseMonster.OnDead` |
| 开 E | `TreeBridgeLogic.Update` |
| 爬刹 | `PlayerBodyCollider.OnCollisionMonster` |
| 自动爬 | `CanNotSomeActionArea.cs` |
| 旁白 | `ForestEastSceneViewBrokenEgg.prefab` |
| Prefab | `WormEggType3.prefab` |
| 0723 | `Assets/Doc/执行文档/7月/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md` |
| 洞口爬锁 | `0914/ForestEast_洞口爬完对白不触发卡死_*` |
| 人 Y−1 | `0914/ForestEast_树洞内主角再降1单位Y_施工说明.md` |
