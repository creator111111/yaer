# Village_老农打水 — 当日不可再接 + ResetQuest 预留 + 禁空跑结算 — 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读溯源 + 方案拍板（**本阶段未改代码 / Prefab / Quest 表**）  
**Unity**：2020.3.48f1  
**范围**：`Village_KenMuNi1` · `Npc_Farmer` · `Quest_003` · `FarmerQuestStoryTrigger` · `QuestManager` · `_完成结算`  
**产品改口**：覆盖 0830/0831「交完同档可再接」——**交完当日不可再接**；预留 `ResetQuest`；消灭空跑 `_完成结算`  
**关联**：`施工说明/0830/Village_老农打水_交完可再接任务_施工说明.md` · `Npc23QuestStoryTrigger` · 提示词 `提示词/0831/Village_老农打水_当日不可再接与禁空跑结算_架构侦探提示词.md`

---

## ① 结论一句话

**空跑根因是「结算对白先播报酬、句末 TurnIn 失败仍 EndAction」；现网 `TurnedIn→Offer` + `AcceptQuest` 放行重接是旧改口，须改回 Npc23 式「TurnedIn→短循环」，重接只经新建 `ResetQuest` 清状态后再 Offer。**

---

## ② 原因（通俗）

老农交完活后，现网又把「帮/不帮」摊开——同一天能无限再接。  
「谢谢，这是报酬」这句在对话里先说完，真正发金币在最后一步；若这一步失败，玩家只听见报酬、口袋没进钱——这就是空跑。  
修法：交完当天只说短感谢、别再进结算图；想再做等「新一天」调 `ResetQuest` 清锁。

---

## ③ 用户检查清单（现网复现 / 改后期望）

| # | 操作 | 现网（改口前） | 改后期望 |
|---|------|----------------|----------|
| 1 | 未接 → 谈老农 | Offer 帮/不帮 | 同左 |
| 2 | 帮 → Accept | 空桶×4 + Tips；InProgress | 同左 |
| 3 | 进行中满&lt;4 | `_进行中`（「水井就在前面…」） | 同左；**绝不** `_完成结算` |
| 4 | 满×4 → 结算 | `_完成结算` → +500 金（一次） | 同左 |
| 5 | **交完再点老农** | **Offer 帮/不帮**（可再接） | **短循环**；无 Offer、无结算、无金、无空桶 Tips |
| 6 | 交完后再走结算图 | 重接后满≥4 可再进结算再发金 | **禁止**再进 `_完成结算` |
| 7 | 调 `ResetQuest("Quest_003")` | （接口不存在） | 可再 Offer→帮→整条重来 |
| 8 | 空跑特征 | 听「这是你的报酬」但 Console 无 `Grant Gold` / 有 `未成功`/`Already turned in` | **不得再出现循环** |

---

## ④ 给程序

### A. 现网状态机（已核实）

```
FarmerQuestStoryTrigger.ResolveStoryPrefabName
  state == null || TurnedIn  →  Village_老农打水任务        // Offer（含帮/不帮）❌ 新产品禁止 TurnedIn 走这支
  InProgress && CanTurnIn    →  Village_老农打水任务_完成结算
  InProgress && !CanTurnIn   →  Village_老农打水任务_进行中
  其它脏数据                 →  _进行中
```

```
接取（帮）
  → QuestAcceptAction → AcceptQuest
       TurnedIn + repeatable=true → 允许重接、progress=0   // ❌ 当日锁要关掉这条
  → GetItem 空桶×4 + Tips

井
  → VillageWellLogic：仅 InProgress 兑换；允许满桶>4

交付
  → _完成结算 对白（先「报酬」句）
  → 句末 QuestTurnInAction
       TryTurnInCollectQuest 成功 → GrantQuestRewards(+500)
       失败 → 只 Log，EndAction，对白已播完     // ← 空跑结构
```

| 对照 | Npc23（Quest_002） | 老农现网（Quest_003） |
|------|-------------------|----------------------|
| `TurnedIn` | → **Thanks**（不回 Offer） | → **Offer**（0830 交完可再接改的） |
| `repeatable` | `false` | `true` |
| Accept 重接 | TurnedIn 直接拒 | TurnedIn + repeatable **放行** |

---

### B. 空跑复现链路（根因钉死）

#### B1. 结构病因（必现条件）

| 层 | 事实 |
|----|------|
| 台本顺序 | `_完成结算`：Statement「谢谢你，这是你的报酬…」→ 雅句 → **最后** `QuestTurnInAction` |
| 失败语义 | `turnedIn==false` 时 **不发奖**，仍 `EndAction()`；对白不会回滚 |
| 金币 Tips | 结算图 **无** 金币 Tips；成功只靠 `AddGold` + `[Quest] Grant Gold`。玩家说的「无 Tips」若指报酬反馈，成功时本就无横幅——**以有无金币 / Grant 日志为准** |

→ **只要误入 `_完成结算` 且 TurnIn 失败 = 空跑**（听报酬、无金）。

#### B2. 现网容易「再进结算」的路径（与旧改口绑定）

| 步骤 | 状态 / 背包 |
|------|-------------|
| ① 首次交满×4 | TurnIn 成功 → TurnedIn；扣 4 满桶；若曾打超，**可能残留满桶≥4** |
| ② 再谈老农 | **Offer**（现网） |
| ③ 再选帮 | Accept 重接 → InProgress；再发空桶×4 Tips |
| ④ 若残留满≥4 或再打满 | **再次** `_完成结算` → 再发金（产品现要禁止的「当日再刷」） |

空跑子集：④ 若 Resolve 进了结算但 Action 失败（`Already turned in`、扣物失败、`questId` 异常等）→ 报酬对白 + 无金。  
对话进行中二次 Trigger 通常被 `HasRunningStory` 挡住，**主修仍是 Trigger 别在 TurnedIn / 不可交时进结算图**。

#### B3. 禁空跑原则（施工硬约束）

| 规则 | 说明 |
|------|------|
| Trigger 门禁 | **仅** `InProgress && CanTurnInCollectQuest(Quest_003)` → `_完成结算` |
| TurnedIn | **禁止** Offer、**禁止** `_完成结算` |
| 不靠对白自救 | 不要指望在图里「失败跳短句」做主方案；**别进这张图**才是根治 |

---

### C. 与「交完可再接」冲突点（改口声明）

| 旧决议（0830/0831） | 本需求（2026-08-31 再改口） | 现网落点 |
|--------------------|---------------------------|----------|
| `TurnedIn` → Offer | **当日不可再 Offer/Accept** | `FarmerQuestStoryTrigger` L38–40 |
| `repeatable=true` + Accept 放行 TurnedIn 重接 | 重接 **仅经 `ResetQuest`** | `QuestManager.AcceptQuest` L77–87 |
| 交完再谈可再发空桶 | 交完再谈 → **短循环**，不发桶不发金 | Offer 图内 Accept+GetItem |

**定性**：产品改口，不是忘了旧案。施工须改注释与 `OPEN_QUESTIONS` 状态。

---

### D. 目标状态机（侦探拍板倾向 · Q 未全定时施工默认）

| 状态 | Prefab | 接取 / 发奖 |
|------|--------|-------------|
| `state==null` 或 **Reset 后** | Offer `Village_老农打水任务` | 帮 → Accept + 空桶×4 + Tips |
| `InProgress` 且满&lt;4 | `_进行中` | 不 Accept、不 TurnIn |
| `InProgress` 且可交 | `_完成结算` **仅有效交付一次** | TurnIn 成功才 +500 |
| `TurnedIn`（当日已交、未 Reset） | **短循环**（见 Q2） | **禁止**结算、**禁止** Accept |
| 日后跳日 / Debug | `ResetQuest("Quest_003")` | 清锁 → 可再 Offer |

**Q1 施工默认**：锁粒度 = **交完（TurnedIn）后锁**；进行中仍催促打水。接了未交不算「今日已完成」。

---

### E. `ResetQuest` API 方案

**推荐签名**（否决另起 `lastCompletedDay`：无日历前多余字段）：

```csharp
/// <summary>
/// 将任务恢复为「未接取」：移除 questStates / questProgress 中该 id。
/// 供日后跳日 / 新一天调用；无日期系统时由 Debug 菜单验收。
/// 不改背包、不发奖、不自动播对白。
/// </summary>
public void ResetQuest(string questId);
```

| 项 | 约定 |
|----|------|
| 清什么 | `PlayerQuestData.questStates`、`questProgress` 中该 `questId`（Remove 键 → `GetQuestState==null`） |
| 不清什么 | 背包空/满桶、金币、其它任务（Q4） |
| 落盘 | 清完后 `SaveQuestProgress()` |
| 日志 | `[Quest] Reset {questId}` |
| 调用方（本期） | Editor/Debug 菜单即可；**不**接真实日期 UI |
| 与 `repeatable` | 保持 `Quest_003.repeatable=true`，语义改为「**可经 Reset 重复**」；`AcceptQuest` 在 TurnedIn 时 **一律拒绝**（或仅当显式「已 Reset 无键」才可接——Reset 后无键，走首次 Accept） |
| 否决 | 大做日历/`lastCompletedDay` 存档字段（无跳日前无收益）；Reset 时自动清桶（Q4 另议） |

**AcceptQuest 门禁调整（与 Reset 配套）**：

```
若 existingState == TurnedIn：
  → 直接拒（Already turned in），不再看 repeatable 放行
  // repeatable 留给日后「Reset 后可再接」的文档语义 / 或其它任务策略
  // Quest_001 埃吉尔若仍依赖 TurnedIn 重接，须分支：仅 Quest_003 收紧，或埃吉尔改走 Reset
```

**风险**：`Quest_001` 也是 `repeatable=true` 且 Accept 共用放行逻辑。施工须：

- **方案 A（推荐）**：`AcceptQuest` 对 TurnedIn **一律不放行**；埃吉尔若需重接也走 `ResetQuest`（与「无日期先留接口」一致）。  
- **方案 B**：仅 `Quest_003` 特殊拒绝 TurnedIn 重接；001 保持旧行为——快但分裂语义。

侦探倾向 **A**（接口统一）；若不敢动 001，写 OPEN 后用 B。

---

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 文件 / 资源 | 优先级 |
|---|------|-------------|--------|
| 1 | `TurnedIn` → 短循环 Prefab（禁 Offer / 禁结算） | `FarmerQuestStoryTrigger.cs` | **P0** |
| 2 | 短循环 Prefab：新建 `_今日已完成` **或** 暂复用 `_进行中`（文案「快去打水」不贴切，见 Q2） | Dialogue Prefab / CSV | **P0** |
| 3 | `AcceptQuest`：TurnedIn 不再因 `repeatable` 放行（方案 A 或 B） | `QuestManager.cs` | **P0** |
| 4 | 新增 `ResetQuest(string questId)` + Debug 入口 | `QuestManager.cs` + Editor Debug | **P0** |
| 5 | 注释 / 施工说明覆盖「交完可再接」 | Trigger / OPEN_QUESTIONS / 本报告 | **P0** |
| 6 | （可选）结算图失败时早退短句 | `_完成结算` Prefab | P2，非根治 |
| 7 | 不动 | 井逻辑、Tips 图、金币 500、Quest_002 每日规则 | — |

**预期 diff（P0）**

- `FarmerQuestStoryTrigger.cs`（状态表对齐 Npc23）  
- `QuestManager.cs`（`ResetQuest` + Accept 门禁）  
- 可选新 Prefab `Village_老农打水任务_今日已完成`  
- `OPEN_QUESTIONS.md` 改口条目  
- `施工说明/0831/Village_老农打水_当日不可再接与禁空跑结算_施工说明.md`（施工阶段）

---

### G. 验收清单（给后续施工员）

- [ ] 未接 → Offer → 帮 → Accept + 空桶 Tips  
- [ ] 进行中满不足 → 只播 `_进行中`，绝不是 `_完成结算`  
- [ ] 满×4 → `_完成结算` → **+500**（一次）；Console `Grant Gold 500`  
- [ ] 交完再点老农 → **短循环**；不再 `_完成结算`；无金、无空桶 Tips  
- [ ] 交完后 **看不见** 帮/不帮（或选了也 Accept 失败）  
- [ ] `ResetQuest("Quest_003")` 后可再 Offer→帮→整条重来  
- [ ] 无空跑循环：不会「结算对白播完却无金」反复出现  
- [ ] Quest_001/002 行为按方案 A/B 回归一遍  

---

### H. 开放问题（写入 OPEN_QUESTIONS · 未拍板勿当已定）

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 当日锁粒度：仅 TurnedIn 锁，还是 Accept 即占坑？ | **交完（TurnedIn）后锁** | ⏳ 待产品确认 |
| Q2 | 短循环用 `_进行中` 还是新建 `_今日已完成`？ | **新建短句**（「今天谢过」）；暂复用进行中仅作跳板 | ⏳ |
| Q3 | `repeatable` 保持 true 还是改 false？ | **true + 仅 Reset 放行**；Accept 不再直接重接 | ⏳ |
| Q4 | Reset 是否清背包空/满桶？ | **本期只清任务状态/进度** | ⏳ |
| Q5 | Accept 收紧是否波及 Quest_001？ | **方案 A 一刀切**；怕回归则 B 仅 003 | ⏳ |

---

### I. Prefab / 文案速查

| Prefab | 现网用途 | 关键句（摘） |
|--------|----------|--------------|
| `Village_老农打水任务` | Offer + 帮/不帮 | 长对白 + Choice |
| `Village_老农打水任务_进行中` | 催促打水 | 「水井就在前面，空桶打满再拿回来。」 |
| `Village_老农打水任务_完成结算` | 交付 | 「谢谢你，这是你的报酬…」+ 句末 TurnIn |

`_进行中` 作 TurnedIn 短循环会文不对题 → 支撑 Q2 倾向新建。
