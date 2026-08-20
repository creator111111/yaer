# Cursor Agent Prompt · 任务①：Quest_002 交付逻辑是否等于「交任务时查背包」

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **任务编号**：交付链路 · 第 1 件（纯逻辑对拍）  
> **产品口径（已定）**：  
> - **不要求**玩家去刷 / 刷新藤蔓果来推进度  
> - 本质是：**提交任务时检测背包**有没有足够藤蔓果（`TenWangFruit` ×5）  
> - 不够 → 不能交、不能发奖；够了 → 才能交并拿 50 金币  
> **样板对照**：埃吉尔 `Quest_001`（杀满 → Complete → TurnIn Prefab）  
> **本阶段**：只读 + 写报告，**不施工**  
> **不在本期**：交付对白文案 / 新建 Prefab（那是第 2 件提示词）

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

接任务已经测通了。接下来做「做任务 / 交任务」。  
产品说：**不用刷果推进度**，交的时候看背包够不够 5 个藤蔓果就行。  
请查明：**现网任务逻辑是不是这套？差在哪？要改哪几环才能变成这套？**

### 产品期望 vs 埃吉尔现网（预扫）

| 维度 | 产品（NPC23 / Quest_002） | 埃吉尔现网（Quest_001） |
|------|---------------------------|-------------------------|
| 进度怎么涨 | **不涨**；交时看背包 | 杀怪 `OnMonsterKilled` 累加 |
| 何时算「能交」 | 背包 `TenWangFruit ≥ 5` | 状态已是 `Complete` |
| TurnIn 前提 | 应查背包（+ 已接取） | `TurnInQuest` **只认** `Complete` |
| 扣物品 | 应交时 `TryRemoveMainItem` ×5 | 无物品；只发 Gold |
| 刷怪 / 刷果 | **不要** | 需要杀够 10 只 |

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| `Quest_002` 配置 | ✅ 已有：`CollectItem` / `targetItem=TenWangFruit` / count 5 / Gold 50 |
| 接取 | ✅ 已测通 Accept |
| CollectItem 运行时 | ❌ `OnMonsterKilled` 只处理 KillMonster；**没有任何**入包/交时把 CollectItem 推到 Complete |
| `TurnInQuest` | ❌ 非 Complete 直接失败 → 若一直 InProgress，**现网交付 Action 交不出去** |
| `QuestTurnInAction` | ❌ 不查背包、不扣果，只 TurnIn + Grant |
| 与产品符合度 | **不符合**「交时查背包」；更接近「先 Complete 再交」，而 CollectItem **没有** Complete 来源 |

生活类比：埃吉尔是「先把考卷答完（Complete）再交卷」；产品要的是「交卷那天当场数书包里有没有 5 个果」，没答完考卷这回事。

### 必读

- `Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md`（§4.4 方案 A 交付查背包）
- `QuestManager.cs`：`AcceptQuest` / `OnMonsterKilled` / `CanTurnInQuest` / `TurnInQuest` / `GrantQuestRewards`
- `QuestTurnInAction.cs`、`QuestDataTableRow`（`targetItem` / CollectItem）
- `QuestConfig.json` 的 `Quest_002`
- `PlayerBagData.GetMainItemCount` / `TryRemoveMainItem`
- 埃吉尔：`AegirQuestStoryTrigger`（Complete 才切 TurnIn Prefab）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md
@Assets/GameRes/Config/QuestConfig/QuestConfig.json
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/QuestTurnInAction.cs
@Assets/Scripts/Game/DataTable/QuestConfig/QuestDataTableRow.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Player/PlayerBagData.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/AegirQuestStoryTrigger.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、配置。只读 + 写溯源报告。

---

## 背景

1. Quest_002 接取已成功。
2. 产品：不做刷藤蔓果进度；交任务时检测背包够不够 5 个。
3. 本期只回答：现网逻辑是否符合？缺口表 + 最小改法方向。不对白 Prefab。

---

## 必查

### A. 接取之后状态机

- Accept 后 Quest_002 是什么状态、progress 多少？
- 谁会把 CollectItem 任务写成 Complete？有没有？
- `CanTurnInQuest` / `TurnInQuest` 对 InProgress 会怎样？

### B. 与「交时查背包」对拍

填符合度表：

| 产品要求 | 现网有无 | 证据（类/方法） |
|----------|----------|-----------------|
| 不靠刷果推 progress | | |
| 交时 GetMainItemCount(TenWangFruit)≥5 | | |
| 不够则 TurnIn/发奖都不发生 | | |
| 够则扣 5 个再发 50 金 | | |

### C. 推荐最小改法（只设计，不施工）

在现网架构上选一个主方案（可附替代）：

| 方案 | 大意 |
|------|------|
| A | 新 Action / 扩展 TurnIn：InProgress + 背包≥5 → 扣果 → 直接 TurnedIn + Grant（可跳过 Complete，或交时临时标 Complete） |
| B | 每次打开交付对话前扫背包，够则先 Set Complete 再走旧 TurnIn |
| C | 拾取时累加 CollectItem（产品已否决刷果进度——仅作对照，勿当主方案） |

钉死：主方案是哪个；要动哪些类；**不要**为了 CollectItem 去加地图刷果。

### D. 与埃吉尔触发器的关系

- `AegirQuestStoryTrigger` 用 Complete 切 Prefab——对 Quest_002 **不能照搬**（永远到不了 Complete）。
- 第 2 件对话任务会用「InProgress + 背包」分支；本期只点明逻辑前提。

---

## 侦探任务

1. **结论一句话**：现网符不符合「交时查背包」；差在哪。  
2. **状态机图**：Accept → ? → TurnIn。  
3. **符合度表**。  
4. **最小改法主方案**（文件级清单，不写完整代码）。  
5. OPEN：「Quest_002 交时查背包逻辑 · 2026-08-20」——是否保留 Complete 状态、扣果失败怎么办。  
6. **禁止**：改资产；设计刷果玩法；写交付 Prefab 台本（留给任务②）。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md`

① 结论 ② 原因（生活类比） ③ 用户检查清单 ④ 程序：状态机、符合度、改法、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑

> 本件是纯逻辑对拍。**不要**附施工员续跑。等与任务②（交付对白）报告一起拍板后再开施工。
