# Cursor Agent Prompt · WestRappRoad 拉普路西宝箱不能互动

> **角色**：先【架构侦探】只读查清「不能互动」根因；拍板后【施工员】最小修  
> **日期**：2026-09-14  
> **场景**：`WestRappRoad`（拉普路西）  
> **目标物体（用户 Hierarchy 截图）**：`Design / Near / Box`（PrefabInstance，名 `Box`）  
> **旁注**：同层附近有 `Design / Interaction`（SortingGroup 分组），**不是**宝箱本体；勿把互动失败怪到这个空壳节点上  
> **现象（用户）**：宝箱 **不能互动**（须钉死：无按键提示 / 有提示按了没反应 / 已开合状态不能再开 / 走近无 Trigger）  
> **产品期望（钉死）**：未开过的西境药水箱，走近应出互动提示，确认键能开箱，发 HP/MP 球 + Tips，存档 `WestRappRoadData.hpMpBoxOpened`；已开过则保持开箱造型且不可再互动（正常）  
> **不是**：改村巨树宝箱 / 重写 Interactive 全局；把 `HomeScene2Box` 当主逻辑挂回；用剧情 Prefab 名硬开对白（现网 `useStoryOnOpen: 0`）  
> **报告落盘**：`Assets/Doc/执行文档/0914/WestRappRoad_宝箱不能互动_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 拉普路西路上那个宝箱，不知道为啥不能互动了。  
> Hierarchy 里在 `Near` 下面叫 `Box`。查是存档已经开过、碰撞对不上人、脚本挂坏了，还是互动组件被关掉。

### 现网挂法（磁盘预扫）

| 项 | 现网值（须再核实） |
|----|-------------------|
| 路径 | `WestRappRoad` → `Design` → `Near` → **`Box`** |
| Prefab | `Assets/Prefabs/Box.prefab`（guid `3d24231045a0614438f37d7cba4b5649`） |
| 世界/本地坐标 | 实例 Local **`(66.582, -3.44, 0)`**（Near/Design 父级约 0） |
| 场景逻辑脚本 | 附加 **`WestRappRoadHpMpBox`**（guid `7f3c1d0a9e2d4aaebf4ac8d2e6c9f0a1`） |
| 开箱配置 | `useStoryOnOpen: 0`；`storyName: WestRappRoadHpMpBox`（仅故事模式用）；`hp/mpBallCount: 2`；`enableDebugLog: 1` |
| Prefab 原 `HomeScene2Box` | 实例上 **`m_RemovedComponents`** 已移除（且曾 `m_Enabled: 0`）→ 应以西境脚本为准 |
| 存档旗 | `WestRappRoadData.hpMpBoxOpened`（key `WestRappRoadData_hpMpBoxOpened`） |
| Prefab 交互 | `InteractiveComponent` + `Clds/Body`（进范围）+ `Click`（射线点选）；`EntityControll.canTouchWithPlayer` 默认 1 |
| 旁邻 `Interaction` | `Design` 下 SortingGroup 父节点，子物体是其它互动点，**≠ Box** |

### 互动链路（预期）

```
走近 Box Body 触发器 / 点选 Click
  → InteractiveComponent（canTouchWithPlayer）
  → onClickInteractiveEvent
  → WestRappRoadHpMpBox.OpenBox
  → OnWestRappRoadHpMpBox_OpenBox + GetHpMp（useStoryOnOpen=false）
```

`OnInit`：若 `hpMpBoxOpened` → 动画 Open + `opened=true` + **`canTouchWithPlayer=false`**（已开则故意不能互动）。

脚本已自带 Debug：`[WestRappRoadHpMpBox][OnInit-…]` / `[OpenBox]` / 与 `HomeScene2Data.boxOpened` 串档告警。

### 「不能互动」嫌疑优先级（侦探排序）

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A** | 存档已开：`hpMpBoxOpened=true` → 合法锁互动 | Console OnInit 日志；清档/新档对比；Animator Open 是否已 true |
| **B** | **Y 对不上**：箱在 **y≈-3.44**，西境地面玩家常在 **≈-6.61**；Body 盒高约 1.5，走近可能 **永不 Overlap** → 无提示 | Scene 里对 Box 与 Player 画 Gizmo；量 bounds；对照同场景其它可互动物 Y |
| **C** | `canTouchWithPlayer=false` 但档未开（串档 / 误读 Home / OnInit 顺序） | 看脚本内 Error：「west=false 但 home.boxOpened=true 且 canTouch=false」 |
| **D** | Interactive 引用丢：`interactiveCollider` / cldListeners / Missing Script | Inspector；对照 SystemTipsPanel2 Missing 案 |
| **E** | Collider 关 / Layer / Trigger 配错 / 被其它大盒挡住 | Body/Click 的 IsTrigger、Layer Matrix |
| **F** | 实体未走 `BaseSceneEntityLogic.OnInit`（未进 Entity 体系） | Play 后有无 `[WestRappRoadHpMpBox][OnInit-…]` |
| **G** | `useStoryOnOpen` 被改 true 且故事 Prefab 缺失 → 开了但像「没反应」（现网磁盘是 0，仍须 Inspector 核实） | 点了有无 OpenBox 日志、有无 TriggerStory 失败 |

### 侦探须先钉死的现象句

在报告开头用 **一条** 写清用户实际是哪一种（Play 核实）：

1. 走近 **完全没有** 互动键提示  
2. 有提示，按确认 **无反应**（无 OpenBox 日志）  
3. 有 OpenBox 日志，但 **无奖励/无动画**  
4. 箱子 **已经是打开造型**（多半存档已开）

### 复现 / 对照

| 操作 | 期望信息 |
|------|----------|
| InitScene → 进 `WestRappRoad`，新档或确认 `hpMpBoxOpened=false` | OnInit 日志 `hpMpBoxOpened=false`，`canTouchWithPlayer=true` |
| 走到 `x≈66.6` 箱旁 | 出键提示；按确认 → OpenBox 日志 → 开箱动画 + GetHpBall/GetMpBall Tips |
| 再读档回西境 | 箱保持开；不能再互动 |
| 对照 | 同场景其它可互动物（告示牌/花丛等）是否正常 → 区分「全局互动坏」vs「仅此箱」 |

### 侦探须回答

1. 现象属于上表 1～4 哪类？  
2. 根因字母（可组合，如 B+A）；**唯一主因**写清楚。  
3. Box Body 世界 bounds 与玩家互动盒是否相交？Y≈-3.44 是否合理（高台箱 vs 摆错高度）？  
4. `HomeScene2Box` 是否已彻底不参与？有无 Missing？  
5. 推荐最小修：挪箱 Y / 放大 Body / 清档说明 / 修引用 / 其它——**禁止**未证实就改 Interactive 全局。  
6. 施工清单 3～8 条 + 验收句。

### 必读

1. `Assets/Project_context.md`、`Assets/Doc/02_SYSTEM_SPEC.md`  
2. `WestRappRoadHpMpBox.cs`、`WestRappRoadData.cs`  
3. `InteractiveComponent.cs`、Prefab `Assets/Prefabs/Box.prefab`  
4. 场景 `Assets/GameRes/Scenes/WestRappRoad.unity` 中 PrefabInstance `Box`（约 fileID `1573113626`）  
5. 对照样板：`HomeScene2Box`；村箱文档仅作挂法参考，**勿改村场景**  
6. 用户 Hierarchy 截图（`Near/Box`）  
7. 本提示词  

### 禁止（侦探阶段）

- 禁止改代码 / 场景坐标 / 存档 / Git  
- 禁止把 `Design/Interaction` 当成宝箱去「修」  
- 禁止未分清「已开锁互动」与「坏了」就改逻辑  

### 侦探输出

1. 溯源报告 → 文首落盘路径  
2. 结论一句话 + 主因字母  
3. 给施工员的最小改清单  

---

## 【施工员】提示词（侦探拍板后整段交给 Agent）

> **角色**：【施工员】  
> **前置**：已读 `Assets/Doc/执行文档/0914/WestRappRoad_宝箱不能互动_架构溯源报告.md`  
> **目标**：未开箱可互动开箱发奖；已开箱保持锁互动  
> **施工说明落盘**：`Assets/Doc/施工说明/0914/WestRappRoad_宝箱不能互动_施工说明.md`

### 按报告主因最小改（示例，以报告为准）

| 报告主因 | 允许动作 |
|----------|----------|
| **A 已开档** | 文档说明如何清 `WestRappRoadData_hpMpBoxOpened` / 测新档；**勿**为了「再开一次」强行忽略存档 |
| **B Y/碰撞** | 只调 **本实例** Transform Y 或 Body 盒，使与玩家地面轴可重叠；注释写清对照地面 Y；勿改全局 Padding 除非报告要求 |
| **C 串档/误读** | 修 `WestRappRoadHpMpBox` 读档/canTouch 赋值；保持读 `WestRappRoadData` |
| **D/E 引用/Collider** | 补绑 Interactive 引用、开 Trigger、Layer；禁止重写组件 |
| **F 未 OnInit** | 按实体注册方式最小接入（对齐同场景其它 SceneEntity） |
| **G 故事模式** | 保持 `useStoryOnOpen=0` 直开，或补齐故事资源（报告二选一） |

### 禁止（施工）

- 改 `VillageKenMuNi1*` / 巨树宝箱  
- 恢复 `HomeScene2Box` 当西境主逻辑  
- 大重构 Interactive  
- `git commit` / `push`（除非用户另嘱）  

### 完工自检

- [ ] 新档/未开旗：走近有提示，确认开箱，双球 + Tips  
- [ ] 读档已开：造型开、不可再互动  
- [ ] Console 无串档 Error；`enableDebugLog` 可保留或按报告关  
- [ ] 施工说明已落盘  

---

## 用户怎么用

1. Agent 先跑侦探段 → 出溯源报告（务必 Play 看 `[WestRappRoadHpMpBox]` 日志 + 对一下箱/人 Y）。  
2. 确认主因后跑施工员段。  
3. 西境按验收再打一遍开箱。  
