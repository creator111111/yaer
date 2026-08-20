# Cursor Agent Prompt · 任务卡设计：NPC23 提交 5 个藤蔓果 / 报酬 50 金币（让玩家能真正接取）

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **产品已定**：这是一张**采集交付任务卡**——玩家接取后，**提交 5 个藤蔓果**即可；**报酬 50 金币**。  
> **本期目的**：查明「如何制作这张任务卡」，让玩家能**真正接取**（点「好呀」后系统记住任务，Console 有 `[Quest] Accept …`）。  
> **挂接对话**：`Village_QuestOffer_NPC23`（椅子孩子 / NPC3 妈妈请托）  
> **对照样板**：埃吉尔 `Quest_001` + `Village_Aegir_QuestOffer` + `QuestAcceptAction`  
> **本阶段**：只读 + 写溯源报告，**不施工**  
> **前置报告**：`Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md`（选项机制已查明；接取被标成批次 B）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. 要做一张**真正的任务卡**：交 **5 个藤蔓果**，奖 **50 金币**。  
2. 不知道现网任务系统要**改/加哪些东西**，玩家才能接上这条任务。  
3. 目的先钉死：**能真正接取**（不是只弹出「好呀」按钮就结束）。  
4. 交付（扣果、发 50 币）要设计清楚，但**不要**把「采集进度未做」写成「所以接取也做不了」。

### 产品规格（已拍板，侦探按此填配置草案；标题等可 OPEN）

| 项 | 值 |
|----|-----|
| 任务类型 | 提交物品（不是杀怪） |
| 提交物 | 藤蔓果 × **5**（现网物品 id 预扫：`TenWangFruit` / `EMainItemName.TenWangFruit`） |
| 报酬 | **Gold × 50** |
| 接取入口 | NPC23 对白末选项 **「好呀」**（拒：**「我有些忙」**） |
| 建议 questId | `Quest_002`（OPEN：是否沿用此名） |

### 预扫结论方向（可证伪）

| 层 | 预判 | 对「真正接取」的含义 |
|----|------|----------------------|
| **配置行** | `QuestConfig.json` 必须新增一行；`AcceptQuest` 查不到 questId 会 Warning 并**不写入存档** | **接取的硬门槛** |
| **对话图** | 「好呀」分支末挂 `QuestAcceptAction(questId)` | **接取的触发器** |
| **场景** | `Village_HomeScene23` / `NpcChair` 现误挂埃吉尔 Offer，应改成 `Village_QuestOffer_NPC23` | 否则玩家点不到这张卡 |
| **选项节点** | 若批次 A 未施工，图里可能还没有「好呀」 | 接取挂不上；侦探须扫现网 Prefab |
| **objectiveType** | 现网只有 `KillMonster`；表注释写过「后续可扩展 CollectItem」 | **接取可以先不计数**；JSON 仍应写成采集/提交，**禁止**拿 `Quest_001` 杀虫行冒充 |
| **提交 5 个** | 背包已有 `AddMainItem` / `TryRemoveMainItem`；任务侧**无** `OnItemCollected`、无按背包判 Complete | **交付批次**；与接取解耦 |
| **50 金币** | `GrantQuestRewards` 已能发 Gold；走 `QuestTurnInAction` 成功后 | **交付批次** |

生活类比：任务卡是厨房订单（JSON）；点「好呀」是在订单本上签字（Accept）；交 5 个果是把货送到后厨（扣背包 + TurnIn + 发 50 币）。签字不需要货已经在手里。

### 禁止的错误做法（侦探须写进报告）

- 把藤蔓果任务写成 `objectiveType: KillMonster` + 假 `targetMonster`，只为骗过校验。  
- 复用 `Quest_001`（埃吉尔杀虫）。  
- 认为「没有左侧追踪 UI 就不能接取」。  
- 本期设计整套采集玩法地图刷果（可列后续，不阻塞接取清单）。

### 已有文档 / 代码（须读并对拍）

- 六阶段总纲：`Assets/Doc/执行文档/6月/0606/经典MMO击杀任务_任务配置文件_架构溯源与执行说明.md`
- 埃吉尔接取施工：`Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_接取追踪_架构溯源与施工执行说明.md`
- 埃吉尔交付发奖：`Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_交付换场与发奖_架构溯源与施工执行说明.md`
- NPC23 选项：`Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md`
- `QuestConfig.json`、`QuestDataTableRow.cs`、`QuestConfigMgr.cs`、`QuestManager.cs`
- `QuestAcceptAction.cs`、`QuestTurnInAction.cs`
- `PlayerBagData`（`AddMainItem` / `TryRemoveMainItem`）
- `EMainItemName.TenWangFruit`
- Prefab：`Village_QuestOffer_NPC23.prefab`、`Village_Aegir_QuestOffer.prefab`

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/6月/0606/经典MMO击杀任务_任务配置文件_架构溯源与执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_接取追踪_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_交付换场与发奖_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md
@Assets/GameRes/Config/QuestConfig/QuestConfig.json
@Assets/Scripts/Game/DataTable/QuestConfig/QuestDataTableRow.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestConfigMgr.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/QuestAcceptAction.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/QuestTurnInAction.cs
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Player/PlayerBagData.cs
@Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、配置表。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. NPC23（妈妈）请玩家帮忙收集藤蔓果。产品已定：**交 5 个藤蔓果，给 50 金币**。
2. 这是第一张「非杀怪」任务卡，要按现有任务架构做，不要另起炉灶。
3. 开发者最急的是：玩家点「好呀」之后，任务被系统真正接取（存档 InProgress，Console 有 Accept）。
4. 「怎么交 5 个果、怎么发 50 币」也要查明并给出最小设计，但施工清单须把「接取」和「交付」拆开，接取不得被交付阻塞。

---

## 必读 / 优先扫描线索

### A. 「真正接取」的最小闭环（本期主目标）

对照埃吉尔，列出 NPC23 还缺哪几环：

1. `QuestConfig.json` 有没有第二行？`AcceptQuest` 找不到行会怎样？
2. `Village_QuestOffer_NPC23` Graph：有没有 MultipleChoice「我有些忙 / 好呀」？「好呀」出边有没有 `QuestAcceptAction`？
3. 场景谁 `TriggerStory` 这段对白？（0820 报告预扫 `NpcChair` 误挂埃吉尔——须再对拍）
4. `QuestConfigMgr.Init` 是否启动必跑？新行重启后能否 `GetQuestRow`？

输出一张 **「接取检查清单」**：做完这些，Play 点「好呀」必出 `[Quest] Accept Quest_00x`。

### B. 任务卡 JSON 怎么写（设计草案，不落盘）

现网行字段以 `QuestDataTableRow` / `Quest_001` 为准。侦探须回答：

| 问题 | 要求 |
|------|------|
| 能否直接加第二行、objectiveType 写 `CollectItem`？ | 启动校验 `ValidateTargetMonsters` 会不会把非杀怪行当错误？`AcceptQuest` 是否不看 type？ |
| 目标物字段 | 现网只有 `targetMonster`。采集任务是 **复用该字段填 TenWangFruit**（脏），还是 **必须新增 `targetItem`**？给推荐 + 替代方案 |
| `targetCount` | 5 |
| `rewards` | `{ type: Gold, amount: 50 }`（注意 JSON 里 amount 现网是字符串还是数字） |
| `id` | 第二行用 `"2"`？ |
| 标题 / objectiveText | 给一版中文草案；英日可 OPEN |
| 禁止 | 写成 KillMonster 假行 |

报告里给出 **完整 JSON 草案对象**（可复制给施工员，但侦探自己不改文件）。

### C. 点「好呀」如何接到这张卡

- 仿埃吉尔：Choice「好呀」→（可选）雅尔「好呀！」→ `QuestAcceptAction` → 收尾
- questId 与 JSON 必须逐字一致
- 若批次 A 选项还没插上：施工顺序写清「先 MC，再挂 Accept」，仍算同一张任务卡，不要拆成两个无关项目

### D. 「提交 5 个藤蔓果」如何做（设计，不阻塞接取）

用户口径：**提交 5 个就可以**（以背包数量为准，不是再打 5 只怪）。

侦探须在现网证据上选一个主方案（可附替代）：

| 方案 | 大意 | 预扫利弊 |
|------|------|----------|
| A 交付时查背包 | TurnIn 时 `GetCount(TenWangFruit) ≥ 5` → `TryRemoveMainItem` ×5 → TurnIn + Grant 50 | 接取后进度可一直是 0；Complete 怎么置位？回 NPC 时再判？ |
| B 拾取计数 | 入包时 `OnItemCollected` 累加，满 5 → Complete，再回 NPC 交 | 更像杀怪任务；须改拾取/入包点 |
| C 接取后扫描背包 | Accept 时若已有 ≥5 个果直接 Complete | 可做，但是否符合「去采」叙事 |

钉死：

- `TryRemoveMainItem` 现网是否够用  
- 现网 `QuestTurnInAction` **会不会扣物品**（预扫：不会，只 TurnIn+Grant）  
- Complete 状态谁来写：交的时候写，还是凑满 5 个就写  
- 要不要独立 TurnIn 对白 Prefab（仿 `Village_Aegir_QuestTurnIn`）还是同一 NPC 按状态切图  
- **左侧 UI / 采集刷怪 / 新地图出果：非接取最小集**

### E. 50 金币

- 走现网 `GrantQuestRewards` 即可？须 TurnIn 成功？  
- 金额写在 JSON 50，不要在 Action 里写死 50（对齐埃吉尔改 JSON 发奖）

### F. 不要误伤

- 不改 Quest_001 数值与埃吉尔 Graph  
- 不把成就系统当任务卡  
- 不设计任务表驱动选项按钮  
- CollectItem 运行时可以「下一批」，但 JSON 设计必须一次写对，避免以后改 type

---

## 侦探任务清单

1. **结论一句话**：要真正接取，最少改配置 + 对话图挂 Accept（+ 场景挂对的 Prefab）；交 5 果/50 币是下一环，怎么接现有背包与 TurnIn。  
2. **接取最小闭环检查清单**（用户可照着勾）。  
3. **Quest_002 JSON 草案**（CollectItem / 目标物字段裁定 / Gold 50）。  
4. **Graph 挂载点**：挂在哪句后、questId 填什么。  
5. **提交 5 果主方案**（A/B/C 选一）+ 与接取的批次切分。  
6. **开放问题**追加 OPEN（「NPC23 藤蔓果任务卡 · 2026-08-20」）：  
   - questId 是否 `Quest_002`  
   - 任务中文标题  
   - 交付时查背包 vs 拾取计数  
   - 是否要独立 TurnIn Prefab  
7. **禁止**：改资产；用杀怪行冒充采集卡；把刷果玩法写进接取必做。

---

## 输出要求

写入：`Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（怎样才算真正接取 + 任务卡最小是什么）  
② 原因（生活类比：订单本签字 vs 交货结账）  
③ 用户需要做什么（接取检查清单；交付另列）  
④ 给程序：JSON 草案、字段扩展建议、Accept 调用链、提交方案对比、场景挂接、OPEN

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再贴）

> 默认先只做 **接取最小闭环**（JSON 新行 +「好呀」挂 QuestAcceptAction + 场景 Prefab 名若报告要求）。  
> **扣 5 个藤蔓果 / 发 50 币** 须报告已裁定提交方案后再另开或续跑，不要和接取捆死一次改完采集系统。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Config/QuestConfig/QuestConfig.json
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab

你现在是【施工员】。按报告做「真正接取」最小闭环：玩家在 NPC23 对白点「好呀」后，任务进入 InProgress，Console 出现 [Quest] Accept <questId>。

必须：新增独立 questId，禁止改 Quest_001、禁止用 KillMonster 假行冒充藤蔓果任务；拒选项「我有些忙」不得 Accept；rewards 金额以报告 JSON 草案为准（50 Gold）。
若报告写明须先补 MultipleChoice，一并做。场景挂错 Prefab 则按报告改 StoryPrefabName。
本期不要实现扣果/刷果，除非报告把交付列入本批必做。

提交说明：questId、JSON 字段、Accept 挂在哪、如何验收接取。
```
