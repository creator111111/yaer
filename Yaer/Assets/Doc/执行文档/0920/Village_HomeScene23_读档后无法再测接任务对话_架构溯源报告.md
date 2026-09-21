# Village_HomeScene23 读档后无法再测接任务对话 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / Prefab / 场景 / 存档 / CSV）  
> Unity：2020.3.48f1  
> 对照：`Assets/Doc/执行文档/8月/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md`（P2）。以现网代码为准。

---

## ① 结论一句话

任务仍和存档绑在一起。点「好呀」的瞬间，**当前这一档**就被写成 Quest_002 已接；之后读的如果就是这一档（含「继续游戏」），椅子不会再播接任务长对白。读一份**接之前另存的档**现网就能再出 Offer，不必改读档。没有那份干净档时，只加一条和老农同类的 Play 模式清任务菜单。

---

## ② 原因

大白话：接任务不是只记在这一局内存里。选「好呀」马上写进正在玩的那个存档槽。读档会把内存整份换成那一档磁盘上的任务表。档里已经是「已接」，椅子就改播感谢或交付，不再给接不接的选项。藤蔓果是另一本账，不是接任务对话消失的原因。

### 链 1：点「好呀」立刻落盘

`Village_HomeScene23` / `NpcChair` → `Npc23QuestStoryTrigger`。未接（`GetQuestState` 为 null）才播 `Village_QuestOffer_NPC23`。

图上选项是「我有些忙」「好呀」。

- 「我有些忙」→「没关系我一会自己去吧」→ 结束。不接任务。
- 「好呀」→「太感谢了」→ `QuestAcceptAction`（`questId = Quest_002`）→ `QuestManager.AcceptQuest`。

`AcceptQuest` 在内存里写入：

- `PlayerQuestData.questStates["Quest_002"] = InProgress`（枚举值 2）
- `questProgress["Quest_002"] = 0`

紧接着 `SaveQuestProgress()` → `ArchiveComponentGM.SaveSpcData<PlayerQuestData>()`：把任务字典刷进当前槽的 `masterGameData`，再把**整份**主存档写进当前槽的虚拟文件。Console 应出现 `[Quest] Accept Quest_002`，以及「保存指定类型数据成功」且类型为 `PlayerQuestData`。

这一下只改**当前正在玩的槽**。以前「另存为」出去的别的槽，文件不会被这次接任务改写。

接任务**不改背包**。Quest_002 配置是收集 `TenWangFruit` × 5，接取时不发物品。

### 链 2：读档整份换掉内存

`LoadArchive(guid)`：

1. 按 guid 打开那一档的文件夹和虚拟文件系统  
2. `masterGameData = LoadGameData()`（整份换成磁盘上的主存档）  
3. `archiveDataDic.Clear()`（清掉内存里缓存过的 `PlayerQuestData`、`PlayerBagData` 等）

下次 `GetData<PlayerQuestData>()` 用新的主存档重新 `ParseInternal`：先 `questStates.Clear()`，再只按盘上的 `Quest_{id}_State` 键填回去。盘上没有 Quest_002 的键，内存里就是没有，`GetQuestState` 返回 **null**，不是残留的 InProgress。

背包同理：读哪一档，藤蔓果就是那一档上次把背包写进去时的数量。和「这一局刚捡的果」不是同一本账。

### 链 3：椅子按状态选 Prefab

`Npc23QuestStoryTrigger` 与 `FarmerQuestStoryTrigger` 一样，**未接只认 null**。没有「Available 也算未接」的分支。

| 读档结束后 `GetQuestState("Quest_002")` | 背包藤蔓果 | 播的 Prefab |
|----------------------------------------|------------|-------------|
| **null**（盘上无此键） | 不看 | `Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab`（接任务长对白，含「我有些忙 / 好呀」） |
| **InProgress**（2） | 不足 5 | `Village_QuestThanks_NPC23`（「感谢你」） |
| **InProgress**（2） | ≥ 5 | `Village_QuestTurnIn_NPC23` |
| **TurnedIn**（4） | 不看 | `Village_QuestThanks_NPC23`（不回 Offer。与老农「交完不回接任务」一致） |
| 其它（如脏数据 Complete） | 不看 | Thanks（兜底，也不回 Offer） |

点的是场景里的 **`NpcChair`**。`Npc1` 挂的是龙宫 `HomeScene1Npc1`，测接任务不要点它。

### 三种操作

| 操作 | 现网 | 原因 |
|------|------|------|
| 接之前先**另存一档**，再点「好呀」，再读**那一档**（不是读当前槽） | **通** | 接任务只写当前槽。那份旧档没有 `Quest_002` 键。读档清缓存后 `GetQuestState` 为 null → Offer |
| 接了之后存档（或接的时候已经自动写入的当前槽），再读**这一档** | **通（产品如此）** | 盘上已是 InProgress。不够果出感谢，够 5 个出交付。没有「好呀」 |
| 没另存，只读「继续游戏」进来的当前档 | **不通，而且不该通** | 点「好呀」时 `SaveSpcData` 已经把**这一档**改成已接。读它不可能回到未接。不是读档没清内存 |

藤蔓果单独说：它跟这次读档走，但走的是**被读的那一档里的背包**，不是任务状态。Offer 出不来只因为任务已接。果没回到「接之前的数量」，是因为你读的档本身果就不是那个数，或接之后又存过背包；不要和「接任务对话没了」算成同一个 bug。进行中时，果 ≥ 5 会进交付图，不足进感谢图，两条都不是 Offer。

### 和老农差在哪

Quest_003 的 P2 **现网仍成立**，和民居是同一条读档管线，**不是同一处坏掉的缺口**。`LoadArchive` 清缓存再按盘解析，两任务共用。接之前的档没有对应 State 键，就再出 Offer。不要为 Quest_002 去改 `FarmerQuestStoryTrigger` 或读档。

老农能反复测，是因为有菜单 `Editor/Quest/ResetQuest_003 老农打水`（`Assets/Scripts/Debug/Editor/QuestResetDebugMenu.cs`）。它只在 Play 模式调用 `QuestManager.ResetQuest("Quest_003")`：从内存字典删掉这个 id，再 `SaveQuestProgress`。不改背包、不播对白。删掉之后同一局再点老农，状态是 null，Offer 回来。

**Quest_002 没有同等菜单。** `ResetQuest` 本身按 questId 通用，缺的只是菜单入口。

已知、本票不要修：`PlayerQuestData.SerializeInternal` 只 `SetValue` 还在字典里的键，**不** `RemoveField` 被删掉的键。所以 Reset 之后同一局内存已是未接，但当前槽磁盘上旧的 State 键可能还在；退出再读**这一档**，已接会回来。老农菜单也是这样。这不是「读接之前的另一档失败」，不要为了它改正式读档，也不要改老农菜单。

---

## ③ 用户需要做什么

1. 进到能点椅子、**还没选「好呀」** 的时候，用游戏里的**另存为新档**（不要只覆盖正在玩的这一档）。记住这份档。  
2. 回 `Village_HomeScene23`，点 **`NpcChair`**（椅子，不是 `Npc1`）。听到「妈妈，有外人！」，选「好呀」。Console 应有 `[Quest] Accept Quest_002`。  
3. 读档时选**第 1 步那份**，不要选「继续」或接任务之后存的档。再进这间屋，再点椅子：应再次出现接任务长对白和「好呀 / 我有些忙」。  
4. 再读**接过之后**的那一档，点同一张椅子：应是「感谢你」或交付句，不能再出现「好呀」。  
5. 若交过任务的档：点椅子仍是感谢，不能变回接任务。  
6. 若从来没有第 1 步那种档：等施工加上 `ResetQuest_002` 后，Play 进游戏，点 `Editor/Quest/ResetQuest_002`，**同一局**再点椅子。退出再读刚才那一档，已接还会在——那是菜单只保证这一局，不是读档坏了。

藤蔓果以所读档为准。测「不够果出感谢 / 够 5 个出交付」时，看的是这份档里的果，不是上一局捡了还没存的果。

---

## ④ 给施工员的补充

### 改哪些 / 不要改哪些

| 全路径 | 做什么 |
|--------|--------|
| `Assets/Scripts/Debug/Editor/QuestResetDebugMenu.cs` | **只加**一个菜单，仿现有 `ResetQuest_003` |
| `Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs` | 不改。`ResetQuest` 已按 id 删除并落盘 |
| `Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/PlayerQuestData.cs` | 不改。不要为了清键去改序列化（会碰到老农 Reset 的落盘方式） |
| `Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveComponentGM.cs` | 不改。读档清缓存是对的 |
| `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/Npc23QuestStoryTrigger.cs` | 不改。null → Offer 已与老农一致 |
| `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/FarmerQuestStoryTrigger.cs` | 不改 |
| `QuestResetDebugMenu` 里现有的 `ResetQuest_003` 方法 | 不改 |
| `Village_QuestOffer_NPC23.prefab` 及 Thanks / TurnIn | 不改 |
| 任何存档文件、CSV | 不改 |

### 推荐方案（只此一种）

在 `QuestResetDebugMenu` 增加：

`Editor/Quest/ResetQuest_002 妈妈的藤蔓果`

Play 模式调用 `QuestManager.getInstance().ResetQuest("Quest_002")`。非 Play 只打警告并返回。与老农那条相同：只清这个任务在内存字典里的状态和进度并按现有 `SaveQuestProgress` 落盘；**不改背包、不改 Quest_003、不改正式读档规则。**

这样：有接之前的档时，现网读档已经能再播 Offer，菜单用不上。没有干净档时，同一局点菜单即可再测接任务长对白。读已接档、已交档，不点菜单则行为与现在一致。

### 否决方案

- 任务不落盘，或把 `PlayerQuestData` 从存档拿掉：已接档、已交档读出来会变回未接，和「读已接不要再出选项」相反。  
- 交完也能再接（`TurnedIn` 再回 Offer，或 `AcceptQuest` 对已交放行）：0831 已否决，民居和老农都要保持交完不回接任务。

### 会误伤的其它场景

按推荐方案：**无。** 菜单不自动跑，不点就不影响 Quest_001 / Quest_003 和任何正式读档。
