# Village_老农打水 — 任务进度与存档不同步 / 读档无法重做 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 最小修复拍板（**本阶段未改代码 / Prefab / 存档**）  
**Unity**：2020.3.48f1  
**范围**：`Quest_003` · `VillageWellLogic` · `FarmerQuestStoryTrigger` · 空/满桶 · `PlayerQuestData` / `PlayerBagData`  
**关联**：`施工说明/0830/Village_老农打水_空满桶…施工说明.md` · `QuestManager` Collect 交时查包 · `ArchiveComponentGM.SaveSpcData`

---

## ① 结论一句话

**主因并列三条，勿混为一谈：**(A) **落盘缺口**：井换桶与接任务 `GetItem` 只改内存背包，**不** `SavePlayerBag`；而 `AcceptQuest` 立刻 `SaveQuestProgress`（`SaveSpcData` 写整份盘），会把**旧背包键**一并落盘 → 读档易出现「已接任务但空桶没了 / 打了几桶读档桶数回退」。(B) **UI 假进度（若看 GetQuestProgress）**：CollectItem 真进度在**背包满桶数**；`questProgress` 接取写 0，井**从不**递增，故面板若只读 Count 会永远 0/4——可交判定却看背包。(C) **「交完无法重做」多为设计**：`TurnedIn` 后 Trigger **永不回 Offer**；要重做须读**接取前**档或 Debug 清状态，不是读「已交完档」还能再帮。

---

## ② 原因（通俗）

任务本上只写「接了 / 交了」；打了几桶水写在**书包里的满桶**上。  
接任务时任务本马上存进存档夹，书包却常常还没存——读档就像「单接了、桶没领」。  
点井也只改书包不存档夹；若中途又有别的「只存任务本」操作，夹子里的桶数还会被旧数据盖住。  
交完任务本盖章「已结」后，老农故意不再给你接单——这不是读档坏了，是产品默认不能同一档无限重刷。

---

## ③ 用户检查清单（怎么存读验）

| # | 操作 | 期望（修后） | 现网风险（修前） |
|---|------|--------------|------------------|
| 1 | 接任务 → 打 2 桶 → **菜单存档** → 读该档 | InProgress；空/满与存时一致 | 手动全量存一般能齐；若只依赖 Accept 即时存则易桶丢 |
| 2 | **接取前**存档 → 接任务打水 → **读接取前档** | 未接；可再 Offer；桶按旧档 | 应支持（读档清缓存后 Reload） |
| 3 | 交完 → 存档 → 读该档 | TurnedIn；**不再**帮/不帮 | 现网即如此（设计） |
| 4 | 接任务后**不**菜单存、杀进程再进 | — | **高概率** InProgress + 无空桶 |
| 5 | 任务面板（若有） | 满桶进度=背包数 | `GetQuestProgress` 现恒≈0/4 |

Console 对账：`[Quest] Accept` / `保存指定类型数据成功！PlayerQuestData` 之后是否紧跟背包存；井后是否有 `PlayerBagData` 存。

---

## ④ 给程序

### A. 用户路径分类

| 路径 | 用户期望 | 现网假说（已证伪） |
|------|----------|-------------------|
| **P1** 打水中途存读 | 桶数与 InProgress 跟上档 | 全量 `SaveGameData` 会串行化背包 → **可通**；仅靠 Accept/`SaveSpcData` 任务 → **桶易丢** |
| **P2** 读接取前档重来 | 再出帮/不帮 | `LoadArchive` 清 `archiveDataDic` 后按盘解析 → **应通**（若盘本身是接取前） |
| **P3** 读已交完档再 Offer | 再接一次 | ❌ Trigger `TurnedIn`→进行中短句；**设计如此** |
| **P4** 拒/帮后状态卡死 | 能再谈 | 拒后仍 Offer（施工说明未做拒绝专线）；帮后 InProgress；`Already accepted` 若重复 Accept |

### B. 进度真源（钉死）

```
接任务（帮）
  → QuestAcceptAction → questStates[Quest_003]=InProgress
  → questProgress[Quest_003]=0
  → SaveQuestProgress()                    // ✅ 任务落盘
  → GetItemActionTask(EmptyWaterBucket,4)  // 内存入包
  → OpenTipsForm                           // ❌ GetItem 无 SavePlayerBag

点井成功（VillageWellLogic）
  → TryRemove Empty + Add Full + Tips
  → ❌ 无 SavePlayerBag
  → ❌ 无 questProgress++
  → 可交：CanTurnInCollectQuest = 背包 Full≥4（不看 questProgress）

交付
  → TryTurnInCollectQuest → 扣满×4 → SavePlayerBag + TurnedIn + SaveQuestProgress  // ✅

再谈（FarmerQuestStoryTrigger）
  → null → Offer
  → InProgress + 满不足 → _进行中
  → InProgress + 满≥4 → _完成结算
  → TurnedIn → _进行中（❌ 不回 Offer）
```

| 数据 | 存哪 | 谁更新 | 谁保存（现网） |
|------|------|--------|----------------|
| 任务状态 | `PlayerQuestData.questStates` | Accept / TurnIn | `SaveQuestProgress` ✅ |
| `questProgress` | 同左 | **仅杀怪**累加；Collect 接取=0，井不写；交时对齐 target | 同上 |
| 空/满桶 | `PlayerBagData` | 发空 / 井换 / 交付扣 | 井❌ 发空❌；交✅；商店买✅ |

**无**仅内存的打水计数器——真进度就是背包。

对照：

| 线 | 进度真源 | Complete？ |
|----|----------|------------|
| KillMonster | `questProgress++` | 有 |
| CollectItem（002/003） | **交时查背包**；进行中靠背包 | **无** |
| `GetQuestProgress` | 只读 `questProgress` 字典 | Collect → **假 0/4** |

现网任务追踪 UI：**未发现**订阅 `GetQuestProgress` 的左侧面板实现（事件已预留）。若调试/Log 调了该 API，会误判。

### C. 落盘审计（缺口表）

| 时机 | Save 调用 | 缺口 |
|------|-----------|------|
| `AcceptQuest` | `SaveQuestProgress` → `SaveSpcData<PlayerQuestData>` | 写**整份**存档文件，但本次只把 Quest 字典刷进 `masterGameData`；**背包键仍是盘上旧值** |
| 随后 `GetItem` 发空×4 | **无** | 内存有桶；盘上可能仍无 → **主缺口 A** |
| `VillageWellLogic` 成功 | **无** | 同左；且若其后任意 `SaveSpcData`（杀怪进度等）再写盘，易用**旧背包**盖文件 → **主缺口 A′** |
| `TryTurnInCollectQuest` | `SavePlayerBag` + `SaveQuestProgress` | ✅ |
| 菜单 `SaveOld/NewArchive` | `SaveGameData`（强制 `bag.SerializeInternal` + 全缓存） | ✅ 可一次对齐 |

`SaveSpcData` 行为（钉死）：`T.SerializeInternal(master)` → `ES3.Serialize(master)` → 写文件。  
**不是**「只改任务键」；未先 Serialize 的背包键会以 master 里旧快照落盘。

商店对标：买完必 `SavePlayerBag`。井/发桶应对齐。

### D. 读档审计

| 步骤 | 代码 | 结论 |
|------|------|------|
| `LoadArchive` | `masterGameData=LoadGameData()`；`archiveDataDic.Clear()` | 下次 `GetData` 从盘重 Parse |
| 任务+背包 | 同属 master 分列键 | **同档应同时还原**；不同步来自**写入时**只刷了任务 |
| 读接取前档 | 无 Quest_003 键 | `GetQuestState==null` → Offer ✅ |
| 读已交档 | State=TurnedIn | 不回 Offer ✅（设计） |

### E. 「无法重做」产品口径

| 期望 | 是否合理 | 做法 |
|------|----------|------|
| 读**接取前**档再帮 | ✅ | 修落盘后应通 |
| 读**进行中**档恢复桶数 | ✅ | 井/发空后 `SavePlayerBag`；或依赖全量存 |
| 读**已交完**档再 Offer | ❌ 默认否 | 文档说明；可选 Debug 清 `Quest_003`+桶 |
| 不存档杀进程仍保留桶 | 仅当已 `SavePlayerBag`/全量存 | 现网井后不保证 |

### F. 最小修复方案（本阶段不执行）

| # | 方案 | 何时用 | 优先级 |
|---|------|--------|--------|
| **1** | `VillageWellLogic` 成功换桶后 `QuestManager.SavePlayerBag()` | 桶进度丢 / 与任务即时存打架 | **P0** |
| **2** | 接任务发空×4 后立刻 `SavePlayerBag`（图节点后挂 Action，或 `GetItemActionTask` 可选落盘，或 Accept 后统一存包） | 读档已接无空桶 | **P0** |
| **3** | （可选）`GetQuestProgress`：若 `objectiveType==CollectItem` 则 `current=背包 targetItem 数` | UI/Log 假 0/4 | P1 |
| **4** | 文档/验收：交完须新档或 Debug 重置才能再 Offer | 「无法重做」实为设计 | **P0 文档** |
| **5** | Debug 清 Quest_003 状态+空满桶 | 开发验收 | P2 |
| ❌ | 井上 `questProgress++` 改成杀怪式 | 破坏交时查包样板 | 不做 |
| ❌ | TurnedIn 自动回 Offer 无限刷 | 无产品批准 | 不做 |

**推荐 diff**

- `VillageWellLogic.cs`：换桶成功末行 `QuestManager.getInstance().SavePlayerBag();` + 注释原因  
- 发空路径：Prefab 增存包节点 **或** 小改 `GetItemActionTask`（`bool saveAfter=true` 默认 false，老农节点勾选）——倾向 **Accept 后显式 SavePlayerBag**，少动全局 GetItem  
- （P1）`QuestManager.GetQuestProgress` Collect 分支读包  

**不改**：`FarmerQuestStoryTrigger` TurnedIn 策略（除非产品改口）；Quest_001/002 行为。

### G. 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 接任务 → 打 2 桶 → **存档** → 读档 | InProgress；空/满一致 |
| 2 | 接任务 → 打 2 桶 → **不**菜单存 → 杀进程进同一档* | *若盘曾被 Accept/`SaveSpcData` 写过：修后应仍有桶（因井/发空已 SaveBag） |
| 3 | 接取前档 → 接+打水 → 读接取前档 | 未接；可再 Offer |
| 4 | 交完存读 | TurnedIn；无帮/不帮 |
| 5 | （若做 P1）进度查询 | Collect 显示满桶/4 与背包一致 |
| 6 | Console | 无误报 Already accepted；可见背包 Save 日志 |

### H. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 每次点井是否自动存？ | **是**（对齐商店/交任务；频率可接受） | ✅ |
| Q2 | `GetQuestProgress` 全局 Collect 改读包？ | **P1 建议改** | ⏳ |
| Q3 | 交完是否日常循环打水（无任务）？ | **否**（井非 InProgress 短反馈） | ✅ |
| Q4 | 交完开发重置入口？ | Debug 工具 P2 | ⏳ |
| Q5 | `SaveSpcData` 写全文件是否改为只合并脏类型？ | 架构债；本期用补 SaveBag 止血 | ⏳ 另案 |

（已追加 `OPEN_QUESTIONS.md`。）
