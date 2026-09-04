# Village_KenMuNi1 — 老农（`农`）基础对话交互 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 接线拍板（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**美术锚点**：`肯姆尼2合层` / **`农`**  
**台本**：`Assets/Dialog/Village_老农打水任务.csv`  
**产品分期**：本期 **仅基础对话**；打水任务 / Choice / AcceptQuest **下期挂钩不施工**

关联：`DialogueSpeakerMapping` · CSV Import · NPC 三件套 · `0820` Speaker 映射先例 · `0608` 埃吉尔 QuestOffer（下期）· 代办「农民的任务对话」

---

## ① 结论一句话

**Import 被 Speaker「老人」未映射堵死——拍板 M1：`老人→老人`（恒等），须同时改 `CreateDefaultInstance` + `DialogueSpeakerMapping_Default.asset`。对话 Prefab 尚无，Import 产出名建议 `Village_老农打水任务`。合层 `农` 仅 Transform+SR 且 local Z≈0.71，不宜直接当交互根；推荐方案 A：在 `Objects` 新建 `Npc_Farmer`（Z=0、近距 overlap、Layer21、`SimpleStoryTrigger`→该 Prefab），合层 `农` 保留美术。本期普通 `SimpleStoryTrigger` 即可；CSV 无 Choice，播完求助句 ≠ 已接任务。不改 GSM C#，只登记 `sceneObjs`。**

---

## ② 原因（通俗）

1. **导入**：台本写「老人」，对照册里没有这个外号，整批拒收——所以 Console 报映射中止。  
2. **场景**：合层里的 `农` 只是画片，没装「门铃三件套」，点了当然没话。  
3. **任务**：文案已经求你打水，但表里还没有「接/不接」选项行——所以本期只能聊天到求助句结束，**接任务留到下期**。

---

## ③ 用户检查清单（本期验收）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Import `Village_老农打水任务.csv`（映射补齐后） | **无**「老人」映射报错；生成 Dialogue Prefab |
| 2 | Import 勾选 **对话框 UI 淡入**（或事后补 UIAlpha） | 播时有可见对话框 |
| 3 | 村内走近 `Objects/Npc_Farmer`（或施工命名），出 E / 点击 | 雅↔老人轮流说话 |
| 4 | 听到末句 | 「…打几桶水浇地吗」类；**不要求**接任务按钮 |
| 5 | Console | TriggerStory 名 = Prefab 文件名；无 Missing Prefab |
| 6 | 结束后再谈 | 可再播（`SingleUseInArchive=false`） |
| 7 | 回归 | Door_Shop / House_Tree / 其它 NPC 不坏 |

无关噪声（本期不修）：`Effect_Player_ChangeDirDust` MissingRef、`Village_KenMuNiStart` Missing guid。

---

## ④ 给程序

### A. 导入堵点（钉死）

| 报错 | 真源 |
|------|------|
| `Speaker「老人」（ID 2/4/5/7/9/11/13/15）未在映射表中找到，导入已中止` | `DialogueCsvGraphBuilder` ← `mapping.TryResolve` 失败 |

**CSV 对拍**（`Village_老农打水任务.csv`）

| ID | Speaker | 摘要 |
|----|---------|------|
| 1,3,6,8,10,12,14 | 雅 | 已有映射 → 雅尔 |
| 2,4,5,7,9,11,13,15 | **老人** | ❌ 无映射 |
| 末句 15 | 老人 | 求助打水；**无 Type=Choice** |

**映射拍板 M1**

| csvSpeaker | actorParameterName |
|------------|-------------------|
| `老人` | `老人` |

**必改两处（0820 先例）**

1. `DialogueSpeakerMapping.CreateDefaultInstance()`  
2. `DialogueSpeakerMapping_Default.asset` entries  

否决：M2 改 CSV 成数字 Speaker；M3 `老人→老农`（除非立绘统一叫老农——现网无立绘资产）。

注释建议：类注释「十二条→十三条」；Import 窗口 HelpBox 摘要可补 `老人→老人`。

### B. Prefab 命名与壳层

| 项 | 拍板 |
|----|------|
| 磁盘对话 Prefab | **`Village_老农打水任务.prefab`**（与 CSV 文件名去扩展名一致） |
| Generated | `Assets/GameRes/DialogueTrees/Generated/Village_老农打水任务.asset`（旁路） |
| Actor | **雅尔** + **老人**（Import 建参数；Prefab 内需 DialogueActor） |
| 现网 | ❌ 无任何 `*老农*` / `*Farmer*` Dialogue Prefab |
| UIAlpha | Import **勾选「对话框 UI 淡入」**；未勾则事后手补（防无框） |
| 雅立绘 | 走现网 GoOut/Mask 即可 |
| 老人立绘/Mask | 工程 **无** 老人 Painting → 本期 **无立绘可播**；空 FaceType→Normal + Warning（同埃吉尔占位） |

### C. `农` 与交互实体

| 项 | 磁盘真源 |
|----|----------|
| 路径 | `Map/Design/肯姆尼2合层/农`（GO `5954497600375318608`） |
| 组件 | **仅** Transform + SpriteRenderer |
| localPos | `(10.62, 10.455, **0.714**)` ← **Z≠0** |
| 父 | `肯姆尼2合层` `(-93.22,-7.8,0)` |
| SceneEntity / Interactive / Story | ❌ 无 |
| Sorting | Default · Order 13 |

**方案拍板**

| 方案 | 裁定 |
|------|------|
| **A · Objects 新建交互实体** | ✅ **推荐**（对齐 House_Tree / 室内 Npc；避开合层 Z/缩放） |
| B · 合层 `农` 直接挂三件套 | ⚠️ 须先 Z→0 且证伪 Bounds；不首选 |
| C · GSM 硬编码 | ❌ |

**推荐实体配置（A）**

| 字段 | 值 |
|------|-----|
| 名 | **`Npc_Farmer`**（开放可用 `老农`） |
| 父 | `Objects`（`objRoot`） |
| 世界位 | 对齐合层 `农` 脚位（约合层原点 + local；Scene 微调） |
| 根 Z | **0**（硬性，见 0820 Npc1 无 E） |
| Layer | **21** |
| 三件套 | SceneEntity + Interactive + Body/Collider + RaycastListener |
| `requirePlayerOverlap` | **true（1）** — 近距 NPC，非物品远程 |
| `SimpleStoryTrigger` | Click；`StoryPrefabName=Village_老农打水任务`；`SingleUseInArchive=false` |
| 合层 `农` | **保留 SR 作美术**；交互体无 Sprite 或透明碰撞即可（一般 **不必** Disable 合层，除非叠双影） |
| Cursor | P1 可选 Chat/View |

### D. GSM 登记

| 项 | 结论 |
|----|------|
| `objRoot` | 已指 `Objects` |
| `sceneObjs` | 现有含 StoneBrand / House_* / Door_Shop / **House_Tree** 等；**无** Farmer |
| C# | **不改** `Village_KenMuNiSceneManager`；场景 YAML 追加 SceneEntity 引用即可（运行时亦可重扫，建议存盘同步） |

### E. 本期闭环 vs 下期任务挂钩

```
【本期】
  走近 Npc_Farmer → E/Click
    → SimpleStoryTrigger
    → TriggerStory("Village_老农打水任务")
    → 播完 ID1～15（含求助句）→ Idle
    → ❌ 无 Choice、无 AcceptQuest

【下期 · 不施工 · 对照埃吉尔】
  CSV 增 Type=Choice（接/不接）
  或手改 Graph + MultipleChoiceNode
  QuestAcceptAction(QuestId=打水…)
  可选 AegirQuestStoryTrigger 式按状态切 Offer/Thanks Prefab
  打水道具 / 浇地判定
```

埃吉尔样板路径：`Village_Aegir_QuestOffer` · `0608` 双选项说明 · HomeScene2 `StoryPrefabName`。

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 映射补 `老人→老人`（代码默认 + Default.asset） | **P0** |
| 2 | Import CSV → `Village_老农打水任务`（勾 UI 淡入） | **P0** |
| 3 | `Objects/Npc_Farmer` 三件套 + Story 名 + Z=0 + overlap=1 | **P0** |
| 4 | `sceneObjs` 登记 | **P0** |
| 5 | 老人立绘/Mask | P1 |
| 6 | Choice + AcceptQuest + 状态 Trigger | ❌ 下期 |

**预期 diff**

- `DialogueSpeakerMapping.cs`  
- `DialogueSpeakerMapping_Default.asset`  
- `GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab`（+ Generated）  
- `Village_KenMuNi1.unity`（Npc_Farmer + sceneObjs）  

### G. 验收清单

同 §③。

### H. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 交互实体名 `Npc_Farmer` / `老农` / `农`？ | **`Npc_Farmer`** | ✅ 本报告 |
| Q2 | 合层 `农` 是否 Disable Renderer？ | **否**（默认可并存） | ✅ |
| Q3 | 下期 QuestId / 打水道具表？ | 待策划 | ⏳ |
| Q4 | 老人立绘资源何时入库？ | P1；不挡本期说话 | ⏳ |

（已追加 `OPEN_QUESTIONS.md`。）
