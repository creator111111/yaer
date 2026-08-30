# Cursor Agent Prompt · Quest_003 老农打水：进度/存档不同步、读档无法重做 — 架构侦探

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **现象（用户白话）**：打水任务进度**好像不跟存档走**；**读档后没法重新做**  
> **范围**：`Quest_003` · `Npc_Farmer` / `FarmerQuestStoryTrigger` · `VillageWellLogic` · 空/满桶背包 · `PlayerQuestData` 存档  
> **关联施工**：`施工说明/0830/Village_老农打水_空满桶道具与井交互及接任务_施工说明.md`  
> **本阶段**：只读；禁止改代码 / Prefab / 存档 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户现象拆解（须分别证伪）

| # | 可能含义 | 预扫假说 |
|---|----------|----------|
| A | 打了几桶水，**存档/读档后桶数或任务状态对不上** | 井换桶改了 `PlayerBagData` 但 **未 `SavePlayerBag`**；进度只在内存 |
| B | 任务 UI / `GetQuestProgress` 一直 **0/4**，和背包满桶数不一致 | CollectItem **真进度在背包**；`questProgress` 接取后写 0，**井成功不递增**；杀怪线才推 Progress |
| C | **交完任务后**想读档「再做一遍」仍进不了 Offer | `TurnedIn` 后 Trigger **永不回 Offer**（设计）；须读**接取前**档，或开发重置 |
| D | 读的是接取前档，但仍像已接/已交 | 读档未还原 `PlayerQuestData` / 读错槽 / 读档后内存未 Reload |
| E | 拒过/帮过一次，对话状态机卡死 | Choice/Accept 与 Trigger 切图不同步；`Already accepted` |

报告 ① 必须钉死：**主因是哪一条（可并列）**，勿含糊「存档有问题」。

### 现网架构假说（进度真源）

```
接任务（帮）
  → QuestAcceptAction → questStates[Quest_003]=InProgress
  → questProgress[Quest_003]=0
  → SaveQuestProgress()          // 任务状态落盘
  → AddMainItem(Empty×4)+Tips    // 背包：须核实是否 SavePlayerBag

点井成功（VillageWellLogic）
  → TryRemove Empty + Add Full + Tips
  → ❌ 预扫无 SavePlayerBag / 无 questProgress++
  → 可交判定：CanTurnInCollectQuest = 背包 Full≥4（不看 questProgress）

交付
  → TryTurnInCollectQuest → 扣满×4 → TurnedIn
  → SavePlayerBag + SaveQuestProgress

再谈老农（FarmerQuestStoryTrigger）
  → null → Offer
  → InProgress + 满不足 → 进行中
  → InProgress + 满≥4 → 完成结算
  → TurnedIn → 进行中短句（❌ 不回 Offer = 「做不了第二遍」）
```

| 数据 | 存哪 | 谁更新 | 谁保存 |
|------|------|--------|--------|
| 任务状态 | `PlayerQuestData.questStates` | Accept / TurnIn | `SaveQuestProgress` |
| `questProgress` | 同存档 | **杀怪**累加；Collect **井不写** | 同上 |
| 空/满桶数 | `PlayerBagData` | 接任务发空、井换桶、交付扣满 | **？井是否落盘** |

生活类比：任务本只写「已接单/已结单」；打水进度写在**书包里的桶**上。若书包没存进存档夹，读档就像时间倒流但桶对不上；若任务 UI 只读任务本上的「0/4」计数器，会永远显示没进度。

### 与 Quest_002 / 杀怪线对照

| 线 | 进度真源 | Complete？ |
|----|----------|------------|
| KillMonster | `questProgress++` → Complete | 有 |
| CollectItem（藤蔓果/打水） | **交时查背包**；进行中靠背包 | **无 Complete** |
| UI 若统一读 `GetQuestProgress` | Collect 会 **假 0/4** | 侦探必须扫任务面板 |

### 「读档重新做」产品口径（侦探拍板写清）

| 期望 | 是否合理 | 做法倾向 |
|------|----------|----------|
| 读**接取前**存档，再帮一次 | ✅ 应支持 | 修读档/落盘即可 |
| 读**进行中**存档，桶数恢复到存档时 | ✅ 应支持 | 井后须落盘背包；读档 Reload 齐 |
| 读**已交完**同一档，再接一次 Offer | ❌ 默认不应；除非开发重置 | 开放：Debug 清 `Quest_003` / 新档 |
| 不存档、杀进程再进，进度还在 | 看自动存档策略 | 扫何时 AutoSave |

### 须排查清单（侦探逐项打勾）

1. `VillageWellLogic` 成功换桶后有无 `SavePlayerBag` / Archive Dirty？  
2. 接任务发 4 空桶的 Action 后有无存背包？  
3. `GetQuestProgress(Quest_003)` 谁在用？任务面板显示什么？  
4. 读档流程是否 Reload `PlayerQuestData` + `PlayerBagData`？  
5. `FarmerQuestStoryTrigger` TurnedIn 分支是否导致「无法重做」误判？  
6. `AcceptQuest` Already accepted 与读档状态是否打架？  
7. 是否存在**仅内存**的打水计数（非背包）？

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉死进度真源与存档缺口 | ❌ 重做整套任务系统 |
| ✅ 区分「UI 假进度」vs「真没落盘」vs「交完不能重接」 | ❌ 无产品批准就做「交完可无限重接」 |
| ✅ 最小修复方案（井后存包 / Progress 读背包 / 读档验收） | ❌ 改 Quest_001/002 行为（可对照） |

### 严禁（本阶段）

- 改代码 / Prefab / 存档文件  
- 未区分 TurnedIn 与读档失败就改 Trigger  
- 把 CollectItem 强行改成杀怪式 Progress++ 而不评估交时查包样板  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `VillageWellLogic.cs` | 换桶是否存档 |
| `FarmerQuestStoryTrigger.cs` | 状态切图 / TurnedIn |
| `QuestManager.cs` | Accept/Progress/TurnIn/Save* |
| `PlayerQuestData.cs` / `PlayerBagData.cs` | 序列化字段 |
| Archive 读档入口 | Reload 是否齐 |
| 任务 UI（若有） | 是否误读 questProgress |
| 施工说明 0830 打水 | 现网意图 |
| Quest_002 Collect 报告 | 交时查包先例 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/施工说明/0830/Village_老农打水_空满桶道具与井交互及接任务_施工说明.md
@Assets/Doc/执行文档/0830/Village_老农打水_空满桶道具与井交互及接任务_架构溯源报告.md
@Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md
@Assets/Doc/执行文档/0820/Quest_002_接取后仍播Offer应切循环对白_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageWellLogic.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/FarmerQuestStoryTrigger.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/PlayerQuestData.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Player/PlayerBagData.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/QuestAcceptAction.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/QuestTurnInAction.cs
@Assets/GameRes/Config/QuestConfig/QuestConfig.json

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、存档。只读扫描 + 写「打水任务进度与存档」溯源报告。

---

## 背景（策划白话）

1. 打水任务进度感觉**不跟存档走**。  
2. **读档之后没法重新做**（须写清是「交完再做」还是「读旧档重来」失败）。  
3. 查清：进度存在哪、何时写入存档、读档还原什么；最小怎么修。

---

## 侦探任务清单

### A. 复现路径分类
列出用户可能操作的 3～4 条路径，每条标「期望 vs 现网假说」。

### B. 进度真源
钉死 CollectItem 进行中以**背包满桶数**为准，还是 `questProgress`；井是否更新后者；UI 读谁。

### C. 落盘审计
Accept / 井换桶 / TurnIn / 手动存读：各自调用了哪些 Save；缺口表。

### D. 读档审计
读档后 `questStates[Quest_003]` 与空/满桶数是否同时还原；有无只还原其一。

### E. 「无法重做」
TurnedIn 切图策略；产品是否要开发重置；与「读接取前档应能重做」分开写。

### F. 最小修复方案（本阶段不执行）

| # | 方案 | 何时用 |
|---|------|--------|
| 1 | 井成功后 `SavePlayerBag`（+必要时与 Quest 同存） | 桶进度丢 |
| 2 | 任务 UI Collect 读背包数量，勿只读 questProgress | UI 假 0/4 |
| 3 | 接任务发桶后确保存包 | 读档空桶丢失 |
| 4 | 文档/提示：交完须新档或 Debug 重置才能再 Offer | 「无法重做」实为设计 |
| 5 | （可选）Debug 清 Quest_003+桶 | 开发验收 |

### G. 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 接任务 → 打 2 桶 → **存档** → 读档 | 仍 InProgress；空/满桶数与存档时一致 |
| 2 | 接任务前存档 → 接任务打水 → **读接取前档** | 未接；可再 Offer；桶按旧档 |
| 3 | 交完任务存档 → 读该档 | TurnedIn；**不**再出帮/不帮（除非产品改口） |
| 4 | 任务面板（若有） | 进行中显示满桶进度与背包一致 |
| 5 | Console | 无 Already accepted 误报；Save 日志可对账 |

### H. 开放问题
- 每次点井是否要自动存（手感 vs 存档频率）？  
- Collect 任务面板公式是否全局改为读背包？  
- 交完是否允许日常循环打水（无任务）？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md`

MASTER 四段式：  
① 结论（主因一句话：落盘缺口 / UI 假进度 / 交完不能重接）  
② 原因（通俗）  
③ 用户检查清单（怎么存读验）  
④ 给程序：数据流表 + Save 缺口 + 最小 diff + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageWellLogic.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs

你现在是【施工员】。按报告修复打水任务进度与存档不同步 / 读档异常。

必须遵守：
- CollectItem 真进度以报告拍板为准（多半是背包）；勿盲目抄杀怪 Progress++ 破坏交时查包；
- 井换桶若缺落盘则补 Save；读档验收三条路径都写进提交说明；
- 「交完再 Offer」仅当报告明确要求；默认不要改成无限重接；
- 代码含详细注释；重要取舍写清原因。

提交说明：改了哪些 Save/UI、如何用存读档验收、未做项。
```
