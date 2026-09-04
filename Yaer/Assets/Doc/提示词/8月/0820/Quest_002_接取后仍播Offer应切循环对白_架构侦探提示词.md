# Cursor Agent Prompt · 接取后仍重复播接任务对白（应切到循环对话）

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **测试现象（开发者已测，未达预期）**：玩家 **已经接取** Quest_002 之后，再按 E 找 NPC，**仍一直重复整段接任务对白**（`Village_QuestOffer_NPC23`：长对白 +「我有些忙 / 好呀」）。  
> **产品期望**：接取成功后，NPC 应进入 **循环对话**（首版：背包不够时反复「感谢你」；够了再播交付句——详见既有任务②提示词）。**不应**再播接任务 Offer。  
> **本阶段**：只读 + 写溯源报告，**不施工**  
> **范围**：查清「为什么接完还播 Offer」+ 最小切换方案（Trigger / Prefab）。不重做接取 Accept；不实现扣果发奖细节（可引用任务①报告）。

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

任务系统测了：接任务成功了，但之后 NPC **还是反复播接任务对话**。  
接完之后就应该开始 **循环对话**。请查明为什么，以及要改什么才能切过去。

### 产品期望（接取后对话）

| 任务状态 | 再按 E 应播 | 不应再播 |
|----------|-------------|----------|
| 未接 | `Village_QuestOffer_NPC23`（接任务） | — |
| **已接 InProgress**（果不够） | **循环短对白「感谢你」** | ❌ 整段 Offer + 再选「好呀」 |
| 已接、果够 | 「有了这些今晚就不用愁了」 | ❌ Offer |
| 已 TurnedIn | OPEN（短句） | ❌ Offer / 再 Accept |

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 根因 | `NpcChair`（或实际 NPC）仍是 **`SimpleStoryTrigger`**，`StoryPrefabName` **写死** `Village_QuestOffer_NPC23`；按 E **永远** Trigger 同一段 Offer |
| Accept 本身 | ✅ 已通（存档 InProgress）；**对话切换与 Accept 无关**，缺的是「按状态选 Prefab」 |
| 对照样板 | 埃吉尔用 `AegirQuestStoryTrigger.ResolveStoryPrefabName()` 按状态切 Offer/TurnIn；NPC23 **没有**对等触发器 |
| 为何不能抄 Complete | 任务①已钉：CollectItem **不会**变 Complete；应用 **`GetQuestState==InProgress`（+ 可选背包）** 切循环对白 |
| 缺资源 | 循环「感谢你」Prefab **很可能尚未创建** → 即使改 Trigger 也无处可切；须写进清单 |
| 相关未跑报告 | 任务②提示词已写、**执行文档可能未出**：本报告可吸收其结论，但主问钉死「接完仍播 Offer」测试失败 |

### 生活类比

接任务像签了订单；Offer 对白是「要不要接单」的推销词。  
签完还天天播推销词 = 店员没换话术本。需要按「已接单」换成「感谢你 / 交货」话术本，而不是订单系统坏了。

### 必读

- `Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md`（§4.3：不能按 Complete 切）
- `Assets/Doc/提示词/0820/Village_NPC23_交付对白Prefab与切换_架构侦探提示词.md`（任务②；若有报告一并读）
- `Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md`
- `SimpleStoryTrigger.cs`（`ResolveStoryPrefabName` 虚方法）
- `AegirQuestStoryTrigger.cs`
- 场景：`Village_HomeScene23` → `NpcChair`（组件类型、`StoryPrefabName`）
- Prefab：`Village_QuestOffer_NPC23`；搜有无 Thanks/TurnIn NPC23 Prefab

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md
@Assets/Doc/提示词/0820/Village_NPC23_交付对白Prefab与切换_架构侦探提示词.md
@Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/AegirQuestStoryTrigger.cs
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景。只读扫描 + 写溯源报告。

---

## 背景（测试失败）

1. 接 Quest_002 已成功（Console 有 Accept）。
2. 再找 NPC 按 E：**仍播完整接任务对白**（含选项），未达预期。
3. 期望：接取后进入**循环对话**（至少「感谢你」可重复），不要再推销接任务。
4. 本期查明根因 + 切换清单。可与任务②合并口径，但报告标题钉本测试失败。

---

## 必查

### A. 现网为什么永远播 Offer

1. 场景交互体是谁（`NpcChair`？）  
2. 组件：`SimpleStoryTrigger` 还是子类？  
3. `StoryPrefabName` 现值？  
4. `TriggerStory` 调用链是否只读 `ResolveStoryPrefabName()` → 默认返回序列化字段？  
5. Accept 之后有没有任何代码改 StoryPrefabName 或换组件？

画出：

```
按 E → Trigger → ResolveStoryPrefabName → 实际 Prefab 名
```

### B. 期望状态机（对话侧）

| 状态 | Prefab |
|------|--------|
| 未接 | Offer（现有） |
| InProgress 且果不够 | Thanks 循环「感谢你」 |
| InProgress 且果够 | TurnInSuccess「今晚不用愁」 |
| TurnedIn | OPEN |

钉死：循环对话 = 短 Prefab 可反复 Trigger，**不是** Offer 图内加「已接则跳过」糊弄（可作替代方案但非首选）。

### C. 最小修复方向（只设计）

对照埃吉尔，列出文件级清单：

| 项 | 建议 |
|----|------|
| 新 Trigger 子类 | 如 `Npc23QuestStoryTrigger`，override `ResolveStoryPrefabName` |
| 分支条件 | `GetQuestState(Quest_002)` +（可选）`GetMainItemCount(TenWangFruit)` |
| 新 Prefab | 至少一个 Thanks；Success 可同批或下批 |
| 场景 | NpcChair 换组件 / 或保留序列化字段作 Offer 默认名 |
| 禁止 | 改 Offer 图把头几句删掉冒充循环；依赖 Complete |

### D. 与任务① / ② 的边界

- 任务①：交时查背包 / TurnIn API —— 本件只引用「不要用 Complete 切对话」  
- 任务②：Thanks / Success Prefab 细则 —— 本件主答「为何还在播 Offer」；Prefab 命名可与②对齐，避免两份报告打架  
- 若任务②报告已存在：注明吸收点，不要重复长文

### E. 验收（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 未接按 E | 仍播 Offer + 选项 |
| 2 | 接取后再按 E（背包无果） | **只**「感谢你」（或报告裁定的循环 Prefab）；**无**「我有些忙/好呀」 |
| 3 | 再按 E 多次 | 仍是循环短对白，不重播 Offer |
| 4 | 埃吉尔线 | 不受影响 |

---

## 侦探任务

1. **结论一句话**：接完仍播 Offer 是因为 Trigger 写死 Offer 名；要按 InProgress 切循环 Prefab。  
2. **现场锚点**：场景物体、组件、StoryPrefabName。  
3. **调用链**。  
4. **最小切换清单**（Trigger + Prefab + 场景）。  
5. OPEN：Thanks Prefab 名；TurnedIn 说什么；是否与扣果同批施工。  
6. **禁止**：改资产；把锅甩给 Accept 失败；用 Complete 当切换条件。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Quest_002_接取后仍播Offer应切循环对白_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：签完单还播推销词）  
③ 用户检查清单（现网卡在哪、要新建/替换什么）  
④ 程序：调用链、状态→Prefab 表、与埃吉尔对照、与任务①②边界、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Quest_002_接取后仍播Offer应切循环对白_架构溯源报告.md
@Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按报告修复：Quest_002 接取后，再按 E 不得再播整段接任务 Offer；应进入循环对白（默认「感谢你」）。

必须：用任务状态切换 Prefab（仿 AegirQuestStoryTrigger，但条件用 InProgress/背包，不用 Complete）；不改 Quest_001；未接仍播 Offer。

若报告要求新建 Thanks Prefab，一并做最小一句对白。扣果发奖可按报告是否列入本批。

提交说明：Trigger 怎么切、新建了哪个 Prefab、如何验收「接完不再出选项」。
```
