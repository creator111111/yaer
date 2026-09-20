# ForestEast · 倒树「遮罩只影响人物」只遮挡主角 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**场景 / Prefab / 材质 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` → `Objects` → **`倒树`** → **`遮罩只影响人物`**  
**产品期望**：遮罩图 **只对主角** 做遮挡/裁切；虫卵、木虫、史莱姆、装饰、特效 **一律不受影响**  
**不是**：删遮罩交差；整棵换 Prefab；改树桥进出；大改全图 Sorting；改死羊  
**对照**：0914 倒树合层溯源 §2.1 — 现网是假遮罩（Effect 普通 Sprite）  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_倒树遮罩只影响主角_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q5

---

## 沟通摘要

### ① 结论一句话

现网 **`遮罩只影响人物` 不是 `SpriteMask`，只是 Effect/0 的普通 Sprite 盖全场**，凡排序在它下面的（主角 + Monster1/2/3 上的卵/虫等）都会被半透明贴纸挡住。产品语义定 **S1（洞形裁主角）**；贴图是 **半透明覆盖 + 透明洞口**，推荐 **方案 A**：加真 `SpriteMask`（同图）+ 关掉/透明原 SR 染色；主角 SR 用 **`Visible Outside Mask`**；卵虫保持 `None`。否决只改 Sorting 的 B。

### ② 原因（通俗）

这张图名字叫「只影响人物」，其实只是贴在最上面的大贴纸，谁在它下面都会被盖住——虫和卵也不例外。要真的只裁主角，得改成 Unity 的 SpriteMask，并只让主角参与遮罩交互。

### ③ 用户检查清单

| # | 操作 | 现网 / 期望 |
|---|------|-------------|
| 1 | Inspector 遮罩物体 | 仅 Transform + SpriteRenderer；**无** SpriteMask；MaskInteraction=None |
| 2 | Sorting | **Effect / 0**，盖在 Player(6)、Monster*(4/5/7) 之上 |
| 3 | 施工后同屏 | 主角被洞形裁；卵/虫 **不被** 该图盖住 |
| 4 | Pass 倒下 | Attached 仍关遮罩；外层靠近淡出仍在 |

### ④ 程序补充

见下文。施工文档待拍板：`施工说明/0914/ForestEast_倒树遮罩只影响主角_施工说明.md`。

---

## 1. 现网硬事实（假遮罩）

| 项 | 现网 |
|----|------|
| 物体 | `倒树/遮罩只影响人物`，fileID **`1773642872123289953`** |
| 组件 | **仅** Transform `1682464289317633030` + SpriteRenderer `6607422896812322715`（**无** `SpriteMask`） |
| Sprite | 合层 `遮罩只影响人物.png` guid **`8596789cec1c9d74a92f4def48945293`** |
| Sorting | **Effect（层 9）/ Order 0** |
| `MaskInteraction` | **0 = None** |
| 材质 | Sprites-Default（`10754`） |
| Active | 工作区 YAML 现为 **`m_IsActive: 0`**（可能 Fall 已关或误关；未倒下时应为开 —— OPEN Q4） |

合层溯源原文：名字靠 **画在 Effect 盖住角色** 冒充「只影响人物」，**并没有**按角色做遮罩交互。

`TreeBridgeLogic.AttachedGameObject[4]` **已绑**此物体（`177364…`）——改实现时 **勿弄丢**；Fall 后仍应关掉。

---

## 2. 为什么卵/虫也被盖

Sorting 序（`TagManager`）：

`Map(1) < Monster1(4) < Monster2(5) < Player(6) < Monster3(7) < SceneObject(8) < **Effect(9)**`

| 被盖对象 | Sorting | 相对遮罩 |
|----------|---------|----------|
| 主角 `Animation` / `ShadowAnimator` | Player(6) | 在 Effect 下 → **被盖** |
| WormEgg / WoodWorm 等 | Monster1/2（4/5） | 更靠下 → **被盖** |
| 场景装饰多数 | Map / SceneObject | 多数在 Effect 下 → **可被盖** |

→ 用户抱怨「虫和道具也被盖」= Effect 贴纸语义，不是 Mask 配错。

---

## 3. 产品语义裁定：**S1**

| 语义 | 观感 | 本票 |
|------|------|------|
| **S1 · 洞形裁主角** | 主角只在树洞轮廓内显示；卵/虫完整 | **采用** |
| S2 · 贴纸只盖主角轮廓 | 遮罩贴图只叠在主角身上 | 需 Stencil；本期不优先 |

### 贴图 alpha 依据（采样 `遮罩只影响人物.png` 8936×1498）

| 采样 | 结果 |
|------|------|
| 角/部分中心 | **A=0**（透明） |
| 中段横带大量像素 | **A≈179** 暗色半透明（**无** A≥200 实心块） |
| 网格统计 | 半透明 ≈ 透明量级；**不是**「只画主角轮廓的贴纸」 |

→ 美术是 **半透明树干覆盖 + 透明洞口通道**，对齐 **S1**，不是 S2 贴纸。  
Unity 默认 SpriteMask **做不到**「遮罩图只画在角色轮廓上」→ S2 走方案 C，记 OPEN。

### Inside vs Outside

洞口 = **透明区**；覆盖 = **半透明区（Mask 有效区）**。  
主角应在洞口内显示 → **`Visible Outside Mask`（枚举 2）**。  
若误用 Inside，会只在半透明覆盖下露人（反了）。

---

## 4. 方案对比

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A · 真 SpriteMask** | 同物体加 `SpriteMask`（同一 Sprite）；原 SR **禁用或 Alpha=0** 防双画盖全场；主角 SR → Outside；卵虫保持 None | **推荐** |
| **B · 只改 Sorting** | 遮罩夹在 Player 与 Monster 之间 | **否决作主方案**：Monster1/2 **低于** Player，仍会被盖；天生做不到「只主角」 |
| **C · Stencil** | 遮罩图只在主角模板内绘制 | 仅当必须 S2；本期不优先 |

---

## 5. 方案 A 操作清单

### 5.1 倒树 / 遮罩物体

1. 在 `遮罩只影响人物` 上添加 **`SpriteMask`**，Sprite = 现网同一张（guid `8596789c…`）。  
2. 原 **SpriteRenderer**：`enabled=false` **或** `color.a=0`（推荐禁用，避免 Effect 再染全场）。  
3. SpriteMask 自定义前后范围按 Play 调（默认即可先测）。  
4. **保持** Attached 绑定；**不要**改内/缝/光/外、CollisonMap、SFX、Fall 逻辑。  
5. 确认未倒下时物体 **Active=true**（现网若为 0 先打开）。

### 5.2 主角 `MaskInteraction` 清单（`Player.prefab`）

| 物体 | SortingLayer | 现网 Interaction | 施工 |
|------|--------------|------------------|------|
| **`Animation`**（主立绘） | **Player(6)** | 0 None | → **Visible Outside Mask** |
| **`ShadowAnimator`** | **Player(6)** | 0 None | → **Visible Outside Mask** |
| `1`～`6`（子特效格） | Default(0) | 0 | 可选同设；优先保证主身+影子 |
| `PlayerFoot` | Default(0) | 0 | 一般可不改（脚点） |

**不要改**：WormEgg / WoodWorm / Slime / 场景装饰的 `MaskInteraction`（保持 None）。

### 5.3 全局 Prefab 影响

工程内 **尚无** `SpriteMask` / 代码改 Interaction 先例。  
`Visible Outside Mask` 在 **无 Mask 作用时通常仍全显示** → 村内等场景风险低于 Inside。  
若仍担心：进洞 `playerIsInTreeBridge=true` 时设 Outside、出洞还原 None（OPEN Q3 备选）。

---

## 6. 验收

1. 人进倒树洞口区域：只见主角被洞形裁切。  
2. 同屏虫卵/木虫/特效 **不被** 该遮罩盖住或裁掉。  
3. 「外」靠近淡出仍在。  
4. Pass 倒下后遮罩随 Attached 关掉。  
5. 出洞 / 其它场景主角显示正常。

Debug 可选：`[TreeMask]` 打玩家 Interaction 与 `SpriteMask.isActiveAndEnabled`。

---

## 7. OPEN

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | S1 还是 S2？ | **S1**（贴图为覆盖+洞口） | ✅ 已裁定 |
| Q2 | Inside 还是 Outside？ | **Outside** | ✅ 已裁定 |
| Q3 | Prefab 全局改 vs 进洞运行时设？ | 默认 **Prefab 改 Animation+Shadow**；若它景异常再改运行时 | 待施工验 |
| Q4 | 遮罩现 `Active=0`？ | 未倒下应 **打开**；已倒下保持关 | 待施工核 |
| Q5 | 必须 S2 贴纸感？ | 另开 C（Stencil） | 降级 |

---

## 8. 侦探声明

未改场景 / Prefab / Git。本机未 Play；S1/Outside 由贴图 alpha + 层序静态钉死，施工后须听视觉验收。
