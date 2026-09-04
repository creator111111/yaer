# Village_村长家继续对话 — 三人大立绘摆位对齐门口 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_村长家继续对话_三人大立绘摆位对齐门口_架构溯源报告.md`  
**真理源**：`Village_村长家门口初次对话.prefab`  
**被改方**：`Village_村长家继续对话.prefab` + Door/Continue Setup

---

## 沟通摘要

### ① 结论一句话

**续聊雅立绘已改为门口定稿 `(348,52)`，Actor「村长」改为 `(1156,-232)` + Y180；Door/Continue Nudge 同源 `VillageChiefDialoguePortraitLayout`，禁止再写 `-380`。**

### ② 原因（通俗）

续聊生成时雅还停在旧左侧占位，村长父节点也没挪到门口那侧。Setup 若仍写 `-380`，一跑菜单就会把门口冲坏。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：继续三人与门口对照表一致 | |
| 2 | Play：门口戏与进屋续聊站位观感一致 | |
| 3 | 村长 Scale **0.65**；Face/前奏淡入正常 | |
| 4 | 针线包 Tips（GetItem / OpenTips / SaveBag）仍在 | |
| 5 | 重跑 Door **及** Continue Setup 后布局仍=门口定稿 | |

### ④ 程序补充

见下文。

---

## 改前 / 改后对照

| 节点 | 改前（继续） | 改后（=门口） |
|------|--------------|---------------|
| `GoOutStoryYaerPainting` | `(-380, 52)` | **`(348, 52)`** |
| Actor `村长` Pos | `(0, 0)` | **`(1156, -232)`** |
| Actor `村长` Rot | 无翻转 | **Y=180** |
| `GushaPainting` | `(0, -330)` | 同（未改） |
| `ChiefPainting` Scale | `0.65` | 同 |

Tips 图节点：磁盘仍含 `GetItemActionTask` / `OpenTipsFormActionTask` / `SavePlayerBagActionTask`（未走整壳覆盖）。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| Prefab | `Village_村长家继续对话.prefab` | 雅 X Override→348；Actor 村长位姿抄门口 |
| Layout | `VillageChiefDialoguePortraitLayout.cs` | 门口定稿常量 + `ApplyToDialogueRoot` |
| Setup | Door / Continue `NudgePortraitLayout` | 改调 Layout，去掉 `-380` |
| ScaleFix | `VillageChiefPaintingScaleFixEditor` | Scale/X 改读 Layout 常量 |

**未改**：门口 Prefab 定稿数值；CSV；其它对话 Prefab。
