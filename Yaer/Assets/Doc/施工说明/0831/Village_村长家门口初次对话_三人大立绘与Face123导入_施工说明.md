# Village_村长家门口初次对话 — 三人大立绘 + Face123 Import — 施工说明

**日期**：2026-08-31  
**依据**：`执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md`（C1）  
**Unity**：须在编辑器执行菜单以落盘 Prefab（本说明对应代码已合入）

---

## ① 结论

已按 **C1 村长行分流**（对齐店）接线：Import 认「村」行 **Face1～3** → `UseChiefPortrait`+`ChiefFace`；运行时 TMP 双驱大立绘+Mask，并加 `chiefMaskActive` 防 Invoke(None) 黑块。  
成品 Prefab / UI `ChiefPainting` 请在 Unity 跑菜单一键生成。

---

## ② 原因（改动意图）

| 卡点 | 处理 |
|------|------|
| Import「Face3 非法」 | Parser：村长行走 `ChiefCsvDefaults`，不再误判为 DialogueFaceType |
| 门口 CSV 已是 Face1～3 | GraphBuilder 写 BB，不经晚宴 Smile→Face3 映射 |
| 大立绘仍 SR | Setup 菜单重建 UI `ChiefPainting`（脚本复用 `ChiefMaskPainting`） |
| Mask 与大立绘同脚本 | TMP 只在 `DialogueSceneContainer` 下找大立绘 |

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Unity：`Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab` | |
| 2 | Console 无 Face3 非法；存在 `…/Village_村长家门口初次对话.prefab` 与 Generated `.asset` | |
| 3 | Prefab 内可见 Yaer + Gusha + ChiefPainting；BB 三路 CanvasGroup | |
| 4 | 村句节点 `UseChiefPortrait=true`，ChiefFace=Face1/2/3 | |
| 5 | Play（可 DialogDebug 或场景 Trigger）村句大立绘+Mask 同脸 | |
| 6 | 雅/古句仍 DialogueFaceType；店 Face1～5 回归 | |
| 7 | 播到「快进屋」无 Missing / 空窗 | |

**场景 Trigger**（`Objects/Npc_Chief` 黑幕）见靠近村长报告，**本期菜单未建**。

---

## ④ 程序清单

### 新增

| 路径 | 说明 |
|------|------|
| `Editor/.../ChiefCsvDefaults.cs` | （已有）Actor「村长」/ Speaker「村」；Face1～3 |
| `Editor/.../VillageChiefDoorDialogueSetupEditor.cs` | 一键：UI 大立绘 + 拷壳 + Import |
| `…/Painting/Editor/ChiefPaintingSetupEditor.cs` | SR→UI `ChiefPainting.prefab` |
| 本施工说明 | |

### 修改

| 路径 | 说明 |
|------|------|
| `DialogueCsvParser.cs` | 村长行 Face1～3 / 晚宴枚举分流 |
| `DialogueCsvGraphBuilder.cs` | `BuildChiefPortraitMap` + `UseChiefPortrait` |
| `StatementNodeEx` / `SubtitlesRequestInfoEx` | `UseChiefPortrait` + `ChiefFace` |
| `DialogueTMPUGUI` | `UseChiefPortrait` → 大立绘 Apply + Mask + None |
| `NormalDialogueFormNewLogic` | `FindInDialogueScene<T>` |
| `DialogueMaskAvatarPresenter` | `chiefMaskActive` |
| `DialogueCsvImportWindow` | 文案：村填 Face1～3 |
| `DialogueFaceTypeCsvDefaults` | 村长空列 Warning 改写 |

### 勿动

- 商人 Body×Face / 店 Face1～5  
- 晚宴 CSV 全文（仍可 Smile→MapToChiefFace）  
- Face1 进全局 `DialogueFaceType`

---

## 菜单（Unity）

1. **`Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab`**（推荐，已含 UI 大立绘）  
2. 可选单项：`Tools / Dialogue / Setup Chief Painting (UI Big Portrait)`  
3. 仅校对图：`Tools / Dialogue / Import CSV` → 选门口 CSV（村行应通过）

---

## 验收对照报告 §G

- [ ] Import 无 Face3 非法；Generated + 成品 Prefab  
- [ ] 三立绘可淡入  
- [ ] 村句 Face1/2/3 大+Mask 一致  
- [ ] 雅/古 / 店 回归  
- [ ] 淡入不误伤 Mask  
- [ ] Play 至「快进屋」
