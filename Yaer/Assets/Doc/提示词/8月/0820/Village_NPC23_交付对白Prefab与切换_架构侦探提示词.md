# Cursor Agent Prompt · 任务②：NPC23 交付对话（不够「感谢你」/ 够了「今晚不用愁」）

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **任务编号**：交付链路 · 第 2 件（对白 Prefab + 怎么切）  
> **依赖**：先或并行读任务①报告 `Quest_002_交时查背包逻辑对拍`（若尚未产出，按提示词预扫自行对拍逻辑，勿假设已 Complete）  
> **产品口径（已定）**：  
> - 已接任务后找 NPC：  
>   - 背包藤蔓果 **不够 5 个** → 一直重复播 **「感谢你」**（短对白，可反复点）  
>   - 背包 **够 5 个** → 播 **「有了这些今晚就不用愁了」**（交付成功对白；随后扣果 + 发 50 金属逻辑层，可挂 Action）  
> - **应该要做新的对话预制体**（开发者倾向）；侦探须裁定：几个 Prefab、谁按什么条件 Trigger  
> **接取 Prefab（已有）**：`Village_QuestOffer_NPC23`  
> **样板**：`Village_Aegir_QuestTurnIn` + `AegirQuestStoryTrigger`（**条件不同**，勿照搬 Complete）  
> **本阶段**：只读 + 写报告，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. 交任务时：不够果子就反复听「感谢你」；够了就听「有了这些今晚就不用愁了」。  
2. 这是不是要**新做对话 Prefab**？做一个还是两个？  
3. 场景里 `NpcChair`（或实际挂接 NPC）怎么从「接任务对白」切到「感谢你 / 交成功」？

### 产品状态机（对话侧，已定）

```
未接 / 拒过未接
  → Village_QuestOffer_NPC23（接任务长对白 + 选项）【已有】

已接 InProgress，背包 < 5
  → 短对白：「感谢你」→ 结束（不扣果、不发奖）
  → 再按 E → 仍是这句（可重复）

已接 InProgress，背包 ≥ 5
  → 对白：「有了这些今晚就不用愁了」（可再扩展句子，本期先钉这句）
  → 末尾 → 扣 5 果 + TurnIn + Grant 50（逻辑见任务①）

已 TurnedIn
  → ？首版可仍「感谢你」或另短句 —— 记 OPEN
```

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 要不要新 Prefab | **要**（至少交付相关；接取 Offer 已独立） |
| 一个还是两个 | **方案对比**：① 两个 Prefab（Thanks / Success）由 Trigger 按背包切；② 一个 Prefab 图内 Condition 分支。侦探推荐一个 |
| 埃吉尔样板 | Complete → TurnIn Prefab；Quest_002 **不能**等 Complete（任务①已说明） |
| Trigger | `NpcChair` 现多半仍是 `SimpleStoryTrigger` 写死 Offer → 需仿 `AegirQuestStoryTrigger` 做 **NPC23 专用触发器**，分支条件 = 任务状态 + **背包数量** |
| 「感谢你」可重复 | Trigger 在 TurnedIn 之前、背包不足时**一直**解析到 Thanks Prefab |
| 文案说话人 | 预扫 NPC3（妈妈）；须对拍 |

### 文案（已定，可再补句但勿改核心）

| 情境 | 台词 |
|------|------|
| 已接、果不够 | **感谢你** |
| 已接、果够了 | **有了这些今晚就不用愁了** |

### 必读

- 任务①提示词 / 报告（若有）：`Assets/Doc/提示词/0820/Quest_002_交时查背包逻辑对拍_架构侦探提示词.md`
- 埃吉尔交付：`Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_交付换场与发奖_架构溯源与施工执行说明.md`
- `AegirQuestStoryTrigger.cs`、`Village_Aegir_QuestTurnIn.prefab`
- `Village_QuestOffer_NPC23.prefab`、场景 `NpcChair` / HomeScene23
- `SimpleStoryTrigger` / `ResolveStoryPrefabName` 可覆写点
- `PlayerBagData.GetMainItemCount`

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/提示词/0820/Quest_002_交时查背包逻辑对拍_架构侦探提示词.md
@Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md
@Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_交付换场与发奖_架构溯源与施工执行说明.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/AegirQuestStoryTrigger.cs
@Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestTurnIn.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Player/PlayerBagData.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景。只读 + 写溯源报告。

---

## 背景

1. 接取 Prefab 与 Quest_002 Accept 已通。
2. 产品要两套交任务后的对白：不够听「感谢你」（可重复）；够了听「有了这些今晚就不用愁了」。
3. 开发者倾向新对话预制体。请钉死：几个 Prefab、命名建议、Trigger 条件、图内要不要挂扣果/发奖 Action。
4. 逻辑层「交时查背包」细节以任务①为准；本期聚焦对话资源与切换。

---

## 必查

### A. 现网场景挂接

- `NpcChair`（或实际 NPC）组件类型、`StoryPrefabName` 现值
- 接取后按 E 会不会仍播整段 Offer（含选项）——这是痛点还是可接受？

### B. 埃吉尔交付样板（可抄什么、不可抄什么）

| 可抄 | 不可照搬 |
|------|----------|
| 独立 TurnIn Prefab + 子类 Trigger | 仅用 `Complete` 切对话 |
| 对白末挂交付 Action | 不查背包 |

### C. Prefab 方案裁定（必选一个主方案）

| 方案 | 结构 | 利弊预扫 |
|------|------|----------|
| P1 | `…_QuestThanks`（感谢你）+ `…_QuestTurnIn`（今晚不用愁）两 Prefab；Trigger 按背包切 | 图简单；Trigger 稍复杂 |
| P2 | 单一 `…_QuestProgress` Prefab，图内 Condition 分支两句 | Prefab 少；图要读背包 |
| P3 | 不够仍播 Offer 截断——**产品已否决**（要短「感谢你」） |

推荐命名草案（可改）：`Village_QuestThanks_NPC23`、`Village_QuestTurnIn_NPC23`。

### D. Trigger 伪逻辑（须写成清晰表）

```
未接 / TurnedIn? → Offer 或短结束句（OPEN）
InProgress && count < 5 → Thanks
InProgress && count >= 5 → TurnInSuccess
```

钉死：count 读谁、questId 写死 Quest_002 还是可配。

### E. Success 图末 Action

- 是否挂扩展后的 TurnIn（扣果+发奖）——依赖任务①主方案  
- 「感谢你」图 **禁止** 挂 TurnIn / Grant  

### F. 验收（施工后）

| 步骤 | 期望 |
|------|------|
| 已接、背包 0～4 果，按 E | 只出「感谢你」；可重复；无 Grant |
| 作弊/商店凑满 5 果，按 E | 「有了这些今晚就不用愁了」→ 扣 5 → +50 金 → TurnedIn |
| 再按 E | 不重复发奖（OPEN 播哪句） |
| 未接 | 仍播 Offer |

---

## 侦探任务

1. **结论一句话**：要几个新 Prefab、怎么切、和埃吉尔差在哪。  
2. **主方案 P1/P2** + 命名。  
3. **Trigger 条件表** + 建议类名（如 `Npc23QuestStoryTrigger`）。  
4. **两段对白最小节点清单**（几句 Statement 即可）。  
5. OPEN：TurnedIn 后再说什么；Success 要不要雅尔回话；是否与任务①同批施工。  
6. **禁止**：改资产；把刷果写进对话条件；改埃吉尔 Trigger。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Village_NPC23_交付对白Prefab与切换_架构溯源报告.md`

① 结论 ② 原因（生活类比：店员看你袋子够不够货再决定说哪句） ③ 用户清单（新建哪些 Prefab、挂哪个 Trigger） ④ 程序：方案对比、条件表、与任务①接口、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（两件报告都拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_NPC23_交付对白Prefab与切换_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按两份报告做最小化修改：已接 Quest_002 后，背包不够播「感谢你」（可重复）；够 5 个藤蔓果播「有了这些今晚就不用愁了」并扣果、发 50 金、TurnedIn。

必须：交时查背包，不靠刷果推 Complete；不改 Quest_001 / 埃吉尔线；「感谢你」不得发奖；TurnedIn 后不重复发奖。

提交说明：逻辑怎么查背包、新建了哪个 Prefab/Trigger、如何验收两种对白。
```
