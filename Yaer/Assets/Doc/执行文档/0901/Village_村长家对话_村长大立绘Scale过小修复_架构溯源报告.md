# Village_村长家对话 — 村长大立绘 Scale 过小 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】只读定方案（本报告为溯源与拍板；施工另开【施工员】）  
**Unity**：2020.3.48f1  
**现象**：门口/继续三人对话中，雅/古大立绘正常，**村长缩成背景小人**；Hierarchy `村长/ChiefPainting`；Inspector Scale **0.32**  
**目标 Prefab**：`Village_村长家门口初次对话` · `Village_村长家继续对话`  
**提示词**：`提示词/0901/Village_村长家对话_村长大立绘Scale过小修复_施工员提示词.md`（需求源；本文件为侦探落盘）  
**关联**：0831 村长大立绘丢失（Sprite 空）≠ 本期尺寸问题  

---

## 沟通摘要

### ① 结论一句话

**母体 `ChiefPainting` 默认 Scale=0.32；门口曾有 0.65 Override 但 target 旧 RectTransform fileID 断链失效；继续对话从未写 Scale——两线都回落 0.32。施工应把两对话实例 Scale Override 钉到当前 RT=`795041340282264463` 为 0.65，并让 Door/Continue Setup 的 Nudge 写 Scale 防回潮；勿改雅/古、勿只改 SizeDelta。**

### ② 原因（通俗）

村长立绘母体自带「偏小」默认尺寸。门口对话以前用 Override 放大过，但重建母体后 ID 对不上，放大等于没写。继续对话生成时只挪了位置、没写放大，所以一直很小。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：门口 / 继续 → `ChiefPainting` Scale | **0.65** |
| 2 | Play 三人并排 | 村长与雅/古同为大立绘体量，不再小豆人 |
| 3 | Face1～3 / 前奏三路淡入 | 仍正常 |
| 4 | 重跑 Door/Continue Setup（若做了 A+） | Scale 仍为 0.65 |
| 5 | 雅/古 Scale | 未被改动 |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 对话内目标 Scale | **`(0.65, 0.65, 0.65)`** |
| SizeDelta / Pos | 保持 `1128×2625` · `(420, -120)` 量级；**勿**用改 Size 冒充放大 |
| 母体默认 | **保留 0.32**；仅对话 Override 放大 |
| 方案 | **A** 两 Prefab 实例 Override + **A+** Setup 写 Scale |
| 否 | B 改母体默认；C 只改 SizeDelta；D 改 Canvas Scaler；改雅/古凑齐 |

---

## ② 现网缺口 / 现象

| 项 | 磁盘核实 |
|----|----------|
| 母体 `Assets/Prefabs/DialougeProtrait/ChiefPainting.prefab` 根 RT | Scale **0.32**；fileID **`795041340282264463`**；Size 1128×2625；Pos (420,-120) |
| `ChiefPaintingSetupEditor` | 写死 `localScale = new Vector3(0.32f…)` |
| 门口 Prefab | YAML 曾有 Scale **0.65** Override，但 target=**`7544350478408598713`**（母体**已无**）→ **断链失效** → Play/Inspector 见 0.32 |
| 继续 Prefab | 仅有 AnchoredX=420 Override；**无** LocalScale Override → 继承 0.32 |
| 父节点「村长」 | Scale **1**（H4 否） |
| Dialogue 下其它嵌同一母体 guid 的 Prefab | **仅**门口 + 继续 |

**不是**：Sprite 又空（0831）；不是缺节点。本期是 **尺寸 / Override 断链**。

---

## ③ 根因裁定

| ID | 假说 | 裁定 |
|----|------|------|
| **H1** | 母体默认 0.32 + Setup 写死 | ✅ 背景条件 |
| **H2** | 门口 0.65 Override fileID 断链 → 回落 0.32 | ✅ **门口主因** |
| **H3** | 继续 Setup 只 Nudge X、不写 Scale | ✅ **继续主因** |
| H4 | 父节点缩放过小 | ❌ |
| H5 | SizeDelta 坏 | ❌ |

触发口述「修改了对话之后」变小：重跑 Setup / 生成继续对话 / 重建 `ChiefPainting` → 新 fileID，旧 Override 失效或新图从未写 0.65。

---

## ④ 挂点 / 修复方案

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A** | 门口 + 继续：`ChiefPainting` 实例 `localScale=(0.65,…)`，Override target=**当前** RT fileID | ✅ **必做** |
| **A+** | Door/Continue `NudgePortraitLayout` 补 `TrySetLocalScale(0.65)` | ✅ **推荐**（防菜单回潮） |
| B | 母体默认 0.32→0.65 | ⚠️ 影响其它引用；本期不优先 |
| C | 只改 SizeDelta | ❌ |
| D | Canvas Scaler | ❌ |

**门口注意**：若磁盘仍见 value=0.65 但 Play 仍小 → 查 Override **target fileID** 是否仍为 `795041340282264463`；断链则删旧 Override 或改 target 后重写。

---

## ⑤ 最小施工清单（交施工员）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 门口 Prefab：Scale Override 指向 **`795041340282264463`**，value **0.65** | **P0** |
| 2 | 继续 Prefab：新增同 fileID 的 Scale x/y/z=**0.65** | **P0** |
| 3 | `VillageChiefDoorDialogueSetupEditor` / `VillageChiefContinueDialogueSetupEditor`：`NudgePortraitLayout` 写 Scale=0.65 | **P0**（A+） |
| 4 | 禁止改雅/古 Scale；禁止清空 Sprite / 拆嵌套 | — |
| 5 | 母体默认保持 0.32（Q1） | — |

落盘施工说明（由【施工员】写）：`Assets/Doc/施工说明/0901/…`（侦探不代写）。

---

## ⑥ 验收清单

同沟通摘要 §③。

---

## ⑦ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 母体默认是否改 0.65？ | **否**；仅对话 Override + Setup | ✅ |
| Q2 | Y 是否微调？ | **先只 Scale**；脚切再调 Pos.y | ⏳ |
| Q3 | 晚宴等其它嵌 Chief？ | Dialogue 检索仅门口+继续；晚宴未嵌此母体 | ✅ |

---

## ⑧ 程序补充（速查）

| 锚点 | 值 |
|------|-----|
| 母体 guid | `6d66031c17b442349b23a5d68cfd922a` |
| 当前根 RT fileID | **`795041340282264463`** |
| 断链旧 fileID | `7544350478408598713` |
| 对话目标 Scale | **0.65** |
| 母体默认 Scale | **0.32** |
| Setup 菜单 | `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab` · `…继续对话 Prefab` |

**一句话**：放大靠对话 Override 对准**现行** fileID；Setup 必须把 Scale 写进 Nudge，否则下次菜单又缩回去。
