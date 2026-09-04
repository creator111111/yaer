# Village_KenMuNi1 — 老农打水：空/满桶道具 · 井交互 Tips · 帮/不帮接任务 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读盘点 + 设计拍板（**本阶段未改代码 / Prefab / 图集 / CSV / Quest**）  
**Unity**：2020.3.48f1  
**场景**：`Village_KenMuNi1` · 合层 **`井`** + **`农`** / `Objects/Npc_Farmer`  
**产品顺序**：先道具表 → 先井交互+同款 Tips → 再帮/不帮接任务 → 交任务挂钩  

关联：`0830/获得道具Tips横幅_…` · `0830/老农基础对话…` · `0704/MainItem 补全` · `Npc23QuestStoryTrigger`（CollectItem 样板）· 埃吉尔 Choice  

---

## ① 结论一句话

**现网 `EMainItemName` / MainItemDatabase / MainItemConfig.json 均无「空桶」「满桶」（室内木桶只是场景物）。拍板 I1：新建 `EmptyWaterBucket`（空桶）、`FullWaterBucket`（满桶），用数量 4，勿做八个独立 ID。合层 `井` 仅 Transform+SR 且 local Z≠0——对齐 Npc_Farmer：Objects 下建 `Well`（Z=0）+ C# 点井：任务进行中且空桶≥1 → 扣 1 空加 1 满 + `OpenTipsForm(GetFullWaterBucket, Item)`。主对白现无 Choice、仅 1 个 Dialogue Prefab；P1 末插帮/不帮 → 接受线 `QuestAcceptAction(Quest_003)` + 发空桶×4 + Tips；状态切对白对照 **Npc23（CollectItem）** 而非埃吉尔 Complete。Tips 须新图；无图则静默。**

---

## ② 原因（通俗）

1. **道具表里根本没有水桶**——不先建表，井和任务都没法扣/加。  
2. **合层井只是画**——和当初的「农」一样，要点就得在 Objects 另装门铃。  
3. **获得提示字在图片里**——空桶/满桶各要 Tip 图，和剑同一条 Tips 管线。  
4. **主对白现在聊完就停**——「帮/不帮」和接任务还没接线；交 4 满桶走现成 CollectItem 交任务管线即可挂钩。

---

## ③ 用户检查清单

| # | 阶段 | 操作 | 通过 |
|---|------|------|------|
| 1 | 现网 | 查枚举/Database/JSON | **没有**空桶、满桶（侦探已证） |
| 2 | P0-A 后 | Inspector 道具表 | 能看到空桶、满桶两种 |
| 3 | P0-B 后 | 有空桶时点 Objects/`Well` | 空-1 满+1 + **花边 Tips** |
| 4 | P0-B | 点井 4 次 | 满×4、空×0 |
| 5 | P1 后 | 老农末句后 | **帮 / 不帮**；帮→接任务+空桶×4+Tips |
| 6 | P1 | 不帮 | 拒绝线；**不**发桶 |
| 7 | P2（挂钩） | 满×4 回老农 | 可交付 + `_完成结算` |

施工前门禁：Tip 三语图 + 道具 Icon（可占位）未齐 → 勿宣「Tips 坏了」。

---

## ④ 给程序

### A. 道具表盘点（必须先答）

| 源 | 空桶 / 满桶 | 结果 |
|----|-------------|------|
| `EMainItemName.cs` | 仅 AiLinSword…Fish | **无** |
| `MainItemDatabase.asset` | displayName 至「鱼」 | **无**「空桶」「满桶」 |
| `MainItemConfig.json` | name/cnName 全表 | **无** Bucket/桶 |
| Tips 图集 | 无 GetEmpty/GetFull… | **无** |
| 场景「木桶」类 | 室内互动物 / 对白 | **不是**背包道具 |

**结论：两种特殊道具都不存在，须新建。**

#### 设计拍板 I1（推荐）

| 字段 | 空桶 | 满桶 |
|------|------|------|
| `EMainItemName` / 存档键 | **`EmptyWaterBucket`** | **`FullWaterBucket`** |
| 中文名 | 空桶 | 满桶 |
| 数量语义 | 接任务发 **×4**；井每次 **-1** | 井每次 **+1**；交任务 **-4** |
| 堆叠 | 全局 `MaxStackPerItem=10` ≥4，**够用**（无需单独堆叠字段） | 同左 |
| `BagItemType` | 建议 **`MaterialItem`(2)**（对齐藤蔓果 Collect） | 同左 |
| buy/sell | **-1**（非卖场） | **-1** |
| 否决 | I2 八独立 ID；I3 单桶状态字段 | — |

**同步清单（0704 先例）**

1. `EMainItemName` 末尾追加两枚举  
2. `MainItemDatabase.asset` 两条 Entry（icon、displayName、detail*）  
3. `MainItemConfig.json` 两行（`name`=枚举名）  
4. Icon PNG → `ArtRes/UI/Item/Icon/` + 图集（可先占位）  
5. **不要**进商店货单  

### B. 井现状与交互方案

| 项 | 磁盘真源 |
|----|----------|
| 路径 | `Map/Design/肯姆尼2合层/井`（GO `4281828997462958571`） |
| 组件 | **仅** Transform + SpriteRenderer |
| localPos | `(25.105, 12.88, **2.14**)` ← Z≠0 |
| 父 | `肯姆尼2合层`（同 `农`） |
| Interactive / Story | ❌ |

| 方案 | 裁定 |
|------|------|
| Objects 新建 **`Well`** + 合层保留美术 | ✅（对齐 Npc_Farmer / House_Tree） |
| 合层 `井` 直接挂三件套 | ⚠️ Z≠0 易无 E；不首选 |

**推荐 `Well` 配置**

| 字段 | 值 |
|------|-----|
| 父 | `Objects`；根 **Z=0**；Layer 21 |
| 位 | 对齐合层井世界脚位（合层原点 + local；Scene 微调） |
| 交互 | SceneEntity + Interactive + Collider + RaycastListener |
| `requirePlayerOverlap` | **0（远程可点）** — 场景物对齐 House_Tree；若手感飘再改近距 |
| 逻辑 | **C#**（仿 `HomeScene2Box`）：点击内做换桶 + Tips；**不宜**只靠对话 `GetItemActionTask` |
| GSM | `sceneObjs` 登记；不改 Manager C# |

**点井状态机（拍板）**

```
Click Well
  ├─ Quest_003 非 InProgress
  │     → 短反馈（短对白 Prefab 或 Info Tips「先找老农」）——开放 Q；默认：可点但不成兑换
  ├─ InProgress 且 空桶 < 1
  │     → 短反馈「没有空桶」（建议短对白；勿静默）
  └─ InProgress 且 空桶 ≥ 1
        → TryRemoveMainItem(EmptyWaterBucket, 1)
        → AddMainItem(FullWaterBucket, 1)
        → OpenTipsForm("GetFullWaterBucket", Item)   // 同剑
```

满桶已满 4 且任务只要 4：仍允许继续换（开放）——默认 **允许**直到空桶用尽（背包可>4 但交任务只扣 4）。

### C. Tips 资源清单

引用：`执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md`  
API：`TipsComponentGSM.OpenTipsForm`；对话内已有 `OpenTipsFormActionTask`（默认 Item）。

| 获得点 | TipKey（建议） | 图文案倾向 | 次数 |
|--------|----------------|------------|------|
| 接任务发 4 空桶 | **`GetEmptyWaterBucketx4`** | 「获得了空桶×4」**一张图弹一次** | ✅ 默认 |
| 备选 | `GetEmptyWaterBucket` ×4 入队 | 弹四次同图 | ❌ 吵 |
| 每次井成功 | **`GetFullWaterBucket`** | 「获得了满桶」 | 每次 1 |
| 失败反馈 | 可选 `NoEmptyBucket` Info 或短对白 | 非花边也可 | P1 |

三语 png → `TipInfoAtlas{,_en,_jp}/` + `tipsInfo*.spriteatlas`。缺图 → Error「未找到Tips图片」+ **不弹窗**。

### D. 帮/不帮 + Quest 设计

#### 台本 / Prefab 现状

| 资产 | 现状 |
|------|------|
| `Village_老农打水任务.csv` | 至求助句；**无 Type=Choice** |
| `Village_老农打水任务.prefab` | UIAlpha+Statement；**无** MultipleChoice / AcceptQuest |
| `_接受/_拒绝/_拒绝之后接受/_完成结算.csv` | 台本齐；Generated `.asset` 有 |
| 对应 Dialogue **Prefab** | ❌ **仅主 Prefab**；分支 Prefab 未落盘 |

#### Choice 接线（P1）

| 选项文案（产品） | 分支 |
|------------------|------|
| **帮** | → `_接受` 图（或主图内嵌接受句）→ `QuestAcceptAction(Quest_003)` → `GetItem(EmptyWaterBucket,4)` + `OpenTipsForm(GetEmptyWaterBucketx4)` |
| **不帮** | → `_拒绝`；不 Accept、不发桶 |

样板：埃吉尔 / 哥布林 `MultipleChoiceNode`；Accept 须在「答应」台词后（`QuestAcceptAction` 注释约定）。

CSV 可增一行 `Type=Choice` Extra=`不帮|帮`（或手改 Graph）。**勿**只播接受台本假装已接任务。

#### QuestConfig 新行（拍板）

| 字段 | 值 |
|------|-----|
| `questId` | **`Quest_003`**（001/002 已占） |
| `title` | 如「老农的浇地水」 |
| `objectiveType` | **`CollectItem`** |
| `targetItem` | **`FullWaterBucket`** |
| `targetCount` | **`4`** |
| `rewards` | `Gold`（金额待策划；CSV「积蓄/报酬」） |
| `repeatable` | 建议 `false` |
| 交付 API | `QuestTurnInAction` → `TryTurnInCollectQuest`（扣满×4） |

#### 状态 Trigger（对照 Npc23，非埃吉尔 Complete）

| 状态 | Prefab（建议名） |
|------|------------------|
| 未接 | `Village_老农打水任务`（含 Choice） |
| 已拒未接（可选） | 再点播 `_拒绝之后接受` + Choice | 开放 |
| InProgress 且满&lt;4 | Thanks/催促短句（新建或挂钩） |
| InProgress 且可交 | `_完成结算` + `QuestTurnInAction` |
| TurnedIn | 短感谢循环 |

**新建** `FarmerQuestStoryTrigger : SimpleStoryTrigger`（抄 `Npc23QuestStoryTrigger`），挂到 `Npc_Farmer`，替换写死单 Prefab。  
原因：CollectItem **到不了 Complete**；照搬 `AegirQuestStoryTrigger` 会切错图。

#### 交任务（P2 挂钩）

- `_完成结算` Prefab + TurnIn；报酬以 QuestConfig Gold 为准（是否另发物 → 开放）。  
- 本期可不做结算 UI 抛光。

### E. 分阶段最小施工清单（本阶段不执行）

| # | 阶段 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | **P0-A** | 枚举+Database+JSON+Icon：空桶/满桶 | **P0** |
| 2 | **P0-A** | Tip 图 `GetEmptyWaterBucketx4` / `GetFullWaterBucket` 进图集 | **P0** |
| 3 | **P0-B** | Objects/`Well` + 换桶脚本 + Tips；sceneObjs | **P0** |
| 4 | **P1** | 主对白 Choice 帮/不帮；Accept Prefab；`Quest_003`；发 4 空桶+Tips | **P1** |
| 5 | **P1** | `FarmerQuestStoryTrigger` 按 Collect 状态切图 | **P1** |
| 6 | **P2** | 交 4 满桶 + `_完成结算` + 发奖 | P2 |

**预期 diff（P0）**

- `EMainItemName.cs`、`MainItemDatabase.asset`、`MainItemConfig.json`  
- Tip/Icon 资源 + atlas  
- 新脚本如 `VillageWellLogic.cs`（名可调）  
- `Village_KenMuNi1.unity`（Well + sceneObjs）  

**预期 diff（P1）**

- `QuestConfig.json` Quest_003  
- `Village_老农打水任务.prefab`（+Choice）及接受/拒绝 Prefab  
- `FarmerQuestStoryTrigger.cs`；Npc_Farmer 换 Trigger  

### F. 验收清单

同 §③。

### G. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 发 4 空桶 Tips：一次「×4」还是四次？ | **一次 ×4 图** | ✅ |
| Q2 | 未接任务能否点井？ | **可点但不成兑换 + 短反馈** | ✅ 默认；文案待定 |
| Q3 | questId？ | **`Quest_003`** | ✅ |
| Q4 | 报酬只金币？金额？ | CSV 偏钱；金额待策划 | ⏳ |
| Q5 | 拒后再谈走 `_拒绝之后接受`？ | **建议做**（P1） | ⏳ |
| Q6 | 井 overlap 远程 vs 近距？ | **先远程 0** | ✅ |
| Q7 | 满桶>4 是否允许继续打？ | **允许至空桶尽** | ✅ |

（已追加 `OPEN_QUESTIONS.md`。）
