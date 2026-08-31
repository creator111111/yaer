# Village_老农打水 — 当日不可再接 + ResetQuest + 禁空跑结算 — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**产品改口**：覆盖 0830「交完可再接」——**交完（TurnedIn）当日不可再接**；重接仅经 `ResetQuest`；禁止误进 `_完成结算` 空跑。  
**溯源**：`Assets/Doc/执行文档/0831/Village_老农打水_当日不可再接与禁空跑结算_架构溯源报告.md`

---

## ① 结论一句话

`TurnedIn` 再谈老农只播 **`_今日已完成` 短循环**；`AcceptQuest` 对 TurnedIn **一律拒绝**；新增 `ResetQuest`（Debug 菜单可验）；结算图仍只在「进行中且可交」时进入。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `FarmerQuestStoryTrigger.cs` | `TurnedIn` → `_今日已完成`；仅 `null` 走 Offer；仅 `InProgress && CanTurnIn` 走结算 | 对齐 Npc23；禁空跑 |
| `QuestManager.cs` | TurnedIn 不再因 `repeatable` 放行 Accept（方案 A） | 当日锁；001/003 统一 |
| `QuestManager.cs` | 新增 `ResetQuest(questId)`：Remove 状态/进度 + Save + 日志 | 跳日预留 / Debug |
| `Village_老农打水任务_今日已完成.prefab` + CSV | 新建短句「今天的水够了…」 | Q2 新建，避免复用「快去打水」 |
| `QuestResetDebugMenu.cs` | `Editor/Quest/ResetQuest_003 老农打水` | Play 下验收 Reset |
| `OPEN_QUESTIONS.md` | Q1～Q5 标已施工 | 改口落盘 |

**未改**：井逻辑、Tips、金币 500、`Quest_003.repeatable=true`（语义改为「可经 Reset 重复」）、`_完成结算` Prefab 台本顺序（根治靠 Trigger 门禁，P2 不改图）。

---

## ③ 状态机（改后）

```
null / Reset 后     → Village_老农打水任务（Offer）
InProgress 满<4     → _进行中
InProgress 可交     → _完成结算（句末 TurnIn → +500）
TurnedIn            → _今日已完成（禁 Offer / 禁结算 / 禁 Accept）
```

---

## ④ 验收清单

- [ ] 未接 → Offer → 帮 → Accept + 空桶 Tips  
- [ ] 进行中满不足 → 只播 `_进行中`，绝不是 `_完成结算`  
- [ ] 满×4 → `_完成结算` → **+500**；Console `Grant Gold 500`  
- [ ] 交完再点老农 → **`_今日已完成`**；无帮/不帮、无金、无空桶 Tips  
- [ ] 交完后即使残留满桶≥4，也**不会**再进结算  
- [ ] Play 中点 `Editor/Quest/ResetQuest_003 老农打水` → Console `[Quest] Reset Quest_003` → 可再 Offer→帮  
- [ ] Quest_001：TurnedIn 后 Accept 也会拒；若需重接同样走 `ResetQuest`  

**注意**：若刚改 Prefab 名仍播旧图，停 Play 确认 `GameRes/Prefabs/Dialogue` 已有 `_今日已完成`；正式包须重打含该 Prefab 的 AB。

---

## ⑤ 剩余风险

| 风险 | 说明 |
|------|------|
| Quest_001 回归 | 方案 A 收紧后，埃吉尔也不能靠 Accept 直接重接，须走 Reset（与侦探倾向一致） |
| 背包残留桶 | Reset 不清桶（Q4）；再接会再发空桶×4，可能叠堆——属既有道具规则 |
| 结算图仍先播报酬句 | 结构病因仍在；门禁正确后不应误进；P2 可选早退短句 |
