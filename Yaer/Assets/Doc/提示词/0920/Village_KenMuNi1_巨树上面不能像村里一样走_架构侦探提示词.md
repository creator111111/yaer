# Cursor Agent Prompt · 巨树上面走不了，要能像村里一样走

> **角色**：【架构侦探】只读；先复核 0903「纵深尺子」修没修上，再查为什么大树上面仍不能像村里 2.5D 那样走  
> **日期**：2026-09-20  
> **场景**：`Village_KenMuNi1` 巨树 2 楼（`VillageWalkArea2`）  
> **入口**：村长家楼梯上楼（`ExitFrom_HomeSceneChief2f`）  
> **现象（用户）**：大树上面**还是没法走**。为什么不能像村子里面一样行走  
> **产品期望（钉死）**：人站在大树可走面上，左右、上下（纵深）都能走，手感对齐村里街道的 `Village2_5D`，不是焊在一个点上，也不是只能挤在一条细缝里  
> **不是**：改 1 楼街道走路；改相机双机切换；改宝箱；把 WalkArea2 点集当第一刀（0903 已禁止，除非本次证明尺子已对、形状才是新主因）  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_KenMuNi1_巨树上面不能像村里一样走_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「现网还是 DepthGap，还是尺子好了但仍被别的约束焊死」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 村子地面可以前后左右走。大树上面不行，人像卡住。  
> 要查现在到底哪一道尺子还在拦，修到和大树这块地板一样能走。不要先改绿线框形状。

### 0903 已下过的结论（对照，勿当现网）

报告：`执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md`  
施工：`施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md`  
复验：`执行文档/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查报告.md`

| 项 | 当时怎么说 |
|----|------------|
| 主因 | **不是** WalkArea2 画坏。纵深上限默认 **8**，2 楼落点 Y≈**41**。Clamp 把人往下压，ClosestPoint 又往 WalkArea2 吸，每帧撕扯 → 动不了 |
| 修法 | KenMuNi1 摆纵深标尺，Max 盖住 2 楼（约 ≥45）；上楼时 `SetVillageWalkAreaOverride(VillageWalkArea2)` + Teleport |
| 严禁 | 先改 WalkArea2 点集；关掉 ClosestPoint；用 1 楼 WalkArea 罩住 2 楼 |

用户现在说「还是没法走，要像村里一样」。两种都要查：

1. **0903 没在本机/现网生效**（Max 仍是 8，或 Override 没绑上）→ 仍是旧 bug，不要改形状。  
2. **尺子已经 ≥45，人仍不能像街上走** → 新主因：多边形太窄、障碍、模式不是 Village2_5D、或又被别的 Clamp 焊住。这时才允许讨论是不是该把树冠走面做成和街道同类的一块地板。

### 「像村里一样走」指什么

| 村里街道 | 大树上面本期要对齐的 |
|----------|----------------------|
| 模式 `Village2_5D` | 同一套，不要改成横版跳跃 |
| Y = 纵深，不是跳 | 树上 W/S 也是在枝干平面里走远近 |
| 脚锁在 `VillageWalkArea` 多边形里 | 2 楼锁在 `VillageWalkArea2`，且这片要够站、够走 |
| 纵深有 Min/Max 标尺 | 2 楼标尺必须盖住这片多边形的 Y，不能还是 8 |

### 嫌疑

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A** | `depthYMaxWorld` 仍约 8，和 WalkArea2（Y 大约 30～45）对拉 | 场景有没有纵深标尺；上楼日志有没有把 Max 抬到 ≥ 多边形上沿 |
| **B** | 没切到 WalkArea2，ClosestPoint 仍吸 1 楼或吸到区外 | `SetVillageWalkAreaOverride` 是否在上楼路径调用 |
| **C** | 尺子对了，但 WalkArea2 太细/太短，W/S 几乎没有可走距离，体感「不能像村里走」 | 量多边形宽高，和街道 WalkArea 比 |
| **D** | 障碍 / 空气墙把人焊在落点 | 落点附近 Obstacle、Collider |
| **E** | 根本不是 Village2_5D（重力、不能改 Y） | `PlayerLocomotionMode` |

### 禁止

- 本阶段不改代码、不改场景、不改 WalkArea 点集。  
- 不要把「改绿线框」写成默认第一方案。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md
@Assets/Doc/施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md
@Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查报告.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab、多边形。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_KenMuNi1_巨树上面不能像村里一样走_架构溯源报告.md

---

## 背景（策划白话）

大树上面现在还是没法走。
村里地面可以前后左右走（2.5D，上下是远近）。
大树上面要能一样走，不要卡在一个点上。

0903 说过主因是纵深上限还是 8，2 楼在 Y≈40，两套约束撕扯，并且禁止先改 WalkArea2 形状。
这次要先核实那个修复在现网还在不在。若已经在，再找新的卡住原因。
不要一上来改绿线框。

---

## 必读 / 优先扫描

### A. 0903 修复是否还在

在 `Village_KenMuNi1` 和上楼脚本里核对：

1. 有没有纵深标尺；2 楼路径会不会把 `depthYMaxWorld` 抬到 ≥ WalkArea2 上沿（不是仍为 8）
2. 上楼是否仍 `SetVillageWalkAreaOverride(VillageWalkArea2)` 并传到落点 `ExitFrom_HomeSceneChief2f`
3. 落点是否在多边形内（Overlap / ClosestPoint）
4. 若 Max 仍是 8 或 Override 没绑：主因仍是 0903，推荐把标尺/绑区补上，**不要改点集**

### B. 若尺子和绑区都对，为什么还是不像村里

对照街道 `VillageWalkArea` 与 `VillageWalkArea2`：

| 对比 | 街道 | 大树 2 楼 |
|------|------|-----------|
| 模式是不是 Village2_5D | | |
| 多边形大致宽、高（世界单位） | | |
| 纵深 Min/Max | | |
| 落点附近有没有障碍把人夹住 | | |
| W/S 是否会被 Clamp 或 ClosestPoint 立刻拉回 | | |

写清：玩家在 2 楼上按 A/D、按 W/S，各自被谁吃掉。

### C. 「像村里一样」的最小改法

只推一种，并满足：

1. 上楼后能离开落点，在树的可走面上左右、纵深都能走
2. 1 楼街道走路规则不变
3. 若主因仍是 Max=8 或没绑 WalkArea2，禁止改多边形
4. 只有证明标尺已盖住、人仍因为多边形太窄所以不像村里走，才允许建议改 WalkArea2，并写明改哪几条边、不要扩到 1 楼

另外两种各用一句话否决（例如「关掉 ClosestPoint」「用 1 楼 WalkArea 罩住整棵树」）。

---

## 报告结构（固定）

① 结论一句话（还是纵深对拉，还是新区太窄/被障碍焊住）  
② 原因（大白话 + 从上楼落到按键移动的调用链）  
③ 用户需要做什么（上楼后看脚的 Y、能否离开落点；Console 滤 Village2f / CLAMP_AT_YMAX）  
④ 给施工员的补充：改哪些文件、不要改 WalkArea2 除非报告写明、推荐方案、否决方案

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_KenMuNi1_巨树上面不能像村里一样走_架构溯源报告.md
@Assets/Doc/施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告推荐方案做最小改。
报告若写「仍是纵深上限 / 没绑 WalkArea2」，禁止改 VillageWalkArea2 点集。
报告若写「必须加宽走面」，只改报告点名的那几条边。

目标：
- 上楼到大树可走面后，左右和纵深都能走，手感对齐村里 2.5D
- 不要被吸回 1 楼，不要焊在落点
- 1 楼街道走路不变

限制：
- 禁止在 Update 里新写一套移动
- 禁止关掉 ClosestPoint、禁止用 1 楼 WalkArea 罩整棵树，除非报告明确要求并说明原因
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_KenMuNi1_巨树上面不能像村里一样走_施工说明.md`

完成后用大白话给验收清单：村长家楼梯上楼，离开落点按左右和上下各走一段，确认还在树上、没有被吸回地面。
```
