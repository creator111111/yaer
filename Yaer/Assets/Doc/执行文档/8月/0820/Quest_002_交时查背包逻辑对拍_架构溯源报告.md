# Quest_002 — 交时查背包逻辑对拍 — 架构溯源报告

**文档性质**：架构侦探产出（纯逻辑对拍；**不施工、不写交付 Prefab**）  
**日期**：2026-08-20  
**任务编号**：交付链路 · 第 1 件  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- 提示词：`Assets/Doc/提示词/0820/Quest_002_交时查背包逻辑对拍_架构侦探提示词.md`
- 前置任务卡：`Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md` §4.4 方案 A  
- 代码：`QuestManager.cs`、`QuestTurnInAction.cs`、`QuestDataTableRow.cs`、`PlayerBagData`、`AegirQuestStoryTrigger.cs`  
- 配置：`QuestConfig.json` → `Quest_002`

**产品口径（已定）**：不刷果推进度；**交任务时**检测背包 `TenWangFruit ≥ 5`；不够不能交/不能发奖；够了扣果并发 **50** 金币。  

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**现网不符合「交时查背包」：Accept 后 Quest_002 会一直停在 InProgress（无人写成 Complete），而 `TurnInQuest` / `QuestTurnInAction` 只认 Complete、且不查包不扣果——要变成产品口径，须新增 CollectItem 交付入口（主推方案 A：InProgress + 背包≥5 → 扣果 → TurnedIn + Grant）。**

---

## ② 原因（生活类比）

| | 埃吉尔 Quest_001 | 产品 Quest_002 |
|--|------------------|----------------|
| 类比 | 先把考卷答完（杀满 → Complete）再交卷 | 交卷那天**当场数书包**有没有 5 个果 |
| 现网 | 有「答完」这条路 | CollectItem **没有答完**这条路，却仍要求「必须答完才能交」 |

所以接取已通，但现网交付 Action 对 Quest_002 **交不出去**（状态永远不是 Complete），也**不会**看背包。

---

## ③ 用户需要做什么（检查清单）

> 本件只对拍逻辑；**不要**现在改代码。与任务②（交付对白 Prefab）一起拍板后再施工。

| # | 核对项 | 现网结论 |
|---|--------|----------|
| 1 | `Quest_002` 配置已有 CollectItem / TenWangFruit×5 / Gold50 | ✅ |
| 2 | 接取后状态 | `InProgress`，progress=`0`（Accept 写死） |
| 3 | 有没有代码把 CollectItem 推到 Complete | ❌ 没有 |
| 4 | 现网 `QuestTurnInAction` 能否交掉 Quest_002 | ❌ 会 `TurnIn 失败，状态非 Complete`，不发奖 |
| 5 | 交时查背包 / 扣果 | ❌ 全无 |
| 6 | 要不要做地图刷果推进度 | **不要**（产品已否决） |
| 7 | 下一拍板 | 主方案 A 细节（是否跳过 Complete）+ 任务②对白 |

**禁止误解**：接取成功 ≠ 能交任务；左侧 UI / 刷果玩法 ≠ 本逻辑缺口。

---

## ④ 给程序看的补充

### 4.1 状态机：Accept → ? → TurnIn

```mermaid
flowchart TB
  subgraph Now["现网 Quest_002"]
    A1["AcceptQuest\n→ InProgress, progress=0"]
    Stuck["一直 InProgress\n无人写 Complete"]
    TI1["TurnInQuest / QuestTurnInAction\n要求 state==Complete"]
    Fail["失败：状态非 Complete\n不 Grant"]
    A1 --> Stuck --> TI1 --> Fail
  end

  subgraph Kill["对照：Quest_001 杀怪"]
    A2["Accept → InProgress"]
    Kill["OnMonsterKilled 累加"]
    Comp["满 targetCount → Complete"]
    TI2["TurnInQuest → TurnedIn"]
    Grant["GrantQuestRewards → Gold"]
    A2 --> Kill --> Comp --> TI2 --> Grant
  end

  subgraph Want["产品目标：交时查背包"]
    A3["Accept → InProgress\nprogress 可一直 0"]
    Bag["交时 GetMainItemCount(TenWangFruit)≥5"]
    Rem["TryRemoveMainItem ×5"]
    TI3["→ TurnedIn + Grant 50"]
    Nope["不够：不 TurnIn、不发奖"]
    A3 --> Bag
    Bag -->|够| Rem --> TI3
    Bag -->|不够| Nope
  end
```

| 问题 | 答案 |
|------|------|
| Accept 后状态 / progress | `InProgress` / `0`（见 `AcceptQuest`） |
| 谁把 CollectItem 写成 Complete？ | **没有**。`OnMonsterKilled` 显式 `objectiveType != KillMonster` 则 skip |
| `CanTurnInQuest` | 仅 `state == Complete` → CollectItem 恒 false |
| `TurnInQuest` 对 InProgress | Warning「状态非 Complete」，return false → Action **不**调 Grant |

### 4.2 符合度表

| 产品要求 | 现网有无 | 证据 |
|----------|----------|------|
| 不靠刷果推 progress | ⚠️ 被动符合 | CollectItem 不会被 `OnMonsterKilled` 累加；progress 停在 0。但这是「缺路径」，不是「交时查包」实现 |
| 交时 `GetMainItemCount(TenWangFruit)≥5` | ❌ | 全工程任务路径无此调用 |
| 不够则 TurnIn/发奖都不发生 | ❌ | 现网卡在 Complete 门槛，**未**按背包判；不够/够都交不了 |
| 够则扣 5 个再发 50 金 | ❌ | `QuestTurnInAction` 只 `TurnInQuest` + `GrantQuestRewards`；无 `TryRemoveMainItem`。`Bag` API **现成可用**，任务侧未接 |

配置侧（对拍 `QuestConfig.json`）：

| 字段 | Quest_002 | 备注 |
|------|-----------|------|
| `objectiveType` | `CollectItem` | ✅ 与杀怪区分 |
| `targetItem` | `TenWangFruit` | ✅ 已进 `QuestDataTableRow` / `FromJsonObject` |
| `targetCount` | 5 | ✅ |
| `rewards` Gold | 50 | ✅；Grant 读表，非写死 |

### 4.3 与埃吉尔触发器

`AegirQuestStoryTrigger`：`Complete` → TurnIn Prefab，否则 Offer。  

对 Quest_002：**不能照搬**——按现网永远到不了 Complete，会永远播 Offer。  
任务②对话分支前提应是：**已接取（InProgress）+ 背包是否够**（或交付专用触发器），不是 Complete。本件只钉逻辑前提，不对白。

### 4.4 最小改法（只设计）

| 方案 | 大意 | 裁定 |
|------|------|------|
| **A** | 扩展交付：`InProgress` + 背包≥`targetCount` → 扣 `targetItem` → `TurnedIn` + `Grant`（可跳过 Complete） | **主推** |
| B | 打开交付对话前先扫包设 Complete，再走旧 `QuestTurnInAction` | 可作过渡；多一步状态、易与杀怪 Complete 语义混淆 |
| C | 拾取累加 CollectItem → Complete | **否决**（产品不要刷果进度） |

**方案 A 文件级清单（施工时）**：

| 文件 | 建议 |
|------|------|
| `QuestManager.cs` | 新增如 `CanTurnInCollectQuest` / `TryTurnInCollectQuest`：读 `objectiveType==CollectItem`、`targetItem`、`targetCount`；`GetMainItemCount`；够则 `TryRemoveMainItem` 成功后再写 `TurnedIn` + `Save`；再由调用方 `GrantQuestRewards`。不够 / 扣失败 → false，不改状态、不发奖 |
| `QuestTurnInAction.cs` **或** 新 Action（如「提交物品任务」） | CollectItem 走新 API；KillMonster 仍走旧 `TurnInQuest`（保持埃吉尔）。替代：同一 Action 内按 `objectiveType` 分支 |
| `PlayerBagData` | **不改**（已有 Get/TryRemove） |
| `QuestConfig.json` / `Quest_001` | **不改** |
| 地图刷果 | **不做** |

**原子性**：先判数量 → 再 Remove → 成功才 TurnedIn/Grant。Remove 失败（并发/数量变了）→ 整次失败，打 Warning。  

**与旧 `TurnInQuest` 关系**：杀怪线保持「仅 Complete→TurnedIn」；CollectItem **不要**强行先 Complete 再调旧 API（除非选方案 B）。

### 4.5 开放问题（已记入 OPEN_QUESTIONS）

| ID | 问题 | 施工默认（侦探建议） |
|----|------|----------------------|
| Q1 | CollectItem 是否保留 Complete 状态？ | **可不保留**：交成功时 `InProgress → TurnedIn`；避免假 Complete 只为过旧 API |
| Q2 | 扣果失败怎么办？ | **整次失败**：不 TurnIn、不 Grant；Console Warning；对白层任务②再定是否播「果不够」 |
| Q3 | 扩展旧 `QuestTurnInAction` 还是新 Action？ | 倾向 **新 Action 或同 Action 按 type 分支**，避免误伤埃吉尔 |
| Q4 | `CanTurnInQuest` 是否对 CollectItem 改为查背包？ | 建议 **新方法** 或按 type 分支，供任务②触发器使用 |

---

## 5. 相关文档

| 主题 | 路径 |
|------|------|
| 本提示词 | `Assets/Doc/提示词/0820/Quest_002_交时查背包逻辑对拍_架构侦探提示词.md` |
| 任务卡 / 方案 A | `Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md` |
| 埃吉尔交付 | `Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_交付换场与发奖_架构溯源与施工执行说明.md` |

---

## 6. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-08-20 | 初版：对拍不符合「交时查背包」；状态机；方案 A 最小改法；不做 Prefab |

**文档路径**：`Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md`
