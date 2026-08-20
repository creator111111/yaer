# Quest_002 — 接取后仍播 Offer、应切循环对白 — 架构溯源报告

**文档性质**：架构侦探产出（测试失败溯源；**本阶段不改资产**）  
**日期**：2026-08-20  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- 提示词：`Assets/Doc/提示词/0820/Quest_002_接取后仍播Offer应切循环对白_架构侦探提示词.md`
- 任务①：`Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md`（勿用 Complete 切对话）
- 任务②提示词（报告未出）：`Assets/Doc/提示词/0820/Village_NPC23_交付对白Prefab与切换_架构侦探提示词.md`
- 样板：`AegirQuestStoryTrigger.cs`、`SimpleStoryTrigger.ResolveStoryPrefabName`

**测试现象**：Quest_002 **已 Accept**，再按 E 仍完整播 `Village_QuestOffer_NPC23`（长对白 +「我有些忙 / 好呀」）。  
**产品期望**：接取后进 **循环对白**（果不够：「感谢你」可反复；果够：「有了这些今晚就不用愁了」），**不再**播接任务 Offer。  

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**接完还播 Offer，不是 Accept 失败，而是 `NpcChair` 仍用 `SimpleStoryTrigger` 把 `StoryPrefabName` 写死为 `Village_QuestOffer_NPC23`——按 E 永远同一段；要仿埃吉尔做按状态选 Prefab 的 Trigger，条件用 `InProgress`（+ 背包），并新建「感谢你」等循环 Prefab（现网还没有）。**

---

## ② 原因（生活类比）

接任务像签了订单；Offer 是「要不要接单」的推销词。  
签完还天天播推销词 = **店员没换话术本**，不是订单本（任务存档）坏了。  
需要按「已接单」换成「感谢你 / 交货」话术本。

---

## ③ 用户需要做什么（检查清单）

| # | 现网卡在哪 | 要做什么 |
|---|------------|----------|
| 1 | `NpcChair` = `SimpleStoryTrigger`，名写死 Offer | 换成/挂上 **NPC23 专用 Trigger 子类**（仿 `AegirQuestStoryTrigger`） |
| 2 | 无 Thanks / TurnIn Prefab | **至少新建**一句「感谢你」的对话 Prefab；果够句可同批或跟任务② |
| 3 | 条件若抄埃吉尔用 Complete | **禁止**；CollectItem 到不了 Complete（任务①）→ 用 **`InProgress` + 背包数量** |
| 4 | Offer 图 | **不要**靠删头几句冒充循环；保留给「未接」 |
| 5 | Accept / QuestConfig | **不必**为切对话再改 |

**施工后验收（摘要）**：未接仍 Offer+选项；接后再按 E（无果）只「感谢你」、无选项；多次仍短循环；埃吉尔线不动。

---

## ④ 给程序看的补充

### 4.1 现场锚点（为何永远播 Offer）

| 项 | 现网 |
|----|------|
| 场景 | `Village_HomeScene23.unity` |
| 物体 | **`NpcChair`**（`m_Name: NpcChair`） |
| 组件 | **`SimpleStoryTrigger`**（guid `6ff85fae…` = 该类 meta，**不是** `AegirQuestStoryTrigger`） |
| `StoryPrefabName` | **`Village_QuestOffer_NPC23`**（写死） |
| Accept 后改名/换组件？ | **无**（全工程无接取后改 `StoryPrefabName` 的逻辑） |
| Thanks / TurnIn Prefab | **不存在**（`Dialogue/` 仅有 `Village_QuestOffer_NPC23`） |

### 4.2 调用链

```
按 E
  → InteractiveComponent 点击
  → SimpleStoryTrigger.OnClickTriggerStory
  → TriggerStory()
  → ResolveStoryPrefabName()
       默认实现：直接 return 序列化字段 StoryPrefabName
       = "Village_QuestOffer_NPC23"（恒定）
  → StoryComponentGSM.TriggerStory(该名)
  → 整段 Offer（含 MultipleChoice）
```

埃吉尔对照：子类 **override** `ResolveStoryPrefabName()`，按 `QuestState` 在 Offer / TurnIn 间切换。NPC23 **没有**对等子类。

### 4.3 期望状态机（对话侧）

| 状态 | 应播 Prefab | 不应播 |
|------|-------------|--------|
| 未接（null） | `Village_QuestOffer_NPC23` | — |
| **InProgress** 且 `TenWangFruit < 5` | **Thanks 循环「感谢你」** | ❌ Offer + 选项 |
| **InProgress** 且 `≥ 5` | **Success「有了这些今晚就不用愁了」**（末可挂扣果/发奖，任务①） | ❌ Offer |
| **TurnedIn** | OPEN（可暂 Thanks 或另短句） | ❌ Offer / 再 Accept |

循环对话 = **短 Prefab 可反复 Trigger**，不是 Offer 图内糊弄跳过。

### 4.4 最小切换清单（只设计）

| 项 | 建议 |
|----|------|
| 新 Trigger | 如 `Npc23QuestStoryTrigger : SimpleStoryTrigger`，override `ResolveStoryPrefabName` |
| 分支条件 | `GetQuestState("Quest_002")`；若 `InProgress` 再 `GetMainItemCount(TenWangFruit)` ≥5？ |
| 新 Prefab（命名对齐埃吉尔 + 任务②口径） | **`Village_QuestThanks_NPC23`**（感谢你）；**`Village_QuestTurnIn_NPC23`**（今晚不用愁；可与任务②合并施工） |
| 场景 | `NpcChair`：脚本换成新 Trigger；序列化字段可仍填 Offer 作「未接默认名」或由代码常量覆盖 |
| 说话人 | 预扫 **NPC3**（妈妈） |
| 禁止 | 改 Offer 冒充循环；用 **Complete** 切；动 Quest_001 |

伪逻辑（施工参考，非落盘）：

```
state = GetQuestState(Quest_002)
if state == null → Offer
if state == TurnedIn → Thanks 或 TurnedIn 短句（OPEN）
if state == InProgress:
    if bag >= 5 → TurnIn Prefab
    else → Thanks Prefab
else → Offer  // 兜底
```

### 4.5 与埃吉尔对照

| | 埃吉尔 | NPC23 / Quest_002 |
|--|--------|-------------------|
| Trigger | `AegirQuestStoryTrigger` | **缺** → 现网裸 `SimpleStoryTrigger` |
| 切到交付条件 | `Complete`（杀满） | **`InProgress` + 背包**（任务①） |
| Offer 写死问题 | 已用子类解决 | **正是当前 bug** |
| TurnedIn 后再按 E | 首版仍可能回 Offer | 须 OPEN，勿默认回 Offer 推销 |

### 4.6 与任务① / ② 边界

| 件 | 职责 | 本件 |
|----|------|------|
| 任务① | 交时查背包 API、扣果、Grant | **只引用**：勿用 Complete 切对话 |
| **本件（测试失败）** | **为何还播 Offer** + Trigger/Prefab 切换清单 | 主答 |
| 任务② | Thanks/Success Prefab 细则、图内是否挂 TurnIn Action | 提示词已有、**执行文档未出**；本件命名与之对齐，避免打架。细则可另出②报告或本批一并施工 |

### 4.7 验收（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 未接按 E | Offer +「我有些忙/好呀」 |
| 2 | 接取后再按 E（背包无果） | **只**「感谢你」；**无**选项 |
| 3 | 再按 E 多次 | 仍短循环，不重播 Offer |
| 4 | 埃吉尔线 | 不受影响 |

### 4.8 开放问题（已记入 OPEN）

| ID | 问题 | 施工默认 |
|----|------|----------|
| Q1 | Thanks Prefab 最终名？ | **`Village_QuestThanks_NPC23`** |
| Q2 | TurnIn/Success Prefab 名？ | **`Village_QuestTurnIn_NPC23`** |
| Q3 | TurnedIn 后再按 E 说什么？ | 首版可暂播 Thanks；或另短句 |
| Q4 | 切对话与扣果发奖是否同批？ | **可先只做 Thanks 切换**（修本测试失败）；果够 Prefab + 扣果可跟任务①②同批或下批 |

---

## 5. 相关文档

| 主题 | 路径 |
|------|------|
| 本提示词 | `Assets/Doc/提示词/0820/Quest_002_接取后仍播Offer应切循环对白_架构侦探提示词.md` |
| 任务① 查背包 | `Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md` |
| 任务② 提示词 | `Assets/Doc/提示词/0820/Village_NPC23_交付对白Prefab与切换_架构侦探提示词.md` |
| 埃吉尔 Trigger | `AegirQuestStoryTrigger.cs` |

---

## 6. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-08-20 | 初版：写死 Offer 根因；InProgress+背包切换；Thanks Prefab 缺失 |

**文档路径**：`Assets/Doc/执行文档/0820/Quest_002_接取后仍播Offer应切循环对白_架构溯源报告.md`
