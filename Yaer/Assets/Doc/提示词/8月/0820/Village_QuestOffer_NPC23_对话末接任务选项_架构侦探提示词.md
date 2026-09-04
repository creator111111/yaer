# Cursor Agent Prompt · Village_QuestOffer_NPC23：仿埃吉尔接任务对话末双选项

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab`（开发者**已创建**；Hierarchy 含 Yaer / NPC2 / NPC3）  
> **对照样板**：`Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab`  
> **产品文案（已定）**：  
> - **拒绝接任务**：「我有些忙」  
> - **接受接任务**：「好呀」  
> **台本源**：`Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv`（末句：妈妈请帮忙采 **藤蔓果 ×5**）  
> **本阶段**：只读扫描 + 写溯源报告，**不施工**  
> **范围**：对话末 MultipleChoice + 接受分支如何挂到任务；本期**不**做左侧 UI、不重做立绘、不新建采集任务运行时（除非侦探发现已有可复用）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. `Village_QuestOffer_NPC23` 已经建好（有 NPC2 / NPC3 节点），后面要像埃吉尔那样在对话末弹出「接 / 不接」。  
2. 文案已定：**拒绝 =「我有些忙」**，**接受 =「好呀」**。  
3. 怎么仿 `Village_Aegir_QuestOffer`？现网图里缺什么？施工最小改哪些节点？  
4. 「好呀」之后要不要立刻 `QuestAcceptAction`？挂哪个 `questId`？（现网 `QuestConfig` 只有埃吉尔 `Quest_001` 杀虫）

### Hierarchy 现场（开发者截图）

```
Village_QuestOffer_NPC23
  Canvas (Environment)
  Village_QuestOffer_NPC23
    Yaer
      GoOutStoryYaerPainting
    NPC2
    NPC3
```

说明：Actor 容器已就位（与 Speaker 映射 `2→NPC2`、`3→NPC3` 对齐）。侦探须再扫 Graph：对白是否已 Bind、末尾有无 MultipleChoice、有无 QuestAccept。

### 产品对照表（文案已拍板）

| 角色 | 埃吉尔样板（现网） | NPC23（本期） |
|------|-------------------|---------------|
| 接任务人 | 埃吉尔 | NPC3（妈妈）为主请托；NPC2 孩子开场 |
| 拒绝按钮 | 我还有事 | **我有些忙** |
| 接受按钮 | 我会努力的 | **好呀** |
| 接受后台词（预扫） | 雅尔：「我会努力的！」 | 是否要有雅尔回一句「好呀！」——侦探对照样板建议，记 OPEN |
| 真正接任务 | `QuestAcceptAction(Quest_001)` | 待钉：新 questId？本期仅选项？ |
| 任务目标 | 杀 WoodWorm ×10 | 台本：采藤蔓果 ×5（`TenWangFruit`？） |

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 选项怎么出 | 与埃吉尔相同：Graph 插 **`MultipleChoiceNode`** → `NormalDialogueNewPanel` 克隆按钮；**不是**任务表刷选项 |
| NPC23 现状 | Prefab 已有对白（CSV 导入痕迹）；**很可能还没有**末尾 MultipleChoice / QuestAccept |
| 埃吉尔完整链 | 末句 → MC（拒/接）→ 接：雅尔句 → **QuestAcceptAction** → 收尾淡出 |
| 批次拆分建议 | **批次 A**：只做双选项 + 分支结束（拒直接 END；接可接一句「好呀」或直接 END）  
| | **批次 B**：挂 `QuestAcceptAction` + `QuestConfig` 新行（采集任务若未实现，先 OPEN 或暂只 Accept 占位） |
| 禁止误解 | 不要新建「任务选项 Prefab」；不要改埃吉尔文案；不要把采集进度系统捆进本期溯源的必做项 |

### 已有文档 / 资源（须读并对拍）

- 埃吉尔双选项：`Assets/Doc/执行文档/6月/0608/Village_Aegir_QuestOffer_对话末尾双选项_架构溯源与施工执行说明.md`
- 埃吉尔接取：`Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_接取追踪_架构溯源与施工执行说明.md`
- 选项机制总述：`Assets/Doc/提示词/0804/任务系统_对话末接取选项机制_架构侦探提示词.md`
- Speaker 2/3：`Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md`
- CSV：`Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv`
- Prefab：`Village_QuestOffer_NPC23.prefab`、`Village_Aegir_QuestOffer.prefab`
- 代码：`QuestAcceptAction.cs`、`QuestManager.AcceptQuest`、`DialogueTMPUGUI.OnMultipleChoiceRequest`
- 配置：`Assets/GameRes/Config/QuestConfig/QuestConfig.json`（现仅 Quest_001）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/6月/0608/Village_Aegir_QuestOffer_对话末尾双选项_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_接取追踪_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md
@Assets/Doc/提示词/0804/任务系统_对话末接取选项机制_架构侦探提示词.md
@Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab
@Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv
@Assets/GameRes/Config/QuestConfig/QuestConfig.json
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/QuestAcceptAction.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、配置表。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 开发者已创建 `Village_QuestOffer_NPC23`（Hierarchy：Yaer、NPC2、NPC3）。
2. 对白内容来自椅子孩子第一天 CSV；末句妈妈请雅尔帮忙采 5 个藤蔓果并给报酬。
3. 需要像 `Village_Aegir_QuestOffer` 一样，在对白末弹出双选项：
   - 拒绝：「我有些忙」→ 不接任务，对话结束
   - 接受：「好呀」→ 走接受分支（并尽量对接任务系统）
4. 本期查明：现网缺什么、怎么仿、批次怎么切、questId 怎么定。不施工。

---

## 必读 / 优先扫描线索

### A. 对照样板：埃吉尔 Graph 怎么做的

对 `Village_Aegir_QuestOffer.prefab` 钉死：

1. 哪一句 Statement 之后接 MultipleChoice  
2. 选项 0 / 1 文案与出边（拒 → 哪；接 → 雅尔句 → QuestAcceptAction → 收尾）  
3. Actor 绑在谁身上；收尾有哪些 Action（淡出 / FightingPanel 等）  
4. 选项是手改 Graph 还是 CSV Choice 导入残留  

画出调用链（可复用 0608 图，但必须与现网 Prefab 对拍）。

### B. 目标 Prefab：NPC23 现在有什么、缺什么

对 `Village_QuestOffer_NPC23.prefab` 钉死：

| 检查项 | 有/无 | 锚点（节点 id / 路径） |
|--------|-------|------------------------|
| Actor：雅尔 / NPC2 / NPC3 | | Hierarchy 已见；Graph actorParameters？ |
| 前奏淡入（CanvasGroup / UI Alpha） | | |
| CSV 对白 1～11 是否都在图里 | | 末句是否「我会给你报酬的。」 |
| MultipleChoice | | |
| 「我有些忙」「好呀」文案 | | |
| QuestAcceptAction | | |
| 收尾淡出 / 结束节点 | | |

结论要写清：**只需在末句后插 MC**，还是还要补 Bind / 重导入 / 修 Actor。

### C. 产品文案落地方式

| 按钮 | 文案（已定） | 建议出边行为（侦探可改，须写理由） |
|------|--------------|-----------------------------------|
| 拒绝 | 我有些忙 | → 收尾 END；**不**调 AcceptQuest |
| 接受 | 好呀 | →（可选）雅尔说「好呀！」→（批次 B）QuestAcceptAction → 收尾 |

钉死：

- 选项 index 0/1 谁在前（建议拒在前、接在后，对齐埃吉尔「我还有事 | 我会努力的」顺序，除非有反例）  
- 接受后要不要雅尔复读「好呀」——样板有复读句；NPC23 CSV **没有**这句，记 OPEN 或建议 Graph 手补一句  
- CSV 是否追加 `Type=Choice` 行，还是**只手改 Graph**（埃吉尔现网常见做法）

### D. 与任务系统对接（分批次，勿混）

1. **现网 QuestConfig** 只有 `Quest_001`（杀虫）。藤蔓果采集 **没有**现成 quest 行。  
2. `QuestManager.OnMonsterKilled` 只认 `KillMonster`；采集进度（CollectItem）是否存在——侦探搜代码，没有则写「批次 B 仅 Accept 占位 / 或须新 objectiveType」。  
3. 给出推荐批次：

| 批次 | 交付物 | 验收 |
|------|--------|------|
| A 选项 | Graph：末句→MC「我有些忙」「好呀」；拒 END；接可一句+END | Play 能弹出正确文案；拒不 Accept |
| B 接取 | QuestConfig 新行 + QuestAcceptAction(questId) | Console `[Quest] Accept …` |
| C 采集进度（远期） | 拾取藤蔓果计数 / 交付 | 另立项 |

4. **questId 命名**记 OPEN（如 `Quest_002`），不要擅自写进配置。  
5. 场景侧：谁 `TriggerStory("Village_QuestOffer_NPC23")`？HomeScene23 哪个 NPC？本期可只列「后续挂场景」，不强制查完。

### E. 不要误伤

- 不改 `Village_Aegir_QuestOffer` 文案与节点  
- 不改 Speaker 映射已拍板的 `2→NPC2`、`3→NPC3`（除非发现 Prefab Actor 名不一致）  
- 不做左侧追踪 UI  
- 不把「采集系统未做」说成「选项做不了」——选项是对话能力，接任务是图上多挂 Action

---

## 侦探任务清单

1. **结论一句话**：NPC23 要仿埃吉尔在末句后插 MultipleChoice；文案「我有些忙 / 好呀」；接任务属批次 B + 新 questId（按证据改写）。  
2. **埃吉尔 vs NPC23 对照表**（缺什么一目了然）。  
3. **推荐 Graph 拓扑**（节点顺序 + 出边）；标注哪些手改、是否动 CSV。  
4. **批次 A/B/C** 与验收清单。  
5. **开放问题**追加 OPEN（「NPC23 接任务对话选项 · 2026-08-20」）：  
   - 接受后雅尔是否要说「好呀！」  
   - questId / 采集 objectiveType 何时做  
   - 场景哪个 NPC 挂 `Village_QuestOffer_NPC23`  
6. **禁止**：改资产；扩成完整采集任务 + 交付 UI 大工程；把 QuestConfig 写成负责弹出选项。

---

## 输出要求

写入：`Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比：剧本末页印菜单；点「好呀」才签收订单）  
③ 用户需要做什么（检查清单：先做选项还是连 Accept；改 Graph 哪几步）  
④ 给程序：对照表、推荐拓扑、节点锚点、批次、OPEN

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再贴）

> 默认先只跑 **批次 A（双选项）**。批次 B 须报告已裁定 questId 后再另开或续跑。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab

你现在是【施工员】。按报告批次 A 做最小化修改：在 Village_QuestOffer_NPC23 对白末插入 MultipleChoice。

必须：
- 拒绝按钮文案「我有些忙」→ 不接任务、对话结束
- 接受按钮文案「好呀」→ 按报告出边（可含雅尔一句）
- 仿 Village_Aegir_QuestOffer 机制；不改埃吉尔 Prefab
- 不新建独立「选项 Prefab」；不改运行时任务核心（批次 A 不挂 QuestAccept，除非报告明确批次 A 含 Accept）

提交说明：插在哪句后、选项顺序、拒/接出边接到哪、如何 Play 验收。
```
