# Cursor Agent Prompt · Village_KenMuNi1：老农（`农`）基础对话交互设置

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **美术锚点（用户 Hierarchy 红箭头）**：`肯姆尼2合层` → **`农`**  
> **台本 CSV**：`Assets/Dialog/Village_老农打水任务.csv`（Speaker：`雅` / **`老人`**）  
> **产品目标（白话 · 分期）**：  
> 1. **本期（钉死）**：**先做基础对话交互**——走近/点击老农能播对白（Import 成功 + NPC 可交互）  
> 2. **下期（挂钩不施工）**：他会发任务（打水浇地）；接取选项 / Quest 状态机 **本期不做完整闭环**  
> **现场 Console（用户截图）**：`[DialogueCsvGraphBuilder] Speaker「老人」（ID 2/4/5/7/9/11/13/15）未在映射表中找到，导入已中止。`  
> **代办对齐**：今日代办「农民的任务对话」  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品分期（防范围膨胀）

| 阶段 | 做 | 不做 |
|------|----|------|
| **本期 · 基础对话** | Speaker 映射 `老人`；CSV→对话 Prefab；`农`/交互实体能点/E 播对白；GSM 登记 | ❌ Choice 接任务 / AcceptQuest / 交水桶判定 |
| **下期 · 任务** | 对话末选项、QuestId、状态切 Offer/循环/Thanks | 本期只出挂钩与样板（埃吉尔） |

现网 CSV 末句已是「帮我打几桶水…」，但 **无 `Type=Choice` 行**——导入后会播完整寒暄到求助句结束；**不等于**任务已接取。

### 现场证据

**Hierarchy（截图 1）**

```
Village_KenMuNi1 / Map / Design / … / 肯姆尼2合层 /
  田 / 商店 / … / 精灵池*
  ★ 农                 ← 红箭头：要做成可对话 NPC
```

预扫：`农` 仅 Transform + SpriteRenderer（Layer 0），**无** SceneEntity / Interactive / SimpleStoryTrigger → 点了没反应属预期。

**Console（截图 2）**

| 报错 | 含义 |
|------|------|
| `Speaker「老人」…未在映射表中找到，导入已中止`（ID 2,4,5,7,9,11,13,15） | CSV 导入 P0 堵死；须补映射，仿 `埃吉尔`/`店` |

无关噪声（本期可记一笔不修）：`Effect_Player_ChangeDirDust` MissingRef、`Village_KenMuNiStart` Missing Prefab guid——**勿当老农主因**。

### CSV / 资源预扫

| 项 | 预扫 |
|----|------|
| CSV | `Village_老农打水任务.csv`：雅↔老人多轮；末句老人求助打水 |
| 对话 Prefab | 预扫 **尚无** `Village_*老农*` / `Village_Farmer*`（仅 CSV） |
| Speaker 映射 | `CreateDefaultInstance` / Default.asset **无「老人」** |
| 建议 Actor 名 | **`老人`**（恒等映射，对齐埃吉尔）；或 `老农`——侦探拍板并与 Prefab Actor GO 名一致 |
| 建议 Story 名 | **`Village_老农打水任务`** 或 `Village_Farmer_WaterOffer`（须与 Import 产出文件名逐字一致） |

### 基础交互金样（复用，勿另起炉灶）

室内样板：`Object/Npc*` 三件套（0820/0822）。  
村外/民居样板：`SimpleStoryTrigger` + `InteractiveComponent` + `RaycastListener`；近距 NPC 通常 **`requirePlayerOverlap=true`**，根 **Z=0**。

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · Objects 下新建交互实体**，合层 `农` 只当美术（或关合层 SR 防叠图） | 对齐 House/Item：交互在 `Objects`，合层装饰 | **✅ 推荐倾向**（合层常被整体缩放/排序，硬挂组件易踩 Z/Bounds） |
| **B · 直接在合层 `农` 上挂三件套** | 少一个物体 | 须证伪 Z≠0 / 合层父级是否破坏 Intersects |
| **C · GSM 硬编码点农** | — | ❌ |

**GSM**：`Village_KenMuNiSceneManager` / `SceneEntityComponentGSM`：`objRoot`、`sceneObjs` 须登记新实体（或运行时重扫规则写清）。

### Speaker 映射方案（侦探必拍板）

| 方案 | CSV | 图内 Actor | 倾向 |
|------|-----|------------|------|
| **M1 · 恒等** | `老人` | `老人` | **✅**（仿埃吉尔） |
| M2 · 改 CSV 为已有名 | 改成 `1`/`NPC1` 等 | — | ❌ 语义不对，且动台本 |
| M3 · `老人→老农` | `老人` | `老农` | 仅当立绘/命名统一叫老农时 |

须同步改：**`DialogueSpeakerMapping.CreateDefaultInstance`** + **`DialogueSpeakerMapping_Default.asset`**（0820 先例：两处都改）。

### 对话 Prefab / 立绘缺口（本期最小）

| 检查 | 期望 |
|------|------|
| Import 成功生成 Prefab + Generated 图 | ✅ |
| Actor：雅尔 + 老人 | Prefab 内需有对应 `DialogueActor(Ex)` |
| 对话框壳 | Fighting / UIAlpha（防「有 Trigger 无框」） |
| 老人 Mask / 大立绘 | 若工程无老人 Painting：**本期可用无立绘/占位**，写入开放问题；勿阻塞「能说话」 |
| 雅 | 走现网 GoOut/Mask 即可 |

### 与「任务系统」边界

| 埃吉尔样板（下期对照） | 本期老农 |
|------------------------|----------|
| `Type=Choice` + `QuestAcceptAction` | CSV **尚无** Choice → 本期不接 Accept |
| `AegirQuestStoryTrigger` 按状态切 Prefab | 本期 **普通** `SimpleStoryTrigger` 即可 |
| QuestConfig / 打水道具 | ❌ 不做 |

报告须单列「下期最小挂钩」：补 Choice、QuestId、Trigger 子类时机。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 映射 `老人` + 能 Import CSV | ❌ 改打水任务玩法/道具 |
| ✅ 对话 Prefab 落地 + Story 名 | ❌ 完整接任务/交任务状态机 |
| ✅ 交互实体 + GSM 登记 + 能播对白 | ❌ 老人完整立绘系统（可占位） |
| ✅ 验收：走近对话 | ❌ 修无关 Missing Prefab / Dust |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 把合层装饰当成已可交互而不对拍组件  
- 未补映射就宣称「对话配好了」  
- 本期强做 QuestAccept / 多状态 Trigger  
- 把 `农` 做成 `SceneChangeDoor` 进屋  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_KenMuNi1.unity` · `农` | 美术锚点 |
| `Village_老农打水任务.csv` | 台本真源 |
| `DialogueSpeakerMapping.cs` + `_Default.asset` | 映射缺口 |
| `DialogueCsvGraphBuilder.cs` / Import 窗口 | 报错同源 |
| `SimpleStoryTrigger` / `RaycastListener` / NPC 样板 | 交互三件套 |
| `Village_KenMuNiSceneManager` / SceneEntity GSM | 登记 |
| `0820` Speaker 映射报告、`0822` NPC45 配置报告 | 先例 |
| `0608` Aegir QuestOffer | **下期**任务挂钩样板 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/CSV导入_Speaker1与4与5映射缺失_架构溯源报告.md
@Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_NPC45配置与GSM绑定_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Npc1无E对照HomeScene23_架构溯源报告.md
@Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_Aegir_QuestOffer_对话末尾双选项_架构溯源与施工执行说明.md
@Assets/Doc/提示词/0804/任务系统_对话末接取选项机制_架构侦探提示词.md
@Assets/Dialog/Village_老农打水任务.csv
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset
@Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Interactive/RaycastListener.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写「老农基础对话交互」溯源报告。

---

## 背景（策划白话）

1. 田边合层里的 **`农`** 要做成能对话的 NPC。  
2. 以后他会给玩家**打水浇地**任务，但**现在先做基础对话**：能 Import、能走近说话。  
3. Console 已报 Speaker「老人」不在映射表——导入被挡。  
4. 本阶段摸清：映射怎么补、Prefab 叫什么、交互挂 Objects 还是合层 `农`、GSM 怎么登记；任务选项留到下期。

---

## 侦探任务清单

### A. 钉死导入堵点
确认报错来自 `老人` 未映射；列出须改的两处映射文件；拍板 M1（`老人→老人`）。

### B. 钉死 CSV → Prefab 命名
建议磁盘对话 Prefab 名；Import 后 Actor 列表；壳层（UIAlpha）是否要补。

### C. 钉死 `农` 与交互实体关系
合层 `农` 组件表；推荐方案 A/B；Z=0 / requirePlayerOverlap / Layer；合层 SR 去重策略。

### D. GSM 登记
`sceneObjs` / objRoot；是否改 C#（倾向只登记场景）。

### E. 本期最小闭环 vs 下期任务挂钩
画：本期 Click→TriggerStory→播完；下期加 Choice/Quest 时切哪（对照埃吉尔，**不施工**）。

### F. 老人立绘/Mask
工程有无老人 Painting；本期能否无立绘先播；缺口 P1。

### G. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 映射补 `老人`（代码默认 + Default.asset） | **P0** |
| 2 | Import `Village_老农打水任务.csv` → Dialogue Prefab | **P0** |
| 3 | 建/配交互实体 + SimpleStoryTrigger(Story名) | **P0** |
| 4 | GSM sceneObjs 登记；Z=0；近距 overlap | **P0** |
| 5 | 对话框壳（若 Import 无前奏） | P0/P1 |
| 6 | 老人立绘/Mask | P1 |
| 7 | Choice + AcceptQuest + 状态 Trigger | ❌ 下期 |

### H. 验收清单（本期）

| # | 操作 | 通过 |
|---|------|------|
| 1 | CSV Import | **无**「老人」映射报错；生成 Prefab |
| 2 | 村内走近 `农`/交互体，点或 E | 出对白；雅/老人轮流说话 |
| 3 | 听到末句 | 「…打几桶水浇地吗」类文案；**不要求**出现接任务按钮 |
| 4 | Console | TriggerStory 名=Prefab 名；无 Missing Actor（或仅立绘 Warning 已记录） |
| 5 | 结束后 | 可再对话（若未 SingleUse） |

### I. 开放问题
- 交互实体最终命名（`Npc_Farmer` / `老农` / 继续用 `农`）？  
- 合层 `农` 是否 Disable Renderer 防双影？  
- 下期 QuestId / 打水道具表是否已有策划案？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md`

MASTER 四段式：  
① 结论（映射 + 交互挂点方案 + 本期不含接任务）  
② 原因（为何 Import 挂、为何合层 `农` 还不能说话）  
③ 用户检查清单（Import → 走近对话）  
④ 给程序：映射表 + 实体方案 + Prefab 名 + 下期任务挂钩 + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md
@Assets/Dialog/Village_老农打水任务.csv
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。按报告完成老农【基础对话交互】：映射「老人」、Import 对白、场景可交互播剧情。

必须遵守：
- CreateDefaultInstance 与 Default.asset 两处都补映射；
- StoryPrefabName 与磁盘 Prefab 文件名一致；
- 复用 NPC/物体交互三件套；近距 overlap；根 Z=0；
- 本期不做 Choice 接任务 / QuestAccept / 状态机 Trigger；
- 代码含详细注释；重要取舍写清原因。

提交说明：映射怎么写、Prefab 名、交互挂在哪、如何验收、未做的任务部分。
```
