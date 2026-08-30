# Village_老农打水 — 任务进度与存档不同步 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md`

---

## ① 结论一句话

井换桶与接任务发空桶后都会立刻 `SavePlayerBag`；Collect 的 `GetQuestProgress` 改读背包满桶数。交完不回 Offer 仍是设计。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `VillageWellLogic.cs` | 换桶成功末行 `QuestManager.SavePlayerBag()` | 只改内存会被后续 SaveSpcData 用旧背包盖盘 |
| `SavePlayerBagActionTask.cs` | 新对话 Action「保存玩家背包」 | Accept 已存任务；GetItem 原先不存包 → 读档无空桶 |
| `Village_老农打水任务.prefab` | Tips 后接 `SavePlayerBag`（id27） | 发空×4 立刻落盘 |
| `QuestManager.GetQuestProgress` | CollectItem 读 `GetMainItemCount(targetItem)` | 消除假 0/4 |

**未改**：`FarmerQuestStoryTrigger` TurnedIn 策略；`SaveSpcData` 架构。

**产品口径**：读**已交完**档不能再帮/不帮——须读接取前档或 Debug 清状态。

---

## ③ 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 接任务 → 打 2 桶 → 菜单存 → 读档 | InProgress；空/满一致 |
| 2 | 接任务（含发桶）后不菜单存、杀进程再进**同一档** | 仍有空/满（Accept/井已 SaveBag） |
| 3 | 接取前档 → 接+打水 → 读接取前档 | 未接；可再 Offer |
| 4 | 交完存读 | TurnedIn；无帮/不帮 |
| 5 | `GetQuestProgress(Quest_003)` | 满桶数/4 与背包一致 |
| 6 | Console | 可见 `SaveSpcData` / 背包保存日志 |

---

## ④ 给程序

- 井每次成功都会存包（报告 Q1）；频率可接受。  
- Debug 清 Quest_003+桶 → P2 另开。
