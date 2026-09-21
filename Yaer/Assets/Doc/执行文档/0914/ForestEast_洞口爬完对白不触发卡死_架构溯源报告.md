# ForestEast 洞口爬完 · 对白不触发 + 卡死 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 场景 Trigger / 相机 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` 倒树**左洞口外侧**（用户截图：洞外站立、HUD 仍在、人已起来）  
**现象**：洞口爬完 **不播对白**，同时 **卡死**（须钉死锁在哪一层）  
**产品期望**：爬完接到**现网原对白**；操作恢复；进/出洞不回归卡死  
**近期改动（对照）**：0914 进洞相机贴底；倒树合层换图；用户可能调过地板 Y  
**不是**：改对白文案；无因果回滚贴底；重写整棵树桥  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_洞口爬完对白不触发卡死_架构侦探提示词.md`  
**OPEN**：`OPEN_QUESTIONS.md` 本节 Q1～Q5

---

## 沟通摘要

### ① 结论一句话

截图钉死的是 **左口外侧、还没进黑幕**：「爬完该播的对白」是进洞后的 **`ForestEastSceneEnterTreeBridge`**（盒在传送点 x≈264），不是洞内 `PassTreeBridge`。卡死主层是 **A：`CanNotSomeActionArea` 自动爬未 `StopAutoCrawl`**（盒横跨洞口～洞内约 80 单位，人还在盒里就不会解锁）。对白不播是因为 **没走完 `AutoEnter` 黑幕传送**，进洞对白盒够不着；**与 0914 贴底无关**。`GroundCenter` 是**洞内地板**，洞外踩的是 `GroundLeft_1`。

### ② 原因（通俗）

洞口有两套东西叠在一起：一套逼你蹲着爬、爬的时候把走和菜单锁死；另一套碰到窄门才黑屏送进树洞，进洞后才播「进树」那句。现在人在洞外已经站起来了，说明黑屏没发生。锁又常常没解开——因为「必须爬」的触发区从洞口一直铺到树洞里面，人没爬出这块地就不会 `StopAutoCrawl`。对白盒还在树里面的落点上，人没被送进去当然没词。镜头贴底只在黑屏之后才跑，这张截图还没走到那一步。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网（未施工） |
|---|------|----------------|
| 1 | 从左侧走近倒树（x≈248） | 应先播 **BeforeEnter**（单次存档）；再爬才是进洞对白 |
| 2 | 卡死帧看 Console | 有 `[CanNotSomeActionArea] StartAutoCrawl` **无** `StopAutoCrawl` → 坐实 A |
| 3 | Hierarchy 卡死帧 | `DisablePlayerMove` / `AutoMoveState` / `isEnableSquatUp` / `hasFindPlayer` / BlackPanel 是否打开 |
| 4 | 人与盒 | 洞外站在 `GroundLeft_1` 上；`AutoEnterLeft` 仅约 **1 单位宽**；`EnterTreeBridge` 在 **x=264.62**（传送点上） |
| 5 | 勿先回滚贴底 | 贴底入口在黑幕回调里，与「洞外站立 + HUD」因果对不上 |
| 6 | 拍板施工 | 优先：AutoEnter 命中时先停自动爬；等待 Sign 加超时；顺手修正 Out **Right** 双写 Left |

### ④ 程序补充

见下文。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| 「洞口爬完」 | **CanNotSomeActionArea 爬段结束、尚未 `AutoEnter` 黑幕**。不是 `PassTreeBridge`（x≈342 洞内更右） |
| 该播的对白 | 走近树：**BeforeEnter**；**钻进洞并传送后**：**EnterTreeBridge**。截图未进洞 → 缺的是 **Enter**（BeforeEnter 可能已 SingleUse 播过） |
| 卡死层 | **主 A**（自动爬锁：`DisablePlayerMove` + `AutoMove` + `isEnableSquatUp=false` + 菜单关）。**次 C**（`AutoEnter.hasFindPlayer` 已置、Sign 一直不是 Idle/Run/Squat → 永不黑幕也不解锁）。**H** 需 Console/`HasRunningStory` 才能坐实 |
| 对白失败 | **没相交到 Enter 盒**（人还在洞外）；不是先改文案。F（SingleUse）可解释「没词」但**不能单独解释卡操作** |
| 0914 贴底 | **无关**（`ChangeCamera(true)` + `SnapLiveOrthoYToConfinerFloor` 只在黑幕 `onShowEnd`） |
| GroundCenter | 洞内 x≈272～328、顶面 Y≈**-7.6**。洞外是 **`GroundLeft_1` 顶≈-6.6**。只改 GroundCenter **解释不了洞外卡死** |
| 推荐 | **方案 A**：`ForestEastTreeEnterTrigger` 命中先 `StopAutoCrawl`；Sign 等待加超时；修正 `ChangeEnterAndOutNodeActive` / `AwakeAllStoryNodeActive` 的 Right 双写。盒可略加宽（次要） |
| 禁止 | 无因果回滚贴底；改 CSV；把 GroundUp 当洞顶乱降 |

---

## 2. 调用链

### 2.1 左口现网空间（场景 YAML）

从左往右（玩家 +X）：

| 物体 | Transform | 盒（Offset / Size） | 世界 X 约 | 世界 Y 约 | 作用 |
|------|-----------|---------------------|-----------|-----------|------|
| `BeforeEnterTreeBridgeStoryTrigger` | (248.14, 0) | (0, 0.02) / (1.9, **14.01**) | 247.2～249.1 | **-7.0～7.0** | 对白 `ForestEastSceneBeforeEnterTreeBridge`，Enter，**SingleUse** |
| `AutoEnterTreeBridgeStoryTriggerLeft` | (255.96, 0.97) | (0.42, **-3.2**) / (**1.05**, 8.7) | **255.86～256.91** | **-6.58～2.12** | `ForestEastTreeEnterTrigger` `isEnterTree=1`，**无对白名** |
| `CanNotSomeActionArea` | (301.32, -5.07) | (-2.34, 0.43) / (**80.64**, 3.08) | **258.66～339.30** | **-6.18～-3.10** | `SquatUp`：自动爬 + 锁操作 |
| `AutoOutTreeBridgeStoryTriggerLeft` | (260.43, 0.97) | (-0.51, -3.2) / (0.88, 8.7) | 259.5～260.5 | 同 AutoEnter 高 | 出洞（`isEnterTree=0`） |
| `EnterTreeBridgeStoryTrigger` | (264.62, 0) | (0, -3.2) / (1.9, 8.7) | 263.7～265.6 | **-7.55～1.15** | 对白 `ForestEastSceneEnterTreeBridge`，Enter，**SingleUse** |
| `TreeBridgeInPosLeft` | (264.33, **-6.3**) | — | 传送 **只抄 X** | 保留进洞前 Y | |
| `TreeBridgeOutPosLeft` | (252.94, -6.3) | — | 出洞落点 X | | |
| `PassTreeBridgeStoryTrigger` | (342.14, 0) | — | 洞内更右 | | **勿与洞口混淆** |

洞外地面：**`GroundLeft_1`** `(128.8, -7.6)` Size `(289.7, 2)` → 顶面 ≈ **-6.6**，X 覆盖到 ≈273。  
**`GroundCenter`** `(300.47, -8.6)` Size `(56.1, 2)` → 顶面 ≈ **-7.6**，X ≈272～328，**只覆盖洞内**。  
`GroundUp` 同 X 段、Y=-7.35，是洞内顶板，不是洞口外地板。

关键间隙：**AutoEnter 右缘 ≈256.91，爬行区左缘 ≈258.66**，中间约 **1.7 单位**既不是进洞门、也尚未强制自动爬。AutoEnter **X 仅约 1 单位**，比 BeforeEnter / Enter 对白盒窄得多。

### 2.2 爬区 → 锁 / 解锁

```
OnTriggerEnter CanNotSomeActionArea (SquatUp)
  isEnableSquatUp = false
  StartAutoCrawl（若当时已有 S + 左右）
    DisablePlayerMove(true)     // 含 isEnableSquatUp=false、SetAllowMove(false)
    SetAllowOpenMenu(false)
    AutoMoveState = Left|Right
    若未蹲：ChangeStateToSquat()

Update：区内且未 isCrawling，再按 S+方向可再次 Start

StopAutoCrawl 仅当：
  a) 已 Exit 盒 + hasEnteredClimbMove + 0.35s jitter
  b) 3s 内从未进入 IsClimbMove
  c) OnDisable
否则：一直锁走 / 锁站起 / 锁菜单 —— 看起来像卡死
```

`ClimbMoveState`：无移动输入会进 **`ClimbUpState`**（**不看** `isEnableSquatUp`），动画结束 → `SquatStay2State`。截图「人已起来」可以是 ClimbUp / 超时 Stop 后站起，**不等于**已经 `StopAutoCrawl`。

玩家 Body 胶囊 Offset.y=2.56 Size.y=5.3，蹲下**不改盒**。Y 擦边对 Body↔Trigger 不如「窄 X + 超宽爬区」致命。

### 2.3 进洞 Trigger → 对白盒

```
AutoEnter OnTriggerEnter / OnCollisionEnter
  hasFindPlayer = true
  DisablePlayerMove()           // 默认 true
  SetSceneObjIsPause(true)      // 只拦怪/部分逻辑；Trigger.Update 仍跑
Update 等到 IsIdle | IsRunning | Squat
  （Climb 在 SquatSM 下，Squat Sign 一般为 true → 按理能过）
  hasInTiggerStory = true
  playerIsInTreeBridge = isEnterTree
  ChangeEnterAndOutNodeActive(true)
    Enter L/R Active
    Out Left SetActive(!enter) 写了两遍；Out Right 从未改  ★E
  AutoMove 1s → Open BlackPanel FadeShow
    只改玩家 X = TreeBridgeInPosLeft.x，Y 保留
    ChangeCamera(true) → 换盒 + Size=5 + SnapLiveOrthoYToConfinerFloor  ★贴底在这里
    ClimbUpState
    CloseFormFade → DisablePlayerMove(false); isEnableSquatUp = false（进洞保持蹲）
    AwakeAllStoryNodeActive()   // 同样只写 Out Left 两遍
```

进洞落在 x≈264.33，与 **`EnterTreeBridgeStoryTrigger` 同位置** → `SimpleStoryTrigger` Enter → `StoryComponentGSM.TriggerStory("ForestEastSceneEnterTreeBridge")`。  
**人还在洞外时这个盒默认够不着**（除非已经爬到 x≈264 且盒 Y 相交——仍无黑幕、无 `playerIsInTreeBridge`）。

`Pause` 注释写明只影响场景物能否移动，**不会停掉** `ForestEastTreeEnterTrigger.Update`。C 成立条件是 **Sign 三者全假**（Hurt / JumpFall / Dash 等），不是 Pause 冻住 Update。

---

## 3. 证据表（复现矩阵）

| 操作 / 项 | 期望 | 现网（只读） |
|-----------|------|----------------|
| 洞外 S+方向爬过矮洞 | 爬完站起或保持蹲 → **黑幕进洞** → 播 **Enter** | 截图：**洞外站立、HUD 在、无黑幕** → 进洞链未完成 |
| BeforeEnter @248 | 第一次走近播「好大的树」 | 高盒，脚在 -6.6 应能碰到；**SingleUse** 后再走无词、不应卡操作 |
| Enter @264 | **传送之后**播 | 盒与 InPos 对齐；未传送则不播 |
| Console Start/StopAutoCrawl | 出爬区必有 Stop | 盒宽 80，洞口仍在区内 → **极易只有 Start** |
| 卡死锁 | 可走、可菜单 | A：锁在 `PlayerLogic.DisablePlayerMove` + `InputComponentGSM.SetAllowOpenMenu(false)` |
| 人 vs AutoEnter | 必进黑幕 | 门宽 ≈1；与爬区还有 1.7 空档 |
| 人 vs Enter 盒 | 进洞后相交 | 洞外默认不相交 |
| SingleUse | 再爬无词 | 无词 ≠ 卡死 |
| 左口 / 右口 | 两边都能进 | **E**：Out **Right** 从未 SetActive；左口首进不是主因，右口/再出可能脏 |
| 0914 贴底 | 进洞镜头低 | **未进洞则未执行** |
| GroundCenter Y | 洞内爬轴 | 洞外不踩它；用户若改的是 **GroundLeft_1** 才可能擦 AutoEnter 底 -6.58（OPEN Q5） |

嫌疑对照：

| 嫌疑 | 裁定 |
|------|------|
| **A 自动爬未解锁** | **主因（卡操作）**。洞口坐标落在 80 宽盒内 |
| **B 脚 Y 与对白盒错开** | Enter 盒底 -7.55，洞外脚 -6.6，**不是主因**。AutoEnter 底 -6.58 略紧，Body 仍高 5.3 |
| **C AutoEnter 等 Sign** | **次因 / 叠加**。Squat 爬行按理能过；卡在 Hurt/跳等才死等。须打 `hasFindPlayer` |
| **D Pause 未解** | Pause 本身不停 Trigger；若 C 死等则 Pause 会一直 true |
| **E 双写 Left** | 确认 bug；**左口首次进入不是主因** |
| **F SingleUse** | 只解释没词 |
| **G 贴底** | **否** |
| **H 对白壳 / isTalking** | 工程无本地 `isTalking`；锁用 `hasInStoryEventState`。壳卡住需 `HasRunningStory` 证据（OPEN Q1） |

---

## 4. 方案对比与推荐

### 方案 A（推荐）：停爬解锁 + Sign 超时 + 修正 Right

1. `ForestEastTreeEnterTrigger` 一旦 `hasFindPlayer`：对当前 `PlayerLogic` **先停自动爬**（抽 `CanNotSomeActionArea.StopAutoCrawl` 或场景里找该组件调用）。避免进洞流程和 AutoMove/Disable 叠锁。  
2. Sign 等待 **加超时**（如 2s）：仍不是 Idle/Run/Squat 则 **强制 `ChangePlayerState`** 或 **解锁并清 hasFindPlayer**（禁止永久 Disable+Pause）。  
3. `ChangeEnterAndOutNodeActive` / `AwakeAllStoryNodeActive`：第二处 `storyTriggerOutNodeLeft` 改为 **`storyTriggerOutNodeRight`**。  
4. （可选）`AutoEnterLeft` 盒 X 加宽、底边略降，与 BeforeEnter 一样覆盖站姿脚 Y。  
5. **不改** `CameraTreeInArea`、贴底公式、对白 Prefab 文本。

**优点**：对上卡死 + 进洞对白；贴底保留。  
**缺点**：要动 Trigger 脚本；超时策略须在 OPEN Q4 钉死「强制进洞 vs 解锁重试」。

### 方案 B：缩短 `CanNotSomeActionArea`

把 80 宽盒收成 **洞内**（约 GroundCenter 段），洞口外不自动爬、不锁站起。洞口只靠碰撞高度逼蹲。  
**优点**：洞外不再被超宽盒锁死。  
**缺点**：洞内仍要禁站起；洞口可能站着撞树；不修 AutoEnter 窄门。可与 A 组合，**不宜单独当最小修**。

### 方案 C：只加高/加宽对白盒与 AutoEnter

只解决擦边，**不解 A 的锁**。作 A 的附件，不单独上。

### 方案 D：回滚 0914 贴底

**否决**：贴底在黑幕之后；与洞外截图无因果。

---

## 5. 施工步骤（拍板后）

1. 按 A：EnterTrigger 命中 → StopAutoCrawl；Sign 超时；Right 双写。  
2. 验收：左、右洞口各爬一次；有原对白；爬完可走、可菜单；进洞贴底仍在；出洞正常。  
3. 文档：`Assets/Doc/施工说明/0914/ForestEast_洞口爬完对白不触发卡死_施工说明.md`。

### 验收

| # | 项 | 期望 |
|---|----|------|
| 1 | Console | Start 后必有 Stop（进洞或离开爬区） |
| 2 | 左口 | 黑幕进洞 + Enter 对白（存档未用过时） |
| 3 | 爬完 | 可走、可 ESC 菜单 |
| 4 | 进洞镜头 | 仍贴底（afterY≈-2.9） |
| 5 | 右口进出 | Out Right 会随进洞显隐 |

---

## 6. 替代方案说明

| 路径 | 说明 |
|------|------|
| **权威 A** | 进洞门负责停爬 + 禁止 Sign 死等 |
| **只缩爬区 B** | 洞外解锁，进洞门仍可能窄 |
| **只改盒 C** | 不解 DisablePlayerMove |
| **FormLogic 空判断式修对白** | 对白没播是因为人没到盒，不是 Image 空 |

---

## 7. 参考路径

| 用途 | 路径 |
|------|------|
| 爬区 | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/limitPlayerPrefab/CanNotSomeActionArea.cs` |
| 进/出洞 | `…/ForestEastTreeEnterTrigger.cs` |
| 树桥 Mgr | `…/ForestEastTreeBridgeStoryMgr.cs` |
| 对白 Trigger | `…/SimpleStoryTrigger.cs` |
| 场景 | `Assets/GameRes/Scenes/ForestEastScene.unity` |
| 贴底施工 | `Assets/Doc/施工说明/0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md` |
