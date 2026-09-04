# ChiefPainting Face2/Face3 贴脸偏离修复 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/ChiefPainting_Face2Face3贴脸偏离修复_架构溯源报告.md`  
**现象**：`ChiefPainting` 下 Face2/Face3 完全偏离（飞肩/虚空）

---

## 沟通摘要

### ① 结论一句话

**Face1 Size 已从 `880×2048` 撑回 Mask 同框 `1128×2625`；Setup 对 Face1 强制满框防回潮；Face2/3 坐标未瞎改。**

### ② 原因（通俗）

贴脸坐标是按「整张大底图」算的。底图框被 Setup 写成了图源像素（偏小），闭眼/笑颜还停在大框位置，所以对不上脸。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：只开 Face1 | 底图正常 |
| 2 | 开 Face2（关 Face3） | 闭眼贴脸，不飞 |
| 3 | 开 Face3（关 Face2） | 笑颜贴脸正确 |
| 4 | Face1 Size | **1128×2625** |
| 5 | Face2/3 Pos/Size | 与 Mask 一致（未改） |
| 6 | Play 门口/继续 | 切 Face2/3 不偏离 |
| 7 | 重跑 `Setup Chief Painting` | Face1 仍满框 |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `ChiefPainting.prefab` | Face1 `sizeDelta` → **1128×2625** |
| 2 | `ChiefPaintingSetupEditor.CreateImageLeaf` | Face1 `forceFullFrame` → `FullFrameSize`；Face2/3 仍用 sprite 尺寸 |

**未改**：Face2/3 Pos；Mask 母体；SR；根 Scale；雅/古；门口/继续（无 Face 子 Override，继承母体）。

---

## ② 数值对照

| 节点 | 施工后（对齐 Mask） |
|------|---------------------|
| Face1 Size | `1128 × 2625` |
| Face2 Pos/Size | `(373, 1016)` / `202×76`（保持） |
| Face3 Pos/Size | `(364, 936.5)` / `216×209`（保持） |

---

## ③ 剩余风险

| 风险 | 处置 |
|------|------|
| 肉眼仍差 1～2px（Q3） | 以 Mask 为准微调，再记施工说明 |
| 误重跑旧版 Setup（无 A+） | 已改 Setup；勿回退 `sprite.rect` 写 Face1 |
