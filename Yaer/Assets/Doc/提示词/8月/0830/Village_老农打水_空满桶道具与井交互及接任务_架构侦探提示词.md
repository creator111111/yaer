# Cursor Agent Prompt · 老农打水：道具表（空桶/满桶）盘点 → 井交互 Tips → 帮/不帮接任务设计

> **角色**：先【架构侦探】只读溯源 + 任务/道具设计拍板，报告后再分阶段【施工员】  
> **日期**：2026-08-30  
> **场景**：`Village_KenMuNi1` · 合层 **`井`**（用户红箭头）+ **`农`** / `Npc_Farmer`  
> **产品目标（白话 · 按依赖排序）**：  
> 1. **先检查并设计道具**：特殊道具「空桶」「满桶」——需要 **空桶×4、满桶×4**（道具列表里要有这两种；数量 4 是任务用量）  
> 2. **先做井交互**：点井弹出与「获得了艾琳之剑」**同款 Tips 横幅**（入包/换桶逻辑与提示对齐现网 Tips 管线）  
> 3. **再设计任务**：老农对白完出选项 **帮 / 不帮** 接任务；任务内容 = 去旁边的井互动打水  
> **已有台本碎片**：`Village_老农打水任务*.csv`（主谈 / `_接受` / `_拒绝` / `_拒绝之后接受` / `_完成结算`）已写「打四桶水」  
> **本阶段**：只读；禁止改代码 / Prefab / 图集 / CSV / Quest 表  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_老农打水_空满桶道具与井交互及接任务_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品分期（施工顺序 · 侦探写进最小清单）

| 阶段 | 内容 | 依赖 |
|------|------|------|
| **P0-A · 道具表** | 确认现网有无空桶/满桶；设计 `EMainItemName` + MainItemDatabase 两条（或报告否决八条独立 ID） | 无 |
| **P0-B · 井交互** | 合层 `井` → 可点；按规则扣空桶/加满桶（或等价）+ **Tips 横幅**（同剑） | 须先有道具 ID + Tip 图 Key |
| **P1 · 接任务** | 主对白末 **帮/不帮** Choice；接受 → AcceptQuest + 发 4 空桶 + Tips；拒 → `_拒绝` 线 | 埃吉尔样板 + QuestConfig 新行 |
| **P2 · 交任务** | 满桶×4 交老农 / `_完成结算`（本期可只挂钩） | P0+P1 |

用户原话强调：**先做井**、**先设空桶/满桶列表并检查有没有**——报告 ① 必须先回答「现网有没有」。

### 现场锚点

```
Village_KenMuNi1 / Map / Design / 肯姆尼2合层 /
  … 精灵池* / 商店*
  ★ 井                 ← 红箭头：打水交互点（预扫仅装饰 SR）
  农                   ← 美术；交互在 Objects/Npc_Farmer
```

### 道具盘点预扫（须磁盘证伪 · 助手倾向「没有」）

| 源 | 预扫 |
|----|------|
| `EMainItemName` | AiLinSword…Fish 等；**无** EmptyBucket / FullBucket / 空桶 / 满桶 |
| `MainItemConfig.json` / `MainItemDatabase.asset` | cnName 列表无「空桶」「满桶」 |
| 室内「木桶」 | 仅 **场景互动物** / 对白 Prefab，**不是**背包道具 |

**结论假说**：两种特殊道具 **都不存在**，须新建。

### 空桶×4 / 满桶×4 · 数据设计倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **I1 · 两种道具 + 数量** | `EmptyWaterBucket`、`FullWaterBucket`；接任务 `AddMainItem(空, 4)`；井上每次空→满；交任务扣满×4 | **✅ 推荐**（与「打四桶」文案一致） |
| I2 · 八个独立 itemId | Empty1…4 Full1…4 | ❌ 列表膨胀，无必要 |
| I3 · 一种桶 + 状态字段 | 非 MainItem 常规 | ❌ 另起炉灶 |

须同步（对照 0704 MainItem 补全先例）：

- `EMainItemName` / `MainItemName` 常量  
- `MainItemDatabase` Entry（icon、displayName、堆叠上限≥4）  
- 可选 JSON 归档  
- TipKey：`GetEmptyWaterBucket` / `GetFullWaterBucket`（或一次发 4 只弹一次「获得了空桶×4」——**开放**：一张图写「四个空桶」还是弹四次）

### 井交互假说

| 项 | 预扫 / 倾向 |
|----|-------------|
| 合层 `井` | 仅 Transform+SR，无 Interactive（同当初 `农`） |
| 挂点 | 推荐 **Objects 下 `Well` / `Npc_Well`**，合层保留美术（对齐 Npc_Farmer） |
| 点击条件 | 任务已接且背包有空桶≥1 → 消耗 1 空 + 获得 1 满 + Tips；否则播短对白/Tips「没有空桶」 |
| 特效 | **同剑**：`AddMainItem` + `OpenTipsForm(TipKey, Item)`；已有 `OpenTipsFormActionTask` 可对话用，井更宜 **C# 交互脚本**（仿 HomeScene2Box / 物品交互） |
| 距离 | 井 = 场景物，倾向 **远程可点** 或近距——侦探对照村内物拍板 |

### 帮 / 不帮 · 接任务假说

| 资产 | 现状 |
|------|------|
| `Village_老农打水任务.prefab` | 主谈至求助句，**无 Choice** |
| `_接受.csv` / `_拒绝.csv` / `_拒绝之后接受.csv` | 台本已有；Generated 有，Prefab 是否齐须核实 |
| 样板 | 埃吉尔：`Type=Choice` → `QuestAcceptAction`；`AegirQuestStoryTrigger` 按状态切 Offer |

| 选项 | 分支 |
|------|------|
| **帮** | 播 `_接受`（或内嵌）→ `AcceptQuest(Quest_0xx)` → 发空桶×4 + Tips |
| **不帮** | 播 `_拒绝`；可再谈走 `_拒绝之后接受`（开放） |

Quest 设计表（侦探填实）：

| 字段 | 倾向 |
|------|------|
| questId | `Quest_003`（或下一空号；勿复用 001/002） |
| 目标 | CollectItem / 自定义「持有满桶×4」 |
| 目标物 | `FullWaterBucket` ×4 |
| 发奖 | `_完成结算`；偏金币（CSV）——开放是否另发物 |

### Tips 复用（已溯源 · 勿重查剑全文，只引用）

见 `执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md`：  
`OpenTipsForm(key)` + 图集 Sprite；无图则静默。井/发桶 **必须** 准备 Tip 图。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 盘点空/满桶是否存在；设计两条道具 | ❌ 八个独立桶 ID（除非证伪必要） |
| ✅ 井交互 + Tips 方案与挂点 | ❌ 重做 TipsPanel |
| ✅ 帮/不帮 + Quest 行设计 + 对白接线方案 | ❌ 强制做完交任务结算 UI（可挂钩） |
| ✅ 分阶段施工清单 | ❌ 改商店货单 |

### 严禁（本阶段）

- 改代码 / Prefab / 图集 / CSV / QuestConfig  
- 未扫 `EMainItemName` / MainItemDatabase 就写「已有水桶」  
- 井交互只 `GetItemActionTask` 不弹 Tips  
- 接任务不挂 `QuestAcceptAction`、只播接受对白  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `EMainItemName.cs` / `MainItemDatabase.asset` / `MainItemConfig.json` | **有无空桶满桶** |
| 合层 `井` / `农` / `Npc_Farmer` | 交互挂点 |
| `HomeScene2Box` + Tips 0830 报告 | 横幅样板 |
| `OpenTipsFormActionTask.cs` | 对话内 Tips |
| `Village_老农打水任务*.csv` + Prefab/Generated | 选项/接受/拒绝台本 |
| 埃吉尔 QuestOffer / QuestAcceptAction / QuestConfig | 接任务样板 |
| 0704 MainItem 补全说明 | 新道具落表流程 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md
@Assets/Doc/执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md
@Assets/Doc/执行文档/7月/0704/MainItem_道具固有属性表_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/7月/0704/MainItem_商店六道具ID补全_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_Aegir_QuestOffer_对话末尾双选项_架构溯源与施工执行说明.md
@Assets/Doc/提示词/0804/任务系统_对话末接取选项机制_架构侦探提示词.md
@Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs
@Assets/GameRes/Config/MainItem/MainItemDatabase.asset
@Assets/GameRes/Config/MainItemConfig/MainItemConfig.json
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Dialog/Village_老农打水任务.csv
@Assets/Dialog/Village_老农打水任务_接受.csv
@Assets/Dialog/Village_老农打水任务_拒绝.csv
@Assets/Dialog/Village_老农打水任务_拒绝之后接受.csv
@Assets/Dialog/Village_老农打水任务_完成结算.csv
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/HomeScene2/HomeScene2Box.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/UIPanel/OpenTipsFormActionTask.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、图集、场景、CSV、Quest 配置。只读扫描 + 写溯源/设计报告。

---

## 背景（策划白话）

1. 老农聊完要出 **帮 / 不帮** 接任务；任务是去旁边 **井** 打水。  
2. **先做井**：点井要弹和艾琳之剑一样的获得提示。  
3. 特殊道具要 **空桶、满桶**；任务要 **四个空桶、四个满桶**——先查道具表有没有，没有就设计怎么进列表。  
4. 本阶段只查清 + 拍板设计与施工顺序，不改工程。

---

## 侦探任务清单

### A. 道具表盘点（必须先答）
扫枚举 / Database / JSON：有无空桶、满桶（任意命名变体）？  
出表：现有相关项 vs 缺口。  
拍板 I1：两 ID + 数量 4；写出建议英文 ID / 中文名 / 堆叠上限。

### B. 井现状与交互方案
合层 `井` 组件；推荐 Objects 实体名；点击→空换满→Tips 的状态机；无空桶/未接任务时行为。

### C. Tips 资源清单
每个获得点需要的 TipKey + 是否要新 png（空桶×4 / 每次满桶 / 等）。

### D. 帮/不帮 + Quest 设计
主 Prefab 如何接 Choice；接受/拒绝 Prefab；questId；Accept 时发 4 空桶；Trigger 按状态切对白（对照埃吉尔）。  
交任务仅挂钩。

### E. 分阶段最小施工清单（本阶段不执行）

| # | 阶段 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | A | 新增空桶/满桶到枚举+Database（+图标） | **P0** |
| 2 | A | Tip 图 GetEmpty… / GetFull… 进图集 | **P0** |
| 3 | B | Objects 井交互 + 空→满 + Tips | **P0** |
| 4 | C | 主对白 Choice 帮/不帮 + AcceptQuest + 发 4 空桶 | **P1** |
| 5 | C | 状态 Trigger（Offer/进行中/可交） | **P1** |
| 6 | D | 交 4 满桶 + 结算对白 | P2 |

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 查表 / Inspector | 能看到空桶、满桶两种道具 |
| 2 |（施工后）接任务 | 背包空桶×4 + Tips |
| 3 | 点井（有空桶） | 空-1 满+1 + **同款花边 Tips** |
| 4 | 点井 4 次 | 满桶×4、空桶×0 |
| 5 | 老农对白末 | 出现 **帮 / 不帮**；帮走接受线 |
| 6 | 不帮 | 拒绝线；不发桶 |

### G. 开放问题
- Tip 发 4 空桶：弹一次「四个空桶」还是弹四次？  
- 井未接任务能否点？  
- questId 正式编号？  
- 报酬只金币还是另有物？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_老农打水_空满桶道具与井交互及接任务_架构溯源报告.md`

MASTER 四段式：  
① 结论（有无空/满桶 + 井怎么点 + 帮/不帮怎么接）  
② 原因（通俗）  
③ 用户检查清单（先验道具表、再验井、再验选项）  
④ 给程序：道具设计表 + 井状态机 + Quest/Choice 接线 + 分期施工 + 开放问题
```

---

## 施工员续跑（可按报告阶段拆贴）

### 阶段 A+B（道具 + 井）优先

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_老农打水_空满桶道具与井交互及接任务_架构溯源报告.md
@Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs
@Assets/GameRes/Config/MainItem/MainItemDatabase.asset
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs

你现在是【施工员】。按报告先做：空桶/满桶进道具表 + 井交互（点井 Tips 同艾琳之剑）。

必须遵守：
- 两种道具+数量方案（除非报告改口）；同步枚举与 MainItemDatabase；
- 井交互复用 TipsComponentGSM.OpenTipsForm；禁止只入包不弹窗；
- TipKey 必须有图集 Sprite；合层井与 Objects 交互体关系按报告；
- 本期可不做帮/不帮（若报告允许分期）；
- 代码含详细注释；重要取舍写清原因。

提交说明：道具 ID、井脚本行为、TipKey、如何验收。
```

### 阶段 C（帮/不帮接任务）报告拍板且 A+B 后

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_老农打水_空满桶道具与井交互及接任务_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab
@Assets/Dialog/Village_老农打水任务_接受.csv
@Assets/Dialog/Village_老农打水任务_拒绝.csv

你现在是【施工员】。按报告给老农主对白加「帮/不帮」，接受则 AcceptQuest 并发 4 空桶+Tips。

必须遵守：
- 对照埃吉尔 Choice + QuestAcceptAction；勿只播接受台本假装已接任务；
- 发桶与 Tips 两步都做；questId 用报告拍板值；
- 代码/图含详细注释。

提交说明：选项文案、questId、发奖节点、如何验收。
```
