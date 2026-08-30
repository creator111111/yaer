# Village_ShopHead — 点头对白补齐雅儿大立绘 — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**目标**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`

关联提示词：`Assets/Doc/提示词/0829/Village_ShopHead_雅儿大立绘_架构侦探提示词.md`  
关联：`0829/…Head热区安装Village_ShopHead` · `0827/Village_ShopStart_新建Merchant` · `0827/首次进店联调`

---

## ① 结论一句话

**不是缺嵌物体：`Yaer` 下已有 `GoOutStoryYaerPainting`（与 ShopStart 同源 guid），真正缺口是「默认 `m_Alpha=0` + 图内无雅立绘淡入 Action + 黑板 `GoOutStoryYaerPainting` 未绑定 CanvasGroup」——雅换脸链本身齐，但立牌一直透明；推荐方案 A：绑 BB + 补短淡入（或直接改 Alpha=1），默认不改 C#；Mask 小头像 ≠ 大立绘。**

---

## ② 原因（通俗）

### 2.1 三种「脸」别混

| 层 | 是什么 | 点头线 |
|----|--------|--------|
| **雅儿大立绘** | Prefab 内 `GoOutStoryYaerPainting`（左侧大半身立牌） | ✅ **本期要看见** |
| **雅儿 Mask** | 对话框旁小头像 | ✅ 可有；**不能冒充**大立绘 |
| **老板娘** | 场景合层 `MerchantPainting` + Mask | ✅ 已有；**禁止**再嵌进对话 Prefab |

生活类比：立牌已经立在箱子里，但罩了全黑布（alpha=0），而且没人拉布（无淡入）；黑板插头也没插上。不是去木匠铺再做一块立牌。

### 2.2 与 ShopStart 差异表（磁盘核实）

| 项 | `Village_ShopStart` | `Village_ShopHead` | 差距 |
|----|---------------------|--------------------|------|
| Actor Yaer / Merchant | ✅（+Gusha） | ✅ Yaer+Merchant；无 Gusha GO | Actor 齐 |
| `Yaer/GoOutStoryYaerPainting` | ✅ 嵌 guid `4c0e9909…` | ✅ **同 guid 已嵌** | **有物体** |
| 默认 `m_Alpha` | **0**（靠图淡入拉到 1） | **0** | 同为隐；ShopStart 有拉起 |
| BB `GoOutStoryYaerPainting` | `"_value":1` + `_objectReferences` 绑 CG | 变量名在；**无 `_value`**；`_objectReferences: []`；`_references: []` | **未绑定** |
| BB `GushaPainting` | 已绑 | 空壳残留 | P1 可删 |
| 图内雅 Alpha Action | ✅ `CanvasGroupAlphaActionTask` 0→1（并行古莎） | ❌ 仅 FightingVisible + **对话框** UIAlpha | **缺雅立绘拉起** |
| ShopStart 黑幕闸门 / WaitBg | 有 | 无（特殊交互正确） | 勿抄全套进店分层 |
| 雅 Painting 锚点 | ≈ (-559, 52) | ≈ **(-835, 52)** | 已不同；可先保留 |
| 店合层 | 场景侧，不嵌 Prefab | 同 | 勿嵌 Merchant |

### 2.3 驱动链断在哪？

```
雅句 Say
  → DialogueActorEx.RefreshAvatar(FaceType)     ✅ Actor 在
  → GoOutStoryYaerPainting 订阅父级 Actor 换脸   ✅ 嵌在 Yaer 下，FindDialogueActorEx 可找到
  → UpdateFace(…)                               ✅ 逻辑在
  → 屏幕看见大立绘                              ❌ CanvasGroup.alpha 一直 0
  →（并行）Mask 小头像                          ✅ 可能仍亮 → 用户易误以为「只有小头」
```

**断点**：**显隐（alpha=0 未拉起）** + **BB 未绑**（即便补淡入 Action 也绑空）。  
**不是**：缺枚举、缺 GoOut Prefab、缺嵌实例、缺换脸代码。

### 2.4 与老板娘合层 / UI

| 问 | 答 |
|----|-----|
| 点头对白时合层可见？ | ✅ `TryTriggerShopkeeperSpecial` 只 Hide **`UI_Shop`**，不关「商店界面合层」 |
| Hide UI_Shop 误伤雅立绘？ | ❌ 雅立绘在对话 Prefab / Dialogue UI 树，不在 UI_Shop |
| 雅会否被合层挡住？ | 合层世界 SR vs 对话 Overlay Canvas：一般 UI 在上；Pos 已偏左 (-835)。若挡脸再微调（开放 Q2） |

---

## ③ 用户检查清单

| # | 操作 | 通过（施工后） |
|---|------|----------------|
| 1 | Idle 点 Head 开对白 | **左侧**出现雅儿 **大半身立牌**（非仅对话框旁小头） |
| 2 | 雅句换脸 | 立牌脸变；Mask 可同步 |
| 3 | 店句 | **右侧**场景老板娘变脸/身；屏幕上**没有**第二份老板娘 Prefab 立绘 |
| 4 | 对白结束 | 雅立绘随对白卸掉；合层 Idle 回默认（现网 Reset） |
| 5 | Console | 无 Missing Painting / BB 空引用 / Face 键 Warning |
| 6 | （施工前对照） | 现网点头：预期 **看不见** 雅大立绘（alpha=0） |

---

## ④ 给程序

### A. ShopHead 现状钉死

| 项 | 值 |
|----|-----|
| Hierarchy | `Village_ShopHead` → `Yaer` → **`GoOutStoryYaerPainting`**（PrefabInstance） |
| 源 Prefab | `GoOutStoryYaerPainting` guid `4c0e9909764ce6e4eb37971a6fe20fd3` |
| override Alpha | **0** |
| BB | 名有、**引用空** |
| 图头节点 | FightingPanelVisible → NormalDialogueUIAlpha（对话框）→ 店/雅 Statements… |
| Play 推论 | 雅句：**无大立绘** / 可能仍有 Mask |

### B. 方案拍板

| 方案 | 裁定 |
|------|------|
| **A · 复用已嵌 GoOut + 绑 BB + 补显隐** | **✅ 首选** |
| A′ 重嵌 | ❌ 物体已在 |
| B 场景常驻雅 | ❌ |
| C 仅 Mask | ❌ 不满足产品 |
| D Dress YaerPainting | ❌ 村线用 GoOut |

**方案 A 细则（拍板）**

1. **嵌哪个**：已嵌 `GoOutStoryYaerPainting`，勿换 Dress。  
2. **Pos/Scale**：默认 **保留** ShopHead 现锚点 (-835, 52)；不必像素级抄 ShopStart (-559)（开放 Q2）。  
3. **显隐（推荐）**：特殊交互无进店黑幕 →  
   - **推荐 A1**：图内在对话框淡入后加一条 `CanvasGroupAlpha`（雅）**0→1、时长约 0.3～1s**（对齐 ShopStart 任务类型，**不要** WaitShopStart / 古莎淡入）。  
   - **备选 A2**：直接把 override `m_Alpha` 改为 **1**（无淡入、改动更小）。  

> ⚠️ **改口（2026-08-29）**：上条 A1「对话框淡入**后**再拉立绘」与产品「先立绘后对话框」冲突，**已作废**。  
> 正确序见：`0829/Village_ShopHead_先立绘后对话框时序_架构溯源报告.md` → **T1**：雅 `CanvasGroupAlpha` **必须在** `NormalDialogueUIAlpha` **之前**。  
4. **BB**：把雅 Painting 根上 `CanvasGroup` 绑到 `GoOutStoryYaerPainting`（对照 ShopStart `_value` + `_objectReferences`）。A2 若永不跑 Alpha Action，BB 仍建议绑好，避免以后加节点踩空。  
5. **Gusha BB**：无 GO、无台本 → P1 删变量，避免误导。  
6. **代码**：默认 **不改** `GoOutStoryYaerPainting` / `DialogueActorEx`。  
7. **CSV Import**：大立绘可见性与台本对齐 **可分单**；本单 **P0=显隐**。若雅脸键错（旧图 Smug/ChiBie），属 Head 安装报告 CSV 重 Import（P1/并行），**勿误诊成缺大立绘**。

### C. 最小施工清单（本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | BB | 绑定 `GoOutStoryYaerPainting` → 实例 CanvasGroup | **P0** |
| 2 | 显隐 | **A1** 图内补雅 `CanvasGroupAlpha` 0→1；或 **A2** `m_Alpha=1` | **P0** |
| 3 | 验收 | 雅大立绘可见 + 雅句换脸（立牌+Mask） | **P0** |
| 4 | 清理 | 删除无用 `GushaPainting` BB（可选） | P1 |
| 5 | 依赖 | `Village_商店点头交互.csv` 重 Import（脸/文案） | 对齐 Head 安装；可另单 |
| 6 | 代码 | 仅当绑好仍不换脸再查 Actor 订阅 | 默认不做 |

**排除**：嵌 MerchantPainting；改合层；Chest；扩 `DialogueFaceType`；抄 ShopStart 全套黑幕分层。

**预期 diff**

- 仅 `Village_ShopHead.prefab`（BB 引用 + Alpha Action 或 m_Alpha）  
- 可选：Generated Graph asset（若 Import/Save 旁路）  
- **不改** `.cs`（默认）

### D. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 点 Head | 左侧雅儿 **大立绘**出现 |
| 2 | 雅句 FaceType | 立牌脸变；Mask 同步 |
| 3 | 店句 | 合层变；无第二份老板娘 Prefab 立绘 |
| 4 | 结束 | 雅立绘卸掉；合层 Reset 默认 |
| 5 | Console | 无 Missing / 未绑 BB / Face Warning |

### E. 开放问题

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 点头雅立绘短淡入还是直接显示？ | **A1 短淡入**（备选 A2 直接 Alpha=1） | 待确认 |
| Q2 | Pos 是否必须对齐 ShopStart (-559)？ | **否**；保留 (-835, 52)，挡脸再调 | 待确认 |
| Q3 | 本单是否顺带 CSV 重 Import？ | **否**（大立绘优先）；Import 跟 Head 安装单 | 待确认 |

（已追加 `OPEN_QUESTIONS.md`。）

---

## 附录 · 关键锚点

| 主题 | 路径 |
|------|------|
| 缺口 Prefab | `GameRes/Prefabs/Dialogue/Village_ShopHead.prefab` |
| 样板 | `Village_ShopStart.prefab`（BB `_value` + CanvasGroupAlpha） |
| 立绘组件 | `GoOutStoryYaerPainting.cs` / `StoryFormPainting` |
| 点头 CSV | `Assets/Dialog/Village_商店点头交互.csv` |
