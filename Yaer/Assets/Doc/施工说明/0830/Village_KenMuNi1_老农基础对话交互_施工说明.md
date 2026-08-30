# Village_KenMuNi1 — 老农基础对话交互 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md`  
**范围**：本期仅基础对话；**不做** Choice / AcceptQuest / 打水任务。

---

## ① 结论一句话

Speaker「老人」已可 Import；`Village_老农打水任务` 对白 Prefab（含 UIAlpha）已落盘；`Objects/Npc_Farmer` 可走近点击播完求助句。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `DialogueSpeakerMapping.cs` + `_Default.asset` | `老人→老人` | Import 映射堵死 |
| `DialogueCsvImportWindow.cs` | HelpBox 补老人 | 文档一致 |
| `Village_老农打水任务.prefab` + Generated | UIAlpha + ID1～15；Actor 雅尔+老人 | 对话真源；防无框 |
| `Village_KenMuNi1.unity` | `Npc_Farmer` 三件套；overlap=1；Z=0；sceneObjs | 合层 `农` 不宜直接挂交互 |

**Npc_Farmer**

- 位：合层 `农` 约 `(-82.6, 2.655, 0)`；可 Scene 微调贴脚  
- `StoryPrefabName=Village_老农打水任务`；可重复；Cursor Chat  
- 合层 `农` **保留**美术  

**未做**：接任务 Choice；**产品明确不要立绘**（见 `施工说明/0830/Village_老农打水任务_取消立绘_施工说明.md`，已拆 GoOut 大立绘）；GSM C#。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 走近 `Npc_Farmer` 点/E | 雅↔老人对白；对话框可见 |
| 2 | 听完末句 | 求助打水；**无**接任务按钮 |
| 3 | 再谈 | 可再播 |
| 4 | Console | 无 Missing；无「老人」映射错 |
| 5 | 回归 | Door_Shop / House_Tree / 其它 NPC |

若点偏：拖 `Npc_Farmer` 对齐合层 `农`。

---

## ④ 给程序

- 下期才加 Choice + AcceptQuest（对照埃吉尔）。  
- 再 Import CSV 须勾「对话框 UI 淡入」，或保留现 Prefab bound。
