# Cursor Agent Prompt · Village_KenMuNi1：ExitFrom_HomeScene45 出村落点 Y 不准

> **角色**：【架构侦探】只读溯源「为何拖 ExitFrom 的 Y 不改玩家落点纵深」；报告拍板后【施工员】改场景或 WalkArea  
> **日期**：2026-08-22  
> **现象场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`（主查）← 从 `Village_HomeScene45` RightDoor 出村  
> **对照**：`ExitFrom_HomeScene1` / `House_Npc1`（1 号屋已验证可用）  
> **用户反馈（2026-08-22 晚）**：`ExitFrom_HomeScene45` **好像并不准**；从 45 室内出来后的位置 **Y 不受该空物体控制**（Scene 视图 Gizmo 在树屋楼梯平台，Play 落点 Y 对不上）  
> **前序**：`0822/回村门口落点` v1（EnterPos 绑 ExitFrom）；`0822/进屋闪回村` v3（室内 R0，与本期 **出村 Y** 无关）  
> **本阶段**：只读；须画清 **EnterPos → SetPos → 村庄 2.5D 后处理** 全链

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 从 45 号屋出村，应落在 `House_Npc45` 门外（与 1 号屋 `ExitFrom_HomeScene1` 同理）。  
> 美术在 Scene 里拖 `ExitFrom_HomeScene45` 的 **Y**，期望玩家跟着变，但 **Play 后 Y 不变 / 不对**。

### 关键概念：村里 **Y = 纵深（2.5D）**，不是楼梯「高度」

`Village_KenMuNi1` 走 **DNF 式 Village2_5D**：`Transform.position.y` 表示 **前后纵深**，不是 2D 侧视里的「抬高上楼梯」。  
树屋楼梯的**视觉高度**多由 **Sprite 排序 / Z / 美术分层**表达，**不等于**把 ExitFrom 的 Y 往上拖就能让玩家「站在台阶上」。

生活类比：ExitFrom 是「进村深度标尺上的刻度」；你挪了刻度，但系统还有一条 **WalkArea 铁轨** 会把人 **吸到铁轨上**——所以 Y 刻度看似失效。

### 落点链路（侦探必须逐步对拍）

```
RightDoor LoadScene(Village_KenMuNi1)
  → LastSceneName = Village_HomeScene45
  → Village_KenMuNi1 GSM EnterPosConfig 命中
  → BaseGameSceneManager.SetPlayerPos
  → playerLogic.SetPos(enterPos.pos.position)   // 仅用 X/Y，保留 Z
  → PlayerLogic.OnInit / LoadingSceneEndHandle
  → RefreshVillageExplorationFromActiveScene()
  → TownPlayerLocomotion.ApplyVillageMode(true)
       ├─ _villageWorldY ← 刚体当前 Y
       ├─ Clamp(depthYMin, depthYMax)
       ├─ ApplyVillageWalkPolygonPostCorrection()  ← ★ 常改 Y
       └─ FlushAuthoritativeVillageTransformAfterSceneDepthInject()
```

**预扫推论**：`EnterPos` **会**读 `ExitFrom_HomeScene45.position`，但 **进村后** `TownPlayerLocomotion` 的 **`VillageWalkArea` 多边形校正** 可能 **覆盖 Y**（甚至 X）。用户拖 ExitFrom Y 无效，多半是 **后处理** 而非 EnterPos 未绑。

### 磁盘预扫（2026-08-22 晚）

| 节点 | 坐标（世界 ≈ local，Map 根 0） | 说明 |
|------|--------------------------------|------|
| **`ExitFrom_HomeScene45`** | **(-4.3, 7.41)** | EnterPos 已绑 `5601461774444444002` ✅ |
| `House_Npc45` | **(-4.39, 5.67)** | 进村门 |
| **`ExitFrom_HomeScene1`（样板）** | **(45.5, -6.1)** | 1 号屋出门；Y 在村街主纵深带 |
| `House_Npc1` | (45.41, 1.9) | 门 ↔ Exit Δy ≈ **-8.0** |
| 按 1 号屋偏移推算 Npc45 Exit | **(-4.30, -2.33)** | OPEN_QUESTIONS 曾建议；**现网 y=7.41 相反方向** |
| `VillageWalkArea` | 根 **(0, -5.91)** + 多边形 | 含树屋平台顶点（local y≈8～9 → 世界 y≈**2～3**） |
| `VillageDepthY_Min/Max` | 场景内 **未搜到** | 用 Prefab 默认 depth 边界 |

**对比**：

- 1 号屋 Exit **y=-6.1** 落在村街主 Walk 带 → 拖点 ≈ Play 落点。  
- 45 号屋 Exit **y=7.41** 高于 Walk 多边形在该 X 的合法纵深 → **WalkArea 校正** 可能吸到 **~2～3** 或地面带 → **「Y 不受 ExitFrom 控制」**。

### 侦探须证伪的假说

| ID | 假说 | 怎么证伪 |
|----|------|----------|
| **H1** | EnterPos 未绑 / lastScene 未命中 | YAML + `[VillageKenMuNiDebug]` / 落点 fallback |
| **H2** | **`VillageWalkArea` 覆盖 Y**（主嫌疑） | Play 前后打 `transform.position`；对比 ExitFrom Y vs 校正后 Y |
| **H3** | `depthYMin/Max` Clamp | 查 `TownPlayerLocomotion` 默认界与场景标尺 |
| **H4** | `SetPos` 只写 XY 但 **Z 保留室内值** | 室内 Z≠0 时排序/视觉错位（次要） |
| **H5** | `LoadingSceneEndHandle` **二次** `RefreshVillageExploration` 再次校正 | 时间线：SetPos 后第几帧被改 |
| **H6** | 用户把 **美术楼梯高度** 当成 **纵深 Y** | 对拍 HomeScene1；读 2.5D 文档 |
| **H7** | `DepthZone` / 障碍挤出改 Y | 树屋区 `DepthZone&Colliders` |
| **H8** | 读档 `archiveStart` 用存档 pos 顶掉 EnterPos | 新游戏 vs 读档对比 |

### 须比较的方案

| 方案 | 做法 | 适用 |
|------|------|------|
| **A（推荐候选）** | 将 `ExitFrom_HomeScene45` **Y 改到 WalkArea 内**该 X 的合法纵深（参考 **(-4.30, -2.33)** 或 Gizmos 贴 `VillageWalkArea` 边） | 最小改动；对齐 1 号屋 Δy |
| B | **扩展 `VillageWalkArea` 多边形** 覆盖树屋门口平台纵深 | 要站在高平台走时必须 |
| C | 树屋单独 `DepthZone` + 平台 Walk 子多边形 | 美术要多层纵深 |
| D | 换场后首帧跳过 WalkArea 校正（改 C#） | 回归面大，最后手段 |
| E | 只改 X 不改 Y（当前误操作） | ❌ 不能解决纵深 |

### 严禁

- 未查 `TownPlayerLocomotion` 就断定「EnterPos 坏了」  
- 把 **室内** `EnterFrom_Village` 与 **村侧** `ExitFrom_HomeScene45` 混为一谈  
- 用 **屏幕竖直高度** 理解村里 Transform.Y  
- 建议长期禁用 WalkArea 校正

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_回村门口落点_架构溯源报告.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告_v3.md
@Assets/Doc/执行文档/0819/Village_KenMuNi1_树屋下边围栏穿模卡住_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Prefabs/Entity/Player/Player.prefab

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止改场景/代码。只读 + 写溯源报告；须含 **落点链路图** 与 **Play 坐标三步对比表**。

---

## 背景

`ExitFrom_HomeScene45` 已建且 EnterPos 已绑，但用户拖 Y 后出村落点纵深仍不对。要查明：是 EnterPos 没生效，还是村庄 2.5D 后处理覆盖了 Y。

---

## 侦探任务清单

### A. EnterPos 与 lastScene（排除 H1）

| 检查项 | 现网 |
|--------|------|
| `KenMuNi1.EnterPosConfig` `Village_HomeScene45` → pos | |
| 是否指向 `ExitFrom_HomeScene45` Transform | |
| `LastSceneName` 出屋时实际值 | |
| `archiveStart` 是否绕过 EnterPos | |

### B. 样板对拍 HomeScene1

| 节点 | 坐标 | 与 `VillageWalkArea` 关系 |
|------|------|---------------------------|
| `House_Npc1` | | |
| `ExitFrom_HomeScene1` | | |
| 门→Exit Δx/Δy | | |
| Play 出 1 号屋后玩家 (x,y) | | |

### C. HomeScene45 / Npc45 区（本期）

| 节点 | 坐标 | 与 WalkArea 关系 |
|------|------|------------------|
| `House_Npc45` | | |
| **`ExitFrom_HomeScene45`（现网）** | | |
| 按 1 号屋 Δy 推算建议 Y | | |
| `VillageWalkArea` 在 x≈-4.3 处合法 Y 范围 | | 多边形世界坐标 |
| 树屋 `DepthZone` 范围 | | |

### D. 落点链路逐步（核心）

1. `SetPlayerPos` 后瞬间玩家 (x,y,z)  
2. `ApplyVillageMode` 后  
3. `ApplyVillageWalkPolygonPostCorrection` 后（推断：对比 ExitFrom Y）  
4. `LoadingSceneEndHandle` 后  
5. 首帧 `FixedUpdate` 后  

（侦探可写「建议施工员加 4 行临时 Debug.Log」，本阶段不改代码则标为待验）

### E. 根因裁定

主因 **H?**（WalkArea 覆盖 / EnterPos 未命中 / 语义误解 / 其它）  
说明：**为何拖 ExitFrom.Y 看起来无效**

### F. 方案对比 + 推荐施工

- 优先场景：挪 `ExitFrom_HomeScene45` 到 WalkArea 合法纵深 + 可选扩 polygon  
- 目标落点：与 `House_Npc45` 门外可站立、可按 E 再进屋  
- **给出建议世界坐标 (x,y)**（勿只写「微调」）

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 45 屋内 RightDoor 出村 | 落在 Npc45 门外 |
| 2 | 玩家 (x,y) 与 `ExitFrom_HomeScene45` | **Δy < 0.15**（或报告写明 WalkArea 故意偏移量） |
| 3 | 拖 ExitFrom **沿 WalkArea 合法方向** 改 Y | Play 落点 **跟随** |
| 4 | 再按 E 进屋 | 不闪回、不卡门 |
| 5 | 对比出 1 号屋 | 体感一致 |

### H. 开放问题

`OPEN_QUESTIONS.md`「ExitFrom_HomeScene45 落点纵深 · 2026-08-22」

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_ExitFrom落点Y轴_架构溯源报告.md`

结构：① 结论 ② 为何拖 Y 无效 ③ 用户验收 ④ 链路图 + 坐标表 + 施工步骤

MASTER 四段式口头汇报。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_ExitFrom落点Y轴_架构溯源报告.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。按报告修正出村后落点纵深（Y）。

必须遵守：
- 先确认 EnterPos 仍绑 `ExitFrom_HomeScene45`；
- 将 ExitFrom 摆到 **VillageWalkArea 多边形内**、Npc45 门外（报告给定坐标）；
- 若树屋平台不在 WalkArea 内：按报告扩 polygon 或加 DepthZone，**禁止**只抬 Y 超出 WalkArea；
- 对齐 HomeScene1 门↔Exit 偏移惯例（Δy≈-8）除非报告另有裁定；
- Play 打印 SetPos 后与 WalkArea 校正后坐标验收；
- 不改 TownPlayerLocomotion（除非报告明确要求）。

提交说明：ExitFrom 改前改后坐标、WalkArea 是否改动、出村落点与再进屋验收。
```
