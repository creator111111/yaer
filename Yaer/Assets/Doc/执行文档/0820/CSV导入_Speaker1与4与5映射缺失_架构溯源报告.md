# CSV 导入 — Speaker「1」「4」「5」映射缺失 — 架构溯源报告

**文档性质**：架构侦探产出（只读溯源 + 最小施工建议；**本阶段不改资产**）  
**日期**：2026-08-20  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- 提示词：`Assets/Doc/提示词/0820/CSV导入_Speaker1与4与5映射缺失_架构侦探提示词.md`
- 同类先例：`Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md`（`2→NPC2`、`3→NPC3` **已落地**）
- 台本：`Assets/Doc/执行文档/6月/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md`
- 映射：`DialogueSpeakerMapping.cs`、`DialogueSpeakerMapping_Default.asset`
- 现场 CSV：`Assets/Dialog/Village_NPC1_对话交互.csv`

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**导入报 Speaker「1」是因为映射表缺数字简称；与 2/3 同一修法：在内置默认 + Default.asset 两处补 `1→NPC1`、`4→NPC4`、`5→NPC5`，一次齐 HomeScene23 数字 Speaker。**

---

## ② 原因（生活类比）

快递单上写了昵称「1」，对照册里只有「2」「3」和雅/古等，没有「1」（以及预留的「4」「5」），系统整批拒收——**安全中止**，不是 CSV 列坏了。

物品交互 CSV（土豆等）Speaker 是 **「雅」**，已有映射，**不会**因缺 `1` 报错；本期卡点是 **`Village_NPC1_对话交互.csv` ID1 的 Speaker=`1`**。

---

## ③ 用户需要做什么（检查清单）

### 必改两处（与 2/3、埃吉尔一致）

| # | 文件 | 操作 |
|---|------|------|
| 1 | `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs` | `CreateDefaultInstance()` 追加三行 |
| 2 | `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset` | Entries 同步三行 |

```csharp
// HomeScene23 屋内数字 Speaker：对齐 2→NPC2 / 3→NPC3；图内名与 HomeScene1Npc1/4 及 0601 台本一致
new Entry { csvSpeaker = "1", actorParameterName = "NPC1" },
new Entry { csvSpeaker = "4", actorParameterName = "NPC4" },
new Entry { csvSpeaker = "5", actorParameterName = "NPC5" },
```

建议顺手：`DialogueCsvImportWindow` HelpBox 内置默认摘要补上 `1→NPC1、4→NPC4、5→NPC5`；更新类注释「九条→十二条」。

### 默认不改

- CSV（继续写数字 `1`/`4`/`5`）
- 运行时 `Assets/Scripts/Game/**`
- 立绘大工程

### 验收

1. Import `Village_NPC1_对话交互.csv` → **无** Speaker「1」报错；图含 Actor **NPC1、雅尔**  
2. 回归：椅子孩子（2/3）、埃吉尔、晚宴仍可导入  
3. （预留）日后含 Speaker `4`/`5` 的 CSV 应能过映射（现网 Dialog 目录尚无此类行）

---

## ④ 给程序看的补充

### 4.1 报错链路（与 2/3 相同）

```
DialogueCsvImportWindow
  → Mapping（拖 SO 或 CreateDefaultInstance）
  → DialogueCsvGraphBuilder.TrySetupActorParameters
  → TryResolve 失败 → Speaker「{n}」（ID …）未在映射表中找到，导入已中止
  → 销毁半成品，return null
```

### 4.2 CSV 对拍

**`Village_NPC1_对话交互.csv`**

| ID | Speaker | 台词摘要 | 映射 |
|----|---------|----------|------|
| 1 | `1` | 哎呀，你怎么自己就进来了… | ❌ 缺 → 与 Console ID1 一致 |
| 2 | `雅` | 不好意思。。。 | ✅ → 雅尔 |

**`Village_NPC1_物品交互_*.csv`（6 份）**  
Speaker 均为 **`雅`**（非 `1`）。补 `1` 映射**不阻塞**这些表；补齐后也不冲突。

**Speaker `4`/`5`**  
`Assets/Dialog/*.csv` 现网**尚无**此类行；按开发者要求与 0601 §1.4（NPC4/NPC5 同台）**预留映射**，避免下次导入再中止。

### 4.3 现网映射完整表

| CSV Speaker | 图内 Actor | 状态 |
|-------------|------------|------|
| 雅 | 雅尔 | ✅ |
| 古 | 古莎 | ✅ |
| 艾米 / 艾莉 | 恒等 | ✅ |
| 村 | 村长 | ✅ |
| 埃吉尔 | 埃吉尔 | ✅ |
| — | 旁白 | ✅ |
| **2** | **NPC2** | ✅ 0820 已补 |
| **3** | **NPC3** | ✅ 0820 已补 |
| **1** | **NPC1** | ❌ **本期** |
| **4** | **NPC4** | ❌ **本期（预留）** |
| **5** | **NPC5** | ❌ **本期（预留）** |

`CreateDefaultInstance` 与 `DialogueSpeakerMapping_Default.asset` 现网对 2/3 **已同步**；补 1/4/5 须再两处同步。HelpBox 文案目前只写到 2/3，施工时可更新。

### 4.4 Actor 名裁定（方案 A）

| CSV | → Actor | 证据 |
|-----|---------|------|
| `1` | **`NPC1`** | 0601 §1.1；`HomeScene1Npc1.prefab` `_keyName`/`_actorName`=NPC1 |
| `4` | **`NPC4`** | 0601 §1.4；`HomeScene1Npc4.prefab` =NPC4 |
| `5` | **`NPC5`** | 0601 §1.4 说话人 NPC5；**尚无**独立 Dialogue Prefab，命名与 1～4 同一约定 |

**不选**：恒等 `1`/`4`/`5` 当图内名；改 CSV 写成 `NPC1`（开发者要的是补映射）。

### 4.5 FaceType

空 FaceType 走 `DialogueFaceTypeCsvDefaults` 通用 Warning + Normal；NPC1 对话行已填 `Awkward`。  
可为 NPC1/4/5 加专支减 Warning（可选，与埃吉尔早期同）；**不阻塞导入**。记 OPEN。

### 4.6 最小施工清单

| 文件 | 操作 | 必做？ |
|------|------|--------|
| `DialogueSpeakerMapping.cs` | +3 行；注释对齐 2/3 | **必做** |
| `DialogueSpeakerMapping_Default.asset` | +3 行 | **必做** |
| `DialogueCsvImportWindow.cs` HelpBox | 摘要补 1/4/5 | 建议 |
| `DialogueFaceTypeCsvDefaults.cs` | NPC1/4/5→Normal | 可选 |
| CSV | **不改** | — |

### 4.7 开放问题（已记入 OPEN）

| ID | 问题 | 施工默认 |
|----|------|----------|
| Q1 | FaceType 是否为 NPC1/4/5 加默认？ | **可选**；空列 Warning+Normal 可接受 |
| Q2 | NPC5 立绘本期是否占位？ | **否**；映射与字幕不阻塞 |
| Q3 | 是否继续允许数字 Speaker？ | **本期允许**（与 2/3 一致）；新台本可另议写 `NPC1` 恒等 |

---

## 5. 相关文档

| 主题 | 路径 |
|------|------|
| 本提示词 | `Assets/Doc/提示词/0820/CSV导入_Speaker1与4与5映射缺失_架构侦探提示词.md` |
| Speaker 2/3 | `Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md` |
| 屋内台本 | `Assets/Doc/执行文档/6月/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md` |

---

## 6. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-08-20 | 初版：缺 1/4/5；裁定 NPC1/4/5；与 2/3 同修法 |

**文档路径**：`Assets/Doc/执行文档/0820/CSV导入_Speaker1与4与5映射缺失_架构溯源报告.md`
