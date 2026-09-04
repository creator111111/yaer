# Village_村长家门口初次对话 — 框出时空头像 — 施工说明

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【施工员】按侦探报告 F1 落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md`  
**产品**：框出现且正文仍空时，左槽 **无** Mask 小头像；首句「奶奶。」出现时再出古莎头像  

---

## 沟通摘要

### ① 结论一句话

**已按 F1 关掉门口 Prefab 的 `PrepareMaskAvatarOnFadeIn`；并给 Door Setup 钉死默认 false，防菜单重跑回潮。运行时 C# / KenMuNiStart 预亮未动。**

### ② 原因（通俗）

门口对话框淡入时误勾了「预亮小头像」，擦掉字的同时先亮了雅儿脸。  
关掉这勾之后：空框 → 等第一句话再出对应头像。进村开场仍要「框+头像同拍」，所以没动它。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 门口三人戏：框出现瞬间（无字）左槽 **无** 小头像 | |
| 2 | 第一句「奶奶。」出现时，左槽出现 **古莎** Happy | |
| 3 | 后续雅/村/古换脸正常；大立绘三人戏不受影响 | |
| 4 | `Village_KenMuNiStart`：框出仍可预亮 Mask（框+头像同拍） | |
| 5 | （可选）再跑一次 `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab` 后，框出仍为空头像 | |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab` | 框 FadeIn：`PrepareMaskAvatarOnFadeIn` **true→false** |
| 2 | `DialoguePreludeOptions.cs` | 新增 `PrepareMaskAvatarOnFadeIn`（默认 **true**，兼容 Shop/KenMuNi） |
| 3 | `DialoguePreludeBuilder.cs` | `CreateDialogueUiFadeNode` 读 options，不再硬写 true |
| 4 | `VillageChiefDoorDialogueSetupEditor.cs` | 门口 Setup：`PrepareMaskAvatarOnFadeIn = false` |

**未改**：`NormalDialogueUIAlphaAnimationTaskAction` 运行时逻辑、`DialogueMaskAvatarPresenter`、CSV、大立绘、`Village_KenMuNiStart.prefab`、商店等其它已勾预亮 Prefab。

---

## ② 方案说明（F1 vs 备选）

| 方案 | 做法 | 本期 |
|------|------|------|
| **F1** | 仅门口关预亮；首句 `OnGetNewStatement` → Apply | ✅ 已做 |
| F2 | FadeIn 清字时未勾预亮则 `HideAll` | 未做（H2/H4 非主因） |
| F3 | Role=None 预亮 | ❌ 无效 |

**为何还改 Editor**：侦探 Q5——Door Setup 拷 KenMuNi 壳 + Prelude 曾硬写预亮=true，只改 Prefab 会被菜单重跑冲掉。  
**替代方案**：只改 Prefab、禁止再跑 Door Setup；脆弱，故加 options 开关。

---

## ③ 验收与回归

| 必测 | 期望 |
|------|------|
| 门口初次对话 | 空框无头像 → 首句古莎头像 |
| KenMuNiStart | 预亮仍生效（options 默认 true；Prefab 未改） |
| Door Setup 重跑 | 重建图后预亮仍为 false |

---

## ④ OPEN

| ID | 项 | 状态 |
|----|----|------|
| Q5 Setup 回潮 | Door Setup 已钉 false；options 默认 true 不伤其它对话 | ✅ 已施工核对 |
