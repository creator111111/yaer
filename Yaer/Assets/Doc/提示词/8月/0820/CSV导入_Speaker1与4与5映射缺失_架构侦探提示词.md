# Cursor Agent Prompt · CSV 导入：补 Speaker「1」「4」「5」映射（顺带齐 HomeScene23 数字 Speaker）

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **现象**：Tools → Dialogue → Import CSV 时报  
> `[DialogueCsvGraphBuilder] Speaker「1」（ID 1）未在映射表中找到，导入已中止。`  
> **开发者目标**：需要 **添加 NPC1**，并 **顺便把 4、5 全部加上**（映射表补齐，数字 Speaker 能导入）  
> **本阶段**：只读 + 写溯源报告，**不施工**  
> **同类先例（已落地）**：`2→NPC2`、`3→NPC3`（0820 报告 + 现网 `DialogueSpeakerMapping`）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. 导入报 Speaker「1」找不到，根因是什么？  
2. 要补哪些映射？`1` / `4` / `5` 应对到图内哪个 Actor 名？  
3. 和之前补 `2`/`3` 是否同一修法？改哪两个文件？

### 现场证据

Console（截图）：

| Speaker | 报错 |
|---------|------|
| `1` | ID 1，导入已中止 |

极可能 CSV：`Assets/Dialog/Village_NPC1_对话交互.csv`

| ID | Speaker | 台词（预扫） |
|----|---------|--------------|
| 1 | `1` | 哎呀，你怎么自己就进来了，吓我一跳。 |
| 2 | `雅` | 不好意思。。。（雅已有映射，不应报错） |

另有一批 `Village_NPC1_物品交互_*.csv`——侦探扫一眼 Speaker 列是否也用 `1`。

### 台本语义（0601 HomeScene23）

| CSV Speaker（数字） | 策划称呼 | 图内 Actor 推荐（对齐 2/3 方案） |
|--------------------|----------|----------------------------------|
| `1` | NPC1 | **`NPC1`** |
| `2` | NPC2（孩子） | `NPC2`（✅ 已有） |
| `3` | NPC3（妈妈） | `NPC3`（✅ 已有） |
| `4` | NPC4 | **`NPC4`** |
| `5` | NPC5 | **`NPC5`** |

现网 Prefab 证据预扫：`HomeScene1Npc1.prefab` Actor=`NPC1`；`HomeScene1Npc4.prefab` Actor=`NPC4`。NPC5 须搜现网是否已有 `_keyName`。

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 根因 | 映射表只有 … + `2`/`3`，**没有** `1`/`4`/`5`；安全中止，非 CSV 解析崩 |
| 修法 | 仿 2/3：`CreateDefaultInstance` **与** `DialogueSpeakerMapping_Default.asset` **两处**各补三行 |
| 推荐映射 | `1→NPC1`、`4→NPC4`、`5→NPC5`（方案 A，与 0820 数字 Speaker 一致） |
| 不要 | 改 CSV 把 `1` 写成 `NPC1`（除非侦探证明必须）；本期不做立绘大工程 |
| FaceType | 空 FaceType 默认 / Warning 是否要为 NPC1/4/5 补 —— 记 OPEN 或顺带建议 |

### 现网映射预扫（缺什么）

已有：雅、古、艾米、艾莉、村、埃吉尔、—、**2、3**  
缺失：**1、4、5** ← 本期

### 必读

- `Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md`
- `Assets/Doc/执行文档/6月/0608/CSV导入工具_埃吉尔Speaker映射缺失_架构溯源与执行说明.md`
- `Assets/Doc/执行文档/6月/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md`
- `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs`
- `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset`
- `Assets/Dialog/Village_NPC1_对话交互.csv`（及同目录 NPC1 物品交互 CSV）
- Prefab 对照：`HomeScene1Npc1`、`HomeScene1Npc4`；搜 NPC5 Actor

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md
@Assets/Doc/执行文档/6月/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset
@Assets/Dialog/Village_NPC1_对话交互.csv
@Assets/GameRes/Prefabs/Dialogue/HomeScene1Npc1.prefab
@Assets/GameRes/Prefabs/Dialogue/HomeScene1Npc4.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、映射资产、CSV。只读扫描 + 写溯源报告。

---

## 背景

1. 导入 NPC1 相关 CSV 时 Speaker「1」未映射，导入中止。
2. 开发者要求：加 NPC1，并顺便把 Speaker 4、5 全部补上（HomeScene23 数字 Speaker 一次齐）。
3. 2/3 已按 `2→NPC2`、`3→NPC3` 做过；本期对齐同一套约定。
4. 只查明映射怎么补；不做对话内容/任务系统。

---

## 必查

### A. 报错链路

- `DialogueCsvImportWindow` → Mapping（SO 或 CreateDefaultInstance）
- `TrySetupActorParameters` 未命中 → 中止
- 确认与 2/3、埃吉尔同一机制

### B. CSV 对拍

- `Village_NPC1_对话交互.csv`：Speaker 取值与报错 ID
- 扫 `Village_NPC1_物品交互_*.csv`：是否也用 `1`
- 工程内是否已有含 Speaker `4`/`5` 的 CSV（即使本期未导入，映射也应预留）

### C. Actor 名裁定

| CSV | → Actor | 证据 |
|-----|---------|------|
| 1 | NPC1？ | HomeScene1Npc1 / 0601 |
| 4 | NPC4？ | HomeScene1Npc4 |
| 5 | NPC5？ | 搜 Prefab / 台本 |

禁止推荐恒等 `1`/`4`/`5` 当图内名（与 2/3 报告一致），除非现网 Actor 真叫数字。

### D. 最小施工清单（只建议）

| 文件 | 操作 |
|------|------|
| `DialogueSpeakerMapping.cs` CreateDefaultInstance | 追加 `1→NPC1`、`4→NPC4`、`5→NPC5` |
| `DialogueSpeakerMapping_Default.asset` | 同步三行 |
| HelpBox / 技术文档映射表 | 是否顺手更新 |
| CSV | 默认不改 |

### E. 验收

- Import `Village_NPC1_对话交互.csv`：无 Speaker「1」报错，能生成图
- 回归：NPC23（2/3）、埃吉尔、晚宴 CSV 仍可导入
- （若有）含 4/5 的 CSV 一并试导

---

## 侦探任务

1. **结论一句话**：缺 `1`/`4`/`5` 映射；补 `1→NPC1`、`4→NPC4`、`5→NPC5`（按证据改写）。  
2. **调用链 + CSV 对拍表**。  
3. **现网映射完整表**（含已有 2/3）。  
4. **三行推荐映射 + 证据**。  
5. **最小文件清单 + 验收**。  
6. OPEN：「CSV Speaker 1/4/5 · 2026-08-20」——FaceType 默认；NPC5 立绘是否占位。  
7. **禁止**：改资产；把数字 Speaker 说成解析 bug；扩成全屋对白施工。

---

## 输出

写入：`Assets/Doc/执行文档/0820/CSV导入_Speaker1与4与5映射缺失_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：对照册缺 1/4/5 号昵称）  
③ 用户检查清单（补哪三行、改哪两处）  
④ 程序：映射表现状、推荐表、与 2/3 先例对齐说明、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/CSV导入_Speaker1与4与5映射缺失_架构溯源报告.md
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset

你现在是【施工员】。按报告补 Speaker 映射：1、4、5 须能通过 CSV 导入。

必须：同时更新 CreateDefaultInstance 与 DialogueSpeakerMapping_Default.asset；保留既有 2/3 与其它映射；未命中仍中止；默认不改 CSV；不改运行时 Game 脚本。

提交说明：补了哪三行、Actor 名、如何 Import Village_NPC1_对话交互.csv 验收。
```
