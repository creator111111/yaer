# Cursor Agent Prompt · 村长家「继续对话」三人大立绘摆位对齐门口初次对话

> **角色**：【施工员】最小化把续聊三人立绘布局抄齐门口（先对表再改）  
> **日期**：2026-09-01  
> **产品设定（钉死）**：`Village_村长家继续对话` 的 **雅 / 古 / 村长** 大立绘 **位置（含父 Actor）** 必须与  
> `Village_村长家门口初次对话` **保持一致**  
> **真理源**：门口 Prefab（已手调定稿）  
> **被改方**：继续对话 Prefab + Continue Setup（防重跑回潮）  
> **说明落盘**：`Assets/Doc/施工说明/0901/Village_村长家继续对话_三人大立绘摆位对齐门口_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须核实，勿当唯一真相）

### 产品白话

> 进屋续聊三人站位要和门口三人戏一样，不能续聊里雅/古/村又错位一版。

### 磁盘预扫对照（须证伪后写入施工说明）

| 节点 | **门口（真理）** | **继续（现网）** | 差 |
|------|------------------|------------------|-----|
| `GoOutStoryYaerPainting` Pos | **`(348, 52)`** | `(-380, 52)` | ⚠️ 雅 X 大偏 |
| `GushaPainting` Pos | `(0, -330)` | `(0, -330)` | 同 |
| `ChiefPainting` Scale | `0.65` | `0.65` | 同 |
| `ChiefPainting` Pos | 母体默认约 `(420, -120)`（门口实例可无 Override） | Override `(420, -120)` | 近 |
| Actor **`村长`** Pos | **`(1156, -232)`**，Rot Y≈180 | Setup 常 **`(0,0)`** | ⚠️ 父节点大偏 → 村长世界位必漂 |
| Setup `Nudge` 常量 | 仍写雅 X=`-380` | 同 | ⚠️ **与门口定稿脱节**；重跑会毁掉门口/继续 |

**结论倾向**：不能只改 Continue 里 Painting 的 X；须 **整树抄门口**（Actor + Painting 的 Pos / Rot / Scale）。Setup 常量也要改成门口定稿，否则菜单一跑又漂。

### 对齐范围（钉死）

| 层 | 要对齐的属性 |
|----|----------------|
| Actor：`雅尔`/`古莎`/`村长`（名以 Prefab 为准） | `anchoredPosition`、`localRotation`、`localScale`、`sizeDelta`（若门口有意义） |
| 立绘：`GoOutStoryYaerPainting` / `GushaPainting` / `ChiefPainting` | `anchoredPosition`、`localScale`（村长已 0.65）、必要时 `sizeDelta` |
| **不做** | 改 CSV / 图节点语句；改 Face Sprite；改 Mask 小窗；改其它对话 Prefab |

### 修复倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A** | Prefab 模式打开两对话，把继续的三人 Actor+Painting **逐项抄门口数值** 并 Save | ✅ 必做 |
| **A+** | 抽共享 `NudgePortraitLayout`（或 Continue/Door 同步）：常量改为门口定稿（雅 `(348,52)` 等）；Actor `村长` 同步 `(1156,-232)` + 翻转 | ✅ 防回潮 |
| B | 只把 Continue 雅 X 从 -380 改 348 | ⚠️ 不够（村长 Actor 仍漂） |
| C | 继续整 Prefab Copy 门口再 Import CSV | ⚠️ 易丢针线包 Tips 节点；非首选 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 继续三人布局 = 门口 | ❌ 反过来改门口去迁就 Setup -380 |
| ✅ Setup 常量对齐门口定稿 | ❌ 改台本 / Tips / 换场 |
| ✅ 短施工说明 + 对照表 | ❌ 改雅/古母体源 Prefab 全局站位（仅对话实例 Override） |

### 严禁

- 只对齐 Scale 不对齐 Pos  
- 只对齐 Painting 忽略 Actor `村长` 父节点  
- 重跑 Setup 用旧 `-380` 覆盖门口定稿  
- 把晚宴台本当对齐源  

### 开放

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 门口雅 348 是否最终视觉？ | **是**（产品：以门口为准） |
| Q2 | Door Setup 是否一并改常量？ | **是**（与 Continue 同源，避免门口也被冲回 -380） |
| Q3 | 共享一个 Layout 静态类？ | 可选；两 Setup 复制同一组常量也可 |

---

## 施工 Prompt（复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md

## 目标
Village_村长家继续对话 的三人大立绘（雅/古/村）位置与布局
必须与 Village_村长家门口初次对话 一致。
真理源 = 门口 Prefab；改继续 Prefab + Setup 防回潮。

## 必读
@Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab
@Assets/Editor/Tool/Dialogue/VillageChiefDoorDialogueSetupEditor.cs
@Assets/Editor/Tool/Dialogue/VillageChiefContinueDialogueSetupEditor.cs
@Assets/Doc/施工说明/0901/Village_村长家对话_村长大立绘Scale过小修复_施工说明.md

## 核实（改前写入施工说明对照表）
对以下节点逐项记录 Pos / Rot / Scale（Actor + Painting）：
- GoOutStoryYaerPainting
- GushaPainting
- ChiefPainting
- 父节点 Actor（雅尔/古莎/村长，以 Hierarchy 名为准）
预扫差异：门口雅 Painting (348,52) vs 继续 (-380,52)；门口 Actor 村长 (1156,-232) vs 继续常 (0,0)。

## 修复
1. 将继续对话中上述节点的布局属性抄齐门口（不要只改雅 X）。
2. 保留村长 Scale=0.65、针线包 Tips 图节点、续聊 CSV 图逻辑。
3. 更新 Door + Continue 的 NudgePortraitLayout：常量改为门口定稿
   （含雅完整 Pos、古 Pos、村长 Painting Pos/Scale、Actor 村长 Pos/Rot），
   禁止继续写死雅 X=-380。
4. 禁止改门口视觉当「迁就 Setup」；禁止动其它对话 Prefab。

## 落盘
Assets/Doc/施工说明/0901/Village_村长家继续对话_三人大立绘摆位对齐门口_施工说明.md
结构：①结论 ②门口 vs 继续对照表（改前/改后）③改了什么 ④验收 ⑤程序补充

## 验收
- [ ] Prefab 模式：继续三人 Actor+Painting 数值与门口一致（对照表打勾）
- [ ] Play：门口戏与进屋续聊三人站位观感一致
- [ ] 村长体量仍正确（Scale 0.65）；Face/前奏淡入正常
- [ ] 针线包 Tips 节点仍在
- [ ] 重跑 Continue（及 Door）Setup 后布局仍对齐门口定稿

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. 直接跑「施工 Prompt」。  
2. **以门口为准**：续聊现在雅还在 Setup 的 `-380`，门口已是 `348`；村长 Actor 门口在 `(1156,-232)`，续聊父节点也要对齐。  
3. Setup 里的 `-380` 必须改掉，否则一键生成又漂回去。
