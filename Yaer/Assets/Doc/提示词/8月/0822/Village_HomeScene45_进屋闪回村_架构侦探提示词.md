# Cursor Agent Prompt · Village_HomeScene45：进屋立刻闪回村 Bug

> **角色**：先【架构侦探】只读溯源 + 复现路径分析，报告拍板后【施工员】修复  
> **日期**：2026-08-22  
> **现象场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
> **对照样板**：`Assets/GameRes/Scenes/Village_HomeScene1.unity`（进屋稳定）、`Village_HomeScene2.unity`（`EnterFrom_Village` 进屋落点）  
> **产品描述（开发者）**：从村 `House_Npc45` 进屋后，**有概率立刻闪现回村**；怀疑出生点与出门点太近，但体感不像单纯距离问题。  
> **本阶段侦探**：只读、不改场景 / 代码；须给出 **Console 过滤关键字** 与 **Scene 视图 Gizmos 验收法**

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### Bug 现象（白话）

```
村 House_Npc45 按 E 进屋
  → 加载 Village_HomeScene45
  → （有时）黑幕一切 / 几乎无停留
  → 又回到 Village_KenMuNi1
```

关键词：**有概率**（非 100%）→ 须查物理重叠边界、初始化时序、落点 fallback，不能只量直线距离。

### 最可疑机制（须证伪/证实）

**RightDoor `TriggerWhenMoveIn=1` + 进屋落点过近 → 玩家生成时脚碰撞体与出门 Trigger 重叠 → `OnTriggerEnter2D` 立刻 `LoadScene(Village_KenMuNi1)`。**

代码链：

```
SetPlayerPos(RightBorn)
  → PlayerFoot 进入 RightDoor BoxCollider2D
  → InteractiveComponent.onEnterInteractiveEvent
  → SceneChangeDoor.EnterDoor（TriggerWhenMoveIn=1）
  → LoadScene(Village_KenMuNi1)
```

`SceneChangeDoor.cs` **无**进屋后冷却/免疫帧；`isEnter` 仅防同场景重复，**不防**「刚进房就被右门踢出去」。

### 工程先例（同类 Bug）

| 文档 | 现象 | 根因 |
|------|------|------|
| `0608/Village_OutSide_Village_KenMuNi1_换场落点错误弹回村外` | 进村后闪回 / 异常换场 | 落点绑错 `RightBorn`，与 `TriggerWhenMoveIn` 门区叠加 |
| `0608/Village_HomeScene2_HouseDoor…` | 2 号屋稳定进屋 | 用 **`EnterFrom_Village`**（x≈-24.12），**远离**右侧出门 Trigger |

### 磁盘预扫：45 vs 1 号屋布局差异（侦探须 Gizmos 复核）

| 项 | HomeScene1（稳定） | HomeScene45（现网） | 风险 |
|----|-------------------|---------------------|------|
| 进屋 EnterPos | `RightBorn` **(-6.91, -3.65)** | `RightBorn` **(-5.4, -3.65)** | 45 更靠右（近右门） |
| `MapRight` 位置 | **(28.8, 0)** | **(18.36, 0)** | 45 整扇右墙左移 **~10.4** 单位 |
| RightDoor 世界 X（约） | 28.8 + (-19.55) ≈ **9.25** | 18.36 + (-19.55) ≈ **-1.19** | 出门 Trigger 大幅靠近出生区 |
| RightDoor `TriggerWhenMoveIn` | **1** | **1** | 走进即换场 |
| 进屋专用 `EnterFrom_Village` | **无**（1 号屋靠远 Born 够用） | **无** | 2 号屋有专用点，45 没有 |
| `LeftBorn` | **(-24.12, -3.65)** 远离右门 | **(-5.61, -3.65)** ≈ RightBorn | 疑似改名/改布局残留，**未**作进屋点 |
| LeftDoor SceneChangeDoor | 禁用 | **禁用** ✅ | 左门不是主因 |

**预扫推论**：不像「出生点与出门点重合」，而是 **RightBorn 与 RightDoor Trigger 水平间距过小**（布局改窄 + 无 `EnterFrom_Village` 缓冲）。脚碰撞体 + `OverlapPadding=0.2` 可能偶发压线 → **概率触发**。

### 须排除的其它假说

| # | 假说 | 侦探怎么证伪 |
|---|------|--------------|
| H1 | `LastSceneName` 不匹配 → 落到 `DefaultBorn` 再踩门 | 查 `[VillageHomeScene45Debug] lastScene=` 与 EnterPos 是否命中 |
| H2 | 村侧 `House_Npc45` 双击 / 连触两次 LoadScene | 查进村 Console 是否连续两条 `[SceneChangeDoor]` |
| H3 | 左门误启用 | YAML：`LeftDoor.SceneChangeDoor.m_Enabled` |
| H4 | `NextSceneName` 写错导致循环 | RightDoor 应为 `Village_KenMuNi1` |
| H5 | 黑幕未结束玩家已位移进 Trigger | 查 `LoadSceneComponentGSM` 与 `SetPlayerPos` 先后顺序 |
| H6 | 村侧 `ExitFrom` 落点与门叠加（出村再进才闪） | 本期是 **进屋** 闪回，优先室内右门；村侧作次要 |

### 侦探须比较的方案

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A（推荐候选）** | 新建 **`EnterFrom_Village`**（或 `EnterFrom_KenMuNi1`），摆室内左侧门口内侧（参考 HomeScene2 **x≈-24.12**）；`EnterPosConfig` `Village_KenMuNi1` → 改绑该点；**保留** `RightBorn` 给 Map 元数据 | 与 2 号屋一致；不动出门 UX | 须 Play 验进屋站位 |
| B | 把 `MapRight` 挪回 HomeScene1 的 **28.8** | 恢复模板间距 | 牵动墙壁/美术，面大 |
| C | 缩小 / 外移 RightDoor `BoxCollider2D` | 快 | 可能走不进出门区 |
| D | `SceneChangeDoor` 加进屋后 N 帧免疫 | 通用 | 改 C#，回归面大 |
| E | RightDoor 改 `TriggerWhenMoveIn=0` 仅按 E 出 | 彻底不误触 | 与 1/23 右门「走进即走」不一致 |

### 生活类比

刚跨进张三家门槛，人还站在门垫上，后门感应器就把你弹回村口——不是门垫和后门重合，是**门垫离后门感应区太近**，稍微挪一步就触发。

### 严禁

- 未看 Console `[SceneChangeDoor]` 就断定「不是门的问题」  
- 只调村侧 `ExitFrom_HomeScene45`（那是**出村落点**问题，不解决**进屋闪回**）  
- 把 `LeftBorn` 当进屋点却不改 EnterPos（LeftBorn 在 45 已与 RightBorn 重叠，无意义）  
- 禁用 RightDoor 出门功能来「修」进屋 Bug

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/6月/0608/Village_OutSide_Village_KenMuNi1_换场落点错误弹回村外_架构溯源与修复执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_HomeScene2_HouseDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_RightDoor回村_架构溯源报告.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_回村门口落点_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Interactive/InteractiveComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene45SceneManager.cs
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Prefabs/Entity/Player/Player.prefab

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止修改场景、代码。只读 + 写溯源报告；须写明 **Play 复现步骤** 与 **Console 过滤词**。

---

## 背景

45 号屋从村进屋后偶发立刻回村。开发者怀疑出生点与出门点太近。要对拍 HomeScene1（稳）与 HomeScene2（EnterFrom_Village），定位是落点/Trigger 叠加还是别的链路。

---

## 侦探任务清单

### A. 复现与日志（必填）

1. Play：村 `House_Npc45` → 进屋，重复 ≥10 次，记录闪回概率。  
2. Console 过滤：`SceneChangeDoor`、`VillageHomeScene45Debug`、`LoadScene`。  
3. 闪回瞬间是否出现 **`[SceneChangeDoor] Enter name=RightDoor`** 且 `activeScene=Village_HomeScene45`？  
4. 若无 RightDoor 日志，列出其它换场来源（LeftDoor、House_Npc45、存档加载等）。

### B. 落点与 Trigger 几何（Gizmos）

| 节点 | 世界坐标（约） | 与 RightDoor Trigger AABB 关系 |
|------|----------------|--------------------------------|
| `RightBorn`（进屋 EnterPos） | | |
| `LeftBorn` | | |
| `DefaultBornPos` | | |
| `Map/MapRight/RightDoor` BoxCollider2D bounds | | |
| HomeScene1 同表对比 | | |

- 计算：RightBorn 中心到 RightDoor Trigger **最近边** 水平距离。  
- 叠加 PlayerFoot collider 半宽（`Player.prefab`）判断是否可能 Enter 即重叠。  
- 说明为何「有概率」而非必现（物理步、输入、OverlapPadding）。

### C. EnterPos 与 fallback

| 检查项 | 现网 |
|--------|------|
| `EnterPosConfig` `Village_KenMuNi1` → pos | |
| `LastSceneName` 进屋时实际值 | |
| 未命中时是否落到 `DefaultBornPos` | |
| `nowSceneName` 与字符串是否一致 | |

### D. 门配置对拍（Left / Right）

| 门 | Enabled | NextSceneName | TriggerWhenMoveIn | Collider Size/Offset |
|----|---------|---------------|-------------------|----------------------|
| LeftDoor | | | | |
| RightDoor | | | | |
| HomeScene1 RightDoor | | | | |

### E. 样板：HomeScene2 的 `EnterFrom_Village`

- 坐标、与 RightDoor 距离  
- 45 号屋是否缺同名节点  
- 方案 A 是否可直接复制 x≈-24.12 模式

### F. 根因裁定（四选一为主 + 次要因素）

- **R1** 进屋落点与 RightDoor Trigger 重叠/过近（主嫌疑）  
- **R2** LastSceneName / EnterPos 未命中  
- **R3** 初始化时序（玩家晚于门注册仍触发 Enter）  
- **R4** 其它（须 YAML 证据）

### G. 推荐施工（最小改动）

按优先级写步骤；预期主推 **方案 A：EnterFrom_Village + 改 EnterPos**，RightDoor 保持 `TriggerWhenMoveIn=1`。

### H. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村进屋 20 次 | **0 次**进屋立刻回村 |
| 2 | 进屋后站位 | 在 **`EnterFrom_Village` 内侧**，不在 RightDoor Trigger 内 |
| 3 | 主动走向 RightDoor | 仍能正常回村 |
| 4 | Console | 进屋后 **无** 意外 `RightDoor` EnterDoor（未走向右门前） |
| 5 | 与 HomeScene1 对比 | 进屋体感一致（先进屋再出门） |

### I. 开放问题

追加 `OPEN_QUESTIONS.md`「Village_HomeScene45 进屋闪回村 · 2026-08-22」（如是否全局加 SceneChangeDoor 免疫帧）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告.md`

报告结构：① 结论一句话（根因 R?） ② 为何有概率 ③ 用户验收 ④ Gizmos 截图说明 + 方案对比 + 施工步骤

MASTER 四段式口头汇报。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/GameRes/Scenes/Village_HomeScene45.unity

你现在是【施工员】。按报告修复「进屋立刻闪回村」。

必须遵守：
- 优先场景侧：新建进屋落点（如 EnterFrom_Village）+ 改 EnterPos，使出生点远离 RightDoor Trigger；
- 参考 HomeScene2 的 EnterFrom_Village 坐标与 EnterPos 绑法；
- 保持 RightDoor 可正常走出回村（TriggerWhenMoveIn=1）；
- 不改 Village_HomeScene45SceneManager.cs（除非报告要求修 nowSceneName）；
- 若报告要求改 C# 免疫帧，须中文注释说明与场景方案取舍。

提交说明：根因、改了哪些 Transform/EnterPos、RightBorn 与 EnterFrom 分工、20 次进屋验收结果。
```
