# Village_村长家对话 — 村长大立绘 Scale 过小修复 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_村长家对话_村长大立绘Scale过小修复_架构溯源报告.md`  
**现象**：门口/继续三人戏里村长缩成背景小人（Scale 回落母体 0.32）

---

## 沟通摘要

### ① 结论一句话

**门口 + 继续 Prefab 的 `ChiefPainting` 实例 Scale 钉为 0.65（对准现行 RT fileID）；Door/Continue Setup 的 Nudge 写 Scale 防回潮；母体默认仍 0.32。**

### ② 原因（通俗）

母体立绘默认偏小。门口曾经放大过，但重建后 Override 对不上旧 ID，等于没放大。继续对话生成时只挪位置没写放大。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：门口 / 继续 → `ChiefPainting` Scale | **0.65** |
| 2 | Play 三人并排 | 村长与雅/古同为大立绘体量 |
| 3 | Face1～3 / 前奏淡入 | 仍正常 |
| 4 | 重跑 Door/Continue Setup | Scale 仍为 0.65 |
| 5 | 雅/古 Scale | 未被改动 |

菜单（可选重跑）：`Tools / Dialogue / Fix Village 村长立绘 Scale 0.65`

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 / 动作 | 说明 |
|---|-------------|------|
| 1 | `Village_村长家门口初次对话.prefab` | `ChiefPainting` Scale Override → **0.65**（现行 RT） |
| 2 | `Village_村长家继续对话.prefab` | 同上 |
| 3 | `VillageChiefDoorDialogueSetupEditor` | `NudgePortraitLayout` → `TrySetLocalScale(0.65)` |
| 4 | `VillageChiefContinueDialogueSetupEditor` | 同上 |
| 5 | `VillageChiefPaintingScaleFixEditor.cs`（新建） | 一键钉 Scale；可 `Library/ChiefPaintingScaleFix.request` |

**未改**：母体默认 0.32；雅/古 Scale；SizeDelta；Canvas Scaler；Sprite/Face。

---

## ② 数值钉死

| 项 | 值 |
|----|-----|
| 对话内 Scale | `(0.65, 0.65, 0.65)` |
| 母体默认 | `0.32`（保留） |
| 现行 RT fileID | `795041340282264463` |
| 断链旧 fileID | `7544350478408598713`（勿再指向） |

---

## ③ 剩余风险

| 风险 | 处置 |
|------|------|
| 再跑 Setup 冲掉 | Door/Continue Nudge 已写 Scale；仍回潮则再点 Fix 菜单 |
| 脚切不准（Q2） | 本期只 Scale；需再调再开 Pos.y |
