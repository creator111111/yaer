# Cursor Agent Prompt · 民居任务：读档后接任务对话测不了

> **角色**：【架构侦探】只读核对任务是否跟存档走；对照老农打水已改过的读档规则；报告通过后再施工  
> **日期**：2026-09-20  
> **现象（用户）**：任务好像还绑在存档上。老农打水改过：读档应回到当时的状态，还能再接。现在 `Village_HomeScene23` 里的任务，读档之后**接任务那段对话出不来**，没法反复测  
> **产品期望（钉死，与老农 P2 相同）**：  
> 1. 读的是**接任务之前**的档 → 任务回到未接，再点 NPC 能播接任务长对白  
> 2. 读的是**已经接了**的档 → 就保持已接，不要又弹出接任务选项  
> 3. 不要把所有任务从存档里解开。正式游玩仍以存档为准  
> **不是**：交完的档还能无限再接；改藤蔓果数量、报酬、台词；改老农「当日不可再接」  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_HomeScene23_读档后无法再测接任务对话_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「是读档没还原，还是根本没有接之前的档可读，还是缺一个和老农一样的调试清任务」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户在问的两件事

| 问法 | 不要答成 |
|------|----------|
| 任务还和存档绑在一起吗 | 不要只答「是」。要写出：接任务时有没有立刻写入存档；读档时内存里的任务本会不会换成这份档里的 |
| 读档后为什么测不了接任务对话 | 不要改成「任务不要存档」。测不了，是因为读档后状态仍是已接，Trigger 不肯播 Offer |

### 老农已经定过的口径（对照，勿当 HomeScene23 的现网）

0830 报告 `Village_老农打水_任务进度与存档不同步`：

- 任务状态在 `PlayerQuestData`，`AcceptQuest` 会马上 `SaveQuestProgress`。
- **读接取前的档** → 应是未接，再出「帮 / 不帮」。这条叫 P2，是要通的。
- **读已交完的档** → 不再给接。这是设计，不是 bug。
- 后来还有编辑器菜单 `Editor/Quest/ResetQuest_003 老农打水`，只清 `Quest_003`，方便 Play 里重测。**没有**看到 Quest_002 的同样菜单。

民居这场要回答：Quest_002 读「接之前的档」时，P2 通不通。若用户手头的档在接的瞬间已经被写成已接，也要写明，避免让人去读一份已经坏掉的档。

### 民居这场怎么选对话（预扫）

`Npc23QuestStoryTrigger`（椅子 NPC）看 `Quest_002`：

| 状态 | 播哪段 |
|------|--------|
| 没有这条任务（未接） | `Village_QuestOffer_NPC23` ← 用户要测的接任务对话 |
| 进行中、果不够 | `Village_QuestThanks_NPC23` |
| 进行中、果够 5 | `Village_QuestTurnIn_NPC23` |
| 已交付 | 仍是感谢，**不回** Offer |

所以只要读档后 `GetQuestState("Quest_002")` 不是空，接任务对话就一定不出现。侦探要查的是：**为什么读档后它不是空**。

### 嫌疑

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A** | 一按「好呀」就 `SaveQuestProgress`，用户以为的旧档其实已经是已接 | 读 `QuestAcceptAction` / `AcceptQuest`：接取是否立刻落盘、落的是当前槽还是另一槽 |
| **B** | 读档没有把内存里的 `PlayerQuestData` 换成档里的；老任务还留在内存 | 跟 `LoadArchive` / `GetData<PlayerQuestData>`：读档后字典是否清空再填 |
| **C** | 档是对的（未接），但 Trigger 没回到 Offer（空状态被当成已接） | 读档后日志里的 state；`ResolveStoryPrefabName` 只有 `state == null` 才回 Offer |
| **D** | 存档是对的，但没有方便的重测入口；老农有 Reset 菜单，Quest_002 没有 | 看 `QuestResetDebugMenu` 是否只有 Quest_003 |

### 禁止

- 本阶段不改代码、不改 Prefab、不改存档、不改 CSV。  
- 不要建议「任务不写存档」来方便测试。那会让正式读档也对不上。  
- 不要改老农 TurnedIn / 当日不可再接。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/8月/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md
@Assets/Doc/施工说明/0830/Village_老农打水_任务进度与存档不同步_施工说明.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/Npc23QuestStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/FarmerQuestStoryTrigger.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/PlayerQuestData.cs
@Assets/Scripts/Debug/Editor/QuestResetDebugMenu.cs
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、存档、CSV。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_HomeScene23_读档后无法再测接任务对话_架构溯源报告.md

---

## 背景（策划白话）

开发者要反复测民居室内（Village_HomeScene23）椅子 NPC 的接任务对话。
读档之后，这段接任务对话不再出现，测起来很麻烦。

他问：任务现在是不是还和存档绑在一起？
老农打水以前改过同一类问题：读档应该回到那份档当时的状态；如果那份档还没接任务，就应该还能再接、再听到接任务对话。

这次主范围是民居 Quest_002，不是把全游戏任务改成不存档。

要的效果：
- 读「接之前」的档，再点椅子 NPC，播 Village_QuestOffer_NPC23（接任务长对白，含接不接的选项）
- 读「已经接了」的档，不要又出现接任务选项
- 读「已经交完」的档，不要变回接任务（这条与老农一致，保持）
- 若现网没有一份干净的「接之前」档能用，要写明原因，并给出和老农 Reset 菜单同类的重测办法，而不是让任务脱离存档

---

## 必读 / 优先扫描

历史结论只作对照，以现网代码为准：

- `Assets/Doc/执行文档/8月/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md`（P2：读接取前档应再出 Offer）
- `QuestManager.AcceptQuest` / `SaveQuestProgress` / `ResetQuest`
- `Npc23QuestStoryTrigger` 与 `FarmerQuestStoryTrigger` 对「未接」的判断是否一样（是不是都认 null）

### A. 先回答「还绑不绑存档」

用调用链写清，不要一句话带过：

1. Quest_002 点「好呀」之后，任务状态写进内存还是立刻写入当前存档槽
2. 读档时，`PlayerQuestData` 会不会被这份档整个换掉（含把已接从内存里清掉）
3. 背包里的藤蔓果跟不跟这次读档走。接任务对话出不来如果只因为任务状态，果没还原要单独写，不要混成一个原因

### B. 为什么读档后 Offer 不出现

对着 `Npc23QuestStoryTrigger` 列一张表：读档结束后 `GetQuestState("Quest_002")` 会是 null / InProgress / TurnedIn 的哪一种，以及因此播哪份 Prefab。

必须分开三种用户操作，每种给现网结论（通 / 不通 + 原因）：

| 操作 | 期望 |
|------|------|
| 接之前先存一档，接了任务，再读那一档 | 未接，再出接任务对话 |
| 接了之后存档，再读这一档 | 已接，出感谢或交付，不出接任务 |
| 没另存档，只是读「继续游戏」进来的当前档 | 写明这档在接取时是否已经被改成已接，所以读它不可能回到未接 |

### C. 和老农差在哪

- Quest_003 的 P2 现网是否仍成立（不要重做老农，只说明民居是不是同一条读档缺口）
- `Editor/Quest/ResetQuest_003 老农打水` 为什么帮得了重测；Quest_002 有没有同等入口
- 若推荐加调试菜单，写明它只在 Play 模式清 Quest_002 的任务状态，不改背包、不改老农、不改正式读档规则

### D. 推荐一种最小改法

只推一种，并满足：

1. 读接之前的档，民居能再播接任务对话
2. 读已接档、已交档，行为与现在的产品一致
3. 不把 PlayerQuestData 从存档里拿掉
4. 老农 Trigger 和 ResetQuest_003 菜单行为不变，除非报告证明它们和 Quest_002 共用了同一处读档 bug，必须一起修

另外两种各用一句话否决。例如「任务不落盘」「交完也能再接」。

---

## 报告结构（固定）

① 结论一句话（读档后仍是已接，还是档本身已经是已接；要改读档还是加调试清任务）  
② 原因（大白话 + 接任务落盘、读档换内存、Trigger 选 Prefab 三条链）  
③ 用户需要做什么（怎么存一档「接之前」、怎么读、点哪个 NPC 才算测到）  
④ 给施工员的补充：改哪些文件、不要改哪些、推荐方案、否决方案

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_HomeScene23_读档后无法再测接任务对话_架构溯源报告.md
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/Npc23QuestStoryTrigger.cs
@Assets/Scripts/Debug/Editor/QuestResetDebugMenu.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告的推荐方案做最小改。
不要让任务脱离存档。不要改老农当日不可再接。不要改藤蔓果台词和报酬。

目标：
- 读「接 Quest_002 之前」的档，再点 HomeScene23 椅子 NPC，能播接任务对话 Village_QuestOffer_NPC23
- 读已经接了或已经交了的档，不要回到接任务选项
- 若报告要求加调试清任务，只清 Quest_002，且只在 Play 模式，不改背包规则，除非报告写明背包也必须一起清

限制：
- 禁止在 Update 里查任务状态
- 注释说明为什么不能把 SaveQuestProgress 删掉
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_HomeScene23_读档后无法再测接任务对话_施工说明.md`
- 没有新架构就不要新造技术文档

完成后用大白话给验收清单：先存接之前的档，接一次，再读那档，确认接任务对话又出现。
```
