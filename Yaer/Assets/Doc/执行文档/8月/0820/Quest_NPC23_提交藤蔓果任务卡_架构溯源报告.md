# Quest_NPC23 — 提交藤蔓果任务卡（真正接取）— 架构溯源报告

**文档性质**：架构侦探产出（只读溯源 + 配置/挂接设计；**本阶段不改资产**）  
**日期**：2026-08-20  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- 提示词：`Assets/Doc/提示词/0820/Quest_NPC23_提交藤蔓果任务卡_架构侦探提示词.md`
- 前置：`Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md`
- 埃吉尔接取 / 交付：`Assets/Doc/执行文档/6月/0608/Village_Aegir_Quest001_接取追踪_…`、`…交付换场与发奖_…`
- 六阶段：`Assets/Doc/执行文档/6月/0606/经典MMO击杀任务_任务配置文件_架构溯源与执行说明.md`

**产品已定**：交 **5 个藤蔓果**（`TenWangFruit`），报酬 **Gold × 50**；接取入口 = NPC23 对白「好呀」。  
**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**要「真正接取」：在 `QuestConfig.json` 新增独立行（建议 `Quest_002` / `CollectItem` / 藤蔓果×5 / Gold50），并在「好呀！」句后挂 `QuestAcceptAction(Quest_002)`——现网选项与场景已齐，缺的就是配置行 + Accept 节点；交 5 果扣背包 / 发 50 币属交付批次，不阻塞接取。**

---

## ② 原因（生活类比）

| 生活 | 现网对应 |
|------|----------|
| 厨房订单本上多写一张单 | `QuestConfig.json` 新行 |
| 点「好呀」= 在订单本上签字 | `QuestAcceptAction` → `AcceptQuest` → 存档 `InProgress` |
| 交 5 个果到后厨 + 结 50 币 | 交付时查背包扣果 + `TurnIn` + `GrantQuestRewards` |

签字时**不必**货已在手里。没有订单行时 `AcceptQuest` 会打 `Unknown questId` 且**不写存档**——这才是接取硬门槛。  
**禁止**：用 `KillMonster` + 假怪名冒充；复用 `Quest_001`；说「没左侧 UI / 没采果进度就不能接」。

---

## ③ 用户需要做什么

### 接取最小闭环（照此勾；做完点「好呀」应出 `[Quest] Accept Quest_002`）

| # | 项 | 现网（2026-08-20 对拍） | 要做吗 |
|---|-----|-------------------------|--------|
| 1 | `QuestConfig.json` 有第二行且 `questId` 可查 | ❌ 仅 `Quest_001` | **必做** |
| 2 | 图有 MC「我有些忙 \| 好呀」 | ✅ `#13`，Actor=NPC3；`#12`→`#13` | 已有 |
| 3 | 「好呀」→ 雅尔「好呀！」 | ✅ `#13`→`#14`→`#15` 收尾 | 已有 |
| 4 | 「好呀！」后挂 `QuestAcceptAction` | ❌ 无 Accept 节点 | **必做**：插在 `#14` 与 `#15` 之间 |
| 5 | 拒「我有些忙」不 Accept | ✅ `#13`→`#15` 直接收尾 | 保持 |
| 6 | 场景播本 Prefab | ✅ `NpcChair.StoryPrefabName = Village_QuestOffer_NPC23` | 已改对（曾误挂埃吉尔） |
| 7 | `QuestConfigMgr.Init` 启动加载 | ✅ `ProcedureComponentGM` 会 Init | 重启 Play 后应见 `Loaded 2 quest(s)` |

**施工顺序（同一张任务卡，一次可做完接取）**：先保证 JSON 行存在 → 再挂 Accept（选项已在，不必重做 MC）。

**接取验收**：

1. InitScene 进 `Village_HomeScene23`，点 **NpcChair**  
2. 播到选项 → 点 **好呀** → 雅尔「好呀！」  
3. Console：`[Quest] Accept Quest_002`，进度 `0/5 (InProgress)`  
4. 点 **我有些忙**：无 Accept 日志  
5. 埃吉尔 `Quest_001` 回归不受影响  

### 交付另列（不阻塞接取）

| # | 项 | 说明 |
|---|-----|------|
| D1 | 回 NPC 交 5 果 | 推荐方案 **A：交付时查背包**（见 §4.4） |
| D2 | 扣 `TenWangFruit` ×5 | `PlayerBagData.TryRemoveMainItem` 现网够用；**现网 `QuestTurnInAction` 不会扣物品** |
| D3 | 发 50 金币 | JSON `rewards`；`GrantQuestRewards` 须 **TurnIn 成功** 后 |
| D4 | TurnIn 对白 / 按状态切 Prefab | 仿埃吉尔 `QuestTurnIn` + 触发器；另立项 |
| D5 | 左侧 UI / 地图刷果 | **非接取最小集** |

---

## ④ 给程序看的补充

### 4.1 Accept 调用链（接取）

```
NpcChair 按 E
  → SimpleStoryTrigger → Village_QuestOffer_NPC23
  → …对白… → #12「我会给你报酬的。」
  → #13 MultipleChoice（我有些忙 | 好呀）
  → 好呀 → #14 雅尔「好呀！」
  → 【缺】QuestAcceptAction(questId=Quest_002)
  → #15 FightingPanel 收尾
  → QuestManager.AcceptQuest
       → GetQuestRow（无行 → Unknown，不存档）
       → 写入 InProgress + progress=0 → Save
```

`AcceptQuest` **不检查** `objectiveType`，只要求配置行存在。  
`ValidateTargetMonsters` **只校验** `objectiveType == KillMonster` 的行 → `CollectItem` 行**不会**因没有怪名而报错。

### 4.2 Quest_002 JSON 草案（侦探不落盘；施工员可复制）

**字段裁定**：

| 问题 | 裁定 |
|------|------|
| 能否写 `CollectItem`？ | **能**。Accept 不拦；怪物校验跳过非杀怪行 |
| 目标物字段 | **推荐新增 `targetItem`**（解析进 `QuestDataTableRow`）；**接取最小集替代**：暂复用 `targetMonster` 填 `"TenWangFruit"`，并注释「CollectItem 下表示物品名」——**禁止**写成 KillMonster |
| `targetCount` | `"5"`（现网 JSON 数字多为字符串，`ParseInt` 兼容） |
| `rewards.amount` | `"50"`（同现网 `Quest_001` 风格） |
| `id` | `"2"` |

**推荐草案（含 `targetItem`；若本期不改 C# 解析，删掉 `targetItem` 键、把值写入 `targetMonster`）**：

```json
{
  "id": "2",
  "questId": "Quest_002",
  "title": "妈妈的藤蔓果",
  "title_en": "Mom's Vine Fruit",
  "title_jp": "お母さんのつる果実",
  "objectiveText": "提交藤蔓果 5 个",
  "objectiveType": "CollectItem",
  "targetMonster": "",
  "targetItem": "TenWangFruit",
  "targetCount": "5",
  "rewards": [
    { "type": "Gold", "amount": "50" }
  ],
  "prerequisiteQuestIds": [],
  "autoAccept": "false",
  "repeatable": "false",
  "sortOrder": "2"
}
```

**零 C# 改动的接取临时写法**（仅当不想本批改 `QuestDataTableRow`）：

```json
"objectiveType": "CollectItem",
"targetMonster": "TenWangFruit",
"targetCount": "5",
"rewards": [ { "type": "Gold", "amount": "50" } ]
```

物品证据：`EMainItemName.TenWangFruit`；`MainItemConfig` `name: TenWangFruit` / `cnName: 藤蔓果`。

标题「妈妈的藤蔓果」为草案，英日可改（OPEN Q2）。

### 4.3 Graph 挂载点（接取）

| 项 | 值 |
|----|-----|
| Prefab | `Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab` |
| 插入位置 | Statement **#14**（雅尔「好呀！」）**之后**，收尾 **#15** **之前** |
| 节点 | Action → Story → **接取任务** `QuestAcceptAction` |
| 参数 | `questId` = **`Quest_002`**（与 JSON 逐字一致） |
| 连线 | `#14 → NEW Accept → #15`；勿挂在 MC 出口上（未播「好呀！」就 Accept） |
| 拒分支 | `#13 → #15` 保持，**不**经 Accept |

现网拓扑（对拍后）：

```
#12 报酬句 → #13 MC
#13 出口0 我有些忙 → #15 收尾
#13 出口1 好呀 → #14 雅尔「好呀！」→ #15 收尾   ← 在此中间插入 Accept
```

### 4.4 提交 5 果：方案对比与主推

| 方案 | 大意 | 利弊 |
|------|------|------|
| **A 交付时查背包（主推）** | 回 NPC 交付时 `GetMainItemCount(TenWangFruit)≥5` → `TryRemoveMainItem(...,5)` → TurnIn + Grant50 | 对齐产品「提交 5 个即可」；接取后 progress 可一直 0；须扩展 TurnIn（现网只认 `Complete`） |
| B 拾取计数 | 入包 `OnItemCollected` 累加满 5 → Complete 再交 | 更像杀怪；要改入包点；非接取最小集 |
| C Accept 时若已有≥5 直接 Complete | 可做 | 弱化「去采」叙事；可作 A 的附加优化 |

**主推 A 细节（交付批次，不阻塞接取）**：

1. `TryRemoveMainItem` / `GetMainItemCount` **现网够用**。  
2. 现网 `QuestTurnInAction` = `TurnInQuest`（须已是 `Complete`）+ `GrantQuestRewards`；**不扣物品**。  
3. 采集交付须新增逻辑（择一）：  
   - **A1**：`QuestManager` 增加 `TryTurnInCollectQuest`：InProgress 且背包够 → Remove → 置 `TurnedIn` → Grant；或  
   - **A2**：交付前先根据背包置 `Complete`，再走现有 `QuestTurnInAction`，并另挂「扣物品」Action。  
4. **Complete 谁写**：方案 A 下可在**交的那一刻**一并完成（不必先 Complete 再交）；与杀怪「先 Complete 再回 NPC」不同，须在代码注释写清。  
5. **独立 TurnIn Prefab**：建议有（仿 `Village_Aegir_QuestTurnIn` + 按状态切图）；OPEN Q4。接取批**不做**。  
6. **50 金币**：只写 JSON；禁止在 Action 里写死 50。走 `GrantQuestRewards`，须 TurnIn 成功。

### 4.5 接取 vs 交付批次切分

| 批次 | 必做 | 明确不做 |
|------|------|----------|
| **接取（本期施工默认）** | JSON 新行 + `#14` 后挂 Accept；确认场景已是 NPC23 Offer | 扣果、刷果、TurnIn Prefab、左侧 UI、改 Quest_001 |
| **交付（下一批）** | 方案 A 扣果 + TurnIn + Grant50 + 交付对白/切图 | — |

### 4.6 禁止清单（写进施工纪律）

- ❌ `objectiveType: KillMonster` + 假 `targetMonster`  
- ❌ 复用 / 改写 `Quest_001`  
- ❌ 因无追踪 UI / 无采果进度而推迟 Accept  
- ❌ 任务表驱动选项按钮  
- ❌ 接取批做整图刷藤蔓果玩法  

### 4.7 开放问题（已记入 OPEN_QUESTIONS）

| ID | 问题 | 施工默认 |
|----|------|----------|
| Q1 | questId 是否 `Quest_002`？ | **是** |
| Q2 | 中文标题？ | 草案「妈妈的藤蔓果」；英日草案见上 |
| Q3 | 交付查背包 vs 拾取计数？ | **交付查背包（方案 A）** |
| Q4 | 是否独立 TurnIn Prefab？ | **建议要**（交付批）；接取批不做 |
| Q5 | 本批是否新增 `targetItem` 字段？ | **推荐要**；否则临时 `targetMonster=TenWangFruit` + CollectItem |

---

## 5. 相关文档索引

| 主题 | 路径 |
|------|------|
| 本提示词 | `Assets/Doc/提示词/0820/Quest_NPC23_提交藤蔓果任务卡_架构侦探提示词.md` |
| NPC23 选项 | `Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md` |
| 埃吉尔接取/交付 | `Assets/Doc/执行文档/6月/0608/…` |
| 配置 / Manager | `QuestConfig.json`、`QuestDataTableRow.cs`、`QuestManager.cs`、`QuestAcceptAction.cs` |

---

## 6. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-08-20 | 初版：接取最小闭环；Quest_002 JSON 草案；方案 A 交付设计；对拍现网已有 MC/场景、缺 Accept |

**文档路径**：`Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md`
