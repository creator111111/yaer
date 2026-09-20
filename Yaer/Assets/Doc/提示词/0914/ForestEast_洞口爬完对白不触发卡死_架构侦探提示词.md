# Cursor Agent Prompt · ForestEast 洞口爬完：对白不触发 + 卡死

> **角色**：先【架构侦探】只读定位卡死点与对白为何没播；拍板后【施工员】最小修复  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` 倒树洞口（用户 Game 截图：洞外站立、HUD 仍在、人已起来）  
> **现象（用户实测）**：  
> 1. **洞口爬完**后 **不会触发对话**  
> 2. **直接卡死**（操作/流程停住；侦探须写清是：不能走、不能出菜单、黑幕停、状态机死在 Climb、还是对白壳卡住）  
> **产品期望（钉死）**：洞口爬完应正常接到原对白（进洞前 / 进洞，以现网设计为准），操作恢复，不卡死  
> **近期改动（必对照，勿当唯一真相）**：  
> - 0914 进洞相机贴底：`ChangeCamera(true)` + `SnapLiveOrthoYToConfinerFloor`；`StopCameraAction` 洞内再贴一次  
> - 用户刚要/可能已调 **`GroundCenter` 地板 Y**（爬行轴线偏高）  
> - 倒树合层换图  
> **不是**：改对白文案；改相机贴底公式当首选（除非报告证明贴底导致卡死）；重写整棵树桥  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_洞口爬完对白不触发卡死_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 倒树洞口蹲着爬过去，爬完该说话，现在不说话还卡住。  
> 刚改过镜头贴底、可能动过地板高度。查是人没碰到对白盒、操作没解锁，还是进洞流程半路停了。

### 洞口相关触发（现网预扫）

| 物体 | 位置（约） | 对白 Prefab | triggerType |
|------|-----------|-------------|-------------|
| `BeforeEnterTreeBridgeStoryTrigger` | x≈**248.14**，盒高约 14 | `ForestEastSceneBeforeEnterTreeBridge` | 须读 Inspector |
| `EnterTreeBridgeStoryTrigger` | x≈**264.62**，Offset.y=**-3.2**，Size **(1.9 × 8.7)** | `ForestEastSceneEnterTreeBridge` | **Enter(1)**，`SingleUseInArchive: 1` |
| `AutoEnterTreeBridgeStoryTriggerLeft/Right` | 左右洞口 | 无对话名（进洞传送） | `ForestEastTreeEnterTrigger` |
| `PassTreeBridgeStoryTrigger` | 洞内更右 | `ForestEastScenePassTreeBridge` | 过桥后，勿与洞口混淆 |

进洞传送点：`TreeBridgeInPosLeft/Right` **Y=-6.3**（代码 **只抄 X**）。

### 「卡死」嫌疑优先级

| # | 嫌疑 | 为何像 |
|---|------|--------|
| **A（主 · 自动爬未解锁）** | `CanNotSomeActionArea`（SquatUp）`StartAutoCrawl` → `DisablePlayerMove(true)` + `AutoMove`；爬完若 **没 `StopAutoCrawl`** → 不能走/像卡死，也对白碰不到或碰了播不了 | 洞口本就有「必须爬过去」区；超时 3s 才停，若一直判定在区内会永不 Stop |
| **B** | 地板/相机改完后 **脚 Y 与对白 Trigger 盒错开**（Enter 盒 Offset.y=-3.2，偏上；人降了可能擦边或穿出） | 换图+用户要降 `GroundCenter` |
| **C** | `ForestEastTreeEnterTrigger`：禁走 → 等 `IsIdle\|IsRunning\|Squat` → 黑幕；若死在 **Climb** 且 Sign 对不上 → **永远不进黑幕也不恢复** | 洞口爬完立刻进 AutoEnter |
| **D** | `hasInTiggerStory` / `hasFindPlayer` 卡住；或 `SetSceneObjIsPause(true)` 未解开 | 半截进洞 |
| **E** | `ChangeEnterAndOutNodeActive` **现网把 `storyTriggerOutNodeLeft` SetActive 写了两遍，Right 从未改** | 出洞/对侧重进可能脏；须核实是否影响「洞口进」 |
| **F** | `SingleUseInArchive` 已写过，再爬无对白；但不应卡死 | 无对白 ≠ 卡操作 |
| **G** | 0914 `SnapLiveOrthoYToConfinerFloor` / `StopCameraAction` 再贴：出洞误贴、或 DOTween 拽 MainCamera 导致输入/对白壳异常 | 刚施工；须证明因果再动相机 |
| **H** | 黑幕 `BlackPanel` 未 Close；`isTalking` 锁操作 | 对白以为开了其实没 |

### 复现矩阵（侦探须填现网）

| 操作 | 期望 | 现网 |
|------|------|------|
| 洞外走近洞口，S+方向爬过矮洞 | 爬完站起，**播对白**（写清是 BeforeEnter 还是 Enter） | 不播 + 卡 |
| Console | 有无 `[CanNotSomeActionArea] Start/StopAutoCrawl` | |
| Hierarchy 卡死时 | `DisablePlayerMove` / `AutoMoveState` / `isEnableSquatUp` / `IsClimb*` / Pause / BlackPanel | |
| 人与 `EnterTreeBridgeStoryTrigger` 盒 | 是否相交 | |
| 存档 `SingleUse` | 是否已消耗 | |
| 左口 / 右口 | 是否一边好一边坏（对照 E） | |

### 侦探须回答

1. 「洞口爬完」精确指：**CanNotSomeActionArea 爬出**、**AutoEnter 进洞黑幕后**、还是 **PassTreeBridge**？用截图+坐标钉死。  
2. 卡死时操作锁在哪一层（A/C/D/H）？证据。  
3. 对白为何不播：没碰到盒 / 碰到了被 Sign 挡 / 存档已用 / Pause？  
4. 与 0914 贴底、`GroundCenter` Y 是否有关？无关写明。  
5. 方案 ≥2 + 推荐最小修（优先恢复解锁 + 对白盒相交；禁止大改相机贴底除非因果成立）。

### 必读

1. `CanNotSomeActionArea.cs`（Start/StopAutoCrawl、3s 超时、Exit 0.35s grace）  
2. `ForestEastTreeEnterTrigger.cs`（禁走、等 Sign、黑幕、只改 X）  
3. `ForestEastTreeBridgeStoryMgr`（ChangeCamera 贴底、`ChangeEnterAndOutNodeActive` 双写 Left）  
4. `SimpleStoryTrigger`（Enter / SingleUseInArchive）  
5. 场景：`BeforeEnter*` / `EnterTreeBridge*` / `AutoEnter*` / `GroundCenter`  
6. 施工说明：`0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md`  
7. 用户 Game 截图 + 本提示词  

### 禁止（侦探阶段）

- 禁止改代码 / 场景 / Git  
- 禁止未证明就把贴底相机整段回滚  
- 禁止改对白 CSV 文案  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0914/ForestEast_洞口爬完对白不触发卡死_架构溯源报告.md`

结构：

1. **结论一句话**（卡死层 + 对白未播原因 + 是否与贴底/地板有关）  
2. **调用链**（爬区 → 解锁 → 对白盒 / 进洞 Trigger）  
3. **证据表**（Sign、Pause、Trigger 相交、存档、左右口）  
4. **方案 ≥2** + 推荐 + 验收  
5. 不清处记 `OPEN_QUESTIONS.md`  

回答风格：①结论 ②原因白话 ③检查清单 ④程序补充。

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告已钉死卡死层与对白失败点。  
> **目标**：洞口爬完能播原对白；玩家可走、可菜单；进/出洞不回归卡死。  
> **优先**：解锁 `DisablePlayerMove`/`AutoMove`/`Pause` 泄漏；Trigger 盒与脚 Y 对齐（只动本案相关盒，不改 CameraTreeInArea）；`ChangeEnterAndOutNodeActive` 若确认 Right 写错则最小改正。  
> **禁止**：无因果回滚贴底；改对白内容；降无关 GroundUp 当洞顶。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_洞口爬完对白不触发卡死_施工说明.md`  
> **验收**：左/右洞口各爬一次；对白出；爬完可走；进洞贴底仍在；出洞正常。

---

## 【验收员】Prompt（可选）

> Log：`[CanNotSomeActionArea]` Start/Stop；`[TreeBridgeCam]`；卡死帧打 `AutoMoveState`、`isEnableSquatUp`、Climb Sign、是否 Pause、Trigger 是否 Overlap。  
> 输出：通过项 + 剩余风险（存档 SingleUse、只修了一侧入口）。
