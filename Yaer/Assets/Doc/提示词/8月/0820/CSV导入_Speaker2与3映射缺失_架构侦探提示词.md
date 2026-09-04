# Cursor Agent Prompt · CSV 导入：Speaker「2」「3」未在映射表中找到

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **现象**：Tools → Dialogue → Import CSV 时 Console 连续报  
> `Speaker「2」（ID …）未在映射表中找到，导入已中止。`  
> `Speaker「3」（ID …）未在映射表中找到，导入已中止。`  
> **开发者目标**：需要**增加两个 Speaker：2 和 3**（补映射，让该 CSV 能导入）  
> **本阶段**：只读扫描 + 写溯源报告，**不施工**  
> **同类先例**：0601 艾米/艾莉/村；0608 埃吉尔 / 旁白

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. 导入 CSV 时报 Speaker「2」「3」找不到映射，**根因是什么**？  
2. 要补哪两行映射？CSV 里的 `2` / `3` 应对到图内哪个 **Actor 名**？  
3. 以前同类（缺 Speaker）是怎么修的？这次改哪些文件、改完怎么验收？

### 现场证据（截图 Console）

`[DialogueCsvGraphBuilder]` 报错（导入已中止），至少包含：

| Speaker | 报错 ID（行） |
|---------|----------------|
| `3` | 2 |
| `2` | 3 |
| `3` | 4、5、7、9、10、11 |

与样例 CSV 行对得上（侦探须再对拍一次文件内容）：

**极可能文件**：`Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv`

| ID | Speaker | 台词摘要（预扫） |
|----|---------|------------------|
| 1 | `2` | 妈妈，有外人！ |
| 2 | `3` | 是谁呀？ |
| 3 | `2` | 是怪人！她还长角！！ |
| 4～5、7、9～11 | `3` | 妈妈侧台词（含请帮忙采藤蔓果） |
| 6、8 | `雅` | 雅尔（映射已有，不应报错） |

台本语义（来自 `0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md` §1.2）：

- **Speaker `2`** ≈ 屋里 **NPC2（孩子）**  
- **Speaker `3`** ≈ 屋里 **NPC3（妈妈）**  
- **Speaker `雅`** → 已有映射 **雅尔**

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 根因 | `DialogueSpeakerMapping`（SO 或 `CreateDefaultInstance`）**没有** `2`、`3` 两条；导入器在 `TrySetupActorParameters` 未命中后**有意中止** |
| 不是 | CSV 列坏了；不是「数字被当成 Actor ID」；不是运行时对话 UI 坏了 |
| 修法方向 | 仿 0608：在 **内置默认** + **`DialogueSpeakerMapping_Default.asset`** 各补两行；Actor 名须侦探钉死（见开放问题） |
| 非本期 | 改 CSV 把 `2`/`3` 改成中文名（除非侦探证明映射方案不可行）；做立绘资源；挂任务 `QuestAcceptAction` |

### 已有文档 / 代码（须读并对拍）

- 先例施工：`Assets/Doc/执行文档/6月/0601/CSV导入工具_Speaker映射扩展_施工执行说明.md`
- 先例溯源：`Assets/Doc/执行文档/6月/0608/CSV导入工具_埃吉尔Speaker映射缺失_架构溯源与执行说明.md`
- 台本：`Assets/Doc/执行文档/6月/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md`（NPC2/NPC3）
- 映射类：`Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs`
- 默认 SO：`Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset`
- 建图：`Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs`（报错文案出处）
- 窗口：`Assets/Editor/Tool/Dialogue/DialogueCsvImportWindow.cs`
- CSV：`Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv`
- FaceType 默认（若有）：`DialogueFaceTypeCsvDefaults.cs`（新 Actor 空 FaceType 会不会 Warning）

### 当前映射表预扫（缺什么）

内置默认 / Default.asset 现网大致已有：

| CSV Speaker | Actor | 状态 |
|-------------|-------|------|
| 雅 | 雅尔 | ✅ |
| 古 | 古莎 | ✅ |
| 艾米 / 艾莉 / 村 / 埃吉尔 / — | … | ✅ |
| **`2`** | **？** | ❌ 本次 |
| **`3`** | **？** | ❌ 本次 |

**Actor 名候选（侦探必须裁定一项，或记 OPEN）**：

| 方案 | `2` → | `3` → | 利弊预扫 |
|------|--------|--------|----------|
| A | `NPC2` | `NPC3` | 与 0601 台本称呼一致；立绘/图集名待查 |
| B | `孩子` | `妈妈` | 可读；须确认工程内无同名 Actor 冲突 |
| C | 恒等 `2`/`3` | 恒等 | 能导入，但图内 Actor 名难看、后续立绘难绑 |
| D | CSV 改写中文名再映射 | — | 改策划文件；非「只加 Speaker」首选 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/6月/0601/CSV导入工具_Speaker映射扩展_施工执行说明.md
@Assets/Doc/执行文档/6月/0608/CSV导入工具_埃吉尔Speaker映射缺失_架构溯源与执行说明.md
@Assets/Doc/执行文档/6月/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset
@Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvImportWindow.cs
@Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、映射资产。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 开发者要导入「椅子孩子 / NPC2·NPC3」第一天对话 CSV。
2. Console 报 Speaker「2」「3」不在映射表，导入整批中止。
3. 明确需求：**增加两个 speaker 2 和 3**（补映射），让导入能过。
4. 本期只要查明根因、钉死映射目标名、列出最小改动清单与验收步骤。**不施工**。

---

## 必读 / 优先扫描线索

### A. 报错链路（钉死「为什么中止」）

- `DialogueCsvImportWindow` 取映射：拖入 SO vs `CreateDefaultInstance()`
- `DialogueCsvGraphBuilder.TrySetupActorParameters`：`TryResolve` 失败 → 收集错误 → **销毁半成品、return null**
- 报错文案是否仍是：`Speaker「{speaker}」（ID {id}）未在映射表中找到，导入已中止。`
- 确认：这是**安全中止**，不是 CSV 解析崩了

### B. 现场 CSV 对拍

- 打开 `Village_NPC23椅子孩子第一天对话.csv`，列出所有出现过的 Speaker 取值与 ID
- 与 Console 报错 ID 一一对应
- 写清：哪些 Speaker 已能解析（如 `雅`）、哪些不能（`2`/`3`）

### C. 现网映射表完整清单

对照读：

1. `DialogueSpeakerMapping.CreateDefaultInstance()`  
2. `DialogueSpeakerMapping_Default.asset`  

两者是否一致？窗口没拖 SO 时走哪条？补映射时是否**两处都要改**（对齐 0608 埃吉尔先例）？

### D. `2` / `3` 应对到哪个 Actor 名（本期核心裁定）

- 搜工程：已有对话 Prefab / Actor 参数是否已有 `NPC2`、`NPC3`、`孩子`、`妈妈` 等
- 搜立绘 / Avatar / DialogueRole：有无对应图集或占位
- `DialogueFaceTypeCsvDefaults`：新 Actor 空 FaceType 默认什么；会不会导入后立绘全空
- 给出**推荐映射一行表**（CSV Speaker → actorParameterName），并写清推荐理由
- 若证据不足：记 OPEN，不要擅自改 CSV 数字为中文

### E. 最小施工清单（只建议，不改）

仿 0601 / 0608，列出：

| 文件 | 建议操作 |
|------|----------|
| `DialogueSpeakerMapping.cs` CreateDefaultInstance | 追加 `2`、`3` 两条 |
| `DialogueSpeakerMapping_Default.asset` | 同步两条 |
| HelpBox / 技术文档映射表 | 是否顺手更新 |
| `DialogueFaceTypeCsvDefaults` | 是否需要为新 Actor 加默认表情 |
| CSV 本身 | 默认**不改**（除非方案 D） |

禁止扩 scope：运行时 `Assets/Scripts/Game/**`、任务接取、新做立绘资源（可记「后续」）。

### F. 验收建议（施工后由人测）

- 再 Import 同一 CSV：Console **无** Speaker「2」「3」报错；能生成 DialogueTree / Prefab
- 图内 actorParameters 含新 Actor；`雅` 仍为雅尔
- 旧 CSV（村内雅古、埃吉尔、晚宴）回归导入不炸

---

## 侦探任务清单

1. **结论一句话**：根因是映射表缺 `2`/`3`；推荐补哪两行（CSV → Actor）。
2. **调用链**：Import 窗口 → Mapping → TrySetupActorParameters → 中止。
3. **对拍表**：CSV 行 ID ↔ Console 报错 ↔ Speaker。
4. **现网映射完整表** + 缺项。
5. **推荐 Actor 名**（A/B/C/D 选一或记 OPEN）及对 FaceType/立绘的影响。
6. **最小文件清单** + 验收步骤（只建议）。
7. **开放问题**追加 OPEN（「CSV Speaker 2/3 映射 · 2026-08-20」）：
   - Actor 最终叫 `NPC2`/`NPC3` 还是中文名？
   - 立绘图集本期是否占位？
   - 是否允许策划以后继续用数字 Speaker，还是应规范成简称？
8. **禁止**：改资产；把数字 Speaker 说成「解析 bug」若实际是映射缺失；扩成整屋 NPC 立绘大工程。

---

## 输出要求

写入：`Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比：快递单昵称没进姓名册 → 整批拒收）  
③ 用户需要做什么（补哪两行、改哪两个文件、如何再点 Import 验收）  
④ 给程序：调用链、映射表现状、推荐 `2`/`3` → Actor、FaceType 注意点、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset

你现在是【施工员】。按报告做最小化修改：让 Speaker「2」「3」能通过 CSV 导入映射。

必须：同时更新 CreateDefaultInstance 与 DialogueSpeakerMapping_Default.asset；保留既有映射；未命中仍中止导入；不改运行时 Game 脚本；默认不改 CSV 数字 Speaker（除非报告裁定方案 D）。

提交说明：补了哪两行映射、Actor 名是什么、如何 Import 验收。
```
