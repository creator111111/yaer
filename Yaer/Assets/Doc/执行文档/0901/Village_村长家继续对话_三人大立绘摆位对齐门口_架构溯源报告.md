# Village_村长家继续对话 — 三人大立绘摆位对齐门口 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】→ 施工已落地（见 `施工说明/0901/Village_村长家继续对话_三人大立绘摆位对齐门口_施工说明.md`）  
**Unity**：2020.3.48f1  
**产品**：`Village_村长家继续对话` 的雅/古/村长大立绘布局 **必须与** `Village_村长家门口初次对话` **一致**  
**真理源**：门口 Prefab（已手调定稿）  
**被改方（施工）**：继续对话 Prefab + Door/Continue Setup（防回潮）  
**提示词**：`提示词/0901/Village_村长家继续对话_三人大立绘摆位对齐门口_施工员提示词.md`  

---

## 沟通摘要

### ① 结论一句话

**续聊相对门口的主差是：雅立绘 X 仍为 Setup 的 `-380`（门口已定稿 `348`），以及 Actor「村长」续聊停在 `(0,0)` 无翻转，而门口是 `(1156,-232)` + RotY=180。须整树抄门口（Painting + 村长 Actor），并把两 Setup 的 Nudge 改成门口定稿，禁止再写死 `-380`。**

### ② 原因（通俗）

续聊是用同一套 Setup 生成的，雅还停在旧的左侧占位；门口后来手调过，两人站位已经对过齐。  
村长父节点在续聊里没搬到门口那一侧，只改立绘子节点也会漂。菜单一跑又会把雅冲回 `-380`。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：继续三人 Actor+Painting 与门口对照表一致 | |
| 2 | Play：门口戏与进屋续聊三人站位观感一致 | |
| 3 | 村长体量 Scale **0.65**；Face/前奏淡入正常 | |
| 4 | 针线包 Tips 节点仍在（GetItem / OpenTips / SaveBag） | |
| 5 | 重跑 Continue **及** Door Setup 后布局仍=门口定稿 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 真理源 | **门口** Prefab；禁止改门口去迁就 Setup `-380` |
| 方案 | **A** 续聊整树抄门口 + **A+** Door/Continue `NudgePortraitLayout` 改定稿常量 |
| 否 | B 只改雅 X；C 整 Prefab 拷门口再 Import（易丢 Tips） |
| Setup | Door **与** Continue **一并**改（Q2）；可选抽共享 Layout 类（Q3） |

---

## ② 门口 vs 继续对照表（磁盘核实 2026-09-01）

### 立绘（Painting）

| 节点 | **门口（真理）** | **继续（现网）** | 差 |
|------|------------------|------------------|-----|
| `GoOutStoryYaerPainting` Pos | **`(348, 52)`** | **`(-380, 52)`** | ⚠️ 雅 X |
| `GushaPainting` Pos | `(0, -330)` | `(0, -330)` | 同 |
| `ChiefPainting` Scale | `0.65` | `0.65` | 同 |
| `ChiefPainting` Pos | 母体默认约 `(420, -120)`（门口可无 Pos Override） | Override **`(420, -120)`** | 同量级 |

### Actor（父节点）

| 节点 | **门口** | **继续** | 差 |
|------|----------|----------|-----|
| `Yaer` Pos / Rot | `(-625, 0)` / 无翻转 | 同 | 同 |
| `Gusha` Pos / Rot | `(-798, 0)` / **Y=180** | 同 | 同 |
| **`村长`** Pos | **`(1156, -232)`** | **`(0, 0)`** | ⚠️ 大偏 |
| **`村长`** Rot | **Y≈180**（`rot y=1,w=0`） | **无翻转** | ⚠️ |
| `村长` Scale / Size | `1` / `100×100` | 同 | 同 |

### Setup 回潮源

| 项 | 现网 |
|----|------|
| `VillageChiefDoorDialogueSetupEditor.NudgePortraitLayout` | 雅 X 仍写死 **`-380`**；古 X=`0`；村长 Painting X=`420` + Scale `0.65` |
| `VillageChiefContinueDialogueSetupEditor.NudgePortraitLayout` | **同上** |
| 均未写 | 雅完整 Pos `(348,52)`；古 Y `-330`；**Actor 村长** `(1156,-232)` + 翻转 |

→ 重跑 Door Setup **会毁掉门口已定稿的雅 348**；Continue 永远对不齐。

### 续聊图逻辑（勿丢）

磁盘核实 Continue Prefab 已含：`GetItemActionTask` / `OpenTipsFormActionTask` / `SavePlayerBag` → **禁止方案 C 整壳覆盖**。

---

## ③ 根因裁定

| ID | 假说 | 裁定 |
|----|------|------|
| **H1** | Continue Setup 雅 X=`-380` 与门口手调 `348` 脱节 | ✅ |
| **H2** | Actor `村长` 续聊未抄门口父节点位姿 | ✅ |
| H3 | 古莎 Painting 也漂 | ❌ 已同 `(0,-330)` |
| H4 | 村长 Scale 未齐 | ❌ 两侧皆 0.65 |
| H5 | Yaer/Gusha Actor 父节点不一致 | ❌ 两侧已同 |

---

## ④ 修复方案（交施工员）

### A · Prefab

将 **继续对话** 中下列属性 **逐项抄门口**：

1. `GoOutStoryYaerPainting.anchoredPosition` → `(348, 52)`  
2. Actor **`村长`**：`anchoredPosition=(1156,-232)`，`localRotation`/`euler` **Y=180**（与门口一致）  
3. 核对：`GushaPainting`、`ChiefPainting` Scale/Pos、Yaer/Gusha Actor（已同则不动）  
4. **保留** Tips 三连节点与续聊图  

### A+ · Setup（Door + Continue）

`NudgePortraitLayout` 改为门口定稿（勿再只写 X=-380）：

| 目标 | 定稿值 |
|------|--------|
| `GoOutStoryYaerPainting` | Pos **`(348, 52)`**（写满 XY） |
| `GushaPainting` | Pos **`(0, -330)`**（写满 XY，勿只改 X） |
| `ChiefPainting` | Pos X=`420`（Y 保持 `-120`）、Scale **`0.65`** |
| Actor `村长` | Pos **`(1156, -232)`** + **RotY=180** |

建议：`TrySetAnchoredPosition(rt, Vector2)` + `TrySetActorChiefLayout`；Door/Continue **同源常量**（或抽小静态 Layout 类）。

**严禁**：为迁就 Setup 把门口雅改回 `-380`。

---

## ⑤ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 继续 Prefab：雅 Painting + Actor 村长 抄门口 | **P0** |
| 2 | Door + Continue Setup：Nudge 改门口定稿（含 Actor 村长） | **P0** |
| 3 | 验收：对照表 + 重跑 Setup 不回潮；Tips 仍在 | **P0** |
| 4 | 不改 CSV/图语句/Face/Mask/其它对话 Prefab | — |

施工说明由【施工员】写：`施工说明/0901/…`（含改前/改后对照表）。

---

## ⑥ 验收清单

同沟通摘要 §③。

---

## ⑦ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 门口雅 348 是否最终视觉？ | **是**（以门口为准） | ✅ |
| Q2 | Door Setup 是否一并改常量？ | **是**（防门口被冲回 -380） | ✅ |
| Q3 | 共享 Layout 静态类？ | 可选；两 Setup 复制同一组常量也可 | ⏳ |

---

## ⑧ 程序补充（速查）

| 锚点 | 说明 |
|------|------|
| `Village_村长家门口初次对话.prefab` | 真理源 |
| `Village_村长家继续对话.prefab` | 被对齐方；含针线包 Tips |
| `VillageChiefDoorDialogueSetupEditor` / `…Continue…` | Nudge 回潮点（现 `-380`） |
| Actor `村长` | 门口 `(1156,-232)` + Y180；续聊现 `(0,0)` |

**一句话**：抄的是「门口整棵三人树」，不是只拧雅的 X；Setup 必须改成同一套定稿。
