# ChiefPainting Face2/Face3 贴脸偏离 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】只读定方案（**禁止改代码 / Prefab**；施工另开【施工员】）  
**Unity**：2020.3.48f1  
**现象**：`ChiefPainting` 下 **Face2 / Face3 完全偏离**（Hierarchy 红箭头指 Face2）  
**对齐真理**：① `ChiefMaskPainting.prefab`（UI 叠法）② `精灵村长游戏中立绘.prefab`（SR：组 2 / 闭眼 / 笑颜）  
**修复目标（施工）**：`ChiefPainting.prefab`（门口/继续若有 Face 子 Override 再同步）  
**提示词**：`提示词/0901/ChiefPainting_Face2Face3贴脸偏离修复_施工员提示词.md`  
**关联**：Scale 0.65 站位另案；本期只叠脸，勿混改根 Scale  

---

## 沟通摘要

### ① 结论一句话

**Face2/3 的坐标数字已与 Mask 一致，但 ChiefPainting 的 Face1 底图 Size 是 `880×2048`（sprite.rect），Mask 是满框 `1128×2625`——底图缩了、贴脸仍按满框偏移，看起来就像完全飞脸。施工把 Face1 Size（及 Pivot/Anchor）抄齐 Mask，并改 Setup 强制 Face1=`1128×2625` 防回潮。**

### ② 原因（通俗）

贴脸坐标是按「整张大底图」算的。大立绘底图框被 Setup 写成了图源像素大小（偏小），闭眼/笑颜还停在大框上的位置，所以对不上脸。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：只开 Face1 | 底图正常 |
| 2 | 开 Face2（关 Face3） | 闭眼贴脸，不飞肩/虚空 |
| 3 | 开 Face3（关 Face2） | 笑颜贴脸正确 |
| 4 | Face1 Size | 与 Mask 一致 **1128×2625** |
| 5 | Face2/3 Pos/Size | 与 Mask 一致（或说明 ≤2px 微调） |
| 6 | Play 门口/继续 | 村长句切 Face2/3 不偏离 |
| 7 | 重跑 Setup Chief Painting（若 A+） | 贴脸仍正确 |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 主因 | **H1**：Face1 SizeDelta 未满框 |
| 回潮源 | **H3**：`CreateImageLeaf` 用 `sprite.rect.size` 写 Face1 |
| 方案 | **A** 母体对表抄 Mask + **A+** Setup 强制 Face1=`1128×2625` |
| Face2/3 Pos | 已与 Mask / SR 公式一致 → **勿瞎改猜坐标** |
| 对话实例 | 门口/继续 **无** Face1/2/3 子 Rect Override → 修母体即可继承 |
| 禁止 | 改 Mask 主修；改 SR 世界坐标；改雅/古；把 Mask 根 Scale(0.18) 抄进大立绘 |

---

## ② 三表对照（磁盘核实 2026-09-01）

### 叠法语义

| 节点 | 图 | 角色 |
|------|-----|------|
| Face1 | `组 2.png` | 底图全身（常开） |
| Face2 | `闭眼.png` | 贴脸（与 Face3 互斥） |
| Face3 | `笑颜.png` | 贴脸 |
| 脚本 | `ChiefMaskPainting.Apply` | 大立绘 Prefab 复用同一脚本 |

### UI 对照

| 节点 | **ChiefMaskPainting**（真理） | **ChiefPainting**（现网） | 差 |
|------|------------------------------|---------------------------|----|
| Face1 Pos | `(0, 0)` | `(0, 0)` | 同 |
| Face1 Size | **`1128 × 2625`** | **`880 × 2048`** | ⚠️ **主差** |
| Face2 Pos | `(373, 1016)` | `(373, 1016.00006)` | 同量级 |
| Face2 Size | `202 × 76` | `202 × 76` | 同 |
| Face3 Pos | `(364, 936.5)` | `(364, 936.4999)` | 同量级 |
| Face3 Size | `216 × 209` | `216 × 209` | 同 |
| 根 Size | `1128 × 2625` | `1128 × 2625` | 同 |
| Anchor/Pivot | 中心 0.5 | 中心 0.5 | 同 |
| preserveAspect | 1 | 1 | 同 |
| 根 Scale | **0.18**（Mask 小窗） | **0.32**（大立绘母体；对话可 Override 0.65） | **勿互抄** |

### SR 源（公式复核）

| 节点 | localPosition |
|------|----------------|
| 组 2（Body） | `(5.77, 13.160001)` |
| 闭眼 | `(9.5, 23.320002)` |
| 笑颜 | `(9.41, 22.525)` |

```
PPU = 100
Face2 UI = (闭眼 - Body) * 100 = (373, 1016)
Face3 UI = (笑颜 - Body) * 100 = (364, 936.5)
```

两 Setup 常量 `BodyLocal` / `Face2Local` / `Face3Local` / `PPU` **一致**；与 Mask Face2/3 磁盘值吻合。

---

## ③ 根因裁定

| ID | 假说 | 裁定 |
|----|------|------|
| **H1** | Face1 Size 未满框 → Face2/3 相对底图错位 | ✅ **主因** |
| H2 | Face2/3 Pos 被手改坏 | ❌ 数已对齐 Mask |
| **H3** | Setup `CreateImageLeaf` 用 `sprite.rect.size` 写 Face1 | ✅ **回潮源**；Mask Face1 满框系事后手调/另修，Setup 同源仍会写 sprite 尺寸 |
| H4 | Pivot/Anchor 不一致 | ❌ 均为中心 |
| H5 | 对话 Prefab Override Face 子节点 | ❌ 门口/继续对 Face1/2/3 RT fileID **refs=0** |

**机制一句话**：贴脸 AnchoredPosition 相对父中心、按「满框底图」标定；Face1 画布变小后，同一坐标相对可见底图就飞了。

---

## ④ 挂点 / 修复方案

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A** | `ChiefPainting` Face1/2/3 的 Pos/Size/Pivot/Anchor **逐项抄齐** Mask；Face2/3 用 SR 公式复核 | ✅ |
| **A+** | `ChiefPaintingSetupEditor.CreateImageLeaf`：对 **Face1** 强制 `sizeDelta = (1128, 2625)`（或等于根框），勿只信 `sprite.rect.size`；Face2/3 仍可用 sprite 尺寸 | ✅ |
| B | 只手挪 Face2/3 猜坐标 | ❌ |
| C | 改 SR Transform | ❌ |

**优先序**：先修 Face1 满框 → 再肉眼验 Face2/3（通常无需再动 Pos）。

**与 Scale 案边界**：根 Scale（0.32 / 对话 0.65）另案；本期只动 Face 子 Rect，禁止把 Mask 的 0.18 抄进大立绘根。

---

## ⑤ 对话实例

| Prefab | Face 子 Override | 动作 |
|--------|------------------|------|
| `Village_村长家门口初次对话` | 无 | 修母体即可 |
| `Village_村长家继续对话` | 无 | 修母体即可 |

若施工后实例仍歪，再查是否新建了子 Override；本期磁盘无此负担。

---

## ⑥ 最小施工清单（交施工员）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `ChiefPainting.prefab`：Face1 `sizeDelta` → **1128×2625**（对齐 Mask）；核对 Face2/3 Pos/Size 已同则不动 | **P0** |
| 2 | `ChiefPaintingSetupEditor`：Face1 强制满框尺寸（A+） | **P0** |
| 3 | 可选：Mask Setup 同改防日后重跑 Mask 回潮（**非本期必做**；Q2 默认不重跑 Mask） | P1 |
| 4 | 禁止改 Sprite guid / 清空贴图；禁止改 SR；禁止改雅/古 | — |
| 5 | 施工说明由【施工员】写到 `施工说明/0901/` | — |

---

## ⑦ 验收清单

同沟通摘要 §③。

---

## ⑧ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | Face1 强制 1128×2625 是否拉伸？ | 对齐 Mask；`preserveAspect` 保持与 Mask 一致（现均为 1） | ✅ |
| Q2 | 是否同步重跑 Mask Setup？ | **否**；Mask 是参考真理 | ✅ |
| Q3 | 肉眼仍差 1～2px？ | 以 Mask 为准微调，写入施工说明 | ⏳ |

---

## ⑨ 程序补充（速查）

| 锚点 | 说明 |
|------|------|
| `ChiefMaskPainting.prefab` | UI 叠法真理 |
| `精灵村长游戏中立绘.prefab` | SR 偏移源 |
| `ChiefPainting.prefab` | 本期修目标 |
| `ChiefPaintingSetupEditor.CreateImageLeaf` | L132 `rt.sizeDelta = sprite.rect.size` → Face1 回潮点 |
| `ChiefMaskPainting.Apply` | Face1 常开；Face2/3 互斥 |
| 公式 | `(FaceLocal - BodyLocal) * 100` |

**一句话**：先把大立绘 Face1 撑回与 Mask 同框，贴脸坐标不用瞎挪。
