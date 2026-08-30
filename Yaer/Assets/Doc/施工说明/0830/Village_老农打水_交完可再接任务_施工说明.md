# Village_老农打水 — 交完可再接任务 — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**产品改口**：交完同一存档可再做，**不必**读接取前档。

---

## ① 结论一句话

交完后再谈老农仍出帮/不帮；选「帮」可再次 Accept、发空桶、点井、交付。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `FarmerQuestStoryTrigger` | `TurnedIn` → Offer（不再锁进行中短句） | 交完要能再看见帮/不帮 |
| `QuestConfig` Quest_003 | `repeatable: true` | 配置与可重复语义对齐 |
| `QuestManager.AcceptQuest` | `TurnedIn` 且 `repeatable` 时允许重接并清 progress | 否则 Trigger 回 Offer 后仍 Already accepted |

**未改**：井仍要求 InProgress（交完未再接时点井仍短反馈）；Quest_002 仍不可重复。

---

## ③ 验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | 交完 → 再谈老农 | 出现 **帮/不帮** |
| 2 | 再选帮 | 再发空桶×4；可点井 |
| 3 | 再交满×4 | 再发金币；状态再 TurnedIn |
| 4 | 交完存读同一档 | 仍可再 Offer（非锁死） |
